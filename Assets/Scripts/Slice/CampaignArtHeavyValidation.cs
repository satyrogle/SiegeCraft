using System;
using System.Collections;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        void HeavyTestTick()
        {
            _visualTime += 1f / SimClock.TicksPerSecond;
            AdvanceCombatPoses(1f / SimClock.TicksPerSecond);
            Anticipate(); _fight.Tick(); DrainEvents();
        }

        // Fixed trait fixtures exercise the live HUD without changing the campaign or its tuning.
        void CheckHunterHeavyCues(string playerId, string enemyId)
        {
            bool wasPaused = _paused;
            _paused = false;
            foreach (var band in new[] { BandTrait.LongReach, BandTrait.Unstoppable, BandTrait.Berserk })
            {
                var player = new[] { new UnitSpec(playerId, UnitClass.Warden, Row.Front, UnitRole.Pal, 100, 100) };
                var enemy = new[] { new UnitSpec(enemyId, UnitClass.Warden, Row.Front, UnitRole.Chief, 100, 100) };
                var rules = new SideRules(0, 0, true, HeavyRules.EveryTicks);
                var theirs = new SideRules(0, 0, true, HeavyRules.EveryTicks, band: band);
                var fixture = new BattleSimulation(new BattleSetup("heavy-hud-trait", 7, player, enemy, null, null,
                    1200, 5, NerveRules.Off, side0Rules: rules, side1Rules: theirs,
                    fightRules: new FightRules(false, false, true, false)));
                bool sawWindow = false, sawBerserk = false, checkedBefore = false, checkedDuring = false;
                for (int tick = 0; tick < 1200 && !fixture.IsFinished; tick++)
                {
                    fixture.Tick();
                    int left = fixture.HeavyTicks(1);
                    if (left <= 0 || left > FightRules.PerfectHoldTicks) continue;
                    bool berserk = fixture.BerserkTicksLeft(1) > 0;
                    if (berserk ? checkedDuring : checkedBefore) continue;
                    bool expected = band != BandTrait.Unstoppable && !berserk;
                    string snapshot = fixture.OrderedStateSnapshot;
                    DrawCastTimer(fixture);
                    HeavyBanner(fixture, out _, out string advice, out _);
                    Check(fixture.HeavyParryable(1) == expected, band + " fixture reaches expected eligibility");
                    Check(_castMark.enabled == expected && _castLabel.key.StartsWith(expected ? "HOLD NOW!|" : "CANNOT PARRY / ", StringComparison.Ordinal),
                        band + (berserk ? " active" : " initial") + " HUD follows eligibility");
                    Check(advice.Contains(expected ? "HOLD NOW" : "cannot be parried"), band + " banner agrees with timer");
                    Check(fixture.OrderedStateSnapshot == snapshot, band + " HUD query leaves sim unchanged");
                    _paused = true;
                    DrawCastTimer(fixture);
                    Check(_castLabel.key.StartsWith(expected ? "RESUME TO PARRY|" : "CANNOT PARRY / ", StringComparison.Ordinal),
                        band + " paused cue stays truthful");
                    _paused = false;
                    sawWindow = true; sawBerserk |= berserk;
                    if (berserk) checkedDuring = true; else checkedBefore = true;
                    if (band != BandTrait.Berserk || (checkedBefore && checkedDuring)) break;
                }
                Check(sawWindow && (band != BandTrait.Berserk || sawBerserk), band + " fixture covers its heavy timing states");
            }
            _paused = wasPaused;
            DrawCastTimer(_fight.Sim);
        }

        // Fixed saved campaign fixtures, actual UI order handlers and authoritative outcomes.
        // No pilot, seed search, altered stats, or owner save access.
        IEnumerator ValidateHeavyPresentation()
        {
            string fixtures = Argument("-ksHeavyValidate");
            for (int scenario = 0; scenario < 3; scenario++)
            {
                bool pausedOrder = scenario == 1, delayed = scenario == 2;
                string tag = delayed ? "delayed" : pausedOrder ? "paused" : "live";
                _savePath = Path.Combine(_capture, tag + "-save.txt");
                // These 18 Oct scenarios exercise the pre-20-Oct heavy rules. A new rules-14 road deliberately
                // has no casting offer here, so retain the original rules rather than changing gameplay to suit this check.
                _campaign = CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures, delayed ? "parry-3.txt" : "parry-0.txt")),
                    new CampaignTuning { RulesCap = 0 });
                // These saved habits belong to Bloodridge; random tier selection can choose an untrained tribe.
                _campaign.ChooseTribe(CampaignTribes.BloodridgeId);
                int offer = -1;
                for (int i = 0; i < _campaign.Offers.Count; i++)
                    if (_campaign.Offers[i].EnemyCasts) { offer = i; break; }
                Check(offer >= 0, tag + " fixed casting encounter available");
                SetPage(Page.Offers); yield return null;
                Check(Click("offer-" + offer), tag + " encounter selected through road UI"); yield return null;
                QaChooseDuelist();yield return null;
                Check(Click("start-fight"), tag + " starts through squad UI");
                _pilotOn = false; _paused = true; _speed = 1; yield return null;
                var sim = _fight.Sim;
                Check(_fight.RulesVersion == 0, tag + " historical heavy fixture keeps its original rules");
                Check(sim.HeavyOn, tag + " uses authoritative heavy rules");
                for (int i = 0; i < 600 && !ViewOver && sim.HeavyTicks(1) == 0; i++) HeavyTestTick();
                Check(!ViewOver && sim.HeavyTicks(1) > FightRules.PerfectHoldTicks, tag + " reached early heavy");
                string actorId = sim.HeavySwingerId(1), targetId = sim.HeavyTargetId(1);
                if (scenario == 0) CheckHunterHeavyCues(targetId, actorId);
                long tellTick = sim.CurrentTick;
                string snapshot = sim.OrderedStateSnapshot;
                RefreshFight(); DrawBattle();
                Check(sim.OrderedStateSnapshot == snapshot, tag + " drawing does not mutate simulation");
                Check(_castBack.enabled && _castCaster == actorId + ":heavy", tag + " timer follows heavy attacker");
                Check(_heavyTargetBack.enabled && FindFigure(targetId) != null, tag + " aimed fighter is marked");
                Check(_castLabel.key.StartsWith("RESUME TO PARRY|", StringComparison.Ordinal), tag + " paused tell never says Hold now");
                Check(_orderEffect[0].key.StartsWith("Paused: no parry|", StringComparison.Ordinal), tag + " Hold plate explains paused input");
                Capture(tag + "-early-paused");
                float savedVisualTime = _visualTime; long tick = sim.CurrentTick;
                for (int i = 0; i < 4; i++) yield return null;
                Check(sim.CurrentTick == tick && _visualTime == savedVisualTime, tag + " pause freezes fight and pose clocks");
                while (!ViewOver && sim.HeavyTicks(1) > 5) HeavyTestTick();
                Check(!ViewOver && sim.HeavyTicks(1) > 0, tag + " reached parry timing");
                var actor = FindFigure(actorId);
                DrawBattle();
                Check(actor.drawnClip == actor.attack, tag + " heavy lead-in uses delivered attack clip");
                _paused = pausedOrder; RefreshFight(); DrawBattle();
                Check(_castLabel.key.StartsWith(pausedOrder ? "RESUME TO PARRY|" : "HOLD NOW!|", StringComparison.Ordinal), tag + " cue respects live versus paused input");
                Capture(tag + "-window");
                int from = sim.Events.Count;
                Check(Click("order-0"), tag + " Hold clicked through actual plate");
                _paused = true;
                if (pausedOrder)
                {
                    Save(); snapshot = sim.OrderedStateSnapshot;
                    _campaign = CampaignSave.Read(File.ReadAllText(_savePath)); ResumeFight(); _pilotOn = false;
                    sim = _fight.Sim;
                    Check(sim.OrderedStateSnapshot == snapshot, "paused order survives isolated save/reload");
                    from = sim.Events.Count;
                }
                for (int i = 0; i < 90 && !ViewOver && sim.HeavyTicks(1) > 0; i++)
                {
                    HeavyTestTick();
                    if (delayed && sim.HeavyWaited(1)) break;
                }
                if (delayed)
                {
                    Check(sim.HeavyTicks(1) > 0 && sim.HeavyWaited(1), "learned heavy actually delayed its release");
                    _paused = false; RefreshFight(); DrawBattle();
                    Check(_pxBanner2.key.Contains("Hold again now"), "delayed tell requests a fresh timed Hold");
                    Capture("delayed-second-window");
                    Check(Click("order-0"), "second live Hold through plate"); _paused = true;
                    while (!ViewOver && sim.HeavyTicks(1) > 0) HeavyTestTick();
                }
                Check(sim.Events.Skip(from).Any(e => e.Kind == BattleEventKind.Parry && e.ActorSide == 0) == !pausedOrder,
                    tag + " real outcome matches live/paused parry advice");
                RefreshFight(); DrawBattle();
                Check(!_castBack.enabled && !_heavyTargetBack.enabled, tag + " tell and target clear after release");
                if (!pausedOrder)
                {
                    Check(sim.NextCombo(0).Combo == FightRules.Riposte, tag + " sim offers Riposte");
                    Check(_heavyParryWord.enabled, tag + " parry is prominent");
                    Capture(tag + "-parried");
                    _paused = false; RefreshFight();
                    Check(Click("order-3"), tag + " Riposte through Unleash plate"); _paused = true;
                    Check(sim.Events.Skip(from).Any(e => e.Kind == BattleEventKind.Combo && e.Detail == FightRules.Riposte), tag + " authoritative Riposte occurred");
                    RefreshFight(); DrawBattle(); Capture(tag + "-riposte");
                }
                else
                {
                    Capture("paused-heavy-landed");
                    Order(Directive.FocusFire);
                    Check(_targeting && Click("target-" + actorId), "Focus Fire target clicked while paused");
                    Check(sim.FocusedId(0)==actorId, "paused Focus Fire still targets normally");
                    Save();
                    var restored = CampaignSave.Read(File.ReadAllText(_savePath));
                    var command = restored.Active.Session.Log.Commands.Last();
                    Check(command.TargetId==actorId && command.Arg0==HeavyRules.PausedOrderArg,
                        "targeted order records paused input through reload too");
                }

                // Save a real, unfinished fight for the separate native-input pass.
                if (scenario == 0) { Save(); File.Copy(_savePath, Path.Combine(_capture,"native-fixture.txt"), true); }
                // Let this exact encounter move normally for four seconds. Retain a sequence for visual review.
                _paused = false; _speed = 1;
                for (int frame = 0; frame < 120 && !ViewOver; frame++)
                { yield return null; if (frame % 2 == 0) Capture(tag + "-motion-" + (frame / 2).ToString("D3")); }
                _paused = true;
                if (scenario == 0)
                {
                    while (!ViewOver) HeavyTestTick();
                    string fightId = _fight.Setup.FightId;
                    var heavyRelease=sim.Events.First(e=>e.Kind==BattleEventKind.Release&&!string.IsNullOrEmpty(e.TargetId)&&
                        (e.Detail==Moves.Full||e.Detail==Moves.Soaked));
                    var heavySwing=new Swing{Side=1-heavyRelease.ActorSide};
                    Classify(sim,new System.Collections.Generic.List<BattleEvent>{heavyRelease},heavySwing,_fight.Encounter.Kind);
                    Check(heavySwing.Kind=="heavy"&&heavySwing.Cause.Contains("heavy swing")&&!heavySwing.Cause.Contains("Eclipse"),
                        "actual heavy release is named as a heavy in the explanation");
                    var castRelease=sim.Events.First(e=>e.Kind==BattleEventKind.Release&&string.IsNullOrEmpty(e.TargetId)&&
                        (e.Detail==Moves.Full||e.Detail==Moves.Soaked));
                    var castSwing=new Swing{Side=1-castRelease.ActorSide};
                    Classify(sim,new System.Collections.Generic.List<BattleEvent>{castRelease},castSwing,_fight.Encounter.Kind);
                    string actualSpell=sim.Events.Last(e=>e.Tick<=castRelease.Tick&&e.Kind==BattleEventKind.Windup&&
                        e.ActorId==castRelease.ActorId&&e.Detail!=HeavyRules.Heavy).Detail;
                    Check(castSwing.Cause.Contains(actualSpell),"actual spell retains its name despite overlapping heavy windups: "+actualSpell);
                    RefreshFight();DrawBattle();Capture("heavy-loss-explanation");
                    ShowResult(); yield return null;
                    var record = _campaign.Archive.Find(fightId);
                    string beforeReplay = CampaignSave.Write(_campaign);
                    WatchFight(record); SeekPlayback(tellTick); yield return null;
                    Check(_playback.Failure.Length == 0 && _castBack.enabled && _heavyTargetBack.enabled,
                        "replay seek restores heavy attacker and target tells");
                    Check(_feed.Last().Contains("heavy swing"), "replay names the heavy instead of calling it a cast");
                    Capture("heavy-replay");
                    LeavePlayback(); yield return null;
                    Check(CampaignSave.Write(_campaign) == beforeReplay, "heavy replay does not mutate campaign");
                }
            }
        }
    }
}
