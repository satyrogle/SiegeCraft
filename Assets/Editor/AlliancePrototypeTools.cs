using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using KindredSiege.Battle;

namespace KindredSiege.EditorTools
{
    [InitializeOnLoad]
    public static class AlliancePrototypeTools
    {
        public const string ScenePath = "Assets/Scenes/Prototypes/AlliancePrototype.unity";
        const string BatchKey = "KS.Alliance.Batch";
        // A fixed random seed and a fixed frame time make a validation run repeatable instead of a fresh dice roll.
        const int Seed = 20260927;
        static double _started;
        static int _step;
        static int _assertions;
        static float _pausedElapsed;
        static int _pausedHealth;
        static double _pauseStarted;
        static readonly List<string> Results = new List<string>();
        static readonly List<string> Failures = new List<string>();
        static string Output => Path.GetFullPath("Logs/AlliancePrototypeValidation");
        static AlliancePrototype _prototype;

        static AlliancePrototypeTools()
        {
            EditorApplication.playModeStateChanged += PlayState;
            if (SessionState.GetBool(BatchKey, false)) EditorApplication.update += Tick;
        }

        [MenuItem("KindredSiege/Prototypes/Open First Alliance")]
        public static void OpenPrototype()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Exit Play Mode before opening the prototype."); return; }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
            if (!AssetDatabase.IsValidFolder("Assets/Scenes/Prototypes")) AssetDatabase.CreateFolder("Assets/Scenes", "Prototypes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("First Alliance prototype", typeof(AlliancePrototype));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        // Uses the existing editor and real UnitController ticks. Does not touch Assets/Tests or campaign saves.
        public static void ValidateBatch()
        {
            Directory.CreateDirectory(Output);
            // Clear the previous verdict first, so an early failure can never leave an old PASS behind.
            File.WriteAllText(Path.Combine(Output, "result.txt"), "RUNNING\n");
            OpenPrototype();
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
            {
                File.WriteAllText(Path.Combine(Output, "result.txt"), "FAIL: the prototype scene did not open\n");
                EditorApplication.Exit(1);
                return;
            }
            try { VerifyMemoryRules(); }
            catch (Exception e)
            {
                File.WriteAllText(Path.Combine(Output, "result.txt"), "FAIL in memory rules: " + e.Message + "\nAssertions=" + _assertions + "\n");
                EditorApplication.Exit(1);
                return;
            }
            SessionState.SetBool(BatchKey, true);
            EditorApplication.EnterPlaymode();
        }

        static void PlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(BatchKey, false))
            {
                UnityEngine.Random.InitState(Seed);
                Time.captureDeltaTime = 1f / 60f;
                Captured.Clear();
                _started = EditorApplication.timeSinceStartup;
                _step = 0;
                _assertions = SessionState.GetInt("KS.Alliance.RuleCount", 0);
                Results.Clear();
                Failures.Clear();
                EditorApplication.update -= Tick;
                EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(BatchKey, false))
            {
                Time.captureDeltaTime = 0f;
                SessionState.SetBool(BatchKey, false);
                EditorApplication.Exit(SessionState.GetInt("KS.Alliance.ExitCode", 1));
            }
        }

