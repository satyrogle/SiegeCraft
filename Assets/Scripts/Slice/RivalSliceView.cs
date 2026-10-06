using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    /// View of the 8 December slice loop on the deterministic sim, under the slice rules (nerve,
    /// refilling orders, tougher units): fight the raider chief with orders that refill, watch both
    /// courage bars, and break the raiders so their survivors walk over and join you. The sim decides
    /// every hit; this class only draws it and passes your orders in. Units are the approved PixelLab
    /// sprites from Resources/RivalSlice/Sheets with their breathing idles, facing each other on a
    /// 2.5D field; a unit without art falls back to a capsule.
    ///
    /// Impact: on a kill, or a big hit when none landed lately, the fight freezes, the screen shakes,
    /// the pair is lit while everyone else dims, the target flashes white and flinches, and the
    /// attacker stretches into its lunge. The dead stagger back, fall with a bounce, kick up dust and
    /// fade low. None of it touches the sim. Timings are starting values, judged by eye.
    public sealed partial class RivalSliceView : MonoBehaviour
    {
        public enum Stage { Intro, Fight, AfterFight, War, BeforeFight, Final }

        /// Command line switch for the recorder: play on autopilot and save every frame to the folder that follows.
        public const string RecordArg = "-ksRivalRecord";

        const float RefWidth = 1280f;
        const float RefHeight = 720f;
        // Run progression is owned by SliceRun.
        const float CardSeconds = 8f;
        const float ShortCardSeconds = 5f;
        const float EndHoldSeconds = 2f;
        const float BreakHoldSeconds = 4f;
        const float FlashTime = 0.14f;
        const float LungeTime = 0.18f;
        const float ShotTime = 0.16f;
        const float PixelsPerUnit = 30f;
        const float SheetPixelsPerUnit = 88f;
        const int SheetFrames = 4;
        const float IdleFrameSeconds = 0.22f;
        const float WalkOverSpeed = 1.6f;

        const float HitFreeze = 0.15f;
        const float KillFreeze = 0.33f;
        const float HitShake = 0.14f;
        const float KillShake = 0.26f;
        const float BreakShake = 0.4f;
        const float ImpactCooldown = 0.7f;
        const float ImpactFlash = 0.3f;
        const float FocusTime = 0.7f;
        const float FocusFade = 0.25f;
        const float PoseTime = 0.36f;
        const float KnockTime = 0.3f;
        const float DeathTime = 0.5f;
        const float DustTime = 0.6f;
        const int BigHit = 40;

        // Center both formations in the clear space between the courage bars and order buttons.
        static readonly Vector3 CameraLook = new Vector3(0f, 0.9f, 1.2f);
        static readonly Vector3 CameraHome = CameraLook + new Vector3(0f, 8f, -8f);

        [Tooltip("Plays the simulation's Smart autopilot for validation instead of waiting for orders.")]
        public bool autopilot;

        [Tooltip("Fight playback speed. 1 is real time.")]
        public float fightSpeed = 1f;

        [Tooltip("Run seed for both manual play and autopilot. No search for a winning seed.")]
        public int worldSeed = 7;

        public Stage CurrentStage { get; private set; }
        public bool Finished { get; private set; }
        public string Summary { get; private set; } = string.Empty;

        static readonly Color Amber = new Color(1f, 0.69f, 0.3f);
        static readonly Color Teal = new Color(0.35f, 0.82f, 0.9f);
        static readonly Color Rival = new Color(1f, 0.42f, 0.36f);
        static readonly Color Ink = new Color(0.9f, 0.9f, 0.88f);
        static readonly Color Dim = new Color(0.62f, 0.62f, 0.6f);
        static readonly Color ClarthBody = new Color(0.85f, 0.66f, 0.35f);
        static readonly Color RaiderBody = new Color(0.7f, 0.24f, 0.2f);
        static readonly Color Fallen = new Color(0.3f, 0.29f, 0.28f);
        static readonly Color HitFlash = new Color(1f, 0.35f, 0.3f);
        static readonly Color HealFlash = new Color(0.5f, 1f, 0.55f);
        static readonly Color DeadTint = new Color(0.36f, 0.34f, 0.33f);
        static readonly Color MoodTint = new Color(0.84f, 0.77f, 0.68f);
        static readonly Color GroundTint = new Color(0.72f, 0.66f, 0.58f);
        static readonly Color TorchColor = new Color(1f, 0.6f, 0.3f);
        static readonly Color DustColor = new Color(0.62f, 0.55f, 0.45f);

        SliceRun _run;
        readonly RunMemory _memory = new RunMemory();
        Tribe _battleEnemy;
        SliceAutopilot _pilot;
        BattleSetup _setup;
        BattleSimulation _sim;
        SimSession _session;
        RunWarResult _lastWar;
        readonly List<RunFightResult> _fights = new List<RunFightResult>();
        int _eventCursor;
        float _tickAccumulator;
        float _stageClock;
        float _animClock;
        float _endHold;
        bool _paused;
        Directive? _targeting;
        string _lastOrder = string.Empty;
        float _lastOrderAge = 99f;

        string _captureFolder = string.Empty;
        int _frame;

        Camera _camera;
        Transform _stageRoot;
        Transform _unitsRoot;
        Texture2D _vignette;
        Sprite _dot;
        Material _silhouetteMaterial;
        readonly List<Light> _torches = new List<Light>();
        readonly Dictionary<string, UnitView> _views = new Dictionary<string, UnitView>();
        readonly List<UnitView> _viewOrder = new List<UnitView>();
        readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
        readonly Dictionary<string, Sprite[]> _sheets = new Dictionary<string, Sprite[]>();
        readonly List<FloatText> _floats = new List<FloatText>();
        readonly List<Shot> _shots = new List<Shot>();
        readonly List<Dust> _dust = new List<Dust>();
        readonly List<Banner> _banners = new List<Banner>();
        readonly List<string> _feed = new List<string>();
        readonly SideFx[] _fx = { new SideFx(), new SideFx() };
        readonly GameObject[] _traps = new GameObject[2];
        readonly GameObject[] _focusRings = new GameObject[2];

        float _freeze;
        float _impactCooldown;
        float _shake;
        float _focusTime;
        UnitView _focusA;
        UnitView _focusB;

        string _cardTitle = string.Empty;
        readonly List<CardLine> _card = new List<CardLine>();

        GUIStyle _title, _body, _small, _tag, _banner, _bannerSub, _button, _cardTitleStyle, _notice, _barLabel;

        sealed class UnitView
        {
            public BattleUnit Unit;
            public GameObject Root;
            public Transform Body;
            public Renderer BodyRenderer;
            public SpriteRenderer Art;
            public SpriteRenderer Silhouette;
            public Sprite[] Idle;
            public Sprite[] IdleTurned;
            public float Phase;
            public Color BaseColor;
            public Vector3 Home;
            public Vector3 Pos;
            public float Height;
            public float Flash;
            public Color FlashColor;
            public float WhiteFlash;
            public float Lunge;
            public Vector3 LungeDir;
            public float Pose;
            public float Knock;
            public Vector3 KnockDir;
            public float DeathAge = -1f;
            public bool ShownDead;
            public GameObject Disc;
            public Renderer DiscRenderer;
            public GameObject Tether;
            public bool WalkOver;
            public bool Fleeing;
            public bool Arrived;
            public Vector3 WalkGoal;
        }

        sealed class FloatText { public Vector3 Pos; public string Text; public Color Color; public float Age; public float Life; public bool Big; }
        sealed class Shot { public GameObject Ball; public Vector3 From; public Vector3 To; public float Age; }
        sealed class Dust { public SpriteRenderer Renderer; public Vector3 Velocity; public float Age; public float Size; }
        sealed class Banner { public string Text; public string Sub; public Color Color; public float Age; public float Life; }
        sealed class CardLine { public string Text; public Color Color; public bool Heading; }
        sealed class SideFx { public int Hold; public int FallBack; public int Unleash; public int Focus; public string FocusId = string.Empty; }


        // ------------------------------------------------------------ lifecycle

        void Awake()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] != RecordArg) continue;
                _captureFolder = args[i + 1];
                autopilot = true;
                fightSpeed = 2f;
            }
            if (_captureFolder.Length > 0)
            {
                Directory.CreateDirectory(_captureFolder);
                Time.captureDeltaTime = 1f / 15f;
            }
        }

        void Start()
        {
            BuildStage();
            _vignette = MakeVignette();
            _dot = MakeDot();
            Shader flat = Shader.Find("GUI/Text Shader");
            if (flat != null) _silhouetteMaterial = new Material(flat);
            StartRun();
        }

        void OnDestroy()
        {
            if (_captureFolder.Length > 0) Time.captureDeltaTime = 0f;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _stageClock += dt;
            _animClock += dt;
            _lastOrderAge += dt;
            HandleInput();

            switch (CurrentStage)
            {
                case Stage.Intro:
                    if (autopilot && _stageClock > CardSeconds) StartFight();
                    break;
                case Stage.Fight:
                    UpdateFight(dt);
                    break;
                case Stage.AfterFight:
                    if (autopilot && _stageClock > CardSeconds + 2f) Continue();
                    break;
                case Stage.War:
                    if (autopilot && _stageClock > CardSeconds) Continue();
                    break;
                case Stage.BeforeFight:
                    if (autopilot && _stageClock > ShortCardSeconds + 1f) Continue();
                    break;
                case Stage.Final:
                    if (autopilot && _stageClock > CardSeconds) Continue();
                    break;
            }

            UpdateViews(dt);
            UpdateEffects(dt);
            UpdateCameraAndLights(dt);
            if (_captureFolder.Length > 0 && !Finished)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(_captureFolder, "frame_" + _frame.ToString("D5", CultureInfo.InvariantCulture) + ".png"));
                _frame++;
            }
        }

        // ------------------------------------------------------------ flow

        void Continue()
        {
            switch (CurrentStage)
            {
                case Stage.Intro: ShowBeforeFight(); break;
                case Stage.BeforeFight:
                    if (_squad.Count == Mathf.Min(SliceRun.SquadSize, _run.Reserve.Count)) StartFight();
                    break;
                case Stage.AfterFight:
                    if (_run.Over) ShowFinal();
                    else if (_run.WarPending) ShowWar();
                    else ShowBeforeFight();
                    break;
                case Stage.War: ShowBeforeFight(); break;
                case Stage.Final:
                    if (autopilot) Finished = true;
                    else { worldSeed++; StartRun(); }
                    break;
            }
        }

        void SetStage(Stage stage)
        {
            CurrentStage = stage;
            _stageClock = 0f;
        }

        void ShowIntro(int seed)
        {
            SetStage(Stage.Intro);
            _cardTitle = "FALLEN STAR";
            _card.Clear();
            Line("The Butcher leads. You choose five fighters and give the orders.", Ink);
            Line("Break a tribe's courage to recruit its surviving fighters. Your new recruits can join the next squad.", Ink);
            Line("Win a chief's recipe, steal Eclipse, and face the raider chief when he returns with what he learned.", Amber);
            Line("Fight one: Bloodridge. Next: Skullpeak. Then the chief's rematch.", Dim);
            Line("The Butcher's art is a placeholder while Jacob draws the Pal.", Dim);
            if (autopilot) Line("AUTOPILOT / seed " + seed + ". Scripted controls; the seed is not searched for a win.", Dim);
        }

        void StartFight()
        {
            ClearFight();
            _battleEnemy = _run.NextEnemy;
            _setup = _run.NextFight(_squad);
            _sim = new BattleSimulation(_setup);
            _session = new SimSession(_sim);
            _eventCursor = 0;
            _tickAccumulator = 0f;
            _endHold = 0f;
            _paused = false;
            _targeting = null;
            _freeze = 0f;
            _impactCooldown = 0f;
            _focusTime = 0f;
            _pilot = new SliceAutopilot(SliceAutopilot.Style.Smart);
            _feed.Clear();
            _showRival = false;
            SpawnUnits();
            SetStage(Stage.Fight);
            Feed("Fight " + (_fights.Count + 1) + " begins.");
        }

        void UpdateFight(float dt)
        {
            if (_paused || _showCodex || _showRival || _sim == null) return;
            if (!_sim.IsFinished)
            {
                // Hit stop: the fight holds after a kill or a big hit.
                if (_freeze > 0f)
                {
                    _freeze -= dt;
                    return;
                }
                _tickAccumulator += dt * SimClock.TicksPerSecond * Mathf.Max(0.1f, fightSpeed);
                while (_tickAccumulator >= 1f && !_sim.IsFinished && _freeze <= 0f)
                {
                    _tickAccumulator -= 1f;
                    if (autopilot) _pilot.Step(_session, _sim);
                    _session.Tick();
                    CountDownFx();
                    DrainEvents();
                }
                if (_sim.IsFinished)
                {
                    bool won = _sim.Winner == SliceRun.PlayerSide;
                    if (_sim.BrokenSide < 0)
                        AddBanner(won ? "VICTORY" : _sim.Winner < 0 ? "DRAW" : "DEFEAT", string.Empty, won ? Amber : Rival, EndHoldSeconds + 1f);
                }
                return;
            }

            _endHold += dt;
            if (_endHold > (_sim.BrokenSide >= 0 ? BreakHoldSeconds : EndHoldSeconds)) ShowAfterFight();
        }

        void ShowAfterFight()
        {
            RunFightResult fight = _run.FinishFight(_setup, _session);
            _fights.Add(fight);
            SetStage(Stage.AfterFight);
            _cardTitle = fight.Won ? "VICTORY  /  " + EnemyName : "YOUR RUN ENDS";
            _card.Clear();
            Line("Fight " + _fights.Count + " finished in " + Seconds(fight.Record.EndTick) + ".", Dim);
            if (!fight.Won)
            {
                Line(fight.ButcherDown ? "The Butcher fell and your line broke." : "Your line could not hold this fight.", Rival);
                Line("The next run starts with a fresh squad. The chiefs and your discoveries remember this session.", Ink);
            }
            else
            {
                Heading(fight.Joined.Count + " JOINED YOUR RESERVE", Amber);
                var names = new List<string>();
                foreach (RunFighter fighter in fight.Joined) names.Add(fighter.DisplayName);
                Line(string.Join(", ", names), Ink);
                if (fight.ChiefEscaped) Line("The raider chief escaped with his home crew. He will return.", Rival);
                if (fight.ChiefCaught) Line("The chief is caught and joins your reserve.", Teal);
                if (fight.StolenGrade > 0) Line("STOLEN: Eclipse, grade " + fight.StolenGrade + ". It is now on your order bar.", Amber);
                if (fight.LearnedRecipe != GearKind.None) Line("THE BUTCHER LEARNED: " + Gear.Name(fight.LearnedRecipe) + ".", Amber);
                if (fight.Forged != GearKind.None)
                    Line("FORGED: " + Gear.Name(fight.Forged) + ". " + Gear.Describe(fight.Forged) + ". Choose its wearer below.", Teal);
            }
            if (fight.Fallen.Count > 0)
            {
                var fallen = new List<string>();
                foreach (RunFighter f in fight.Fallen) fallen.Add(f.DisplayName);
                Line("LOST THIS RUN: " + string.Join(", ", fallen) + ".", Rival);
            }
            Heading("THE CHIEF'S CHANGED TACTICS", Rival);
            AddLessons(fight.EnemyLessons, Ink, 3);
            _cardScroll = Vector2.zero;
        }

        void ShowWar()
        {
            _lastWar = _run.RunWar();
            SetStage(Stage.War);
            _cardTitle = "WHILE YOU REGROUP";
            _card.Clear();
            Line("The escaped chief's splinter crew has raided Skullpeak.", Ink);
            if (_lastWar.Fought)
                Line((_lastWar.Winner == 0 ? "The splinter crew won." : _lastWar.Winner == 1 ? "Skullpeak won." : "Neither side won.") +
                    " Splinter losses: " + _lastWar.SplinterDead + ". Skullpeak losses: " + _lastWar.SkullpeakDead + ".", Ink);
            Line("Skullpeak now has " + _run.Skullpeak.Strength + " fighters and " + Percent(_run.Skullpeak.Nerve) + " courage after regrouping.", Amber);
            Heading("WHAT THE CHIEF TOOK FROM THE WAR", Rival);
            AddLessons(_lastWar.SplinterLessons, Ink, 3);
            _cardScroll = Vector2.zero;
        }

        void ShowBeforeFight()
        {
            SetStage(Stage.BeforeFight);
            _cardTitle = "PICK YOUR FIVE";
            _reserveScroll = Vector2.zero;
            _squad.Clear();
            _squad.AddRange(_run.DefaultSquad());
        }

        void ShowFinal()
        {
            SetStage(Stage.Final);
            _cardTitle = _run.Stage == RunStage.Won ? "THE RAIDER CHIEF IS YOURS" : "REGROUP AT FALLEN STAR";
            _card.Clear();
            var summary = new System.Text.StringBuilder();
            foreach (RunFightResult fight in _fights)
            {
                string line = fight.Fought + ": " + (fight.Won ? "victory" : "defeat") + " in " + Seconds(fight.Record.EndTick) +
                    "; " + fight.Joined.Count + " recruits.";
                Line(line, Ink);
                summary.AppendLine(line);
            }
            Line("Reserve: " + _run.Reserve.Count + " fighters. Discoveries this session: " + _memory.Codex.Count + ".", Amber);
            Line("A new run resets the squad and gear. Chief memories and discoveries remain for this session.", Dim);
            Summary = summary.ToString();
            _cardScroll = Vector2.zero;
        }

        // ------------------------------------------------------------ orders

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_showCodex) { _showCodex = false; return; }
                if (_showGear) { _showGear = false; return; }
                if (_showRival) { _showRival = false; return; }
            }
            if (Input.GetKeyDown(KeyCode.Space) && CurrentStage == Stage.Fight) _paused = !_paused;
            if (CurrentStage != Stage.Fight || autopilot || _showCodex || _showRival || _sim == null || _sim.IsFinished) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) PickOrder(Directive.Hold);
            if (Input.GetKeyDown(KeyCode.Alpha2)) PickOrder(Directive.FallBack);
            if (Input.GetKeyDown(KeyCode.Alpha3)) PickOrder(Directive.FocusFire);
            if (Input.GetKeyDown(KeyCode.Alpha4)) PickOrder(Directive.Unleash);
            if (Input.GetKeyDown(KeyCode.Alpha5)) PickOrder(Directive.Sacrifice);
            if (Input.GetKeyDown(KeyCode.Alpha6)) PickOrder(Directive.Eclipse);
            if (Input.GetKeyDown(KeyCode.Alpha7)) PickOrder(Directive.Frenzy);
            if (Input.GetKeyDown(KeyCode.Escape)) _targeting = null;
            if (_targeting.HasValue && Input.GetMouseButtonDown(0) && _camera != null && PointerInBattle())
            {
                if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f) && hit.collider.transform.parent != null)
                {
                    IssueOrder(_targeting.Value, hit.collider.transform.parent.name);
                    // Retain targeting after an invalid target; Esc cancels it.
                }
            }
        }

        void PickOrder(Directive directive)
        {
            if (_sim == null || !_sim.CanUse(0, directive, out _)) return;
            if (directive == Directive.FocusFire || directive == Directive.Sacrifice)
            {
                _targeting = directive;
                return;
            }
            IssueOrder(directive, string.Empty);
        }

        void IssueOrder(Directive directive, string targetId)
        {
            if (_session == null || _sim.IsFinished) return;
            if (!_session.TryIssue(Directives.CommandKind, "0", targetId, 0, 0, Directives.Name(directive), out string failure))
                AddBanner("Can't: " + failure, string.Empty, Dim, 1.6f);
            else _targeting = null;
            DrainEvents();
        }

        // ------------------------------------------------------------ events

        void DrainEvents()
        {
            if (_sim == null) return;
            for (; _eventCursor < _sim.Events.Count; _eventCursor++) OnEvent(_sim.Events[_eventCursor]);
        }

        void CountDownFx()
        {
            for (int s = 0; s < 2; s++)
            {
                SideFx fx = _fx[s];
                if (fx.Hold > 0) fx.Hold--;
                if (fx.FallBack > 0) fx.FallBack--;
                if (fx.Unleash > 0) fx.Unleash--;
                if (fx.Focus > 0) fx.Focus--;
            }
        }

        void OnEvent(BattleEvent e)
        {
            if (PresentRunEvent(e)) return;
            switch (e.Kind)
            {
                case BattleEventKind.Hit:
                    OnHit(e);
                    break;
                case BattleEventKind.Heal:
                    if (View(e.TargetId) is UnitView healed)
                    {
                        Flash(healed, HealFlash);
                        AddFloat(healed, "+" + e.Amount, HealFlash, false);
                    }
                    break;
                case BattleEventKind.Guard:
                    if (View(e.TargetId) is UnitView ward) AddFloat(ward, "covered", Teal, false);
                    break;
                case BattleEventKind.TrapArmed:
                    SetTrap(e.ActorSide, true);
                    break;
                case BattleEventKind.TrapSprung:
                    SetTrap(e.ActorSide, false);
                    if (View(e.TargetId) is UnitView caught) AddFloat(caught, "TRAP", new Color(1f, 0.6f, 0.2f), true);
                    Feed(Name(e.TargetId) + " springs a trap.");
                    break;
                case BattleEventKind.Kill:
                    Feed(Name(e.TargetId) + (e.Detail == BattleSimulation.SacrificeDetail ? " is sacrificed." : " falls."));
                    if (e.Detail != BattleSimulation.SacrificeDetail) StartImpact(View(e.ActorId), View(e.TargetId), true);
                    break;
                case BattleEventKind.Directive:
                    OnDirective(e);
                    break;
                case BattleEventKind.Break:
                    OnBreak(e.ActorSide);
                    break;
            }
        }

        /// A side's courage ran out. Raiders who break walk over to the Clarth; Clarth who break flee.
        void OnBreak(int side)
        {
            _shake = Mathf.Max(_shake, BreakShake);
            if (side == SliceRun.PlayerSide)
            {
                AddBanner("YOUR LINE BREAKS", "Your fighters retreat. The enemy wins this one.", Rival, BreakHoldSeconds + 1f);
                Feed("Your line breaks.");
            }
            else
            {
                AddBanner(EnemyName.ToUpperInvariant() + " BREAKS", "Survivors join you. The fight is won.", Amber, BreakHoldSeconds + 1f);
                Feed(EnemyName + " breaks. The survivors are yours.");
            }
            int walking = 0;
            for (int i = 0; i < _viewOrder.Count; i++)
            {
                UnitView v = _viewOrder[i];
                if (v.Unit.Side != side || !v.Unit.Alive) continue;
                if (side == SliceRun.PlayerSide || (v.Unit.Role == UnitRole.Chief && _run.Stage == RunStage.Raiders))
                {
                    v.Fleeing = true;
                    v.WalkGoal = new Vector3(side == 0 ? -9f : 9f, 0f, v.Home.z);
                }
                else
                {
                    v.WalkOver = true;
                    v.WalkGoal = new Vector3(-3.8f - 1.0f * (walking % 2), 0f, 3.4f - 1.3f * (walking / 2));
                    walking++;
                }
            }
        }

        void OnHit(BattleEvent e)
        {
            UnitView victim = View(e.TargetId);
            UnitView attacker = View(e.ActorId);
            if (victim == null) return;
            bool big = e.Amount >= BigHit && e.Detail.Length == 0;
            Flash(victim, HitFlash);
            AddFloat(victim, "-" + e.Amount, e.ActorSide == 0 ? Amber : Rival, big);
            if (attacker == null || e.Detail == BattleSimulation.TrapDetail) return;
            if (SliceClasses.Stats(attacker.Unit.Class).Ranged) Fire(attacker, victim, e.ActorSide == 0 ? Amber : Rival);
            else
            {
                attacker.Lunge = LungeTime;
                Vector3 dir = victim.Pos - attacker.Pos;
                dir.y = 0f;
                attacker.LungeDir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.right;
            }
            if (big) StartImpact(attacker, victim, false);
        }

        void OnDirective(BattleEvent e)
        {
            int side = e.ActorSide;
            SideFx fx = _fx[side];
            string detail = e.Detail;
            if (detail == Directives.Name(Directive.Hold)) fx.Hold = BattleSimulation.HoldTicks;
            else if (detail == Directives.Name(Directive.FallBack)) fx.FallBack = BattleSimulation.FallBackTicks;
            else if (detail == Directives.Name(Directive.Unleash)) fx.Unleash = BattleSimulation.UnleashTicks;
            else if (detail == Directives.Name(Directive.FocusFire))
            {
                fx.Focus = BattleSimulation.FocusTicks;
                fx.FocusId = e.TargetId;
            }

            bool auto = e.ActorId.StartsWith(BattleSimulation.AutoActorPrefix, System.StringComparison.Ordinal);
            string target = e.TargetId.Length > 0 ? " on " + Name(e.TargetId) : string.Empty;
            if (!auto && side == SliceRun.PlayerSide)
            {
                _lastOrder = detail;
                _lastOrderAge = 0f;
                AddBanner("YOUR ORDER: " + Pretty(detail).ToUpperInvariant(), target.Trim(), Amber, 1.8f);
                Feed("You order " + Pretty(detail) + target + ".");
                return;
            }
            if (!auto || detail == Directives.Name(Directive.Eclipse)) return;

            TribeMind mind = side == SliceRun.PlayerSide ? _memory.ClarthMind : _battleEnemy.Mind;
            Lesson taught = LessonFor(mind, detail);
            string sub = taught == null ? string.Empty : "Learned in an earlier fight.";
            if (side == SliceRun.PlayerSide)
            {
                AddBanner("YOUR FIGHTERS ANSWER: " + Pretty(detail).ToUpperInvariant(), sub, Teal, 3.2f);
                Feed("Your fighters answer with " + Pretty(detail) + " (learned).");
            }
            else
            {
                AddBanner("THE CHIEF ANSWERS: " + Pretty(detail).ToUpperInvariant() + target.ToUpperInvariant(), sub, Rival, 3.6f);
                Feed("The chief answers with " + Pretty(detail) + target + " (learned).");
            }
        }

        static Lesson LessonFor(TribeMind mind, string detail)
        {
            string rule = detail == Directives.Name(Directive.Hold) ? LessonRules.Unleash :
                detail == Directives.Name(Directive.Unleash) ? LessonRules.FallBack :
                detail == BattleSimulation.CoverDetail ? LessonRules.Focus : string.Empty;
            if (rule.Length == 0) return null;
            for (int i = mind.Lessons.Count - 1; i >= 0; i--)
            {
                if (mind.Lessons[i].Rule == rule) return mind.Lessons[i];
            }
            return null;
        }

        // ------------------------------------------------------------ impact

        /// Kills always land an impact; big hits only when none landed lately.
        void StartImpact(UnitView attacker, UnitView target, bool kill)
        {
            if (target == null) return;
            if (!kill && _impactCooldown > 0f) return;
            _impactCooldown = ImpactCooldown;
            _freeze = Mathf.Max(_freeze, kill ? KillFreeze : HitFreeze);
            _shake = Mathf.Max(_shake, kill ? KillShake : HitShake);
            _focusA = attacker;
            _focusB = target;
            _focusTime = FocusTime;
            target.WhiteFlash = ImpactFlash;
            if (attacker != null && attacker != target) attacker.Pose = PoseTime;
            target.Knock = KnockTime;
            Vector3 from = attacker != null ? attacker.Root.transform.position : target.Root.transform.position;
            Vector3 away = target.Root.transform.position - from;
            away.y = 0f;
            target.KnockDir = away.sqrMagnitude > 0.001f ? away.normalized : (target.Unit.Side == 0 ? Vector3.left : Vector3.right);
        }

        void UpdateCameraAndLights(float dt)
        {
            if (_impactCooldown > 0f) _impactCooldown -= dt;
            if (_focusTime > 0f) _focusTime -= dt;
            Vector3 shake = Vector3.zero;
            if (_shake > 0f)
            {
                _shake = Mathf.Max(0f, _shake - dt);
                float amount = _shake * 0.45f;
                shake = new Vector3(Mathf.PerlinNoise(_animClock * 37f, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, _animClock * 37f) - 0.5f, 0f) * amount;
            }
            _camera.transform.position = CameraHome + shake;
            _camera.transform.LookAt(CameraLook + shake);
            for (int i = 0; i < _torches.Count; i++)
                _torches[i].intensity = 1.6f + 0.7f * (Mathf.PerlinNoise(_animClock * 5f + i * 11.3f, 0.5f) - 0.5f);
        }

        static Texture2D MakeVignette()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float d = Mathf.Sqrt(dx * dx * 0.75f + dy * dy * 1.15f);
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.5f, 1.35f, d));
                    pixels[y * size + x] = new Color32(0, 0, 0, (byte)(a * 255f));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        /// A soft round puff for dust.
        static Sprite MakeDot()
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * a * 255f));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        // ------------------------------------------------------------ stage and units

        void BuildStage()
        {
            _stageRoot = new GameObject("Stage").transform;

            var cameraObject = new GameObject("Slice camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(_stageRoot);
            _camera = cameraObject.AddComponent<Camera>();
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.035f, 0.03f, 0.028f);
            _camera.orthographic = true;
            _camera.orthographicSize = 5.2f;
            cameraObject.transform.position = CameraHome;
            cameraObject.transform.LookAt(CameraLook);

            // Dim warm light, low ambient and torches: the mood comes from what is not lit.
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.transform.SetParent(_stageRoot);
            sun.type = LightType.Directional;
            sun.intensity = 0.55f;
            sun.color = new Color(1f, 0.8f, 0.6f);
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.2f, 0.21f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = _camera.backgroundColor;
            RenderSettings.fogStartDistance = 12f;
            RenderSettings.fogEndDistance = 24f;

            GameObject ground = Block("Ground", PrimitiveType.Cube, new Vector3(0f, -0.1f, 1.2f), new Vector3(17f, 0.2f, 9.5f), GroundTint);
            var sand = Resources.Load<Texture2D>("RivalSlice/Ground/sand");
            if (sand != null)
            {
                Material groundMaterial = ground.GetComponent<Renderer>().material;
                groundMaterial.mainTexture = sand;
                groundMaterial.mainTextureScale = new Vector2(4f, 2.2f);
            }
            Block("Centre line", PrimitiveType.Cube, new Vector3(0f, 0.01f, 1.2f), new Vector3(0.05f, 0.02f, 9f), new Color(0.3f, 0.26f, 0.2f));
            Block("Rock", PrimitiveType.Sphere, new Vector3(-4.9f, 0.25f, 4.3f), new Vector3(1.6f, 0.9f, 1.2f), new Color(0.5f, 0.44f, 0.36f));
            Block("Rock", PrimitiveType.Sphere, new Vector3(4.6f, 0.2f, 4.9f), new Vector3(1.3f, 0.75f, 1f), new Color(0.48f, 0.42f, 0.34f));
            Block("Rock", PrimitiveType.Sphere, new Vector3(-4.4f, 0.15f, -1.6f), new Vector3(0.8f, 0.45f, 0.7f), new Color(0.5f, 0.44f, 0.36f));

            // Torches behind the lines, close enough to light the fighters.
            Vector3[] feet = { new Vector3(-3.4f, 0f, 4.1f), new Vector3(2.9f, 0f, 4.3f) };
            for (int t = 0; t < feet.Length; t++)
            {
                Vector3 foot = feet[t];
                Block("Torch pole", PrimitiveType.Cylinder, foot + new Vector3(0f, 0.75f, 0f), new Vector3(0.12f, 0.75f, 0.12f), new Color(0.16f, 0.12f, 0.09f));
                GameObject fire = Block("Torch fire", PrimitiveType.Sphere, foot + new Vector3(0f, 1.6f, 0f), new Vector3(0.3f, 0.42f, 0.3f), TorchColor);
                Material fireMaterial = fire.GetComponent<Renderer>().material;
                fireMaterial.EnableKeyword("_EMISSION");
                fireMaterial.SetColor("_EmissionColor", TorchColor * 2.2f);
                var light = new GameObject("Torch light").AddComponent<Light>();
                light.transform.SetParent(_stageRoot);
                light.transform.position = foot + new Vector3(0f, 1.8f, 0f);
                light.type = LightType.Point;
                light.range = 9f;
                light.color = TorchColor;
                light.intensity = 1.6f;
                _torches.Add(light);
            }

            for (int s = 0; s < 2; s++)
            {
                _traps[s] = Block("Trap", PrimitiveType.Cylinder, new Vector3(s == 0 ? -0.6f : 0.6f, 0.02f, 1.2f), new Vector3(0.6f, 0.02f, 0.6f), new Color(0.55f, 0.3f, 0.75f));
                _traps[s].SetActive(false);
                _focusRings[s] = Block("Focus", PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.25f, 0.01f, 1.25f), s == 0 ? Amber : Rival);
                _focusRings[s].SetActive(false);
            }

            _unitsRoot = new GameObject("Units").transform;
            _unitsRoot.SetParent(_stageRoot);
        }

        static void Surface(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            Material source = Resources.Load<Material>("RivalSlice/StageSurface");
            if (source != null) renderer.sharedMaterial = source;
            renderer.material.color = color;
        }

        GameObject Block(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(_stageRoot);
            go.transform.position = position;
            go.transform.localScale = scale;
            Destroy(go.GetComponent<Collider>());
            Surface(go, color);
            return go;
        }

        void ClearFight()
        {
            for (int i = 0; i < _viewOrder.Count; i++)
            {
                Destroy(_viewOrder[i].Root);
                if (_viewOrder[i].Tether != null) Destroy(_viewOrder[i].Tether);
            }
            _views.Clear();
            _viewOrder.Clear();
            for (int i = 0; i < _shots.Count; i++) Destroy(_shots[i].Ball);
            _shots.Clear();
            for (int i = 0; i < _dust.Count; i++) Destroy(_dust[i].Renderer.gameObject);
            _dust.Clear();
            _floats.Clear();
            _banners.Clear();
            _focusA = null;
            _focusB = null;
            for (int s = 0; s < 2; s++)
            {
                _fx[s] = new SideFx();
                _traps[s].SetActive(false);
                _focusRings[s].SetActive(false);
            }
        }

        void SpawnUnits()
        {
            for (int side = 0; side < 2; side++)
            {
                for (int row = 0; row < 2; row++)
                {
                    var members = new List<BattleUnit>();
                    for (int i = 0; i < _sim.Units.Count; i++)
                    {
                        BattleUnit u = _sim.Units[i];
                        if (u.Side == side && (int)u.Row == row) members.Add(u);
                    }
                    for (int i = 0; i < members.Count; i++)
                    {
                        float sign = side == 0 ? -1f : 1f;
                        int frontCount = 0;
                        foreach (BattleUnit u in _sim.Units) if (u.Side == side && u.Row == Row.Front) frontCount++;
                        // Fan a crowded row into columns of three without changing its combat row.
                        int column = i / 3;
                        int inColumn = Mathf.Min(3, members.Count - column * 3);
                        float rowX = row == (int)Row.Front ? 1.6f : frontCount > 3 ? 5.2f : 4.0f;
                        float x = sign * (rowX + column * 1.6f);
                        float z = 1.2f + (i % 3 - (inColumn - 1) * 0.5f) * 2.2f;
                        UnitView view = MakeView(members[i], new Vector3(x, 0f, z));
                        _views[members[i].Id] = view;
                        _viewOrder.Add(view);
                    }
                }
            }
        }

        /// Sprite sheet prefix per side: the Clarth (player) and the Bloodridge raiders.
        string ArtPrefix(BattleUnit unit)
        {
            if (unit.Role == UnitRole.Pal) return string.Empty;
            RunFighter recruit = unit.Side == 0 ? _run.FindFighter(unit.Id) : null;
            string tribe = recruit != null ? recruit.TribeId : _battleEnemy.Id;
            return tribe == SliceRun.ClarthId ? "clarth" : tribe == SliceRun.SkullpeakId ? "skull" : "blood_rot";
        }

        /// Old single stills, kept as a fallback: clarth_warden, raider_medic, and so on.
        string ArtName(BattleUnit unit)
        {
            string prefix = ArtPrefix(unit);
            if (prefix.Length == 0) return string.Empty;
            return (prefix == "blood_rot" ? "raider" : prefix) + "_" + ArtPiece(unit);
        }

        Sprite LoadSprite(string name)
        {
            if (_sprites.TryGetValue(name, out Sprite cached)) return cached;
            var texture = string.IsNullOrEmpty(name) ? null : Resources.Load<Texture2D>("RivalSlice/Units/" + name);
            Sprite sprite = null;
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0f), PixelsPerUnit);
            }
            _sprites[name] = sprite;
            return sprite;
        }

        /// A breathing idle: four frames side by side, feet on the bottom edge. Facing "se" looks
        /// right, "sw" looks left. Returns null when the sheet is missing.
        Sprite[] LoadSheet(string prefix, string piece, string facing)
        {
            string name = prefix + "_" + piece + "_idle_" + facing;
            if (_sheets.TryGetValue(name, out Sprite[] cached)) return cached;
            var texture = string.IsNullOrEmpty(prefix) ? null : Resources.Load<Texture2D>("RivalSlice/Sheets/" + name);
            Sprite[] frames = null;
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                int w = texture.width / SheetFrames;
                frames = new Sprite[SheetFrames];
                for (int i = 0; i < SheetFrames; i++)
                    frames[i] = Sprite.Create(texture, new Rect(i * w, 0f, w, texture.height), new Vector2(0.5f, 0f), SheetPixelsPerUnit);
            }
            _sheets[name] = frames;
            return frames;
        }

        UnitView MakeView(BattleUnit unit, Vector3 home)
        {
            var root = new GameObject(unit.Id);
            root.transform.SetParent(_unitsRoot);
            root.transform.position = home;
            var view = new UnitView { Unit = unit, Root = root, Home = home, Pos = home, Phase = unit.Index * 0.37f };

            Part(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.012f, 0f), new Vector3(1.05f, 0.004f, 0.55f), new Color(0.08f, 0.07f, 0.06f));

            // The Clarth stand on the left and face right; the raiders face left. Converts turn when they arrive.
            string facing = unit.Side == 0 ? "se" : "sw";
            Sprite[] idle = LoadSheet(ArtPrefix(unit), ArtPiece(unit), facing);
            Sprite still = idle != null ? idle[0] : LoadSprite(ArtName(unit));
            if (still != null)
            {
                var art = new GameObject("Art");
                art.transform.SetParent(root.transform, false);
                SpriteRenderer renderer = art.AddComponent<SpriteRenderer>();
                renderer.sprite = still;
                renderer.flipX = idle == null && unit.Side == 0; // the old stills face left
                float height = still.rect.height / still.pixelsPerUnit;
                BoxCollider clickArea = art.AddComponent<BoxCollider>();
                clickArea.center = new Vector3(0f, height * 0.45f, 0f);
                clickArea.size = new Vector3(1.3f, height * 0.9f, 0.2f);
                view.Art = renderer;
                view.Body = art.transform;
                view.BaseColor = Color.white;
                view.Height = idle != null ? height * 0.82f : 1.95f;
                view.Idle = idle;
                view.IdleTurned = idle != null ? LoadSheet(ArtPrefix(unit), ArtPiece(unit), unit.Side == 0 ? "sw" : "se") : null;
                if (_silhouetteMaterial != null)
                {
                    // The same shape in flat white, drawn just in front: a real white flash.
                    var flash = new GameObject("Flash");
                    flash.transform.SetParent(art.transform, false);
                    flash.transform.localPosition = new Vector3(0f, 0f, -0.02f);
                    SpriteRenderer silhouette = flash.AddComponent<SpriteRenderer>();
                    silhouette.sprite = still;
                    silhouette.flipX = renderer.flipX;
                    silhouette.sharedMaterial = _silhouetteMaterial;
                    silhouette.color = new Color(1f, 1f, 1f, 0f);
                    view.Silhouette = silhouette;
                }
            }
            else
            {
                float size = unit.Class == UnitClass.Warden ? 0.95f : unit.Class == UnitClass.Skirmisher ? 0.72f : 0.82f;
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = new Vector3(size * 0.8f, size, size * 0.8f);
                body.transform.localPosition = new Vector3(0f, size, 0f);
                Color baseColor = unit.Side == 0 ? ClarthBody : RaiderBody;
                Renderer bodyRenderer = body.GetComponent<Renderer>();
                Surface(body, baseColor);
                Part(body.transform, PrimitiveType.Cube, new Vector3(0f, 1.05f, 0f), new Vector3(0.6f, 0.3f, 0.6f), ClassColor(unit.Class));
                view.Body = body.transform;
                view.BodyRenderer = bodyRenderer;
                view.BaseColor = baseColor;
                view.Height = size * 2f;
            }

            GameObject disc = Part(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), new Vector3(1.15f, 0.01f, 1.15f), Color.white);
            disc.SetActive(false);
            view.Disc = disc;
            view.DiscRenderer = disc.GetComponent<Renderer>();
            return view;
        }

        static GameObject Part(Transform parent, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Destroy(go.GetComponent<Collider>());
            Surface(go, color);
            return go;
        }

        void UpdateViews(float dt)
        {
            float focus = Mathf.Clamp01(_focusTime / FocusFade);
            for (int i = 0; i < _viewOrder.Count; i++)
            {
                UnitView v = _viewOrder[i];
                BattleUnit u = v.Unit;
                Vector3 goal = v.Home;
                if (u.Alive && u.Row == Row.Front && _fx[u.Side].FallBack > 0) goal.x += u.Side == 0 ? -1.1f : 1.1f;

                // A broken side's survivors walk: raiders cross over to join the Clarth, Clarth flee.
                if (u.Alive && (v.WalkOver || v.Fleeing))
                {
                    Vector3 to = v.WalkGoal - v.Pos;
                    to.y = 0f;
                    float step = WalkOverSpeed * dt;
                    if (to.magnitude <= step)
                    {
                        v.Pos = v.WalkGoal;
                        if (v.WalkOver && !v.Arrived)
                        {
                            v.Arrived = true;
                            AddFloat(v, "JOINS YOU", Amber, false);
                        }
                    }
                    else v.Pos += to.normalized * step;
                }
                else v.Pos = Vector3.Lerp(v.Pos, goal, 1f - Mathf.Exp(-dt * 7f));

                Vector3 offset = Vector3.zero;
                if (v.Lunge > 0f)
                {
                    v.Lunge -= dt;
                    offset += v.LungeDir * 0.6f * Mathf.Sin(Mathf.Clamp01(1f - v.Lunge / LungeTime) * Mathf.PI);
                }
                float pose = 0f;
                if (v.Pose > 0f)
                {
                    v.Pose -= dt;
                    pose = Mathf.Sin(Mathf.Clamp01(1f - v.Pose / PoseTime) * Mathf.PI);
                    offset += v.LungeDir * 0.55f * pose;
                }
                float knock = 0f;
                if (v.Knock > 0f)
                {
                    v.Knock -= dt;
                    knock = Mathf.Sin(Mathf.Clamp01(1f - v.Knock / KnockTime) * Mathf.PI);
                    offset += v.KnockDir * 0.5f * knock;
                }

                // Dying: stagger back, fall over with a small bounce, then fade low.
                float fall = 0f;
                float fade = 1f;
                if (!u.Alive && v.Art != null)
                {
                    if (v.DeathAge < 0f) StartDeath(v);
                    v.DeathAge += dt;
                    float k = Mathf.Clamp01(v.DeathAge / DeathTime);
                    fall = EaseOutBack(k);
                    offset += (u.Side == 0 ? Vector3.left : Vector3.right) * 0.45f * (1f - (1f - k) * (1f - k));
                    fade = Mathf.Lerp(1f, 0.45f, Mathf.Clamp01((v.DeathAge - DeathTime) / 0.5f));
                }
                if (v.Fleeing && u.Alive) fade = Mathf.Clamp01(1f - Mathf.Abs(v.Home.x - v.Pos.x) / 6f);
                v.Root.transform.position = v.Pos + offset;

                // During an impact the pair is lit and everyone else sinks into the dark.
                bool inFocus = _focusTime > 0f && (v == _focusA || v == _focusB);
                Color light = inFocus ? Color.Lerp(MoodTint, Color.white, focus) : MoodTint * (1f - 0.5f * focus);
                Color color = u.Alive ? v.BaseColor : (v.Art != null ? Color.Lerp(v.BaseColor, DeadTint, Mathf.Clamp01(v.DeathAge / DeathTime)) : Fallen);
                color = new Color(color.r * light.r, color.g * light.g, color.b * light.b, fade);
                if (v.Flash > 0f)
                {
                    v.Flash -= dt;
                    color = Color.Lerp(color, new Color(v.FlashColor.r, v.FlashColor.g, v.FlashColor.b, fade), Mathf.Clamp01(v.Flash / FlashTime));
                }

                if (v.Art != null)
                {
                    // Sprites face the camera and breathe; old stills bob a little instead.
                    float bob = u.Alive && v.Idle == null ? Mathf.Sin(_animClock * 3.2f + v.Phase) * 0.04f : 0f;
                    v.Body.localPosition = new Vector3(0f, bob, 0f);
                    v.Body.localScale = new Vector3(1f + 0.22f * pose, (1f - 0.1f * pose) * (1f - 0.12f * Mathf.Clamp01(fall)), 1f);
                    Quaternion face = _camera.transform.rotation;
                    float side = u.Side == 0 ? 1f : -1f;
                    float tilt = u.Alive ? side * 12f * knock : side * 86f * fall;
                    v.Body.rotation = face * Quaternion.Euler(0f, 0f, tilt);
                    Sprite[] frames = v.Arrived && v.IdleTurned != null ? v.IdleTurned : v.Idle;
                    if (u.Alive && frames != null)
                        v.Art.sprite = frames[(int)((_animClock + v.Phase) / IdleFrameSeconds) % frames.Length];
                    v.Art.color = color;
                    if (v.Silhouette != null)
                    {
                        if (v.WhiteFlash > 0f) v.WhiteFlash -= dt;
                        v.Silhouette.sprite = v.Art.sprite;
                        v.Silhouette.color = new Color(1f, 1f, 1f, 0.9f * Mathf.Clamp01(v.WhiteFlash / ImpactFlash));
                    }
                }
                else
                {
                    v.BodyRenderer.material.color = color;
                    if (!u.Alive && !v.ShownDead)
                    {
                        v.ShownDead = true;
                        v.Body.localRotation = Quaternion.Euler(0f, 0f, u.Side == 0 ? 82f : -82f);
                        v.Body.localPosition = new Vector3(0f, 0.32f, 0f);
                    }
                }

                SideFx fx = _fx[u.Side];
                bool walking = v.WalkOver || v.Fleeing;
                bool unleashed = u.Alive && !walking && fx.Unleash > 0;
                bool holding = u.Alive && !walking && fx.Hold > 0 && u.Row == Row.Front;
                v.Disc.SetActive(unleashed || holding);
                if (unleashed) v.DiscRenderer.material.color = new Color(0.9f, 0.25f, 0.2f);
                else if (holding) v.DiscRenderer.material.color = new Color(0.3f, 0.55f, 0.95f);

                UpdateTether(v);
            }

            for (int s = 0; s < 2; s++)
            {
                SideFx fx = _fx[s];
                UnitView focused = fx.Focus > 0 ? View(fx.FocusId) : null;
                bool show = focused != null && focused.Unit.Alive && !focused.WalkOver;
                _focusRings[s].SetActive(show);
                if (show) _focusRings[s].transform.position = focused.Root.transform.position + new Vector3(0f, 0.03f, 0f);
            }
        }

        void StartDeath(UnitView v)
        {
            v.DeathAge = 0f;
            v.WhiteFlash = ImpactFlash;
            SpawnDust(v.Pos);
        }

        /// 0 to 1 with a small overshoot near the end, so a falling body lands with a bounce.
        static float EaseOutBack(float k)
        {
            const float c1 = 1.2f;
            const float c3 = c1 + 1f;
            float t = k - 1f;
            return 1f + c3 * t * t * t + c1 * t * t;
        }

        void SpawnDust(Vector3 at)
        {
            if (_dot == null) return;
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f + 0.3f;
                var go = new GameObject("Dust");
                go.transform.SetParent(_unitsRoot);
                go.transform.position = at + new Vector3(Mathf.Cos(a) * 0.25f, 0.15f, Mathf.Sin(a) * 0.12f);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = _dot;
                renderer.color = new Color(DustColor.r, DustColor.g, DustColor.b, 0.8f);
                _dust.Add(new Dust
                {
                    Renderer = renderer,
                    Velocity = new Vector3(Mathf.Cos(a) * 1.6f, 0.6f + 0.3f * (i % 3), Mathf.Sin(a) * 0.6f),
                    Size = 0.45f + 0.15f * (i % 2)
                });
            }
        }

        void UpdateTether(UnitView v)
        {
            BattleUnit u = v.Unit;
            UnitView ward = u.Class == UnitClass.Warden && u.Alive && !v.WalkOver && !v.Fleeing && u.GuardTargetId.Length > 0 ? View(u.GuardTargetId) : null;
            if (ward == null || !ward.Unit.Alive)
            {
                if (v.Tether != null) v.Tether.SetActive(false);
                return;
            }
            if (v.Tether == null)
            {
                v.Tether = GameObject.CreatePrimitive(PrimitiveType.Cube);
                v.Tether.name = "Guard";
                v.Tether.transform.SetParent(_unitsRoot);
                Destroy(v.Tether.GetComponent<Collider>());
                Surface(v.Tether, Teal);
            }
            Vector3 a = v.Root.transform.position + Vector3.up * 0.15f;
            Vector3 b = ward.Root.transform.position + Vector3.up * 0.15f;
            float length = Vector3.Distance(a, b);
            v.Tether.SetActive(length > 0.05f);
            if (length <= 0.05f) return;
            v.Tether.transform.position = (a + b) * 0.5f;
            v.Tether.transform.rotation = Quaternion.LookRotation(b - a);
            v.Tether.transform.localScale = new Vector3(0.07f, 0.03f, length);
        }

        void UpdateEffects(float dt)
        {
            for (int i = _floats.Count - 1; i >= 0; i--)
            {
                _floats[i].Age += dt;
                if (_floats[i].Age >= _floats[i].Life) _floats.RemoveAt(i);
            }
            for (int i = _shots.Count - 1; i >= 0; i--)
            {
                Shot shot = _shots[i];
                shot.Age += dt;
                float t = Mathf.Clamp01(shot.Age / ShotTime);
                shot.Ball.transform.position = Vector3.Lerp(shot.From, shot.To, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.5f;
                if (shot.Age >= ShotTime)
                {
                    Destroy(shot.Ball);
                    _shots.RemoveAt(i);
                }
            }
            for (int i = _dust.Count - 1; i >= 0; i--)
            {
                Dust d = _dust[i];
                d.Age += dt;
                float k = Mathf.Clamp01(d.Age / DustTime);
                Transform t = d.Renderer.transform;
                t.position += d.Velocity * dt;
                d.Velocity *= Mathf.Exp(-3.5f * dt);
                t.rotation = _camera.transform.rotation;
                t.localScale = Vector3.one * d.Size * (1f + 1.6f * k);
                d.Renderer.color = new Color(DustColor.r, DustColor.g, DustColor.b, 0.8f * (1f - k));
                if (k >= 1f)
                {
                    Destroy(d.Renderer.gameObject);
                    _dust.RemoveAt(i);
                }
            }
            for (int i = _banners.Count - 1; i >= 0; i--)
            {
                _banners[i].Age += dt;
                if (_banners[i].Age >= _banners[i].Life) _banners.RemoveAt(i);
            }
        }

        void SetTrap(int side, bool armed)
        {
            if (side == 0 || side == 1) _traps[side].SetActive(armed);
        }

        void Flash(UnitView view, Color color)
        {
            view.Flash = FlashTime;
            view.FlashColor = color;
        }

        void Fire(UnitView from, UnitView to, Color color)
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Shot";
            ball.transform.SetParent(_unitsRoot);
            ball.transform.localScale = Vector3.one * 0.2f;
            Destroy(ball.GetComponent<Collider>());
            Surface(ball, color);
            Vector3 a = from.Root.transform.position + Vector3.up * from.Height * 0.6f;
            Vector3 b = to.Root.transform.position + Vector3.up * to.Height * 0.5f;
            ball.transform.position = a;
            _shots.Add(new Shot { Ball = ball, From = a, To = b });
        }

        void AddFloat(UnitView view, string text, Color color, bool big)
        {
            _floats.Add(new FloatText
            {
                Pos = view.Root.transform.position + Vector3.up * (view.Height + 0.3f),
                Text = text,
                Color = color,
                Life = big ? 1.2f : 0.9f,
                Big = big
            });
        }

        void AddBanner(string text, string sub, Color color, float life)
        {
            _banners.Add(new Banner { Text = text, Sub = sub, Color = color, Life = life });
            if (_banners.Count > 3) _banners.RemoveAt(0);
        }

        void Feed(string line)
        {
            _feed.Add(line);
            if (_feed.Count > 6) _feed.RemoveAt(0);
        }

        UnitView View(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return _views.TryGetValue(id, out UnitView view) ? view : null;
        }

        string Name(string id)
        {
            UnitView view = View(id);
            if (view == null) return "a fighter";
            if (view.Unit.Role == UnitRole.Pal) return "the Butcher";
            if (view.Unit.Role == UnitRole.Chief) return EnemyName + " chief";
            RunFighter recruit = view.Unit.Side == 0 ? _run.FindFighter(id) : null;
            return recruit != null ? recruit.DisplayName : EnemyName + " " + SliceClasses.Name(view.Unit.Class);
        }

        static Color ClassColor(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Warden: return new Color(0.45f, 0.6f, 0.82f);
                case UnitClass.Marksman: return new Color(0.45f, 0.78f, 0.4f);
                case UnitClass.Skirmisher: return new Color(0.98f, 0.56f, 0.2f);
                case UnitClass.Trapper: return new Color(0.62f, 0.42f, 0.85f);
                case UnitClass.Medic: return new Color(0.96f, 0.96f, 0.96f);
                default: return Color.grey;
            }
        }

        static string Pretty(string detail)
        {
            if (detail == Directives.Name(Directive.FallBack)) return "Fall Back";
            if (detail == Directives.Name(Directive.FocusFire)) return "Focus Fire";
            if (detail == BattleSimulation.CoverDetail) return "Cover";
            return detail;
        }

        static string Meaning(Habit habit)
        {
            switch (habit)
            {
                case Habit.CoverFocused: return "his Warden steps in front of whoever you focus";
                case Habit.HoldOnUnleash: return "he Holds the moment you Unleash";
                case Habit.PressOnFallBack: return "he Unleashes the moment you Fall Back";
                case Habit.AvoidGuarded: return "his archers shoot whoever your Warden isn't covering";
                case Habit.TrapCaution: return "his fighters wait for his Warden to spring your traps";
                default: return TribeMind.HabitName(habit);
            }
        }

        static string Seconds(long ticks)
        {
            return (ticks / (float)SimClock.TicksPerSecond).ToString("0.0", CultureInfo.InvariantCulture) + "s";
        }

        static string Percent(int nerve)
        {
            return (nerve * 100 / NerveRules.Full).ToString(CultureInfo.InvariantCulture) + "%";
        }

        void Line(string text, Color color)
        {
            _card.Add(new CardLine { Text = text, Color = color });
        }

        void Heading(string text, Color color)
        {
            _card.Add(new CardLine { Text = text, Color = color, Heading = true });
        }

        void AddLessons(IReadOnlyList<Lesson> lessons, Color color, int max)
        {
            int shown = 0;
            for (int i = 0; i < lessons.Count && shown < max; i++)
            {
                Lesson lesson = lessons[i];
                Line("- " + LessonSummary(lesson), color);
                shown++;
            }
            if (shown == 0) Line("- Nothing this time.", Dim);
        }

        // ------------------------------------------------------------ drawing

        void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Min(Screen.width / RefWidth, Screen.height / RefHeight);
            var offset = new Vector2((Screen.width - RefWidth * scale) * 0.5f, (Screen.height - RefHeight * scale) * 0.5f);
            GUI.matrix = Matrix4x4.TRS(new Vector3(offset.x, offset.y, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

            if (_vignette != null)
            {
                Color old = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.75f);
                GUI.DrawTexture(new Rect(0f, 0f, RefWidth, RefHeight), _vignette);
                GUI.color = old;
            }

            if (CurrentStage == Stage.Fight)
            {
                DrawUnitTags(scale, offset);
                DrawFloats(scale, offset);
            }
            DrawHeader();
            if (_run != null && GUI.Button(new Rect(1110f, 12f, 146f, 30f), "CODEX  " + _memory.Codex.Count, _button))
            { _showCodex = !_showCodex; _targeting = null; }
            if (_showCodex) DrawCodex();
            else if (_showGear) DrawGearChooser();
            else if (CurrentStage == Stage.Fight)
            {
                DrawCourage();
                DrawRivalPanel();
                DrawOrders();
                DrawFeed();
                DrawBanners();
            }
            else if (CurrentStage == Stage.BeforeFight) DrawReserve();
            else DrawCard();
            GUI.Label(new Rect(20f, RefHeight - 24f, 900f, 20f),
                "Prototype  /  1-7 orders  /  Space pauses  /  Esc cancels targeting", _small);
        }

        void EnsureStyles()
        {
            if (_body != null) return;
            _title = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, richText = true };
            _title.normal.textColor = Amber;
            _cardTitleStyle = new GUIStyle(_title) { fontSize = 24, wordWrap = true };
            _body = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, richText = true };
            _body.normal.textColor = Ink;
            _small = new GUIStyle(_body) { fontSize = 13 };
            _small.normal.textColor = Dim;
            _notice = new GUIStyle(_small) { fontStyle = FontStyle.Bold };
            _notice.normal.textColor = Amber;
            _tag = new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter, wordWrap = false, fontSize = 13 };
            _tag.normal.textColor = Ink;
            _banner = new GUIStyle(_body) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = false };
            _bannerSub = new GUIStyle(_body) { fontSize = 13, alignment = TextAnchor.UpperCenter };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold, richText = true };
            _barLabel = new GUIStyle(_body) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, wordWrap = false };
        }

        void DrawHeader()
        {
            Fill(new Rect(0f, 0f, RefWidth, 54f), new Color(0f, 0f, 0f, 0.55f));
            GUI.Label(new Rect(20f, 8f, 520f, 26f), "KINDRED SIEGE  /  FALLEN STAR SLICE", _title);
            GUI.Label(new Rect(20f, 32f, 620f, 20f), "Break their courage and their survivors join you.", _small);
            if (CurrentStage == Stage.Fight && _sim != null)
            {
                string info = "FIGHT " + (_fights.Count + 1) + "   " + Seconds(_sim.CurrentTick) +
                    (fightSpeed > 1.01f ? "   (" + fightSpeed.ToString("0.#", CultureInfo.InvariantCulture) + "x speed)" : string.Empty) +
                    (_paused ? "   PAUSED" : string.Empty);
                GUI.Label(new Rect(640f, 14f, 420f, 26f), info, _title);
            }
        }

        /// The two courage bars, the biggest thing on screen: that is what you are attacking.
        void DrawCourage()
        {
            if (_sim == null) return;
            DrawCourageBar(new Rect(20f, 62f, 600f, 34f), "YOUR COURAGE", _sim.Nerve(0), Amber, false);
            DrawCourageBar(new Rect(660f, 62f, 600f, 34f), EnemyName.ToUpperInvariant() + " COURAGE", _sim.Nerve(1), Rival, true);
        }

        void DrawCourageBar(Rect bar, string label, int nerve, Color color, bool fromRight)
        {
            Fill(bar, new Color(0f, 0f, 0f, 0.7f));
            float frac = Mathf.Clamp01(nerve / (float)NerveRules.Full);
            float w = (bar.width - 4f) * frac;
            var fill = new Rect(fromRight ? bar.xMax - 2f - w : bar.x + 2f, bar.y + 2f, w, bar.height - 4f);
            Fill(fill, new Color(color.r, color.g, color.b, 0.85f));
            string text = nerve <= 0 ? label + "   BROKEN" : label + "   " + Percent(nerve);
            var style = new GUIStyle(_barLabel);
            style.alignment = fromRight ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(bar.x + 12f, bar.y, bar.width - 24f, bar.height), text, style);
        }

        void DrawUnitTags(float scale, Vector2 offset)
        {
            for (int i = 0; i < _viewOrder.Count; i++)
            {
                UnitView v = _viewOrder[i];
                BattleUnit u = v.Unit;
                if (!u.Alive || v.Fleeing) continue;
                Vector2 p = ToGui(v.Root.transform.position + Vector3.up * (v.Height + 0.3f), scale, offset);
                if (p.x < -100f || p.x > RefWidth + 100f) continue;
                string cls = u.Role == UnitRole.Pal ? "BUTCHER" : u.Role == UnitRole.Chief ? "CHIEF" : SliceClasses.Name(u.Class);
                GUI.Label(new Rect(p.x - 60f, p.y - 28f, 120f, 18f), v.WalkOver ? cls + " (joining)" : cls, _tag);
                if (v.WalkOver) continue;
                var bar = new Rect(p.x - 34f, p.y - 8f, 68f, 8f);
                Fill(bar, new Color(0f, 0f, 0f, 0.7f));
                float frac = Mathf.Clamp01(u.Hp / (float)u.MaxHp);
                Fill(new Rect(bar.x + 1f, bar.y + 1f, (bar.width - 2f) * frac, bar.height - 2f), Color.Lerp(new Color(0.9f, 0.25f, 0.2f), new Color(0.35f, 0.85f, 0.4f), frac));
                if (_sim != null && _sim.IsGuarded(u))
                    GUI.Label(new Rect(p.x + 36f, p.y - 14f, 30f, 18f), "<color=#59d1e6>[G]</color>", _tag);
            }
        }

        void DrawFloats(float scale, Vector2 offset)
        {
            var style = new GUIStyle(_tag) { fontSize = 16, fontStyle = FontStyle.Bold };
            var bigStyle = new GUIStyle(style) { fontSize = 30 };
            for (int i = 0; i < _floats.Count; i++)
            {
                FloatText f = _floats[i];
                float k = f.Age / f.Life;
                Vector2 p = ToGui(f.Pos + Vector3.up * k * 0.9f, scale, offset);
                GUIStyle s = f.Big ? bigStyle : style;
                s.normal.textColor = new Color(f.Color.r, f.Color.g, f.Color.b, 1f - k);
                GUI.Label(new Rect(p.x - 60f, p.y - 16f, 120f, 32f), f.Text, s);
            }
        }

        Vector2 ToGui(Vector3 world, float scale, Vector2 offset)
        {
            Vector3 p = _camera.WorldToScreenPoint(world);
            return new Vector2((p.x - offset.x) / scale, (Screen.height - p.y - offset.y) / scale);
        }

        static string Dots(int points)
        {
            var text = new System.Text.StringBuilder();
            for (int i = 0; i < BattleSetup.DefaultDirectivePoints; i++) text.Append(i < points ? "■ " : "□ ");
            return text.ToString();
        }

        static void Fill(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }
    }
}
