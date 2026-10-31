using System;
using System.Collections.Generic;
using System.Globalization;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// A named hunter's trait (Jacob, 18 Oct 2026: "16 traits", the hunters as the mercenaries of Assassin's Creed
    /// Odyssey). Each one makes one of your four buttons the answer, four traits to a button, in this order: Hold,
    /// Fall Back, Focus Fire, Unleash. The list, names and words are Claude's placeholders; numbers are in BandRules.
    ///
    /// The parry at the center (Jacob, 24 Oct 2026; DESIGN_LOCKS "The parry at the center", part 3) makes 17: Unstoppable
    /// is Delay (their beat comes late), Quickstrike is Double swing (light then heavy), and Feint is new (a fake beat
    /// first). The names and words below are those; in a fight under the older rules Unstoppable and Quickstrike still
    /// play as before. Feint is only ever learned, never rolled at recruitment (HunterTraits.Rolled).
    public enum HunterTrait
    {
        LongReach = 0,
        Quickstrike = 1,
        BigHit = 2,
        Terror = 3,
        Unstoppable = 4,
        Shieldbreaker = 5,
        Firebomb = 6,
        Berserk = 7,
        Unshakable = 8,
        Mender = 9,
        WarCry = 10,
        SecondWind = 11,
        Evasive = 12,
        ThickHide = 13,
        IronWall = 14,
        Banner = 15,
        Feint = 16
    }

    /// A named hunter's weakness: one button that hurts them more than it should.
    public enum HunterWeakness
    {
        Parry = 0,
        Dodge = 1,
        Focus = 2,
        Scare = 3
    }

    public static class HunterTraits
    {
        public const int Count = 17;

        /// The traits a hunter can roll for themselves: all but Feint, which is only learned (24 Oct 2026).
        public const int Rolled = 16;

        /// A hunter never has more traits than this. Past it, beating you only adds levels.
        public const int MaxPerHunter = 3;

        /// Quickstrike: their heavy swings come every this percent of the usual wait.
        public const int QuickstrikePercent = 50;

        private static readonly Directive[] Answers = { Directive.Hold, Directive.FallBack, Directive.FocusFire, Directive.Unleash };

        /// The one button that answers this trait, for the card and its colour. Delay and Double swing are answered by the
        /// parry, a Feint by hitting the faker (Focus Fire).
        public static Directive Answer(HunterTrait trait)
        {
            if (trait == HunterTrait.Feint) return Directive.FocusFire;
            if (trait == HunterTrait.Unstoppable) return Directive.Hold;
            return Answers[(int)trait / 4];
        }

        public static string Name(HunterTrait trait)
        {
            switch (trait)
            {
                case HunterTrait.LongReach: return "Long Reach";
                case HunterTrait.Quickstrike: return "Double Swing";
                case HunterTrait.BigHit: return "Big Hit";
                case HunterTrait.Terror: return "Terror";
                case HunterTrait.Unstoppable: return "Delay";
                case HunterTrait.Shieldbreaker: return "Shieldbreaker";
                case HunterTrait.Firebomb: return "Firebomb";
                case HunterTrait.Berserk: return "Berserk";
                case HunterTrait.Unshakable: return "Unshakable";
                case HunterTrait.Mender: return "Mender";
                case HunterTrait.WarCry: return "War Cry";
                case HunterTrait.SecondWind: return "Second Wind";
                case HunterTrait.Evasive: return "Evasive";
                case HunterTrait.ThickHide: return "Thick Hide";
                case HunterTrait.IronWall: return "Iron Wall";
                case HunterTrait.Banner: return "War Banner";
                case HunterTrait.Feint: return "Feint";
                default: return string.Empty;
            }
        }

        /// One line for the card: what it does and the button that answers it.
        public static string Describe(HunterTrait trait)
        {
            switch (trait)
            {
                case HunterTrait.LongReach: return "They reach through your Fall Back, but nobody covers them while they reach. Hold and parry.";
                case HunterTrait.Quickstrike: return "Every heavy swing comes twice, light then heavy. Parry both and the second cracks them twice; Fall Back clears only the first.";
                case HunterTrait.BigHit: return "The hunter winds up a huge blow on your front row. Hold and parry it.";
                case HunterTrait.Terror: return "Their blows shake your nerve twice as hard unless you are holding. Hold.";
                case HunterTrait.Unstoppable: return "Their beat comes late. Fake a Hold, then parry the late one.";
                case HunterTrait.Shieldbreaker: return "A heavy swing on your Hold breaks it and shakes you. Fall Back, or parry it clean.";
                case HunterTrait.Firebomb: return "The hunter throws fire at your front row. Parry the throw, or Fall Back.";
                case HunterTrait.Berserk: return "Hurt the hunter and their band goes berserk for a few seconds, hitting twice as hard. Fall Back until it passes, or parry.";
                case HunterTrait.Unshakable: return "Your Unleash never shakes their nerve, and they stop Holding against it. Focus Fire.";
                case HunterTrait.Mender: return "The hunter patches up their band every two seconds. Focus Fire on the hunter.";
                case HunterTrait.WarCry: return "The hunter winds up a cry that steadies their band. Focus Fire on the hunter to break it.";
                case HunterTrait.SecondWind: return "Once a fight, the hunter rallies them back from breaking. Focus Fire on the hunter.";
                case HunterTrait.Evasive: return "They slip plain blows. Unleashed blows never miss them. Unleash.";
                case HunterTrait.ThickHide: return "The one you focus braces: your blows do half, but braced he cannot swing.";
                case HunterTrait.IronWall: return "The hunter raises a Steel Storm wall. Unleash smashes it.";
                case HunterTrait.Banner: return "Their nerve will not break while their banner flies. Unleash tears it down.";
                case HunterTrait.Feint: return "A fake beat first: the tell flashes and nothing comes. Hit them while they fake it.";
                default: return string.Empty;
            }
        }

        /// The traits that wind up a move in the chief spell's slot. A hunter has at most one.
        public static bool IsCast(HunterTrait trait)
        {
            return trait == HunterTrait.BigHit || trait == HunterTrait.Firebomb || trait == HunterTrait.WarCry || trait == HunterTrait.IronWall;
        }

        /// Two traits that cannot sit on one hunter: the same one twice, two casts, Long Reach with Unstoppable (their
        /// swings would have no answer), and Terror with Shieldbreaker (one says Hold, the other punishes it).
        public static bool Clashes(HunterTrait a, HunterTrait b)
        {
            if (a == b || (IsCast(a) && IsCast(b))) return true;
            return Pair(a, b, HunterTrait.LongReach, HunterTrait.Unstoppable) || Pair(a, b, HunterTrait.Terror, HunterTrait.Shieldbreaker);
        }

        /// A trait that takes away the button a weakness rewards cannot sit with it.
        public static bool Denies(HunterTrait trait, HunterWeakness weakness)
        {
            return (trait == HunterTrait.Unstoppable && weakness == HunterWeakness.Parry) ||
                (trait == HunterTrait.LongReach && weakness == HunterWeakness.Dodge) ||
                (trait == HunterTrait.Unshakable && weakness == HunterWeakness.Scare);
        }

        /// What a hunter learns against the order you gave most in the fight they beat you in (Jacob's go, 18 Oct 2026:
        /// "they learn your favourite tricks"), first choice first.
        internal static HunterTrait[] Counters(Directive favourite)
        {
            switch (favourite)
            {
                case Directive.Hold: return new[] { HunterTrait.Unstoppable, HunterTrait.Shieldbreaker };
                case Directive.FallBack: return new[] { HunterTrait.LongReach };
                case Directive.FocusFire: return new[] { HunterTrait.ThickHide, HunterTrait.Evasive };
                case Directive.Unleash: return new[] { HunterTrait.Unshakable };
                default: return new HunterTrait[0];
            }
        }

        /// Under the parry at the center (Jacob, 24 Oct 2026): parried most, a rhythm, one more each time he beats you, in the
        /// order Delay, Double swing, Feint; fell back most, Long Reach; focused most, Thick Hide; unleashed most, Unshakable.
        /// Shieldbreaker and Evasive stay traits hunters roll for themselves.
        public static HunterTrait[] Counters(Directive favourite, bool parryCenter)
        {
            if (!parryCenter) return Counters(favourite);
            switch (favourite)
            {
                case Directive.Hold: return new[] { HunterTrait.Unstoppable, HunterTrait.Quickstrike, HunterTrait.Feint };
                case Directive.FallBack: return new[] { HunterTrait.LongReach };
                case Directive.FocusFire: return new[] { HunterTrait.ThickHide };
                case Directive.Unleash: return new[] { HunterTrait.Unshakable };
                default: return new HunterTrait[0];
            }
        }

        /// The battle flag of a trait, or None for the ones that work through the side's spell or swing rate.
        internal static BandTrait Flag(HunterTrait trait)
        {
            switch (trait)
            {
                case HunterTrait.LongReach: return BandTrait.LongReach;
                case HunterTrait.Terror: return BandTrait.Terror;
                case HunterTrait.Unstoppable: return BandTrait.Unstoppable;
                case HunterTrait.Shieldbreaker: return BandTrait.Shieldbreaker;
                case HunterTrait.Berserk: return BandTrait.Berserk;
                case HunterTrait.Unshakable: return BandTrait.Unshakable;
                case HunterTrait.Mender: return BandTrait.Mender;
                case HunterTrait.SecondWind: return BandTrait.SecondWind;
                case HunterTrait.Evasive: return BandTrait.Evasive;
                case HunterTrait.ThickHide: return BandTrait.ThickHide;
                case HunterTrait.Banner: return BandTrait.Banner;
                case HunterTrait.Feint: return BandTrait.Feint;
                default: return BandTrait.None;
            }
        }

        /// The chief spell a cast trait winds up.
        internal static ChiefSpell Spell(HunterTrait trait)
        {
            switch (trait)
            {
                case HunterTrait.Firebomb: return ChiefSpell.Firebomb;
                case HunterTrait.WarCry: return ChiefSpell.WarCry;
                case HunterTrait.IronWall: return ChiefSpell.SteelStorm;
                default: return ChiefSpell.Eclipse;
            }
        }

        private static bool Pair(HunterTrait a, HunterTrait b, HunterTrait x, HunterTrait y)
        {
            return (a == x && b == y) || (a == y && b == x);
        }
    }

    public static class HunterWeaknesses
    {
        public const int Count = 4;

        private static readonly Directive[] Answers = { Directive.Hold, Directive.FallBack, Directive.FocusFire, Directive.Unleash };

        /// The button a weakness rewards.
        public static Directive Answer(HunterWeakness weakness)
        {
            return Answers[(int)weakness];
        }

        public static string Name(HunterWeakness weakness)
        {
            switch (weakness)
            {
                case HunterWeakness.Parry: return "Weak to the parry";
                case HunterWeakness.Dodge: return "Off balance";
                case HunterWeakness.Focus: return "Weak to Focus Fire";
                default: return "Skittish";
            }
        }

        public static string Describe(HunterWeakness weakness)
        {
            switch (weakness)
            {
                case HunterWeakness.Parry: return "A parried swing cracks twice the nerve.";
                case HunterWeakness.Dodge: return "A swing you Fall Back from staggers them like a parry.";
                case HunterWeakness.Focus: return "Focused blows hit them half as hard again.";
                default: return "Your Unleash shakes them twice as hard, even through a Hold.";
            }
        }

        internal static BandTrait Flag(HunterWeakness weakness)
        {
            switch (weakness)
            {
                case HunterWeakness.Parry: return BandTrait.WeakToParry;
                case HunterWeakness.Dodge: return BandTrait.WeakToDodge;
                case HunterWeakness.Focus: return BandTrait.WeakToFocus;
                default: return BandTrait.WeakToScare;
            }
        }
    }

    /// Where a hunter learned a trait (Jacob, 24 Oct 2026: "Hunters keep where they learned each trait"): the fight they beat
    /// you in, the button you used most there and how many times. Kept from campaign rules version 8; a trait rolled at
    /// recruitment, or learned before, has none.
    public sealed class HunterLesson
    {
        public HunterLesson(HunterTrait trait, string fightId, Directive button, int times)
        {
            Trait = trait;
            FightId = fightId ?? string.Empty;
            Button = button;
            Times = Math.Max(0, times);
        }

        public HunterTrait Trait { get; }
        public string FightId { get; }

        /// Your favourite button in that fight: Hold (counted in parries), Fall Back, Focus Fire or Unleash.
        public Directive Button { get; }
        public int Times { get; }
    }

    /// One hunter of the company: a name, a class, levels, up to three traits and one weakness. The same person all
    /// campaign until you kill them.
    public sealed class CampaignHunter
    {
        private readonly List<HunterTrait> _traits = new List<HunterTrait>();
        private readonly List<HunterLesson> _lessons = new List<HunterLesson>();

        internal CampaignHunter(string id, string name, string title, UnitClass unitClass, int level, HunterWeakness weakness)
        {
            Id = id;
            Name = name ?? string.Empty;
            Title = title ?? string.Empty;
            Class = unitClass;
            Level = Math.Max(0, Math.Min(HunterRules.MaxLevel, level));
            Weakness = weakness;
        }

        public string Id { get; }

        /// Their name alone, for a label: "Vessk".
        public string Name { get; }

        /// Their title: "Twice Paid".
        public string Title { get; }

        /// Both: "Vessk, Twice Paid".
        public string FullName => Name + ", " + Title;

        public UnitClass Class { get; }

        /// Their own levels: what they started with, plus HunterRules.PromotionLevels each run they beat you. A fight
        /// adds the company's might on top (Campaign.HunterLevel).
        public int Level { get; internal set; }

        public IReadOnlyList<HunterTrait> Traits => _traits;
        public HunterWeakness Weakness { get; internal set; }

        /// Fights they won against you.
        public int Wins { get; internal set; }

        /// Fights you won against them that they walked away from.
        public int Escapes { get; internal set; }

        public bool Has(HunterTrait trait) => _traits.Contains(trait);

        /// Where they learned their traits, oldest first (HunterLesson).
        public IReadOnlyList<HunterLesson> Lessons => _lessons;

        /// The lesson behind one of their traits, or null when they rolled it or learned it before rules version 8.
        public HunterLesson LessonFor(HunterTrait trait)
        {
            for (int i = 0; i < _lessons.Count; i++) if (_lessons[i].Trait == trait) return _lessons[i];
            return null;
        }

        internal void Record(HunterLesson lesson)
        {
            if (lesson != null) _lessons.Add(lesson);
        }

        /// They wind up a move of their own (a cast trait).
        public bool Casts
        {
            get
            {
                for (int i = 0; i < _traits.Count; i++) if (HunterTraits.IsCast(_traits[i])) return true;
                return false;
            }
        }

        /// What this hunter brings to a fight's rules: their traits' flags and their weakness.
        public BandTrait Band
        {
            get
            {
                BandTrait band = HunterWeaknesses.Flag(Weakness);
                for (int i = 0; i < _traits.Count; i++) band |= HunterTraits.Flag(_traits[i]);
                return band;
            }
        }

        internal bool CanLearn(HunterTrait trait)
        {
            if (_traits.Count >= HunterTraits.MaxPerHunter || (int)trait < 0 || (int)trait >= HunterTraits.Count) return false;
            for (int i = 0; i < _traits.Count; i++) if (HunterTraits.Clashes(_traits[i], trait)) return false;
            return true;
        }

        /// Adds the trait; a weakness the trait takes away moves to the first one none of their traits takes away.
        internal void Learn(HunterTrait trait)
        {
            _traits.Add(trait);
            if (!WeaknessAllowed(Weakness))
                for (int w = 0; w < HunterWeaknesses.Count; w++)
                    if (WeaknessAllowed((HunterWeakness)w))
                    {
                        Weakness = (HunterWeakness)w;
                        break;
                    }
        }

        internal bool WeaknessAllowed(HunterWeakness weakness)
        {
            for (int i = 0; i < _traits.Count; i++) if (HunterTraits.Denies(_traits[i], weakness)) return false;
            return true;
        }
    }

    /// The hunters' ladder (Jacob, 18 Oct 2026: the hunters are the rival, the 25th tribe, built like Odyssey's
    /// mercenaries and growing across the whole game). Size hunters ranked top to bottom, with you among them, first at
    /// the bottom. Beat one ranked above you and you take their place; a hunter who beats you climbs one place; one you
    /// kill is gone and a new recruit joins at the bottom. The company is camp state: it outlives every run.
    public sealed class HunterCompany
    {
        /// Your place on the ladder.
        public const string You = "you";

        /// Hunters on the ladder. Every one you kill is replaced, so it never runs short.
        public const int Size = 10;

        private readonly List<string> _ladder = new List<string>();
        private readonly List<CampaignHunter> _hunters = new List<CampaignHunter>();

        private HunterCompany() { }

        /// Everyone on the ladder by id, top first, You among them.
        public IReadOnlyList<string> Ladder => _ladder;

        /// Your rank, 1 at the top.
        public int PlayerRank => _ladder.IndexOf(You) + 1;

        /// The living hunters, top first.
        public IReadOnlyList<CampaignHunter> Hunters
        {
            get
            {
                var list = new List<CampaignHunter>();
                for (int i = 0; i < _ladder.Count; i++)
                {
                    CampaignHunter h = Find(_ladder[i]);
                    if (h != null) list.Add(h);
                }
                return list;
            }
        }

        /// The last hunter who beat you, while they live: they come for you first when the bounty lets them. Empty
        /// when none.
        public string NemesisId { get; internal set; } = string.Empty;

        public CampaignHunter Nemesis => Find(NemesisId);

        /// Hunters ever recruited, the starting ladder included. Each new one takes the next name.
        public int Recruited { get; private set; }

        /// A hunter's rank, 1 at the top, or 0 when they are not on the ladder.
        public int Rank(CampaignHunter hunter)
        {
            return hunter == null ? 0 : _ladder.IndexOf(hunter.Id) + 1;
        }

        public CampaignHunter Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < _hunters.Count; i++) if (_hunters[i].Id == id) return _hunters[i];
            return null;
        }

        /// The ladder a new campaign starts with, the same for the same seed: Size hunters, the higher ranks with
        /// more levels and traits, and you at the bottom.
        internal static HunterCompany Make(ulong seed)
        {
            var company = new HunterCompany();
            for (int rank = 1; rank <= Size; rank++) company.Recruit(seed, HunterRules.StartLevel(rank), HunterRules.StartTraits(rank));
            company._ladder.Add(You);
            return company;
        }

        /// For CampaignSave: the company as saved. False, with why, when the records do not fit together.
        internal static bool Restore(List<CampaignHunter> hunters, List<string> ladder, string nemesisId, int recruited,
            out HunterCompany company, out string why)
        {
            company = null;
            why = string.Empty;
            var c = new HunterCompany();
            int you = 0;
            for (int i = 0; i < ladder.Count; i++)
            {
                if (ladder[i] == You) you++;
                else if (FindIn(hunters, ladder[i]) == null) why = "The ladder names a hunter the company does not have.";
                if (ladder.IndexOf(ladder[i]) != i) why = "The ladder names someone twice.";
            }
            if (you != 1) why = "You are not on the ladder exactly once.";
            if (ladder.Count != hunters.Count + 1) why = "The ladder and the company do not match.";
            if (recruited < hunters.Count) why = "The company recruited fewer hunters than it has.";
            if (!string.IsNullOrEmpty(nemesisId) && FindIn(hunters, nemesisId) == null) why = "The nemesis is not in the company.";
            if (why.Length > 0) return false;
            c._hunters.AddRange(hunters);
            c._ladder.AddRange(ladder);
            c.NemesisId = nemesisId ?? string.Empty;
            c.Recruited = recruited;
            company = c;
            return true;
        }

        /// A new hunter at the bottom of the ladder, from their own dice: a class (the first Size take each class
        /// twice), traits that fit together, and a weakness none of them takes away.
        internal CampaignHunter Recruit(ulong seed, int level, int traits)
        {
            int index = Recruited++;
            HunterNames.For(seed, index, out string name, out string title);
            int[] classes = HunterNames.Shuffled(SliceClasses.Count, HunterNames.Dice(seed, "classes"));
            UnitClass unitClass = SliceClasses.All[classes[index % SliceClasses.Count]];
            DeterministicRng dice = HunterNames.Dice(seed, "hunter." + index.ToString(CultureInfo.InvariantCulture));
            var hunter = new CampaignHunter("hunter." + index.ToString(CultureInfo.InvariantCulture), name, title, unitClass, level,
                HunterWeakness.Focus);
            for (int tries = 0; tries < 64 && hunter.Traits.Count < Math.Min(traits, HunterTraits.MaxPerHunter); tries++)
            {
                var trait = (HunterTrait)dice.Next(HunterTraits.Rolled);
                if (hunter.CanLearn(trait)) hunter.Learn(trait);
            }
            var allowed = new List<HunterWeakness>();
            for (int w = 0; w < HunterWeaknesses.Count; w++) if (hunter.WeaknessAllowed((HunterWeakness)w)) allowed.Add((HunterWeakness)w);
            hunter.Weakness = allowed[dice.Next(allowed.Count)];
            _hunters.Add(hunter);
            _ladder.Add(hunter.Id);
            return hunter;
        }

        /// You beat this hunter: if they ranked above you, you take their place and everyone from them down to your
        /// old place moves down one.
        internal void YouBeat(CampaignHunter hunter)
        {
            int you = _ladder.IndexOf(You);
            int them = hunter == null ? -1 : _ladder.IndexOf(hunter.Id);
            if (them < 0 || them > you) return;
            _ladder.RemoveAt(you);
            _ladder.Insert(them, You);
        }

        /// This hunter beat you: they climb one place, past you if you stood just above.
        internal void TheyBeat(CampaignHunter hunter)
        {
            int at = hunter == null ? -1 : _ladder.IndexOf(hunter.Id);
            if (at <= 0) return;
            string above = _ladder[at - 1];
            _ladder[at - 1] = hunter.Id;
            _ladder[at] = above;
        }

        /// This hunter died: off the ladder and out of the company.
        internal void Slay(CampaignHunter hunter)
        {
            if (hunter == null) return;
            _ladder.Remove(hunter.Id);
            _hunters.Remove(hunter);
            if (NemesisId == hunter.Id) NemesisId = string.Empty;
        }

        private static CampaignHunter FindIn(List<CampaignHunter> hunters, string id)
        {
            for (int i = 0; i < hunters.Count; i++) if (hunters[i].Id == id) return hunters[i];
            return null;
        }
    }

    /// The hunters' numbers. Every one is a starting value: Jacob tunes them once he has played it.
    public static class HunterRules
    {
        /// Every level makes a hunter this percent stronger, compounding (Jacob, 18 Oct 2026: "exponentially grow").
        public const int PerLevelPercent = 8;

        /// No hunter goes past this level.
        public const int MaxLevel = 30;

        /// Levels a hunter gains the first time in a run that they beat you.
        public const int PromotionLevels = 2;

        /// Company blades in an ambush at bounty 2; one more for every bounty above.
        public const int AmbushFighters = 3;

        /// The hunt after the last chief brings one more blade for every bounty above this.
        public const int HuntBountyFrom = 3;

        /// What a level does: percent of a hunter's base power and health, compounding.
        public static int LevelPercent(int level)
        {
            int percent = 100;
            for (int i = 0; i < Math.Min(Math.Max(0, level), MaxLevel); i++) percent = percent * (100 + PerLevelPercent) / 100;
            return percent;
        }

        /// Materials sent to camp for beating the hunter at this rank.
        public static int Reward(int rank)
        {
            return rank <= 1 ? 50 : rank <= 3 ? 30 : rank <= 6 ? 20 : 15;
        }

        /// The best rank the company sends after you at this bounty: bounty 2 sends rank 7 and below, 3 rank 4 and
        /// below, 4 rank 2 and below, 5 anyone. Bounty 1 sends nobody but the hunt after the last chief.
        public static int TopRankSent(int bounty)
        {
            return bounty >= 5 ? 1 : bounty == 4 ? 2 : bounty == 3 ? 4 : bounty == 2 ? 7 : int.MaxValue;
        }

        /// Percent of a road's rows a hunter waits in at this bounty.
        public static int AmbushChance(int bounty)
        {
            return bounty >= 5 ? 100 : bounty == 4 ? 80 : bounty == 3 ? 60 : bounty == 2 ? 40 : 0;
        }

        internal static int StartLevel(int rank)
        {
            return rank <= 1 ? 3 : rank <= 3 ? 2 : rank <= 6 ? 1 : 0;
        }

        internal static int StartTraits(int rank)
        {
            return rank <= 1 ? 3 : rank <= 6 ? 2 : 1;
        }
    }

    /// The bounty on your head (Jacob's go, 18 Oct 2026), from 1 to Max: it rises as you beat chiefs and strip
    /// gear, and sends more hunters, and better ones. It is per run: every run starts at 1.
    public static class BountyRules
    {
        public const int Max = 5;

        /// Heat for each bounty level.
        public const int HeatPerLevel = 2;

        /// Heat for beating a chief, captured or killed.
        public const int HeatPerChief = 2;

        /// Heat for stripping a piece of gear instead of banking its materials.
        public const int HeatPerStrip = 1;

        /// From this bounty on, hunters wait on the road.
        public const int AmbushFrom = 2;

        public static int Level(int heat)
        {
            return Math.Max(1, Math.Min(Max, 1 + Math.Max(0, heat) / HeatPerLevel));
        }
    }

    /// The hunters' names and titles. Every word is Claude's placeholder: Jacob writes the story. Names come from the
    /// campaign seed through their own dice, so naming never moves a draw that decides a fight.
    internal static class HunterNames
    {
        // A stream no campaign, battle or other name draw uses.
        private const ulong Stream = 11;

        private static readonly string[] Names =
        {
            "Vessk", "Ixil", "Thraxa", "Sabek", "Ulvane", "Zerith", "Oxa", "Drevan", "Skarn", "Myrrex", "Quell", "Tavash",
            "Syrrin", "Korvex", "Nhal", "Pellix", "Vorra", "Xannic", "Ressa", "Grisk", "Lhoss", "Cyrex", "Ozzith", "Hesk"
        };

        private static readonly string[] Titles =
        {
            "the Patient", "Twice Paid", "Who Bills by the Hour", "the Unrefunded", "Who Reads the Fine Print", "the Repossessor",
            "Who Never Misses a Deadline", "Collector of Debts", "the Polite", "Who Hunted a Moon", "the Late Fee",
            "Who Takes Half Up Front", "the Freelancer", "Who Signed in Blood", "the Overtime", "Who Brings Receipts"
        };

        /// The index-th hunter's name and title in this campaign. Past the list, names come round again as "the Second".
        internal static void For(ulong seed, int index, out string name, out string title)
        {
            int[] names = Shuffled(Names.Length, Dice(seed, "names"));
            int[] titles = Shuffled(Titles.Length, Dice(seed, "titles"));
            name = CampaignNames.WithOrdinal(Names[names[index % Names.Length]], index / Names.Length + 1);
            title = Titles[titles[index % Titles.Length]];
        }

        /// Fresh dice for one key: FNV 1a over the key, mixed into the seed.
        internal static DeterministicRng Dice(ulong seed, string key)
        {
            string k = "hunters|" + key;
            ulong hash = 14695981039346656037UL;
            for (int i = 0; i < k.Length; i++)
            {
                hash ^= k[i];
                hash *= 1099511628211UL;
            }
            return new DeterministicRng(seed ^ hash, Stream);
        }

        // Fisher and Yates over 0 to count minus 1.
        internal static int[] Shuffled(int count, DeterministicRng dice)
        {
            var order = new int[count];
            for (int i = 0; i < count; i++) order[i] = i;
            for (int i = count - 1; i > 0; i--)
            {
                int j = dice.Next(i + 1);
                int keep = order[i];
                order[i] = order[j];
                order[j] = keep;
            }
            return order;
        }
    }
}
