using System.Collections.Generic;
using System.Linq;
using System.Text;
using KindredSiege.Sim.Battle;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Danger (Jacob, 31 Oct 2026: "Go, lock and build"; DESIGN_LOCKS, "Jacob's playtest fixes 3, 7 and 9"; FightRules.Danger,
    /// campaign rules 21). One quick test per piece, each against the same fight without danger, only to prove it runs
    /// (Jacob, 12 Oct 2026: build first). Nothing here is tuning.
    public class DangerTests
    {
        private static UnitSpec U(string id, UnitClass unitClass, int health = 3000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, 100, health);
        }

        private static UnitSpec Chief(UnitClass chiefClass, int health = 5000)
        {
            return new UnitSpec("chief", chiefClass, SliceClasses.Stats(chiefClass).DefaultRow, UnitRole.Chief, 100, health);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units) => new List<UnitSpec>(units);

        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, SideRules rules,
            FightRules fight, bool danger)
        {
            FightRules played = fight.WithParryCenter();
            if (danger) played = played.WithDanger();
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.On(NerveRules.Full, NerveRules.Full),
                10, 400, new SideRules(1, 0, true), rules, played));
        }

        private static bool Order(SimSession session, string directive, string target = "", int arg0 = 0)
        {
            return session.TryIssue(Directives.CommandKind, "0", target, arg0, 0, directive, out _);
        }

        // Ticks on until a threat of this name on side 0 is a tick from its beat.
        private static void Beat(SimSession session, BattleSimulation sim, string name)
        {
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                if (sim.Threats(0).Any(t => t.Name == name && t.BeatTicks == 1)) return;
                session.Tick();
            }
            Assert.Fail(name + "'s beat never came");
        }

        private static void PlayOut(BattleSimulation sim)
        {
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit) session.Tick();
        }

        [Test]
        public void AStaleHoldSoaksNothingFromABlast()
        {
            // A Hold given while paused never parries, so it is the stale Hold: it soaks an Eclipse, and from rules 21 nothing.
            foreach (bool danger in new[] { false, true })
            {
                BattleSimulation sim = Fight("blast", 5, Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman)),
                    Side(U("x.s", UnitClass.Skirmisher), Chief(UnitClass.Marksman)), new SideRules(2, 300, false), FightRules.Campaign, danger);
                Assert.AreEqual(danger, sim.DangerOn);
                var session = new SimSession(sim);
                Beat(session, sim, Directives.Name(Directive.Eclipse));
                // The screen's warning: from rules 21 a Hold is no answer to it, a parry and a Fall Back still are.
                Threat eclipse = sim.Threats(0).First(t => t.Name == Directives.Name(Directive.Eclipse) && t.BeatTicks == 1);
                Assert.AreEqual(!danger, (eclipse.Answers & ThreatAnswer.Hold) != 0, "a Hold's place in the Eclipse's answers");
                Assert.AreNotEqual(0, (int)(eclipse.Answers & ThreatAnswer.Parry));
                Assert.AreNotEqual(0, (int)(eclipse.Answers & ThreatAnswer.FallBack));
                Assert.IsTrue(Order(session, "Hold", string.Empty, HeavyRules.PausedOrderArg));
                session.Tick();
                Assert.AreEqual(danger ? Moves.Full : Moves.Soaked, sim.Events.Last(e => e.Kind == BattleEventKind.Release).Detail);
                // What the Hold did, for the screen's receipt: it soaked before rules 21, and from it nothing.
                bool soaked = sim.OrderResults(0).Any(r => r.Order == Directive.Hold && r.Effects.Any(x => x.Kind == OrderEffectKind.Soaked));
                Assert.AreEqual(!danger, soaked, "the Hold's soak in its order result");
            }
        }

        [Test]
        public void RoadBandsCarryFromTheFirstRowAndHitForAThird()
        {
            foreach (bool danger in new[] { false, true })
            {
                List<UnitSpec> ours = Side(U("w", UnitClass.Warden, 9000), U("s", UnitClass.Skirmisher, 9000), U("t", UnitClass.Trapper, 9000),
                    U("m", UnitClass.Marksman, 9000), U("h", UnitClass.Medic, 9000));
                List<UnitSpec> theirs = Side(U("xw", UnitClass.Warden, 9000).WithRoadKit(1, ChiefMove.Cleave | ChiefMove.Brand),
                    U("xs", UnitClass.Skirmisher, 9000), U("xm", UnitClass.Marksman, 9000));
                FightRules road = FightRules.Campaign.WithClearRoad(1);
                if (danger) road = road.WithDanger();
                var sim = new BattleSimulation(new BattleSetup("carry", 9, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                    2400, 8, NerveRules.On(NerveRules.Full, NerveRules.Full), 10, 400, new SideRules(1, 0, true), new SideRules(0, 0, false, 150),
                    road));
                PlayOut(sim);
                List<BattleEvent> cleaves = sim.Events.Where(e => e.Kind == BattleEventKind.Land && e.ActorSide == 1 &&
                    e.ActorId == "xw" && e.Detail == ChiefKits.CleaveName).ToList();
                if (!danger)
                {
                    Assert.IsEmpty(cleaves, "a first row band carried before rules 21");
                    continue;
                }
                Assert.IsNotEmpty(cleaves, "the band carried nothing in the first row");
                int old = CarryRules.CleavePercent * sim.FindById("xw").Power / 100;
                Assert.Greater(cleaves[0].Amount, 5 * old, "a carried Cleave still hits like twice a blow");
            }
        }

        [Test]
        public void TheChiefsCleaveWaitsForABlast()
        {
            // His Cleave every 6 s and his Brand every 15 s: before rules 21 two Cleaves come in a row, from it never.
            foreach (bool danger in new[] { false, true })
            {
                BattleSimulation sim = Fight("turns", 11, Side(U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman), U("t", UnitClass.Trapper)),
                    Side(U("x.s", UnitClass.Skirmisher), Chief(UnitClass.Warden)),
                    new SideRules(2, 0, false, 0, ChiefSpell.Eclipse, kit: ChiefKit.Bloodridge, moves: ChiefMove.Cleave | ChiefMove.Brand),
                    FightRules.CampaignChiefTactics, danger);
                PlayOut(sim);
                var order = new StringBuilder();
                foreach (BattleEvent e in sim.Events)
                {
                    if (e.ActorSide != 1) continue;
                    if (e.Kind == BattleEventKind.Tell && e.Detail == ChiefKits.CleaveName) order.Append('C');
                    if (e.Kind == BattleEventKind.Land && e.Detail.StartsWith(ChiefKits.BrandName)) order.Append('B');
                }
                string seen = order.ToString();
                Assert.GreaterOrEqual(seen.Count(c => c == 'B'), 1, "no Brand came: " + seen);
                if (danger) Assert.IsFalse(seen.Contains("CC"), "two Cleaves came in a row: " + seen);
                else Assert.IsTrue(seen.Contains("CC"), "the fight never showed two Cleaves in a row: " + seen);
            }
        }

        [Test]
        public void TheChiefsBrandHitsHarder()
        {
            // His Brand alone, nobody to cover or heal: the same first Brand, at 20% before rules 21 and 35% from it.
            var first = new int[2];
            for (int n = 0; n < 2; n++)
            {
                BattleSimulation sim = Fight("brand", 3, Side(U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman), U("t", UnitClass.Trapper)),
                    Side(U("x.s", UnitClass.Skirmisher), Chief(UnitClass.Warden)),
                    new SideRules(2, 0, false, 0, ChiefSpell.Eclipse, kit: ChiefKit.Bloodridge, moves: ChiefMove.Brand),
                    FightRules.CampaignChiefTactics, n == 1);
                PlayOut(sim);
                BattleEvent brand = sim.Events.First(e => e.Kind == BattleEventKind.Land && e.ActorSide == 1 &&
                    e.Detail.StartsWith(ChiefKits.BrandName));
                first[n] = brand.Amount;
            }
            Assert.Greater(first[1], first[0], "the Brand is no bigger");
            Assert.AreEqual(DangerRules.ChiefBrandPercent * 100 / ChiefKits.BrandPercent, first[1] * 100 / first[0], 3);
        }
    }
}
