using System;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// The Glutton's 35 talents, his seven limbs of five as locked on 28 Oct 2026 (DESIGN_LOCKS, "The Butcher, Glutton and Shade
    /// locked" and its later fixes): tier 1, tier 2, the tier 3 pair, then the keystone. What each one does is designed later;
    /// the store keeps them (DESIGN_LOCKS, "The floor under the three Pals"). Saves keep the numbers; never reorder.
    public enum GluttonTalent
    {
        None = 0,
        DarkMatterPlating = 1, EntropySink = 2, FoldShield = 3, NeutronMass = 4, BranchAnchor = 5,
        NucleusEdge = 6, Strip = 7, FoldStep = 8, EchoStrike = 9, AnswerTheParry = 10,
        SlowLightRounds = 11, DarkMatterRounds = 12, LightStake = 13, ProbabilityLock = 14, NothingWasted = 15,
        GravityMesh = 16, TimeLock = 17, CrawlField = 18, FoldTrap = 19, StasisWindUp = 20,
        MatterPrinter = 21, RewindWound = 22, SynthArmour = 23, EntropyPurge = 24, Recompile = 25,
        Precognition = 26, GlancingBlow = 27, DeepNode = 28, ChargedBranches = 29, CommandRelay = 30,
        RoguePlanetProspecting = 31, BranchHedge = 32, SynthesisDiscount = 33, GeneLibrary = 34, NodeCache = 35
    }

    // WhispererHabit and SquadPair live in Battle (PalKit.cs), where the fight reads them (rules 24).

    /// A talent the Glutton lent this run, and whether Feed doubled it.
    public sealed class HeldTalent
    {
        internal HeldTalent(GluttonTalent talent, bool fed)
        {
            Talent = talent;
            Fed = fed;
        }

        public GluttonTalent Talent { get; }
        public bool Fed { get; internal set; }
    }

    /// One of the Shade's routes this run, at its form.
    public sealed class HeldRoute
    {
        internal HeldRoute(ShadeRoute route, int form)
        {
            Route = route;
            Form = form;
        }

        public ShadeRoute Route { get; }
        public int Form { get; internal set; }
    }

    /// One class's habit. From rules 24 it is set at camp and kept, and the Whisperer's road card can upgrade it for the run.
    public sealed class HeldHabit
    {
        internal HeldHabit(WhispererHabit habit, bool upgraded = false)
        {
            Habit = habit;
            Upgraded = upgraded;
        }

        public WhispererHabit Habit { get; }
        public UnitClass Class => PalStoreRules.ClassOf(Habit);

        /// Upgraded by the Whisperer this run (rules 24).
        public bool Upgraded { get; internal set; }
    }

    /// The rules of the Pals' stores (campaign rules 23, DESIGN_LOCKS "The floor under the three Pals").
    public static class PalStoreRules
    {
        public const int TalentCount = 35;
        public const int HabitCount = WhisperRules.HabitCount;
        public const int PairCount = WhisperRules.PairCount;

        /// A fighter's ranks, and the rank that opens his second slot.
        public const int MaxRank = 3;
        public const int SecondSlotRank = 3;

        /// The class a habit belongs to.
        public static UnitClass ClassOf(WhispererHabit habit) => WhisperRules.ClassOf(habit);
    }
}
