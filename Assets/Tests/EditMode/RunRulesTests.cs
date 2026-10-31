using System;
using System.Collections.Generic;
using NUnit.Framework;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Sim.Tests
{
    /// The rules changed after Codex's audit of 6 Oct 2026: a Hold blocks Unleash's scare, a
    /// fighter who dies is gone for the run, you choose who wears the forged gear, the chief's
    /// "why" only says what its fight shows, and the memory survives a save and a reload.
    public sealed class RunRulesTests
    {
        [Test]
        public void AnUnleashIntoAHold_ScaresNobody()
        {
            var open = new BattleSimulation(Duel("open", null));
            Assert.IsTrue(Order(new SimSession(open), 0, Directive.Unleash));
            Assert.AreEqual(NerveRules.Full - BattleSimulation.UnleashNerveCost, open.Nerve(1), "without a Hold the scare lands");
            Assert.IsFalse(HasBlock(open, 1));

            var held = new BattleSimulation(Duel("held", null));
            var session = new SimSession(held);
            Assert.IsTrue(Order(session, 1, Directive.Hold));
            Assert.Greater(held.HoldTicksLeft(1), 0, "shields up");
            int before = held.Nerve(1);
            Assert.IsTrue(Order(session, 0, Directive.Unleash));
            Assert.AreEqual(before, held.Nerve(1), "an Unleash into raised shields scares nobody");
            Assert.IsTrue(HasBlock(held, 1), "and the block is an event the screen can show");
            Assert.Greater(held.UnleashTicksLeft(0), 0, "the Unleash still runs");
        }

        [Test]
        public void AChiefWhoLearnedToHold_BlocksYourUnleash_WhileHeHasAPoint()
        {
            var learned = new TacticsProfile();
            learned.SetHoldOnUnleash(BattleSimulation.HabitThreshold);
            var sim = new BattleSimulation(Duel("learned", learned));
            Assert.IsTrue(SliceAutopilot.EnemyCanBlock(sim), "he has learned it and has the points");
            Assert.IsTrue(Order(new SimSession(sim), 0, Directive.Unleash));
            Assert.Greater(sim.HoldTicksLeft(1), 0, "his reflex raised the shields");
            Assert.AreEqual(NerveRules.Full, sim.Nerve(1), "before the scare could land");
            Assert.IsTrue(HasBlock(sim, 1));
        }

        /// Sacrifice is a death too, so it forces one: since blows spread out (6 Oct 2026) the
        /// autopilot's won fights rarely cost anyone.
        [Test]
        public void ADeadFighter_LeavesTheReserveForGood_AndNeverGetsTheGear()
        {
            SliceRun run = SliceRun.Start(new RunMemory(), 5);
            BattleSetup setup = run.NextFight(run.DefaultSquad());
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            Assert.IsTrue(session.TryIssue(Directives.CommandKind, "0", "clarth.marksman", 0, 0,
                Directives.Name(Directive.Sacrifice), out string failure), failure);
            var pilot = new SliceAutopilot(SliceAutopilot.Style.Smart);
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                pilot.Step(session, sim);
                session.Tick();
            }
            RunFightResult result = run.FinishFight(setup, session);

            Assert.IsTrue(HasFighter(result.Fallen, "clarth.marksman"), "the sacrificed fighter is listed as fallen");
            Assert.IsNull(run.FindFighter("clarth.marksman"), "and is gone from the reserve");
            CollectionAssert.DoesNotContain(run.DefaultSquad(), "clarth.marksman");
            foreach (RunFighter f in result.Fallen) Assert.IsNull(run.FindFighter(f.Id), f.Id + " died, so it must leave the reserve");
            if (result.Won && result.GearWearer != null)
            {
                Assert.IsFalse(HasFighter(result.Fallen, result.GearWearer.Id), "the dead get no gear");
                Assert.AreSame(result.GearWearer, run.FindFighter(result.GearWearer.Id), "the gear goes to someone alive");
            }
        }

        /// Jacob's playtest, 6 Oct 2026: every enemy piled onto the Butcher. Three enemy Wardens
        /// against your Pal and two Skirmishers each go for whoever stands across from them first.
        [Test]
        public void TheEnemyLineSpreadsItsBlows_InsteadOfAllHittingYourLeader()
        {
            var us = new List<UnitSpec>
            {
                new UnitSpec("pal", UnitClass.Skirmisher, Row.Front, UnitRole.Pal, 100, 100),
                new UnitSpec("s1", UnitClass.Skirmisher),
                new UnitSpec("s2", UnitClass.Skirmisher)
            };
            var them = new List<UnitSpec>
            {
                new UnitSpec("e0", UnitClass.Warden), new UnitSpec("e1", UnitClass.Warden), new UnitSpec("e2", UnitClass.Warden)
            };
            var sim = new BattleSimulation(new BattleSetup("lanes", 3, us, them, new TacticsProfile(), new TacticsProfile()));
            var firstTarget = new Dictionary<string, string>();
            while (firstTarget.Count < 3 && !sim.IsFinished)
            {
                sim.Tick();
                foreach (BattleEvent e in sim.Events)
                {
                    if (e.Kind == BattleEventKind.Hit && e.ActorSide == 1 && !firstTarget.ContainsKey(e.ActorId))
                        firstTarget[e.ActorId] = e.TargetId;
                }
            }
            Assert.AreEqual("pal", firstTarget["e0"]);
            Assert.AreEqual("s1", firstTarget["e1"]);
            Assert.AreEqual("s2", firstTarget["e2"]);
        }

        /// Only a Focus Fire on the caster breaks a cast (6 Oct 2026). Three Wardens pound your Pal
        /// for more than the interrupt share while he winds up, and nobody is focusing him.
        [Test]
        public void StrayHitsNoLongerBreakYourEclipse()
        {
            var us = new List<UnitSpec>
            {
                new UnitSpec("pal", UnitClass.Warden, Row.Front, UnitRole.Pal, 100, 100),
                new UnitSpec("m", UnitClass.Marksman)
            };
            var them = new List<UnitSpec>
            {
                new UnitSpec("e0", UnitClass.Warden), new UnitSpec("e1", UnitClass.Warden), new UnitSpec("e2", UnitClass.Warden)
            };
            var setup = new BattleSetup("stray", 3, us, them, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.Off, 0, 100,
                new SideRules(1, 0, false), null);
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            Assert.IsTrue(Order(session, 0, Directive.Eclipse));
            while (!sim.IsFinished && sim.WindupTicks(0) > 0) session.Tick();

            int taken = 0;
            bool released = false, interrupted = false;
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == BattleEventKind.Hit && e.ActorSide == 1 && e.TargetId == "pal") taken += e.Amount;
                if (e.Kind == BattleEventKind.Release && e.ActorSide == 0) released = true;
                if (e.Kind == BattleEventKind.Interrupt) interrupted = true;
            }
            Assert.Greater(taken * 100, Moves.InterruptPercent * sim.FindById("pal").MaxHp,
                "precondition: the Pal lost more than the interrupt share while casting");
            Assert.IsFalse(interrupted, "nobody was focusing him, so stray hits did not break the cast");
            Assert.IsTrue(released);
        }

        /// The rematch chief's weak spot (6 Oct 2026): after his own Eclipse he is worn out and
        /// takes double. Your Eclipse lands inside that window: his guard takes the plain share of
        /// its health, the chief twice his.
        [Test]
        public void AChiefWornOutByHisOwnEclipse_TakesDoubleDamage()
        {
            var us = new List<UnitSpec> { new UnitSpec("pal", UnitClass.Warden, Row.Front, UnitRole.Pal, 100, 400) };
            var them = new List<UnitSpec>
            {
                new UnitSpec("guard", UnitClass.Warden, Row.Front, UnitRole.Fighter, 100, 400),
                new UnitSpec("chief", UnitClass.Marksman, Row.Front, UnitRole.Chief, 100, 400)
            };
            var setup = new BattleSetup("worn", 3, us, them, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.Off, 0, 100,
                new SideRules(1, 0, false), new SideRules(1, 30, false));
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);

            BattleEvent his = TickUntil(session, sim, BattleEventKind.Release, 1);
            Assert.AreEqual(Moves.WornOutTicks - (int)(sim.CurrentTick - his.Tick), sim.WornOutTicksLeft(1), "his Eclipse wears him out");
            Assert.IsTrue(Order(session, 0, Directive.Eclipse));
            BattleEvent ours = TickUntil(session, sim, BattleEventKind.Release, 0);
            Assert.Less(ours.Tick - his.Tick, Moves.WornOutTicks, "precondition: yours lands while he is worn out");

            int plain = Math.Max(1, sim.FindById("guard").MaxHp * Moves.EclipsePercent(1) / 100);
            int chief = Math.Max(1, sim.FindById("chief").MaxHp * Moves.EclipsePercent(1) / 100);
            Assert.AreEqual(plain, EclipseHitOn(sim, ours.Tick, "guard"), "a plain fighter takes the plain share");
            Assert.AreEqual(chief * Moves.WornOutDamagePercent / 100, EclipseHitOn(sim, ours.Tick, "chief"), "the worn out chief takes double");
        }

        [Test]
        public void EveryTribeHasATrait_AndItsFightersBringItToBattle()
        {
            Assert.AreEqual(TribeTrait.Hunters, TribeTraits.For(SliceRun.ClarthId));
            Assert.AreEqual(TribeTrait.Dread, TribeTraits.For(SliceRun.RaidersId));
            Assert.AreEqual(TribeTrait.Dread, TribeTraits.For(SliceRun.SplinterId), "the chief's splinter crew are raiders");
            Assert.AreEqual(TribeTrait.SteelBones, TribeTraits.For(SliceRun.SkullpeakId));
            CollectionAssert.AllItemsAreUnique(new[]
            {
                TribeTraits.Describe(TribeTrait.Hunters), TribeTraits.Describe(TribeTrait.Dread), TribeTraits.Describe(TribeTrait.SteelBones)
            });

            SliceRun run = SliceRun.Start(new RunMemory(), 5);
            BattleSetup setup = run.NextFight(run.DefaultSquad());
            foreach (UnitSpec u in setup.Side(0))
                Assert.AreEqual(u.Role == UnitRole.Fighter ? TribeTraits.HuntersPercent : 0, u.WoundedBonusPercent, u.Id);
            foreach (UnitSpec u in setup.Side(1))
                Assert.AreEqual(TribeTraits.DreadPercent, u.FearPercent, u.Id);
        }

        private static bool HasFighter(IReadOnlyList<RunFighter> fighters, string id)
        {
            foreach (RunFighter f in fighters) if (f.Id == id) return true;
            return false;
        }

        private static BattleEvent TickUntil(SimSession session, BattleSimulation sim, BattleEventKind kind, int side)
        {
            int seen = sim.Events.Count;
            while (!sim.IsFinished && sim.CurrentTick < sim.Setup.TickLimit)
            {
                session.Tick();
                for (int i = seen; i < sim.Events.Count; i++)
                {
                    if (sim.Events[i].Kind == kind && sim.Events[i].ActorSide == side) return sim.Events[i];
                }
                seen = sim.Events.Count;
            }
            Assert.Fail("No " + kind + " from side " + side + ".");
            return sim.Events[sim.Events.Count - 1];
        }

        private static int EclipseHitOn(BattleSimulation sim, long tick, string id)
        {
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == BattleEventKind.Hit && e.Tick == tick && e.TargetId == id && e.Detail == Moves.EclipseDetail) return e.Amount;
            }
            return -1;
        }

        [Test]
        public void YouChooseWhoWearsTheForgedGear_UntilTheWarStarts()
        {
            SliceRun run = SliceRun.Start(new RunMemory(), 5);
            RunFightResult first = WinNext(run);
            Assert.AreEqual(GearKind.LavaSunTabard, run.ForgedGear);
            RunFighter pick = run.ForgedWearer;
            Assert.IsNotNull(pick);
            Assert.AreSame(first.GearWearer, pick, "the Butcher's own pick stands until you change it");
            List<RunFighter> candidates = run.GearCandidates();
            Assert.AreSame(pick, candidates[0], "whoever wears it now comes first");
            RunFighter other = candidates.Find(c => !ReferenceEquals(c, pick));
            Assert.IsNotNull(other, "precondition: someone else could wear it");

            Assert.IsTrue(run.GiveForgedGear(other.Id, out string failure), failure);
            Assert.AreEqual(GearKind.LavaSunTabard, other.Gear);
            Assert.AreEqual(GearKind.None, pick.Gear);
            Assert.AreSame(other, run.ForgedWearer);
            Assert.IsFalse(run.GiveForgedGear("nobody", out _));

            Assert.IsTrue(run.WarPending);
            run.RunWar();
            Assert.AreEqual(GearKind.None, run.ForgedGear, "the choice closes once the war starts");
            Assert.IsEmpty(run.GearCandidates());
            Assert.IsFalse(run.GiveForgedGear(pick.Id, out _));
            Assert.AreEqual(GearKind.LavaSunTabard, other.Gear, "and the piece stays where you put it");
        }

        [Test]
        public void AFallBackLesson_SaysOnlyWhatItsFightShows()
        {
            // The learner fields no melee fighters at all, so "our melee could not reach you" would be false.
            var archive = new FightArchive();
            BattleSetup lost = FallBackFight("ranged.lost", 300, 100, archive, out BattleSimulation lostSim);
            Assert.AreEqual(0, lostSim.Winner, "precondition: the side that fell back won");
            var mind = new TribeMind("ranged", "Ranged chief");
            Lesson fallBack = null;
            foreach (Lesson l in LessonRules.Learn(mind, archive.Find(lost.FightId).Record, 1))
                if (l.Rule == LessonRules.FallBack) fallBack = l;
            Assert.IsNotNull(fallBack);
            StringAssert.DoesNotContain("melee", fallBack.Why);
            StringAssert.Contains("did not win", fallBack.Why);
            Assert.IsTrue(archive.TryVerifyLesson(fallBack, out string failure), failure);

            // The same claim about a fight the learner won fails, though the Fall Back count matches.
            BattleSetup won = FallBackFight("ranged.won", 100, 300, archive, out BattleSimulation wonSim);
            Assert.AreEqual(1, wonSim.Winner, "precondition: the learner won this one");
            var claim = new Lesson("ranged", won.FightId, 1, LessonRules.FallBack, UnitClass.Warden, 0, LessonRules.HabitStep,
                archive.Find(won.FightId).Record.FallBacksUsed(0, out _), 0, BattleSimulation.FallBackTicks,
                "You fell back 1 time, first at 0.0s, and we did not win.");
            Assert.AreEqual(1, claim.Evidence, "precondition: the count alone would match");
            Assert.IsFalse(archive.TryVerifyLesson(claim, out _));
        }

        [Test]
        public void TheMemory_SurvivesASaveAndAReload()
        {
            var memory = new RunMemory();
            SliceAutopilot.PlayRun(memory, 5, SliceAutopilot.Style.Smart);
            SliceAutopilot.PlayRun(memory, 6, SliceAutopilot.Style.Passive);
            Assert.Greater(memory.RaiderMind.Lessons.Count, 0, "precondition: the chief learned something");

            string text = RunSave.Write(memory);
            RunMemory loaded = RunSave.Read(text);
            Assert.AreEqual(memory.Snapshot(), loaded.Snapshot());
            Assert.AreEqual(text, RunSave.Write(loaded));
            for (int i = 0; i < memory.RaiderMind.Lessons.Count; i++)
                Assert.AreEqual(memory.RaiderMind.Lessons[i].Why, loaded.RaiderMind.Lessons[i].Why);

            // The next run plays exactly the same on the reloaded memory as on the original.
            RunOutcome a = SliceAutopilot.PlayRun(memory, 7, SliceAutopilot.Style.Smart);
            RunOutcome b = SliceAutopilot.PlayRun(loaded, 7, SliceAutopilot.Style.Smart);
            Assert.AreEqual(a.Run.Snapshot(), b.Run.Snapshot());
        }

        [Test]
        public void ABrokenOrNewerSave_IsRefused()
        {
            string text = RunSave.Write(new RunMemory());
            Assert.IsFalse(RunSave.TryRead(text.Substring(0, text.Length / 2), out _, out _), "cut short");
            Assert.IsFalse(RunSave.TryRead(text.Replace(RunSave.Header + "\t1", RunSave.Header + "\t2"), out _, out _), "newer");
            Assert.IsFalse(RunSave.TryRead("hello", out _, out _));
            Assert.IsFalse(RunSave.TryRead(string.Empty, out _, out _));
            Assert.Throws<FormatException>(() => RunSave.Read("hello"));
        }

        /// Codex's regression baseline (audit, 6 Oct 2026): pressing Unleash whenever it is
        /// affordable, with no other order, used to win all 300 of its runs.
        [Test]
        public void PressingUnleashWheneverYouCan_NoLongerWinsMostRuns()
        {
            const int runs = 60;
            int won = 0;
            for (ulong seed = 41000; seed < 41000 + runs; seed++)
            {
                SliceRun run = SliceRun.Start(new RunMemory(), seed);
                while (!run.Over)
                {
                    if (run.WarPending) run.RunWar();
                    BattleSetup setup = run.NextFight(run.DefaultSquad());
                    run.FinishFight(setup, PlayUnleashOnly(setup));
                }
                if (run.Stage == RunStage.Won) won++;
            }
            TestContext.WriteLine("Unleash whenever affordable won " + won + " of " + runs + " runs.");
            Assert.Less(won, runs / 2);
        }

        /// The chief's Eclipse has to be answered (6 Oct 2026). Two players who differ only in
        /// the dodge: both Frenzy and Unleash whenever affordable, and one also Falls Back to dodge
        /// every Eclipse, keeping a point for it. With an Eclipse every 540 ticks, ignoring him
        /// still won 278 of 300 runs.
        [Test]
        public void IgnoringTheChief_LosesTheRun_AndDodgingHimWins()
        {
            const int runs = 60;
            int ignored = 0, dodged = 0;
            for (ulong k = 1; k <= runs; k++)
            {
                if (PlayPressureRun(k * 7919, false)) ignored++;
                if (PlayPressureRun(k * 7919, true)) dodged++;
            }
            TestContext.WriteLine("Over " + runs + " runs, ignoring the chief won " + ignored +
                " and dodging every Eclipse won " + dodged + ".");
            Assert.Less(ignored, runs / 3, "a player who never answers the chief should usually lose");
            Assert.Greater(dodged, ignored + runs / 2, "and answering him should be what wins");
        }

        private static bool PlayPressureRun(ulong seed, bool dodge)
        {
            SliceRun run = SliceRun.Start(new RunMemory(), seed);
            while (!run.Over)
            {
                if (run.WarPending) run.RunWar();
                BattleSetup setup = run.NextFight(run.DefaultSquad());
                var sim = new BattleSimulation(setup);
                var session = new SimSession(sim);
                long next = SliceAutopilot.StartTick;
                bool dodgedThis = false;
                while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
                {
                    long t = sim.CurrentTick;
                    if (t >= SliceAutopilot.StartTick)
                    {
                        if (!sim.FrenzyUsed(0) && sim.Nerve(1) <= SliceAutopilot.FrenzyAtNerve) Order(session, 0, Directive.Frenzy);
                        int windup = sim.WindupTicks(1);
                        if (windup == 0) dodgedThis = false;
                        else if (dodge && !dodgedThis && windup <= SliceAutopilot.DodgeAtWindup && Order(session, 0, Directive.FallBack))
                            dodgedThis = true;
                        int spare = dodge && setup.Rules(1).EclipseGrade > 0 ? Directives.Cost(Directive.FallBack) : 0;
                        bool waiting = dodge && windup > 0;
                        if (!waiting && t >= next && sim.DirectivePoints(0) >= Directives.Cost(Directive.Unleash) + spare &&
                            Order(session, 0, Directive.Unleash))
                            next = t + BattleSimulation.UnleashTicks;
                    }
                    session.Tick();
                }
                run.FinishFight(setup, session);
            }
            return run.Stage == RunStage.Won;
        }

        // ---------------------------------------------------------------- helpers

        private static bool Order(SimSession session, int side, Directive directive)
        {
            return session.TryIssue(Directives.CommandKind, side == 0 ? "0" : "1", string.Empty, 0, 0, Directives.Name(directive), out _);
        }

        private static bool HasBlock(BattleSimulation sim, int holder)
        {
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == BattleEventKind.Directive && e.ActorSide == holder &&
                    e.Detail == BattleSimulation.BlockedDetail) return true;
            }
            return false;
        }

        /// A Warden and a Marksman each side, under the nerve rules, before anyone has struck.
        private static BattleSetup Duel(string id, TacticsProfile side1Tactics)
        {
            var us = new List<UnitSpec> { new UnitSpec(id + ".w0", UnitClass.Warden), new UnitSpec(id + ".m0", UnitClass.Marksman) };
            var them = new List<UnitSpec> { new UnitSpec(id + ".w1", UnitClass.Warden), new UnitSpec(id + ".m1", UnitClass.Marksman) };
            return new BattleSetup(id, 7, us, them, new TacticsProfile(), side1Tactics ?? new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.On());
        }

        /// Side 0 (Warden and Skirmisher) falls back at once against side 1's two Marksmen, then
        /// they fight it out. Strength in percent per side. Archived with its orders.
        private static BattleSetup FallBackFight(string id, int strength0, int strength1, FightArchive archive, out BattleSimulation sim)
        {
            var us = new List<UnitSpec>
            {
                new UnitSpec(id + ".w", UnitClass.Warden, Row.Front, UnitRole.Fighter, strength0, strength0),
                new UnitSpec(id + ".s", UnitClass.Skirmisher, Row.Front, UnitRole.Fighter, strength0, strength0)
            };
            var them = new List<UnitSpec>
            {
                new UnitSpec(id + ".m0", UnitClass.Marksman, Row.Back, UnitRole.Fighter, strength1, strength1),
                new UnitSpec(id + ".m1", UnitClass.Marksman, Row.Back, UnitRole.Fighter, strength1, strength1)
            };
            var setup = new BattleSetup(id, 11, us, them, new TacticsProfile(), new TacticsProfile());
            sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            Assert.IsTrue(Order(session, 0, Directive.FallBack));
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit) session.Tick();
            archive.Add(setup, session.Log, sim);
            return setup;
        }

        private static SimSession Play(BattleSetup setup, SliceAutopilot.Style style)
        {
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            var pilot = new SliceAutopilot(style);
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                pilot.Step(session, sim);
                session.Tick();
            }
            return session;
        }

        private static SimSession PlayUnleashOnly(BattleSetup setup)
        {
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            long next = SliceAutopilot.StartTick;
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                if (sim.CurrentTick >= next && Order(session, 0, Directive.Unleash))
                    next = sim.CurrentTick + BattleSimulation.UnleashTicks;
                session.Tick();
            }
            return session;
        }

        private static RunFightResult WinNext(SliceRun run)
        {
            BattleSetup setup = run.NextFight(run.DefaultSquad());
            RunFightResult result = run.FinishFight(setup, Play(setup, SliceAutopilot.Style.Smart));
            Assert.IsTrue(result.Won, "precondition: the autopilot won " + setup.FightId);
            return result;
        }
    }
}
