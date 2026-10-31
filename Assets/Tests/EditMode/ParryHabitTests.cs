using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Tribes learn your parry (Jacob, 14 Oct 2026: "yes go build the parry learning"). Claude's diagnosis found the
    /// parry was the one answer no tribe ever learned: free, timed for you, and never countered. Now a tribe you
    /// parried makes its caster wait out your Hold, so a perfect Hold only soaks, and it forgets once you stop.
    public class ParryHabitTests
    {
        private static BattleSetup Setup(string id, TacticsProfile theirs)
        {
            var clarth = new List<UnitSpec>
            {
                new UnitSpec("c.warden", UnitClass.Warden), new UnitSpec("c.skirm", UnitClass.Skirmisher),
                new UnitSpec("c.marks", UnitClass.Marksman), new UnitSpec("c.trap", UnitClass.Trapper),
                new UnitSpec("c.medic", UnitClass.Medic)
            };
            var raiders = new List<UnitSpec>
            {
                new UnitSpec("r.warden", UnitClass.Warden), new UnitSpec("r.skirm", UnitClass.Skirmisher),
                new UnitSpec("r.marks", UnitClass.Marksman), new UnitSpec("r.medic", UnitClass.Medic),
                new UnitSpec("r.chief", UnitClass.Skirmisher, Row.Back, UnitRole.Chief, 100, 100)
            };
            return new BattleSetup(id, 13, clarth, raiders, new TacticsProfile(), theirs, BattleSetup.DefaultTickLimit,
                BattleSetup.DefaultDirectivePoints, NerveRules.On(), SliceRules.RefillTicks, 400, new SideRules(0, 0, true),
                new SideRules(2, 60, false), new FightRules(false, false, true, false));
        }

        // Plays the fight to its end. With holdAt above 0 it Holds once, when their first cast has that many ticks
        // left. Returns the tick of that Hold, or -1.
        private static long Play(BattleSimulation sim, int holdAt)
        {
            var session = new SimSession(sim);
            long held = -1;
            while (!sim.IsFinished && sim.CurrentTick < sim.Setup.TickLimit)
            {
                if (holdAt > 0 && held < 0 && sim.WindupTicks(1) == holdAt &&
                    session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, "Hold", out _))
                    held = sim.CurrentTick;
                session.Tick();
            }
            return held;
        }

        private static SimCommandLog PlayRecorded(BattleSimulation sim, int holdAt)
        {
            var session = new SimSession(sim);
            bool held = false;
            while (!sim.IsFinished)
            {
                if (!held && sim.WindupTicks(1) == holdAt)
                    held = session.TryIssue(Directives.CommandKind, "0", "", 0, 0, "Hold", out _);
                session.Tick();
            }
            Assert.IsTrue(held, "the fixture issued its Hold");
            return session.Log;
        }

        [Test]
        public void ALearnedCasterWaitsOutAFreshHoldSoItOnlySoaks()
        {
            // Not learned: the Hold at the last moment parries, as under the 13 Oct rules.
            var plain = new BattleSimulation(Setup("parry.plain", new TacticsProfile()));
            Play(plain, 10);
            Assert.IsFalse(plain.WaitsOutParry(1));
            Assert.AreEqual(FightRules.Parried, plain.Events.First(e => e.Kind == BattleEventKind.Release).Detail);

            // Learned: the same Hold makes the caster wait until it is older than the parry window, so it soaks.
            var learned = new TacticsProfile();
            learned.SetWaitOutParry(LessonRules.HabitStep);
            var sim = new BattleSimulation(Setup("parry.learned", learned));
            long held = Play(sim, 10);
            Assert.IsTrue(sim.WaitsOutParry(1));
            BattleEvent wait = sim.Events.First(e => e.Kind == BattleEventKind.Reaction && e.Detail == BattleSimulation.WaitOutDetail);
            Assert.AreEqual(1, wait.ActorSide);
            Assert.AreEqual("r.chief", wait.ActorId);
            BattleEvent release = sim.Events.First(e => e.Kind == BattleEventKind.Release);
            Assert.AreEqual(Moves.Soaked, release.Detail, "a perfect Hold only soaks");
            Assert.AreEqual(held + FightRules.PerfectHoldTicks + 1, release.Tick, "it waits just past the parry window");
            Assert.AreEqual(0, sim.Events.Count(e => e.Kind == BattleEventKind.Parry), "nothing is parried");
        }

        [Test]
        public void ATribeLearnsYourParryAndForgetsItWhenYouStop()
        {
            var mind = new TribeMind("raiders", "Chief");
            var first = new BattleSimulation(Setup("parry.teach", new TacticsProfile()));
            Play(first, 10);
            FightRecord taught = FightRecord.From(first);
            Lesson lesson = LessonRules.Learn(mind, taught, 1).Single(l => l.Rule == LessonRules.Parry);
            Assert.AreEqual(0, lesson.ValueBefore);
            Assert.AreEqual(LessonRules.HabitStep, lesson.ValueAfter);
            Assert.AreEqual(UnitClass.Skirmisher, lesson.Subject, "the chief who was parried");
            Assert.AreEqual(lesson.Evidence, LessonRules.Evidence(lesson.Rule, taught, 1, lesson.Subject), "a replay bears it out");
            Assert.IsTrue(mind.HasHabit(Habit.WaitOutParry));

            // Trying to parry a caster who waits keeps the habit; never trying wears it down a step.
            var tried = new BattleSimulation(Setup("parry.tried", mind.CopyTactics()));
            Play(tried, 10);
            Assert.IsFalse(LessonRules.Learn(mind, FightRecord.From(tried), 1)
                .Any(l => l.Rule == LessonRules.FadePrefix + LessonRules.Parry), "you tried, so they keep it");
            var idle = new BattleSimulation(Setup("parry.idle", mind.CopyTactics()));
            Play(idle, 0);
            Lesson fade = LessonRules.Learn(mind, FightRecord.From(idle), 1)
                .Single(l => l.Rule == LessonRules.FadePrefix + LessonRules.Parry);
            Assert.AreEqual(LessonRules.HabitStep - 1, fade.ValueAfter);
        }

        [Test]
        public void ALearnedParryIsKeptThroughASaveAndItsFightsStillReplay()
        {
            const string tribe = CampaignTribes.BloodridgeId;
            Campaign c = Campaign.New(921);
            var mind = c.Mind(tribe);
            var first = new BattleSimulation(Setup("parry.saved.first", new TacticsProfile()));
            var firstLog = PlayRecorded(first, 10);
            c.Archive.Add(first.Setup, firstLog, first);
            Assert.Greater(first.Events.Count(e => e.Kind == BattleEventKind.Parry), 0);
            LessonRules.Learn(mind, FightRecord.From(first), 1);
            c = CampaignSave.Read(CampaignSave.Write(c));
            Assert.IsTrue(c.Mind(tribe).HasHabit(Habit.WaitOutParry));
            var learned = new BattleSimulation(Setup("parry.saved.learned", c.Mind(tribe).CopyTactics()));
            var learnedLog = PlayRecorded(learned, 10);
            c.Archive.Add(learned.Setup, learnedLog, learned);
            Assert.IsTrue(learned.Events.Any(e => e.Detail == BattleSimulation.WaitOutDetail));
            Assert.AreEqual(Moves.Soaked, learned.Events.First(e => e.Kind == BattleEventKind.Release).Detail);
            LessonRules.Learn(c.Mind(tribe), FightRecord.From(learned), 1);

            Campaign read = CampaignSave.Read(CampaignSave.Write(c));
            Assert.AreEqual(CampaignSave.Write(c), CampaignSave.Write(read));
            Assert.AreEqual(c.Mind(tribe).HabitValue(Habit.WaitOutParry), read.Mind(tribe).HabitValue(Habit.WaitOutParry));
            foreach (var fight in c.Archive.Fights)
            {
                Assert.IsTrue(read.Archive.TryReplay(fight.FightId, out var replay, out string why), why);
                Assert.AreEqual(fight.Record.Checksum, StateChecksum.Compute(replay.OrderedStateSnapshot));
            }
            foreach (Lesson lesson in read.Mind(tribe).Lessons)
                Assert.IsTrue(read.Archive.TryVerifyLesson(lesson, out string why), why);
        }
    }
}