        static void VerifyMemoryRules()
        {
            _assertions = 0;
            var memory = new AllianceMemory();
            Check(!memory.WillCover("Ash guard 1"), "Unproven guard does not cover.");
            Check(memory.Record(1, 10, "Ash guard 1", AllianceFact.Rescued), "Rescue is recorded.");
            Check(!memory.Record(1, 11, "Ash guard 1", AllianceFact.Rescued), "Duplicate rescue is ignored.");
            Check(memory.WillCover("Ash guard 1"), "Rescue changes behaviour.");
            Check(!memory.WillCover("Ash guard 2"), "Unwitnessed rescue does not affect another guard.");
            memory.Record(2, 10, "Ash guard 1", AllianceFact.Abandoned);
            Check(!memory.WillCover("Ash guard 1"), "Abandonment can undo trust.");
            memory.Record(2, 12, "Ash guard 1", AllianceFact.Covered);
            Check(memory.WillCover("Ash guard 1"), "Real cover can repair trust.");
            memory.Record(2, 14, "Ash guard 1", AllianceFact.Fell);
            Check(!memory.WillCover("Ash guard 1"), "Fallen units offer no cover.");
            string path = Path.Combine(Output, "invalid-checkpoint.json");
            File.WriteAllText(path, "{\"Version\":99}");
            Check(!AllianceCheckpoint.Load(out _, out _, path), "Unknown save version rejected.");
            Check(File.ReadAllText(path) == "{\"Version\":99}", "Rejected save preserved.");
            File.WriteAllText(path, "{broken");
            Check(!AllianceCheckpoint.Load(out _, out _, path), "Corrupt checkpoint rejected.");
            Check(!AllianceCheckpoint.Save(memory, out _, path), "Invalid replacement rejected.");
            Check(File.ReadAllText(path) == "{broken", "Invalid save does not overwrite existing data.");

            // Checkpoints the first fight can never produce are refused, and real ones still load.
            Check(RescueCheckpoint().IsValid(), "A normal rescue checkpoint is accepted.");
            Check(SalvageCheckpoint().IsValid(), "A normal salvage checkpoint is accepted.");
            var lostButStanding = SalvageCheckpoint();
            lostButStanding.Survivors[1].Sanity = 0;
            Check(!lostButStanding.IsValid(), "A standing survivor at zero sanity is refused.");
            var mixed = TimeoutCheckpoint();
            mixed.Events.RemoveAll(e => e.Subject == "Ash guard 1");
            mixed.Record(1, 22, "Ash guard 1", AllianceFact.Rescued);
            Check(!mixed.IsValid(), "One guard rescued while the other was left behind is refused.");
            var guardRescuer = RescueCheckpoint();
            guardRescuer.Events[0].Other = "Ash guard 2";
            Check(!guardRescuer.IsValid(), "A guard credited as the rescuer is refused.");

            // Recovery: a missing checkpoint falls back to its backup, and a damaged one never replaces it.
            string backup = Path.Combine(Output, "backup-checkpoint.json");
            foreach (var file in new[] { backup, backup + ".bak", backup + ".tmp", backup + ".rejected" })
                if (File.Exists(file)) File.Delete(file);
            var first = RescueCheckpoint();
            first.Survivors[0].Health = 111;
            Check(AllianceCheckpoint.Save(first, out _, backup) && AllianceCheckpoint.Save(RescueCheckpoint(), out _, backup),
                "Two saves keep the first as the backup.");
            File.Delete(backup);
            Check(AllianceCheckpoint.Load(out var fromBackup, out _, backup) && fromBackup.Survivors[0].Health == 111,
                "A missing checkpoint falls back to its backup.");
            Check(AllianceCheckpoint.Save(RescueCheckpoint(), out _, backup), "A new save works while only the backup exists.");
            File.WriteAllText(backup, "{broken");
            Check(AllianceCheckpoint.Save(RescueCheckpoint(), out _, backup) &&
                AllianceCheckpoint.Load(out var kept, out _, backup + ".bak") && kept.Survivors[0].Health == 111,
                "A damaged checkpoint never replaces the good backup.");
            SessionState.SetInt("KS.Alliance.RuleCount", _assertions);
        }

        static AllianceMemory RescueCheckpoint()
        {
            var memory = new AllianceMemory { FirstFightComplete = true };
            memory.Record(1, 9, "Ash guard 1", AllianceFact.Rescued);
            memory.Record(1, 9, "Ash guard 2", AllianceFact.Rescued);
            AddSurvivors(memory, 211, 130, 117, 180);
            return memory;
        }

        static AllianceMemory SalvageCheckpoint()
        {
            var memory = new AllianceMemory { FirstFightComplete = true, HasSalvage = true };
            memory.Record(1, 7, "Commander", AllianceFact.Salvaged);
            memory.Record(1, 7, "Ash guard 1", AllianceFact.Abandoned);
            memory.Record(1, 7, "Ash guard 2", AllianceFact.Abandoned);
            AddSurvivors(memory, 220, 130, 126, 180);
            return memory;
        }

        static AllianceMemory TimeoutCheckpoint()
        {
            var memory = new AllianceMemory { FirstFightComplete = true };
            memory.Record(1, 22, "Ash guard 1", AllianceFact.Abandoned);
            memory.Record(1, 22, "Ash guard 2", AllianceFact.Abandoned);
            AddSurvivors(memory, 220, 126, 36, 171);
            return memory;
        }

