using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    /// One encounter over Claude's Campaign API. No saves, balancing or campaign progression UI.
    public sealed class ClarthBattleStudy : MonoBehaviour
    {
        const float W = 1440, H = 900;
        static readonly Color Ink = Hex("201C23"), Bone = Hex("EFDFBE"), Muted = Hex("AD9A86"),
            Bronze = Hex("A87848"), Red = Hex("DA6B51"), Mint = Hex("99BEAC");
        readonly Dictionary<string, Clip> _clips = new Dictionary<string, Clip>();
        readonly List<Figure> _figures = new List<Figure>();
        readonly List<Number> _numbers = new List<Number>();
        readonly List<Button> _orderButtons = new List<Button>();
        readonly Directive[] _orders = { Directive.Hold, Directive.FallBack, Directive.FocusFire, Directive.Unleash, Directive.Sacrifice };
        Campaign _campaign;
        CampaignFight _fight;
        EncounterResult _result;
        CampaignPilot _pilot;
        Canvas _canvas;
        RectTransform _root, _stage, _effects, _intro, _ending;
        Text _time, _points, _notice, _target, _leftCount, _rightCount, _mode, _endTitle, _endText;
        Image _leftNerve, _rightNerve;
        Button _pauseButton, _speedButton, _startButton;
        Font _font;
        Sprite _shadow;
        RenderTexture _captureTarget;
        Texture2D _capturePixels;
        bool _started, _paused, _demo;
        float _speed = 1, _accum, _visualTime, _endAge;
        int _scanned, _acceptedOrders;
        string _selected = "", _capture;

        [Serializable] sealed class FrameRect { public int x, y, w, h; }
        [Serializable] sealed class Frame { public FrameRect frame; public int duration; }
        [Serializable] sealed class Sheet { public Frame[] frames; public float anchorX, anchorY; }
        sealed class Clip
        {
            public Texture2D texture;
            public Sheet sheet;
            public float seconds;
            public int FrameAt(float time, bool loop)
            {
                float ms = (loop ? Mathf.Repeat(time, seconds) : Mathf.Clamp(time, 0, seconds)) * 1000;
                for (int i = 0; i < sheet.frames.Length; i++)
                {
                    if (ms < sheet.frames[i].duration) return i;
                    ms -= sheet.frames[i].duration;
                }
                return sheet.frames.Length - 1;
            }
        }
        sealed class Figure
        {
            public BattleUnit unit;
            public RectTransform root;
            public RawImage body;
            public Image health, selection, guard;
            public Text label;
            public Clip idle, attack, hit, death;
            public Vector2 feet;
            public float scale, attackAt = -100, hitAt = -100, deathAt = -1, attackLength;
            public int numberLane;
            public bool anticipated;
        }
        sealed class Number { public Text text; public Vector2 origin; public float at; }

        void Start()
        {
            Application.runInBackground = true;
            _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _capture = Argument("-ksClarthCapture");
            if (_capture != null)
            {
                Directory.CreateDirectory(_capture);
                Time.captureDeltaTime = 1f / 30f;
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 30;
            }
            BuildScreen();
            ResetFight();
            if (_capture != null) StartCoroutine(CaptureRun());
        }

        void BuildScreen()
        {
            Camera.main.backgroundColor = Ink;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            _canvas = new GameObject("Clarth study canvas", typeof(Canvas), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (_capture != null)
            {
                // A hidden Windows player has no reliable back buffer. Render the same canvas
                // through its camera into an explicit target instead of capturing black frames.
                _captureTarget = new RenderTexture((int)W, (int)H, 24);
                _captureTarget.Create();
                _capturePixels = new Texture2D((int)W, (int)H, TextureFormat.RGB24, false);
                Camera.main.targetTexture = _captureTarget;
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = Camera.main;
                _canvas.planeDistance = 1;
            }
            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("Clarth study input", typeof(EventSystem), typeof(StandaloneInputModule));
            _root = new GameObject("1440 x 900 stage", typeof(RectTransform)).GetComponent<RectTransform>();
            _root.SetParent(_canvas.transform, false);
            _root.anchorMin = _root.anchorMax = _root.pivot = new Vector2(.5f, .5f);
            _root.sizeDelta = new Vector2(W, H);
            Panel(_root, 0, 0, W, H, Ink);
            _stage = Rect(_root, "Painted stage", 0, 0, W, H);
            var bg = Raw(_stage, "Existing PixelLab canyon", 32, 104, 1376, 768);
            bg.texture = Resources.Load<Texture2D>("ClarthStudy/canyon");
            _effects = Rect(_root, "Combat feedback", 0, 0, W, H);

            Panel(_root, 0, 0, W, 105, Ink);
            Label(_root, "K I N D R E D   S I E G E", 38, 18, 500, 22, 13, Muted);
            Label(_root, "Fallen Star", 36, 41, 560, 48, 35, Bone, FontStyle.Bold);
            Label(_root, "FIRST ALLIANCE", 1040, 25, 360, 22, 13, Bronze, FontStyle.Bold, TextAnchor.MiddleRight);
            _mode = Label(_root, "CLARTH  /  OPENING ENCOUNTER", 910, 50, 490, 30, 17, Bone, FontStyle.Normal, TextAnchor.MiddleRight);
            Panel(_root, 32, 103, 1376, 2, Bronze);

            Panel(_root, 48, 121, 386, 69, Alpha(Ink, .92f));
            _leftCount = Label(_root, "CLARTH", 64, 130, 350, 23, 16, Bone, FontStyle.Bold);
            Panel(_root, 64, 164, 354, 6, Hex("4B3A37"));
            _leftNerve = Panel(_root, 64, 164, 354, 6, Mint);
            Panel(_root, 1006, 121, 386, 69, Alpha(Ink, .92f));
            _rightCount = Label(_root, "BLOODRIDGE", 1022, 130, 354, 23, 16, Bone, FontStyle.Bold, TextAnchor.MiddleRight);
            Panel(_root, 1022, 164, 354, 6, Hex("4B3A37"));
            _rightNerve = Panel(_root, 1022, 164, 354, 6, Red);
            Panel(_root, 632, 120, 176, 47, Alpha(Ink, .9f));
            _time = Label(_root, "00:00", 632, 127, 176, 29, 22, Bone, FontStyle.Bold, TextAnchor.MiddleCenter);

            Panel(_root, 0, 679, W, 221, Ink);
            Panel(_root, 32, 679, 1376, 2, Bronze);
            _notice = Label(_root, "Bloodridge came to take you. Command the Clarth and drive them off.", 40, 690, 1120, 28, 17, Bone);
            _points = Label(_root, "ORDERS  0", 1170, 690, 228, 28, 17, Bronze, FontStyle.Bold, TextAnchor.MiddleRight);
            string[] titles = { "HOLD", "FALL BACK", "FOCUS FIRE", "UNLEASH", "SACRIFICE" };
            string[] hints = { "Brace for impact", "Open the distance", "Select an enemy", "Break their nerve", "Select one of yours" };
            for (int i = 0; i < _orders.Length; i++)
            {
                int n = i;
                var button = ActionButton(_root, 40 + i * 274, 730, 264, 76, (i + 1) + "   " + titles[i], () => Order(_orders[n]));
                Label(button.transform, hints[i], 14, 44, 238, 20, 13, Muted);
                Label(button.transform, Directives.Cost(_orders[i]) + "", 211, 12, 36, 25, 18, Bronze, FontStyle.Bold, TextAnchor.MiddleRight);
                _orderButtons.Add(button);
            }
            _target = Label(_root, "Select a fighter to inspect them.", 42, 825, 815, 24, 16, Bone);
            Label(_root, "Fighters act automatically.  1–5 orders  ·  SPACE pause  ·  R restart", 42, 855, 930, 24, 13, Muted);
            _pauseButton = ActionButton(_root, 1005, 831, 125, 42, "PAUSE", TogglePause, 14);
            _speedButton = ActionButton(_root, 1140, 831, 110, 42, "1×", ToggleSpeed, 15);
            ActionButton(_root, 1260, 831, 140, 42, "RESTART", ResetFight, 14);

            _intro = Rect(_root, "Opening", 76, 219, 525, 415);
            Panel(_intro, 0, 0, 525, 415, Alpha(Ink, .96f));
            Panel(_intro, 0, 0, 4, 415, Bronze);
            Label(_intro, "THE RAID ON YOUR JAILERS", 28, 24, 470, 22, 13, Bronze, FontStyle.Bold);
            Label(_intro, "Your jailers.\nYour first allies.", 26, 60, 480, 118, 39, Bone, FontStyle.Bold);
            Label(_intro, "Bloodridge came to take you.\nCommand the Clarth and drive them off.", 28, 189, 466, 68, 21, Bone);
            _startButton = ActionButton(_intro, 28, 282, 468, 55, "COMMAND THE CLARTH", () => Begin(false), 19);
            ActionButton(_intro, 28, 351, 468, 39, "WATCH A DEMO", () => Begin(true), 13);

            _ending = Rect(_root, "Encounter outcome", 398, 246, 644, 383);
            Panel(_ending, 0, 0, 644, 383, Alpha(Ink, .97f));
            Panel(_ending, 0, 0, 644, 3, Bronze);
            Label(_ending, "FIRST ALLIANCE", 30, 27, 580, 24, 14, Bronze, FontStyle.Bold);
            _endTitle = Label(_ending, "", 30, 69, 584, 88, 34, Bone, FontStyle.Bold);
            _endText = Label(_ending, "", 30, 174, 580, 96, 19, Bone);
            ActionButton(_ending, 30, 303, 580, 53, "PLAY AGAIN", ResetFight, 18);

            var shadowTex = new Texture2D(64, 24, TextureFormat.RGBA32, false);
            shadowTex.filterMode = FilterMode.Point;
            for (int y = 0; y < 24; y++) for (int x = 0; x < 64; x++)
                shadowTex.SetPixel(x, y, new Color(0, 0, 0, (Mathf.Pow((x - 32) / 32f, 2) + Mathf.Pow((y - 12) / 12f, 2) < 1) ? .3f : 0));
            shadowTex.Apply();
            _shadow = Sprite.Create(shadowTex, new UnityEngine.Rect(0, 0, 64, 24), new Vector2(.5f, .5f));
        }

        void ResetFight()
        {
            foreach (Figure f in _figures) Destroy(f.root.gameObject);
            foreach (Number n in _numbers) Destroy(n.text.gameObject);
            _figures.Clear(); _numbers.Clear();
            _campaign = Campaign.New(7);
            _fight = _campaign.StartTutorial();
            _result = null; _pilot = null;
            _started = _paused = _demo = false;
            _speed = 1; _accum = _visualTime = _endAge = 0; _scanned = _acceptedOrders = 0;
            _selected = "";
            _intro.gameObject.SetActive(true); _ending.gameObject.SetActive(false);
            _notice.text = _fight.Encounter.Text;
            _mode.text = "CLARTH  /  OPENING ENCOUNTER";
            int[,] slots = new int[2, 2];
            foreach (BattleUnit u in _fight.Sim.Units)
            {
                int slot = u.IsLeader ? 0 : slots[u.Side, (int)u.Row]++;
                Vector2 feet;
                if (u.IsLeader) feet = new Vector2(149, 549);
                else if (u.Row == Row.Front) feet = new Vector2(u.Side == 0 ? 568 : 895, 421 + slot * 151);
                else feet = new Vector2(u.Side == 0 ? (slot == 1 ? 277 : (slot == 2 ? 402 : 345)) : 1144, 348 + slot * 128);
                _figures.Add(MakeFigure(u, feet));
                if (u.Side == 1 && _selected == "") _selected = u.Id;
            }
            _figures.Sort((a, b) => a.feet.y.CompareTo(b.feet.y));
            foreach (Figure f in _figures) f.root.SetAsLastSibling();
            RefreshHud();
        }

        Figure MakeFigure(BattleUnit unit, Vector2 feet)
        {
            var f = new Figure { unit = unit, feet = feet, root = Rect(_stage, unit.Id, feet.x, feet.y, 1, 1) };
            var shadow = Panel(f.root, -44, -8, 88, 30, Color.white); shadow.sprite = _shadow;
            f.selection = Panel(f.root, -49, 25, 98, 3, Bronze);
            f.health = Panel(f.root, -41, 3, 82, 5, unit.Side == 0 ? Mint : Red);
            Panel(f.root, -65, 12, 130, 21, Alpha(Ink, .8f));
            f.label = Label(f.root, unit.IsLeader ? "COMMANDER" : SliceClasses.Name(unit.Class).ToUpperInvariant(), -76, 10, 152, 24, 12, Bone, FontStyle.Bold, TextAnchor.MiddleCenter);
            f.guard = Panel(f.root, -46, -14, 92, 3, Mint);
            f.guard.gameObject.SetActive(false);
            if (!unit.IsLeader)
            {
                string key = (unit.Side == 0 ? "clarth_" : "blood_rot_") + unit.Class.ToString().ToLowerInvariant() + (unit.Side == 0 ? "_se" : "_sw");
                f.idle = Load(key + "_idle"); f.attack = Load(key + "_attack");
                f.hit = Load(key + "_hit"); f.death = Load(key + "_death");
                f.scale = 1.25f;
                f.body = Raw(f.root, "PixelLab figure", 0, 0, 1, 1);
                f.body.transform.SetAsFirstSibling(); shadow.transform.SetAsFirstSibling();
                f.attackLength = unit.Stats.CooldownTicks / (float)SimClock.TicksPerSecond;
            }
            else
            {
                // Art belongs to Jacob. A labelled tactical token avoids inventing his commander.
                Panel(f.root, -35, -104, 70, 88, Ink);
                Label(f.root, "C", -35, -99, 70, 62, 43, Bronze, FontStyle.Bold, TextAnchor.MiddleCenter);
                Label(f.root, "ART PENDING", -64, -37, 128, 18, 10, Muted, FontStyle.Normal, TextAnchor.MiddleCenter);
            }
            var click = ActionButton(f.root, -68, -170, 136, 196, "", () => Select(unit.Id));
            click.image.color = Color.clear;
            ColorBlock cb = click.colors; cb.normalColor = cb.highlightedColor = cb.pressedColor = Color.white; click.colors = cb;
            return f;
        }

        Clip Load(string key)
        {
            if (_clips.TryGetValue(key, out Clip clip)) return clip;
            var json = Resources.Load<TextAsset>("ClarthStudy/" + key);
            var texture = Resources.Load<Texture2D>("ClarthStudy/" + key);
            if (json == null || texture == null) throw new InvalidOperationException("Missing approved clip: " + key);
            clip = new Clip { texture = texture, sheet = JsonUtility.FromJson<Sheet>(json.text) };
            foreach (Frame f in clip.sheet.frames) clip.seconds += f.duration / 1000f;
            _clips.Add(key, clip);
            return clip;
        }

        void Begin(bool demo)
        {
            if (_started) return;
            _started = true; _demo = demo;
            _intro.gameObject.SetActive(false);
            _pilot = demo ? new CampaignPilot(PilotStyle.Reference, 7) : null;
            _mode.text = demo ? "DEMO  /  SCRIPTED ORDERS" : "CLARTH  /  YOU COMMAND";
            _notice.text = "Drive Bloodridge off. Select an enemy for Focus Fire.";
        }

        void Update()
        {
            if (_fight == null) return;
            _canvas.scaleFactor = Mathf.Min(Screen.width / W, Screen.height / H);
            if (Input.GetKeyDown(KeyCode.Space)) TogglePause();
            if (Input.GetKeyDown(KeyCode.R)) ResetFight();
            for (int i = 0; i < _orders.Length; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) Order(_orders[i]);
            float dt = _capture != null ? 1f / 30f : Mathf.Min(Time.unscaledDeltaTime, .1f);
            if (!_paused)
            {
                _visualTime += dt * (_started && _result == null ? _speed : 1);
                if (_started && _result == null)
                {
                    _accum += dt * _speed * SimClock.TicksPerSecond;
                    while (_accum >= 1 && !_fight.Over)
                    {
                        Anticipate();
                        _pilot?.Step(_fight);
                        _fight.Tick();
                        DrainEvents();
                        _accum -= 1;
                    }
                    if (_fight.Over) Finish();
                }
                if (_result != null) _endAge += dt;
            }
            foreach (Figure f in _figures) DrawFigure(f);
            for (int i = _numbers.Count - 1; i >= 0; i--)
            {
                Number n = _numbers[i]; float age = _visualTime - n.at;
                if (age > .85f) { Destroy(n.text.gameObject); _numbers.RemoveAt(i); continue; }
                n.text.rectTransform.anchoredPosition = new Vector2(n.origin.x, -(n.origin.y - age * 53));
                n.text.color = Alpha(n.text.color, Mathf.Clamp01((.85f - age) * 3));
            }
            _ending.gameObject.SetActive(_result != null && _endAge > 3.6f);
            RefreshHud();
        }

        void Anticipate()
        {
            foreach (Figure f in _figures)
            {
                if (f.attack == null || !f.unit.Alive) continue;
                float lead = f.attackLength * .4f;
                if (f.unit.Cooldown > 0 && f.unit.Cooldown <= lead * SimClock.TicksPerSecond && !f.anticipated)
                {
                    f.attackAt = _visualTime - (lead - f.unit.Cooldown / (float)SimClock.TicksPerSecond);
                    f.anticipated = true;
                }
            }
        }

        void DrainEvents()
        {
            IReadOnlyList<BattleEvent> events = _fight.Sim.Events;
            while (_scanned < events.Count)
            {
                BattleEvent e = events[_scanned++];
                Figure actor = Find(e.ActorId), target = Find(e.TargetId);
                if (e.Kind == BattleEventKind.Hit || e.Kind == BattleEventKind.Heal || e.Kind == BattleEventKind.TrapArmed)
                {
                    if (actor != null && actor.attack != null)
                    {
                        actor.attackAt = _visualTime - actor.attackLength * .4f;
                        actor.anticipated = false;
                    }
                    if (target != null && e.Amount > 0)
                    {
                        FloatNumber(target, (e.Kind == BattleEventKind.Heal ? "+" : "") + e.Amount,
                            e.Kind == BattleEventKind.Heal ? Mint : Bone);
                        if (e.Kind == BattleEventKind.Hit) target.hitAt = _visualTime;
                    }
                }
                if (e.Kind == BattleEventKind.Kill)
                {
                    // Victim identity is TargetId in the sim, never infer it from the actor.
                    if (target != null && !target.unit.Alive) target.deathAt = _visualTime;
                }
                if (e.Kind == BattleEventKind.Directive)
                    _notice.text = (e.ActorSide == 0 ? (_demo ? "DEMO ORDER  /  " : "YOUR ORDER  /  ") : "BLOODRIDGE  /  ") + e.Detail;
                if (e.Kind == BattleEventKind.Windup) _notice.text = "INCOMING  /  Bloodridge is preparing an Eclipse.";
            }
        }

        void DrawFigure(Figure f)
        {
            float hp = f.unit.Hp / (float)f.unit.MaxHp;
            f.health.rectTransform.sizeDelta = new Vector2(82 * hp, 5);
            f.selection.gameObject.SetActive(_selected == f.unit.Id && f.unit.Alive);
            f.guard.gameObject.SetActive(f.unit.Alive && _fight.Sim.IsGuarded(f.unit));
            f.label.color = f.unit.Alive ? Bone : Muted;
            if (f.body == null) return;
            Clip clip = f.idle; float time = _visualTime + f.unit.Index * .41f; bool loop = true;
            if (!f.unit.Alive)
            {
                if (f.deathAt < 0) f.deathAt = _visualTime;
                clip = f.death; time = _visualTime - f.deathAt; loop = false;
            }
            else if (_visualTime - f.hitAt < .22f)
            {
                clip = f.hit; time = (_visualTime - f.hitAt) / .22f * clip.seconds; loop = false;
            }
            else if (_visualTime - f.attackAt < f.attackLength)
            {
                clip = f.attack; time = (_visualTime - f.attackAt) / f.attackLength * clip.seconds; loop = false;
            }
            FrameRect frame = clip.sheet.frames[clip.FrameAt(time, loop)].frame;
            f.body.texture = clip.texture;
            f.body.uvRect = new UnityEngine.Rect(frame.x / (float)clip.texture.width,
                1 - (frame.y + frame.h) / (float)clip.texture.height,
                frame.w / (float)clip.texture.width, frame.h / (float)clip.texture.height);
            f.body.rectTransform.anchoredPosition = new Vector2(-clip.sheet.anchorX * f.scale, clip.sheet.anchorY * f.scale);
            f.body.rectTransform.sizeDelta = new Vector2(frame.w * f.scale, frame.h * f.scale);
            f.body.color = Color.white;
        }

        void Select(string id)
        {
            Figure f = Find(id);
            if (f == null || !f.unit.Alive) return;
            _selected = id;
        }

        bool Order(Directive order)
        {
            if (!_started || _result != null || _demo) return false;
            Figure f = Find(_selected);
            if (order == Directive.FocusFire && (f == null || f.unit.Side != 1 || !f.unit.Alive))
            { _notice.text = "Select a living enemy before ordering Focus Fire."; return false; }
            if (order == Directive.Sacrifice && (f == null || f.unit.Side != 0 || f.unit.IsLeader || !f.unit.Alive))
            { _notice.text = "Select one of your living fighters before ordering Sacrifice."; return false; }
            string target = order == Directive.FocusFire || order == Directive.Sacrifice ? _selected : null;
            bool ok = _fight.TryOrder(order, target, out string failure);
            if (!ok) _notice.text = failure;
            else { _acceptedOrders++; DrainEvents(); }
            return ok;
        }

        void TogglePause() { if (_started && _result == null) _paused = !_paused; }
        void ToggleSpeed() { _speed = _speed == 1 ? 2 : 1; }

        void Finish()
        {
            _result = _campaign.Finish(_fight);
            _paused = false;
            _endTitle.text = _result.TutorialWon ? "THE CLARTH\nSTAND WITH YOU." : "THE RAID\nBROKE YOUR LINE.";
            int losses = 0;
            foreach (BattleUnit u in _fight.Sim.Units) if (u.Side == 0 && u.Role == UnitRole.Fighter && !u.Alive) losses++;
            _endText.text = _result.TutorialWon
                ? _result.JoinedFighters.Count + " surviving Clarth joined your roster.\n" + losses + " fighters fell.\nThis study ends at the first alliance."
                : (_result.TimedOut ? "Time ran out." : "Your force could not hold.") + "\nTry different orders. The campaign state decides the result.";
            _notice.text = _result.TutorialWon ? "VICTORY  /  First alliance earned." : "DEFEAT  /  Your line broke.";
            Debug.Log("[ClarthStudy] Finished: won=" + _result.Won + " ticks=" + _fight.Sim.CurrentTick + " events=" + _fight.Sim.Events.Count);
        }

        void RefreshHud()
        {
            var sim = _fight.Sim;
            int seconds = (int)(sim.CurrentTick / SimClock.TicksPerSecond);
            _time.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00") + (_paused ? "  II" : "");
            _leftCount.text = "ALLIES  " + sim.AliveCount(0) + " ALIVE   /   NERVE " + sim.Nerve(0) / 10;
            _rightCount.text = "NERVE " + sim.Nerve(1) / 10 + "   /   " + sim.AliveCount(1) + " ALIVE  BLOODRIDGE";
            _leftNerve.rectTransform.sizeDelta = new Vector2(354 * sim.Nerve(0) / 1000f, 6);
            _rightNerve.rectTransform.sizeDelta = new Vector2(354 * sim.Nerve(1) / 1000f, 6);
            _points.text = "ORDER POINTS  " + sim.DirectivePoints(0);
            for (int i = 0; i < _orderButtons.Count; i++)
                _orderButtons[i].interactable = _started && !_demo && _result == null && sim.CanUse(0, _orders[i], out _);
            _pauseButton.GetComponentInChildren<Text>().text = _paused ? "RESUME" : "PAUSE";
            _speedButton.GetComponentInChildren<Text>().text = _speed == 1 ? "1×" : "2×";
            Figure f = Find(_selected);
            _target.text = f == null ? "Select a fighter to inspect them." :
                (f.unit.Side == 0 ? "YOUR " : "ENEMY ") + (f.unit.IsLeader ? "COMMANDER" : SliceClasses.Name(f.unit.Class).ToUpperInvariant()) +
                "   /   " + f.unit.Hp + " / " + f.unit.MaxHp + " HP   /   " + f.unit.Row.ToString().ToUpperInvariant() + " ROW";
        }

        void FloatNumber(Figure f, string value, Color colour)
        {
            float offset = (f.numberLane++ % 3 - 1) * 44;
            var origin = new Vector2(f.feet.x - 56 + offset, f.feet.y - 145);
            Text text = Label(_effects, value, origin.x, origin.y, 112, 43, 31, colour, FontStyle.Bold, TextAnchor.MiddleCenter);
            text.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
            _numbers.Add(new Number { text = text, origin = origin, at = _visualTime });
        }
        Figure Find(string id) { foreach (Figure f in _figures) if (f.unit.Id == id) return f; return null; }

        IEnumerator CaptureRun()
        {
            yield return new WaitForEndOfFrame();
            Capture("opening.png");
            yield return null;
            // Exercise the same UI handlers, not a duplicate simulation, before the labelled demo.
            bool startHit = Click(_startButton);
            bool pauseHit = Click(_pauseButton);
            long pausedAt = _fight.Sim.CurrentTick;
            for (int i = 0; i < 5; i++) yield return null;
            bool pausePassed = _fight.Sim.CurrentTick == pausedAt;
            Click(_pauseButton);
            Select(FindEnemy());
            int before = _acceptedOrders;
            bool focusHit = Click(_orderButtons[2]);
            bool focusPassed = _acceptedOrders == before + 1;
            bool noAssist = !_fight.TryOrder(Directive.Eclipse, null, out string assistReason);
            bool raycastsPassed = startHit && pauseHit && focusHit;
            Debug.Log("[ClarthStudy] UI smoke: raycasts=" + raycastsPassed + " pause=" + pausePassed + " focusButton=" + focusPassed + " assistRejected=" + noAssist);
            File.WriteAllText(Path.Combine(_capture, "ui-smoke.txt"), "raycasts=" + raycastsPassed + "\npause=" + pausePassed + "\nfocusButton=" + focusPassed + "\nassistRejected=" + noAssist + "\n" + assistReason);
            ResetFight(); Begin(true);
            int frame = 0;
            while (_endAge < 6 && _visualTime < 125)
            {
                yield return new WaitForEndOfFrame();
                if (Time.frameCount % 2 == 0)
                    Capture("frame-" + (frame++).ToString("D5") + ".png");
            }
            Capture("result.png");
            File.WriteAllText(Path.Combine(_capture, "result.txt"), "result=" + (_result == null ? "TIMEOUT" : _result.Won.ToString()) +
                "\nticks=" + _fight.Sim.CurrentTick + "\nevents=" + _fight.Sim.Events.Count + "\nframes=" + frame + "\nclips=" + _clips.Count);
            yield return new WaitForSecondsRealtime(1);
            Application.Quit(_result != null && pausePassed && focusPassed && noAssist && raycastsPassed ? 0 : 1);
        }

        string FindEnemy() { foreach (Figure f in _figures) if (f.unit.Side == 1 && f.unit.Alive) return f.unit.Id; return ""; }
        bool Click(Button button)
        {
            Canvas.ForceUpdateCanvases();
            var rect = button.GetComponent<RectTransform>();
            var pointer = new PointerEventData(EventSystem.current)
            { position = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            if (hits.Count == 0 || hits[0].gameObject.GetComponentInParent<Button>() != button) return false;
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            return true;
        }
        void Capture(string name)
        {
            Canvas.ForceUpdateCanvases();
            Camera.main.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = _captureTarget;
            _capturePixels.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0);
            _capturePixels.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes(Path.Combine(_capture, name), _capturePixels.EncodeToPNG());
        }
        static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
            return null;
        }
        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out Color c); return c; }
        static Color Alpha(Color c, float a) { c.a = a; return c; }
        static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
        }
        static Image Panel(Transform parent, float x, float y, float w, float h, Color c)
        {
            Image image = Rect(parent, "Panel", x, y, w, h).gameObject.AddComponent<Image>();
            image.color = c; image.raycastTarget = false; return image;
        }
        static RawImage Raw(Transform parent, string name, float x, float y, float w, float h)
        { RawImage image = Rect(parent, name, x, y, w, h).gameObject.AddComponent<RawImage>(); image.raycastTarget = false; return image; }
        Text Label(Transform parent, string value, float x, float y, float w, float h, int size, Color c,
            FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            Text text = Rect(parent, value, x, y, w, h).gameObject.AddComponent<Text>();
            text.font = _font; text.text = value; text.fontSize = size; text.fontStyle = style; text.color = c;
            text.alignment = alignment; text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Wrap;
            return text;
        }
        Button ActionButton(Transform parent, float x, float y, float w, float h, string title, UnityEngine.Events.UnityAction action, int size = 18)
        {
            Image image = Panel(parent, x, y, w, h, Hex("3E302C")); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colours = button.colors; colours.normalColor = Color.white; colours.highlightedColor = Hex("FFE0AF");
            colours.pressedColor = Hex("D49359"); colours.disabledColor = new Color(.7f, .7f, .7f, .52f); button.colors = colours;
            button.onClick.AddListener(action);
            Label(button.transform, title, 14, 6, w - 28, h > 60 ? 34 : h - 12, size, Bone, FontStyle.Bold);
            return button;
        }
        void OnDestroy()
        {
            if (_shadow != null) { Destroy(_shadow.texture); Destroy(_shadow); }
            if (_canvas != null) Destroy(_canvas.gameObject);
            if (_capturePixels != null) Destroy(_capturePixels);
            if (_captureTarget != null) { _captureTarget.Release(); Destroy(_captureTarget); }
            if (_capture != null) Time.captureDeltaTime = 0;
        }
    }
}
