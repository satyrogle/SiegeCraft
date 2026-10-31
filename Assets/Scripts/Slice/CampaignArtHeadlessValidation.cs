using System.Collections;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        EncounterResult _qaLastResult;
        // Four separate player processes share only this isolated save directory.
        // Fixed default-rules scenarios: no tuning or search for favourable outcomes.
        IEnumerator ValidateHeadlessCampaign()
        {
            string phase = Argument("-ksHeadlessValidate");
            yield return null;
            if (phase == "active")
            {
                Check(Click("new"), "new campaign through title"); yield return null;
                Check(Click("start-tutorial"), "tutorial starts through control"); yield return null;
                _pilotOn = false; _paused = true; RefreshFight();
                Check(Click("order-0"), "Hold accepted through control while paused");
                Check(Click("pause"), "resume control");
                for (int i = 0; i < 8; i++) yield return null;
                Check(Click("pause"), "pause control"); long tick = _fight.Sim.CurrentTick;
                for (int i = 0; i < 4; i++) yield return null;
                Check(tick > 0 && tick == _fight.Sim.CurrentTick && !_fight.Over, "active fight pauses at a nonterminal tick");
                Check(Click("speed"), "speed control");
                QaCheckpoint("active");
                Capture("active-before-exit");
                yield break;
            }

            string checkpoint = phase == "road" ? "active" : phase == "run" ? "road" : "camp";
            string expected = File.ReadAllText(Path.Combine(_capture, checkpoint + "-expected.txt"));
            Check(Click("continue"), checkpoint + " Continue in a fresh process"); yield return null;
            Check(CampaignSave.Write(_campaign) == expected, checkpoint + " process restart preserves complete campaign");
            Check(string.IsNullOrEmpty(_campaign.LoadNote), checkpoint + " loads without recovery or rollback");
            if (phase == "road")
            {
                Check(_page == Page.Fight && _paused && _fight != null, "active fight resumes paused on fight screen");
                var tutorial = QaFinishFight(21); while (tutorial.MoveNext()) yield return tutorial.Current;
                Check(_result.TutorialWon, "tutorial unlocks camp");
                Check(Click("after-result"), "tutorial receipt continues"); yield return null;
                Check(_page == Page.ChooseTribe, "camp opens after tutorial");
                Check(Click("tribe-" + _campaign.Choosable()[0].Id), "run starts through camp"); yield return null;
                var first = QaEncounter(21, 0); while (first.MoveNext()) yield return first.Current;
                Check(_campaign.Stage == CampaignStage.InRun && _page == Page.Offers, "first victory returns to road");
                QaCheckpoint("road"); Capture("road-before-exit");
                yield break;
            }
            if (phase == "run")
            {
                Check(_page == Page.Offers, "road reopens without restarting the run");
                for (int n = 1; n < 24 && _campaign.Stage == CampaignStage.InRun; n++)
                {
                    var next = QaEncounter(21, n); while (next.MoveNext()) yield return next.Current;
                }
                Check(_campaign.Stage == CampaignStage.ChooseTribe && _campaign.RunsStarted == 1,
                    "fixed seed 21 completes its whole run at camp");
                Log("WHOLE RUN seed21 won="+_campaign.RunsWon);
                QaCheckpoint("camp"); Capture("camp-before-exit");
                yield break;
            }
            Check(phase == "verify", "recognised final validation phase");
            Check(_page == Page.ChooseTribe, "camp reopens in fresh process");
            var history = QaArchive(); while (history.MoveNext()) yield return history.Current;

            // A second fixed run exercises the current rules; its actual verdict determines the transition.
            _campaign = Campaign.New(16); _fight = null; Save(); ShowTitle(); yield return null;
            Check(Click("continue"), "fixed seed 16 enters through Continue"); yield return null;
            Check(Click("start-tutorial"), "second fixed tutorial starts"); yield return null;
            var secondTutorial = QaFinishFight(16); while (secondTutorial.MoveNext()) yield return secondTutorial.Current;
            Check(_result.TutorialWon, "second tutorial won");
            Check(Click("after-result"), "second tutorial receipt continues"); yield return null;
            Check(Click("tribe-" + _campaign.Choosable()[0].Id), "second fixed run starts"); yield return null;
            bool chiefTransition = false;
            for (int n = 0; n < 24 && _campaign.Stage == CampaignStage.InRun; n++)
            {
                string target = _campaign.Target.Id;
                var next = QaEncounter(16, n); while (next.MoveNext()) yield return next.Current;
                if (_campaign.Stage == CampaignStage.InRun && _campaign.Target.Id != target)
                {
                    chiefTransition = true;
                    Check(_campaign.HasJoined(target)==_qaLastResult.ChiefCaptured&&(_qaLastResult.ChiefCaptured||_qaLastResult.ChiefKilled),
                        "chief transition grants the tribe only for an actual capture");
                }
            }
            Check(_campaign.Stage == CampaignStage.ChooseTribe, "seed16 completes its run at camp");
            Log("WHOLE RUN seed16 won="+_campaign.RunsWon+" crossedTribes="+chiefTransition);
            history = QaArchive(); while (history.MoveNext()) yield return history.Current;
            Capture("second-run-camp");

            // Authentic pre-hunt save, kept separately from the natural runs above.
            string huntFile = Argument("-ksHeadlessHunt");
            Check(!string.IsNullOrEmpty(huntFile), "explicit historical hunt fixture supplied");
            _campaign = CampaignSave.Read(File.ReadAllText(huntFile)); _fight = null; Save(); ShowTitle(); yield return null;
            Check(Click("continue"), "hunt fixture Continue"); yield return null;
            Check(_campaign.Offers[0].Kind == EncounterKind.AlphaHunt, "fixture is at the final rival hunt");
            for (int n = 0; n < 8 && _campaign.Stage == CampaignStage.InRun; n++)
            {
                var hunt = QaEncounter(_campaign.Seed, n); while (hunt.MoveNext()) yield return hunt.Current;
            }
            Check(_campaign.Stage == CampaignStage.ChooseTribe, "rival hunt outcome returns to camp");
            Capture("hunt-ended");

            // A damaged main file must preserve and reopen the last good save.
            _savePath = Path.Combine(_capture, "recovery-test.txt"); Save(); Save();
            string good = CampaignSave.Write(_campaign);
            Check(File.Exists(_savePath + ".bak"), "safe writer retains backup");
            File.WriteAllText(_savePath, "damaged test save");
            _campaign = null; _fight = null; ShowTitle(); yield return null;
            Check(_notice.StartsWith(CampaignSave.BackupLoadedNote), "title discloses backup recovery");
            Check(Click("continue"), "backup opens through Continue"); yield return null;
            Check(CampaignSave.Write(_campaign) == good, "backup recovery preserves complete campaign");
            Capture("recovered-camp");
        }

        void QaCheckpoint(string name)
        {
            Save();
            string state = CampaignSave.Write(_campaign);
            Check(CampaignSave.TryReadFile(_savePath, null, out var disk, out _) &&
                CampaignSave.Write(disk) == state, name + " sealed disk save matches live state");
            File.WriteAllText(Path.Combine(_capture, name + "-expected.txt"), state);
        }

        IEnumerator QaEncounter(ulong seed, int index)
        {
            int offer = new CampaignPilot(PilotStyle.Reference, seed).PickOffer(_campaign);
            Check(Click("offer-" + offer), "road encounter " + index + " raycast"); yield return null;
            Check(_page == Page.Squad, "encounter opens squad");
            QaChooseDuelist();yield return null;
            Check(Click("start-fight"), "squad starts encounter " + index); yield return null;
            var fight = QaFinishFight(seed * 31UL + (ulong)index);
            while (fight.MoveNext()) yield return fight.Current;
            _qaLastResult=_result; // AfterResult deliberately clears the displayed receipt.
            if (index == 0 || _result.RunOver || _result.Encounter.Kind == EncounterKind.Chief)
                Capture("result-" + seed + "-" + index);
            Check(Click(_modal!=null&&_buttons.ContainsKey("bank-reward")?"bank-reward":"after-result"), "receipt returns to authoritative stage"); yield return null;
            Check(_page == (_campaign.Stage == CampaignStage.InRun ? Page.Offers : Page.ChooseTribe),
                "receipt navigation matches campaign stage");
        }

        IEnumerator QaFinishFight(ulong pilotSeed)
        {
            _paused = true;
            // The independent continuation predicts the receipt through the same public sim API.
            // The player reaches it via Update and the real result control.
            var expected = CampaignSave.Read(CampaignSave.Write(_campaign));
            Check(expected.Active != null && string.IsNullOrEmpty(expected.LoadNote), "active fight round-trips before continuation");
            var expectedFight=expected.Active;
            var expectedPilot=new CampaignPilot(PilotStyle.Reference,pilotSeed);
            for(int tick=0;tick<6000&&!expectedFight.Over;tick++)
            {
                // A player's free verdict is sent outside live combat. Match that command context,
                // while every outcome still comes from the independent authoritative continuation.
                if(expectedFight.Sim.AwaitingVerdict)expectedFight.TryOrder(Directive.Hold,null,false,out _);
                else expectedPilot.Step(expectedFight);
                expectedFight.Tick();
            }
            Check(expectedFight.Over,"independent continuation reaches outcome");
            var expectedResult=expected.Finish(expectedFight);
            string final = CampaignSave.Write(expected);
            _pilot = new CampaignPilot(PilotStyle.Reference, pilotSeed); _pilotOn = true; _speed = 60; _paused = false;
            int frames = 0;
            while (!_fight.Over && frames++ < 2400)
            {
                if(_fight.Sim.AwaitingVerdict)
                    Check(Click("order-0"),"reference playtest explicitly spares the kneeling chief through UI");
                yield return null;
            }
            Check(_fight.Over, "fight completes within bounded player frames");
            _paused = true; _endAge = 3; RefreshFight(); yield return null;
            Check(Click("see-result"), "completed fight receipt raycast"); yield return null;
            Check(_result.Won == expectedResult.Won && _result.MaterialsEarned == expectedResult.MaterialsEarned,
                "receipt reports authoritative outcome and reward");
            Check(CampaignSave.Write(_campaign) == final, "UI finish applies rewards losses and learning exactly once");
            Check(CampaignSave.TryReadFile(_savePath, null, out var disk, out _) && CampaignSave.Write(disk) == final,
                "result is durably saved before continuing");
            Log("OUTCOME " + _result.Encounter.Kind + " won=" + _result.Won + " runWon=" + _result.RunWon +
                " runOver=" + _result.RunOver + " materials=" + _result.MaterialsEarned);
        }

        IEnumerator QaArchive()
        {
            string before = CampaignSave.Write(_campaign);
            foreach (var record in _campaign.Archive.Fights.ToArray())
            {
                ShowArchivedFight(record); yield return null;
                Check(Click("replay-watch"), "history opens " + record.FightId); yield return null;
                Check(_playback != null, "fresh recording is playable");
                Check(Click("playback-end"), "history seeks to end"); yield return null;
                Check(_playback.Matched, "full state and history match " + record.FightId);
                Check(Click("playback-back"), "history returns"); yield return null;
                CloseModal();
            }
            Check(CampaignSave.Write(_campaign) == before, "watching whole run changes no campaign state");
        }
    }
}
