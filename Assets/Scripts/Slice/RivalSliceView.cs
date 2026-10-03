using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    /// View of the 8 December slice loop on the deterministic sim: fight the raider chief with
    /// directives, see what he learned and why (each lesson is checked by replaying its fight), an
    /// off screen war, then fight him again. The sim decides every hit; this class only draws it and
    /// passes your orders in. Units are pixel sprites from Resources/RivalSlice/Units (stopgap art
    /// until PixelLab), facing the camera on a 2.5D field; a unit without art falls back to a capsule.
    ///
    /// Impact, until PixelLab gives real frames: on a kill, or a big hit when none landed lately, the
    /// fight freezes, the screen shakes, the pair is lit while everyone else dims, the target flashes
    /// white and flinches, and the attacker stretches into its lunge. The dead stagger back, fall with
    /// a bounce, kick up dust and fade low. Torchlight, warm dim light and a vignette set the mood.
    /// None of it touches the sim. Timings are starting values, judged by eye in the recorded clip.
    public sealed class RivalSliceView : MonoBehaviour
    {
        public enum Stage { Intro, Fight, AfterFight, War, BeforeFight, Final }

        /// Command line switch for the recorder: play on autopilot and save every frame to the folder that follows.
        public const string RecordArg = "-ksRivalRecord";

        const float RefWidth = 1280f;
        const float RefHeight = 720f;
        const int FightsToPlay = 2;
        const float CardSeconds = 8f;
        const float ShortCardSeconds = 5f;
        const float EndHoldSeconds = 2f;
        const float FlashTime = 0.14f;
        const float LungeTime = 0.18f;
        const float ShotTime = 0.16f;
        const float PixelsPerUnit = 30f;
        const float IdleFrameSeconds = 0.28f;

        const float HitFreeze = 0.15f;
        const float KillFreeze = 0.33f;
        const float HitShake = 0.14f;
        const float KillShake = 0.26f;
        const float ImpactCooldown = 0.7f;
        const float ImpactFlash = 0.3f;
        const float FocusTime = 0.7f;
        const float FocusFade = 0.25f;
        const float PoseTime = 0.36f;
        const float KnockTime = 0.3f;
        const float DeathTime = 0.5f;
        const float DustTime = 0.6f;
        const int BigHit = 18;

        // Closer than the first pass, on a tighter formation, so a unit is about twice the size on screen.
        // Aimed right of the formation so the panels on the right cover empty ground, not fighters.
        static readonly Vector3 CameraLook = new Vector3(1.45f, 0.7f, 1.2f);
        static readonly Vector3 CameraHome = CameraLook + new Vector3(0f, 5.05f, -8.63f);

        [Tooltip("Plays a scripted plan instead of waiting for your orders: focus their Marksman, Unleash, Fall Back twice.")]
        public bool autopilot;

        [Tooltip("Fight playback speed. 1 is real time.")]
        public float fightSpeed = 1f;

        [Tooltip("World seed for manual play. Autopilot picks the first world where the rival visibly learns.")]
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

        SliceWorld _world;
        BattleSetup _setup;
        BattleSimulation _sim;
        SimSession _session;
        WarResult _lastWar;
        readonly List<PlayerFightResult> _fights = new List<PlayerFightResult>();
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

        GUIStyle _title, _body, _small, _tag, _banner, _bannerSub, _button, _cardTitleStyle, _notice;

        sealed class UnitView
        {
            public BattleUnit Unit;
            public GameObject Root;
            public Transform Body;
            public Renderer BodyRenderer;
            public SpriteRenderer Art;
            public SpriteRenderer Silhouette;
            public Sprite[] Idle;
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
            int seed = autopilot ? FindStorySeed() : worldSeed;
            _world = SliceWorld.Create((ulong)seed);
            ShowIntro(seed);
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
                case Stage.Intro:
                    StartFight();
                    break;
                case Stage.AfterFight:
                    if (_fights.Count >= FightsToPlay || _world.Raiders.Broken) ShowFinal();
                    else if (!_world.WarOver) ShowWar();
                    else ShowBeforeFight();
                    break;
                case Stage.War:
                    if (_world.Raiders.Broken) ShowFinal();
                    else ShowBeforeFight();
                    break;
                case Stage.BeforeFight:
                    StartFight();
                    break;
                case Stage.Final:
                    Finished = true;
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
            _cardTitle = "FALLEN STAR: the raider chief has come for the Clarth";
            _card.Clear();
            Line("Your fighters fight on their own. You give orders: Hold, Fall Back, Focus Fire, Unleash, Sacrifice. 5 points a fight.", Ink);
            Line("Whatever beats the chief, he learns from. Every lesson cites the fight it came from, and replaying that fight must give the same numbers.", Ink);
            Line(string.Empty, Ink);
            if (autopilot)
            {
                Line("Autopilot plays the same plan in both fights: focus their Marksman, Unleash, then Fall Back twice.", Amber);
                Line("World " + seed + " is the first world where this plan wins fight 1, the chief learns from it, and then beats the same plan in fight 2. " +
                    "It was picked to show the loop, not drawn at random. Fights play at " + fightSpeed.ToString("0.#", CultureInfo.InvariantCulture) + "x speed.", Dim);
            }
            else
            {
                Line("Keys: 1 Hold, 2 Fall Back, 3 Focus Fire then click an enemy, 4 Unleash, 5 Sacrifice then click your unit. Space pauses.", Amber);
            }
        }

        void StartFight()
        {
            ClearFight();
            _setup = _world.NextPlayerFight();
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
            SpawnUnits();
            SetStage(Stage.Fight);
            Feed("Fight " + (_fights.Count + 1) + " begins.");
        }

        void UpdateFight(float dt)
        {
            if (_paused || _sim == null) return;
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
                    if (autopilot) PlanOrders(_session, _sim);
                    _session.Tick();
                    CountDownFx();
                    DrainEvents();
                }
                if (_sim.IsFinished)
                {
                    bool won = _sim.Winner == SliceWorld.PlayerSide;
                    AddBanner(won ? "VICTORY" : _sim.Winner < 0 ? "DRAW" : "DEFEAT", string.Empty, won ? Amber : Rival, EndHoldSeconds + 1f);
                }
                return;
            }

            _endHold += dt;
            if (_endHold > EndHoldSeconds) ShowAfterFight();
        }

        void ShowAfterFight()
        {
            PlayerFightResult fight = _world.FinishPlayerFight(_setup, _session);
            _fights.Add(fight);
            SetStage(Stage.AfterFight);
            bool won = fight.Record.Winner == SliceWorld.PlayerSide;
            _cardTitle = "Fight " + _fights.Count + ": " + (won ? "you won" : fight.Record.Winner < 0 ? "a draw" : "the raider chief won") +
                " after " + Seconds(fight.Record.EndTick);
            _card.Clear();
            Line("Raiders lost " + fight.RaiderDead + (fight.RaidersBroke
                ? " and broke."
                : ". After regrouping they have " + _world.Raiders.Strength + " of " + _world.Raiders.HomeStrength + "."), Ink);
            Heading("WHAT THE RAIDER CHIEF LEARNED", Rival);
            AddLessons(fight.RaiderLessons, Ink, 6);
            Heading("WHAT YOUR CLARTH LEARNED", Teal);
            AddLessons(fight.ClarthLessons, Ink, 2);
        }

        void ShowWar()
        {
            _lastWar = _world.RunRaiderWar();
            SetStage(Stage.War);
            _cardTitle = "Off screen: the raiders fought the third tribe";
            _card.Clear();
            string result = _lastWar.WinnerId.Length == 0 ? "A draw." :
                (_lastWar.WinnerId == SliceWorld.RaidersId ? "The raiders won." : "The third tribe won.");
            Line(result + " Raiders lost " + _lastWar.DeadA + ", the third tribe lost " + _lastWar.DeadB + ".", Ink);
            Line("After regrouping: raiders " + _world.Raiders.Strength + " of " + _world.Raiders.HomeStrength + ", third tribe " +
                _world.ThirdTribe.Strength + " of " + _world.ThirdTribe.HomeStrength + "." +
                (_lastWar.LoserBroke ? " The " + (_lastWar.LoserId == SliceWorld.RaidersId ? "raiders" : "third tribe") + " broke." : string.Empty), Ink);
            List<Tribe> recruitable = _world.RecruitableTribes();
            if (recruitable.Count > 0) Line("Now open to your recruiters: " + recruitable[0].DisplayName + ", " + recruitable[0].Strength + " fighters.", Amber);
            Heading("WHAT THE RAIDER CHIEF TOOK FROM THE WAR", Rival);
            AddLessons(_lastWar.LessonsA, Ink, 3);
        }

        void ShowBeforeFight()
        {
            SetStage(Stage.BeforeFight);
            _cardTitle = "Fight " + (_fights.Count + 1) + ": the raider chief is back";
            _card.Clear();
            Heading("HE NOW ANSWERS YOU", Rival);
            int shown = 0;
            for (int i = 0; i < TribeMind.HabitCount; i++)
            {
                var habit = (Habit)i;
                if (!_world.Raiders.Mind.HasHabit(habit)) continue;
                Line("- " + Meaning(habit), Ink);
                shown++;
            }
            if (shown == 0) Line("Nothing new. He has not learned anything he can use against you yet.", Ink);
            if (autopilot) Line("Autopilot keeps the same plan as fight 1.", Amber);
        }

        void ShowFinal()
        {
            SetStage(Stage.Final);
            _cardTitle = "Same plan, different rival";
            _card.Clear();
            var summary = new System.Text.StringBuilder();
            for (int i = 0; i < _fights.Count; i++)
            {
                FightRecord r = _fights[i].Record;
                string outcome = r.Winner == SliceWorld.PlayerSide ? "you won" : r.Winner < 0 ? "a draw" : "the raider chief won";
                string line = "Fight " + (i + 1) + ": " + outcome + " after " + Seconds(r.EndTick) + ".";
                string answers = AutoAnswers(r);
                if (answers.Length > 0) line += " He answered your orders with " + answers + ".";
                Line(line, Ink);
                summary.AppendLine(line);
            }
            Line(string.Empty, Ink);
            Line("Every lesson shown was replayed from its fight and gave the same numbers.", Teal);
            Line("Stopgap art until PixelLab. Provisional numbers. Next: your orders by hand.", Dim);
            Summary = summary.ToString();
        }

        // ------------------------------------------------------------ orders

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Space) && CurrentStage == Stage.Fight) _paused = !_paused;
            if (CurrentStage != Stage.Fight || autopilot || _sim == null || _sim.IsFinished) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) PickOrder(Directive.Hold);
            if (Input.GetKeyDown(KeyCode.Alpha2)) PickOrder(Directive.FallBack);
            if (Input.GetKeyDown(KeyCode.Alpha3)) PickOrder(Directive.FocusFire);
            if (Input.GetKeyDown(KeyCode.Alpha4)) PickOrder(Directive.Unleash);
            if (Input.GetKeyDown(KeyCode.Alpha5)) PickOrder(Directive.Sacrifice);
            if (Input.GetKeyDown(KeyCode.Escape)) _targeting = null;
            if (_targeting.HasValue && Input.GetMouseButtonDown(0) && _camera != null)
            {
                if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f) && hit.collider.transform.parent != null)
                {
                    IssueOrder(_targeting.Value, hit.collider.transform.parent.name);
                    _targeting = null;
                }
            }
        }

        void PickOrder(Directive directive)
        {
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
            DrainEvents();
        }

        /// The autopilot plan: focus their Marksman, Unleash, then Fall Back twice.
        static void PlanOrders(SimSession session, BattleSimulation sim)
        {
            long t = sim.CurrentTick;
            if (t == 20)
            {
                BattleUnit marksman = FirstAlive(sim, 1, UnitClass.Marksman);
                if (marksman != null) session.TryIssue(Directives.CommandKind, "0", marksman.Id, 0, 0, Directives.Name(Directive.FocusFire), out _);
            }
            if (t == 40) session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, Directives.Name(Directive.Unleash), out _);
            if (t == 130 || t == 250) session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, Directives.Name(Directive.FallBack), out _);
        }

        static BattleUnit FirstAlive(BattleSimulation sim, int side, UnitClass unitClass)
        {
            for (int i = 0; i < sim.Units.Count; i++)
            {
                BattleUnit u = sim.Units[i];
                if (u.Side == side && u.Alive && u.Class == unitClass) return u;
            }
            return null;
        }

        /// First world where the autopilot plan wins fight 1, the chief learns answers to Unleash and
        /// Fall Back, and then uses both to win fight 2. Played headless; the shown run is the same world.
        static int FindStorySeed()
        {
            for (int seed = 1; seed <= 500; seed++)
            {
                SliceWorld world = SliceWorld.Create((ulong)seed);
                BattleSetup first = world.NextPlayerFight();
                PlayerFightResult r1 = world.FinishPlayerFight(first, PlayHeadless(first));
                if (r1.Record.Winner != SliceWorld.PlayerSide || world.Raiders.Broken || world.WarOver) continue;
                if (!world.Raiders.Mind.HasHabit(Habit.HoldOnUnleash) || !world.Raiders.Mind.HasHabit(Habit.PressOnFallBack)) continue;
                world.RunRaiderWar();
                if (world.Raiders.Broken) continue;
                BattleSetup second = world.NextPlayerFight();
                PlayerFightResult r2 = world.FinishPlayerFight(second, PlayHeadless(second));
                if (r2.Record.Winner == 1 && CountAuto(r2.Record, "Hold") > 0 && CountAuto(r2.Record, "Unleash") > 0) return seed;
            }
            return 7;
        }

        static SimSession PlayHeadless(BattleSetup setup)
        {
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                PlanOrders(session, sim);
                session.Tick();
            }
            return session;
        }

        static int CountAuto(FightRecord record, string detail)
        {
            int count = 0;
            for (int i = 0; i < record.Events.Count; i++)
            {
                BattleEvent e = record.Events[i];
                if (e.Kind == BattleEventKind.Directive && e.ActorSide == 1 &&
                    e.ActorId.StartsWith(BattleSimulation.AutoActorPrefix, System.StringComparison.Ordinal) && e.Detail == detail)
                    count++;
            }
            return count;
        }

        static string AutoAnswers(FightRecord record)
        {
            var parts = new List<string>();
            string[] details = { "Hold", "Unleash", BattleSimulation.CoverDetail };
            for (int i = 0; i < details.Length; i++)
            {
                int n = CountAuto(record, details[i]);
                if (n > 0) parts.Add(Pretty(details[i]) + (n > 1 ? " x" + n : string.Empty));
            }
            return string.Join(", ", parts);
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
            if (!auto && side == SliceWorld.PlayerSide)
            {
                _lastOrder = detail;
                _lastOrderAge = 0f;
                AddBanner("YOUR ORDER: " + Pretty(detail).ToUpperInvariant(), target.Trim(), Amber, 1.8f);
                Feed("You order " + Pretty(detail) + target + ".");
                return;
            }
            if (!auto) return;

            TribeMind mind = side == SliceWorld.PlayerSide ? _world.Clarth.Mind : _world.Raiders.Mind;
            Lesson taught = LessonFor(mind, detail);
            string sub = taught == null ? string.Empty : "Learned in " + taught.FightId + ": " + taught.Why;
            if (side == SliceWorld.PlayerSide)
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
            _camera.fieldOfView = 38f;
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

        GameObject Block(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(_stageRoot);
            go.transform.position = position;
            go.transform.localScale = scale;
            Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().material.color = color;
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
                        float x = sign * (row == (int)Row.Front ? 1.2f : 3.0f);
                        float z = 1.2f + (i - (members.Count - 1) * 0.5f) * 1.5f;
                        UnitView view = MakeView(members[i], new Vector3(x, 0f, z));
                        _views[members[i].Id] = view;
                        _viewOrder.Add(view);
                    }
                }
            }
        }

        /// Art file names: clarth_warden, raider_medic, and so on; Wardens may add _idle1 to _idle4.
        static string ArtName(BattleUnit unit)
        {
            return (unit.Side == 0 ? "clarth_" : "raider_") + SliceClasses.Name(unit.Class).ToLowerInvariant();
        }

        Sprite LoadSprite(string name)
        {
            if (_sprites.TryGetValue(name, out Sprite cached)) return cached;
            var texture = Resources.Load<Texture2D>("RivalSlice/Units/" + name);
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

        UnitView MakeView(BattleUnit unit, Vector3 home)
        {
            var root = new GameObject(unit.Id);
            root.transform.SetParent(_unitsRoot);
            root.transform.position = home;
            var view = new UnitView { Unit = unit, Root = root, Home = home, Pos = home, Phase = unit.Index * 0.37f };

            Part(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.012f, 0f), new Vector3(1.05f, 0.004f, 0.55f), new Color(0.08f, 0.07f, 0.06f));

            Sprite still = LoadSprite(ArtName(unit));
            if (still != null)
            {
                var art = new GameObject("Art");
                art.transform.SetParent(root.transform, false);
                SpriteRenderer renderer = art.AddComponent<SpriteRenderer>();
                renderer.sprite = still;
                renderer.flipX = unit.Side == 0; // the art faces left; the Clarth stand on the left facing right
                BoxCollider clickArea = art.AddComponent<BoxCollider>();
                clickArea.center = new Vector3(0f, 1f, 0f);
                clickArea.size = new Vector3(1.3f, 2f, 0.2f);
                view.Art = renderer;
                view.Body = art.transform;
                view.BaseColor = Color.white;
                view.Height = 1.95f;
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
                if (unit.Class == UnitClass.Warden)
                {
                    var frames = new List<Sprite>();
                    for (int i = 1; i <= 4; i++)
                    {
                        Sprite frame = LoadSprite(ArtName(unit) + "_idle" + i.ToString(CultureInfo.InvariantCulture));
                        if (frame != null) frames.Add(frame);
                    }
                    if (frames.Count > 1) view.Idle = frames.ToArray();
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
                bodyRenderer.material.color = baseColor;
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
            go.GetComponent<Renderer>().material.color = color;
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
                v.Pos = Vector3.Lerp(v.Pos, goal, 1f - Mathf.Exp(-dt * 7f));

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
                    // Sprites face the camera; still art bobs a little, the Warden plays its idle.
                    float bob = u.Alive && v.Idle == null ? Mathf.Sin(_animClock * 3.2f + v.Phase) * 0.04f : 0f;
                    v.Body.localPosition = new Vector3(0f, bob, 0f);
                    v.Body.localScale = new Vector3(1f + 0.22f * pose, (1f - 0.1f * pose) * (1f - 0.12f * Mathf.Clamp01(fall)), 1f);
                    Quaternion face = _camera.transform.rotation;
                    float side = u.Side == 0 ? 1f : -1f;
                    float tilt = u.Alive ? side * 12f * knock : side * 86f * fall;
                    v.Body.rotation = face * Quaternion.Euler(0f, 0f, tilt);
                    if (u.Alive && v.Idle != null)
                        v.Art.sprite = v.Idle[(int)((_animClock + v.Phase) / IdleFrameSeconds) % v.Idle.Length];
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
                bool unleashed = u.Alive && fx.Unleash > 0;
                bool holding = u.Alive && fx.Hold > 0 && u.Row == Row.Front;
                v.Disc.SetActive(unleashed || holding);
                if (unleashed) v.DiscRenderer.material.color = new Color(0.9f, 0.25f, 0.2f);
                else if (holding) v.DiscRenderer.material.color = new Color(0.3f, 0.55f, 0.95f);

                UpdateTether(v);
            }

            for (int s = 0; s < 2; s++)
            {
                SideFx fx = _fx[s];
                UnitView focused = fx.Focus > 0 ? View(fx.FocusId) : null;
                bool show = focused != null && focused.Unit.Alive;
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
            UnitView ward = u.Class == UnitClass.Warden && u.Alive && u.GuardTargetId.Length > 0 ? View(u.GuardTargetId) : null;
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
                v.Tether.GetComponent<Renderer>().material.color = Teal;
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
            ball.GetComponent<Renderer>().material.color = color;
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
            string cls = SliceClasses.Name(view.Unit.Class);
            return view.Unit.Side == SliceWorld.PlayerSide ? "your " + cls : "the raider " + cls;
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
                bool checkedOk = _world.Archive.TryVerifyLesson(lesson, out _);
                Line("- " + lesson.Why + (checkedOk ? "  [replay checked]" : "  [REPLAY MISMATCH]"), checkedOk ? color : Rival);
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
            DrawRivalPanel();
            if (CurrentStage == Stage.Fight)
            {
                DrawOrders();
                DrawFeed();
                DrawBanners();
            }
            else DrawCard();
            GUI.Label(new Rect(20f, RefHeight - 24f, 900f, 20f),
                "Stopgap art until PixelLab. Provisional numbers. The deterministic sim decides every hit; this scene only draws it.", _small);
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
        }

        void DrawHeader()
        {
            Fill(new Rect(0f, 0f, RefWidth, 54f), new Color(0f, 0f, 0f, 0.55f));
            GUI.Label(new Rect(20f, 8f, 520f, 26f), "KINDRED SIEGE  /  FALLEN STAR SLICE", _title);
            GUI.Label(new Rect(20f, 32f, 620f, 20f), "The Clarth against a raider chief who learns from every fight.", _small);
            if (CurrentStage == Stage.Fight && _sim != null)
            {
                string info = "FIGHT " + (_fights.Count + 1) + " OF " + FightsToPlay + "   " + Seconds(_sim.CurrentTick) +
                    (fightSpeed > 1.01f ? "   (" + fightSpeed.ToString("0.#", CultureInfo.InvariantCulture) + "x speed)" : string.Empty) +
                    (_paused ? "   PAUSED" : string.Empty);
                GUI.Label(new Rect(560f, 14f, 420f, 26f), info, _title);
            }
        }

        void DrawRivalPanel()
        {
            var box = new Rect(RefWidth - 320f, 66f, 304f, 236f);
            Fill(box, new Color(0f, 0f, 0f, 0.55f));
            var style = new GUIStyle(_title) { fontSize = 15 };
            style.normal.textColor = Rival;
            GUI.Label(new Rect(box.x + 12f, box.y + 8f, box.width - 24f, 22f), "WHAT THE RAIDER CHIEF ANSWERS", style);
            float y = box.y + 36f;
            TribeMind mind = _world == null ? null : _world.Raiders.Mind;
            int shown = 0;
            if (mind != null)
            {
                for (int i = 0; i < TribeMind.HabitCount; i++)
                {
                    var habit = (Habit)i;
                    if (!mind.HasHabit(habit)) continue;
                    string text = "- " + Meaning(habit);
                    float h = _small.CalcHeight(new GUIContent(text), box.width - 24f);
                    GUI.Label(new Rect(box.x + 12f, y, box.width - 24f, h), "<color=#e8e8e0>" + text + "</color>", _small);
                    y += h + 4f;
                    shown++;
                }
            }
            if (shown == 0) GUI.Label(new Rect(box.x + 12f, y, box.width - 24f, 40f), "Nothing yet. He learns from every fight.", _small);
            if (mind != null)
                GUI.Label(new Rect(box.x + 12f, box.yMax - 26f, box.width - 24f, 20f), "Fights he has learned from: " + mind.FightsLearned.Count, _small);
        }

        void DrawOrders()
        {
            var bar = new Rect(20f, RefHeight - 96f, 900f, 66f);
            Fill(bar, new Color(0f, 0f, 0f, 0.55f));
            string points = "YOUR ORDERS  " + Dots(_sim.DirectivePoints(0)) + "      CHIEF  " + Dots(_sim.DirectivePoints(1));
            GUI.Label(new Rect(bar.x + 12f, bar.y + 4f, 600f, 20f), points, _small);
            Directive[] all = { Directive.Hold, Directive.FallBack, Directive.FocusFire, Directive.Unleash, Directive.Sacrifice };
            for (int i = 0; i < all.Length; i++)
            {
                Directive d = all[i];
                string label = (i + 1) + "  " + Pretty(Directives.Name(d)).ToUpperInvariant() + "  (" + Directives.Cost(d) + ")";
                if (_lastOrderAge < 1.2f && _lastOrder == Directives.Name(d)) label = "<color=#ffb04c>" + label + "</color>";
                GUI.enabled = !autopilot && !_sim.IsFinished;
                if (GUI.Button(new Rect(bar.x + 12f + i * 176f, bar.y + 26f, 168f, 32f), label, _button)) PickOrder(d);
                GUI.enabled = true;
            }
            string mode = autopilot ? "AUTOPILOT: focus their Marksman at 0.7s, Unleash at 1.3s, Fall Back at 4.3s and 8.3s." :
                _targeting.HasValue ? "Click a unit for " + Pretty(Directives.Name(_targeting.Value)) + ". Esc cancels." :
                "Your fighters act on their own. Orders cost points.";
            GUI.Label(new Rect(bar.x, bar.y - 22f, 900f, 20f), mode, autopilot || _targeting.HasValue ? _notice : _small);
        }

        void DrawFeed()
        {
            var box = new Rect(RefWidth - 320f, 314f, 304f, 150f);
            Fill(box, new Color(0f, 0f, 0f, 0.45f));
            float y = box.y + 8f;
            for (int i = 0; i < _feed.Count; i++)
            {
                GUI.Label(new Rect(box.x + 12f, y, box.width - 24f, 22f), _feed[i], _small);
                y += 23f;
            }
        }

        void DrawBanners()
        {
            float y = 70f;
            for (int i = _banners.Count - 1; i >= 0; i--)
            {
                Banner b = _banners[i];
                float alpha = Mathf.Clamp01((b.Life - b.Age) / 0.4f);
                var main = new GUIStyle(_banner);
                main.normal.textColor = new Color(b.Color.r, b.Color.g, b.Color.b, alpha);
                float width = 860f;
                float subHeight = b.Sub.Length > 0 ? _bannerSub.CalcHeight(new GUIContent(b.Sub), width - 40f) : 0f;
                var area = new Rect(20f + (900f - width) * 0.5f, y, width, 40f + subHeight + 8f);
                Fill(area, new Color(0f, 0f, 0f, 0.6f * alpha));
                GUI.Label(new Rect(area.x, area.y + 4f, area.width, 34f), b.Text, main);
                if (subHeight > 0f)
                {
                    var sub = new GUIStyle(_bannerSub);
                    sub.normal.textColor = new Color(Ink.r, Ink.g, Ink.b, alpha);
                    GUI.Label(new Rect(area.x + 20f, area.y + 38f, area.width - 40f, subHeight), b.Sub, sub);
                }
                y += area.height + 6f;
            }
        }

        void DrawUnitTags(float scale, Vector2 offset)
        {
            for (int i = 0; i < _viewOrder.Count; i++)
            {
                UnitView v = _viewOrder[i];
                BattleUnit u = v.Unit;
                Vector2 p = ToGui(v.Root.transform.position + Vector3.up * (v.Height + 0.3f), scale, offset);
                if (p.x < -100f || p.x > RefWidth + 100f) continue;
                string cls = SliceClasses.Name(u.Class);
                if (!u.Alive) continue;
                GUI.Label(new Rect(p.x - 60f, p.y - 28f, 120f, 18f), cls, _tag);
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

        void DrawCard()
        {
            var box = new Rect(150f, 80f, 790f, 560f);
            Fill(box, new Color(0.04f, 0.045f, 0.05f, 0.92f));
            float width = box.width - 48f;
            float y = box.y + 22f;
            float titleHeight = _cardTitleStyle.CalcHeight(new GUIContent(_cardTitle), width);
            GUI.Label(new Rect(box.x + 24f, y, width, titleHeight), _cardTitle, _cardTitleStyle);
            y += titleHeight + 14f;
            for (int i = 0; i < _card.Count; i++)
            {
                CardLine line = _card[i];
                var style = new GUIStyle(_body);
                if (line.Heading)
                {
                    style.fontStyle = FontStyle.Bold;
                    style.fontSize = 14;
                    y += 8f;
                }
                style.normal.textColor = line.Color;
                string text = line.Text.Length == 0 ? " " : line.Text;
                float h = style.CalcHeight(new GUIContent(text), width);
                if (y + h > box.yMax - 50f) break;
                GUI.Label(new Rect(box.x + 24f, y, width, h), text, style);
                y += h + 6f;
            }
            if (autopilot)
            {
                float hold = CurrentStage == Stage.AfterFight ? CardSeconds + 2f : CurrentStage == Stage.BeforeFight ? ShortCardSeconds + 1f : CardSeconds;
                Fill(new Rect(box.x, box.yMax - 4f, box.width * Mathf.Clamp01(_stageClock / hold), 4f), Amber);
            }
            else if (!Finished && GUI.Button(new Rect(box.xMax - 200f, box.yMax - 48f, 176f, 34f), CurrentStage == Stage.Final ? "FINISH" : "CONTINUE", _button))
            {
                Continue();
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
