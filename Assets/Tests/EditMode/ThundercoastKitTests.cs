using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The Thundercoast chief's kit, the tide's clock (Jacob, 22 Oct 2026, later: "go, build the Stoneveil and Thundercoast
    /// chief kits in the sim and hand the API to Codex"; DESIGN_LOCKS "The Stoneveil and Thundercoast chiefs"). One quick test
    /// per piece, only to prove it runs. Nothing here is tuning.
    public class ThundercoastKitTests
    {
        // A fighter at these percents of power and health, in its class's row. Tough by default, so fights last.
        private static UnitSpec U(string id, UnitClass unitClass, int power = 100, int health = 1000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, power, health);
        }

        // He leads from the front, a Warden, as in the campaign.
        private static UnitSpec Chief(int health = 1000)
        {
            return new UnitSpec("chief", UnitClass.Warden, Row.Front, UnitRole.Chief, 100, health);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        // His side floods the fight and plays his kit with these moves and no heavy swings, under the campaign's chief rules;
        // ours has a grade 1 Eclipse on its 5th button, and combos. No nerve, so nothing breaks.
        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, ChiefMove moves)
        {
            var kit = new SideRules(2, (moves & ChiefMove.Tide) != 0 ? ChiefKits.TideEveryTicks : 0, false, 0, ChiefSpell.Tide, flood: true,
                kit: ChiefKit.Thundercoast, moves: moves);
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.Off, 10, 400, new SideRules(1, 0, true), kit,
                FightRules.CampaignChiefTactics));
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

        private static void TickTo(SimSession session, BattleSimulation sim, long tick)
        {
            while (sim.CurrentTick < tick && !sim.IsFinished) session.Tick();
        }

        private static string Landed(string move, string how)
        {
            return move + "." + how;
        }

        private static bool Land(BattleEvent e)
        {
            return e.Kind == BattleEventKind.Land;
        }

        private static bool GloryAt(BattleSimulation sim, ChiefAnswer answer, long tick)
        {
            return sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.Name(answer) && e.Tick == tick);
        }

        private static bool EatenAt(BattleSimulation sim, long tick)
        {
            return sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.EatenDetail && e.Tick == tick);
        }

        private static List<BattleEvent> Hits(BattleSimulation sim, string detail)
        {
            return sim.Events.Where(e => e.Kind == BattleEventKind.Hit && e.Detail == detail).ToList();
        }

        private static int Count(ChiefMove moves)
        {
            int n = 0;
            for (int bit = 0; bit < 30; bit++)
                if (((int)moves & (1 << bit)) != 0) n++;
            return n;
        }

        // ------------------------------------------------------------ the tide's clock

        [Test]
        public void InHisFightTheWaterRisesAndFallsOnAClockAndLowWaterStrandsHisSide()
        {
            var sim = Fight("tc.clock", 91, Side(U("a", UnitClass.Warden), U("m", UnitClass.Marksman)), Side(U("y", UnitClass.Marksman), Chief()),
                ChiefMove.None);
            var session = new SimSession(sim);
            Assert.IsTrue(sim.TideOn);
            Assert.AreEqual(1, sim.TideSide);
            Assert.AreEqual(ChiefKit.Thundercoast, sim.Kit(1));
            Assert.IsTrue(sim.Floods(1));
            Assert.IsFalse(sim.Floods(0));

            // Low water at the start: his side is stranded, and high water is three steps away.
            Assert.AreEqual(0, sim.FloodLevel);
            Assert.IsTrue(sim.Stranded(1));
            Assert.IsFalse(sim.Stranded(0));
            Assert.AreEqual(ChiefKits.HighStep * FloodRules.StepTicks, sim.TideTicksToHigh);
            Assert.AreEqual(0, sim.TideTicksToLow);

            // A step every FloodRules.StepTicks: up to high water and back down to low, each told by a Tide event.
            TickTo(session, sim, ChiefKits.TideCycleTicks + 10);
            List<BattleEvent> tides = sim.Events.Where(e => e.Kind == BattleEventKind.Tide).ToList();
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 2, 1, 0 }, tides.Select(e => e.Amount).ToList());
            CollectionAssert.AreEqual(Enumerable.Range(1, ChiefKits.TideSteps).Select(s => (long)s * FloodRules.StepTicks - 1).ToList(),
                tides.Select(e => e.Tick).ToList());
            Assert.IsTrue(tides.All(e => e.Detail == ChiefKits.TideClock && e.ActorSide == 1 && e.ActorId == "chief"));
            Assert.AreEqual(0, sim.FloodLevel);
            Assert.IsTrue(sim.Stranded(1), "low water again");
        }

        // ------------------------------------------------------------ Tide

        [Test]
        public void HisTideDrownsYourBackRowHarderTheHigherTheWaterAndFallBackGetsClear()
        {
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Warden), U("m1", UnitClass.Marksman), U("m2", UnitClass.Marksman));
            List<UnitSpec> Theirs() => Side(U("y", UnitClass.Marksman), Chief());

            var sim = Fight("tc.tide", 92, Ours(), Theirs(), ChiefMove.Tide);
            var session = new SimSession(sim);
            BattleEvent windup = Next(sim, session, e => e.Kind == BattleEventKind.Windup && e.ActorId == "chief").Value;
            Assert.AreEqual(ChiefKits.TideEveryTicks, windup.Tick);
            Assert.AreEqual("Tide", windup.Detail);
            Assert.AreEqual("Tide", ChiefKits.Name(ChiefMove.Tide));
            BattleEvent release = Next(sim, session, e => e.Kind == BattleEventKind.Release).Value;
            int level = sim.Events.Where(e => e.Kind == BattleEventKind.Tide && e.Tick < release.Tick).Select(e => e.Amount).LastOrDefault();
            Assert.Greater(level, 0);
            int percent = Moves.EclipsePercent(2) * (level + 1) * ChiefKits.TidePercentPerLevel / 100;
            Assert.AreEqual(percent, release.Amount, "it hits harder the higher the water");
            List<BattleEvent> drowned = Hits(sim, ChiefSpells.TideDetail);
            CollectionAssert.AreEquivalent(new[] { "m1", "m2" }, drowned.Select(e => e.TargetId).ToList(), "our back row");
            Assert.IsTrue(drowned.All(e => e.Tick == release.Tick && e.Amount == sim.FindById(e.TargetId).MaxHp * percent / 100));
            Assert.IsTrue(EatenAt(sim, release.Tick));

            // Dodged: Fall Back as it comes.
            var dodge = Fight("tc.tide.dodge", 92, Ours(), Theirs(), ChiefMove.Tide);
            var ds = new SimSession(dodge);
            BattleEvent dodged = Next(dodge, ds, e => e.Kind == BattleEventKind.Release, () =>
            {
                if (dodge.WindupTicks(1) > 0 && dodge.WindupTicks(1) <= 20 && dodge.FallBackTicksLeft(0) == 0) Order(ds, "FallBack");
            }).Value;
            Assert.AreEqual(Moves.Dodged, dodged.Detail);
            Assert.IsEmpty(Hits(dodge, ChiefSpells.TideDetail));
            Assert.AreEqual(ChiefKits.DodgeGlory, dodge.Glory(0));
        }

        // ------------------------------------------------------------ Sluice Gate

        [Test]
        public void HisEngineerCranksASluiceGateAndTheWaterJumpsALevelUnlessYouJamItOrSlamItShut()
        {
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Warden), U("m", UnitClass.Marksman));
            List<UnitSpec> Theirs() => Side(U("eng", UnitClass.Trapper), Chief());

            // Cranked: the water jumps a level and the next low water comes late.
            var sim = Fight("tc.sluice", 93, Ours(), Theirs(), ChiefMove.SluiceGate);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.SluiceName, tell.Detail);
            Assert.AreEqual("eng", tell.ActorId, "his engineer, his band's Trapper");
            Assert.AreEqual(ChiefKits.SluiceEveryTicks, tell.Tick);
            Assert.AreEqual(ChiefKits.CrankTicks, tell.Amount);
            Assert.AreEqual("eng", sim.SluiceEngineerId(1));
            int level = sim.FloodLevel;
            int lowBefore = sim.TideTicksToLow;
            BattleEvent land = Next(sim, session, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.SluiceName, ChiefKits.Full), land.Detail);
            Assert.AreEqual(tell.Tick + ChiefKits.CrankTicks - 1, land.Tick);
            Assert.AreEqual(level + 1, land.Amount);
            Assert.AreEqual(level + 1, sim.FloodLevel);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Tide && e.Detail == ChiefKits.TideGate && e.Amount == level + 1 &&
                e.Tick == land.Tick));
            Assert.AreEqual(1, sim.TideGateLevels);
            Assert.AreEqual(lowBefore - (land.Tick - tell.Tick) + ChiefKits.SluiceHoldTicks, sim.TideTicksToLow, "low water comes 10 s late");
            Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Tick == land.Tick), "nothing eaten, nothing answered");

            // Jammed: Focus Fire his engineer while it grinds.
            var jam = Fight("tc.sluice.jam", 93, Ours(), Theirs(), ChiefMove.SluiceGate);
            var js = new SimSession(jam);
            Next(jam, js, e => e.Kind == BattleEventKind.Tell);
            Assert.IsTrue(Order(js, "FocusFire", "eng"));
            BattleEvent jammed = Next(jam, js, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.SluiceName, ChiefKits.Broken), jammed.Detail);
            Assert.IsTrue(GloryAt(jam, ChiefAnswer.Break, jammed.Tick));
            Assert.AreEqual(0, jam.TideGateLevels);

            // Slammed shut: Unleash while it grinds.
            var slam = Fight("tc.sluice.slam", 93, Ours(), Theirs(), ChiefMove.SluiceGate);
            var ss = new SimSession(slam);
            Next(slam, ss, e => e.Kind == BattleEventKind.Tell);
            Assert.IsTrue(Order(ss, "Unleash"));
            BattleEvent slammed = slam.Events.Last(Land);
            Assert.AreEqual(Landed(ChiefKits.SluiceName, ChiefKits.Smashed), slammed.Detail);
            Assert.IsTrue(GloryAt(slam, ChiefAnswer.Smash, slammed.Tick));
            Assert.AreEqual(0, slam.SluiceTicksLeft(1));
        }

        // ------------------------------------------------------------ Riptide

        [Test]
        public void TheRiptidePullsYouIntoTheDeepIfYouFallBackUnlessYouHold()
        {
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Warden), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman));
            List<UnitSpec> Theirs() => Side(U("y", UnitClass.Marksman), Chief());

            // Pulled: Fall Back while it runs.
            var sim = Fight("tc.rip", 94, Ours(), Theirs(), ChiefMove.Riptide);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.RiptideName, tell.Detail);
            Assert.AreEqual(ChiefKits.RiptideEveryTicks, tell.Tick);
            Assert.AreEqual(ChiefKits.RiptideTicks, tell.Amount);
            Assert.IsTrue(Order(session, "FallBack"));
            BattleEvent pulled = Next(sim, session, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.RiptideName, ChiefKits.Full), pulled.Detail);
            Assert.AreEqual(tell.Tick + 1, pulled.Tick, "the moment we fell back");
            List<BattleEvent> hits = Hits(sim, ChiefKits.RiptideDetail);
            CollectionAssert.AreEquivalent(new[] { "a", "b", "m" }, hits.Select(e => e.TargetId).ToList());
            Assert.IsTrue(hits.All(e => e.Amount == sim.FindById(e.TargetId).MaxHp * ChiefKits.RiptidePercent / 100));
            Assert.AreEqual(hits.Sum(e => e.Amount), pulled.Amount);
            Assert.IsTrue(sim.Units.Where(u => u.Side == 0).All(u => u.StunTicks == ChiefKits.PulledTicks - 1), "doing nothing for 2 s");
            Assert.IsTrue(EatenAt(sim, pulled.Tick));
            Assert.AreEqual(0, sim.RiptideTicksLeft(1), "it is spent");

            // Stood fast: Hold, then Fall Back.
            var hold = Fight("tc.rip.hold", 94, Ours(), Theirs(), ChiefMove.Riptide);
            var hs = new SimSession(hold);
            BattleEvent runs = Next(hold, hs, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.IsTrue(Order(hs, "Hold"));
            Assert.IsTrue(Order(hs, "FallBack"));
            BattleEvent braced = Next(hold, hs, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.RiptideName, ChiefKits.Braced), braced.Detail);
            Assert.AreEqual(runs.Tick + ChiefKits.RiptideTicks - 1, braced.Tick);
            Assert.IsEmpty(Hits(hold, ChiefKits.RiptideDetail));
            Assert.IsTrue(GloryAt(hold, ChiefAnswer.Brace, braced.Tick));

            // Nobody fell back and nobody held: nothing came of it.
            var calm = Fight("tc.rip.calm", 94, Ours(), Theirs(), ChiefMove.Riptide);
            BattleEvent clear = Next(calm, new SimSession(calm), Land).Value;
            Assert.AreEqual(Landed(ChiefKits.RiptideName, ChiefKits.Clear), clear.Detail);
            Assert.AreEqual(0, calm.Glory(0));
        }

        // ------------------------------------------------------------ Tidal Bore

        [Test]
        public void TheTidalBoreRunsLaneByLaneUntilAWardenBreaksItOrYouFallBackOrHold()
        {
            List<UnitSpec> Theirs() => Side(U("y", UnitClass.Marksman), Chief());

            // Eaten: it reaches our front row's ends and our back row's middle in turn, from the end it started at.
            var sim = Fight("tc.bore", 95, Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman)), Theirs(),
                ChiefMove.TidalBore);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.BoreName, tell.Detail);
            Assert.AreEqual(ChiefKits.BoreEveryTicks, tell.Tick);
            Assert.AreEqual(ChiefKits.BoreTicks, tell.Amount);
            bool fromRight = sim.BoreFromRight(1);
            BattleEvent land = Next(sim, session, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.BoreName, ChiefKits.Full), land.Detail);
            Assert.AreEqual(tell.Tick + ChiefKits.BoreTicks - 1, land.Tick);
            List<BattleEvent> hits = Hits(sim, ChiefKits.BoreDetail);
            CollectionAssert.AreEqual(fromRight ? new[] { "b", "m", "a" } : new[] { "a", "m", "b" }, hits.Select(e => e.TargetId).ToList());
            CollectionAssert.AreEqual(new[] { 0L, 29L, 59L }.Select(d => tell.Tick + d).ToList(), hits.Select(e => e.Tick).ToList(),
                "lane by lane over 2 s");
            Assert.IsTrue(hits.All(e => e.Amount == sim.FindById(e.TargetId).MaxHp * ChiefKits.BorePercent / 100));
            Assert.IsTrue(EatenAt(sim, land.Tick));

            // Broken: a Warden at each end of our front row, so it breaks on the first it reaches.
            var wall = Fight("tc.bore.warden", 95, Side(U("w1", UnitClass.Warden), U("a", UnitClass.Skirmisher), U("w2", UnitClass.Warden),
                U("m", UnitClass.Marksman)), Theirs(), ChiefMove.TidalBore);
            var ws = new SimSession(wall);
            BattleEvent broke = Next(wall, ws, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.BoreName, ChiefKits.Guarded), broke.Detail);
            Assert.AreEqual(wall.BoreFromRight(1) ? "w2" : "w1", broke.TargetId);
            List<BattleEvent> wallHits = Hits(wall, ChiefKits.BoreDetail);
            Assert.AreEqual(1, wallHits.Count, "nobody past him is touched");
            Assert.AreEqual(broke.TargetId, wallHits[0].TargetId);
            Assert.IsTrue(GloryAt(wall, ChiefAnswer.Guard, broke.Tick));

            // Dodged: Fall Back as it starts.
            var dodge = Fight("tc.bore.dodge", 95, Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman)),
                Theirs(), ChiefMove.TidalBore);
            var ds = new SimSession(dodge);
            TickTo(ds, dodge, ChiefKits.BoreEveryTicks - 5);
            Assert.IsTrue(Order(ds, "FallBack"));
            BattleEvent dodged = Next(dodge, ds, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.BoreName, ChiefKits.Dodged), dodged.Detail);
            Assert.IsEmpty(Hits(dodge, ChiefKits.BoreDetail));
            Assert.AreEqual(ChiefKits.DodgeGlory, dodge.Glory(0));

            // Soaked: a Hold as it starts. Half of it lands, and nobody is knocked down.
            var soak = Fight("tc.bore.soak", 95, Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman)),
                Theirs(), ChiefMove.TidalBore);
            var ks = new SimSession(soak);
            TickTo(ks, soak, ChiefKits.BoreEveryTicks - 5);
            Assert.IsTrue(Order(ks, "Hold"));
            BattleEvent soaked = Next(soak, ks, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.BoreName, ChiefKits.Soaked), soaked.Detail);
            List<BattleEvent> soft = Hits(soak, ChiefKits.BoreDetail);
            Assert.AreEqual(3, soft.Count);
            Assert.IsTrue(soft.All(e => e.Amount == soak.FindById(e.TargetId).MaxHp * ChiefKits.BorePercent / 100 * ChiefKits.BoreSoakPercent / 100));
            Assert.IsTrue(GloryAt(soak, ChiefAnswer.Brace, soaked.Tick));
        }

        // ------------------------------------------------------------ Storm Surge

        [Test]
        public void HisHornCallsAStormSurgeThatHoldsTheWaterAtItsHighestUnlessYouBreakTheCall()
        {
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Warden), U("m", UnitClass.Marksman));
            List<UnitSpec> Theirs() => Side(U("y", UnitClass.Marksman), Chief());

            // Eaten: the high water stands 8 s past the clock's.
            var sim = Fight("tc.surge", 96, Ours(), Theirs(), ChiefMove.StormSurge);
            var session = new SimSession(sim);
            BattleEvent horn = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.SurgeName, horn.Detail);
            Assert.AreEqual(ChiefKits.SurgeEveryTicks, horn.Tick);
            Assert.AreEqual(ChiefKits.HornTicks, horn.Amount);
            TickTo(session, sim, horn.Tick + ChiefKits.HornTicks);
            Assert.AreEqual(ChiefKits.SurgeTicks - 1, sim.SurgeTicksLeft);
            Assert.AreEqual(FloodRules.MaxLevel, sim.FloodLevel);
            BattleEvent land = Next(sim, session, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.SurgeName, ChiefKits.Full), land.Detail);
            // The surge starts on the horn's last tick and stands ChiefKits.SurgeTicks from there.
            Assert.AreEqual(horn.Tick + ChiefKits.HornTicks - 1 + ChiefKits.SurgeTicks - 1, land.Tick);
            Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Tide && e.Tick > horn.Tick && e.Tick < land.Tick),
                "the water stood at its highest past the clock's high water");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Tide && e.Detail == ChiefKits.TideSurge && e.Tick == land.Tick &&
                e.Amount < FloodRules.MaxLevel), "then it falls back to the clock");
            Assert.IsTrue(EatenAt(sim, land.Tick));

            // Braced: a Hold during the high water.
            var brace = Fight("tc.surge.hold", 96, Ours(), Theirs(), ChiefMove.StormSurge);
            var bs = new SimSession(brace);
            BattleEvent braced = Next(brace, bs, Land, () =>
            {
                if (brace.SurgeTicksLeft > 0 && brace.HoldTicksLeft(0) == 0 && !brace.Events.Any(e => e.Kind == BattleEventKind.Directive &&
                    e.Detail == "Hold" && e.ActorSide == 0))
                    Order(bs, "Hold");
            }).Value;
            Assert.AreEqual(Landed(ChiefKits.SurgeName, ChiefKits.Braced), braced.Detail);
            Assert.IsTrue(GloryAt(brace, ChiefAnswer.Brace, braced.Tick));

            // Broken: Focus Fire him during the horn.
            var cut = Fight("tc.surge.cut", 96, Ours(), Theirs(), ChiefMove.StormSurge);
            var cs = new SimSession(cut);
            Next(cut, cs, e => e.Kind == BattleEventKind.Tell);
            Assert.IsTrue(Order(cs, "FocusFire", "chief"));
            BattleEvent broken = Next(cut, cs, Land).Value;
            Assert.AreEqual(Landed(ChiefKits.SurgeName, ChiefKits.Broken), broken.Detail);
            Assert.IsTrue(GloryAt(cut, ChiefAnswer.Break, broken.Tick));
            TickTo(cs, cut, ChiefKits.SurgeEveryTicks + ChiefKits.HornTicks + 5);
            Assert.AreEqual(0, cut.SurgeTicksLeft, "no surge came");
        }

        // ------------------------------------------------------------ Spring Tide

        [Test]
        public void AtHalfHealthSpringTideRunsTheClockFasterNeverBelowTheFirstLevelAndHisTideDrownsBothRows()
        {
            // His band has no Warden to cover him, so our heavy blades bring him to half.
            var sim = Fight("tc.spring", 97, Side(U("a", UnitClass.Skirmisher, 300), U("b", UnitClass.Skirmisher, 300),
                U("c", UnitClass.Skirmisher, 300), U("m", UnitClass.Marksman)), Side(U("y", UnitClass.Marksman), Chief()), ChiefMove.Tide);
            var session = new SimSession(sim);
            BattleEvent phase = Next(sim, session, e => e.Kind == BattleEventKind.Phase, () =>
            {
                if (sim.FocusTicksLeft(0) == 0 && sim.DirectivePoints(0) > 0) Order(session, "FocusFire", "chief");
            }).Value;
            Assert.AreEqual("Spring Tide", phase.Detail);
            Assert.AreEqual(ChiefKits.PhaseName(ChiefKit.Thundercoast), phase.Detail);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Line && e.Detail == ChiefKits.SpringTideLine && e.Tick == phase.Tick));
            Assert.IsTrue(sim.SpringTide(1));
            Assert.GreaterOrEqual(sim.FloodLevel, ChiefKits.SpringFloor);

            // He casts his Tide again at once, and it drowns both our rows.
            BattleEvent release = Next(sim, session, e => e.Kind == BattleEventKind.Release && e.ActorId == "chief").Value;
            List<string> drowned = Hits(sim, ChiefSpells.TideDetail).Where(e => e.Tick == release.Tick).Select(e => e.TargetId).ToList();
            Assert.IsTrue(drowned.Contains("m"), "the back row");
            Assert.IsTrue(drowned.Any(id => sim.FindById(id).Row == Row.Front), "and the front row");

            // The clock turns twice as fast and the water never drops below the first level.
            long from = sim.CurrentTick;
            int lowest = FloodRules.MaxLevel;
            while (sim.CurrentTick < from + ChiefKits.TideCycleTicks && !sim.IsFinished)
            {
                session.Tick();
                lowest = Math.Min(lowest, sim.FloodLevel);
            }
            Assert.AreEqual(ChiefKits.SpringFloor, lowest);
            List<long> turns = sim.Events.Where(e => e.Kind == BattleEventKind.Tide && e.Detail == ChiefKits.TideClock && e.Tick > phase.Tick)
                .Select(e => e.Tick).ToList();
            Assert.IsTrue(turns.Zip(turns.Skip(1), (x, y) => y - x).Contains(FloodRules.StepTicks / ChiefKits.SpringSpeed),
                "a step every 5 s");
        }

        // ------------------------------------------------------------ Turn of the Tide, his hidden chain

        [Test]
        public void AnswerThreeOfHisMovesInARowAtHighWaterAndTheTideTurns()
        {
            // A Warden at each end of our front row, so his Tidal Bore always breaks on one.
            var sim = Fight("tc.turn", 98, Side(U("w1", UnitClass.Warden), U("a", UnitClass.Skirmisher), U("w2", UnitClass.Warden),
                U("m", UnitClass.Marksman)), Side(U("eng", UnitClass.Trapper), U("y", UnitClass.Marksman), Chief()),
                ChiefMove.SluiceGate | ChiefMove.StormSurge | ChiefMove.TidalBore);
            var session = new SimSession(sim);
            bool held = false;
            BattleEvent turn = Next(sim, session, e => e.Kind == BattleEventKind.Combo && e.Detail == ChiefKits.TurnOfTheTide, () =>
            {
                // We jam every Sluice Gate, so the clock never holds; we let his horn sound, and Hold once while the surge stands.
                if (sim.SluiceTicksLeft(1) > 0 && sim.FocusedId(0) != "eng") Order(session, "FocusFire", "eng");
                if (!held && sim.SurgeTicksLeft > 0 && sim.SurgeTicksLeft < 200 && sim.HoldTicksLeft(0) == 0) held = Order(session, "Hold");
            }).Value;
            Assert.AreEqual(ChiefKits.TurnOfTheTide, ChiefKits.ChainName(ChiefKit.Thundercoast));
            Assert.AreEqual(0, turn.ActorSide);
            Assert.AreEqual("chief", turn.TargetId);
            Assert.AreEqual(ChiefKits.DrainTicks, turn.Amount);

            // The three answers, each given while the water stood at its highest: the gate jammed, the bore broken on a Warden,
            // the surge braced.
            List<BattleEvent> answers = sim.Events.Where(e => e.Kind == BattleEventKind.Glory && e.ActorSide == 0).ToList();
            CollectionAssert.AreEqual(new[] { ChiefAnswer.Break, ChiefAnswer.Guard, ChiefAnswer.Brace }.Select(ChiefKits.Name).ToList(),
                answers.Skip(answers.Count - 3).Select(e => e.Detail).ToList());
            Assert.AreEqual(turn.Tick, answers.Last().Tick);

            // The tide turns: drained to nothing, so his side is stranded.
            Assert.AreEqual(0, sim.FloodLevel);
            Assert.IsTrue(sim.Stranded(1));
            Assert.Greater(sim.DrainTicksLeft, 0);
            Assert.AreEqual(0, sim.SurgeTicksLeft, "the surge ended with it");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Tide && e.Detail == ChiefKits.TideDrain && e.Amount == 0 && e.Tick == turn.Tick));
            TickTo(session, sim, turn.Tick + ChiefKits.DrainTicks + 1);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Tide && e.Detail == ChiefKits.TideDrain && e.Amount > 0 && e.Tick > turn.Tick),
                "and comes back when it ends");
        }

        // ------------------------------------------------------------ the campaign

        [Test]
        public void TheThundercoastChiefFightsWithHisKitAndHisFightSavesAndPlaysBackExactly()
        {
            var t = new CampaignTuning { PathFights = 0 };
            for (ulong seed = 4700; seed < 4740; seed++)
            {
                Campaign c = Campaign.New(seed, t);
                Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
                c.ChooseTribe(CampaignTribes.ThundercoastId);
                Assert.AreEqual(EncounterKind.Chief, c.Offers[0].Kind);
                CampaignFight fight = c.StartEncounter(0);
                Assert.AreEqual(Campaign.RulesVersion, fight.RulesVersion);
                Assert.IsTrue(fight.Setup.Fight.ChiefTactics);
                Assert.AreEqual(ChiefKit.Thundercoast, fight.Sim.Kit(1));
                Assert.IsTrue(fight.Sim.TideOn);
                ChiefMove moves = fight.Sim.KitMoves(1);
                Assert.AreEqual(c.ChiefMovesThisRun(CampaignTribes.Thundercoast), moves);
                Assert.AreEqual(ChiefKits.Takes, Count(moves), "three of his five");
                Assert.AreEqual(moves, moves & ChiefKits.Pool(ChiefKit.Thundercoast));
                Assert.AreEqual((moves & ChiefMove.Tide) != 0 ? ChiefKits.TideEveryTicks : 0, fight.Setup.Rules(1).AutoEclipseTicks);
                Assert.AreEqual(string.Empty, fight.ChiefLine(ChiefKits.KneelLine), "his words are Jacob's to write");

                // Halfway the save holds the fight exactly.
                var pilot = new CampaignPilot(PilotStyle.Reference, seed);
                while (!fight.Over && fight.Sim.CurrentTick < ChiefKits.BoreEveryTicks + 30)
                {
                    pilot.Step(fight);
                    fight.Tick();
                }
                if (fight.Over) continue;
                Campaign half = CampaignSave.Read(CampaignSave.Write(c), t);
                Assert.IsEmpty(half.LoadNote);
                Assert.AreEqual(fight.Session.Checksum, half.Active.Session.Checksum);

                // Played out, you have met him, and the recorded fight plays back exactly.
                CampaignPilot.PlayFight(c, fight, pilot);
                Assert.IsTrue(c.MetChief(CampaignTribes.ThundercoastId), "you have met him now, win or lose");
                Assert.IsTrue(c.Archive.TryReplay(fight.Setup.FightId, out _, out string failure), failure);
                return;
            }
            Assert.Fail("no Thundercoast chief fight lasted past his first Tidal Bore in forty campaigns");
        }
    }
}
