using System;
using System.Collections.Generic;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// The four Pals at camp (DESIGN_LOCKS: the Butcher, the Glutton, the Shade, the Whisperer; names are placeholders).
    public enum Pal
    {
        None = 0,
        Butcher = 1,
        Glutton = 2,
        Shade = 3,
        Whisperer = 4
    }

    /// A Pal's favour for one run, like a Hades boon (Jacob, 18 Oct 2026: "favours from the Pals on road nodes"). Some
    /// road fights drop a Pal's favour instead of gear; win one and pick one of that Pal's favours, in place of the
    /// fight's materials, the way a piece of gear is taken. Each favour bends one of your buttons with a number the
    /// fight already has, and taking it again makes it stronger, up to its last rank. Favours go when the run does.
    /// The Butcher's bend Unleash, the Glutton's Hold, the Shade's Fall Back and the Whisperer's Focus Fire. Names,
    /// words and numbers are Claude's placeholders and starting values.
    public enum Favour
    {
        None = 0,

        // The Butcher: Unleash.
        LongRage = 1,
        ColdFury = 2,
        Dread = 3,

        // The Glutton: Hold.
        IronGut = 4,
        ThickHide = 5,
        Bulwark = 6,

        // The Shade: Fall Back.
        Slip = 7,
        LongShadow = 8,
        Wrongfoot = 9,

        // The Whisperer: Focus Fire.
        Mesmer = 10,
        LongGaze = 11,
        Heartbreak = 12,

        // The Butcher's 15 combos (29 Oct 2026; DESIGN_LOCKS, "The Pals on the road"), in Battle.ClassBend's order: from
        // campaign rules 15 his road fights offer three of these in place of his three favours above. Taking one puts it in
        // a fighter's gear slot as a piece (CampaignGear, Battle.ClassBend), not on a button, and a run takes each once.
        BlackHoleRound = 13,
        Collapse = 14,
        EventHorizon = 15,
        SingularityNet = 16,
        MercyWell = 17,
        AroundTheCorner = 18,
        ExecutionCorridor = 19,
        WrongDoor = 20,
        Doorway = 21,
        Evac = 22,
        StoppedBullets = 23,
        FrozenFlurry = 24,
        StillWall = 25,
        TimeSnare = 26,
        Rewind = 27,

        // The Whisperer's road card (31 Oct 2026, campaign rules 24; DESIGN_LOCKS, "Habits come with the game, the Whisperer
        // upgrades them"): from rules 24 her road fights offer three of these in place of her three favours above, each with
        // its fighter, pair or habit in FavourOffer.Picks. Taking one ranks that fighter up, sets that pair, or upgrades that
        // habit, for the run (Campaign.PalStores.cs).
        WhisperRankUp = 28,
        WhisperPair = 29,
        WhisperHabitUp = 30,

        // The Shade's road card (campaign rules 25; Jacob in chat, 31 Oct 2026: "go" on Claude's Shade design): one a route, in
        // Battle.ShadeRoute's order. From rules 25 her road fights offer three of these in place of her three favours above.
        // Taking one gives that route its next form, or puts it on its plain button at form 1 (Campaign.TryTakeRoute). Only
        // Sanctuary, Backstab and Fear are offered so far (Battle.ShadeRules.Built).
        ShadeMirror = 31,
        ShadeSanctuary = 32,
        ShadeBackstab = 33,
        ShadeRiftMine = 34,
        ShadeRupture = 35,
        ShadeFear = 36,
        ShadeBreach = 37,
        ShadeEcho = 38
    }

    /// One of the Whisperer's picks on offer (rules 24): its kind, and the fighter it ranks up, the pair it sets or the habit it
    /// upgrades.
    public sealed class WhisperPick
    {
        internal WhisperPick(Favour kind, string fighterId, WhispererHabit habit, SquadPair pair)
        {
            Kind = kind;
            FighterId = fighterId ?? string.Empty;
            Habit = habit;
            Pair = pair;
        }

        public Favour Kind { get; }

        /// WhisperRankUp: the fighter, by id. Empty otherwise.
        public string FighterId { get; }

        /// WhisperHabitUp: the habit. None otherwise.
        public WhispererHabit Habit { get; }

        /// WhisperPair: the pair. None otherwise.
        public SquadPair Pair { get; }
    }

    /// A favour the run holds, and how many times it was taken.
    public sealed class HeldFavour
    {
        internal HeldFavour(Favour favour, int rank)
        {
            Favour = favour;
            Rank = rank;
        }

        public Favour Favour { get; }

        /// 1 to FavourRules.MaxRank.
        public int Rank { get; internal set; }
    }

    /// The favours a won favour fight offers, until the next fight starts (Campaign.TryTakeFavour).
    public sealed class FavourOffer
    {
        private readonly List<WhisperPick> _picks = new List<WhisperPick>();

        internal FavourOffer(Pal pal, IReadOnlyList<Favour> choices, IReadOnlyList<WhisperPick> picks = null)
        {
            Pal = pal;
            Choices = choices;
            for (int i = 0; picks != null && i < picks.Count; i++) _picks.Add(picks[i]);
        }

        /// From rules 24, the Whisperer's: each choice's fighter, pair or habit, in the same order as Choices. Empty for any
        /// other Pal's offer.
        public IReadOnlyList<WhisperPick> Picks => _picks;

        internal void AddPick(WhisperPick pick)
        {
            _picks.Add(pick);
        }

        /// Whose favours they are.
        public Pal Pal { get; }

        /// One to three favours, each one rank short of its last.
        public IReadOnlyList<Favour> Choices { get; }
    }

    public static class FavourRules
    {
        /// Taking a favour a third time is its last rank.
        public const int MaxRank = 3;

        /// How many favours a won favour fight offers.
        public const int Offered = 3;

        // Per rank. Starting values.
        public const int LongerPercent = 20;
        public const int DrainCutQuarters = 1;
        public const int ScarePercent = 25;
        public const int HoldAvoid = 5;
        public const int SoakCut = 10;
        public const int DodgePoints = 1;
        public const int WrongfootTicks = SimClock.TicksPerSecond / 2;
        public const int FocusHit = 5;
        public const int FocusFallNervePercent = 25;

        public static readonly Pal[] Pals = { Pal.Butcher, Pal.Glutton, Pal.Shade, Pal.Whisperer };

        /// How much a Pal weighs when a road from campaign rules 18 picks its Pal fights (DESIGN_LOCKS, "The Butcher's theme"),
        /// given the Pals the run held picks from (Campaign.HeldPals): 1 each, three times that for a Pal the run holds a
        /// pick from, and a third for a Pal it holds nothing from once it holds picks from two. Counted in thirds so they stay
        /// whole: 9 held, 3 not, 1 not once two are held. Starting values.
        public static int PalWeight(Pal pal, int heldPals)
        {
            if ((heldPals & (1 << (int)pal)) != 0) return HeldPalWeight;
            int held = 0;
            for (int i = 0; i < Pals.Length; i++) if ((heldPals & (1 << (int)Pals[i])) != 0) held++;
            return held >= 2 ? OutsidePalWeight : PlainPalWeight;
        }

        public const int HeldPalWeight = 9;
        public const int PlainPalWeight = 3;
        public const int OutsidePalWeight = 1;

        // One Pal by PalWeight, from these dice, but none of the Pals in skip (one bit a Pal, 1 << (int)pal).
        internal static Pal WeightedPal(DeterministicRng dice, int heldPals, int skip = 0)
        {
            int total = 0;
            for (int i = 0; i < Pals.Length; i++) total += SkippedOr(Pals[i], heldPals, skip);
            int roll = dice.Next(total);
            for (int i = 0; i < Pals.Length; i++)
            {
                int weight = SkippedOr(Pals[i], heldPals, skip);
                if (roll < weight) return Pals[i];
                roll -= weight;
            }
            return Pals[Pals.Length - 1];
        }

        private static int SkippedOr(Pal pal, int heldPals, int skip)
        {
            return (skip & (1 << (int)pal)) != 0 ? 0 : PalWeight(pal, heldPals);
        }

        private static readonly Favour[] Butcher = { Favour.LongRage, Favour.ColdFury, Favour.Dread };
        private static readonly Favour[] Glutton = { Favour.IronGut, Favour.ThickHide, Favour.Bulwark };
        private static readonly Favour[] Shade = { Favour.Slip, Favour.LongShadow, Favour.Wrongfoot };
        private static readonly Favour[] Whisperer = { Favour.Mesmer, Favour.LongGaze, Favour.Heartbreak };
        private static readonly Favour[] NoFavours = new Favour[0];

        // The Butcher's 15 combos, in order.
        private static readonly Favour[] ButcherCombos =
        {
            Favour.BlackHoleRound, Favour.Collapse, Favour.EventHorizon, Favour.SingularityNet, Favour.MercyWell,
            Favour.AroundTheCorner, Favour.ExecutionCorridor, Favour.WrongDoor, Favour.Doorway, Favour.Evac,
            Favour.StoppedBullets, Favour.FrozenFlurry, Favour.StillWall, Favour.TimeSnare, Favour.Rewind
        };

        /// Every button favour, in order. The combos come after them (Combos).
        public const int Count = 12;

        /// A Pal's combos, in order: the Butcher's, offered from campaign rules 15 (Campaign.ButcherComboRules). Nobody else
        /// has any.
        public static IReadOnlyList<Favour> Combos(Pal pal)
        {
            return pal == Pal.Butcher ? ButcherCombos : NoFavours;
        }

        /// True for one of the Whisperer's road card picks (rules 24): its details are in FavourOffer.Picks.
        public static bool IsWhisper(Favour favour)
        {
            return favour >= Favour.WhisperRankUp && favour <= Favour.WhisperHabitUp;
        }

        /// True for one of the Shade's road card picks (rules 25): a route, its form in the run's store.
        public static bool IsShade(Favour favour)
        {
            return favour >= Favour.ShadeMirror && favour <= Favour.ShadeEcho;
        }

        /// The route a Shade pick takes; None for any other favour.
        public static ShadeRoute RouteOf(Favour favour)
        {
            return IsShade(favour) ? (ShadeRoute)((int)favour - (int)Favour.ShadeMirror + (int)ShadeRoute.Mirror) : ShadeRoute.None;
        }

        /// The Shade pick for a route.
        public static Favour ShadePick(ShadeRoute route)
        {
            if (route <= ShadeRoute.None || (int)route >= ShadeRules.RouteCount) return Favour.None;
            return (Favour)((int)route - (int)ShadeRoute.Mirror + (int)Favour.ShadeMirror);
        }

        /// A pair's name.
        public static string PairName(SquadPair pair)
        {
            switch (pair)
            {
                case SquadPair.PinAndShoot: return "Pin and Shoot";
                case SquadPair.OpenAndCut: return "Open and Cut";
                case SquadPair.SnareAndRip: return "Snare and Rip";
                case SquadPair.MendAndHold: return "Mend and Hold";
                default: return string.Empty;
            }
        }

        /// A habit's name.
        public static string HabitName(WhispererHabit habit)
        {
            switch (habit)
            {
                case WhispererHabit.CounterShot: return "Counter Shot";
                case WhispererHabit.FrontFirst: return "Front First";
                case WhispererHabit.None: return string.Empty;
                default: return habit.ToString();
            }
        }

        /// True for a combo: it goes in a fighter's gear slot, not on a button, and has one rank.
        public static bool IsCombo(Favour favour)
        {
            return favour >= Favour.BlackHoleRound && favour <= Favour.Rewind;
        }

        /// What a combo does in a fight: its bend on the fighter who wears it. None for a button favour.
        public static ClassBend ComboBend(Favour favour)
        {
            return IsCombo(favour) ? (ClassBend)((int)favour - (int)Favour.BlackHoleRound + (int)ClassBend.BlackHoleRound) : ClassBend.None;
        }

        /// A Pal's favours, in order.
        public static IReadOnlyList<Favour> Of(Pal pal)
        {
            switch (pal)
            {
                case Pal.Butcher: return Butcher;
                case Pal.Glutton: return Glutton;
                case Pal.Shade: return Shade;
                case Pal.Whisperer: return Whisperer;
                default: return NoFavours;
            }
        }

        public static Pal PalOf(Favour favour)
        {
            if (IsCombo(favour)) return Pal.Butcher;
            if (IsWhisper(favour)) return Pal.Whisperer;
            if (IsShade(favour)) return Pal.Shade;
            if (favour <= Favour.None || (int)favour > Count) return Pal.None;
            return (Pal)(((int)favour - 1) / 3 + 1);
        }

        /// The button a Pal's favours bend.
        public static Directive Button(Pal pal)
        {
            switch (pal)
            {
                case Pal.Butcher: return Directive.Unleash;
                case Pal.Glutton: return Directive.Hold;
                case Pal.Shade: return Directive.FallBack;
                default: return Directive.FocusFire;
            }
        }

        public static string PalName(Pal pal)
        {
            switch (pal)
            {
                case Pal.Butcher: return "the Butcher";
                case Pal.Glutton: return "the Glutton";
                case Pal.Shade: return "the Shade";
                case Pal.Whisperer: return "the Whisperer";
                default: return string.Empty;
            }
        }

        public static string Name(Favour favour)
        {
            if (IsCombo(favour)) return ClassSpells.Name(ComboBend(favour));
            if (favour == Favour.WhisperRankUp) return "Rank up";
            if (favour == Favour.WhisperPair) return "Pair";
            if (favour == Favour.WhisperHabitUp) return "Habit upgrade";
            if (IsShade(favour)) return ShadeRules.Name(RouteOf(favour));
            switch (favour)
            {
                case Favour.LongRage: return "Long Rage";
                case Favour.ColdFury: return "Cold Fury";
                case Favour.Dread: return "Dread";
                case Favour.IronGut: return "Iron Gut";
                case Favour.ThickHide: return "Thick Hide";
                case Favour.Bulwark: return "Bulwark";
                case Favour.Slip: return "Slip";
                case Favour.LongShadow: return "Long Shadow";
                case Favour.Wrongfoot: return "Wrongfoot";
                case Favour.Mesmer: return "Mesmer";
                case Favour.LongGaze: return "Long Gaze";
                case Favour.Heartbreak: return "Heartbreak";
                default: return string.Empty;
            }
        }

        /// What it does at this rank, in one line: "Unleash lasts 40% longer."
        public static string Describe(Favour favour, int rank)
        {
            // A combo has one rank: "For a Marksman's gear slot. His Aimed Shot drags ..."
            if (IsCombo(favour))
            {
                ClassBend bend = ComboBend(favour);
                return "For a " + SliceClasses.Name(ClassSpells.ClassOf(bend)) + "'s gear slot. " + ClassSpells.Describe(bend);
            }
            rank = Math.Max(1, Math.Min(MaxRank, rank));
            // A Shade pick's rank is its route's form (Campaign.FavourRank): what that form adds.
            if (IsShade(favour)) return ShadeRules.FormName(RouteOf(favour), rank) + ": " + ShadeRules.Describe(RouteOf(favour), rank);
            switch (favour)
            {
                case Favour.LongRage: return "Unleash lasts " + LongerPercent * rank + "% longer.";
                case Favour.ColdFury: return "Unleash drains " + Quarters(rank) + " less of your own nerve.";
                case Favour.Dread: return "Unleash breaks " + ScarePercent * rank + "% more of their nerve.";
                case Favour.IronGut: return "Hold drains " + Quarters(rank) + " less of your own nerve.";
                case Favour.ThickHide: return "Holding gives your front row " + HoldAvoid * rank + " more Avoid.";
                case Favour.Bulwark: return "Holding, your front row takes " + SoakCut * rank + "% less from blows.";
                case Favour.Slip: return "Each cast or heavy swing you dodge with Fall Back earns " + DodgePoints * rank + " more point" +
                    (DodgePoints * rank == 1 ? "." : "s.");
                case Favour.LongShadow: return "Fall Back lasts " + LongerPercent * rank + "% longer.";
                case Favour.Wrongfoot: return "Whoever throws a cast or heavy swing you dodge with Fall Back staggers for " +
                    Seconds(WrongfootTicks * rank) + ".";
                case Favour.Mesmer: return "Focus Fire gives " + FocusHit * rank + " more Hit.";
                case Favour.LongGaze: return "Focus Fire lasts " + LongerPercent * rank + "% longer.";
                case Favour.Heartbreak: return "When the one you focus falls, their side loses " + FocusFallNervePercent * rank +
                    "% more nerve.";
                default: return string.Empty;
            }
        }

        /// The fight's numbers for the favours a run holds.
        public static SideFavours ToSide(IReadOnlyList<HeldFavour> held)
        {
            if (held == null || held.Count == 0) return SideFavours.None;
            int R(Favour f)
            {
                for (int i = 0; i < held.Count; i++) if (held[i].Favour == f) return held[i].Rank;
                return 0;
            }
            return new SideFavours(
                unleashLongerPercent: LongerPercent * R(Favour.LongRage),
                unleashDrainCut: DrainCutQuarters * R(Favour.ColdFury),
                unleashScarePercent: ScarePercent * R(Favour.Dread),
                holdDrainCut: DrainCutQuarters * R(Favour.IronGut),
                holdAvoid: HoldAvoid * R(Favour.ThickHide),
                holdSoakCut: SoakCut * R(Favour.Bulwark),
                dodgePoints: DodgePoints * R(Favour.Slip),
                fallBackLongerPercent: LongerPercent * R(Favour.LongShadow),
                wrongfootTicks: WrongfootTicks * R(Favour.Wrongfoot),
                focusHit: FocusHit * R(Favour.Mesmer),
                focusLongerPercent: LongerPercent * R(Favour.LongGaze),
                focusFallNervePercent: FocusFallNervePercent * R(Favour.Heartbreak));
        }

        private static string Quarters(int rank)
        {
            switch (rank)
            {
                case 1: return "a quarter";
                case 2: return "half";
                default: return "three quarters";
            }
        }

        private static string Seconds(int ticks)
        {
            int tenths = ticks * 10 / SimClock.TicksPerSecond;
            return (tenths / 10) + (tenths % 10 == 0 ? string.Empty : "." + (tenths % 10)) + (tenths == 10 ? " second" : " seconds");
        }
    }
}
