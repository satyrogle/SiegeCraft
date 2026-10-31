using System;
using System.Collections;
using System.IO;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Opt-in presentation regression, not a balance sweep. Supply three copies of a camp save with
        // Bloodridge WaitOutParry at 0, just below threshold, and threshold. All writes stay in _capture.
        // Each fixture must offer a casting encounter on its first road. No pilot or tuning changes.
        IEnumerator ValidateParryPrompts()
        {
            string fixtures = Argument("-ksParryPromptValidate");
            foreach (int habit in new[] { 0, BattleSimulation.HabitThreshold - 1, BattleSimulation.HabitThreshold })
            {
                string tag = "parry-" + habit;
                _savePath = Path.Combine(_capture, tag + "-save.txt");
                _campaign = CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures, tag + ".txt")));
                _campaign.StartRun();
                int offer = -1;
                for (int i = 0; i < _campaign.Offers.Count; i++)
                    if (_campaign.Offers[i].EnemyCasts) { offer = i; break; }
                Check(offer >= 0, tag + " has a casting encounter");
                SetPage(Page.Offers); yield return null;
                Check(Click("offer-" + offer), tag + " select encounter through UI"); yield return null;
                Check(Click("start-fight"), tag + " start through UI");
                _pilotOn = false; _paused = true; _speed = 1;
                yield return null;
                var sim = _fight.Sim;
                bool waits = habit >= BattleSimulation.HabitThreshold;
                Check(sim.WaitsOutParry(1) == waits, tag + " authoritative habit");
                Check(_theyKnow.Contains("wait out your Hold") == waits, tag + " tactics explain active habit only");
                while (!_fight.Over && sim.WindupTicks(1) == 0) { _fight.Tick(); DrainEvents(); }
                Check(!_fight.Over && sim.WindupTicks(1) > FightRules.PerfectHoldTicks, tag + " reached early cast");
                RefreshFight(); DrawBattle();
                Check(_castLabel.key.StartsWith((waits ? "HOLD WON'T PARRY" : "CAST " + Seconds(sim.WindupTicks(1))) + "|", StringComparison.Ordinal), tag + " early cast label");
                Check(_castMark.enabled == !waits, tag + " parry marker only for parryable cast");
                Check(waits ? _pxBanner.key.Contains("cannot parry") : _pxBanner2.key.Contains("to parry"), tag + " early cast advice");
                yield return null; Capture(tag + "-early");
                while (!_fight.Over && sim.WindupTicks(1) > 5) { _fight.Tick(); DrainEvents(); }
                Check(!_fight.Over && sim.WindupTicks(1) > 0, tag + " reached late cast");
                RefreshFight(); DrawBattle();
                long cueTick = sim.CurrentTick;
                Check(_castLabel.key.StartsWith((waits ? "HOLD WON'T PARRY" : "HOLD NOW!") + "|", StringComparison.Ordinal), tag + " late cast label");
                Check(waits ? _pxBanner.key.Contains("cannot parry") : _pxBanner.key.StartsWith("NOW: Hold to parry", StringComparison.Ordinal), tag + " late cast advice");
                yield return null; Capture(tag + "-late");
                Check(Click("order-0"), tag + " Hold through its plate");
                int eventStart = sim.Events.Count;
                while (!_fight.Over && sim.WindupTicks(1) > 0)
                {
                    _fight.Tick(); DrainEvents();
                    if (sim.Events.Skip(eventStart).Any(e => e.Kind == BattleEventKind.Reaction && e.Detail == BattleSimulation.WaitOutDetail))
                        break;
                }
                if (waits)
                {
                    Check(sim.WindupTicks(1) > 0, tag + " cast actually waits out Hold");
                    RefreshFight(); DrawBattle();
                    Check(_castLabel.key.StartsWith("HOLD WON'T PARRY|", StringComparison.Ordinal) && !_castMark.enabled && _pxBanner.key.Contains("cannot parry"), tag + " no false cue during actual wait");
                    yield return null; Capture(tag + "-waiting");
                    while (!_fight.Over && sim.WindupTicks(1) > 0) { _fight.Tick(); DrainEvents(); }
                }
                Check(sim.Events.Skip(eventStart).Any(e => e.Kind == BattleEventKind.Parry && e.ActorSide == 0) == !waits, tag + " actual parry outcome matches advice");
                RefreshFight(); DrawBattle();
                Check(!_castMark.enabled && !_castBack.enabled && _castLabel.key.StartsWith("|", StringComparison.Ordinal), tag + " cue clears after cast");
                foreach (string kind in new[] { "cast", "halved" })
                {
                    Check(Tip(kind, false, false, sim.WaitsOutParry(1)).Contains("cannot parry") == waits, tag + " result advice " + kind);
                    Check(!Tip(kind, false, true, sim.WaitsOutParry(1)).Contains("Fall Back"), tag + " restricted result does not offer Fall Back " + kind);
                }
                // Finish this one fixture without issuing any more orders, then check its recorded state.
                while (!_fight.Over) _fight.Tick();
                string fightId = _fight.Setup.FightId;
                ShowResult(); yield return null;
                var archived = _campaign.Archive.Find(fightId);
                Check(archived != null && new BattleSimulation(archived.Setup).WaitsOutParry(1) == waits, tag + " archive retains fight-time habit");
                string beforeReplay = CampaignSave.Write(_campaign);
                WatchFight(archived); yield return null;
                SeekPlayback(cueTick); yield return null;
                Check(_playback.Failure.Length == 0 && _playback.Sim.WaitsOutParry(1) == waits, tag + " replay uses recorded habit");
                Capture(tag + "-replay");
                LeavePlayback(); yield return null;
                Check(CampaignSave.Write(_campaign) == beforeReplay, tag + " replay preserves campaign");
                Check(CampaignSave.Write(CampaignSave.Read(File.ReadAllText(_savePath))) == beforeReplay, tag + " isolated save reload");

                // A fresh copy keeps this real blocked-Unleash event separate from the cast-timing fixture.
                _savePath = Path.Combine(_capture, tag + "-blocked-save.txt");
                _campaign = CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures, tag + ".txt")));
                _campaign.StartRun();
                SetPage(Page.Offers); yield return null;
                Check(Click("offer-" + offer), tag + " select blocked-Unleash encounter"); yield return null;
                Check(Click("start-fight"), tag + " start blocked-Unleash encounter");
                _pilotOn = false; _paused = true; _speed = 1;
                yield return null;
                sim = _fight.Sim;
                Check(sim.WaitsOutParry(1) == waits, tag + " blocked-Unleash authoritative habit");
                int beforeUnleash = sim.Events.Count;
                Check(Click("order-3"), tag + " Unleash through its plate");
                RefreshFight(); DrawBattle();
                Check(sim.Events.Skip(beforeUnleash).Any(e => e.Kind == BattleEventKind.Directive &&
                    e.ActorSide == 1 && e.Detail == BattleSimulation.BlockedDetail), tag + " Unleash actually blocked");
                Check(_pxBanner.key.StartsWith("Their shields stopped your Unleash.|", StringComparison.Ordinal), tag + " blocked-Unleash explanation");
                string expectedHint = waits
                    ? "They wait out your Hold: it only halves their casts. No parry, no Riposte."
                    : "Parry their cast first, then Unleash: the Riposte gets through.";
                Check(_pxBanner2.key.StartsWith(expectedHint + "|", StringComparison.Ordinal), tag + " blocked-Unleash hint respects parry learning");
                yield return null; Capture(tag + "-unleash-blocked");
            }
        }
    }
}