        static void AddSurvivors(AllianceMemory memory, int commander, int scout, int guard1, int guard2)
        {
            memory.Survivors.Add(new AllianceSurvivor { Id = "Commander", Health = commander, Sanity = 95 });
            memory.Survivors.Add(new AllianceSurvivor { Id = "Ember scout", Health = scout, Sanity = 100 });
            memory.Survivors.Add(new AllianceSurvivor { Id = "Ash guard 1", Health = guard1, Sanity = 70 });
            memory.Survivors.Add(new AllianceSurvivor { Id = "Ash guard 2", Health = guard2, Sanity = 100 });
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            try
            {
                // Game time is locked to 1/60 s per frame, so a slow hidden editor needs more real time than game time.
                if (EditorApplication.timeSinceStartup - _started > 400) throw new Exception("Runtime validation timed out at step " + _step);
                if (_prototype == null) _prototype = UnityEngine.Object.FindObjectOfType<AlliancePrototype>();
                if (_prototype == null || _prototype.ViewCamera == null) return;
                _prototype.PersistenceEnabled = false;
                switch (_step)
                {
                    case 0:
                        Check(_prototype.CurrentStage == AlliancePrototype.Stage.Briefing, "Isolated prototype opens at briefing.");
                        Check(KindredSiege.Core.GameManager.Instance == null, "Campaign manager absent.");
                        Capture("01-briefing.png");
                        _pauseStarted = EditorApplication.timeSinceStartup;
                        _step++; break;
                    case 1:
                        if (EditorApplication.timeSinceStartup - _pauseStarted < 1) return;
                        Click(170, 720);
                        _pauseStarted = EditorApplication.timeSinceStartup;
                        _step++; break;
                    case 2:
                        if (_prototype.CurrentStage == AlliancePrototype.Stage.Briefing)
                        {
                            if (EditorApplication.timeSinceStartup - _pauseStarted > 3) throw new Exception("Begin button did not receive the queued Game view click.");
                            return;
                        }
                        Check(_prototype.CurrentStage == AlliancePrototype.Stage.FirstFight, "Begin GUI button starts combat.");
                        Click(170, 665);
                        _step++; break;
                    case 3:
                        if (_prototype.CurrentOrder != AlliancePrototype.Order.Rescue) return;
                        Check(true, "Rescue GUI button issues a movement order.");
                        _prototype.TogglePause();
                        _pausedElapsed = _prototype.Elapsed;
                        _pausedHealth = SumHealth();
                        _pauseStarted = EditorApplication.timeSinceStartup;
                        _step++; break;
                    case 4:
                        if (EditorApplication.timeSinceStartup - _pauseStarted < .6) return;
                        Check(Mathf.Approximately(_prototype.Elapsed, _pausedElapsed), "Pause freezes encounter time.");
                        Check(SumHealth() == _pausedHealth, "Pause freezes combat damage.");
                        Check(!_prototype.IssueOrder(AlliancePrototype.Order.Salvage), "Paused orders rejected.");
                        _prototype.TogglePause();
                        _step++; break;
                    case 5:
                        if (_prototype.Elapsed > 5 && _prototype.CurrentStage == AlliancePrototype.Stage.FirstFight) CaptureOnce("02-rescue-fight.png");
                        if (_prototype.CurrentStage != AlliancePrototype.Stage.Debrief) return;
                        Check(_prototype.Memory.WillCover("Ash guard 1"), "Actual rescue grants guard one cover behaviour.");
                        Check(_prototype.Memory.WillCover("Ash guard 2"), "Actual rescue grants guard two cover behaviour.");
                        Check(!_prototype.Memory.HasSalvage, "Rescue forfeits salvage.");
                        string checkpoint = Path.Combine(Output, "rescue-checkpoint.json");
                        Check(AllianceCheckpoint.Save(_prototype.Memory, out _, checkpoint), "First-fight checkpoint saved to isolated test path.");
                        Check(AllianceCheckpoint.Load(out var restored, out _, checkpoint) && restored.WillCover("Ash guard 1"), "Checkpoint reload retains behavioural cause.");
                        Capture("03-rescue-memories.png");
                        _pauseStarted = EditorApplication.timeSinceStartup;
                        _step++; break;
                    case 6:
                        if (EditorApplication.timeSinceStartup - _pauseStarted < 1) return;
                        _prototype.Restart();
                        Check(_prototype.LoadCheckpoint(Path.Combine(Output, "rescue-checkpoint.json")), "Encounter reloads from disk after in-memory reset.");
                        foreach (var unit in _prototype.Fighters)
                        {
                            var saved = _prototype.Memory.Survivors.Find(s => s.Id == unit.UnitName);
                            Check(unit.CurrentHP == saved.Health && unit.CurrentSanity == saved.Sanity, "Loaded debrief matches saved health/sanity: " + unit.UnitName);
                        }
                        Capture("09-loaded-checkpoint.png");
                        _pauseStarted = EditorApplication.timeSinceStartup;
                        _step = 18; break;
                    case 18:
                        if (EditorApplication.timeSinceStartup - _pauseStarted < 1) return;
                        _prototype.BeginSecondFight();
                        foreach (var unit in _prototype.Fighters)
                        {
                            if (unit.TeamId != 1) continue;
                            var saved = _prototype.Memory.Survivors.Find(s => s.Id == unit.UnitName);
                            Check(unit.CurrentHP == Mathf.Min(unit.MaxHP, saved.Health + 20) && unit.CurrentSanity == Mathf.Min(100, saved.Sanity + 10),
                                "Patch-up is applied once: " + unit.UnitName);
                        }
                        _step = 7; break;
                    case 7:
                        if (_prototype.Elapsed > 7) CaptureOnce("04-earned-cover.png");
                        if (_prototype.CurrentStage != AlliancePrototype.Stage.Complete && _prototype.CurrentStage != AlliancePrototype.Stage.Defeat) return;
                        Check(_prototype.CoverCount > 0, "Rescued guard physically intercepts a real pulse.");
                        Expect(_prototype.CurrentStage == AlliancePrototype.Stage.Complete, "Rescue route can win the follow-up fight.");
                        Results.Add("RESCUE: " + _prototype.Status + " Covers=" + _prototype.CoverCount + " Time=" + _prototype.Elapsed);
                        Capture("05-rescue-result.png");
                        _pauseStarted = EditorApplication.timeSinceStartup;
                        _step++; break;
                    case 8:
                        if (EditorApplication.timeSinceStartup - _pauseStarted < 1) return;
                        _prototype.Restart();
                        Check(_prototype.Memory.Events.Count == 0, "Restart clears in-memory history.");
                        _prototype.BeginFirstFight();
                        _prototype.IssueOrder(AlliancePrototype.Order.Salvage);
                        _step++; break;
                    case 9:
                        if (_prototype.CurrentStage != AlliancePrototype.Stage.Debrief) return;
                        Check(_prototype.Memory.HasSalvage, "Salvage requires actual travel and extraction.");
                        Check(!_prototype.Memory.WillCover("Ash guard 1"), "Abandoned survivor distrust persists.");
                        Check(_prototype.Memory.Events.Exists(e => e.Fact == AllianceFact.Abandoned), "Abandonment is explained in the ledger.");
                        Capture("06-salvage-memories.png");
                        _pauseStarted = EditorApplication.timeSinceStartup;
                        _step++; break;
                    case 10:
                        if (EditorApplication.timeSinceStartup - _pauseStarted < 1) return;
                        _prototype.BeginSecondFight();
                        Check(_prototype.Charges == 2, "Salvage grants exactly two charges.");
                        Check(!_prototype.DisruptPulse(), "Disruptor cannot be wasted outside a pulse.");
                        _step++; break;
                    case 11:
                        if (!_prototype.PulsePending) return;
                        Check(_prototype.CoverCount == 0, "Wary guards have not granted unearned cover.");
                        Check(_prototype.DisruptPulse() && _prototype.Charges == 1, "Disruptor cancels a real pulse and spends one charge.");
                        _prototype.IssueOrder(AlliancePrototype.Order.CoverGuards);
                        _step++; break;
                    case 12:
                        if (_prototype.Memory.Events.Exists(e => e.Fight == 2 && e.Fact == AllianceFact.Covered))
                        {
                            Check(true, "Taking a guard's pulse repairs distrust through actual combat.");
                            _prototype.IssueOrder(AlliancePrototype.Order.Advance);
                            _step++;
                        }
                        else if (_prototype.CurrentStage == AlliancePrototype.Stage.Defeat || _prototype.CurrentStage == AlliancePrototype.Stage.Complete)
                            throw new Exception("Second fight ended before distrust could be repaired: " + _prototype.Status);
                        break;
                    case 13:
                        if (_prototype.PulsePending && _prototype.Charges > 0) _prototype.DisruptPulse();
                        if (_prototype.CurrentStage != AlliancePrototype.Stage.Complete && _prototype.CurrentStage != AlliancePrototype.Stage.Defeat) return;
                        Expect(_prototype.CurrentStage == AlliancePrototype.Stage.Complete, "Salvage plus repaired trust can win the follow-up fight.");
                        Results.Add("SALVAGE: " + _prototype.Status + " Covers=" + _prototype.CoverCount + " Time=" + _prototype.Elapsed);
                        Capture("07-salvage-result.png");
                        _pauseStarted = EditorApplication.timeSinceStartup; _step++; break;
                    case 14:
                        if (EditorApplication.timeSinceStartup - _pauseStarted < 1) return;
                        _prototype.Restart(); _prototype.BeginFirstFight();
                        foreach (var unit in _prototype.Fighters) if (unit.UnitName == "Commander") unit.TakeDamage(10000, null);
                        _step++; break;
                    case 15:
                        Check(_prototype.CurrentStage == AlliancePrototype.Stage.Defeat, "Commander death produces defeat, not victory.");
                        Check(!_prototype.IssueOrder(AlliancePrototype.Order.Rescue), "Orders rejected after defeat.");
                        Capture("08-defeat.png");
                        _pauseStarted = EditorApplication.timeSinceStartup; _step++; break;
                    case 16:
                        if (EditorApplication.timeSinceStartup - _pauseStarted < 1) return;
                        _prototype.Restart(); _prototype.BeginFirstFight();
                        foreach (var unit in _prototype.Fighters) if (unit.UnitName == "Ash guard 2") unit.TakeDamage(10000, null);
                        _prototype.IssueOrder(AlliancePrototype.Order.Rescue);
                        _step++; break;
                    case 17:
                        if (_prototype.CurrentStage != AlliancePrototype.Stage.Debrief) return;
                        Check(!_prototype.Memory.WillCover("Ash guard 2"), "A fallen guard receives no rescue credit.");
                        _prototype.BeginSecondFight();
                        foreach (var unit in _prototype.Fighters)
                            if (unit.UnitName == "Ash guard 2") Check(!unit.IsAlive && unit.CurrentHP == 0, "Fallen guard stays fallen in fight two.");
                        Check(! _prototype.IssueOrder(AlliancePrototype.Order.Salvage), "First-fight commands are rejected in fight two.");
                        Complete(Failures.Count == 0 ? 0 : 1, Failures.Count == 0 ? "PASS" : "FAIL: " + string.Join(" | ", Failures)); break;
                }
            }
            catch (Exception e) { Complete(1, "FAIL at step " + _step + ": " + e); }
        }

