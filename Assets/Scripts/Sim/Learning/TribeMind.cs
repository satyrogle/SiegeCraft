using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// Habits a tribe can learn. Each is a 0 to 10 value in its TacticsProfile and switches on at 3.
    public enum Habit
    {
        CoverFocused = 0,
        HoldOnUnleash = 1,
        PressOnFallBack = 2,
        AvoidGuarded = 3,
        TrapCaution = 4
    }

    /// One change a tribe made to how it fights, with the reason and the evidence behind it.
    /// FightId plus the tick window say exactly where to look in the replay.
    public sealed class Lesson
    {
        public Lesson(
            string tribeId,
            string fightId,
            int learnerSide,
            string rule,
            UnitClass subject,
            int valueBefore,
            int valueAfter,
            int evidence,
            long fromTick,
            long toTick,
            string why)
        {
            TribeId = tribeId ?? string.Empty;
            FightId = fightId ?? string.Empty;
            LearnerSide = learnerSide;
            Rule = rule ?? string.Empty;
            Subject = subject;
            ValueBefore = valueBefore;
            ValueAfter = valueAfter;
            Evidence = evidence;
            FromTick = fromTick;
            ToTick = toTick;
            Why = why ?? string.Empty;
        }

        public string TribeId { get; }
        public string FightId { get; }

        /// Which side the learning tribe fought on in that fight.
        public int LearnerSide { get; }

        /// Rule id from LessonRules, for example "focus".
        public string Rule { get; }

        /// The class the lesson is about: for focus, our unit that was killed; for threat, the
        /// enemy class that cost us most; for traps, the Trapper. Warden when nothing else fits.
        public UnitClass Subject { get; }

        /// The value the rule changed: a habit value, or remembered threat for the "threat" rule.
        public int ValueBefore { get; }
        public int ValueAfter { get; }

        /// The number the rule measured. A replay of the fight must reproduce it exactly.
        public int Evidence { get; }

        public long FromTick { get; }
        public long ToTick { get; }

        /// Plain sentence a player can read.
        public string Why { get; }
    }

    /// What a tribe has learned from real fights: habits that answer what beat it, a memory of
    /// which enemy classes hurt it, and the lessons that got it there. Nothing here is random or
    /// scripted. Every change comes from a fight record through LessonRules.
    public sealed class TribeMind
    {
        public const int HabitCount = 5;

        private readonly TacticsProfile _tactics = new TacticsProfile();
        private readonly int[] _threatMemory = new int[SliceClasses.Count];
        private readonly List<Lesson> _lessons = new List<Lesson>();
        private readonly List<string> _fightsLearned = new List<string>();

        public TribeMind(string tribeId, string leaderName)
        {
            TribeId = tribeId ?? string.Empty;
            LeaderName = leaderName ?? string.Empty;
        }

        public string TribeId { get; }
        public string LeaderName { get; }
        public IReadOnlyList<Lesson> Lessons => _lessons;
        public IReadOnlyList<string> FightsLearned => _fightsLearned;

        public int HabitValue(Habit habit)
        {
            switch (habit)
            {
                case Habit.CoverFocused: return _tactics.CoverFocused;
                case Habit.HoldOnUnleash: return _tactics.HoldOnUnleash;
                case Habit.PressOnFallBack: return _tactics.PressOnFallBack;
                case Habit.AvoidGuarded: return _tactics.AvoidGuarded;
                case Habit.TrapCaution: return _tactics.TrapCaution;
                default: return 0;
            }
        }

        /// A habit is in use at the sim's habit threshold or above.
        public bool HasHabit(Habit habit)
        {
            return HabitValue(habit) >= BattleSimulation.HabitThreshold;
        }

        /// Remembered harm from an enemy class, in HP. Each fight counts half, older fights fade.
        public int ThreatMemory(UnitClass unitClass)
        {
            return _threatMemory[(int)unitClass];
        }

        /// A copy for a BattleSetup, so the fight keeps what the tribe knew at the time.
        public TacticsProfile CopyTactics()
        {
            return _tactics.Clone();
        }

        public bool HasLearnedFrom(string fightId)
        {
            for (int i = 0; i < _fightsLearned.Count; i++)
            {
                if (string.Equals(_fightsLearned[i], fightId, StringComparison.Ordinal)) return true;
            }
            return false;
        }

        /// Habits in use, for example "covers the focused 4, holds on unleash 4". "none" when empty.
        public string HabitsText()
        {
            var b = new StringBuilder();
            for (int i = 0; i < HabitCount; i++)
            {
                var habit = (Habit)i;
                if (!HasHabit(habit)) continue;
                if (b.Length > 0) b.Append(", ");
                b.Append(HabitName(habit)).Append(' ').Append(HabitValue(habit).ToString(CultureInfo.InvariantCulture));
            }
            return b.Length == 0 ? "none" : b.ToString();
        }

        public static string HabitName(Habit habit)
        {
            switch (habit)
            {
                case Habit.CoverFocused: return "cover the focused";
                case Habit.HoldOnUnleash: return "hold on unleash";
                case Habit.PressOnFallBack: return "press on fall back";
                case Habit.AvoidGuarded: return "avoid the guarded";
                case Habit.TrapCaution: return "trap caution";
                default: return "unknown";
            }
        }

        public TribeMind Clone()
        {
            var clone = new TribeMind(TribeId, LeaderName);
            for (int i = 0; i < HabitCount; i++) clone.SetHabit((Habit)i, HabitValue((Habit)i));
            Array.Copy(_threatMemory, clone._threatMemory, _threatMemory.Length);
            clone._lessons.AddRange(_lessons);
            clone._fightsLearned.AddRange(_fightsLearned);
            return clone;
        }

        public string Snapshot()
        {
            var b = new SnapshotBuilder();
            b.Section("mind:" + TribeId);
            b.Field("leader", LeaderName);
            _tactics.AppendTo(b, string.Empty);
            for (int i = 0; i < SliceClasses.All.Length; i++)
                b.Field("mem." + SliceClasses.Name(SliceClasses.All[i]), _threatMemory[i]);
            b.Field("fights", _fightsLearned.Count);
            b.Field("lessons", _lessons.Count);
            for (int i = 0; i < _lessons.Count; i++)
            {
                Lesson l = _lessons[i];
                b.Field("l" + i.ToString(CultureInfo.InvariantCulture),
                    l.FightId + "/" + l.Rule + "/" + SliceClasses.Name(l.Subject) + "/" +
                    l.ValueBefore.ToString(CultureInfo.InvariantCulture) + ">" +
                    l.ValueAfter.ToString(CultureInfo.InvariantCulture) + "/" +
                    l.Evidence.ToString(CultureInfo.InvariantCulture));
            }
            return b.ToString();
        }

        internal void MarkLearned(string fightId)
        {
            _fightsLearned.Add(fightId);
        }

        internal void Record(Lesson lesson)
        {
            _lessons.Add(lesson);
        }

        /// Folds one fight's threat into memory: new memory is the average of old memory and this fight.
        internal void BlendThreat(int[] fightThreat)
        {
            for (int i = 0; i < _threatMemory.Length; i++)
                _threatMemory[i] = (_threatMemory[i] + fightThreat[i]) / 2;
        }

        internal void SetHabit(Habit habit, int value)
        {
            switch (habit)
            {
                case Habit.CoverFocused: _tactics.SetCoverFocused(value); break;
                case Habit.HoldOnUnleash: _tactics.SetHoldOnUnleash(value); break;
                case Habit.PressOnFallBack: _tactics.SetPressOnFallBack(value); break;
                case Habit.AvoidGuarded: _tactics.SetAvoidGuarded(value); break;
                case Habit.TrapCaution: _tactics.SetTrapCaution(value); break;
            }
        }
    }
}
