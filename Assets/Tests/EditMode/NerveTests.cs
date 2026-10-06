using System;
using System.Collections.Generic;
using NUnit.Framework;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Sim.Tests
{
    /// The nerve bar (Jacob, 2 Oct 2026): hits, deaths and the enemy's scary orders drain a side's
    /// nerve, Hold drains your own, and a side at zero breaks with its survivors alive. A tribe's
    /// nerve carries between fights, and a tribe the player breaks joins the player. The rules sit
    /// behind a switch that is off by default, so the older tests keep their numbers.
    public sealed class NerveTests
    {
        private const int Full = NerveRules.Full;

        // ---------------------------------------------------------------- the switch

        [Test]
        public void NerveOff_NothingChanges()
        {
            var old = new BattleSetup("x", 31, Clarth(), Raiders(), new TacticsProfile(), null);
            BattleSetup off = Setup("x", 31, NerveRules.Off);
            var a = (BattleSimulation)PlayScary(old).Simulation;
            var b = (BattleSimulation)PlayScary(off).Simulation;
            Assert.AreEqual(a.EventHash, b.EventHash);
            Assert.AreEqual(a.OrderedStateSnapshot, b.OrderedStateSnapshot);
            Assert.IsFalse(old.Nerve.Enabled, "the old constructor means the old rules");
            Assert.IsFalse(b.NerveOn);
            Assert.AreEqual(-1, b.BrokenSide);
            Assert.AreEqual(Full, b.Nerve(0));
            Assert.AreEqual(Full, b.Nerve(1));
            Assert.IsFalse(HasEvent(b, BattleEventKind.Break));
            StringAssert.DoesNotContain("nerveOn", b.OrderedStateSnapshot);
            StringAssert.DoesNotContain("nerveRem", b.OrderedStateSnapshot);
        }

        // ---------------------------------------------------------------- drains

        [Test]
        public void Hits_DrainTheVictimsNerve_AndADeathDrainsMore()
        {
            int hits = 0;
            int deaths = 0;
            for (ulong seed = 1; seed <= 10; seed++)
            {
                var sim = new BattleSimulation(Setup("d", seed, NerveRules.On()));
                var startHp = new long[2];
                foreach (BattleUnit u in sim.Units) startHp[u.Side] += u.MaxHp;
                var healed = new long[2];
                int seen = 0;
                while (!sim.IsFinished)
                {
                    sim.Tick();
                    for (; seen < sim.Events.Count; seen++)
                    {
                        BattleEvent e = sim.Events[seen];
                        if (e.Kind == BattleEventKind.Heal) healed[e.ActorSide] += e.Amount;
                        if (e.Kind == BattleEventKind.Hit) hits++;
                        if (e.Kind == BattleEventKind.Kill) deaths++;
                    }
                    for (int side = 0; side < 2; side++)
                    {
                        // Losing the side's whole starting health costs NerveHitPercent of a full bar,
                        // and each death costs NerveDeathCost on top.
                        long lost = healed[side];
                        int dead = 0;
                        foreach (BattleUnit u in sim.Units)
                        {
                            if (u.Side != side) continue;
                            lost += u.MaxHp - u.Hp;
                            if (!u.Alive) dead++;
                        }
                        long expected = Full - lost * BattleSimulation.NerveHitPercent * Full / (100 * startHp[side])
                            - dead * (long)BattleSimulation.NerveDeathCost;
                        Assert.AreEqual(Math.Max(0L, expected), (long)sim.Nerve(side),
                            "seed " + seed + ", tick " + sim.CurrentTick + ", side " + side);
                    }
                }
            }
            Assert.Greater(hits, 0);
            Assert.Greater(deaths, 0, "the check should cover deaths too");
        }

        [Test]
        public void ZeroNerve_BreaksTheSide_TheFightEndsAndSurvivorsLive()
        {
            var sim = new BattleSimulation(Setup("z", 5, NerveRules.On(Full, 1)));
            sim.RunUntilFinished();
            Assert.AreEqual(1, sim.BrokenSide);
            Assert.AreEqual(0, sim.Winner, "the side that did not break wins");
            Assert.AreEqual(0, sim.Nerve(1));
            Assert.Less(sim.EndTick, BattleSetup.DefaultTickLimit - 1, "the fight ends on the spot, not on the clock");
            BattleEvent broke = LastEvent(sim, BattleEventKind.Break);
            Assert.AreEqual(1, broke.ActorSide);
            Assert.AreEqual(sim.AliveCount(1), broke.Amount);
            Assert.Greater(sim.AliveCount(1), 0, "the broken side's survivors live");
            BattleEvent end = LastEvent(sim, BattleEventKind.End);
            Assert.AreEqual(BattleSimulation.BreakDetail, end.Detail);
            Assert.AreEqual(0, end.ActorSide);
        }

        [Test]
        public void Unleash_DrainsTheEnemysNerve()
        {
            var sim = new BattleSimulation(Setup("u", 3, NerveRules.On()));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "Unleash", ""));
            Assert.AreEqual(Full - BattleSimulation.UnleashNerveCost, sim.Nerve(1));
            Assert.AreEqual(Full, sim.Nerve(0), "your own Unleash costs you no nerve");
        }

        [Test]
        public void Sacrifice_ShakesTheEnemy_NotYourOwnSide()
        {
            var sim = new BattleSimulation(Setup("s", 3, NerveRules.On()));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "Sacrifice", "c.4"));
            Assert.IsFalse(sim.FindById("c.4").Alive);
            Assert.AreEqual(Full - BattleSimulation.SacrificeNerveCost, sim.Nerve(1));
            Assert.AreEqual(Full, sim.Nerve(0), "a chosen death costs your own side no nerve");
        }

        [Test]
        public void FearGear_RaisesTheNerveDamageYouDeal()
        {
            var sim = new BattleSimulation(Setup("g", 3, NerveRules.On(Full, Full, 50, 0)));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "Unleash", ""));
            Assert.AreEqual(Full - BattleSimulation.UnleashNerveCost * 150 / 100, sim.Nerve(1));
        }

        [Test]
        public void Hold_DrainsYourOwnNerveWhileItRuns()
        {
            for (ulong seed = 1; seed <= 20; seed++)
            {
                var held = new BattleSimulation(Setup("h", seed, NerveRules.On()));
                var session = new SimSession(held);
                Assert.IsTrue(Issue(session, "Hold", ""));
                session.Tick();
                if (HasEvent(held, BattleEventKind.Hit)) continue; // a hit on the first tick would drain nerve too

                var plain = new BattleSimulation(Setup("h", seed, NerveRules.On()));
                plain.Tick();
                Assert.AreEqual(Full, plain.Nerve(0), "without Hold nothing drains on a quiet first tick");
                Assert.AreEqual(Full - BattleSimulation.HoldNervePerTick, held.Nerve(0));
                Assert.AreEqual(Full, held.Nerve(1), "Hold costs only the side holding");
                return;
            }
            Assert.Fail("no seed with a quiet first tick");
        }

        // ---------------------------------------------------------------- what it does to fights

        [Test]
        public void MedicAndHoldExploit_LosesItsEdgeUnderNerve()
        {
            var cover = new TacticsProfile();
            cover.SetCoverFocused(3);
            const int fights = 100;
            int off = 0;
            int on = 0;
            foreach (TacticsProfile raiders in new[] { new TacticsProfile(), cover })
            {
                for (int i = 0; i < fights; i++)
                {
                    ulong seed = 40000UL + (ulong)i;
                    if (PlayHoldExploit(Setup("m" + i, seed, NerveRules.Off, raiders)).Winner == 0) off++;
                    if (PlayHoldExploit(Setup("m" + i, seed, NerveRules.On(), raiders)).Winner == 0) on++;
                }
            }
            TestContext.WriteLine("Focus their Medic and Hold back to back, " + (2 * fights) + " fights against untaught and covering raiders: won " +
                off + " under the old rules, " + on + " under nerve.");
            Assert.Less(on, off, "holding should cost the trick its edge");
            Assert.Less(on, fights, "under nerve the trick should lose more fights than it wins");
        }

        [Test]
        public void ABreak_LeavesMoreSurvivorsThanAWipe()
        {
            int offLoserSurvivors = 0;
            int onLoserSurvivors = 0;
            int breaks = 0;
            for (ulong seed = 1; seed <= 30; seed++)
            {
                var off = new BattleSimulation(Setup("b", seed, NerveRules.Off));
                var on = new BattleSimulation(Setup("b", seed, NerveRules.On()));
                off.RunUntilFinished();
                on.RunUntilFinished();
                if (off.Winner >= 0) offLoserSurvivors += off.AliveCount(1 - off.Winner);
                if (on.Winner >= 0) onLoserSurvivors += on.AliveCount(1 - on.Winner);
                if (on.BrokenSide < 0) continue;
                breaks++;
                Assert.AreEqual(1 - on.BrokenSide, on.Winner, "the side that did not break wins");
            }
            TestContext.WriteLine("30 fights without orders: the losers kept " + offLoserSurvivors + " fighters under the old rules, " +
                onLoserSurvivors + " under nerve (" + breaks + " breaks).");
            Assert.Greater(breaks, 0);
            Assert.Greater(onLoserSurvivors, offLoserSurvivors);
        }

        [Test]
        public void NerveFights_ReplayExactly()
        {
            BattleSetup setup = Setup("live", 31, NerveRules.On(Full, 900, 20, 0));
            SimSession session = PlayScary(setup);
            var sim = (BattleSimulation)session.Simulation;
            Assert.IsTrue(sim.IsFinished);
            Assert.Greater(session.Log.Count, 0, "the live fight should have used directives");

            var archive = new FightArchive();
            archive.Add(setup, session.Log, sim);
            Assert.IsTrue(archive.TryReplay("live", out BattleSimulation replayed, out string failure), failure);
            Assert.AreEqual(sim.EventHash, replayed.EventHash);
            Assert.AreEqual(sim.Winner, replayed.Winner);
            Assert.AreEqual(sim.BrokenSide, replayed.BrokenSide);
            Assert.AreEqual(sim.Nerve(0), replayed.Nerve(0));
            Assert.AreEqual(sim.Nerve(1), replayed.Nerve(1));
        }

        // ---------------------------------------------------------------- the world

        [Test]
        public void World_BreakingTheRaiders_ConvertsEverySurvivor()
        {
            int found = 0;
            for (ulong seed = 1; seed <= 40 && found < 3; seed++)
            {
                SliceWorld world = SliceWorld.Create(seed, true);
                var survivors = new int[SliceClasses.Count];
                foreach (UnitClass c in world.Raiders.Fighters) survivors[(int)c]++;

                BattleSetup setup = world.NextPlayerFight();
                SimSession session = PlayWardenUnleash(setup);
                var sim = (BattleSimulation)session.Simulation;
                foreach (BattleUnit u in sim.Units)
                {
                    if (u.Side == 1 && !u.Alive) survivors[(int)u.Class]--;
                }
                PlayerFightResult result = world.FinishPlayerFight(setup, session);
                if (!result.RaidersBroke) continue;
                found++;

                Assert.AreEqual(1, sim.BrokenSide, "the player broke them in the fight");
                Assert.AreEqual(world.Raiders.HomeStrength - result.RaiderDead, result.Converted, "the field survivors and everyone at home");
                Assert.AreEqual(result.Converted, world.Converts.Count);
                var joined = new int[SliceClasses.Count];
                foreach (ConvertedFighter f in world.Converts)
                {
                    Assert.AreEqual(SliceWorld.RaidersId, f.FromTribeId);
                    joined[(int)f.Class]++;
                }
                CollectionAssert.AreEqual(survivors, joined, "every survivor joins the player");
                Assert.IsTrue(world.Raiders.Broken);
                Assert.IsTrue(world.Raiders.JoinedPlayer);
                Assert.AreEqual(0, world.Raiders.Strength);
                Assert.IsFalse(world.Raiders.RecruitableByPlayer, "they already joined, there is nobody left to recruit");
                Assert.IsFalse(world.Raiders.Destroyed);
                Assert.IsTrue(world.WarOver);
                CollectionAssert.DoesNotContain(world.RecruitableTribes(), world.Raiders);
            }
            Assert.Greater(found, 0, "the player should break the raiders in some world");
        }

        [Test]
        public void World_ATribeBrokenInAWar_StaysRecruitable()
        {
            for (ulong seed = 1; seed <= 60; seed++)
            {
                SliceWorld world = SliceWorld.Create(seed, true);
                BattleSetup setup = world.NextPlayerFight();
                world.FinishPlayerFight(setup, PlayNoOrders(setup));
                if (world.WarOver) continue;
                WarResult war = world.RunRaiderWar();
                if (!war.LoserBroke) continue;

                Tribe loser = war.LoserId == SliceWorld.RaidersId ? world.Raiders : world.ThirdTribe;
                Assert.IsTrue(loser.Broken);
                Assert.IsFalse(loser.JoinedPlayer, "only a tribe the player breaks joins the player");
                Assert.Greater(loser.Strength, 0);
                Assert.IsTrue(loser.RecruitableByPlayer);
                CollectionAssert.Contains(world.RecruitableTribes(), loser);
                Assert.AreEqual(0, world.Converts.Count);
                return;
            }
            Assert.Fail("no world where a war broke a tribe");
        }

        [Test]
        public void World_NerveCarriesBetweenFights_AndRecoversAtRegroup()
        {
            for (ulong seed = 1; seed <= 40; seed++)
            {
                SliceWorld world = SliceWorld.Create(seed, true);
                BattleSetup first = world.NextPlayerFight();
                Assert.AreEqual(Full, first.Nerve.StartNerve(1), "a fresh tribe starts at full nerve");
                SimSession session = PlayNoOrders(first);
                var sim = (BattleSimulation)session.Simulation;
                PlayerFightResult result = world.FinishPlayerFight(first, session);
                if (result.RaidersBroke || world.WarOver) continue;
                if (sim.Nerve(1) + Tribe.NerveRecoveryPerRegroup >= Full) continue; // the recovery would be capped

                Assert.AreEqual(sim.Nerve(1) + Tribe.NerveRecoveryPerRegroup, world.Raiders.Nerve,
                    "the raiders keep what the fight left them, plus a regroup");

                int raidersBefore = world.Raiders.Nerve;
                int thirdBefore = world.ThirdTribe.Nerve;
                WarResult war = world.RunRaiderWar();
                ArchivedFight fought = world.Archive.Find(war.FightId);
                Assert.AreEqual(raidersBefore, fought.Setup.Nerve.StartNerve(0), "and bring it into their war");
                Assert.AreEqual(thirdBefore, fought.Setup.Nerve.StartNerve(1));
                Assert.IsTrue(world.Archive.TryReplay(war.FightId, out BattleSimulation replayed, out string failure), failure);
                Tribe[] sides = { world.Raiders, world.ThirdTribe };
                for (int s = 0; s < 2; s++)
                {
                    if (sides[s].Broken) Assert.AreEqual(0, sides[s].Nerve, sides[s].Id + " broke and does not recover");
                    else Assert.AreEqual(Math.Min(Full, replayed.Nerve(s) + Tribe.NerveRecoveryPerRegroup), sides[s].Nerve, sides[s].Id);
                }
                if (!world.Raiders.Broken)
                    Assert.AreEqual(world.Raiders.Nerve, world.NextPlayerFight().Nerve.StartNerve(1), "and into the next rival fight");
                return;
            }
            Assert.Fail("no world where the raiders came out of the first fight shaken but unbroken");
        }

        [Test]
        public void World_WithNerve_IsDeterministic_AndEveryFightReplays()
        {
            int fights = 0;
            int wars = 0;
            for (ulong seed = 11; seed <= 16; seed++)
            {
                SliceWorld a = SliceWorld.Create(seed, true);
                SliceWorld b = SliceWorld.Create(seed, true);
                Campaign(a, 3);
                Campaign(b, 3);
                Assert.AreEqual(a.Snapshot(), b.Snapshot(), "seed " + seed);
                StringAssert.Contains("nerveRules", a.Snapshot());
                foreach (ArchivedFight fight in a.Archive.Fights)
                {
                    Assert.IsTrue(a.Archive.TryReplay(fight.FightId, out _, out string failure), failure);
                    fights++;
                    if (fight.FightId.EndsWith(".war", StringComparison.Ordinal)) wars++;
                }
            }
            Assert.AreNotEqual(Snapshot(11), Snapshot(12));
            Assert.Greater(wars, 0, "some wars should have been replayed too");
            TestContext.WriteLine(fights + " nerve fights replayed, " + wars + " of them wars.");
        }

        [Test]
        public void World_OldRules_StayTheDefault()
        {
            SliceWorld plain = SliceWorld.Create(7);
            SliceWorld off = SliceWorld.Create(7, false);
            Assert.IsFalse(plain.NerveOn);
            BattleSetup a = plain.NextPlayerFight();
            BattleSetup b = off.NextPlayerFight();
            Assert.IsFalse(a.Nerve.Enabled);
            plain.FinishPlayerFight(a, PlayScary(a));
            off.FinishPlayerFight(b, PlayScary(b));
            Campaign(plain, 2);
            Campaign(off, 2);
            Assert.AreEqual(plain.Snapshot(), off.Snapshot());
            StringAssert.DoesNotContain("nerve", plain.Snapshot());
            Assert.AreEqual(0, plain.Converts.Count);
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

        private static BattleSetup Setup(string fightId, ulong seed, NerveRules nerve, TacticsProfile raiders = null)
        {
            return new BattleSetup(fightId, seed, Clarth(), Raiders(), new TacticsProfile(), raiders,
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, nerve);
        }

        private static bool Issue(SimSession session, string directive, string target)
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        /// Plays a fight to the end through a session, so the orders are logged. Side 0 gives the
        /// orders the plan names for each tick; null means no orders at all.
        private static SimSession Play(BattleSetup setup, Action<SimSession, BattleSimulation, long> orders)
        {
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                if (orders != null) orders(session, sim, sim.CurrentTick);
                session.Tick();
            }
            return session;
        }

        private static SimSession PlayNoOrders(BattleSetup setup)
        {
            return Play(setup, null);
        }

        /// The hole from 1 Oct: focus their Medic, then Hold back to back with the other four
        /// points, never Unleash.
        private static BattleSimulation PlayHoldExploit(BattleSetup setup)
        {
            return (BattleSimulation)Play(setup, (session, sim, t) =>
            {
                if (t == 30) Focus(session, FirstAlive(sim, 1, UnitClass.Medic));
                if (t == 45 || t == 195 || t == 345 || t == 495) Issue(session, "Hold", "");
            }).Simulation;
        }

        /// The scary orders: Unleash twice.
        private static SimSession PlayScary(BattleSetup setup)
        {
            return Play(setup, (session, sim, t) =>
            {
                if (t == 30 || t == 160) Issue(session, "Unleash", "");
            });
        }

        /// Focus their Warden, then Unleash twice.
        private static SimSession PlayWardenUnleash(BattleSetup setup)
        {
            return Play(setup, (session, sim, t) =>
            {
                if (t == 20) Focus(session, FirstAlive(sim, 1, UnitClass.Warden));
                if (t == 40 || t == 200) Issue(session, "Unleash", "");
            });
        }

        private static void Focus(SimSession session, BattleUnit target)
        {
            if (target != null) Issue(session, "FocusFire", target.Id);
        }

        private static BattleUnit FirstAlive(BattleSimulation sim, int side, UnitClass unitClass)
        {
            foreach (BattleUnit u in sim.Units)
            {
                if (u.Side == side && u.Alive && u.Class == unitClass) return u;
            }
            return null;
        }

        private static bool HasEvent(BattleSimulation sim, BattleEventKind kind)
        {
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == kind) return true;
            }
            return false;
        }

        private static BattleEvent LastEvent(BattleSimulation sim, BattleEventKind kind)
        {
            for (int i = sim.Events.Count - 1; i >= 0; i--)
            {
                if (sim.Events[i].Kind == kind) return sim.Events[i];
            }
            Assert.Fail("no " + kind + " event");
            return default;
        }

        /// Rival fights and wars. No orders in the first fight, so most worlds also see a war.
        private static void Campaign(SliceWorld world, int rounds)
        {
            for (int round = 0; round < rounds && !world.Raiders.Broken; round++)
            {
                BattleSetup setup = world.NextPlayerFight();
                world.FinishPlayerFight(setup, round == 0 ? PlayNoOrders(setup) : PlayScary(setup));
                if (!world.WarOver) world.RunRaiderWar();
            }
        }

        private static string Snapshot(ulong seed)
        {
            SliceWorld world = SliceWorld.Create(seed, true);
            Campaign(world, 3);
            return world.Snapshot();
        }
    }
}
