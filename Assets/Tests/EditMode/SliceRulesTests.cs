using System;
using System.Collections.Generic;
using NUnit.Framework;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Sim.Tests
{
    /// The slice's newer rules (Jacob, 2 and 3 Oct 2026): orders that refill during the fight,
    /// tougher units so a fight lasts longer, and SliceRules putting both together with the nerve
    /// bar. Both switches are off by default, so the older tests keep their numbers.
    public sealed class SliceRulesTests
    {
        // ---------------------------------------------------------------- refilling orders

        [Test]
        public void Refill_GivesAPointBackEveryRefillTicks_UpToTheStartingPoints()
        {
            const int refill = 30;
            var sim = new BattleSimulation(Setup("r", 7, NerveRules.Off, refill, 400));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "Unleash", ""));
            Assert.IsTrue(Issue(session, "Unleash", ""));
            Assert.IsTrue(Issue(session, "Hold", ""));
            Assert.AreEqual(0, sim.DirectivePoints(0));

            for (int i = 0; i < refill - 1; i++) session.Tick();
            Assert.AreEqual(0, sim.DirectivePoints(0), "nothing comes back before the refill tick");
            session.Tick();
            Assert.AreEqual(1, sim.DirectivePoints(0));

            for (int i = 0; i < refill * 10 && !sim.IsFinished; i++) session.Tick();
            Assert.IsFalse(sim.IsFinished, "precondition: the fight is still running");
            Assert.AreEqual(BattleSetup.DefaultDirectivePoints, sim.DirectivePoints(0), "refills stop at the starting points");
            Assert.AreEqual(BattleSetup.DefaultDirectivePoints, sim.DirectivePoints(1), "a side that spends nothing stays full");
        }

        [Test]
        public void NoRefill_SpentPointsNeverComeBack()
        {
            var sim = new BattleSimulation(new BattleSetup("o", 7, Clarth(), Raiders(), new TacticsProfile(), null));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "Unleash", ""));
            for (int i = 0; i < 300 && !sim.IsFinished; i++) session.Tick();
            Assert.AreEqual(BattleSetup.DefaultDirectivePoints - Directives.Cost(Directive.Unleash), sim.DirectivePoints(0));
        }

        // ---------------------------------------------------------------- pace

        [Test]
        public void HealthPercent_ScalesEveryUnitsHealth()
        {
            var sim = new BattleSimulation(Setup("h", 7, NerveRules.Off, 0, 250));
            foreach (BattleUnit u in sim.Units)
            {
                Assert.AreEqual(SliceClasses.Stats(u.Class).MaxHp * 250 / 100, u.MaxHp, u.Id);
                Assert.AreEqual(u.MaxHp, u.Hp, u.Id);
            }
        }

        [Test]
        public void OldDefaults_ChangeNothing()
        {
            var old = new BattleSetup("x", 31, Clarth(), Raiders(), new TacticsProfile(), null);
            Assert.AreEqual(0, old.RefillTicks);
            Assert.AreEqual(BattleSetup.NormalHealthPercent, old.HealthPercent);
            var a = new BattleSimulation(old);
            var b = new BattleSimulation(Setup("x", 31, NerveRules.Off, 0, BattleSetup.NormalHealthPercent));
            a.RunUntilFinished();
            b.RunUntilFinished();
            Assert.AreEqual(a.EventHash, b.EventHash);
            Assert.AreEqual(a.OrderedStateSnapshot, b.OrderedStateSnapshot);
        }

        [Test]
        public void SliceRules_FightsLastLonger_AndThePlayerStaysBusy()
        {
            const int fights = 30;
            double oldSeconds = 0, newSeconds = 0, newOrders = 0;
            for (int i = 0; i < fights; i++)
            {
                ulong seed = 40000UL + (ulong)i;
                BattleSimulation nerveOnly = PlayWardenUnleash(Setup("o" + i, seed, NerveRules.On(), 0, BattleSetup.NormalHealthPercent));
                BattleSimulation full = PlayWardenUnleash(Setup("n" + i, seed, NerveRules.On(), SliceRules.RefillTicks, SliceRules.HealthPercent));
                oldSeconds += nerveOnly.EndTick / (double)SimClock.TicksPerSecond;
                newSeconds += full.EndTick / (double)SimClock.TicksPerSecond;
                newOrders += PlayerOrders(full);
            }
            double oldAverage = oldSeconds / fights;
            double newAverage = newSeconds / fights;
            double perMinute = newOrders / newSeconds * 60;
            TestContext.WriteLine("Focus their Warden and Unleash whenever possible, " + fights + " fights: " +
                oldAverage.ToString("0.0") + " s under the nerve bar alone, " + newAverage.ToString("0.0") +
                " s under the slice rules, with " + perMinute.ToString("0.0") + " orders a minute.");
            Assert.Greater(newAverage, 25.0, "a fight should last well past twenty seconds");
            Assert.Less(newAverage, 90.0, "but not drag on");
            Assert.Greater(newAverage, oldAverage * 2, "and run much longer than under the nerve bar alone");
            Assert.GreaterOrEqual(perMinute, 10.0, "an eager player gives an order every few seconds");
        }

        // ---------------------------------------------------------------- the world

        [Test]
        public void SliceRules_World_IsDeterministic_AndEveryFightReplays()
        {
            int fights = 0;
            int wars = 0;
            for (ulong seed = 21; seed <= 24; seed++)
            {
                SliceWorld a = SliceRules.CreateWorld(seed);
                SliceWorld b = SliceRules.CreateWorld(seed);
                Campaign(a, 3);
                Campaign(b, 3);
                Assert.AreEqual(a.Snapshot(), b.Snapshot(), "seed " + seed);
                foreach (ArchivedFight fight in a.Archive.Fights)
                {
                    Assert.IsTrue(fight.Setup.Nerve.Enabled, fight.FightId);
                    Assert.AreEqual(SliceRules.HealthPercent, fight.Setup.HealthPercent, fight.FightId);
                    if (fight.FightId.EndsWith(".war", StringComparison.Ordinal))
                    {
                        wars++;
                        Assert.AreEqual(0, fight.Setup.RefillTicks, "wars have no orders to refill");
                    }
                    else
                    {
                        Assert.AreEqual(SliceRules.RefillTicks, fight.Setup.RefillTicks, fight.FightId);
                    }
                    Assert.IsTrue(a.Archive.TryReplay(fight.FightId, out _, out string failure), failure);
                    fights++;
                }
            }
            Assert.Greater(fights, 4);
            Assert.Greater(wars, 0, "some wars should have been replayed too");
        }

        // ---------------------------------------------------------------- helpers

        private static List<UnitSpec> Clarth()
        {
            return new List<UnitSpec>
            {
                new UnitSpec("c.0", UnitClass.Warden),
                new UnitSpec("c.1", UnitClass.Marksman),
                new UnitSpec("c.2", UnitClass.Skirmisher),
                new UnitSpec("c.3", UnitClass.Trapper),
                new UnitSpec("c.4", UnitClass.Medic)
            };
        }

        private static List<UnitSpec> Raiders()
        {
            return new List<UnitSpec>
            {
                new UnitSpec("r.0", UnitClass.Skirmisher),
                new UnitSpec("r.1", UnitClass.Warden),
                new UnitSpec("r.2", UnitClass.Marksman),
                new UnitSpec("r.3", UnitClass.Skirmisher),
                new UnitSpec("r.4", UnitClass.Medic)
            };
        }

        private static BattleSetup Setup(string fightId, ulong seed, NerveRules nerve, int refillTicks, int healthPercent)
        {
            return new BattleSetup(fightId, seed, Clarth(), Raiders(), new TacticsProfile(), null,
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, nerve, refillTicks, healthPercent);
        }

        private static bool Issue(SimSession session, string directive, string target)
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        /// An eager player: focus their Warden (or the most hurt once he is down) whenever no focus
        /// is running, and Unleash whenever two points are free and no Unleash is running.
        private static BattleSimulation PlayWardenUnleash(BattleSetup setup)
        {
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            long focusUntil = -1;
            long unleashUntil = -1;
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                long t = sim.CurrentTick;
                int points = sim.DirectivePoints(0);
                if (t >= focusUntil && points >= 1)
                {
                    BattleUnit target = FirstAlive(sim, 1, UnitClass.Warden) ?? MostHurt(sim, 1);
                    if (target != null && Issue(session, "FocusFire", target.Id))
                    {
                        focusUntil = t + BattleSimulation.FocusTicks;
                        points--;
                    }
                }
                if (t >= unleashUntil && points >= 2 && Issue(session, "Unleash", ""))
                    unleashUntil = t + BattleSimulation.UnleashTicks;
                session.Tick();
            }
            return sim;
        }

        private static SimSession PlayNoOrders(BattleSetup setup)
        {
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit) session.Tick();
            return session;
        }

        private static int PlayerOrders(BattleSimulation sim)
        {
            int count = 0;
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == BattleEventKind.Directive && string.Equals(e.ActorId, "side0", StringComparison.Ordinal)) count++;
            }
            return count;
        }

        private static BattleUnit FirstAlive(BattleSimulation sim, int side, UnitClass unitClass)
        {
            foreach (BattleUnit u in sim.Units)
            {
                if (u.Side == side && u.Alive && u.Class == unitClass) return u;
            }
            return null;
        }

        private static BattleUnit MostHurt(BattleSimulation sim, int side)
        {
            BattleUnit best = null;
            foreach (BattleUnit u in sim.Units)
            {
                if (u.Side == side && u.Alive && (best == null || u.HpPermille < best.HpPermille)) best = u;
            }
            return best;
        }

        /// Rival fights and wars. No orders in the first fight, so most worlds also see a war; an
        /// eager player after that.
        private static void Campaign(SliceWorld world, int rounds)
        {
            for (int round = 0; round < rounds && !world.Raiders.Broken; round++)
            {
                BattleSetup setup = world.NextPlayerFight();
                SimSession session;
                if (round == 0)
                {
                    session = PlayNoOrders(setup);
                }
                else
                {
                    var sim = PlayWardenUnleashInSession(setup, out session);
                }
                world.FinishPlayerFight(setup, session);
                if (!world.WarOver) world.RunRaiderWar();
            }
        }

        private static BattleSimulation PlayWardenUnleashInSession(BattleSetup setup, out SimSession session)
        {
            var sim = new BattleSimulation(setup);
            session = new SimSession(sim);
            long focusUntil = -1;
            long unleashUntil = -1;
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                long t = sim.CurrentTick;
                int points = sim.DirectivePoints(0);
                if (t >= focusUntil && points >= 1)
                {
                    BattleUnit target = FirstAlive(sim, 1, UnitClass.Warden) ?? MostHurt(sim, 1);
                    if (target != null && Issue(session, "FocusFire", target.Id))
                    {
                        focusUntil = t + BattleSimulation.FocusTicks;
                        points--;
                    }
                }
                if (t >= unleashUntil && points >= 2 && Issue(session, "Unleash", ""))
                    unleashUntil = t + BattleSimulation.UnleashTicks;
                session.Tick();
            }
            return sim;
        }
    }
}
