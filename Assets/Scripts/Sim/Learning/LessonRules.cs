using System;
using System.Collections.Generic;
using System.Globalization;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// How a tribe learns from a finished fight. Each rule reads one measured number from the
    /// fight record and answers what actually beat it:
    ///   focus     Your focus fire killed one of ours: our Warden now covers whoever you focus.
    ///   unleash   Your Unleash hurt us: we now answer your Unleash with Hold.
    ///   fallback  You fell back and we did not win: we now answer your Fall Back with Unleash.
    ///   guard     Your Warden stopped real damage: our ranged fighters now shoot whoever he isn't covering.
    ///   threat    Which of your classes cost us most, kept as memory for the story. Changes no tactic.
    /// A habit eases off one step after a fight where the enemy could have done what it answers
    /// and didn't (rule "fade." plus the habit's rule). Wars between tribes have no orders, so
    /// they never wear down what a tribe learned about the player.
    /// Not learned, because on 1 Oct 2026 each made the learner lose more: target priorities,
    /// opening focus fire and trap caution. Thresholds are provisional.
    ///
    /// Every sentence says only what its fight record measured, and a replay checks each claim it
    /// makes (Codex audit, 6 Oct 2026: a tribe with no melee fighters was told its melee could
    /// not reach you, and the check still passed because it only counted the Fall Backs).
    public static class LessonRules
    {
        public const string Focus = "focus";
        public const string Unleash = "unleash";
        public const string FallBack = "fallback";
        public const string Guard = "guard";
        public const string Threat = "threat";

        /// Trap caution's rule id. It is never raised (see above), so it only appears as "fade.traps".
        public const string Traps = "traps";

        public const string FadePrefix = "fade.";

        public const int HabitStep = 4;
        public const int UnleashMinDamage = 60;
        public const int GuardMinPrevented = 30;
        public const int MinThreatToRemember = 80;

        /// Learns from one fight once. The mind fought on learnerSide. Returns the new lessons in
        /// the order they were made. Learning twice from the same fight changes nothing.
        public static IReadOnlyList<Lesson> Learn(TribeMind mind, FightRecord record, int learnerSide)
        {
            if (mind == null) throw new ArgumentNullException(nameof(mind));
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (learnerSide != 0 && learnerSide != 1) throw new ArgumentOutOfRangeException(nameof(learnerSide));

            var made = new List<Lesson>();
            if (mind.HasLearnedFrom(record.FightId)) return made;
            int enemy = 1 - learnerSide;

            int kills = record.FocusKills(enemy, true, out _, out long focusTick, out int victimClass, out long killTicks);
            if (kills > 0)
            {
                Raise(made, mind, record, learnerSide, Habit.CoverFocused, (UnitClass)victimClass, kills,
                    focusTick, focusTick + killTicks,
                    "At " + Seconds(focusTick) + " you focused our " + SliceClasses.Name((UnitClass)victimClass) +
                    " and killed it " + Seconds(killTicks) + " later. Now our Warden steps in front of whoever you focus.");
            }

            int unleashDamage = record.UnleashDamage(enemy, out _, out long unleashTick);
            if (unleashDamage >= UnleashMinDamage)
            {
                Raise(made, mind, record, learnerSide, Habit.HoldOnUnleash, UnitClass.Warden, unleashDamage,
                    unleashTick, unleashTick + BattleSimulation.UnleashTicks,
                    "While your Unleash ran, from " + Seconds(unleashTick) + ", you did " + N(unleashDamage) +
                    " damage to us. Now we Hold the moment you unleash.");
            }

            int fallBacks = record.FallBacksUsed(enemy, out long fallTick);
            if (fallBacks > 0 && record.Winner != learnerSide)
            {
                Raise(made, mind, record, learnerSide, Habit.PressOnFallBack, UnitClass.Warden, fallBacks,
                    fallTick, fallTick + BattleSimulation.FallBackTicks,
                    "You fell back " + N(fallBacks) + (fallBacks == 1 ? " time" : " times") + ", first at " + Seconds(fallTick) +
                    ", and we did not win. Now we Unleash the moment you fall back.");
            }

            int prevented = record.GuardPrevented(enemy, out int guards, out long guardFrom, out long guardTo);
            if (prevented >= GuardMinPrevented)
            {
                Raise(made, mind, record, learnerSide, Habit.AvoidGuarded, UnitClass.Warden, prevented, guardFrom, guardTo,
                    "Your Warden covered " + N(guards) + " hits and stopped " + N(prevented) +
                    " damage. Now our ranged fighters shoot whoever he isn't covering.");
            }

            int[] threat = record.ThreatByClass(learnerSide, out int totalThreat);
            if (totalThreat >= MinThreatToRemember)
            {
                int top = 0;
                for (int i = 1; i < threat.Length; i++)
                {
                    if (threat[i] > threat[top]) top = i;
                }
                int before = mind.ThreatMemory((UnitClass)top);
                mind.BlendThreat(threat);
                Keep(made, mind, new Lesson(mind.TribeId, record.FightId, learnerSide, Threat, (UnitClass)top, before,
                    mind.ThreatMemory((UnitClass)top), threat[top], 0, record.EndTick,
                    "Of everything that cost us HP, your " + SliceClasses.Name((UnitClass)top) + " cost the most: " +
                    N(threat[top]) + " of " + N(totalThreat) + ". We will remember it."));
            }

            for (int i = 0; i < TribeMind.HabitCount; i++)
            {
                var habit = (Habit)i;
                int value = mind.HabitValue(habit);
                if (value == 0 || !CouldTrigger(habit, record, learnerSide) || TriggerCount(habit, record, learnerSide) > 0) continue;
                mind.SetHabit(habit, value - 1);
                Keep(made, mind, new Lesson(mind.TribeId, record.FightId, learnerSide, FadePrefix + RuleFor(habit),
                    habit == Habit.TrapCaution ? UnitClass.Trapper : UnitClass.Warden,
                    value, mind.HabitValue(habit), 0, 0, record.EndTick, FadeWhy(habit)));
            }

            mind.MarkLearned(record.FightId);
            return made;
        }

        /// Recomputes the number a rule measured, from a record. A replay of the lesson's fight must
        /// give the same number. Fade lessons measure how often the enemy did the thing: always 0.
        /// A claim the replay does not bear out gives -1, so the lesson fails: a focus lesson names
        /// the class that was killed, and a fall back lesson says the learner did not win. Unknown
        /// rules give -1 too, so they never verify.
        public static int Evidence(string rule, FightRecord record, int learnerSide, UnitClass subject)
        {
            if (rule == null || record == null) return -1;
            int enemy = 1 - learnerSide;
            if (rule.StartsWith(FadePrefix, StringComparison.Ordinal))
            {
                string answered = rule.Substring(FadePrefix.Length);
                for (int i = 0; i < TribeMind.HabitCount; i++)
                {
                    if (string.Equals(RuleFor((Habit)i), answered, StringComparison.Ordinal))
                        return TriggerCount((Habit)i, record, learnerSide);
                }
                return -1;
            }
            if (string.Equals(rule, Focus, StringComparison.Ordinal))
            {
                int kills = record.FocusKills(enemy, true, out _, out _, out int victimClass, out _);
                return kills > 0 && victimClass != (int)subject ? -1 : kills;
            }
            if (string.Equals(rule, Unleash, StringComparison.Ordinal))
                return record.UnleashDamage(enemy, out _, out _);
            if (string.Equals(rule, FallBack, StringComparison.Ordinal))
                return record.Winner == learnerSide ? -1 : record.FallBacksUsed(enemy, out _);
            if (string.Equals(rule, Guard, StringComparison.Ordinal))
                return record.GuardPrevented(enemy, out _, out _, out _);
            if (string.Equals(rule, Threat, StringComparison.Ordinal))
                return record.ThreatByClass(learnerSide, out _)[(int)subject];
            return -1;
        }

        /// The rule that raises a habit, which is also the thing the enemy does that the habit answers.
        public static string RuleFor(Habit habit)
        {
            switch (habit)
            {
                case Habit.CoverFocused: return Focus;
                case Habit.HoldOnUnleash: return Unleash;
                case Habit.PressOnFallBack: return FallBack;
                case Habit.AvoidGuarded: return Guard;
                case Habit.TrapCaution: return Traps;
                default: return string.Empty;
            }
        }

        /// Times the enemy did what a habit answers: focus orders (not on Wardens), Unleashes,
        /// Fall Backs, guarded hits, or traps armed.
        public static int TriggerCount(Habit habit, FightRecord record, int learnerSide)
        {
            int enemy = 1 - learnerSide;
            switch (habit)
            {
                case Habit.CoverFocused:
                    record.FocusKills(enemy, true, out int focuses, out _, out _, out _);
                    return focuses;
                case Habit.HoldOnUnleash:
                    record.UnleashDamage(enemy, out int unleashes, out _);
                    return unleashes;
                case Habit.PressOnFallBack:
                    return record.FallBacksUsed(enemy, out _);
                case Habit.AvoidGuarded:
                    record.GuardPrevented(enemy, out int guards, out _, out _);
                    return guards;
                case Habit.TrapCaution:
                    return record.TrapsArmedBy(enemy);
                default:
                    return 0;
            }
        }

        /// Whether the enemy was able to do what a habit answers: orders need directive points,
        /// guarding needs a Warden, traps need a Trapper.
        public static bool CouldTrigger(Habit habit, FightRecord record, int learnerSide)
        {
            int enemy = 1 - learnerSide;
            switch (habit)
            {
                case Habit.CoverFocused: return record.StartingDirectivePoints >= Directives.Cost(Directive.FocusFire);
                case Habit.HoldOnUnleash: return record.StartingDirectivePoints >= Directives.Cost(Directive.Unleash);
                case Habit.PressOnFallBack: return record.StartingDirectivePoints >= Directives.Cost(Directive.FallBack);
                case Habit.AvoidGuarded: return record.Fielded(enemy, UnitClass.Warden);
                case Habit.TrapCaution: return record.Fielded(enemy, UnitClass.Trapper);
                default: return false;
            }
        }

        private static string FadeWhy(Habit habit)
        {
            switch (habit)
            {
                case Habit.CoverFocused: return "You never focused fire this fight, so our Warden eases off covering the focused.";
                case Habit.HoldOnUnleash: return "You never unleashed this fight, so we ease off holding against it.";
                case Habit.PressOnFallBack: return "You never fell back this fight, so we ease off pressing when you do.";
                case Habit.AvoidGuarded: return "Your Warden covered nobody this fight, so our ranged fighters ease off avoiding the guarded.";
                case Habit.TrapCaution: return "You set no traps this fight, so we ease off our trap caution.";
                default: return "We ease off.";
            }
        }

        private static void Raise(List<Lesson> made, TribeMind mind, FightRecord record, int learnerSide, Habit habit,
            UnitClass subject, int evidence, long fromTick, long toTick, string why)
        {
            int before = mind.HabitValue(habit);
            mind.SetHabit(habit, before + HabitStep);
            Keep(made, mind, new Lesson(mind.TribeId, record.FightId, learnerSide, RuleFor(habit), subject, before,
                mind.HabitValue(habit), evidence, fromTick, toTick, why));
        }

        private static void Keep(List<Lesson> made, TribeMind mind, Lesson lesson)
        {
            mind.Record(lesson);
            made.Add(lesson);
        }

        private static string N(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Seconds(long ticks)
        {
            long safe = ticks < 0 ? 0 : ticks;
            long tenths = safe * 10 / SimClock.TicksPerSecond;
            return (tenths / 10).ToString(CultureInfo.InvariantCulture) + "." + (tenths % 10).ToString(CultureInfo.InvariantCulture) + "s";
        }
    }
}
