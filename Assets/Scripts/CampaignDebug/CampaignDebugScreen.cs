using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.CampaignDebug
{
    /// Plain debug screens for the slice loop (Jacob's go, 9 Oct 2026): grey boxes, no art, no
    /// style, just enough to play the campaign by hand and judge it before the look is locked.
    /// Tutorial, tribe choice, three encounter cards, respect bar, squad, the fight with its
    /// orders and assist, results, recruiting, and a save that survives closing the game. Every
    /// rule lives in Campaign; this screen only shows state and sends orders through CampaignFight.
    /// The real frontend is Codex's (AGENTS.md, 6 Oct 2026); this is a harness to playtest the loop.
    ///
    /// Run with -ksCampaignShots folder to play itself on the reference pilot, save a screenshot
    /// of each screen into that folder, reopen its own save and quit.
    public sealed class CampaignDebugScreen : MonoBehaviour
    {
        public const string ShotsArg = "-ksCampaignShots";
        const float RefWidth = 1280f;
        const float RefHeight = 720f;
        const string SaveName = "campaign_loop_test.txt";
        const float AutoTimeoutSeconds = 900f;

        enum Page { Title, Tutorial, ChooseTribe, Offers, Squad, Fight, Result, Finished }

        static readonly Directive[] Orders = { Directive.Hold, Directive.FallBack, Directive.FocusFire, Directive.Unleash, Directive.Eclipse };
        static readonly Color Ink = new Color(0.92f, 0.92f, 0.9f);
        static readonly Color Panel = new Color(0.16f, 0.16f, 0.17f, 1f);
        static readonly Color Trough = new Color(0.08f, 0.08f, 0.09f, 1f);
        static readonly Color Ours = new Color(0.45f, 0.75f, 0.45f);
        static readonly Color Theirs = new Color(0.85f, 0.38f, 0.33f);
        static readonly Color Warning = new Color(1f, 0.72f, 0.3f);
        static readonly Color Respect = new Color(0.4f, 0.65f, 0.95f);

        Campaign _campaign;
        Page _page = Page.Title;
        float _pageClock;
        string _notice = string.Empty;
        string _savePath = string.Empty;
        string _saveSummary = string.Empty;
        bool _showCodex;
        Vector2 _scrollLeft, _scrollRight;

        int _offer = -1;
        readonly List<string> _squad = new List<string>();

        CampaignFight _fight;
        CampaignPilot _pilot;
        bool _pilotOn;
        bool _paused;
        float _speed = 1f;
        float _accum;
        bool _targeting;
        int _eventCursor;
        string _theyKnow = string.Empty;
        readonly List<string> _feed = new List<string>();
        EncounterResult _result;

        string _shots = string.Empty;
        bool _auto;
        int _shotCount;
        readonly List<string> _shotsTaken = new List<string>();
        readonly StringBuilder _log = new StringBuilder();
        int _autoRuns;
        bool _reopenDone;
        bool _windupShot;
        float _autoClock;

        GUIStyle _h1, _h2, _body, _small, _tag, _warn, _button;

        // ------------------------------------------------------------ lifecycle

        void Awake()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == ShotsArg) _shots = args[i + 1];
                if (args[i] == TrialShotArg) _trialShot = args[i + 1];
            }
            _auto = _shots.Length > 0;
            if (_auto)
            {
                Application.runInBackground = true;
                Directory.CreateDirectory(_shots);
                _savePath = Path.Combine(_shots, SaveName);
                if (File.Exists(_savePath)) File.Delete(_savePath);
            }
            else
            {
                _savePath = Path.Combine(Application.persistentDataPath, SaveName);
            }
            ShowTitle();
            if (_trialShot.Length > 0) OpenSkullpeakTrial();
        }

        /// Run with -ksSkullpeakTrialShot file.png to open Skullpeak's precision trial straight away,
        /// let the reference pilot play ten seconds of it, save one screenshot there and quit
        /// (10 Oct 2026: to look at the trial's set lineup in the real build). Nothing is saved.
        const string TrialShotArg = "-ksSkullpeakTrialShot";
        string _trialShot = string.Empty;
        bool _trialTaken;
        float _trialQuitAt = -1f;

        void OpenSkullpeakTrial()
        {
            Application.runInBackground = true;
            for (ulong seed = 1; seed < 200; seed++)
            {
                Campaign c = Campaign.New(seed);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) continue;
                c.ChooseTribe(CampaignTribes.SkullpeakId);
                for (int i = 0; i < c.Offers.Count; i++)
                {
                    if (c.Offers[i].Kind != EncounterKind.Trial) continue;
                    _campaign = c;
                    _fight = c.StartEncounter(i);
                    BeginFight();
                    _pilotOn = true;
                    _speed = 2f;
                    Log("Skullpeak trial from seed " + seed);
                    return;
                }
            }
            Log("No Skullpeak trial was offered in 200 seeds.");
            Application.Quit(1);
        }

        void TrialShotStep()
        {
            if (!_trialTaken && _fight != null && _fight.Sim.CurrentTick >= 300)
            {
                _trialTaken = true;
                ScreenCapture.CaptureScreenshot(_trialShot);
                _trialQuitAt = Time.unscaledTime + 1.5f;
            }
            if (_trialQuitAt > 0f && Time.unscaledTime >= _trialQuitAt) Application.Quit();
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _pageClock += dt;
            if (_page == Page.Fight) UpdateFight(dt);
            if (_trialShot.Length > 0) TrialShotStep();
            if (!_auto) return;
            _autoClock += dt;
            if (_autoClock > AutoTimeoutSeconds) FinishAuto("FAIL: timed out on page " + _page);
            else AutoStep();
        }

        void UpdateFight(float dt)
        {
            if (_fight == null || _paused || _fight.Over) return;
            _accum += dt * SimClock.TicksPerSecond * _speed;
            while (_accum >= 1f && !_fight.Over)
            {
                _accum -= 1f;
                if (_pilotOn) _pilot.Step(_fight);
                _fight.Tick();
            }
            DrainEvents();
        }

        // ------------------------------------------------------------ flow

        void SetPage(Page page)
        {
            _page = page;
            _pageClock = 0f;
            _scrollLeft = _scrollRight = Vector2.zero;
            _showCodex = false;
        }

        void ShowTitle()
        {
            _saveSummary = string.Empty;
            if (File.Exists(_savePath))
            {
                if (CampaignSave.TryRead(File.ReadAllText(_savePath), null, out Campaign saved, out string failure))
                    _saveSummary = Summary(saved);
                else
                    _saveSummary = "The save could not be read: " + failure;
            }
            SetPage(Page.Title);
        }

        void NewCampaign()
        {
            ulong seed = _auto ? 21UL : (ulong)(DateTime.Now.Ticks % 1000000L) + 1UL;
            _campaign = Campaign.New(seed);
            _notice = string.Empty;
            Save();
            Log("New campaign, seed " + seed);
            SetPage(Page.Tutorial);
        }

        void ContinueSaved()
        {
            if (!File.Exists(_savePath))
            {
                _notice = "No save found.";
                return;
            }
            if (!CampaignSave.TryRead(File.ReadAllText(_savePath), null, out Campaign c, out string failure))
            {
                _notice = "The save could not be read: " + failure;
                return;
            }
            _campaign = c;
            _notice = string.Empty;
            Log("Reopened the save: " + Summary(c));
            GoToStage();
        }

        void GoToStage()
        {
            switch (_campaign.Stage)
            {
                case CampaignStage.Tutorial: SetPage(Page.Tutorial); break;
                case CampaignStage.ChooseTribe: SetPage(Page.ChooseTribe); break;
                case CampaignStage.InRun: SetPage(Page.Offers); break;
                default: SetPage(Page.Finished); break;
            }
        }

        /// A run in progress is not saved (CampaignSave): the file only changes between runs.
        void Save()
        {
            if (_campaign == null || _campaign.Stage == CampaignStage.InRun) return;
            try
            {
                File.WriteAllText(_savePath, CampaignSave.Write(_campaign));
            }
            catch (Exception e)
            {
                _notice = "Saving failed: " + e.Message;
            }
        }

        void GoAfter(string tribeId)
        {
            _campaign.ChooseTribe(tribeId);
            Log("Run " + _campaign.RunsStarted + " against " + TribeName(tribeId) + ", respect " + _campaign.Respect(tribeId) + "/" +
                _campaign.RespectNeeded(tribeId));
            SetPage(Page.Offers);
        }

        void TakeOffer(int index)
        {
            _offer = index;
            _squad.Clear();
            List<CampaignFighter> squad = _campaign.DefaultSquad(_campaign.Offers[index].SquadSize);
            for (int i = 0; i < squad.Count; i++) _squad.Add(squad[i].Id);
            SetPage(Page.Squad);
        }

        void StartTutorialFight()
        {
            _fight = _campaign.StartTutorial();
            BeginFight();
        }

        void StartEncounterFight()
        {
            try
            {
                _fight = _campaign.StartEncounter(_offer, _squad);
            }
            catch (ArgumentException e)
            {
                _notice = e.Message;
                return;
            }
            BeginFight();
        }

        void BeginFight()
        {
            _pilot = new CampaignPilot(PilotStyle.Reference, _campaign.Seed + (ulong)_campaign.Step);
            _pilotOn = _auto;
            _speed = _auto ? 8f : 1f;
            _paused = false;
            _accum = 0f;
            _targeting = false;
            _eventCursor = 0;
            _feed.Clear();
            _notice = string.Empty;
            TacticsProfile theirs = _fight.Setup.CopyTactics(1);
            var known = new List<string>();
            if (theirs.CoverFocused >= BattleSimulation.HabitThreshold) known.Add("their Warden covers whoever you focus");
            if (theirs.HoldOnUnleash >= BattleSimulation.HabitThreshold) known.Add("they raise shields when you Unleash");
            if (theirs.PressOnFallBack >= BattleSimulation.HabitThreshold) known.Add("they Unleash when you fall back");
            if (theirs.AvoidGuarded >= BattleSimulation.HabitThreshold) known.Add("their archers shoot whoever your Warden is not covering");
            if (theirs.TrapCaution >= BattleSimulation.HabitThreshold) known.Add("they let their Warden spring your traps");
            _theyKnow = known.Count == 0 ? "nothing yet" : string.Join("; ", known.ToArray());
            Feed(_fight.Encounter.Title + " begins.");
            SetPage(Page.Fight);
        }

        void ShowResult()
        {
            _result = _campaign.Finish(_fight);
            _fight = null;
            if (_result.TutorialWon || _result.RunOver) Save();
            Log(_result.Encounter.Title + ": " + Outcome(_result) + ", respect " + _result.RespectBefore + " -> " + _result.RespectAfter +
                (_result.Joined ? ", JOINED" : string.Empty) + (_result.SentAway ? ", SENT AWAY" : string.Empty));
            SetPage(Page.Result);
        }

        void AfterResult()
        {
            _result = null;
            GoToStage();
        }

        // ------------------------------------------------------------ orders

        void Order(Directive directive)
        {
            if (_fight == null || _fight.Over) return;
            if (directive == Directive.FocusFire)
            {
                _targeting = !_targeting;
                return;
            }
            if (!_fight.TryOrder(directive, null, out string failure)) _notice = failure;
            else _notice = string.Empty;
        }

        void Focus(BattleUnit target)
        {
            _targeting = false;
            if (!_fight.TryOrder(Directive.FocusFire, target.Id, out string failure)) _notice = failure;
            else _notice = string.Empty;
        }

        void HandleKeys(Event e)
        {
            if (e.type != EventType.KeyDown || _page != Page.Fight || _fight == null) return;
            switch (e.keyCode)
            {
                case KeyCode.Alpha1: Order(Directive.Hold); break;
                case KeyCode.Alpha2: Order(Directive.FallBack); break;
                case KeyCode.Alpha3: Order(Directive.FocusFire); break;
                case KeyCode.Alpha4: Order(Directive.Unleash); break;
                case KeyCode.Alpha5: Order(Directive.Eclipse); break;
                case KeyCode.Space: _paused = !_paused; break;
                case KeyCode.Escape: _targeting = false; break;
                default: return;
            }
            e.Use();
        }

        // ------------------------------------------------------------ drawing

        void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Min(Screen.width / RefWidth, Screen.height / RefHeight);
            var offset = new Vector2((Screen.width - RefWidth * scale) * 0.5f, (Screen.height - RefHeight * scale) * 0.5f);
            GUI.matrix = Matrix4x4.TRS(new Vector3(offset.x, offset.y, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
            HandleKeys(Event.current);
            Fill(new Rect(0f, 0f, RefWidth, RefHeight), new Color(0.11f, 0.11f, 0.12f, 1f));
            switch (_page)
            {
                case Page.Title: DrawTitle(); break;
                case Page.Tutorial: DrawTutorial(); break;
                case Page.ChooseTribe: DrawChooseTribe(); break;
                case Page.Offers: DrawOffers(); break;
                case Page.Squad: DrawSquad(); break;
                case Page.Fight: DrawFight(); break;
                case Page.Result: DrawResult(); break;
                case Page.Finished: DrawFinished(); break;
            }
            if (_showCodex) DrawCodex();
            if (_notice.Length > 0) GUI.Label(new Rect(20f, RefHeight - 46f, 1240f, 24f), _notice, _warn);
            GUI.Label(new Rect(20f, RefHeight - 22f, 1240f, 20f),
                "Loop test build: debug screens, no art, no final UI." + (_auto ? "  AUTOPLAY for screenshots." : string.Empty), _small);
        }

        void DrawTitle()
        {
            GUI.Label(new Rect(40f, 40f, 1200f, 40f), "KINDRED SIEGE  /  LOOP TEST", _h1);
            GUI.Label(new Rect(40f, 90f, 1200f, 50f),
                "Play the slice loop by hand: the tutorial, choosing a tribe, three encounters a step, the respect bar, the chief, " +
                "recruiting, the assist, then close the game and reopen it. Placeholder screens only.", _body);
            bool hasSave = File.Exists(_savePath);
            if (hasSave)
            {
                Fill(new Rect(40f, 160f, 1200f, 110f), Panel);
                GUI.Label(new Rect(56f, 170f, 1170f, 24f), "SAVED CAMPAIGN", _h2);
                GUI.Label(new Rect(56f, 198f, 1170f, 66f), _saveSummary, _body);
                if (GUI.Button(new Rect(40f, 290f, 300f, 48f), "CONTINUE", _button)) ContinueSaved();
                if (GUI.Button(new Rect(360f, 290f, 360f, 48f), "NEW CAMPAIGN (replaces the save)", _button)) NewCampaign();
            }
            else if (GUI.Button(new Rect(40f, 170f, 300f, 48f), "NEW CAMPAIGN", _button))
            {
                NewCampaign();
            }
            if (GUI.Button(new Rect(40f, 360f, 300f, 40f), "QUIT", _button)) Application.Quit();
            GUI.Label(new Rect(40f, 430f, 1200f, 60f),
                "Keys in a fight: 1 Hold, 2 Fall Back, 3 Focus Fire then click an enemy, 4 Unleash, 5 call your assist, Space pause, Esc cancel.", _body);
            GUI.Label(new Rect(40f, 490f, 1200f, 40f), "Save file: " + _savePath, _small);
        }

        void DrawTutorial()
        {
            Fill(new Rect(140f, 80f, 1000f, 440f), Panel);
            GUI.Label(new Rect(170f, 100f, 940f, 36f), "TUTORIAL  /  THE RAID ON YOUR JAILERS", _h1);
            GUI.Label(new Rect(170f, 150f, 940f, 120f),
                "The Clarth found what was left of you after the wipe and locked you up. Now Bloodridge has come to take you. " +
                "Command the Clarth and drive them off: win, and the Clarth are yours.", _body);
            GUI.Label(new Rect(170f, 260f, 940f, 90f),
                "Your fighters act on their own. You give the orders, and orders cost points that refill. " +
                "If you lose, you try again.", _body);
            if (GUI.Button(new Rect(170f, 440f, 260f, 48f), "FIGHT", _button)) StartTutorialFight();
            if (GUI.Button(new Rect(860f, 440f, 250f, 48f), "BACK TO TITLE", _button)) ShowTitle();
        }

        void DrawChooseTribe()
        {
            GUI.Label(new Rect(20f, 14f, 1240f, 36f), "CHOOSE A TRIBE TO GO AFTER", _h1);
            DrawForces(new Rect(20f, 60f, 400f, 590f));
            List<CampaignTribe> choosable = _campaign.Choosable();
            float x = 440f;
            for (int i = 0; i < CampaignTribes.All.Length; i++)
            {
                CampaignTribe t = CampaignTribes.All[i];
                if (t.IsStarter) continue;
                DrawTribePanel(new Rect(x, 60f, 405f, 590f), t, choosable.Contains(t));
                x += 415f;
            }
            if (GUI.Button(new Rect(1010f, 14f, 120f, 34f), "CODEX", _button)) _showCodex = !_showCodex;
            if (GUI.Button(new Rect(1140f, 14f, 120f, 34f), "TITLE", _button)) ShowTitle();
        }

        void DrawForces(Rect r)
        {
            Fill(r, Panel);
            GUI.Label(new Rect(r.x + 12f, r.y + 8f, r.width - 24f, 26f), "YOUR FORCES", _h2);
            var b = new StringBuilder();
            b.Append("Tribes with you: ");
            for (int i = 0; i < _campaign.Joined.Count; i++) b.Append(i > 0 ? ", " : string.Empty).Append(TribeName(_campaign.Joined[i]));
            b.Append("\nAssists: ");
            if (_campaign.Assists.Count == 0) b.Append("none yet");
            for (int i = 0; i < _campaign.Assists.Count; i++)
                b.Append(i > 0 ? ", " : string.Empty).Append(_campaign.Assists[i].Name).Append(" from ").Append(_campaign.Assists[i].ChiefName);
            b.Append("\nRuns started ").Append(_campaign.RunsStarted).Append(", won ").Append(_campaign.RunsWon);
            GUI.Label(new Rect(r.x + 12f, r.y + 38f, r.width - 24f, 90f), b.ToString(), _body);
            GUI.Label(new Rect(r.x + 12f, r.y + 130f, r.width - 24f, 22f), "Fighters (" + _campaign.Reserve.Count + ")", _h2);
            _scrollLeft = GUI.BeginScrollView(new Rect(r.x + 12f, r.y + 158f, r.width - 24f, r.height - 170f), _scrollLeft,
                new Rect(0f, 0f, r.width - 44f, Math.Max(r.height - 170f, _campaign.Reserve.Count * 24f)));
            for (int i = 0; i < _campaign.Reserve.Count; i++)
                GUI.Label(new Rect(0f, i * 24f, r.width - 44f, 22f), _campaign.Reserve[i].DisplayName, _body);
            GUI.EndScrollView();
        }

        void DrawTribePanel(Rect r, CampaignTribe t, bool choosable)
        {
            Fill(r, Panel);
            GUI.Label(new Rect(r.x + 12f, r.y + 8f, r.width - 24f, 30f), t.Name.ToUpperInvariant() + "   tier " + t.Tier, _h1);
            RespectBar(new Rect(r.x + 12f, r.y + 46f, r.width - 24f, 22f), _campaign.Respect(t.Id), _campaign.RespectNeeded(t.Id));
            float y = r.y + 78f;
            if (_campaign.HasJoined(t.Id))
            {
                GUI.Label(new Rect(r.x + 12f, y, r.width - 24f, 50f), "JOINED YOU. Their chief answers your call: " + t.AssistName + ".", _warn);
                y += 54f;
            }
            var code = new StringBuilder("They respect:");
            for (int i = 0; i < t.Respects.Count; i++) code.Append("\n  ").Append(Feats.Name(t.Respects[i])).Append(": ").Append(Feats.Describe(t.Respects[i]));
            code.Append("\nThey hate: ").Append(Feats.Name(t.Hates)).Append(". ").Append(Feats.Describe(t.Hates));
            float h = _small.CalcHeight(new GUIContent(code.ToString()), r.width - 24f);
            GUI.Label(new Rect(r.x + 12f, y, r.width - 24f, h), code.ToString(), _small);
            y += h + 8f;

            TribeMind mind = _campaign.Mind(t.Id);
            GUI.Label(new Rect(r.x + 12f, y, r.width - 24f, 22f), "WHAT THEY KNOW", _h2);
            y += 24f;
            GUI.Label(new Rect(r.x + 12f, y, r.width - 24f, 36f), mind.HabitsText(), _small);
            y += 38f;
            GUI.Label(new Rect(r.x + 12f, y, r.width - 24f, 22f), mind.Lessons.Count == 0 ? "THEY HAVE NOT MET YOU" : "THEY REMEMBER YOU", _h2);
            y += 24f;
            var why = new StringBuilder();
            int shown = 0;
            for (int i = mind.Lessons.Count - 1; i >= 0 && shown < 4; i--, shown++) why.Append("- ").Append(mind.Lessons[i].Why).Append('\n');
            float bottom = r.yMax - (choosable ? 58f : 10f);
            GUI.Label(new Rect(r.x + 12f, y, r.width - 24f, Math.Max(20f, bottom - y)), why.Length == 0 ? "No fights with you yet." : why.ToString(), _small);
            if (choosable && GUI.Button(new Rect(r.x + 12f, r.yMax - 52f, r.width - 24f, 42f), "GO AFTER " + t.Name.ToUpperInvariant(), _button))
                GoAfter(t.Id);
        }

        void DrawOffers()
        {
            CampaignTribe t = _campaign.Target;
            GUI.Label(new Rect(20f, 12f, 1000f, 34f), "RUN AGAINST " + t.Name.ToUpperInvariant() + "   /   step " + (_campaign.Step + 1) + " of up to " +
                _campaign.Tuning.MaxSteps, _h1);
            RespectBar(new Rect(20f, 54f, 760f, 24f), _campaign.Respect(t.Id), _campaign.RespectNeeded(t.Id));
            var shownRun = new StringBuilder("Shown this run: ");
            if (_campaign.FeatsThisRun.Count == 0) shownRun.Append("nothing yet (the first time you show a feat it is worth 20, again 5, a win 10)");
            for (int i = 0; i < _campaign.FeatsThisRun.Count; i++) shownRun.Append(i > 0 ? ", " : string.Empty).Append(Feats.Name(_campaign.FeatsThisRun[i]));
            GUI.Label(new Rect(800f, 54f, 460f, 44f), shownRun.ToString(), _small);
            GUI.Label(new Rect(20f, 84f, 1240f, 40f), "They hate " + Feats.Name(t.Hates).ToLowerInvariant() + ": " + Feats.Describe(t.Hates) +
                "  Losing a fight ends the run and halves this bar. Your dead stay dead.", _small);

            int count = _campaign.Offers.Count;
            float width = count == 1 ? 600f : 405f;
            float x = count == 1 ? 340f : 20f;
            for (int i = 0; i < count; i++)
            {
                DrawOfferCard(new Rect(x, 130f, width, 500f), _campaign.Offers[i], i);
                x += width + 12f;
            }
            if (GUI.Button(new Rect(1140f, 12f, 120f, 34f), "CODEX", _button)) _showCodex = !_showCodex;
        }

        void DrawOfferCard(Rect r, Encounter e, int index)
        {
            Fill(r, Panel);
            GUI.Label(new Rect(r.x + 14f, r.y + 10f, r.width - 28f, 32f), e.Title.ToUpperInvariant(), _h1);
            GUI.Label(new Rect(r.x + 14f, r.y + 48f, r.width - 28f, 90f), e.Text, _body);
            string sides = "Yours: " + e.SquadSize + (e.Kind == EncounterKind.Duel ? " fighter" : " fighters" + " and your leader") +
                "\nTheirs: " + (e.EnemyFighters > 0 ? e.EnemyFighters + " fighters" : string.Empty) +
                (e.EnemyUnits > e.EnemyFighters ? (e.EnemyFighters > 0 ? " and " : string.Empty) + (e.Kind == EncounterKind.Chief ? "their chief" :
                    e.Kind == EncounterKind.Duel ? "their champion" : "their shaman") : string.Empty) +
                (e.EnemyCasts ? "\nSomeone on their side winds up Eclipse." : string.Empty) +
                (e.NoFallBack ? "\nFall Back is refused here." : string.Empty);
            GUI.Label(new Rect(r.x + 14f, r.y + 142f, r.width - 28f, 100f), sides, _body);
            var show = new StringBuilder("Can show their code:");
            if (e.CanShow.Count == 0) show.Append("\n  nothing (a win is still worth 10)");
            for (int i = 0; i < e.CanShow.Count; i++)
            {
                Feat f = e.CanShow[i];
                bool again = Contains(_campaign.FeatsThisRun, f);
                show.Append("\n  ").Append(Feats.Name(f)).Append(again ? "  (again, +5)" : "  (new, +20)");
            }
            GUI.Label(new Rect(r.x + 14f, r.y + 250f, r.width - 28f, 180f), show.ToString(), _body);
            if (GUI.Button(new Rect(r.x + 14f, r.yMax - 56f, r.width - 28f, 44f), e.Kind == EncounterKind.Chief ? "FACE THE CHIEF" : "TAKE THIS", _button))
                TakeOffer(index);
        }

        void DrawSquad()
        {
            Encounter e = _campaign.Offers[_offer];
            GUI.Label(new Rect(20f, 14f, 1240f, 36f), e.Title.ToUpperInvariant() + "   /   pick up to " + e.SquadSize +
                (e.SquadSize == 1 ? " fighter" : " fighters"), _h1);
            GUI.Label(new Rect(20f, 54f, 1240f, 24f), "Selected " + _squad.Count + " of up to " + e.SquadSize +
                ". Fighters fight at the strength of this tribe's tier. Whoever dies here stays dead.", _body);
            for (int i = 0; i < _campaign.Reserve.Count; i++)
            {
                CampaignFighter f = _campaign.Reserve[i];
                bool picked = _squad.Contains(f.Id);
                var cell = new Rect(20f + (i % 4) * 312f, 92f + (i / 4) * 52f, 300f, 44f);
                Fill(cell, picked ? new Color(0.25f, 0.33f, 0.25f) : Panel);
                if (GUI.Button(cell, (picked ? "[x] " : "[  ] ") + f.DisplayName, _button))
                {
                    if (picked) _squad.Remove(f.Id);
                    else if (_squad.Count < e.SquadSize) _squad.Add(f.Id);
                }
            }
            GUI.enabled = _squad.Count > 0;
            if (GUI.Button(new Rect(1020f, 600f, 240f, 48f), "FIGHT", _button)) StartEncounterFight();
            GUI.enabled = true;
            if (GUI.Button(new Rect(760f, 600f, 240f, 48f), "DEFAULT SQUAD", _button)) TakeOffer(_offer);
            if (GUI.Button(new Rect(20f, 600f, 240f, 48f), "BACK TO THE CARDS", _button)) SetPage(Page.Offers);
        }

        void DrawFight()
        {
            BattleSimulation sim = _fight.Sim;
            Encounter e = _fight.Encounter;
            long limit = _fight.Setup.TickLimit;
            GUI.Label(new Rect(20f, 8f, 860f, 32f), e.Title.ToUpperInvariant() + "  /  " + e.Tribe.Name, _h1);
            string clock = Seconds(sim.CurrentTick) + " of " + Seconds(limit) +
                (e.Kind == EncounterKind.HoldTheLine ? "  (hold until the end)" : "  (break them before time runs out)");
            GUI.Label(new Rect(880f, 12f, 380f, 26f), clock, _body);

            NerveBar(new Rect(20f, 44f, 600f, 20f), "YOUR NERVE", sim.Nerve(0), Ours);
            NerveBar(new Rect(660f, 44f, 600f, 20f), "THEIR NERVE", sim.Nerve(1), Theirs);
            DrawFightBanner(sim, e);

            DrawSide(new Rect(20f, 112f, 600f, 300f), sim, 0);
            DrawSide(new Rect(660f, 112f, 600f, 300f), sim, 1);

            Fill(new Rect(20f, 418f, 600f, 110f), Panel);
            GUI.Label(new Rect(30f, 422f, 580f, 102f), Tracker(sim, e), _small);
            Fill(new Rect(660f, 418f, 600f, 110f), Panel);
            GUI.Label(new Rect(670f, 422f, 580f, 102f), "THEY KNOW: " + _theyKnow + "\n" + States(sim) +
                "A LEADER who drops breaks his whole side.", _small);

            // Three lines, kept clear of the order points line below (it overlapped, 9 Oct 2026).
            var feed = new StringBuilder();
            for (int i = Math.Max(0, _feed.Count - 3); i < _feed.Count; i++) feed.Append(_feed[i]).Append('\n');
            GUI.Label(new Rect(20f, 532f, 1240f, 54f), feed.ToString(), _small);

            DrawOrders(sim);
        }

        void DrawFightBanner(BattleSimulation sim, Encounter e)
        {
            var r = new Rect(20f, 70f, 1240f, 36f);
            int w = sim.WindupTicks(1);
            if (w > 0)
            {
                Fill(r, new Color(0.45f, 0.12f, 0.1f));
                BattleUnit caster = sim.FindById(sim.WindupCasterId(1));
                GUI.Label(new Rect(r.x + 10f, r.y + 6f, r.width - 20f, 26f), "THEIR " + (caster == null ? "CASTER" : UnitName(caster).ToUpperInvariant()) +
                    " IS WINDING UP ECLIPSE: " + Seconds(w) + ".  Break it (focus him), dodge it (Fall Back" +
                    (e.NoFallBack ? ", refused here" : string.Empty) + ") or soak it (Hold).", _h2);
                return;
            }
            if (sim.WornOutTicksLeft(1) > 0)
            {
                Fill(r, new Color(0.15f, 0.3f, 0.15f));
                GUI.Label(new Rect(r.x + 10f, r.y + 6f, r.width - 20f, 26f), "THEIR CASTER IS WORN OUT FOR " + Seconds(sim.WornOutTicksLeft(1)) +
                    ": he takes double damage. Focus him now.", _h2);
                return;
            }
            if (_fight.Over)
            {
                Fill(r, new Color(0.2f, 0.2f, 0.28f));
                GUI.Label(new Rect(r.x + 10f, r.y + 6f, r.width - 20f, 26f), "THE FIGHT IS OVER.  Press SEE RESULT.", _h2);
                return;
            }
            if (_targeting)
            {
                Fill(r, new Color(0.3f, 0.25f, 0.1f));
                GUI.Label(new Rect(r.x + 10f, r.y + 6f, r.width - 20f, 26f), "FOCUS FIRE: click an enemy on the right. Esc cancels.", _h2);
            }
        }

        void DrawSide(Rect r, BattleSimulation sim, int side)
        {
            Fill(r, Panel);
            GUI.Label(new Rect(r.x + 10f, r.y + 4f, r.width - 20f, 22f), side == 0 ? "YOURS" : "THEIRS", _h2);
            float y = r.y + 30f;
            for (int i = 0; i < sim.Units.Count; i++)
            {
                BattleUnit u = sim.Units[i];
                if (u.Side != side) continue;
                var row = new Rect(r.x + 10f, y, r.width - 20f, 30f);
                bool caster = sim.WindupTicks(side) > 0 && sim.WindupCasterId(side) == u.Id;
                string tags = (u.Row == Row.Back ? "back" : "front") + (u.IsLeader ? ", LEADER" : string.Empty) +
                    (sim.IsGuarded(u) ? ", guarded" : string.Empty) + (caster ? ", CASTING" : string.Empty);
                if (side == 1 && _targeting && u.Alive && GUI.Button(new Rect(row.x, row.y, 230f, 28f), "FOCUS " + UnitName(u), _button))
                    Focus(u);
                else
                    GUI.Label(new Rect(row.x, row.y + 4f, 230f, 24f), UnitName(u), _body);
                if (u.Alive)
                {
                    Fill(new Rect(row.x + 236f, row.y + 6f, 140f, 16f), Trough);
                    Fill(new Rect(row.x + 236f, row.y + 6f, 140f * u.HpPermille / 1000f, 16f), side == 0 ? Ours : Theirs);
                    GUI.Label(new Rect(row.x + 382f, row.y + 4f, row.width - 382f, 24f), tags, _tag);
                }
                else
                {
                    GUI.Label(new Rect(row.x + 236f, row.y + 4f, row.width - 236f, 24f), "DOWN", _warn);
                }
                y += 32f;
                if (y > r.yMax - 30f) break;
            }
        }

        void DrawOrders(BattleSimulation sim)
        {
            var bar = new Rect(20f, 612f, 1240f, 62f);
            Fill(bar, Panel);
            int points = sim.DirectivePoints(0);
            int refill = _fight.Setup.RefillTicks;
            string pointsText = "ORDER POINTS " + points + " of " + _fight.Setup.StartingDirectivePoints +
                (points >= _fight.Setup.StartingDirectivePoints || refill <= 0 ? string.Empty : ", +1 in " + Seconds(refill - sim.CurrentTick % refill)) +
                (sim.IsEmpowered(0) ? "   NEXT ORDER DOUBLED" : string.Empty);
            GUI.Label(new Rect(bar.x + 10f, bar.y - 22f, 760f, 22f), pointsText, _h2);
            for (int i = 0; i < Orders.Length; i++)
            {
                Directive d = Orders[i];
                string text = (i + 1) + " " + OrderName(d) + " [" + Directives.Cost(d) + "]" + TabooNote(d);
                bool allowed = !_fight.Over && sim.CanUse(0, d, out _);
                if (d == Directive.FallBack && _fight.Encounter.NoFallBack) { allowed = false; text = "2 Fall Back: refused here"; }
                if (d == Directive.Eclipse)
                {
                    if (_fight.Assist == null) { allowed = false; text = "5 Assist: none yet"; }
                    else if (_fight.AssistUsed) { allowed = false; text = "5 " + _fight.Assist.Name + ": used"; }
                    else text = "5 Call " + _fight.Assist.Name + " [" + Directives.Cost(d) + "]";
                }
                GUI.enabled = allowed && !_pilotOn;
                if (GUI.Button(new Rect(bar.x + 8f + i * 168f, bar.y + 8f, 160f, 46f), text, _button)) Order(d);
            }
            GUI.enabled = true;
            if (_fight.Over)
            {
                if (GUI.Button(new Rect(bar.x + 860f, bar.y + 8f, 370f, 46f), "SEE RESULT", _button)) ShowResult();
                return;
            }
            if (GUI.Button(new Rect(bar.x + 860f, bar.y + 8f, 110f, 46f), _paused ? "RESUME" : "PAUSE", _button)) _paused = !_paused;
            if (GUI.Button(new Rect(bar.x + 980f, bar.y + 8f, 110f, 46f), "SPEED " + _speed.ToString("0", CultureInfo.InvariantCulture) + "x", _button))
                _speed = _speed >= 4f ? 1f : _speed * 2f;
            if (GUI.Button(new Rect(bar.x + 1100f, bar.y + 8f, 130f, 46f), _pilotOn ? "BOT PLAYS" : "YOU PLAY", _button)) _pilotOn = !_pilotOn;
        }

        void DrawResult()
        {
            EncounterResult r = _result;
            CampaignTribe t = r.Encounter.Tribe;
            GUI.Label(new Rect(20f, 14f, 1240f, 40f), Outcome(r).ToUpperInvariant() + "  /  " + r.Encounter.Title, _h1);
            Fill(new Rect(20f, 64f, 610f, 530f), Panel);
            var left = new StringBuilder();
            if (r.Encounter.Kind != EncounterKind.Tutorial)
            {
                left.Append("RESPECT FROM ").Append(t.Name.ToUpperInvariant()).Append(": ").Append(r.RespectBefore).Append(" -> ").Append(r.RespectAfter)
                    .Append(" of ").Append(r.RespectNeeded);
                if (!r.Won) left.Append("\nLosing ended the run and halved the bar (it was ").Append(r.RespectBeforeHalving).Append(").");
                left.Append("\n");
                if (r.Won) left.Append("\n  +").Append(RespectRules.WinPoints).Append("  the win");
                for (int i = 0; i < r.Awards.Count; i++)
                {
                    FeatAward a = r.Awards[i];
                    string who = a.TribeId == t.Id ? string.Empty : " (" + TribeName(a.TribeId) + ")";
                    if (a.Taboo && a.Points == 0)
                        left.Append("\n  0  ").Append(Feats.Name(a.Feat)).Append(who).Append(": they heard it, but had no respect to lose yet");
                    else
                        left.Append("\n  ").Append(a.Points > 0 ? "+" : string.Empty).Append(a.Points).Append("  ").Append(Feats.Name(a.Feat)).Append(who);
                }
                if (r.Encounter.EnemyCasts) left.Append("\n\nEclipse answers this fight: ").Append(r.EclipseWays).Append(" of 4 ways.");
                left.Append("\n\n");
            }
            left.Append("FALLEN: ");
            if (r.Fallen.Count == 0) left.Append("nobody");
            for (int i = 0; i < r.Fallen.Count; i++) left.Append(i > 0 ? ", " : string.Empty).Append(r.Fallen[i].DisplayName);
            if (r.JoinedFighters.Count > 0)
            {
                left.Append("\n\nJOINED YOU: ");
                for (int i = 0; i < r.JoinedFighters.Count; i++) left.Append(i > 0 ? ", " : string.Empty).Append(r.JoinedFighters[i].DisplayName);
            }
            if (r.AssistUnlocked != null)
                left.Append("\n\nASSIST UNLOCKED: ").Append(r.AssistUnlocked.Name).Append(" from ").Append(r.AssistUnlocked.ChiefName)
                    .Append(". Call it once a fight for ").Append(Directives.Cost(Directive.Eclipse)).Append(" order points.");
            if (r.ChiefReady) left.Append("\n\nTHE BAR IS FULL. ").Append(t.ChiefName).Append(" calls you out next.");
            if (r.SentAway) left.Append("\n\nTHE BAR DID NOT FILL IN TIME. ").Append(t.Name).Append(" sent you away. The run is over; your respect is kept.");
            if (r.RunOver && !r.SentAway) left.Append("\n\nTHE RUN IS OVER.");
            GUI.Label(new Rect(34f, 72f, 586f, 516f), left.ToString(), _body);

            Fill(new Rect(650f, 64f, 610f, 530f), Panel);
            var right = new StringBuilder(t.Name.ToUpperInvariant() + " LEARNED FROM THIS FIGHT\n");
            if (r.EnemyLessons.Count == 0) right.Append("\nNothing new.");
            for (int i = 0; i < r.EnemyLessons.Count; i++) right.Append("\n- ").Append(r.EnemyLessons[i].Why);
            right.Append("\n\nWHAT THEY KNOW NOW: ").Append(_campaign.Mind(t.Id).HabitsText());
            GUI.Label(new Rect(664f, 72f, 586f, 516f), right.ToString(), _small);
            if (GUI.Button(new Rect(1020f, 610f, 240f, 48f), "CONTINUE", _button)) AfterResult();
        }

        void DrawFinished()
        {
            GUI.Label(new Rect(20f, 14f, 1240f, 40f), "EVERY SLICE TRIBE HAS JOINED YOU", _h1);
            GUI.Label(new Rect(20f, 60f, 1240f, 50f),
                "Close the game and reopen it: CONTINUE loads this campaign. What each tribe learned about you is kept below.", _body);
            DrawForces(new Rect(20f, 116f, 400f, 480f));
            float x = 440f;
            for (int i = 0; i < CampaignTribes.All.Length; i++)
            {
                CampaignTribe t = CampaignTribes.All[i];
                if (t.IsStarter) continue;
                DrawTribePanel(new Rect(x, 116f, 405f, 480f), t, false);
                x += 415f;
            }
            if (GUI.Button(new Rect(20f, 610f, 240f, 48f), "TITLE", _button)) ShowTitle();
            if (GUI.Button(new Rect(280f, 610f, 240f, 48f), "CODEX", _button)) _showCodex = !_showCodex;
        }

        void DrawCodex()
        {
            var r = new Rect(240f, 80f, 800f, 540f);
            Fill(r, new Color(0.06f, 0.06f, 0.07f, 0.98f));
            GUI.Label(new Rect(r.x + 16f, r.y + 10f, r.width - 32f, 30f), "CODEX", _h1);
            _scrollRight = GUI.BeginScrollView(new Rect(r.x + 16f, r.y + 48f, r.width - 32f, r.height - 110f), _scrollRight,
                new Rect(0f, 0f, r.width - 52f, Math.Max(r.height - 110f, _campaign.Codex.Count * 24f)));
            for (int i = 0; i < _campaign.Codex.Count; i++) GUI.Label(new Rect(0f, i * 24f, r.width - 52f, 22f), _campaign.Codex[i], _body);
            GUI.EndScrollView();
            if (GUI.Button(new Rect(r.xMax - 176f, r.yMax - 52f, 160f, 40f), "CLOSE", _button)) _showCodex = false;
        }

        // ------------------------------------------------------------ fight text

        string Tracker(BattleSimulation sim, Encounter e)
        {
            CampaignTribe t = e.Tribe;
            IReadOnlyList<BattleEvent> ev = sim.Events;
            var b = new StringBuilder(e.Kind == EncounterKind.Tutorial ? "TUTORIAL: win and the Clarth join you." : t.Name.ToUpperInvariant() + " IS WATCHING FOR:");
            if (e.Kind == EncounterKind.Tutorial) return b.ToString();
            for (int i = 0; i < e.CanShow.Count; i++)
            {
                Feat f = e.CanShow[i];
                b.Append("\n  ").Append(Feats.Name(f)).Append(": ");
                switch (f)
                {
                    case Feat.AnsweredThreeWays: b.Append(Feats.EclipseWays(ev)).Append(" of ").Append(Feats.EclipseWaysNeeded).Append(" ways so far"); break;
                    case Feat.ScaredThem: b.Append(Feats.ScaresLanded(ev)).Append(" of ").Append(Feats.ScaresNeeded).Append(" unblocked Unleashes"); break;
                    case Feat.BrokeByFear: b.Append("their nerve must break with ").Append(Feats.BrokeByFearStanding).Append(" or more standing"); break;
                    case Feat.CleanWin: b.Append(Lost(sim) == 0 ? "nobody lost yet" : Lost(sim) + " lost"); break;
                    case Feat.QuickWin: b.Append("win before ").Append(Seconds(Feats.QuickWinTicks)); break;
                    case Feat.Outnumbered: b.Append("you are outnumbered, win anyway"); break;
                }
            }
            for (int i = 0; i < CampaignTribes.All.Length; i++)
            {
                CampaignTribe other = CampaignTribes.All[i];
                if (other.IsStarter || _campaign.HasJoined(other.Id)) continue;
                Directive hated = other.Hates == Feat.Hid ? Directive.Hold : Directive.Unleash;
                int acts = Feats.Ordered(ev, hated);
                if (acts == 0 && other.Id != t.Id) continue;
                bool nothingToLose = other.Id != t.Id && _campaign.Respect(other.Id) == 0;
                b.Append("\n  ").Append(other.Name).Append(" hate ").Append(OrderName(hated)).Append(": ").Append(acts).Append(" so far, ")
                    .Append(nothingToLose ? "no respect to lose yet" : "-" + RespectRules.TabooPoints + " each");
            }
            return b.ToString();
        }

        string States(BattleSimulation sim)
        {
            var b = new StringBuilder();
            if (sim.HoldTicksLeft(1) > 0) b.Append("Their shields are up for ").Append(Seconds(sim.HoldTicksLeft(1))).Append(": your Unleash will not scare them.\n");
            if (sim.UnleashTicksLeft(1) > 0) b.Append("They are unleashing for ").Append(Seconds(sim.UnleashTicksLeft(1))).Append(": Hold to keep your nerve.\n");
            if (sim.HoldTicksLeft(0) > 0) b.Append("Your Hold: ").Append(Seconds(sim.HoldTicksLeft(0))).Append(".\n");
            if (sim.UnleashTicksLeft(0) > 0) b.Append("Your Unleash: ").Append(Seconds(sim.UnleashTicksLeft(0))).Append(".\n");
            if (sim.WindupTicks(0) > 0) b.Append("Your assist is winding up: ").Append(Seconds(sim.WindupTicks(0))).Append(".\n");
            return b.ToString();
        }

        void DrainEvents()
        {
            BattleSimulation sim = _fight.Sim;
            IReadOnlyList<BattleEvent> ev = sim.Events;
            for (; _eventCursor < ev.Count; _eventCursor++)
            {
                BattleEvent e = ev[_eventCursor];
                string time = Seconds(e.Tick) + "  ";
                switch (e.Kind)
                {
                    case BattleEventKind.Directive:
                        if (e.Detail == BattleSimulation.BlockedDetail) Feed(time + "Their shields blocked your Unleash: no scare.");
                        else if (e.ActorSide == 0 && e.ActorId == "side0") Feed(time + "You: " + Pretty(e.Detail) + (e.TargetId.Length > 0 ? " on " + NameOf(e.TargetId) : string.Empty) + ".");
                        else if (e.ActorSide == 1 && Directives.TryParse(e.Detail, out _)) Feed(time + "They: " + Pretty(e.Detail) + ".");
                        break;
                    case BattleEventKind.Windup:
                        Feed(time + (e.ActorSide == 0 ? "Your assist winds up " : NameOf(e.ActorId) + " winds up ") + e.Detail + ".");
                        break;
                    case BattleEventKind.Release:
                        Feed(time + (e.ActorSide == 1
                            ? (e.Detail == Moves.Dodged ? "You dodged their Eclipse." : e.Detail == Moves.Soaked ? "Your Hold halved their Eclipse." : "Their Eclipse hit your front line.")
                            : (e.Detail == Moves.Dodged ? "They dodged your Eclipse." : e.Detail == Moves.Soaked ? "Their Hold halved your Eclipse." : "Your Eclipse hit their front line.")));
                        break;
                    case BattleEventKind.Interrupt:
                        Feed(time + (e.ActorSide == 0 ? "You broke " + NameOf(e.TargetId) + "'s cast. Your next order is doubled." : "They broke your cast."));
                        break;
                    case BattleEventKind.Kill:
                        Feed(time + NameOf(e.TargetId) + " is down.");
                        break;
                    case BattleEventKind.Break:
                        Feed(time + (e.ActorSide == 0 ? "YOUR NERVE BROKE." : "THEIR NERVE BROKE with " + e.Amount + " standing."));
                        break;
                    case BattleEventKind.End:
                        Feed(time + "Fight over (" + e.Detail + ").");
                        break;
                }
            }
        }

        string NameOf(string unitId)
        {
            BattleUnit u = _fight == null ? null : _fight.Sim.FindById(unitId);
            return u == null ? unitId : UnitName(u);
        }

        string UnitName(BattleUnit u)
        {
            if (u.Side == 0)
            {
                if (u.Id == Campaign.LeaderId) return "Your leader (" + SliceClasses.Name(u.Class) + ")";
                for (int i = 0; i < _fight.Squad.Count; i++) if (_fight.Squad[i].Id == u.Id) return _fight.Squad[i].DisplayName;
                return SliceClasses.Name(u.Class);
            }
            string tribe = _fight.Encounter.Tribe.Name;
            if (u.Id.EndsWith(".chief", StringComparison.Ordinal)) return tribe + " chief";
            if (u.Id.EndsWith(".shaman", StringComparison.Ordinal)) return tribe + " shaman";
            if (u.Id.EndsWith(".champion", StringComparison.Ordinal)) return tribe + " champion";
            return tribe + " " + SliceClasses.Name(u.Class);
        }

        int Lost(BattleSimulation sim)
        {
            int lost = 0;
            for (int i = 0; i < sim.Units.Count; i++) if (sim.Units[i].Side == 0 && !sim.Units[i].Alive) lost++;
            return lost;
        }

        /// No respect is at stake in the tutorial, so no taboo warning there.
        string TabooNote(Directive d)
        {
            if (d != Directive.Hold && d != Directive.Unleash) return string.Empty;
            if (_fight != null && _fight.Encounter.Kind == EncounterKind.Tutorial) return string.Empty;
            Feat feat = d == Directive.Hold ? Feat.Hid : Feat.Loud;
            for (int i = 0; i < CampaignTribes.All.Length; i++)
            {
                CampaignTribe t = CampaignTribes.All[i];
                if (!t.IsStarter && t.Hates == feat && !_campaign.HasJoined(t.Id)) return "\n" + t.Name + " hate it";
            }
            return string.Empty;
        }

        // ------------------------------------------------------------ helpers

        void Feed(string line)
        {
            _feed.Add(line);
            if (_feed.Count > 40) _feed.RemoveAt(0);
        }

        void Log(string line)
        {
            _log.Append(line).Append('\n');
            Debug.Log("[CampaignDebug] " + line);
        }

        static string Summary(Campaign c)
        {
            var b = new StringBuilder("Seed " + c.Seed.ToString(CultureInfo.InvariantCulture) + ". Tribes with you: ");
            for (int i = 0; i < c.Joined.Count; i++) b.Append(i > 0 ? ", " : string.Empty).Append(TribeName(c.Joined[i]));
            if (c.Joined.Count == 0) b.Append("none (the tutorial is next)");
            b.Append(". Runs started ").Append(c.RunsStarted).Append(", won ").Append(c.RunsWon).Append(".");
            for (int i = 0; i < CampaignTribes.All.Length; i++)
            {
                CampaignTribe t = CampaignTribes.All[i];
                if (t.IsStarter || c.HasJoined(t.Id)) continue;
                b.Append(" ").Append(t.Name).Append(" respect ").Append(c.Respect(t.Id)).Append(" of ").Append(c.RespectNeeded(t.Id))
                    .Append(", ").Append(c.Mind(t.Id).Lessons.Count).Append(" lessons about you.");
            }
            return b.ToString();
        }

        static string TribeName(string tribeId)
        {
            CampaignTribe t = CampaignTribes.Find(tribeId);
            return t == null ? tribeId : t.Name;
        }

        static string Outcome(EncounterResult r)
        {
            if (r.Won && r.Encounter.Kind == EncounterKind.HoldTheLine && r.TimedOut) return "You held the line";
            if (r.Won) return "Won";
            return r.TimedOut ? "Lost: time ran out" : "Lost";
        }

        static string OrderName(Directive d)
        {
            switch (d)
            {
                case Directive.Hold: return "Hold";
                case Directive.FallBack: return "Fall Back";
                case Directive.FocusFire: return "Focus Fire";
                case Directive.Unleash: return "Unleash";
                case Directive.Eclipse: return "Eclipse";
                default: return d.ToString();
            }
        }

        static string Pretty(string directiveName)
        {
            for (int i = 0; i < Orders.Length; i++) if (Directives.Name(Orders[i]) == directiveName) return OrderName(Orders[i]);
            return directiveName;
        }

        static bool Contains(IReadOnlyList<Feat> feats, Feat feat)
        {
            for (int i = 0; i < feats.Count; i++) if (feats[i] == feat) return true;
            return false;
        }

        static string Seconds(long ticks)
        {
            return (ticks / (float)SimClock.TicksPerSecond).ToString("0.0", CultureInfo.InvariantCulture) + "s";
        }

        void RespectBar(Rect r, int value, int needed)
        {
            Fill(r, Trough);
            Fill(new Rect(r.x, r.y, r.width * Mathf.Clamp01(needed <= 0 ? 0f : value / (float)needed), r.height), Respect);
            GUI.Label(new Rect(r.x + 6f, r.y + 1f, r.width - 12f, r.height), "RESPECT " + value + " of " + needed +
                (value >= needed ? "  (the chief calls you out)" : string.Empty), _small);
        }

        void NerveBar(Rect r, string label, int nerve, Color color)
        {
            Fill(r, Trough);
            Fill(new Rect(r.x, r.y, r.width * Mathf.Clamp01(nerve / (float)NerveRules.Full), r.height), color);
            GUI.Label(new Rect(r.x + 6f, r.y, r.width - 12f, r.height), label + "  " + (nerve / 10).ToString(CultureInfo.InvariantCulture) + "%", _small);
        }

        static void Fill(Rect r, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = old;
        }

        void EnsureStyles()
        {
            if (_h1 != null) return;
            _h1 = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, wordWrap = true };
            _h2 = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, wordWrap = true };
            _body = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
            _small = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            _tag = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = false, clipping = TextClipping.Clip };
            _warn = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, wordWrap = true };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 14, wordWrap = true };
            foreach (GUIStyle s in new[] { _h1, _h2, _body, _small, _tag }) s.normal.textColor = Ink;
            _warn.normal.textColor = Warning;
        }

        // ------------------------------------------------------------ autoplay for screenshots

        void AutoStep()
        {
            if (_pageClock < 0.6f) return;
            string key = PageKey();
            if (key.Length > 0 && !_shotsTaken.Contains(key))
            {
                Shot(key);
                return;
            }
            if (_pageClock < 1.2f) return;
            switch (_page)
            {
                case Page.Title:
                    if (_reopenDone && _campaign == null) ContinueSaved();
                    else if (!_reopenDone) NewCampaign();
                    else FinishAuto("PASS");
                    break;
                case Page.Tutorial:
                    StartTutorialFight();
                    break;
                case Page.ChooseTribe:
                    if (_reopenDone) { FinishAuto("PASS"); break; }
                    if (_autoRuns >= 4) { Reopen(); break; }
                    _autoRuns++;
                    GoAfter(_campaign.HasJoined(CampaignTribes.BloodridgeId) ? CampaignTribes.SkullpeakId : CampaignTribes.BloodridgeId);
                    break;
                case Page.Offers:
                    TakeOffer(new CampaignPilot(PilotStyle.Reference, _campaign.Seed).PickOffer(_campaign));
                    break;
                case Page.Squad:
                    StartEncounterFight();
                    break;
                case Page.Fight:
                    if (_fight != null && _fight.Over) ShowResult();
                    break;
                case Page.Result:
                    AfterResult();
                    break;
                case Page.Finished:
                    if (_reopenDone) FinishAuto("PASS");
                    else Reopen();
                    break;
            }
        }

        /// Which screens get a picture: each kind once, plus a mid fight, a windup and the chief.
        string PageKey()
        {
            switch (_page)
            {
                case Page.Title: return _reopenDone ? "reopen_title" : "title";
                case Page.Tutorial: return "tutorial";
                case Page.ChooseTribe: return _reopenDone ? "reopen_choose_tribe" : _campaign.RunsStarted == 0 ? "choose_tribe" : "choose_tribe_later";
                case Page.Offers: return _campaign.Offers.Count == 1 ? "offer_chief" : "offers";
                case Page.Squad: return "squad";
                case Page.Fight:
                    if (_fight == null) return string.Empty;
                    if (!_windupShot && _fight.Sim.WindupTicks(1) > 0 && _fight.Sim.WindupTicks(1) < 75)
                    {
                        _windupShot = true;
                        return "fight_windup";
                    }
                    if (_fight.Sim.CurrentTick < 240) return string.Empty;
                    return _fight.Encounter.Kind == EncounterKind.Chief ? "fight_chief" : _fight.Encounter.Kind == EncounterKind.Tutorial ? "fight_tutorial" : "fight";
                case Page.Result:
                    if (_result.Joined && _result.Encounter.Kind == EncounterKind.Chief) return "result_chief_joined";
                    if (!_result.Won) return "result_lost";
                    return _result.Encounter.Kind == EncounterKind.Tutorial ? "result_tutorial" : "result";
                case Page.Finished: return _reopenDone ? "reopen_finished" : "finished";
                default: return string.Empty;
            }
        }

        void Shot(string key)
        {
            _shotsTaken.Add(key);
            _shotCount++;
            string file = Path.Combine(_shots, _shotCount.ToString("D2", CultureInfo.InvariantCulture) + "_" + key + ".png");
            ScreenCapture.CaptureScreenshot(file);
            Log("Screenshot " + file);
        }

        void Reopen()
        {
            Save();
            _reopenDone = true;
            _campaign = null;
            Log("Closing the campaign and reopening it from " + _savePath);
            ShowTitle();
        }

        void FinishAuto(string verdict)
        {
            Log(verdict);
            File.WriteAllText(Path.Combine(_shots, "result.txt"), verdict + "\n" + _log);
            _auto = false;
            Application.Quit();
        }
    }
}
