using System;
using System.Collections.Generic;
using NUnit.Framework;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Sim.Tests
{
    /// The slice's big moves (Jacob, 4 Oct 2026): the chief's Eclipse winds up and can be soaked,
    /// dodged or broken off, a Pal can Frenzy once a fight, orders fuse into Ambush and Pyre, and
    /// a side whose Pal or Chief drops breaks. All of it comes from side rules and unit roles, so
    /// a fight without them is exactly as before.
    public sealed class MovesTests
    {
        private const int Health = 400;

        // ---------------------------------------------------------------- Eclipse

        [Test]
        public void Eclipse_WindsUpThenHitsTheFrontRow()
        {
            var sim = new BattleSimulation(ChiefFight("e", 3, new SideRules(2, 60, false)));
            var session = new SimSession(sim);
            BattleEvent release = PlayUntil(session, sim, BattleEventKind.Release);

            BattleEvent windup = First(sim, BattleEventKind.Windup);
            Assert.AreEqual(60, windup.Tick, "the chief casts as soon as it is due and he has the points");
            Assert.AreEqual(1, windup.ActorSide);
            Assert.AreEqual("r.chief", windup.ActorId);
            Assert.AreEqual(Moves.EclipseWindupTicks, windup.Amount);
            Assert.AreEqual("Eclipse", windup.Detail);
            Assert.AreEqual(windup.Tick + Moves.EclipseWindupTicks - 1, release.Tick, "it goes off when the windup has run");
            Assert.AreEqual(Moves.Full, release.Detail);
            Assert.AreEqual(Moves.EclipsePercent(2), release.Amount);

            List<BattleEvent> hits = EclipseHits(sim, release.Tick);
            Assert.Greater(hits.Count, 0);
            foreach (BattleEvent hit in hits)
            {
                BattleUnit target = sim.FindById(hit.TargetId);
                Assert.AreEqual(0, target.Side);
                Assert.AreEqual(Row.Front, target.Row, "Eclipse hits the front row");
                Assert.AreEqual(target.MaxHp * Moves.EclipsePercent(2) / 100, hit.Amount, target.Id);
            }
        }

        [Test]
        public void Eclipse_HoldSoaksHalf()
        {
            var sim = new BattleSimulation(ChiefFight("s", 3, new SideRules(2, 60, false)));
            var session = new SimSession(sim);
            PlayUntil(session, sim, BattleEventKind.Windup);
            Assert.IsTrue(Issue(session, "Hold", ""));
            BattleEvent release = PlayUntil(session, sim, BattleEventKind.Release);

            Assert.AreEqual(Moves.Soaked, release.Detail);
            List<BattleEvent> hits = EclipseHits(sim, release.Tick);
            Assert.Greater(hits.Count, 0);
            foreach (BattleEvent hit in hits)
            {
                BattleUnit target = sim.FindById(hit.TargetId);
                Assert.AreEqual(target.MaxHp * Moves.EclipsePercent(2) / 100 * Moves.EclipseHoldPercent / 100, hit.Amount, target.Id);
            }
        }

        [Test]
        public void Eclipse_FallBackDodgesIt()
        {
            var sim = new BattleSimulation(ChiefFight("d", 3, new SideRules(2, 60, false)));
            var session = new SimSession(sim);
            PlayUntil(session, sim, BattleEventKind.Windup);
            while (sim.WindupTicks(1) > SliceAutopilot.DodgeAtWindup) session.Tick();
            Assert.IsTrue(Issue(session, "FallBack", ""));
            BattleEvent release = PlayUntil(session, sim, BattleEventKind.Release);

            Assert.AreEqual(Moves.Dodged, release.Detail);
            Assert.AreEqual(0, EclipseHits(sim, release.Tick).Count, "a dodged Eclipse hits nobody");
        }

        [Test]
        public void Eclipse_HitTheCasterDuringTheWindup_BreaksItOff_AndDoublesYourNextOrder()
        {
            // A frail chief, so one blow is more than the interrupt share of his health.
            var side1 = new List<UnitSpec>
            {
                new UnitSpec("r.0", UnitClass.Skirmisher),
                new UnitSpec("r.1", UnitClass.Marksman),
                new UnitSpec("r.chief", UnitClass.Marksman, Row.Back, UnitRole.Chief, 100, 25)
            };
            var sim = new BattleSimulation(Setup("i", 5, Clarth(), side1, null, new SideRules(2, 60, false), 0));
            var session = new SimSession(sim);
            PlayUntil(session, sim, BattleEventKind.Windup);
            Assert.IsTrue(Issue(session, "FocusFire", "r.chief"));
            BattleEvent interrupt = PlayUntil(session, sim, BattleEventKind.Interrupt);

            Assert.AreEqual(0, interrupt.ActorSide, "the side that broke the cast");
            Assert.AreEqual("r.chief", interrupt.TargetId);
            Assert.AreEqual(0, sim.WindupTicks(1));
            Assert.IsFalse(HasEvent(sim, BattleEventKind.Release), "the Eclipse never went off");
            Assert.IsTrue(sim.IsEmpowered(0));

            Assert.IsTrue(Issue(session, "Hold", ""));
            BattleEvent empowered = First(sim, BattleEventKind.Empowered);
            Assert.AreEqual(0, empowered.ActorSide);
            Assert.AreEqual("Hold", empowered.Detail);
            Assert.AreEqual(Moves.EmpoweredPercent, empowered.Amount);
            Assert.IsFalse(sim.IsEmpowered(0), "only the next order is doubled");
        }

        [Test]
        public void Eclipse_IsNotYoursUntilYouHaveIt()
        {
            var sim = new BattleSimulation(Setup("n", 5, PalSquad(), Raiders(), null, null, 0));
            var session = new SimSession(sim);
            Assert.IsFalse(session.TryIssue(Directives.CommandKind, "0", "", 0, 0, "Eclipse", out string failure));
            StringAssert.Contains("not yours", failure);

            var owned = new BattleSimulation(Setup("o", 5, PalSquad(), Raiders(), new SideRules(1, 0, false), null, 0));
            var ownedSession = new SimSession(owned);
            Assert.IsTrue(Issue(ownedSession, "Eclipse", ""));
            Assert.AreEqual("pal", First(owned, BattleEventKind.Windup).ActorId, "your Pal casts it");
            Assert.IsFalse(ownedSession.TryIssue(Directives.CommandKind, "0", "", 0, 0, "Eclipse", out failure));
            StringAssert.Contains("already", failure);
        }

        // ---------------------------------------------------------------- Frenzy

        [Test]
        public void Frenzy_IsFree_OncePerFight_AndNeedsAPal()
        {
            var sim = new BattleSimulation(Setup("f", 5, PalSquad(), Raiders(), null, null, 0));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "Frenzy", ""));
            Assert.AreEqual(BattleSetup.DefaultDirectivePoints, sim.DirectivePoints(0), "Frenzy costs nothing");
            BattleEvent transform = First(sim, BattleEventKind.Transform);
            Assert.AreEqual("pal", transform.ActorId);
            Assert.AreEqual(Moves.FrenzyTicks, transform.Amount);
            Assert.IsFalse(session.TryIssue(Directives.CommandKind, "0", "", 0, 0, "Frenzy", out string failure));
            StringAssert.Contains("used up", failure);

            var noPal = new BattleSimulation(Setup("p", 5, Clarth(), Raiders(), null, null, 0));
            Assert.IsFalse(new SimSession(noPal).TryIssue(Directives.CommandKind, "0", "", 0, 0, "Frenzy", out failure));
            StringAssert.Contains("Pal", failure);
        }

        [Test]
        public void Frenzy_DoublesThePalsBlows_AndRaisesEveryAlly()
        {
            var side0 = new List<UnitSpec>
            {
                new UnitSpec("pal", UnitClass.Warden, Row.Front, UnitRole.Pal, 100, 100),
                new UnitSpec("c.m", UnitClass.Marksman)
            };
            var side1 = new List<UnitSpec> { new UnitSpec("r.s", UnitClass.Skirmisher), new UnitSpec("r.m", UnitClass.Marksman) };
            var plain = new BattleSimulation(Setup("fz", 9, side0, side1, null, null, 0));
            var frenzied = new BattleSimulation(Setup("fz", 9, side0, side1, null, null, 0));
            Assert.IsTrue(Issue(new SimSession(frenzied), "Frenzy", ""));
            for (int i = 0; i < 200; i++)
            {
                plain.Tick();
                frenzied.Tick();
            }
            BattleEvent palPlain = FirstHitBy(plain, "pal");
            BattleEvent palFrenzied = FirstHitBy(frenzied, "pal");
            Assert.AreEqual(palPlain.Tick, palFrenzied.Tick, "same blow, same moment");
            Assert.AreEqual(palPlain.Amount * Moves.FrenzyPalPowerPercent / 100, palFrenzied.Amount);
            BattleEvent allyPlain = FirstHitBy(plain, "c.m");
            BattleEvent allyFrenzied = FirstHitBy(frenzied, "c.m");
            Assert.AreEqual(allyPlain.Amount * Moves.FrenzyAllyPowerPercent / 100, allyFrenzied.Amount);
        }

        // ---------------------------------------------------------------- leaders

        [Test]
        public void ALeaderDropping_BreaksTheSide()
        {
            var side0 = new List<UnitSpec>
            {
                new UnitSpec("pal", UnitClass.Warden, Row.Front, UnitRole.Pal, 100, 1),
                new UnitSpec("c.m", UnitClass.Marksman, Row.Back, UnitRole.Fighter, 100, 1000)
            };
            var sim = new BattleSimulation(Setup("l", 5, side0, Raiders(), null, null, 0));
            sim.RunUntilFinished();
            BattleEvent kill = First(sim, BattleEventKind.Kill);
            Assert.AreEqual("pal", kill.TargetId, "precondition: the frail Pal is the first to drop");
            BattleEvent broke = First(sim, BattleEventKind.Break);
            Assert.AreEqual(0, broke.ActorSide);
            Assert.AreEqual(Moves.LeaderDetail, broke.Detail);
            Assert.AreEqual(kill.Tick, broke.Tick, "on the spot");
            Assert.AreEqual(0, sim.BrokenSide);
            Assert.AreEqual(1, sim.Winner);
            Assert.AreEqual(0, sim.Nerve(0));
            Assert.IsTrue(sim.FindById("c.m").Alive, "the rest of the line lives to run");
        }

        [Test]
        public void ALeader_CannotBeSacrificed()
        {
            var sim = new BattleSimulation(Setup("x", 5, PalSquad(), Raiders(), null, null, 0));
            var session = new SimSession(sim);
            Assert.IsFalse(session.TryIssue(Directives.CommandKind, "0", "pal", 0, 0, "Sacrifice", out string failure));
            StringAssert.Contains("leader", failure);
            Assert.IsTrue(Issue(session, "Sacrifice", "c.4"));
        }

        [Test]
        public void Fear_MakesAUnitsHitsDrainMoreCourage()
        {
            var plainSide = new List<UnitSpec> { new UnitSpec("c.m", UnitClass.Marksman) };
            var fearSide = new List<UnitSpec> { new UnitSpec("c.m", UnitClass.Marksman, Row.Back, UnitRole.Fighter, 100, 100, 100) };
            var foes = new List<UnitSpec> { new UnitSpec("r.w", UnitClass.Warden) };
            var plain = new BattleSimulation(Setup("fe", 3, plainSide, foes, null, null, 0));
            var feared = new BattleSimulation(Setup("fe", 3, fearSide, foes, null, null, 0));
            Assert.AreEqual(0, plain.FearPercent(plain.FindById("c.m")));
            Assert.AreEqual(100, feared.FearPercent(feared.FindById("c.m")));

            bool HitBy(BattleSimulation sim)
            {
                foreach (BattleEvent e in sim.Events)
                {
                    if (e.Kind == BattleEventKind.Hit && e.ActorId == "c.m") return true;
                }
                return false;
            }

            for (int i = 0; i < 300 && !HitBy(plain); i++)
            {
                plain.Tick();
                feared.Tick();
            }
            Assert.IsTrue(HitBy(plain) && HitBy(feared), "precondition: the Marksman hit in both fights");
            Assert.Less(feared.Nerve(1), plain.Nerve(1), "the same blow shook them harder");
        }

        // ---------------------------------------------------------------- combos

        [Test]
        public void Ambush_FallBackThenUnleash_HitsTheirBackRow_OncePerFight()
        {
            var sim = new BattleSimulation(Setup("a", 5, Clarth(), Raiders(), new SideRules(0, 0, true), null, 30));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "FallBack", ""));
            for (int i = 0; i < 10; i++) session.Tick();
            Assert.IsTrue(Issue(session, "Unleash", ""));

            BattleEvent combo = First(sim, BattleEventKind.Combo);
            Assert.AreEqual(0, combo.ActorSide);
            Assert.AreEqual(Moves.Ambush, combo.Detail);
            int ambushed = 0;
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind != BattleEventKind.Hit || e.Detail != Moves.AmbushDetail) continue;
                BattleUnit target = sim.FindById(e.TargetId);
                Assert.AreEqual(Row.Back, target.Row, "the back row is caught in the open");
                Assert.AreEqual(target.MaxHp * Moves.AmbushPercent / 100, e.Amount);
                ambushed++;
            }
            Assert.AreEqual(2, ambushed, "both of their back row");
            Assert.IsTrue(sim.ComboUsed(0, Moves.Ambush));

            for (int i = 0; i < 120 && !sim.IsFinished; i++) session.Tick();
            Assert.IsTrue(Issue(session, "FallBack", ""));
            Assert.IsTrue(Issue(session, "Unleash", ""));
            Assert.AreEqual(1, Count(sim, BattleEventKind.Combo), "once per fight");
        }

        [Test]
        public void Ambush_TooSlow_OrWithoutCombos_DoesNothing()
        {
            var slow = new BattleSimulation(Setup("a1", 5, Clarth(), Raiders(), new SideRules(0, 0, true), null, 0));
            var session = new SimSession(slow);
            Assert.IsTrue(Issue(session, "FallBack", ""));
            for (int i = 0; i <= Moves.AmbushWindowTicks; i++) session.Tick();
            Assert.IsTrue(Issue(session, "Unleash", ""));
            Assert.AreEqual(0, Count(slow, BattleEventKind.Combo));

            var off = new BattleSimulation(Setup("a2", 5, Clarth(), Raiders(), null, null, 0));
            var offSession = new SimSession(off);
            Assert.IsTrue(Issue(offSession, "FallBack", ""));
            Assert.IsTrue(Issue(offSession, "Unleash", ""));
            Assert.AreEqual(0, Count(off, BattleEventKind.Combo), "combos are a side rule");
        }

        [Test]
        public void Pyre_FocusUnleashEclipse_BurnsTheFocusedTarget()
        {
            var side1 = new List<UnitSpec>
            {
                new UnitSpec("r.0", UnitClass.Skirmisher),
                new UnitSpec("r.1", UnitClass.Warden),
                new UnitSpec("r.tank", UnitClass.Medic, Row.Back, UnitRole.Fighter, 100, 1000)
            };
            var sim = new BattleSimulation(Setup("py", 5, PalSquad(), side1, new SideRules(1, 0, true), null, 30));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "FocusFire", "r.tank"));
            Assert.IsTrue(Issue(session, "Unleash", ""));
            while (!sim.CanUse(0, Directive.Eclipse, out _)) session.Tick();
            Assert.IsTrue(Issue(session, "Eclipse", ""));
            Assert.IsTrue(sim.WindupIsPyre(0));
            BattleEvent windup = First(sim, BattleEventKind.Windup);
            Assert.AreEqual(Moves.Pyre, windup.Detail);
            Assert.AreEqual("r.tank", windup.TargetId);

            BattleEvent combo = PlayUntil(session, sim, BattleEventKind.Combo);
            Assert.AreEqual(Moves.Pyre, combo.Detail);
            Assert.AreEqual("r.tank", combo.TargetId);
            BattleUnit tank = sim.FindById("r.tank");
            bool burned = false;
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == BattleEventKind.Hit && e.Detail == Moves.PyreDetail && e.TargetId == "r.tank")
                {
                    Assert.AreEqual(tank.MaxHp * Moves.PyrePercent / 100, e.Amount);
                    burned = true;
                }
            }
            Assert.IsTrue(burned);
            Assert.IsTrue(sim.ComboUsed(0, Moves.Pyre));
        }

        // ---------------------------------------------------------------- the old rules and replays

        [Test]
        public void AFightWithoutRulesOrRoles_NeverMeetsTheBigMoves()
        {
            var sim = new BattleSimulation(new BattleSetup("plain", 31, Clarth(), Raiders(), new TacticsProfile(), null));
            sim.RunUntilFinished();
            foreach (BattleEvent e in sim.Events)
                Assert.Less((int)e.Kind, (int)BattleEventKind.Windup, e.Kind.ToString());
            StringAssert.DoesNotContain("windup", sim.OrderedStateSnapshot);
        }

        [Test]
        public void TheChecksum_SeesRowAndClass()
        {
            var front = new List<UnitSpec>(Clarth());
            var back = new List<UnitSpec>(Clarth());
            back[0] = new UnitSpec("c.0", UnitClass.Warden, Row.Back);
            var otherClass = new List<UnitSpec>(Clarth());
            otherClass[0] = new UnitSpec("c.0", UnitClass.Skirmisher, Row.Front);
            string a = StateChecksum.Compute(new BattleSimulation(new BattleSetup("k", 3, front, Raiders(), null, null)).OrderedStateSnapshot);
            string b = StateChecksum.Compute(new BattleSimulation(new BattleSetup("k", 3, back, Raiders(), null, null)).OrderedStateSnapshot);
            string c = StateChecksum.Compute(new BattleSimulation(new BattleSetup("k", 3, otherClass, Raiders(), null, null)).OrderedStateSnapshot);
            Assert.AreNotEqual(a, b, "a unit moved to the back row");
            Assert.AreNotEqual(a, c, "a unit of another class");
        }

        [Test]
        public void AFightWithEveryMove_ReplaysExactly()
        {
            BattleSetup setup = Setup("rp", 11, PalSquad(), ChiefRaiders(), new SideRules(2, 0, true), new SideRules(2, 450, false), 120);
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            var pilot = new SliceAutopilot(SliceAutopilot.Style.Smart);
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                pilot.Step(session, sim);
                session.Tick();
            }
            Assert.IsTrue(HasEvent(sim, BattleEventKind.Windup), "precondition: Eclipses were cast");
            var archive = new FightArchive();
            archive.Add(setup, session.Log, sim);
            Assert.IsTrue(archive.TryReplay("rp", out BattleSimulation replayed, out string failure), failure);
            Assert.AreEqual(sim.EventHash, replayed.EventHash);
            Assert.AreEqual(sim.OrderedStateSnapshot, replayed.OrderedStateSnapshot);
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

        /// The Clarth with a Pal leading at the front.
        private static List<UnitSpec> PalSquad()
        {
            var squad = Clarth();
            squad.Insert(0, new UnitSpec("pal", UnitClass.Warden, Row.Front, UnitRole.Pal, 175, 175));
            return squad;
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

        /// The raiders with their chief in the back row.
        private static List<UnitSpec> ChiefRaiders()
        {
            var raiders = Raiders();
            raiders.Add(new UnitSpec("r.chief", UnitClass.Marksman, Row.Back, UnitRole.Chief, 150, 300));
            return raiders;
        }

        private static BattleSetup ChiefFight(string fightId, ulong seed, SideRules chiefRules)
        {
            return Setup(fightId, seed, Clarth(), ChiefRaiders(), null, chiefRules, 0);
        }

        private static BattleSetup Setup(string fightId, ulong seed, List<UnitSpec> side0, List<UnitSpec> side1,
            SideRules rules0, SideRules rules1, int refillTicks)
        {
            return new BattleSetup(fightId, seed, side0, side1, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.On(), refillTicks, Health,
                rules0, rules1);
        }

        private static bool Issue(SimSession session, string directive, string target)
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        /// Ticks until an event of this kind appears and returns it. Fails if the fight ends first.
        private static BattleEvent PlayUntil(SimSession session, BattleSimulation sim, BattleEventKind kind)
        {
            int seen = sim.Events.Count;
            while (!sim.IsFinished)
            {
                for (; seen < sim.Events.Count; seen++)
                {
                    if (sim.Events[seen].Kind == kind) return sim.Events[seen];
                }
                session.Tick();
            }
            for (; seen < sim.Events.Count; seen++)
            {
                if (sim.Events[seen].Kind == kind) return sim.Events[seen];
            }
            Assert.Fail("The fight ended before a " + kind + " event.");
            return default;
        }

        private static BattleEvent First(BattleSimulation sim, BattleEventKind kind)
        {
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == kind) return e;
            }
            Assert.Fail("No " + kind + " event.");
            return default;
        }

        private static BattleEvent FirstHitBy(BattleSimulation sim, string actorId)
        {
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == BattleEventKind.Hit && e.ActorId == actorId) return e;
            }
            Assert.Fail(actorId + " never hit anyone.");
            return default;
        }

        private static bool HasEvent(BattleSimulation sim, BattleEventKind kind)
        {
            return Count(sim, kind) > 0;
        }

        private static int Count(BattleSimulation sim, BattleEventKind kind)
        {
            int count = 0;
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == kind) count++;
            }
            return count;
        }

        private static List<BattleEvent> EclipseHits(BattleSimulation sim, long tick)
        {
            var hits = new List<BattleEvent>();
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == BattleEventKind.Hit && e.Tick == tick && e.Detail == Moves.EclipseDetail) hits.Add(e);
            }
            return hits;
        }
    }
}
