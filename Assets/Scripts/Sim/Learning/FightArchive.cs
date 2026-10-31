using System;
using System.Collections.Generic;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// One stored fight: how it started, every order given, how long it ran and how it ended.
    public sealed class ArchivedFight
    {
        private readonly SimCommandLog _log;
        private readonly Dictionary<string, string> _names = new Dictionary<string, string>(StringComparer.Ordinal);
        internal string CachedEvidence;
        internal string CachedSetupIdentity;
        internal ArchivedFight(BattleSetup setup, SimCommandLog log, long ticks, FightRecord record,
            string rulesIdentity = ReplayEvidence.RulesIdentity)
        {
            Setup = setup;
            _log = log.CloneForArchive();
            Ticks = ticks;
            Record = record;
            RulesIdentity = rulesIdentity;
        }

        public string FightId => Setup.FightId;
        public BattleSetup Setup { get; }
        public SimCommandLog Log => _log.CloneForArchive();
        public long Ticks { get; }
        public FightRecord Record { get; }
        public string RulesIdentity { get; }

        /// Everyone's names in this fight by unit id (CampaignNames, Jacob's go on 12 Oct 2026), kept so a
        /// replay names them the way the fight did. Empty for a fight recorded before names were kept; your
        /// leader and the Sovereign's hunters are never in it.
        public IReadOnlyDictionary<string, string> Names => _names;

        /// A unit's name in this fight, or empty.
        public string NameOf(string unitId)
        {
            return unitId != null && _names.TryGetValue(unitId, out string name) ? name : string.Empty;
        }

        internal void SetNames(IReadOnlyDictionary<string, string> names)
        {
            _names.Clear();
            if (names == null) return;
            foreach (KeyValuePair<string, string> pair in names)
                if (!string.IsNullOrEmpty(pair.Key) && !string.IsNullOrEmpty(pair.Value)) _names[pair.Key] = pair.Value;
        }
    }

    /// Every fight a world has seen, kept so any fight can be played again exactly.
    /// This is the "replay why": a lesson names its fight and tick window, the archive replays
    /// that fight from its seed and orders, and the lesson's number must come out the same.
    public sealed class FightArchive
    {
        private readonly List<ArchivedFight> _fights = new List<ArchivedFight>();

        public IReadOnlyList<ArchivedFight> Fights => _fights;

        internal bool Restore(ArchivedFight fight)
        {
            if (fight == null || Find(fight.FightId) != null) return false;
            _fights.Add(fight);
            return true;
        }

        /// For CampaignSave: everyone's names in a recorded fight, as unit id and name pairs from start. A fight
        /// the archive no longer keeps is skipped. False if the pairs are broken, an id or a name is empty or
        /// repeated, or the fight has its names already.
        internal bool RestoreNames(string fightId, IReadOnlyList<string> pairs, int start)
        {
            int count = pairs.Count - start;
            if (start < 0 || count < 2 || count % 2 != 0) return false;
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = start; i < pairs.Count; i += 2)
            {
                if (string.IsNullOrEmpty(pairs[i]) || string.IsNullOrEmpty(pairs[i + 1]) || names.ContainsKey(pairs[i])) return false;
                names[pairs[i]] = pairs[i + 1];
            }
            ArchivedFight fight = Find(fightId);
            if (fight == null) return true;
            if (fight.Names.Count > 0) return false;
            fight.SetNames(names);
            return true;
        }

        /// Stores a finished fight. Pass the command log of the live session, or null if there were no orders,
        /// and everyone's names in it by unit id, or null when nobody had one.
        public ArchivedFight Add(BattleSetup setup, SimCommandLog log, BattleSimulation finished,
            IReadOnlyDictionary<string, string> names = null)
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
            archived.SetNames(names);
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

            if (!ReplayEvidence.Replayable(fight))
            {
                failure = "This fight uses different battle rules. Its history is kept, but replay is unavailable in this build.";
                return false;
            }
            replayed = new BattleSimulation(fight.Setup);
            ReplayResult result = ReplayRunner.Run(replayed, fight.Log.CloneForReplay(), fight.Ticks);
            if (!result.Success)
            {
                failure = result.Failure;
                return false;
            }
            if (!replayed.IsFinished || result.Ticks != fight.Ticks ||
                !string.Equals(result.Checksum, fight.Record.Checksum, StringComparison.Ordinal))
            {
                failure = "Replay of " + fightId + " ended in a different state: " + result.Checksum +
                    " instead of " + fight.Record.Checksum + ".";
                return false;
            }
            if (!ReplayEvidence.SameRecord(fight.Record, FightRecord.From(replayed)))
            {
                failure = "The saved fight history differs from its replay.";
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
            int evidence = LessonRules.Evidence(lesson.Rule, FightRecord.From(replayed), lesson.LearnerSide, lesson.Subject, lesson.Move);
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
