using System.Collections.Generic;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// What a tribe is learning from you in a fight still going (Jacob, 24 Oct 2026, DESIGN_LOCKS "The parry at the center",
    /// part 3: "what he is learning now"): the habit, what you have done toward it so far, what it takes, and the play it will
    /// open. Ready when the fight's end will teach it; a Fall Back lesson also needs them not to win.
    public readonly struct LessonProgress
    {
        public LessonProgress(Habit habit, int count, int needed, string play, string move = "")
        {
            Habit = habit;
            Count = count;
            Needed = needed;
            Play = play ?? string.Empty;
            Move = move ?? string.Empty;
        }

        public Habit Habit { get; }
        public string Rule => LessonRules.RuleFor(Habit);
        public int Count { get; }
        public int Needed { get; }
        public bool Ready => Count >= Needed;
        public string Play { get; }

        /// Under the squad first the move a wait out or feint would sit on ("Cleave"); empty otherwise.
        public string Move { get; }
    }

    /// What a hunter is learning from you in a fight still going (DESIGN_LOCKS "The parry at the center", part 3: "You've parried
    /// 7 of his swings. Beat you, and he learns Delay."): your favourite button so far and how many times, and the trait he
    /// learns if he beats you now. Learns is false when you used no button yet, or he has no room (he takes a level instead).
    public readonly struct HunterProgress
    {
        public static readonly HunterProgress None = new HunterProgress(Directive.Hold, 0, false, HunterTrait.LongReach);

        public HunterProgress(Directive button, int times, bool learns, HunterTrait trait)
        {
            Button = button;
            Times = times;
            Learns = learns;
            Trait = trait;
        }

        /// Hold counts your parries under the parry at the center, else your Holds.
        public Directive Button { get; }
        public int Times { get; }
        public bool Learns { get; }
        public HunterTrait Trait { get; }
    }

    /// What Spare or Execute gives before you choose, while a chief kneels (DESIGN_LOCKS "The parry at the center", part 3).
    /// Spare: his band from this fight joins you under their names, his tribe joins, and his order is yours. Execute: you take
    /// his Princess piece, and the same chief leads his tribe next run. Either way the bounty on you rises.
    public sealed class VerdictPreview
    {
        internal VerdictPreview(string tribeId, IReadOnlyList<UnitClass> joinClasses, IReadOnlyList<string> joinNames,
            string assistName, int assistGrade, string pieceName, int heat)
        {
            TribeId = tribeId ?? string.Empty;
            JoinClasses = joinClasses;
            JoinNames = joinNames;
            AssistName = assistName ?? string.Empty;
            AssistGrade = assistGrade;
            PieceName = pieceName ?? string.Empty;
            BountyHeat = heat;
        }

        public string TribeId { get; }
        public IReadOnlyList<UnitClass> JoinClasses { get; }
        public IReadOnlyList<string> JoinNames { get; }
        public string AssistName { get; }
        public int AssistGrade { get; }
        public string PieceName { get; }
        public int BountyHeat { get; }
    }

    public sealed partial class Campaign
    {
        /// What this fight's tribe is learning from you so far (LessonProgress), in habit order, only what you have started on.
        /// Under the squad first (campaign rules 9) only the one lesson it would take now, from the mind that learns from this
        /// fight (a chief's own in his fight). Empty outside a tribe's fight.
        public IReadOnlyList<LessonProgress> TribeLearning(CampaignFight fight)
        {
            var list = new List<LessonProgress>();
            if (fight == null || fight.Encounter == null || fight.Encounter.Tribe == null) return list;
            FightRecord record = FightRecord.SoFar(fight.Sim);
            TribeMind mind = LearnerMind(fight.Encounter, record.SquadFirst);
            if (mind == null) return list;
            return LessonRules.Progress(mind, record, 1);
        }

        /// What this fight's hunter is learning from you so far (HunterProgress), or HunterProgress.None without one.
        public HunterProgress HunterLearning(CampaignFight fight)
        {
            CampaignHunter hunter = fight == null || fight.Encounter == null ? null : fight.Encounter.Hunter;
            if (hunter == null) return HunterProgress.None;
            bool parryCenter = fight.Sim.Fight.ParryCenter;
            int button = Favourite(fight.Sim.Events, parryCenter, out int times);
            if (button < 0) return HunterProgress.None;
            return new HunterProgress((Directive)button, times, WouldLearn(hunter, button, parryCenter, out HunterTrait trait), trait);
        }

        /// What Spare or Execute gives (VerdictPreview), or null while no chief kneels in this fight.
        public VerdictPreview Verdict(CampaignFight fight)
        {
            if (fight == null || fight.Encounter == null || fight.Encounter.Kind != EncounterKind.Chief || !fight.Sim.AwaitingVerdict)
                return null;
            CampaignTribe tribe = fight.Encounter.Tribe;
            var classes = new List<UnitClass>();
            var names = new List<string>();
            for (int i = 0; i < fight.Sim.Units.Count; i++)
            {
                BattleUnit u = fight.Sim.Units[i];
                if (u.Side != 1 || u.Role != UnitRole.Fighter || u.Benched) continue;
                classes.Add(u.Class);
                names.Add(fight.NameOf(u.Id));
            }
            CampaignChief chief = ChiefOf(tribe.Id);
            GearStat stat = CampaignGearRules.ChiefStat(tribe.Id);
            string piece = CampaignGearRules.Name(tribe, stat, GearTier.Princess, chief == null ? tribe.ChiefName : chief.Name);
            return new VerdictPreview(tribe.Id, classes, names, tribe.AssistName, tribe.AssistGrade, piece, BountyRules.HeatPerChief);
        }

        // The trait a hunter would learn against this button now, as TryLearnAgainst picks it, without learning it.
        private bool WouldLearn(CampaignHunter hunter, int favourite, bool parryCenter, out HunterTrait learned)
        {
            learned = HunterTrait.LongReach;
            if (hunter.Traits.Count >= HunterTraits.MaxPerHunter) return false;
            HunterTrait[] counters = HunterTraits.Counters((Directive)favourite, parryCenter);
            for (int i = 0; i < counters.Length; i++)
            {
                if (!hunter.CanLearn(counters[i])) continue;
                learned = counters[i];
                return true;
            }
            DeterministicRng dice = HunterNames.Dice(Seed, hunter.Id + ".learn." + hunter.Traits.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
            for (int tries = 0; tries < 64; tries++)
            {
                var trait = (HunterTrait)dice.Next(HunterTraits.Rolled);
                if (!hunter.CanLearn(trait)) continue;
                learned = trait;
                return true;
            }
            return false;
        }
    }
}
