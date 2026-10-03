using System;
using System.Collections.Generic;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// One stored fight: how it started, every order given, how long it ran and how it ended.
    public sealed class ArchivedFight
    {
        internal ArchivedFight(BattleSetup setup, SimCommandLog log, long ticks, FightRecord record)
        {
            Setup = setup;
            Log = log;
            Ticks = ticks;
            Record = record;
        }

        public string FightId => Setup.FightId;
        public BattleSetup Setup { get; }
        public SimCommandLog Log { get; }
        public long Ticks { get; }
        public FightRecord Record { get; }
    }

    /// Every fight a world has seen, kept so any fight can be played again exactly.
    /// This is the "replay why": a lesson names its fight and tick window, the archive replays
    /// that fight from its seed and orders, and the lesson's number must come out the same.
    public sealed class FightArchive
    {
        private readonly List<ArchivedFight> _fights = new List<ArchivedFight>();

        public IReadOnlyList<ArchivedFight> Fights => _fights;

        /// Stores a finished fight. Pass the command log of the live session, or null if there were no orders.
        public ArchivedFight Add(BattleSetup setup, SimCommandLog log, BattleSimulation finished)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            if (finished == null) throw new ArgumentNullException(nameof(finished));
            if (!finished.IsFinished) throw new InvalidOperationException("Only finished fights can be archived.");
            if (!ReferenceEquals(setup, finished.Setup)) throw new ArgumentException("The setup must be the one the fight ran with.");
            if (Find(setup.FightId) != null) throw new ArgumentException("Fight " + setup.FightId + " is already archived.");

            var archived = new ArchivedFight(
                setup,
                log == null ? new SimCommandLog() : log.CloneForArchive(),
                finished.CurrentTick,
                FightRecord.From(finished));
            _fights.Add(archived);
            return archived;
        }

        public ArchivedFight Find(string fightId)
        {
            for (int i = 0; i < _fights.Count; i++)
            {
                if (string.Equals(_fights[i].FightId, fightId, StringComparison.Ordinal)) return _fights[i];
            }
            return null;
        }

        /// Plays an archived fight again from scratch. Succeeds only if it ends in exactly the same state.
        public bool TryReplay(string fightId, out BattleSimulation replayed, out string failure)
        {
            replayed = null;
            failure = string.Empty;
            ArchivedFight fight = Find(fightId);
            if (fight == null)
            {
                failure = "No archived fight " + fightId + ".";
                return false;
            }

            replayed = new BattleSimulation(fight.Setup);
            ReplayResult result = ReplayRunner.Run(replayed, fight.Log.CloneForReplay(), fight.Ticks);
            if (!result.Success)
            {
                failure = result.Failure;
                return false;
            }
            if (!string.Equals(result.Checksum, fight.Record.Checksum, StringComparison.Ordinal))
            {
                failure = "Replay of " + fightId + " ended in a different state: " + result.Checksum +
                    " instead of " + fight.Record.Checksum + ".";
                return false;
            }
            return true;
        }

        /// Replays the lesson's fight and checks the lesson's evidence comes out the same.
        /// Fade lessons too: the replay must show the enemy never did the thing.
        public bool TryVerifyLesson(Lesson lesson, out string failure)
        {
            failure = string.Empty;
            if (lesson == null)
            {
                failure = "Lesson is null.";
                return false;
            }

            if (!TryReplay(lesson.FightId, out BattleSimulation replayed, out failure)) return false;
            int evidence = LessonRules.Evidence(lesson.Rule, FightRecord.From(replayed), lesson.LearnerSide, lesson.Subject);
            if (evidence != lesson.Evidence)
            {
                failure = "Lesson " + lesson.Rule + " from " + lesson.FightId + " says " + lesson.Evidence +
                    " but the replay measures " + evidence + ".";
                return false;
            }
            return true;
        }
    }
}