        static void Check(bool condition, string label)
        {
            if (!condition) throw new Exception(label);
            _assertions++;
            Debug.Log("[AllianceCheck] PASS " + label);
        }

        // An outcome check on a whole fight: a failure is recorded and the run carries on, so one lost fight
        // cannot hide the checks after it. The run still ends as FAIL.
        static void Expect(bool condition, string label)
        {
            if (condition) { _assertions++; Debug.Log("[AllianceCheck] PASS " + label); }
            else { Failures.Add(label); Debug.Log("[AllianceCheck] FAIL " + label); }
        }
        static int SumHealth()
        {
            int total = 0;
            foreach (var unit in _prototype.Fighters) total += unit.CurrentHP;
            return total;
        }
        static void Click(float x, float y)
        {
            var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var window = EditorWindow.GetWindow(type);
            window.Focus();
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 800f);
            var position = new Vector2((Screen.width - 1280 * scale) / 2 + x * scale, (Screen.height - 800 * scale) / 2 + y * scale);
            var method = typeof(EditorGUIUtility).GetMethod("QueueGameViewInputEvent", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null) throw new Exception("Game view input bridge unavailable.");
            foreach (var eventType in new[] { EventType.MouseDown, EventType.MouseUp })
                method.Invoke(null, new object[] { new Event { type = eventType, mousePosition = position, button = 0 } });
        }
        static readonly HashSet<string> Captured = new HashSet<string>();
        static void CaptureOnce(string name) { if (Captured.Add(name)) Capture(name); }
        static void Capture(string name) => ScreenCapture.CaptureScreenshot(Path.Combine(Output, name));
        static void Complete(int exitCode, string message)
        {
            Debug.Log("[AllianceValidation] " + message + " assertions=" + _assertions);
            File.WriteAllText(Path.Combine(Output, "result.txt"), message + "\nAssertions=" + _assertions + "\nFailed=" + Failures.Count +
                "\nSeed=" + Seed + "\n" + string.Join("\n", Results));
            SessionState.SetInt("KS.Alliance.ExitCode", exitCode);
            EditorApplication.update -= Tick;
            EditorApplication.ExitPlaymode();
        }
    }
}
