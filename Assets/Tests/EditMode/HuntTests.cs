using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The hunt (Jacob, 22 Oct 2026: "yea go i like it", then "yes go"; DESIGN_LOCKS "The hunters' boss, the hunt"). One
    /// quick test per piece, only to prove it runs. Nothing here is tuning.
    public class HuntTests
    {
        // A fighter at these percents of power and health, in its class's row. Tough by default, so fights last.
        private static UnitSpec U(string id, UnitClass unitClass, int power = 100, int health = 1000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, power, health);
        }

        // The hunt captain leads from the back, a Marksman, as in the campaign.
        private static UnitSpec Captain(int health = 1000)
        {
            return new UnitSpec("captain", UnitClass.Marksman, Row.Back, UnitRole.Chief, 100, health);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        // His Warden and Skirmisher in front, his Trapper and him behind.
        private static List<UnitSpec> Hunters()
        {
            return Side(U("w", UnitClass.Warden), U("x", UnitClass.Skirmisher), U("t", UnitClass.Trapper), Captain());
        }

        // Ours with no Warden and no Medic, so nothing breaks a step without an order.
        private static List<UnitSpec> Plain()
        {
            return Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman));
        }

        // The hunters run the hunt with no cast of their own and no heavy swings; ours has a grade 1 Eclipse and combos.
        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, bool nerve = false)
        {
            var hunters = new SideRules(0, 0, false, kit: ChiefKit.Hunt);
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, nerve ? NerveRules.On() : NerveRules.Off, 10, 400,
                new SideRules(1, 0, true), hunters, FightRules.CampaignChiefTactics));
        }

        private static bool Order(SimSession session, string directive, string target = "")
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        // Ticks until an event matches, calling before ahead of each tick; events from before the call are not looked at.
        private static BattleEvent? Next(BattleSimulation sim, SimSession session, Func<BattleEvent, bool> match, Action before = null)
        {
            int seen = sim.Events.Count;
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                before?.Invoke();
                session.Tick();
                for (; seen < sim.Events.Count; seen++)
                    if (match(sim.Events[seen])) return sim.Events[seen];
            }
            return null;
        }

        private static string Landed(HuntStep step, string how)
        {
            return ChiefKits.Name(step) + "." + how;
        }

        private static bool IsHuntLand(BattleEvent e)
        {
            return e.Kind == BattleEventKind.Land && (e.Detail.StartsWith("Track.", StringComparison.Ordinal) ||
                e.Detail.StartsWith("Drive.", StringComparison.Ordinal) || e.Detail.StartsWith("Corner.", StringComparison.Ordinal) ||
                e.Detail.StartsWith("Kill.", StringComparison.Ordinal));
        }

        // A step broken, or the Kill landing: every hunt Land but the net landing.
        private static bool HuntOutcome(BattleEvent e)
        {
            return IsHuntLand(e) && e.Detail != Landed(HuntStep.Corner, ChiefKits.Full);
        }

        private static bool GloryFor(BattleSimulation sim, ChiefAnswer answer, long tick)
        {
            return sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.ActorSide == 0 && e.Detail == ChiefKits.Name(answer) && e.Tick == tick);
        }

        // ------------------------------------------------------------ the four steps

        [Test]
        public void TheHuntTracksDrivesCornersAndKillsWhenNobodyBreaksIt()
        {
            var sim = Fight("hunt.steps", 81, Plain(), Hunters());
            var session = new SimSession(sim);
            Assert.IsTrue(sim.HuntOn);
            Assert.AreEqual(1, sim.HuntSide);
            Assert.AreEqual(1, sim.HuntCount);
            Assert.AreEqual(ChiefKit.Hunt, sim.Kit(1));

            // Track: his Trapper marks your most hurt fighter.
            BattleEvent track = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.Name(HuntStep.Track), track.Detail);
            Assert.AreEqual(ChiefKits.HuntStartTicks, track.Tick);
            Assert.AreEqual(ChiefKits.TrackTicks, track.Amount);
            Assert.AreEqual("captain", track.ActorId);
            string prey = track.TargetId;
            Assert.AreEqual(0, sim.FindById(prey).Side);
            Assert.AreEqual(HuntStep.Track, sim.HuntStepOf(0));
            Assert.AreEqual(prey, sim.HuntPreyId(0));

            // Drive, Corner (the line, his Trapper throwing), then the net and the Kill.
            BattleEvent drive = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.Name(HuntStep.Drive), drive.Detail);
            Assert.AreEqual(track.Tick + ChiefKits.TrackTicks, drive.Tick);
            Assert.AreEqual(ChiefKits.DriveTicks, drive.Amount);
            Assert.AreEqual(prey, drive.TargetId);
            BattleEvent corner = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.Name(HuntStep.Corner), corner.Detail);
            Assert.AreEqual(drive.Tick + ChiefKits.DriveTicks, corner.Tick);
            Assert.AreEqual(ChiefKits.LineTicks, corner.Amount);
            Assert.AreEqual("t", sim.HuntNetterId(0), "his Trapper throws the net");
            BattleEvent kill = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.Name(HuntStep.Kill), kill.Detail);
            Assert.AreEqual(corner.Tick + ChiefKits.LineTicks, kill.Tick);
            Assert.AreEqual(ChiefKits.KillTicks, kill.Amount);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Mark && e.Detail == ChiefKits.NetMark && e.ActorId == "t" &&
                e.TargetId == prey && e.Tick == kill.Tick), "the net lands");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Land && e.Detail == Landed(HuntStep.Corner, ChiefKits.Full) &&
                e.Tick == kill.Tick));
            Assert.Greater(sim.FindById(prey).SnareTicks, 0, "held in the net");

            // Nobody breaks it: the glaive lands in full and you ate it. The next hunt takes a new prey.
            BattleEvent land = Next(sim, session, IsHuntLand).Value;
            Assert.AreEqual(Landed(HuntStep.Kill, ChiefKits.Full), land.Detail);
            Assert.AreEqual(kill.Tick + ChiefKits.KillTicks, land.Tick);
            Assert.AreEqual(sim.FindById(prey).MaxHp * ChiefKits.KillPercent / 100, land.Amount);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.KillDetail && e.TargetId == prey &&
                e.Amount == land.Amount && e.Tick == land.Tick));
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.EatenDetail && e.Tick == land.Tick));
            BattleEvent again = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.Name(HuntStep.Track), again.Detail);
            Assert.AreNotEqual(prey, again.TargetId, "a new prey");
        }

        [Test]
        public void YourWardenCoveringThePreyOrYourMedicHealingThemOverHalfBreaksTheTrack()
        {
            // Our Warden covers our hurt Marksman, the prey, so the hunt loses the scent at once.
            var guard = Fight("hunt.track.guard", 82, Side(U("m", UnitClass.Marksman, 100, 100), U("g", UnitClass.Warden)), Hunters());
            var gs = new SimSession(guard);
            BattleEvent track = Next(guard, gs, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual("m", track.TargetId);
            BattleEvent guarded = Next(guard, gs, IsHuntLand).Value;
            Assert.AreEqual(Landed(HuntStep.Track, ChiefKits.Guarded), guarded.Detail);
            Assert.AreEqual(track.Tick + 1, guarded.Tick);
            Assert.AreSame(guard.FindById("g"), guard.CoveredBy(guard.FindById("m")));
            Assert.IsTrue(GloryFor(guard, ChiefAnswer.Guard, guarded.Tick));
            Assert.AreEqual(HuntStep.None, guard.HuntStepOf(0), "it starts again at Track");

            // Our Medic heals the prey from under half to over it. Her treatments are small, so the prey slides down past half
            // and we look for a fight where one lands just then, in the Track.
            for (ulong seed = 1; seed < 40; seed++)
            {
                var heal = Fight("hunt.track.heal", seed, Side(U("s", UnitClass.Skirmisher, 100, 100), U("h", UnitClass.Medic)), Hunters());
                BattleEvent? healed = Next(heal, new SimSession(heal), e => e.Kind == BattleEventKind.Land &&
                    e.Detail == Landed(HuntStep.Track, ChiefKits.Healed));
                if (healed == null) continue;
                Assert.IsTrue(heal.Events.Any(e => e.Kind == BattleEventKind.Heal && e.ActorId == "h" && e.TargetId == healed.Value.TargetId &&
                    e.Tick == healed.Value.Tick));
                Assert.IsTrue(GloryFor(heal, ChiefAnswer.Heal, healed.Value.Tick));
                return;
            }
            Assert.Fail("in forty fights our Medic never healed a prey over half in the Track");
        }

        [Test]
        public void HoldTheLineOrFocusFireOnHisFrontRowBreaksTheDrive()
        {
            var hold = Fight("hunt.drive.hold", 83, Plain(), Hunters());
            var hs = new SimSession(hold);
            BattleEvent braced = Next(hold, hs, IsHuntLand, () =>
            {
                if (hold.HuntStepOf(0) == HuntStep.Drive && hold.HoldTicksLeft(0) == 0) Order(hs, "Hold");
            }).Value;
            Assert.AreEqual(Landed(HuntStep.Drive, ChiefKits.Braced), braced.Detail);
            Assert.IsTrue(GloryFor(hold, ChiefAnswer.Brace, braced.Tick));

            var focus = Fight("hunt.drive.focus", 83, Plain(), Hunters());
            var fs = new SimSession(focus);
            BattleEvent broken = Next(focus, fs, IsHuntLand, () =>
            {
                if (focus.HuntStepOf(0) == HuntStep.Drive && focus.FocusTicksLeft(0) == 0) Order(fs, "FocusFire", "w");
            }).Value;
            Assert.AreEqual(Landed(HuntStep.Drive, ChiefKits.Broken), broken.Detail);
            Assert.IsTrue(GloryFor(focus, ChiefAnswer.Break, broken.Tick));
        }

        [Test]
        public void FocusFireOnHisTrapperYourMedicOrYourUnleashBreaksTheCorner()
        {
            // Focus Fire on the one who throws the net while the line glows: the net never comes.
            var focus = Fight("hunt.corner.focus", 84, Plain(), Hunters());
            var fs = new SimSession(focus);
            BattleEvent broken = Next(focus, fs, IsHuntLand, () =>
            {
                string netter = focus.HuntNetterId(0);
                if (netter.Length > 0 && focus.FocusTicksLeft(0) == 0) Order(fs, "FocusFire", netter);
            }).Value;
            Assert.AreEqual(Landed(HuntStep.Corner, ChiefKits.Broken), broken.Detail);
            Assert.IsFalse(focus.Events.Any(e => e.Kind == BattleEventKind.Mark && e.Detail == ChiefKits.NetMark));
            Assert.IsTrue(GloryFor(focus, ChiefAnswer.Break, broken.Tick));

            // Unleash while the prey waits in the net for the Kill: the net is torn.
            var tear = Fight("hunt.corner.tear", 84, Plain(), Hunters());
            var ts = new SimSession(tear);
            BattleEvent torn = Next(tear, ts, HuntOutcome, () =>
            {
                if (tear.HuntStepOf(0) == HuntStep.Kill && tear.UnleashTicksLeft(0) == 0) Order(ts, "Unleash");
            }).Value;
            Assert.AreEqual(Landed(HuntStep.Corner, ChiefKits.Torn), torn.Detail);
            Assert.AreEqual(0, tear.FindById(torn.TargetId).SnareTicks, "free");
            Assert.IsTrue(GloryFor(tear, ChiefAnswer.Smash, torn.Tick));

            // Our Medic treats the netted prey and cuts them free. We look for a fight where she gets to them in time.
            for (ulong seed = 1; seed < 40; seed++)
            {
                var cut = Fight("hunt.corner.heal", seed, Side(U("s", UnitClass.Skirmisher), U("h", UnitClass.Medic)), Hunters());
                BattleEvent? healed = Next(cut, new SimSession(cut), e => e.Kind == BattleEventKind.Land &&
                    e.Detail == Landed(HuntStep.Corner, ChiefKits.Healed));
                if (healed == null) continue;
                Assert.IsTrue(cut.Events.Any(e => e.Kind == BattleEventKind.Heal && e.ActorId == "h" && e.TargetId == healed.Value.TargetId &&
                    e.Tick == healed.Value.Tick));
                Assert.IsTrue(GloryFor(cut, ChiefAnswer.Heal, healed.Value.Tick));
                return;
            }
            Assert.Fail("in forty fights our Medic never cut a prey out of the net");
        }

        [Test]
        public void ATimedHoldParriesTheKillOrYourWardenStepsInAndTakesIt()
        {
            var parry = Fight("hunt.kill.parry", 85, Plain(), Hunters());
            var ps = new SimSession(parry);
            BattleEvent parried = Next(parry, ps, HuntOutcome, () =>
            {
                if (parry.KillParryable(0) && parry.HuntTicksLeft(0) <= 10 && parry.HoldTicksLeft(0) == 0) Order(ps, "Hold");
            }).Value;
            Assert.AreEqual(Landed(HuntStep.Kill, ChiefKits.Parried), parried.Detail);
            Assert.IsTrue(parry.Events.Any(e => e.Kind == BattleEventKind.Parry && e.ActorSide == 0 && e.TargetId == "captain" &&
                e.Tick == parried.Tick), "he staggers");
            Assert.IsFalse(parry.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.KillDetail));
            Assert.IsTrue(GloryFor(parry, ChiefAnswer.Parry, parried.Tick));
            Assert.IsTrue(parry.Events.Any(e => e.Kind == BattleEventKind.Line && e.ActorId == "captain" && e.Detail == ChiefKits.ParryLine));

            // Our Warden covers our hurt Marksman, so a prey of ours he does not cover gets netted, and he steps in for them.
            for (ulong seed = 1; seed < 40; seed++)
            {
                var guard = Fight("hunt.kill.guard", seed, Side(U("g", UnitClass.Warden), U("m", UnitClass.Marksman, 100, 100),
                    U("s", UnitClass.Skirmisher)), Hunters());
                BattleEvent? land = Next(guard, new SimSession(guard), e => e.Kind == BattleEventKind.Land &&
                    e.Detail.StartsWith("Kill.", StringComparison.Ordinal));
                if (land == null || land.Value.Detail != Landed(HuntStep.Kill, ChiefKits.Guarded)) continue;
                Assert.IsTrue(guard.Events.Any(e => e.Kind == BattleEventKind.Guard && e.ActorId == "g" && e.TargetId == land.Value.TargetId &&
                    e.Tick == land.Value.Tick));
                Assert.IsTrue(guard.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.KillDetail && e.TargetId == "g" &&
                    e.Tick == land.Value.Tick), "he takes it");
                Assert.IsTrue(GloryFor(guard, ChiefAnswer.Guard, land.Value.Tick));
                return;
            }
            Assert.Fail("in forty fights our Warden never stepped in for a netted prey");
        }

        // ------------------------------------------------------------ the Pack, Turned Hunt and the end

        [Test]
        public void AtHalfHealthThePackRunsTwoHuntsAtOnce()
        {
            // No Warden of his, so nothing covers him from our blades.
            var sim = Fight("hunt.pack", 86, Side(U("a", UnitClass.Skirmisher, 300), U("b", UnitClass.Skirmisher, 300),
                U("c", UnitClass.Skirmisher, 300), U("m", UnitClass.Marksman)), Side(U("x", UnitClass.Skirmisher), U("t", UnitClass.Trapper),
                Captain(300)));
            var session = new SimSession(sim);
            BattleEvent phase = Next(sim, session, e => e.Kind == BattleEventKind.Phase, () =>
            {
                if (sim.FocusTicksLeft(0) == 0) Order(session, "FocusFire", "captain");
            }).Value;
            Assert.AreEqual(ChiefKits.PhaseName(ChiefKit.Hunt), phase.Detail);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Line && e.Detail == ChiefKits.PackLine && e.Tick == phase.Tick));
            Assert.AreEqual(string.Empty, ChiefKits.Line(ChiefKit.Hunt, ChiefKits.PackLine, false), "his words are Jacob's to write");
            Assert.IsTrue(sim.PackOn);
            Assert.AreEqual(2, sim.HuntCount);
            for (int i = 0; i < 2 && !sim.IsFinished; i++) session.Tick();
            Assert.AreNotEqual(HuntStep.None, sim.HuntStepOf(1), "the second hunt is on");
            Assert.AreNotEqual(sim.HuntPreyId(0), sim.HuntPreyId(1), "on a second prey");
        }

        [Test]
        public void BreakEachStepWithADifferentAnswerAndTheHuntTurnsOnHim()
        {
            // We break each step not yet broken in this run of breaks and let the rest pass; our Warden covers our Medic, so a
            // Track breaks only when she is the prey or she heals one over half.
            for (ulong seed = 1; seed < 30; seed++)
            {
                var sim = Fight("hunt.turned", seed, Side(U("g", UnitClass.Warden), U("s", UnitClass.Skirmisher), U("r", UnitClass.Skirmisher),
                    U("h", UnitClass.Medic)), Hunters());
                var session = new SimSession(sim);
                var done = new List<HuntStep>();
                var used = new List<ChiefAnswer>();
                int seen = 0;
                BattleEvent? turned = Next(sim, session, e => e.Kind == BattleEventKind.Combo && e.Detail == ChiefKits.TurnedHunt, () =>
                {
                    for (; seen < sim.Events.Count; seen++) Mirror(sim.Events[seen], done, used);
                    HuntStep step = sim.HuntStepOf(0);
                    if (step == HuntStep.Drive && !done.Contains(HuntStep.Drive) && sim.HoldTicksLeft(0) == 0) Order(session, "Hold");
                    else if (step == HuntStep.Corner && !done.Contains(HuntStep.Corner) && sim.FocusTicksLeft(0) == 0 && sim.HuntNetterId(0).Length > 0)
                        Order(session, "FocusFire", sim.HuntNetterId(0));
                    else if (step == HuntStep.Kill && !done.Contains(HuntStep.Kill) && sim.HuntTicksLeft(0) <= 10 && sim.HoldTicksLeft(0) == 0)
                        Order(session, "Hold");
                });
                if (turned == null) continue;
                Assert.AreEqual(0, turned.Value.ActorSide);
                Assert.AreEqual("captain", turned.Value.TargetId);
                Assert.Greater(sim.TurnedTicksLeft, 0);
                Assert.IsNull(sim.CoveredBy(sim.FindById("captain")), "his band cannot cover him");
                Assert.AreEqual(HuntStep.None, sim.HuntStepOf(0));
                for (int i = 0; i < 30 && !sim.IsFinished; i++) session.Tick();
                Assert.AreEqual(HuntStep.None, sim.HuntStepOf(0), "his hunts wait while it turns");
                return;
            }
            Assert.Fail("in thirty fights the hunt never turned");
        }

        // Turned Hunt's count, read back from the Land events: a step broken that was not yet, with an answer not yet used,
        // adds to it; anything else starts it over; a Kill that lands clears it.
        private static void Mirror(BattleEvent e, List<HuntStep> done, List<ChiefAnswer> used)
        {
            if (!IsHuntLand(e)) return;
            string[] parts = e.Detail.Split('.');
            HuntStep step = parts[0] == "Track" ? HuntStep.Track : parts[0] == "Drive" ? HuntStep.Drive : parts[0] == "Corner" ? HuntStep.Corner : HuntStep.Kill;
            if (parts[1] == ChiefKits.Full)
            {
                if (step == HuntStep.Kill)
                {
                    done.Clear();
                    used.Clear();
                }
                return;
            }
            ChiefAnswer answer = parts[1] == ChiefKits.Guarded ? ChiefAnswer.Guard : parts[1] == ChiefKits.Healed ? ChiefAnswer.Heal
                : parts[1] == ChiefKits.Braced ? ChiefAnswer.Brace : parts[1] == ChiefKits.Torn ? ChiefAnswer.Smash
                : parts[1] == ChiefKits.Parried ? ChiefAnswer.Parry : ChiefAnswer.Break;
            if (done.Contains(step) || used.Contains(answer))
            {
                done.Clear();
                used.Clear();
            }
            done.Add(step);
            used.Add(answer);
        }

        [Test]
        public void TheHuntersNeverKneelTheirNerveBreaks()
        {
            // His captain is too tough to fall first; our Unleash and blades break their nerve.
            var sim = Fight("hunt.break", 88, Side(U("a", UnitClass.Skirmisher, 300), U("b", UnitClass.Skirmisher, 300),
                U("c", UnitClass.Skirmisher, 300), U("m", UnitClass.Marksman, 300)), Side(U("w", UnitClass.Warden, 100, 100),
                U("x", UnitClass.Skirmisher, 100, 100), Captain(5000)), nerve: true);
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                if (sim.UnleashTicksLeft(0) == 0) Order(session, "Unleash");
                session.Tick();
            }
            Assert.IsTrue(sim.IsFinished);
            Assert.AreEqual(0, sim.Winner);
            Assert.AreEqual(1, sim.BrokenSide);
            Assert.AreEqual(FightEnd.NerveBreak, sim.Ending.How);
            Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Yield), "no kneel: the hunters never join");
            Assert.IsTrue(sim.FindById("captain").Alive);
        }

        // ------------------------------------------------------------ the campaign

        private static EncounterResult PlayToTheHunt(Campaign c, ulong seed)
        {
            var picker = new CampaignPilot(PilotStyle.Reference, seed);
            for (ulong n = 0; c.Stage == CampaignStage.InRun; n++)
            {
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(picker.PickOffer(c)),
                    new CampaignPilot(PilotStyle.Reference, seed * 31UL + n));
                if (c.Stage == CampaignStage.InRun && c.Target == AlphaHunt.Tribe) return r;
            }
            return null;
        }

        [Test]
        public void TheRunsLastFightIsTheHuntAndItSavesAndPlaysBackExactly()
        {
            var t = new CampaignTuning { EnemyScaleTier1 = 50, EnemyScaleTier2 = 50 };
            for (ulong seed = 510; seed < 560; seed++)
            {
                Campaign c = Campaign.New(seed, t);
                Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
                c.StartRun();
                if (PlayToTheHunt(c, seed) == null) continue;
                CampaignFight fight = c.StartEncounter(0);
                Assert.AreEqual(Campaign.RulesVersion, fight.RulesVersion);
                Assert.IsTrue(fight.Setup.Fight.ChiefTactics);
                Assert.AreEqual(ChiefKit.Hunt, fight.Sim.Kit(1));
                Assert.IsTrue(fight.Sim.HuntOn);
                Assert.Greater(fight.Setup.Rules(1).AutoEclipseTicks, 0, "his big hit you can see coming stays (call J2)");
                Assert.IsTrue(fight.Sim.GloryOn(0));

                // Once the hunt is on, a save holds the fight exactly.
                var pilot = new CampaignPilot(PilotStyle.Reference, seed);
                while (!fight.Over && fight.Sim.CurrentTick < ChiefKits.HuntStartTicks + 60)
                {
                    pilot.Step(fight);
                    fight.Tick();
                }
                if (fight.Over) continue;
                Assert.AreNotEqual(HuntStep.None, fight.Sim.HuntStepOf(0), "the hunt is on");
                Campaign half = CampaignSave.Read(CampaignSave.Write(c), t);
                Assert.IsEmpty(half.LoadNote);
                Assert.AreEqual(fight.Session.Checksum, half.Active.Session.Checksum);

                // Played out, the recorded fight plays back exactly, and a win is graded on the hunt captain's codex page.
                EncounterResult r = CampaignPilot.PlayFight(c, fight, pilot);
                Assert.IsTrue(c.Archive.TryReplay(fight.Setup.FightId, out _, out string failure), failure);
                if (r.Won)
                {
                    Assert.IsTrue(r.Graded);
                    Assert.IsTrue(c.BestGrade(AlphaHunt.Id, out GloryRank best));
                    Assert.AreEqual(r.Grade, best);
                }
                return;
            }
            Assert.Fail("the reference pilot never reached the hunt in fifty campaigns");
        }
    }
}
