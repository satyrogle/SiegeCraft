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
    ///   parry     You parried our cast: our caster now waits out your Hold, so it only soaks (14 Oct 2026).
    ///   feint     You parried 3 of our moves in one fight: we throw a fake beat first (24 Oct 2026).
    ///   threat    Which of your classes cost us most, kept as memory for the story. Changes no tactic.
    /// From a fight that played the parry at the center (FightRecord.ParryCenter, campaign rules version 8; Jacob, 24 Oct
    /// 2026): every move can be parried, so "parry" and "feint" read every parry, and "guard" is cut: it is never learned
    /// again, and a tribe that had it drops it.
    /// From a fight that played the squad first (FightRecord.SquadFirst, campaign rules version 9; Jacob, 24 Oct 2026, later:
    /// "a chief learns only from fights against him, one lesson per fight, on one move") a mind raises at most one habit a
    /// fight (LearnOne), and a wait out or a feint sits on one move.
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
        public const string Parry = "parry";
        public const string Threat = "threat";
        public const string FeintRule = "feint";

        /// Parries in one fight that teach a tribe to feint.
        public const int FeintParries = 3;

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

            if (record.SquadFirst) LearnOne(made, mind, record, learnerSide);
            else RaiseAll(made, mind, record, learnerSide);

            int prevented = record.GuardPrevented(enemy, out int guards, out long guardFrom, out long guardTo);
            // Cut under the parry at the center: never learned again, and dropped by a tribe that had it.
            if (record.ParryCenter && mind.HabitValue(Habit.AvoidGuarded) > 0) mind.SetHabit(Habit.AvoidGuarded, 0);
            if (prevented >= GuardMinPrevented && !record.ParryCenter)
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

        // Every lesson a fight before the squad first teaches, in this order.
        private static void RaiseAll(List<Lesson> made, TribeMind mind, FightRecord record, int learnerSide)
        {
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

            int parries = record.Parries(enemy, out long parryTick, out int casterClass);
            if (parries > 0 && record.ParryCenter)
            {
                var doer = (UnitClass)casterClass;
                string move = record.FirstParryMove(enemy);
                Raise(made, mind, record, learnerSide, Habit.WaitOutParry, doer, parries, parryTick,
                    parryTick + FightRules.StaggerTicks,
                    "At " + Seconds(parryTick) + " you parried our " + SliceClasses.Name(doer) + "'s " +
                    (move.Length > 0 ? move : "move") +
                    (parries == 1 ? string.Empty : ", and " + N(parries - 1) + " more after it") +
                    ". Now we hold each beat until your Hold goes stale. Fake a Hold, then parry the real one.");
                if (parries >= FeintParries)
                    Raise(made, mind, record, learnerSide, Habit.Feint, doer, parries, parryTick, record.EndTick,
                        "You parried " + N(parries) + " of our moves in one fight. Now we throw a fake beat first, and we are " +
                        "open while we fake it: hit us then.");
            }
            else if (parries > 0)
            {
                var caster = (UnitClass)casterClass;
                Raise(made, mind, record, learnerSide, Habit.WaitOutParry, caster, parries, parryTick,
                    parryTick + FightRules.StaggerTicks,
                    "At " + Seconds(parryTick) + " you parried our " + SliceClasses.Name(caster) + "'s cast" +
                    (parries == 1 ? string.Empty : ", and " + N(parries - 1) + " more after it") +
                    ". Now our caster waits out your Hold, so it only soaks.");
            }

            int fallBacks = record.FallBacksUsed(enemy, out long fallTick);
            if (fallBacks > 0 && record.Winner != learnerSide)
            {
                Raise(made, mind, record, learnerSide, Habit.PressOnFallBack, UnitClass.Warden, fallBacks,
                    fallTick, fallTick + BattleSimulation.FallBackTicks,
                    "You fell back " + N(fallBacks) + (fallBacks == 1 ? " time" : " times") + ", first at " + Seconds(fallTick) +
                    ", and we did not win. Now we Unleash the moment you fall back.");
            }
        }

        // The one lesson a fight under the squad first teaches, if any (Choose), raised and put on its move.
        private static void LearnOne(List<Lesson> made, TribeMind mind, FightRecord record, int learnerSide)
        {
            Pick pick = Choose(mind, record, learnerSide, true);
            if (!pick.Found) return;
            int before = mind.HabitValue(pick.Habit);
            mind.SetHabit(pick.Habit, before + HabitStep);
            mind.SetHabitMove(pick.Habit, pick.Move);
            Keep(made, mind, new Lesson(mind.TribeId, record.FightId, learnerSide, RuleFor(pick.Habit), pick.Subject, before,
                mind.HabitValue(pick.Habit), pick.Evidence, pick.From, pick.To, pick.Why, pick.Move));
        }

        // A lesson a fight could teach under the squad first, and its button's count: what you did toward it.
        private struct Pick
        {
            public bool Found;
            public Habit Habit;
            public string Move;
            public UnitClass Subject;
            public int Evidence;
            public long From;
            public long To;
            public string Why;
            public int Count;
            public int Needed;
        }

        // The lesson a fight under the squad first teaches: of the ones it qualifies for, the one answering the button you used
        // most (your parries count for Hold; a tie goes to Hold, Fall Back, Focus Fire, Unleash). ended is false for a fight
        // still going, where a Fall Back lesson cannot yet know who won. A wait out sits on the move you parried most (a big
        // beat before a band's swing); a feint, at FeintParries parries in the fight, on the most parried move the wait out is
        // not on; with both learned, a parry lesson holds the wait out firmer on its own move.
        private static Pick Choose(TribeMind mind, FightRecord record, int learnerSide, bool ended)
        {
            int enemy = 1 - learnerSide;
            var best = new Pick();
            int most = 0;

            int parries = record.Parries(enemy, out _, out _);
            if (parries > 0 && ParryPick(mind, record, enemy, parries, out Pick parry) && parries > most)
            {
                best = parry;
                most = parries;
            }

            int fallBacks = record.FallBacksUsed(enemy, out long fallTick);
            if (fallBacks > 0 && (!ended || record.Winner != learnerSide) && fallBacks > most)
            {
                best = new Pick
                {
                    Found = true, Habit = Habit.PressOnFallBack, Move = string.Empty, Subject = UnitClass.Warden, Evidence = fallBacks,
                    From = fallTick, To = fallTick + BattleSimulation.FallBackTicks, Count = fallBacks, Needed = 1,
                    Why = "You fell back " + N(fallBacks) + (fallBacks == 1 ? " time" : " times") + ", first at " + Seconds(fallTick) +
                          ", and we did not win. Now we Unleash the moment you fall back."
                };
                most = fallBacks;
            }

            int kills = record.FocusKills(enemy, true, out int focuses, out long focusTick, out int victimClass, out long killTicks);
            if (kills > 0 && focuses > most)
            {
                best = new Pick
                {
                    Found = true, Habit = Habit.CoverFocused, Move = string.Empty, Subject = (UnitClass)victimClass, Evidence = kills,
                    From = focusTick, To = focusTick + killTicks, Count = kills, Needed = 1,
                    Why = "At " + Seconds(focusTick) + " you focused our " + SliceClasses.Name((UnitClass)victimClass) +
                          " and killed it " + Seconds(killTicks) + " later. Now our Warden steps in front of whoever you focus."
                };
                most = focuses;
            }

            int unleashDamage = record.UnleashDamage(enemy, out int unleashes, out long unleashTick);
            if (unleashDamage >= UnleashMinDamage && unleashes > most)
            {
                best = new Pick
                {
                    Found = true, Habit = Habit.HoldOnUnleash, Move = string.Empty, Subject = UnitClass.Warden, Evidence = unleashDamage,
                    From = unleashTick, To = unleashTick + BattleSimulation.UnleashTicks, Count = unleashDamage, Needed = UnleashMinDamage,
                    Why = "While your Unleash ran, from " + Seconds(unleashTick) + ", you did " + N(unleashDamage) +
                          " damage to us. Now we Hold the moment you unleash."
                };
            }
            return best;
        }

        // The parry lesson a fight under the squad first could teach, on its move. False when there is none to place.
        private static bool ParryPick(TribeMind mind, FightRecord record, int enemy, int parries, out Pick pick)
        {
            pick = new Pick();
            bool waits = mind.HasHabit(Habit.WaitOutParry) && mind.WaitOutMove.Length > 0;
            bool feints = mind.HasHabit(Habit.Feint) && mind.FeintMove.Length > 0;
            Habit habit = Habit.WaitOutParry;
            string move = string.Empty;
            if (!waits) move = record.MostParriedMove(enemy, feints ? mind.FeintMove : string.Empty);
            else
            {
                if (!feints && parries >= FeintParries)
                {
                    move = record.MostParriedMove(enemy, mind.WaitOutMove);
                    if (move.Length > 0) habit = Habit.Feint;
                }
                if (move.Length == 0) move = mind.WaitOutMove;
            }
            if (move.Length == 0) return false;
            int times = record.ParriesOf(enemy, move, out long first, out int doerClass);
            if (times <= 0) return false;
            var doer = (UnitClass)doerClass;
            pick.Found = true;
            pick.Habit = habit;
            pick.Move = move;
            pick.Subject = doer;
            pick.Evidence = times;
            pick.From = first;
            if (habit == Habit.Feint)
            {
                pick.To = record.EndTick;
                pick.Count = parries;
                pick.Needed = FeintParries;
                pick.Why = "You parried " + N(parries) + " of our moves in one fight, " + N(times) + " of them our " + move +
                           ". Now our " + move + " throws a fake beat first, and we are open while we fake it: hit us then.";
            }
            else
            {
                pick.To = first + FightRules.StaggerTicks;
                pick.Count = times;
                pick.Needed = 1;
                pick.Why = "At " + Seconds(first) + " you parried our " + SliceClasses.Name(doer) + "'s " + move +
                           (times == 1 ? string.Empty : ", " + N(times) + " times in all") + ". Now our " + move +
                           " waits out your Hold: Hold again for the late beat, or Focus Fire or Fall Back while we wait.";
            }
            return true;
        }

        /// Recomputes the number a rule measured, from a record. A replay of the lesson's fight must
        /// give the same number. Fade lessons measure how often the enemy did the thing: always 0.
        /// A claim the replay does not bear out gives -1, so the lesson fails: a focus lesson names
        /// the class that was killed, and a fall back lesson says the learner did not win. Unknown
        /// rules give -1 too, so they never verify. move is the lesson's (Lesson.Move): under the squad first a wait out or feint
        /// lesson counts the parries of its own move, whose first doer must be its subject, and a feint also needs FeintParries
        /// parries in the fight.
        public static int Evidence(string rule, FightRecord record, int learnerSide, UnitClass subject, string move = "")
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
            if (record.SquadFirst && !string.IsNullOrEmpty(move) &&
                (string.Equals(rule, Parry, StringComparison.Ordinal) || string.Equals(rule, FeintRule, StringComparison.Ordinal)))
            {
                int times = record.ParriesOf(enemy, move, out _, out int doerClass);
                if (times <= 0 || doerClass != (int)subject) return -1;
                if (string.Equals(rule, FeintRule, StringComparison.Ordinal) && record.Parries(enemy, out _, out _) < FeintParries) return -1;
                return times;
            }
            if (string.Equals(rule, Parry, StringComparison.Ordinal))
            {
                int parries = record.Parries(enemy, out _, out int casterClass);
                return parries > 0 && casterClass != (int)subject ? -1 : parries;
            }
            if (string.Equals(rule, FallBack, StringComparison.Ordinal))
                return record.Winner == learnerSide ? -1 : record.FallBacksUsed(enemy, out _);
            if (string.Equals(rule, Guard, StringComparison.Ordinal))
                return record.GuardPrevented(enemy, out _, out _, out _);
            if (string.Equals(rule, FeintRule, StringComparison.Ordinal))
            {
                int parries = record.Parries(enemy, out _, out int doerClass);
                return parries < FeintParries || doerClass != (int)subject ? -1 : parries;
            }
            if (string.Equals(rule, Threat, StringComparison.Ordinal))
                return record.ThreatByClass(learnerSide, out _)[(int)subject];
            return -1;
        }

        /// What each habit's lesson stands at in a fight so far, for the screen (Campaign.TribeLearning): only what the enemy
        /// has started on, in habit order. From a fight under the parry at the center every parry counts toward waiting out
        /// the parry and, at FeintParries, the feint; the guard lesson is cut there.
        public static IReadOnlyList<LessonProgress> Progress(FightRecord record, int learnerSide)
        {
            var list = new List<LessonProgress>();
            if (record == null || (learnerSide != 0 && learnerSide != 1)) return list;
            int enemy = 1 - learnerSide;
            int kills = record.FocusKills(enemy, true, out _, out _, out _, out _);
            if (kills > 0) list.Add(new LessonProgress(Habit.CoverFocused, kills, 1, Play(Habit.CoverFocused)));
            int unleashDamage = record.UnleashDamage(enemy, out _, out _);
            if (unleashDamage > 0) list.Add(new LessonProgress(Habit.HoldOnUnleash, unleashDamage, UnleashMinDamage, Play(Habit.HoldOnUnleash)));
            int fallBacks = record.FallBacksUsed(enemy, out _);
            if (fallBacks > 0) list.Add(new LessonProgress(Habit.PressOnFallBack, fallBacks, 1, Play(Habit.PressOnFallBack)));
            int prevented = record.GuardPrevented(enemy, out _, out _, out _);
            if (prevented > 0 && !record.ParryCenter) list.Add(new LessonProgress(Habit.AvoidGuarded, prevented, GuardMinPrevented, Play(Habit.AvoidGuarded)));
            int parries = record.Parries(enemy, out _, out _);
            if (parries > 0)
            {
                list.Add(new LessonProgress(Habit.WaitOutParry, parries, 1, Play(Habit.WaitOutParry)));
                if (record.ParryCenter) list.Add(new LessonProgress(Habit.Feint, parries, FeintParries, Play(Habit.Feint)));
            }
            return list;
        }

        /// What a mind is learning from you in a fight so far: under the squad first only the one lesson it would take if the
        /// fight ended now (Choose), with its move; otherwise every habit's, as Progress(record, learnerSide) gives them.
        public static IReadOnlyList<LessonProgress> Progress(TribeMind mind, FightRecord record, int learnerSide)
        {
            if (mind == null || record == null || !record.SquadFirst) return Progress(record, learnerSide);
            var list = new List<LessonProgress>();
            if (learnerSide != 0 && learnerSide != 1) return list;
            Pick pick = Choose(mind, record, learnerSide, false);
            if (pick.Found)
                list.Add(new LessonProgress(pick.Habit, pick.Count, pick.Needed,
                    pick.Habit == Habit.WaitOutParry ? WaitOutPlay : Play(pick.Habit), pick.Move));
            return list;
        }

        /// The play a learned wait out opens under the squad first, where it holds its beat at least a second.
        public const string WaitOutPlay = "Hold again for the late beat, or Focus Fire or Fall Back while he waits.";

        /// The play a habit's lesson opens for you (Jacob, 24 Oct 2026: "whatever a rival learned to answer, the same button of
        /// yours now baits him into an opening"). Empty for one that opens none. Claude's placeholder words.
        public static string Play(Habit habit)
        {
            switch (habit)
            {
                case Habit.CoverFocused: return "Parry first: nobody can cover a staggered fighter. Or focus a decoy, then switch: his Wardens stay stuck for 5 s.";
                case Habit.HoldOnUnleash: return "A Riposte lands before his Hold. Unleash just before his cast, and his cast comes late.";
                case Habit.PressOnFallBack: return "Fall back behind a Hold, and he wastes 2 points and his own nerve.";
                case Habit.WaitOutParry: return "Fake a Hold, then parry the real beat.";
                case Habit.Feint: return "Hit him while he fakes it: Focus Fire or Unleash.";
                default: return string.Empty;
            }
        }

        /// The play behind a lesson a tribe holds against you (TribeMind.Lessons), by its rule; empty for fades, threat memory and
        /// a rule that raises nothing.
        public static string PlayFor(string rule)
        {
            return RematchClue.Raises(rule, out Habit habit) ? Play(habit) : string.Empty;
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
                case Habit.WaitOutParry: return Parry;
                case Habit.Feint: return FeintRule;
                default: return string.Empty;
            }
        }

        /// Times the enemy did what a habit answers: focus orders (not on Wardens), Unleashes,
        /// Fall Backs, guarded hits, traps armed, or parries tried (a parry, or a Hold our caster waited out).
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
                case Habit.WaitOutParry:
                    return record.Parries(enemy, out _, out _) + record.WaitOuts(learnerSide);
                case Habit.Feint:
                    return record.Parries(enemy, out _, out _);
                default:
                    return 0;
            }
        }

        /// Whether the enemy was able to do what a habit answers: orders need directive points,
        /// guarding needs a Warden, traps need a Trapper, a parry needs our cast.
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
                case Habit.WaitOutParry:
                    return record.StartingDirectivePoints >= Directives.Cost(Directive.Hold) &&
                           (record.ParryCenter ? record.Beats(learnerSide) : record.Casts(learnerSide)) > 0;
                case Habit.Feint:
                    return record.ParryCenter && record.StartingDirectivePoints >= Directives.Cost(Directive.Hold) &&
                           record.Beats(learnerSide) > 0;
                default: return false;
            }
        }

        private static string FadeWhy(Habit habit)
        {
            switch (habit)
            {
                case Habit.CoverFocused: return "You never focused fire on anyone but a Warden this fight, so our Warden eases off covering the focused.";
                case Habit.HoldOnUnleash: return "You never unleashed this fight, so we ease off holding against it.";
                case Habit.PressOnFallBack: return "You never fell back this fight, so we ease off pressing when you do.";
                case Habit.AvoidGuarded: return "Your Warden covered nobody this fight, so our ranged fighters ease off avoiding the guarded.";
                case Habit.TrapCaution: return "You set no traps this fight, so we ease off our trap caution.";
                case Habit.WaitOutParry: return "You never tried to parry our cast this fight, so we ease off waiting out your Hold.";
                case Habit.Feint: return "You never parried us this fight, so we ease off our fake beats.";
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
