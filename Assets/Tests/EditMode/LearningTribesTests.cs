using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Sim.Tests
{
    /// Proves the slice's core claim in the sim: units and tribes that learn from real fights,
    /// including wars between tribes, with a replay that shows why.
    public sealed class LearningTribesTests
    {
        private const int Points = BattleSetup.DefaultDirectivePoints;

        private enum Plan
        {
            None,

            /// Focus their Medic, Unleash, focus the most hurt, Hold.
            FocusMedicUnleash,

            /// Focus their Marksman, Unleash, Fall Back twice.
            FocusMarksmanUnleashFallBack
        }

        // ---------------------------------------------------------------- battle

        [Test]
        public void Battle_SameSetup_SameResult()
        {
            var a = new BattleSimulation(Setup("x", 11));
            var b = new BattleSimulation(Setup("x", 11));
            a.RunUntilFinished();
            b.RunUntilFinished();
            Assert.AreEqual(StateChecksum.Compute(a.OrderedStateSnapshot), StateChecksum.Compute(b.OrderedStateSnapshot));
            Assert.AreEqual(a.EventHash, b.EventHash);
        }

        [Test]
        public void Battle_DifferentSeed_DifferentFight()
        {
            var a = new BattleSimulation(Setup("x", 11));
            var b = new BattleSimulation(Setup("x", 12));
            a.RunUntilFinished();
            b.RunUntilFinished();
            Assert.AreNotEqual(a.EventHash, b.EventHash);
        }

        [Test]
        public void Battle_EndsByAWipe_InsideTheTimeLimit()
        {
            var sim = new BattleSimulation(Setup("x", 5));
            sim.RunUntilFinished();
            Assert.IsTrue(sim.IsFinished);
            Assert.Less(sim.EndTick, BattleSetup.DefaultTickLimit - 1, "a slice fight should end by a wipe, not the clock");
            Assert.AreNotEqual(-1, sim.Winner);
        }

        [Test]
        public void Battle_EveryClassUsesItsSignature()
        {
            var sim = new BattleSimulation(Setup("x", 21));
            sim.RunUntilFinished();
            FightRecord record = FightRecord.From(sim);
            Assert.Greater(record.HealedBy(0, UnitClass.Medic, out _, out _, out _), 0, "the Clarth Medic should heal");
            Assert.Greater(record.GuardPrevented(0, out _, out _, out _), 0, "the Clarth Warden should guard");
            Assert.Greater(record.TrapsArmedBy(0), 0, "the Clarth Trapper should arm a trap");
            Assert.Greater(record.TrapsSprungOn(1, false, out _, out _), 0, "a raider should spring it");
        }

        [Test]
        public void Battle_ReplayWithDirectives_MatchesLivePlay()
        {
            BattleSetup setup = Setup("live", 31);
            SimSession session = Play(setup, Plan.FocusMedicUnleash);
            var sim = (BattleSimulation)session.Simulation;
            Assert.IsTrue(sim.IsFinished);
            Assert.Greater(session.Log.Count, 0, "the live fight should have used directives");

            var archive = new FightArchive();
            archive.Add(setup, session.Log, sim);
            Assert.IsTrue(archive.TryReplay("live", out BattleSimulation replayed, out string failure), failure);
            Assert.AreEqual(sim.EventHash, replayed.EventHash);
            Assert.AreEqual(sim.Winner, replayed.Winner);
        }

        // ---------------------------------------------------------------- directives

        [Test]
        public void Directive_SpendsPoints_AndRefusesWhenOut()
        {
            var sim = new BattleSimulation(Setup("x", 3));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "Unleash", ""));
            Assert.AreEqual(Points - 2, sim.DirectivePoints(0));
            Assert.IsTrue(Issue(session, "Unleash", ""));
            Assert.AreEqual(Points - 4, sim.DirectivePoints(0));
            Assert.IsFalse(session.TryIssue(Directives.CommandKind, "0", "", 0, 0, "Unleash", out string failure));
            StringAssert.Contains("Not enough directive points", failure);
            Assert.IsTrue(Issue(session, "Hold", ""));
            Assert.AreEqual(0, sim.DirectivePoints(0));
            Assert.IsFalse(Issue(session, "FallBack", ""));
            Assert.AreEqual(Points, sim.DirectivePoints(1), "the untaught raiders spent nothing");
        }

        [Test]
        public void Directive_RefusesBadOrders()
        {
            var sim = new BattleSimulation(Setup("x", 3));
            Assert.IsFalse(sim.TryApply(Cmd(sim, "FocusFire", "c.1"), out _), "focus on your own unit");
            Assert.IsFalse(sim.TryApply(Cmd(sim, "FocusFire", "nobody"), out _), "focus on nobody");
            Assert.IsFalse(sim.TryApply(Cmd(sim, "Sacrifice", "r.0"), out _), "sacrifice an enemy");
            Assert.IsFalse(sim.TryApply(Cmd(sim, "Charge", ""), out _), "unknown directive");
            Assert.IsFalse(sim.TryApply(new SimCommand(1, 0, 1, Directives.CommandKind, "2", "", 0, 0, "Hold"), out _), "no side 2");
            Assert.IsFalse(sim.TryApply(new SimCommand(1, 0, 1, "move", "0", "", 0, 0, "Hold"), out _), "not a directive");
            Assert.AreEqual(Points, sim.DirectivePoints(0), "refused orders cost nothing");
            Assert.AreEqual(0, sim.Events.Count, "refused orders leave no trace");
        }

        [Test]
        public void Directive_FocusFire_PullsAttacksOntoTheTarget()
        {
            int focused = AttacksOn(Setup("x", 41), "r.4", true);
            int normal = AttacksOn(Setup("x", 41), "r.4", false);
            Assert.Greater(focused, normal, "focus fire should send more attacks at the raider Medic");
        }

        [Test]
        public void Directive_Sacrifice_KillsOneAndHealsTheRest()
        {
            var sim = new BattleSimulation(Setup("x", 7));
            RunTicks(sim, 200);
            Assert.GreaterOrEqual(sim.AliveCount(0), 2, "precondition: two Clarth alive at 200 ticks");
            BattleUnit victim = null;
            var before = new Dictionary<string, int>();
            foreach (BattleUnit u in sim.Units)
            {
                if (u.Side != 0 || !u.Alive) continue;
                if (victim == null) victim = u;
                before[u.Id] = u.Hp;
            }

            Assert.IsTrue(sim.TryApply(Cmd(sim, "Sacrifice", victim.Id), out string failure), failure);
            Assert.IsFalse(victim.Alive);
            Assert.AreEqual(Points - 3, sim.DirectivePoints(0));
            foreach (BattleUnit u in sim.Units)
            {
                if (u.Side != 0 || !u.Alive) continue;
                int expected = Math.Min(u.MaxHp, before[u.Id] + u.MaxHp * BattleSimulation.SacrificeHealPercent / 100);
                Assert.AreEqual(expected, u.Hp, u.Id + " should be healed by the sacrifice");
            }
        }

        [Test]
        public void Directive_FallBack_StopsAllMeleeForItsWindow()
        {
            var sim = new BattleSimulation(Setup("x", 9));
            var session = new SimSession(sim);
            RunTicks(session, 60);
            Assert.IsTrue(Issue(session, "FallBack", ""));
            long from = sim.CurrentTick;
            RunTicks(session, BattleSimulation.FallBackTicks);
            Assert.IsFalse(sim.IsFinished, "precondition: the fight is still on");
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Tick < from || e.Kind != BattleEventKind.Hit) continue;
                Assert.IsTrue(SliceClasses.Stats((UnitClass)e.ActorClass).Ranged, "melee hit at tick " + e.Tick + " during fall back");
            }
        }

        // ---------------------------------------------------------------- learned reflexes

        [Test]
        public void Reflex_CoverFocused_WardenStepsInFrontOfTheFocusedUnit()
        {
            var raiders = new TacticsProfile();
            raiders.SetCoverFocused(4);
            var sim = new BattleSimulation(Setup("x", 13, raiders));
            var session = new SimSession(sim);
            RunTicks(session, 30);
            Assert.IsTrue(Issue(session, "FocusFire", "r.4"));
            Assert.AreEqual("r.4", sim.FindById("r.1").GuardTargetId, "the raider Warden covers the focused Medic");
            BattleEvent cover = LastEvent(sim, BattleEventKind.Directive);
            Assert.AreEqual(BattleSimulation.CoverDetail, cover.Detail);
            Assert.AreEqual(1, cover.ActorSide);
            Assert.AreEqual(Points, sim.DirectivePoints(1), "covering is free");

            var untaught = new BattleSimulation(Setup("x", 13));
            var plain = new SimSession(untaught);
            RunTicks(plain, 30);
            Assert.IsTrue(Issue(plain, "FocusFire", "r.4"));
            Assert.AreNotEqual(BattleSimulation.CoverDetail, LastEvent(untaught, BattleEventKind.Directive).Detail);
        }

        [Test]
        public void Reflex_HoldOnUnleash_AnswersAtOnce()
        {
            var raiders = new TacticsProfile();
            raiders.SetHoldOnUnleash(4);
            var sim = new BattleSimulation(Setup("x", 13, raiders));
            var session = new SimSession(sim);
            RunTicks(session, 30);
            Assert.IsTrue(Issue(session, "Unleash", ""));
            BattleEvent hold = LastEvent(sim, BattleEventKind.Directive);
            Assert.AreEqual(Directives.Name(Directive.Hold), hold.Detail);
            Assert.AreEqual(BattleSimulation.AutoActorPrefix + "1", hold.ActorId);
            Assert.AreEqual(30, hold.Tick);
            Assert.AreEqual(Points - Directives.Cost(Directive.Hold), sim.DirectivePoints(1));
        }

        [Test]
        public void Reflex_PressOnFallBack_UnleashesAtOnce()
        {
            var raiders = new TacticsProfile();
            raiders.SetPressOnFallBack(4);
            var sim = new BattleSimulation(Setup("x", 13, raiders));
            var session = new SimSession(sim);
            RunTicks(session, 30);
            Assert.IsTrue(Issue(session, "FallBack", ""));
            BattleEvent press = LastEvent(sim, BattleEventKind.Directive);
            Assert.AreEqual(Directives.Name(Directive.Unleash), press.Detail);
            Assert.AreEqual(BattleSimulation.AutoActorPrefix + "1", press.ActorId);
            Assert.AreEqual(Points - Directives.Cost(Directive.Unleash), sim.DirectivePoints(1));
        }

        [Test]
        public void Reflexes_ReplayExactly()
        {
            var raiders = new TacticsProfile();
            raiders.SetCoverFocused(4);
            raiders.SetHoldOnUnleash(4);
            raiders.SetPressOnFallBack(4);
            BattleSetup setup = Setup("taught", 17, raiders);
            SimSession session = Play(setup, Plan.FocusMarksmanUnleashFallBack);
            var sim = (BattleSimulation)session.Simulation;
            Assert.IsTrue(HasAutoDirective(sim, BattleSimulation.CoverDetail), "cover should fire");
            Assert.IsTrue(HasAutoDirective(sim, Directives.Name(Directive.Hold)), "hold should fire");
            Assert.IsTrue(HasAutoDirective(sim, Directives.Name(Directive.Unleash)), "press should fire");

            var archive = new FightArchive();
            archive.Add(setup, session.Log, sim);
            Assert.IsTrue(archive.TryReplay("taught", out BattleSimulation replayed, out string failure), failure);
            Assert.AreEqual(sim.EventHash, replayed.EventHash);
        }

        [Test]
        public void Habit_AvoidGuarded_SteersRangedOnly()
        {
            var raiders = new TacticsProfile();
            raiders.SetAvoidGuarded(4);
            var taught = new BattleSimulation(Setup("x", 23, raiders));
            var untaught = new BattleSimulation(Setup("x", 23));
            taught.RunUntilFinished();
            untaught.RunUntilFinished();
            Assert.Greater(GuardedHits(untaught, 1, true), 0, "untaught raider archers shoot covered units");
            Assert.AreEqual(0, GuardedHits(taught, 1, true), "taught raider archers never do");
            Assert.Greater(GuardedHits(taught, 1, false), 0, "raider melee ignore the habit");
        }

        // ---------------------------------------------------------------- learning

        [Test]
        public void Learning_FocusKill_TeachesCover_WithTheRealNumbers()
        {
            for (ulong seed = 1; seed <= 60; seed++)
            {
                var sim = (BattleSimulation)Play(Setup("f" + seed, seed), Plan.FocusMedicUnleash).Simulation;
                FightRecord record = FightRecord.From(sim);
                int kills = record.FocusKills(0, true, out _, out long orderTick, out int victim, out long killTicks);
                if (kills == 0) continue;

                bool killSeen = false;
                foreach (BattleEvent e in sim.Events)
                {
                    if (e.Kind == BattleEventKind.Kill && e.ActorSide == 0 && e.Tick == orderTick + killTicks && e.TargetClass == victim)
                        killSeen = true;
                }
                Assert.IsTrue(killSeen, "the kill the lesson cites is in the fight's events");

                var mind = new TribeMind("raiders", "chief");
                Lesson lesson = Find(LessonRules.Learn(mind, record, 1), LessonRules.Focus);
                Assert.IsNotNull(lesson);
                Assert.AreEqual(kills, lesson.Evidence);
                Assert.AreEqual(0, lesson.ValueBefore);
                Assert.AreEqual(LessonRules.HabitStep, lesson.ValueAfter);
                Assert.AreEqual(orderTick, lesson.FromTick);
                Assert.AreEqual(orderTick + killTicks, lesson.ToTick);
                StringAssert.Contains(SliceClasses.Name((UnitClass)victim), lesson.Why);
                Assert.IsTrue(mind.HasHabit(Habit.CoverFocused));
                return;
            }
            Assert.Fail("no focus kill in 60 seeds");
        }

        [Test]
        public void Learning_Unleash_TeachesHold_WithTheRealNumbers()
        {
            FightRecord record = FightRecord.From((BattleSimulation)Play(Setup("u", 2), Plan.FocusMedicUnleash).Simulation);
            int damage = record.UnleashDamage(0, out int unleashes, out long first);
            Assert.AreEqual(1, unleashes);
            Assert.GreaterOrEqual(damage, LessonRules.UnleashMinDamage, "precondition");

            int sum = 0;
            foreach (BattleEvent e in record.Events)
            {
                if (e.Kind == BattleEventKind.Hit && e.ActorSide == 0 && e.Tick >= first && e.Tick < first + BattleSimulation.UnleashTicks)
                    sum += e.Amount;
            }
            Assert.AreEqual(sum, damage, "the evidence is every Clarth hit inside the Unleash window");

            var mind = new TribeMind("raiders", "chief");
            Lesson lesson = Find(LessonRules.Learn(mind, record, 1), LessonRules.Unleash);
            Assert.IsNotNull(lesson);
            Assert.AreEqual(damage, lesson.Evidence);
            StringAssert.Contains(damage.ToString(CultureInfo.InvariantCulture), lesson.Why);
            Assert.IsTrue(mind.HasHabit(Habit.HoldOnUnleash));
        }

        [Test]
        public void Learning_FallBack_TeachesPress_OnlyWhenItCostUs()
        {
            int taughtByLosses = 0;
            for (ulong seed = 1; seed <= 20; seed++)
            {
                FightRecord record = FightRecord.From((BattleSimulation)Play(Setup("b" + seed, seed), Plan.FocusMarksmanUnleashFallBack).Simulation);
                Assert.AreEqual(2, record.FallBacksUsed(0, out _));
                var mind = new TribeMind("raiders", "chief");
                Lesson press = Find(LessonRules.Learn(mind, record, 1), LessonRules.FallBack);
                if (record.Winner == 1)
                {
                    Assert.IsNull(press, "raiders who won learn nothing from the fall backs");
                    continue;
                }
                Assert.IsNotNull(press);
                Assert.AreEqual(2, press.Evidence);
                Assert.IsTrue(mind.HasHabit(Habit.PressOnFallBack));
                taughtByLosses++;
            }
            Assert.Greater(taughtByLosses, 0);
        }

        [Test]
        public void Learning_SameFightTwice_TeachesNothingNew()
        {
            FightRecord record = FightRecord.From((BattleSimulation)Play(Setup("x", 2), Plan.FocusMedicUnleash).Simulation);
            var mind = new TribeMind("raiders", "chief");
            Assert.Greater(LessonRules.Learn(mind, record, 1).Count, 0);
            string after = mind.Snapshot();
            Assert.AreEqual(0, LessonRules.Learn(mind, record, 1).Count);
            Assert.AreEqual(after, mind.Snapshot());
        }

        [Test]
        public void Learning_SameFights_SameMind()
        {
            var a = new TribeMind("raiders", "chief");
            var b = new TribeMind("raiders", "chief");
            for (ulong seed = 1; seed <= 6; seed++)
            {
                Plan plan = seed % 2 == 0 ? Plan.FocusMedicUnleash : Plan.FocusMarksmanUnleashFallBack;
                LessonRules.Learn(a, FightRecord.From((BattleSimulation)Play(Setup("s" + seed, seed, a.CopyTactics()), plan).Simulation), 1);
                LessonRules.Learn(b, FightRecord.From((BattleSimulation)Play(Setup("s" + seed, seed, b.CopyTactics()), plan).Simulation), 1);
            }
            Assert.AreEqual(a.Snapshot(), b.Snapshot());
            Assert.Greater(a.Lessons.Count, 0);
        }

        [Test]
        public void Learning_HabitsStayInBounds_OverManyFights()
        {
            var mind = new TribeMind("raiders", "chief");
            for (ulong seed = 1; seed <= 30; seed++)
            {
                var plan = (Plan)(int)(seed % 3);
                LessonRules.Learn(mind, FightRecord.From((BattleSimulation)Play(Setup("m" + seed, seed, mind.CopyTactics()), plan).Simulation), 1);
                for (int h = 0; h < TribeMind.HabitCount; h++)
                    Assert.That(mind.HabitValue((Habit)h), Is.InRange(TacticsProfile.MinValue, TacticsProfile.MaxValue));
                foreach (UnitClass c in SliceClasses.All) Assert.GreaterOrEqual(mind.ThreatMemory(c), 0);
            }
        }

        [Test]
        public void Learning_NeverLearnsTrapCaution()
        {
            var mind = new TribeMind("raiders", "chief");
            for (ulong seed = 1; seed <= 15; seed++)
                LessonRules.Learn(mind, FightRecord.From((BattleSimulation)Play(Setup("t" + seed, seed, mind.CopyTactics()), Plan.None).Simulation), 1);
            Assert.AreEqual(0, mind.HabitValue(Habit.TrapCaution), "measured harmful, so never learned");
        }

        [Test]
        public void Habits_FadeOnlyWhenTheEnemyCouldHaveUsedTheTrick()
        {
            var mind = new TribeMind("raiders", "chief");
            for (ulong seed = 1; seed <= 40; seed++)
            {
                if (mind.HasHabit(Habit.HoldOnUnleash) && mind.HasHabit(Habit.PressOnFallBack)) break;
                LessonRules.Learn(mind, FightRecord.From((BattleSimulation)Play(Setup("t" + seed, seed, mind.CopyTactics()), Plan.FocusMarksmanUnleashFallBack).Simulation), 1);
            }
            Assert.IsTrue(mind.HasHabit(Habit.HoldOnUnleash) && mind.HasHabit(Habit.PressOnFallBack), "precondition");
            int hold = mind.HabitValue(Habit.HoldOnUnleash);
            int press = mind.HabitValue(Habit.PressOnFallBack);
            int cover = mind.HabitValue(Habit.CoverFocused);

            // A war: nobody can give orders, so nothing learned about orders fades.
            var war = new BattleSimulation(new BattleSetup("war", 5, Raiders(), Clarth(), mind.CopyTactics(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, 0));
            war.RunUntilFinished();
            LessonRules.Learn(mind, FightRecord.From(war), 0);
            Assert.AreEqual(hold, mind.HabitValue(Habit.HoldOnUnleash));
            Assert.AreEqual(press, mind.HabitValue(Habit.PressOnFallBack));
            Assert.AreEqual(cover, mind.HabitValue(Habit.CoverFocused));

            // The player had points and used none: both ease off one step, and the fade checks out on replay.
            BattleSetup quiet = Setup("quiet", 6, mind.CopyTactics());
            SimSession session = Play(quiet, Plan.None);
            var archive = new FightArchive();
            archive.Add(quiet, session.Log, (BattleSimulation)session.Simulation);
            IReadOnlyList<Lesson> lessons = LessonRules.Learn(mind, archive.Find("quiet").Record, 1);
            Assert.AreEqual(hold - 1, mind.HabitValue(Habit.HoldOnUnleash));
            Assert.AreEqual(press - 1, mind.HabitValue(Habit.PressOnFallBack));
            Lesson fade = Find(lessons, LessonRules.FadePrefix + LessonRules.Unleash);
            Assert.IsNotNull(fade);
            Assert.AreEqual(0, fade.Evidence);
            Assert.IsTrue(archive.TryVerifyLesson(fade, out string failure), failure);
        }

        [Test]
        public void Rival_TaughtByRealFights_BeatsThePlanThatBeatIt()
        {
            foreach (Plan plan in new[] { Plan.FocusMedicUnleash, Plan.FocusMarksmanUnleashFallBack })
            {
                var mind = new TribeMind("raiders", "chief");
                for (int k = 0; k < 3; k++)
                {
                    var lesson = new BattleSetup("teach" + k, 90000UL + (ulong)k, Clarth(), Raiders(), new TacticsProfile(), mind.CopyTactics());
                    LessonRules.Learn(mind, FightRecord.From((BattleSimulation)Play(lesson, plan).Simulation), 1);
                }
                int untaught = PlayerWins(plan, new TacticsProfile(), 40);
                int taught = PlayerWins(plan, mind.CopyTactics(), 40);
                TestContext.WriteLine(plan + ": the player won " + untaught + " of 40 against untaught raiders, " + taught +
                    " of 40 once they had learned from 3 real fights (" + mind.HabitsText() + ")");
                Assert.Less(taught * 2, untaught, plan + " should win less than half as often once the raiders have learned from it");
            }
        }

        // ---------------------------------------------------------------- replay why

        [Test]
        public void Lessons_AllMatchTheirReplays()
        {
            SliceWorld world = SliceWorld.Create(3);
            Campaign(world, 4, null);
            int checkedLessons = 0;
            foreach (Tribe tribe in world.Tribes)
            {
                foreach (Lesson lesson in tribe.Mind.Lessons)
                {
                    Assert.IsTrue(world.Archive.TryVerifyLesson(lesson, out string failure), failure);
                    checkedLessons++;
                }
            }
            Assert.Greater(checkedLessons, 0);
        }

        [Test]
        public void Lesson_WithAWrongNumber_FailsItsReplay()
        {
            BattleSetup setup = Setup("w", 2);
            SimSession session = Play(setup, Plan.FocusMedicUnleash);
            var archive = new FightArchive();
            archive.Add(setup, session.Log, (BattleSimulation)session.Simulation);
            var mind = new TribeMind("raiders", "chief");
            Lesson real = Find(LessonRules.Learn(mind, archive.Find("w").Record, 1), LessonRules.Unleash);
            Assert.IsNotNull(real);
            var forged = new Lesson(real.TribeId, real.FightId, real.LearnerSide, real.Rule, real.Subject, real.ValueBefore,
                real.ValueAfter, real.Evidence + 1, real.FromTick, real.ToTick, real.Why);
            Assert.IsTrue(archive.TryVerifyLesson(real, out string failure), failure);
            Assert.IsFalse(archive.TryVerifyLesson(forged, out _));
        }

        // ---------------------------------------------------------------- the world

        [Test]
        public void Rival_ComesBackChanged()
        {
            for (ulong seed = 1; seed <= 20; seed++)
            {
                SliceWorld world = SliceWorld.Create(seed);
                PlayerFightResult first = PlayRivalFight(world, Plan.FocusMarksmanUnleashFallBack);
                if (Find(first.RaiderLessons, LessonRules.FallBack) == null || world.Raiders.Broken) continue;

                BattleSetup next = world.NextPlayerFight();
                TacticsProfile rival = next.CopyTactics(1);
                Assert.GreaterOrEqual(rival.PressOnFallBack, BattleSimulation.HabitThreshold, "the rival now answers Fall Back");
                Assert.GreaterOrEqual(rival.HoldOnUnleash, BattleSimulation.HabitThreshold, "and Unleash");
                Assert.AreEqual(0, next.CopyTactics(0).PressOnFallBack, "the Clarth learned nothing about Fall Back");
                return;
            }
            Assert.Fail("no world in 20 where the raiders lost to the fall back plan");
        }

        [Test]
        public void Wars_DoNotEraseWhatTheRivalLearnedAboutThePlayer()
        {
            for (ulong seed = 1; seed <= 20; seed++)
            {
                SliceWorld world = SliceWorld.Create(seed);
                PlayRivalFight(world, Plan.FocusMarksmanUnleashFallBack);
                if (world.WarOver) continue;
                TribeMind chief = world.Raiders.Mind;
                int hold = chief.HabitValue(Habit.HoldOnUnleash);
                int press = chief.HabitValue(Habit.PressOnFallBack);
                int cover = chief.HabitValue(Habit.CoverFocused);
                Assert.Greater(hold + press, 0, "precondition: the rival learned something about orders");
                world.RunRaiderWar();
                Assert.AreEqual(hold, chief.HabitValue(Habit.HoldOnUnleash));
                Assert.AreEqual(press, chief.HabitValue(Habit.PressOnFallBack));
                Assert.AreEqual(cover, chief.HabitValue(Habit.CoverFocused));
                return;
            }
            Assert.Fail("no world in 20 that reached a war");
        }

        [Test]
        public void War_LoserBreaks_AndOpensToRecruitment()
        {
            SliceWorld world = SliceWorld.Create(5);
            Assert.AreEqual(0, world.RecruitableTribes().Count);
            for (int i = 0; i < 40 && !world.WarOver; i++) world.RunRaiderWar();
            Assert.IsTrue(world.WarOver, "someone should break within 40 wars");

            Tribe broken = world.Raiders.Broken ? world.Raiders : world.ThirdTribe;
            Assert.Less(broken.Strength * 2, broken.HomeStrength);
            Assert.AreEqual(broken.Strength > 0, broken.RecruitableByPlayer);
            Assert.AreEqual(broken.Strength == 0, broken.Destroyed);
            Assert.AreEqual(broken.RecruitableByPlayer, world.RecruitableTribes().Contains(broken));
            Assert.IsFalse(world.Clarth.Broken);
            Assert.Throws<InvalidOperationException>(() => world.RunRaiderWar());
        }

        [Test]
        public void Tribe_RosterStaysInHomeOrder_AndRegroups()
        {
            SliceWorld world = SliceWorld.Create(9);
            Tribe raiders = world.Raiders;
            Assert.AreEqual(raiders.HomeStrength, raiders.Strength);
            for (int round = 0; round < 4 && !world.Raiders.Broken; round++)
            {
                int strength = raiders.Strength;
                PlayerFightResult fight = PlayRivalFight(world, Plan.FocusMedicUnleash);
                if (!raiders.Broken)
                {
                    int expected = Math.Min(raiders.HomeStrength, strength - fight.RaiderDead + SliceWorld.RegroupPerFight);
                    Assert.AreEqual(expected, raiders.Strength, "the dead leave, then " + SliceWorld.RegroupPerFight + " come back");
                }
                AssertInHomeOrder(raiders);
                if (!world.WarOver) world.RunRaiderWar();
                AssertInHomeOrder(raiders);
                AssertInHomeOrder(world.ThirdTribe);
            }
        }

        [Test]
        public void World_SameSeed_SameHistory()
        {
            SliceWorld a = SliceWorld.Create(11);
            SliceWorld b = SliceWorld.Create(11);
            SliceWorld c = SliceWorld.Create(12);
            Campaign(a, 4, null);
            Campaign(b, 4, null);
            Campaign(c, 4, null);
            Assert.AreEqual(a.Snapshot(), b.Snapshot());
            Assert.AreNotEqual(a.Snapshot(), c.Snapshot());
        }

        [Test]
        public void World_EveryArchivedFightReplays()
        {
            SliceWorld world = SliceWorld.Create(12);
            Campaign(world, 4, null);
            Assert.Greater(world.Archive.Fights.Count, 1);
            foreach (ArchivedFight fight in world.Archive.Fights)
                Assert.IsTrue(world.Archive.TryReplay(fight.FightId, out _, out string failure), failure);
        }

        [Test]
        public void World_PrintsReadableTranscript()
        {
            string text = Transcript(7);
            TestContext.WriteLine(text);
            Assert.AreEqual(text, Transcript(7));
            StringAssert.Contains("Rival chief: ", text);
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

        private static BattleSetup Setup(string fightId, ulong seed, TacticsProfile raiders = null)
        {
            return new BattleSetup(fightId, seed, Clarth(), Raiders(), new TacticsProfile(), raiders);
        }

        private static int _sequence;

        private static SimCommand Cmd(BattleSimulation sim, string directive, string target)
        {
            _sequence++;
            return new SimCommand(SimCommandLog.CurrentSchemaVersion, sim.CurrentTick, _sequence, Directives.CommandKind, "0", target, 0, 0, directive);
        }

        private static bool Issue(SimSession session, string directive, string target)
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        private static void RunTicks(BattleSimulation sim, int ticks)
        {
            for (int i = 0; i < ticks && !sim.IsFinished; i++) sim.Tick();
        }

        private static void RunTicks(SimSession session, int ticks)
        {
            var sim = (BattleSimulation)session.Simulation;
            for (int i = 0; i < ticks && !sim.IsFinished; i++) session.Tick();
        }

        /// Plays a fight to the end with a scripted player on side 0, through a session so the orders are logged.
        private static SimSession Play(BattleSetup setup, Plan plan)
        {
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                Order(session, sim, plan);
                session.Tick();
            }
            return session;
        }

        private static void Order(SimSession session, BattleSimulation sim, Plan plan)
        {
            long t = sim.CurrentTick;
            if (plan == Plan.FocusMedicUnleash)
            {
                if (t == 30) Focus(session, FirstAlive(sim, 1, UnitClass.Medic));
                if (t == 45) Issue(session, "Unleash", "");
                if (t == 240) Focus(session, MostHurt(sim, 1));
                if (t == 400) Issue(session, "Hold", "");
            }
            else if (plan == Plan.FocusMarksmanUnleashFallBack)
            {
                if (t == 20) Focus(session, FirstAlive(sim, 1, UnitClass.Marksman));
                if (t == 40) Issue(session, "Unleash", "");
                if (t == 130) Issue(session, "FallBack", "");
                if (t == 250) Issue(session, "FallBack", "");
            }
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

        private static BattleUnit MostHurt(BattleSimulation sim, int side)
        {
            BattleUnit best = null;
            foreach (BattleUnit u in sim.Units)
            {
                if (u.Side == side && u.Alive && (best == null || u.HpPermille < best.HpPermille)) best = u;
            }
            return best;
        }

        private static int PlayerWins(Plan plan, TacticsProfile raiders, int fights)
        {
            int wins = 0;
            for (int i = 0; i < fights; i++)
            {
                var setup = new BattleSetup("m" + i, 40000UL + (ulong)i, Clarth(), Raiders(), new TacticsProfile(), raiders);
                if (((BattleSimulation)Play(setup, plan).Simulation).Winner == 0) wins++;
            }
            return wins;
        }

        /// Side 0 attacks aimed at a target in the focus window from tick 30, landed or guarded.
        private static int AttacksOn(BattleSetup setup, string targetId, bool focus)
        {
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            RunTicks(session, 30);
            if (focus) Assert.IsTrue(Issue(session, "FocusFire", targetId));
            RunTicks(session, BattleSimulation.FocusTicks);
            int count = 0;
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Tick < 30 || !string.Equals(e.TargetId, targetId, StringComparison.Ordinal)) continue;
                if (e.Kind == BattleEventKind.Hit && e.ActorSide == 0 && e.Detail.Length == 0) count++;
                if (e.Kind == BattleEventKind.Guard && e.ActorSide == 1) count++;
            }
            return count;
        }

        /// Hits a side's ranged or melee fighters aimed at a covered unit, which the Warden took instead.
        private static int GuardedHits(BattleSimulation sim, int side, bool ranged)
        {
            int count = 0;
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind != BattleEventKind.Hit || e.ActorSide != side) continue;
                if (!string.Equals(e.Detail, BattleSimulation.GuardedDetail, StringComparison.Ordinal)) continue;
                if (SliceClasses.Stats((UnitClass)e.ActorClass).Ranged == ranged) count++;
            }
            return count;
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

        private static bool HasAutoDirective(BattleSimulation sim, string detail)
        {
            foreach (BattleEvent e in sim.Events)
            {
                if (e.Kind == BattleEventKind.Directive && e.ActorId.StartsWith(BattleSimulation.AutoActorPrefix, StringComparison.Ordinal) &&
                    string.Equals(e.Detail, detail, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static Lesson Find(IReadOnlyList<Lesson> lessons, string rule)
        {
            for (int i = 0; i < lessons.Count; i++)
            {
                if (string.Equals(lessons[i].Rule, rule, StringComparison.Ordinal)) return lessons[i];
            }
            return null;
        }

        private static void AssertInHomeOrder(Tribe tribe)
        {
            int h = 0;
            for (int f = 0; f < tribe.Fighters.Count; f++)
            {
                while (h < tribe.HomeRoster.Count && tribe.HomeRoster[h] != tribe.Fighters[f]) h++;
                Assert.Less(h, tribe.HomeRoster.Count, tribe.Id + " fighter " + f + " is out of home roster order");
                h++;
            }
        }

        private static PlayerFightResult PlayRivalFight(SliceWorld world, Plan plan)
        {
            BattleSetup setup = world.NextPlayerFight();
            return world.FinishPlayerFight(setup, Play(setup, plan));
        }

        /// The scripted campaign the world tests share: a rival fight, then a war, alternating plans.
        private static void Campaign(SliceWorld world, int rounds, StringBuilder story)
        {
            for (int round = 0; round < rounds && !world.Raiders.Broken; round++)
            {
                Plan plan = round % 2 == 0 ? Plan.FocusMarksmanUnleashFallBack : Plan.FocusMedicUnleash;
                PlayerFightResult fight = PlayRivalFight(world, plan);
                if (story != null)
                {
                    story.AppendLine(fight.Record.FightId + " (" + plan + "): " + Outcome(fight.Record.Winner, "the Clarth", "the raiders") +
                        " after " + (fight.Record.EndTick / SimClock.TicksPerSecond) + "s. Raiders lost " + fight.RaiderDead + ", " +
                        world.Raiders.Strength + " of " + world.Raiders.HomeStrength + " left" + (fight.RaidersBroke ? " and broke." : "."));
                    AppendLessons(story, "  Rival chief", fight.RaiderLessons);
                    AppendLessons(story, "  Clarth", fight.ClarthLessons);
                }
                if (world.WarOver) continue;
                WarResult war = world.RunRaiderWar();
                if (story != null)
                {
                    story.AppendLine(war.FightId + " (off screen): " +
                        (war.WinnerId.Length == 0 ? "a draw" : war.WinnerId + " beat " + war.LoserId) +
                        (war.LoserBroke ? ", and " + war.LoserId + " broke." : "."));
                    AppendLessons(story, "  Rival chief", war.LessonsA);
                    AppendLessons(story, "  Third tribe chief", war.LessonsB);
                }
            }
        }

        private static string Outcome(int winner, string side0, string side1)
        {
            return winner == 0 ? side0 + " won" : winner == 1 ? side1 + " won" : "a draw";
        }

        private static void AppendLessons(StringBuilder b, string who, IReadOnlyList<Lesson> lessons)
        {
            for (int i = 0; i < lessons.Count; i++) b.AppendLine(who + ": " + lessons[i].Why);
        }

        private static string Transcript(ulong seed)
        {
            SliceWorld world = SliceWorld.Create(seed);
            var b = new StringBuilder();
            Campaign(world, 4, b);
            foreach (Tribe t in world.RecruitableTribes())
                b.AppendLine("Now recruitable: " + t.DisplayName + ", " + t.Strength + " left.");
            return b.ToString();
        }
    }
}
