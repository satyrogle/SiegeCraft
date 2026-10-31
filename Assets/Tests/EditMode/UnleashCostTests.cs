using KindredSiege.Sim.Battle;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Call J3 (Jacob, 11 Oct 2026): Unleash drains your own nerve while it runs, at half Hold's rate ("halve the
    /// cost"). Both sides are all melee and yours falls back first, so no blow lands and only the orders move nerve.
    public class UnleashCostTests
    {
        private static BattleSimulation Quiet(params string[] orders)
        {
            var ours = new[] { new UnitSpec("a.warden", UnitClass.Warden), new UnitSpec("a.skirmisher", UnitClass.Skirmisher) };
            var theirs = new[] { new UnitSpec("b.warden", UnitClass.Warden), new UnitSpec("b.skirmisher", UnitClass.Skirmisher) };
            var sim = new BattleSimulation(new BattleSetup("unleash-cost", 9UL, ours, theirs, null, null,
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.On()));
            int sequence = 0;
            foreach (string order in orders)
                Assert.IsTrue(sim.TryApply(new SimCommand(1, sim.CurrentTick, sequence++, Directives.CommandKind, "0", string.Empty, 0, 0, order),
                    out string failure), order + ": " + failure);
            return sim;
        }

        private static int[] Drops(BattleSimulation sim, int ticks)
        {
            var drops = new int[ticks];
            for (int t = 0; t < ticks; t++)
            {
                int before = sim.Nerve(0);
                sim.Tick();
                drops[t] = before - sim.Nerve(0);
            }
            return drops;
        }

        [Test]
        public void UnleashCostsHalfWhatHoldCosts()
        {
            const int Ticks = 60;
            int[] quiet = Drops(Quiet("FallBack"), Ticks);
            int[] hold = Drops(Quiet("FallBack", "Hold"), Ticks);
            BattleSimulation unleashing = Quiet("FallBack", "Unleash");
            int[] unleash = Drops(unleashing, Ticks);
            int holdTotal = 0;
            int unleashTotal = 0;
            for (int t = 0; t < Ticks; t++)
            {
                Assert.AreEqual(0, quiet[t], "falling back alone costs nothing, tick " + t);
                Assert.AreEqual(BattleSimulation.HoldNervePerTick, hold[t], "Hold, tick " + t);
                Assert.IsTrue(unleash[t] == 0 || unleash[t] == BattleSimulation.HoldNervePerTick, "Unleash, tick " + t);
                holdTotal += hold[t];
                unleashTotal += unleash[t];
            }
            Assert.AreEqual(holdTotal / 2, unleashTotal, "Unleash costs half what Hold costs");
            Assert.Greater(unleashing.UnleashTicksLeft(0), 0, "Unleash still ran for the whole window");
            Assert.AreEqual(NerveRules.Full - BattleSimulation.UnleashNerveCost, unleashing.Nerve(1),
                "their nerve took the scare once and nothing else");
        }
    }
}
