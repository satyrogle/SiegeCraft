using System;
using System.Collections.Generic;
using KindredSiege.Sim.Battle;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Payoffs that spread (Jacob's playtest, 31 Oct 2026: "read log then go build fix 1"; FightRules.Payoffs, campaign
    /// rules 20). One quick test per piece, only to prove each runs (Jacob, 12 Oct 2026: build first). Nothing here is tuning.
    public class PayoffTests
    {
        private static readonly FightRules Before = FightRules.Campaign.WithLayers().WithMarks();
        private static readonly FightRules Payoffs = Before.WithPayoffs();

        // A fighter at four times his health: a Skirmisher is then over the cap, and nobody dies from one shot.
        private static UnitSpec Big(string id, UnitClass unitClass, Row row) =>
            new UnitSpec(id, unitClass, row, UnitRole.Fighter, 100, 400, 0, 0, 0, 0, 0);

        // Three Skirmishers charging in, so they spring her snares.
        private static BattleSimulation Fight(string id, List<UnitSpec> ours, FightRules rules)
        {
            var theirs = new List<UnitSpec>();
            for (int i = 1; i <= 3; i++) theirs.Add(Big("s" + i, UnitClass.Skirmisher, Row.Front));
            return new BattleSimulation(new BattleSetup(id, 7, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.Off, 0, 400, null, null, rules));
        }

        // The index of the first event that matches, ticking on as needed up to this tick; -1 if none.
        private static int RunUntil(BattleSimulation sim, int ticks, Func<BattleEvent, bool> match)
        {
            int seen = 0;
            while (true)
            {
                for (; seen < sim.Events.Count; seen++) if (match(sim.Events[seen])) return seen;
                if (sim.IsFinished || sim.CurrentTick >= ticks) return -1;
                sim.Tick();
            }
        }

        [Test]
        public void ASnarePaysOneCritThenBreaks()
        {
            List<UnitSpec> Squad() => new List<UnitSpec>
                { Big("w", UnitClass.Warden, Row.Front), Big("t", UnitClass.Trapper, Row.Back), Big("a", UnitClass.Marksman, Row.Back) };
            var sim = Fight("snare", Squad(), Payoffs);
            Assert.IsTrue(sim.PayoffsOn);
            int at = RunUntil(sim, 4000, e => e.Kind == BattleEventKind.Hit && e.Detail == ClassSpells.AimedPinDetail);
            Assert.GreaterOrEqual(at, 0, "no Aimed Shot landed on a snared enemy");
            Assert.IsFalse(sim.FindById(sim.Events[at].TargetId).Statuses[StatusKind.Snare].On, "the snare outlived its crit");

            // Below rules 20 the same shot leaves the snare on.
            var old = Fight("snare", Squad(), Before);
            Assert.IsFalse(old.PayoffsOn);
            int then = RunUntil(old, 4000, e => e.Kind == BattleEventKind.Hit && e.Detail == ClassSpells.AimedPinDetail);
            Assert.GreaterOrEqual(then, 0);
            Assert.AreEqual(sim.Events[at].Tick, old.Events[then].Tick, "the two fights parted before the first crit");
            Assert.IsTrue(old.FindById(old.Events[then].TargetId).Statuses[StatusKind.Snare].On);
        }

        [Test]
        public void BleedingCountsAtMostTheCap()
        {
            // A Warden and a Trapper: her snare's wound bleeds, and no Aimed Shot breaks a snare first.
            List<UnitSpec> Squad() => new List<UnitSpec> { Big("w", UnitClass.Warden, Row.Front), Big("t", UnitClass.Trapper, Row.Back) };
            var sim = Fight("bleed", Squad(), Payoffs);
            int at = RunUntil(sim, 4000, e => e.Kind == BattleEventKind.Hit && e.Detail == BleedRules.Detail);
            Assert.GreaterOrEqual(at, 0, "nobody bled");
            BattleEvent tick = sim.Events[at];
            Assert.Greater(sim.FindById(tick.TargetId).MaxHp, PayoffRules.PercentHealthCap);
            Assert.LessOrEqual(tick.Amount, PayoffRules.PercentHealthCap * BleedRules.MaxPercent / 100 + 1);

            // Below rules 20 the same second takes its share of all his health.
            var old = Fight("bleed", Squad(), Before);
            int then = RunUntil(old, 4000, e => e.Kind == BattleEventKind.Hit && e.Detail == BleedRules.Detail);
            Assert.GreaterOrEqual(then, 0);
            Assert.AreEqual(tick.Tick, old.Events[then].Tick, "the two fights parted before the first bleed");
            Assert.Less(tick.Amount, old.Events[then].Amount);
        }
    }
}
