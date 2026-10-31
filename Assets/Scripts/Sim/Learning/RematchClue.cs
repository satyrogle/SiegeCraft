using System;

namespace KindredSiege.Sim.Learning
{
    /// The chief who comes back smarter, shown before the fight (Jacob, 11 Oct 2026: C7 is this clue
    /// only, and the tribe war is cut). A tribe's mind keeps every lesson it learned from your fights,
    /// each with its fight and tick window. The clue is the newest thing it learned from you that it
    /// still does, so a fight card can say what changed and point at the fight that taught it. A habit
    /// that has faded back out of use is never claimed; fades and threat memory are not clues.
    public sealed class RematchClue
    {
        private RematchClue(string tribeId, string chiefName, Habit habit, Lesson lesson, ArchivedFight source)
        {
            TribeId = tribeId;
            ChiefName = chiefName;
            Habit = habit;
            Lesson = lesson;
            Source = source;
        }

        public string TribeId { get; }

        /// Who remembers you, for example "the Bloodridge chief".
        public string ChiefName { get; }

        /// What they picked up from you and still do.
        public Habit Habit { get; }
        public string HabitName => TribeMind.HabitName(Habit);

        /// The lesson behind it: the sentence a player reads, its fight and where in that fight.
        public Lesson Lesson { get; }
        public string Text => Lesson.Why;
        public string FightId => Lesson.FightId;

        /// The move the lesson sits on under the squad first ("Cleave"), else empty.
        public string Move => Lesson.Move;

        /// The fight that taught it, from the campaign's archive. Campaign.Archive.TryVerifyLesson(Lesson)
        /// plays it again and checks the lesson's number comes out the same. Null if the archive lacks it.
        public ArchivedFight Source { get; }

        /// The clue for a fight against this tribe, or null when it has learned nothing from you that it
        /// still does. Your own cult and the Sovereign's hunters never have one.
        public static RematchClue For(Campaign campaign, string tribeId)
        {
            if (campaign == null) throw new ArgumentNullException(nameof(campaign));
            if (tribeId == CampaignTribes.CultId) return null;
            return From(campaign, tribeId, campaign.Mind(tribeId));
        }

        /// The clue for this encounter, from the mind that will fight you in it (Campaign.MindFor): from campaign rules 9 a
        /// chief's comes from his own mind, which only fights against him teach.
        public static RematchClue For(Campaign campaign, Encounter encounter)
        {
            if (campaign == null) throw new ArgumentNullException(nameof(campaign));
            if (encounter == null || encounter.Tribe == null || encounter.Tribe.Id == CampaignTribes.CultId) return null;
            return From(campaign, encounter.Tribe.Id, campaign.MindFor(encounter));
        }

        private static RematchClue From(Campaign campaign, string tribeId, TribeMind mind)
        {
            if (mind == null) return null;
            for (int i = mind.Lessons.Count - 1; i >= 0; i--)
            {
                Lesson lesson = mind.Lessons[i];
                if (!Raises(lesson.Rule, out Habit habit) || lesson.ValueAfter <= lesson.ValueBefore || !mind.HasHabit(habit)) continue;
                return new RematchClue(tribeId, mind.LeaderName, habit, lesson, campaign.Archive.Find(lesson.FightId));
            }
            return null;
        }

        /// The habit a LessonRules rule raises. Fades ("fade." rules) and threat memory raise none.
        public static bool Raises(string rule, out Habit habit)
        {
            switch (rule)
            {
                case LessonRules.Focus: habit = Habit.CoverFocused; return true;
                case LessonRules.Unleash: habit = Habit.HoldOnUnleash; return true;
                case LessonRules.FallBack: habit = Habit.PressOnFallBack; return true;
                case LessonRules.Guard: habit = Habit.AvoidGuarded; return true;
                case LessonRules.Parry: habit = Habit.WaitOutParry; return true;
                case LessonRules.FeintRule: habit = Habit.Feint; return true;
                default: habit = Habit.CoverFocused; return false;
            }
        }
    }
}
