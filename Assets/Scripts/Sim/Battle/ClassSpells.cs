namespace KindredSiege.Sim.Battle
{
    /// What a class piece does to its own class's spell (Jacob, 20 Oct 2026: "Gear becomes class pieces that bend that
    /// class's own spell"; DESIGN_LOCKS, "The squad's spells, marks and class pieces"). One per piece, and a piece only
    /// ever bends its own class's spell: which mark it leaves, or how hard it cashes one in. Two for each class so far;
    /// the names are Claude's placeholders.
    public enum ClassBend
    {
        None = 0,

        /// Warden: his Guard takes ClassSpells.BastionGuardPercent off a guarded blow instead of the usual 30.
        Bastion = 1,

        /// Warden: an enemy whose blow lands on his Guard is left with an Opening.
        Retaliator = 2,

        /// Marksman: aims half again as long, and the shot hits half again as hard.
        Deadeye = 3,

        /// Marksman: aims half as long and no blow breaks the aim, but the shot hits a quarter lighter.
        Quickdraw = 4,

        /// Skirmisher: her strike hits an Opening three times as hard instead of twice.
        Opportunist = 5,

        /// Skirmisher: below half health her strike comes twice as often.
        Bloodrush = 6,

        /// Trapper: the snare's wound bleeds twice as hard.
        Barbs = 7,

        /// Trapper: the snare holds twice as long.
        LongSnare = 8,

        /// Medic: each treatment heals half again as much, and she treats half again as slowly.
        Surgeon = 9,

        /// Medic: each treatment also treats the next most hurt ally for half.
        FieldKit = 10,

        // The Butcher's 15 combos (Jacob, 29 Oct 2026: "Go, Black Hole Round", then "Yes, go" to the other 14; DESIGN_LOCKS,
        // "The four Pals' verbs and costs" and "The four Pals audited and fixed"): a fighter's own move made bigger by one of
        // his three augments, in the locks' order (Marksman, Skirmisher, Warden, Trapper, Medic for each augment), each with
        // its augment's pact (Augment). A combo is never a dropped piece: it comes from the Butcher's road fights
        // (Learning.FavourRules.Combos), from campaign rules 15. What each does: ClassSpells.Describe and
        // BattleSimulation.Combos.cs.

        // Gravity Well (purple): pulls. Pact, Heavy.
        BlackHoleRound = 11,
        Collapse = 12,
        EventHorizon = 13,
        SingularityNet = 14,
        MercyWell = 15,

        // Fold Door (blue): portals. Pact, Unnerving.
        AroundTheCorner = 16,
        ExecutionCorridor = 17,
        WrongDoor = 18,
        Doorway = 19,
        Evac = 20,

        // Time Stop (gold): freezes, slows and rewinds. Pact, Borrowed time.
        StoppedBullets = 21,
        FrozenFlurry = 22,
        StillWall = 23,
        TimeSnare = 24,
        Rewind = 25
    }

    /// The Butcher's three augments (DESIGN_LOCKS, "The four Pals' verbs and costs", 28 Oct 2026), each with its pact, a cost
    /// written in words, halved on 29 Oct (fix 1): Heavy, Unnerving and Borrowed time.
    public enum Augment
    {
        None = 0,
        GravityWell = 1,
        FoldDoor = 2,
        TimeStop = 3
    }

    /// How big a hit is, for the screen (the fight-feel skill, 19 Oct 2026): a plain blow, a spell's hit, or a spell
    /// cashing in a mark.
    public enum HitTier
    {
        Light = 0,
        Heavy = 1,
        Finisher = 2
    }

    /// The class spells (Jacob, 20 Oct 2026: "classes have auto attacks then have specific spells only they can use tank
    /// cant heal and heal cant dps"; FightRules.ClassSpells; BattleSimulation.Class.cs). Every class keeps its auto attack
    /// and has one spell only it has. The spells leave marks, and another class's spell hits harder on a mark:
    /// - Warden, Guard: he covers one ally, and blows on that ally land on him lighter, as before. With no Cover order
    ///   he covers his side's Medic (Covered Treatment), so she carries the Guard mark.
    /// - Marksman, Aimed Shot: every AimEveryTicks he stops and aims at one enemy for AimTicks, then fires one big shot that
    ///   leaves an Opening. A blow on him while he aims loses the shot. At a snared enemy he aims twice as fast and always
    ///   crits (the Snare cashed in).
    /// - Skirmisher, Strike: every StrikeEveryTicks a melee strike that reaches the back row. It hits an Opening twice as
    ///   hard and rips a bleeding wound open, so all the bleeding left lands at once (Opening and Bleed cashed in).
    /// - Trapper, Barbed Snare: the trap as before, but whoever springs it is held for SnareTicks (does nothing and cannot
    ///   dodge), bleeds, and its blow never lands. Snare and Bleed are its marks.
    /// - Medic, Treatment: she never attacks. She treats the most hurt ally, half again as much while a Warden covers her
    ///   (the Guard cashed in: the 19 Oct pitch's Covered Treatment).
    /// Starting values: Jacob's rule is build first, tune after he plays.
    public static class ClassSpells
    {
        // ------------------------------------------------------------ marks

        /// How long a snare holds, and the share of health a second its wound bleeds (for BleedRules.Ticks).
        public const int SnareTicks = 2 * SimClock.TicksPerSecond;
        public const int SnareBleedPercent = 2;

        /// How long an Opening stays, and how hard the Skirmisher's strike hits one, in percent.
        public const int OpeningTicks = 3 * SimClock.TicksPerSecond;
        public const int OpeningPercent = 200;

        /// A treatment by a Medic a Warden covers heals this percent.
        public const int GuardedTreatPercent = 150;

        // ------------------------------------------------------------ spells

        /// The Marksman aims this often, for this long, and the shot hits this percent of his power; at a snared enemy he
        /// aims PinAimPercent as long and the shot crits for PinCritPercent.
        public const int AimEveryTicks = 5 * SimClock.TicksPerSecond;
        public const int AimTicks = 45;

        /// After a blow breaks his aim he can aim again this soon (Jacob, 20 Oct 2026: "2 seconds after a break").
        public const int AimRetryTicks = 2 * SimClock.TicksPerSecond;
        public const int AimedShotPercent = 250;
        public const int PinAimPercent = 50;
        public const int PinCritPercent = 200;

        /// The Skirmisher strikes this often, for this percent of her power.
        public const int StrikeEveryTicks = 4 * SimClock.TicksPerSecond;
        public const int StrikePercent = 200;

        // ------------------------------------------------------------ the pieces

        public const int BastionGuardPercent = 50;
        public const int DeadeyePercent = 150;
        public const int QuickdrawAimPercent = 50;
        public const int QuickdrawShotPercent = 75;
        public const int OpportunistPercent = 300;
        public const int BloodrushBelowPermille = 500;
        public const int BloodrushEveryPercent = 50;
        public const int BarbsPercent = 200;
        public const int LongSnarePercent = 200;
        public const int SurgeonPercent = 150;
        public const int FieldKitPercent = 50;

        // ------------------------------------------------------------ the Butcher's combos (29 Oct 2026)

        /// Each enemy a Black Hole Round drags in takes this percent of the Aimed Shot's own weight, with the shot's crit when
        /// it cashed a Snare (the combos' hooks, 29 Oct 2026).
        public const int BlackHoleCrushPercent = 50;

        /// The pacts (DESIGN_LOCKS, 29 Oct 2026, fix 1: halved). Heavy, the Gravity Well's: the spell's own clock runs this
        /// percent (for the Warden and the Medic, whose spells come every turn, the turn; for the Trapper, her rearm, rounded
        /// up). Unnerving, the Fold Door's: the wearer starts each fight with an eighth less nerve, or, where fighters have no
        /// nerve of their own, the side's bar loses an eighth of the wearer's share. Borrowed time, the Time Stop's: an eighth
        /// less health.
        public const int HeavyPactPercent = 125;
        public const int UnnervingEighths = 8;
        public const int BorrowedTimePermille = 875;

        /// Collapse: every CollapseEvery landed strike, and every strike that rips a bleeding wound open (the hooks), drops a
        /// black hole that pins her mark for CollapseTicks.
        public const int CollapseEvery = 2;
        public const int CollapseTicks = 2 * SimClock.TicksPerSecond;

        /// Event Horizon: every EventHorizonEvery swing his bash pulls in the enemies beside his mark, and the cleave hits
        /// them all for this percent of his power (the mark through his blow, the rest crushed), leaving each one it hits
        /// bleeding as a snare's wound bleeds (the hooks).
        public const int EventHorizonEvery = 4;
        public const int EventHorizonPercent = 200;

        /// Mercy Well: each ally beside her patient is healed this percent of the treatment, and what they do not need flows
        /// back into the patient (rethought 29 Oct 2026).
        public const int MercyWellPercent = 50;

        /// Mercy Well (the hooks): each treatment heals this percent more for every bleeding enemy, up to MercyBleedingMost of
        /// them. It reads the wounds without closing them.
        public const int MercyBleedPercent = 20;
        public const int MercyBleedingMost = 3;

        /// Execution Corridor: the hit from behind is this percent of her strike's own weight.
        public const int CorridorPercent = 100;

        /// Around the Corner (rethought 29 Oct 2026): his Aimed Shot comes through a portal from behind, so no Guard takes it;
        /// past your Focus Fire and snared enemies it goes for their healer, and a shot from behind at a healer hits this
        /// percent.
        public const int CornerCritPercent = 200;

        /// Around the Corner (the hooks): a snared enemy shot from behind takes this percent, not PinCritPercent.
        public const int CornerSnarePercent = 300;

        /// Evac: the fighter her serpent pulls through comes back with this share of health, out of reach this long, and
        /// whoever's blow would have killed him is left with an Opening (the hooks).
        public const int EvacHealthPermille = 400;
        public const int EvacTicks = 3 * SimClock.TicksPerSecond;

        /// Stopped Bullets: this many shots, the last firing them all at once, and it cannot miss or be guarded. Each bullet
        /// that hangs frozen in its mark freezes the mark for StoppedFreezeTicks and leaves its Opening (rethought 29 Oct 2026).
        public const int StoppedShots = 3;
        public const int StoppedFreezeTicks = 2 * SimClock.TicksPerSecond;

        /// Frozen Flurry: how long her strike freezes its mark, every hit on it waiting until it thaws.
        public const int FlurryTicks = 3 * SimClock.TicksPerSecond / 2;

        /// Frozen Flurry (the hooks): an Opening on her frozen mark when it thaws is cashed by the thaw, and the whole kept hit
        /// lands this percent.
        public const int FlurryOpenPercent = 200;

        /// Still Wall: whoever hits his shield freezes this long and is left with an Opening (the hooks), at most once every
        /// StillWallEveryTicks.
        public const int StillWallTicks = 2 * SimClock.TicksPerSecond;
        public const int StillWallEveryTicks = 6 * SimClock.TicksPerSecond;

        /// Time Snare (rethought 29 Oct 2026): for this long after her trap's hold, the one it caught moves at half speed (its
        /// turns come every other tick), and every big move it starts in that time winds up this percent as long. The snare's
        /// wound bleeds as long as the slow (the hooks).
        public const int TimeSnareTicks = 6 * SimClock.TicksPerSecond;
        public const int TimeSnarePercent = 200;

        /// Rewind: every RewindEveryTicks she gives the most hurt ally back what they lost in the last RewindSeconds, and whoever
        /// last struck that ally is left with an Opening (the hooks).
        public const int RewindEveryTicks = 15 * SimClock.TicksPerSecond;
        public const int RewindSeconds = 3;

        // ------------------------------------------------------------ what the events say

        /// Aim event details: aiming, or aiming at a snared enemy.
        public const string AimDetail = "aim";
        public const string PinDetail = "pin";

        /// Hit details: the Aimed Shot, the Aimed Shot crit on a snared enemy, the strike, the strike on an Opening, and
        /// a bleeding wound ripped open. Heal detail: a treatment.
        public const string AimedDetail = "aimed";
        public const string AimedPinDetail = "aimed.pin";
        public const string StrikeDetail = "strike";
        public const string StrikeOpenDetail = "strike.open";
        public const string RipDetail = "rip";
        public const string TreatDetail = "treat";

        /// Hit detail: an enemy a Black Hole Round dragged in and crushed (after its Pull event, whose detail is the combo's
        /// name).
        public const string BlackHoleDetail = "blackhole";

        /// Hit details: an enemy Event Horizon's bash pulled in, then cleaved; Execution Corridor's hit from behind; a Frozen
        /// Flurry's waiting hits landing at once. Heal details: Mercy Well's on the allies beside her patient, and Rewind's.
        public const string EventHorizonDetail = "eventhorizon";
        public const string CorridorDetail = "corridor";
        public const string FlurryDetail = "flurry";
        public const string MercyDetail = "mercy";
        public const string RewindDetail = "rewind";

        /// Mark event details.
        public const string SnareMark = "snare";
        public const string BleedMark = "bleed";
        public const string OpeningMark = "opening";

        /// Cash event details: the Snare (PinDetail), an Opening (OpeningMark), a Bleed (RipDetail) and a Guard. Mercy Well's
        /// treatment fed by bleeding enemies cashes as MercyDetail (the hooks).
        public const string CoveredDetail = "covered";

        /// Every bend, None first, in enum order, the combos too: a save and a replay keep them.
        public const int BendCount = 26;

        // The combos' classes, in the order each augment's five come.
        private static readonly UnitClass[] ComboClasses =
            { UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Trapper, UnitClass.Medic };

        // The pieces a fight drops, by class. The combos are not among them.
        private static readonly ClassBend[][] ByClass =
        {
            new[] { ClassBend.Bastion, ClassBend.Retaliator },
            new[] { ClassBend.Deadeye, ClassBend.Quickdraw },
            new[] { ClassBend.Opportunist, ClassBend.Bloodrush },
            new[] { ClassBend.Barbs, ClassBend.LongSnare },
            new[] { ClassBend.Surgeon, ClassBend.FieldKit }
        };

        /// How many bends each class has.
        public const int BendsPerClass = 2;

        /// A class's bends, in order: index 0 to BendsPerClass - 1.
        public static ClassBend BendOf(UnitClass unitClass, int index)
        {
            ClassBend[] bends = ByClass[(int)unitClass];
            return bends[((index % bends.Length) + bends.Length) % bends.Length];
        }

        /// The class a bend belongs to. None belongs to nobody: Warden is returned, so ask IsFor instead.
        public static UnitClass ClassOf(ClassBend bend)
        {
            if (IsCombo(bend)) return ComboClasses[((int)bend - (int)ClassBend.BlackHoleRound) % ComboClasses.Length];
            switch (bend)
            {
                case ClassBend.Deadeye:
                case ClassBend.Quickdraw: return UnitClass.Marksman;
                case ClassBend.Opportunist:
                case ClassBend.Bloodrush: return UnitClass.Skirmisher;
                case ClassBend.Barbs:
                case ClassBend.LongSnare: return UnitClass.Trapper;
                case ClassBend.Surgeon:
                case ClassBend.FieldKit: return UnitClass.Medic;
                default: return UnitClass.Warden;
            }
        }

        /// True when this bend is one of this class's.
        public static bool IsFor(ClassBend bend, UnitClass unitClass)
        {
            return bend != ClassBend.None && ClassOf(bend) == unitClass;
        }

        /// True for a Butcher combo (29 Oct 2026): it comes from his road fights, never as a dropped piece.
        public static bool IsCombo(ClassBend bend)
        {
            return bend >= ClassBend.BlackHoleRound && bend <= ClassBend.Rewind;
        }

        /// The augment a combo is made with, so its pact. None for a class piece.
        public static Augment AugmentOf(ClassBend bend)
        {
            return IsCombo(bend) ? (Augment)(((int)bend - (int)ClassBend.BlackHoleRound) / ComboClasses.Length + 1) : Augment.None;
        }

        public static string Name(ClassBend bend)
        {
            switch (bend)
            {
                case ClassBend.Bastion: return "Bastion";
                case ClassBend.Retaliator: return "Retaliator";
                case ClassBend.Deadeye: return "Deadeye";
                case ClassBend.Quickdraw: return "Quickdraw";
                case ClassBend.Opportunist: return "Opportunist";
                case ClassBend.Bloodrush: return "Bloodrush";
                case ClassBend.Barbs: return "Barbs";
                case ClassBend.LongSnare: return "Long Snare";
                case ClassBend.Surgeon: return "Surgeon";
                case ClassBend.FieldKit: return "Field Kit";
                case ClassBend.BlackHoleRound: return "Black Hole Round";
                case ClassBend.Collapse: return "Collapse";
                case ClassBend.EventHorizon: return "Event Horizon";
                case ClassBend.SingularityNet: return "Singularity Net";
                case ClassBend.MercyWell: return "Mercy Well";
                case ClassBend.AroundTheCorner: return "Around the Corner";
                case ClassBend.ExecutionCorridor: return "Execution Corridor";
                case ClassBend.WrongDoor: return "Wrong Door";
                case ClassBend.Doorway: return "Doorway";
                case ClassBend.Evac: return "Evac";
                case ClassBend.StoppedBullets: return "Stopped Bullets";
                case ClassBend.FrozenFlurry: return "Frozen Flurry";
                case ClassBend.StillWall: return "Still Wall";
                case ClassBend.TimeSnare: return "Time Snare";
                case ClassBend.Rewind: return "Rewind";
                default: return string.Empty;
            }
        }

        /// One line for a card, as x + y = z (Jacob, 31 Oct 2026: "all he needs to know x +y = z or something simple"; HANDOFF_LOG
        /// 12817). Every number comes from the constants above, so a line stays true when one changes. A combo ends on its pact.
        public static string Describe(ClassBend bend)
        {
            switch (bend)
            {
                case ClassBend.Bastion: return "Guarded blow = -" + BastionGuardPercent + "% damage";
                case ClassBend.Retaliator: return "Blow on his Guard = attacker open";
                case ClassBend.Deadeye: return "Longer aim = Aimed Shot " + X(DeadeyePercent);
                case ClassBend.Quickdraw: return "Aim time " + X(QuickdrawAimPercent) + ", can't break = shot " + X(QuickdrawShotPercent);
                case ClassBend.Opportunist: return "Strike + Opening = " + X(OpportunistPercent);
                case ClassBend.Bloodrush: return "Under " + Pm(BloodrushBelowPermille) + " health = strikes x" + Decimal(100, BloodrushEveryPercent) + " as often";
                case ClassBend.Barbs: return "Snare = bleed " + X(BarbsPercent);
                case ClassBend.LongSnare: return "Snare = hold " + X(LongSnarePercent);
                case ClassBend.Surgeon: return "Treatment = heal " + X(SurgeonPercent) + ", slower";
                case ClassBend.FieldKit: return "Treatment = next most hurt healed " + FieldKitPercent + "%";
                case ClassBend.BlackHoleRound: return "Aimed Shot = pull neighbours in, crush " + BlackHoleCrushPercent + "% (crit if snared)" + Heavy;
                case ClassBend.Collapse: return "Every " + Ord(CollapseEvery) + " strike or a rip = pin " + Sec(CollapseTicks) + Heavy;
                case ClassBend.EventHorizon: return "Every " + Ord(EventHorizonEvery) + " swing = pull neighbours + cleave " + X(EventHorizonPercent) + " + bleed" + Heavy;
                case ClassBend.SingularityNet: return "Trap sprung = neighbours pulled in + all snared" + Heavy;
                case ClassBend.MercyWell: return "Treatment = neighbours healed " + MercyWellPercent + "%, +" + MercyBleedPercent + "% a bleeding enemy (max " + MercyBleedingMost + ")" + Heavy;
                case ClassBend.AroundTheCorner: return "Aimed Shot from behind = no Guard, snared " + X(CornerSnarePercent) + ", healer " + X(CornerCritPercent) + Unnerving;
                case ClassBend.ExecutionCorridor: return "Strike + Opening = second hit from behind" + Unnerving;
                case ClassBend.WrongDoor: return "Blow on his neighbours = onto his shield + attacker bleeds" + Unnerving;
                case ClassBend.Doorway: return "Her trap + shooters = they fall in too" + Unnerving;
                case ClassBend.Evac: return "Ally falls = saved at " + Pm(EvacHealthPermille) + ", out " + Sec(EvacTicks) + ", killer open, once a fight" + Unnerving;
                case ClassBend.StoppedBullets: return "Each shot = freeze " + Sec(StoppedFreezeTicks) + ", " + Ord(StoppedShots) + " = all " + StoppedShots + " at once" + Borrowed;
                case ClassBend.FrozenFlurry: return "Strike = freeze " + Sec(FlurryTicks) + ", hits land on thaw, open " + X(FlurryOpenPercent) + Borrowed;
                case ClassBend.StillWall: return "Hit on his shield = freeze " + Sec(StillWallTicks) + " + open, every " + Sec(StillWallEveryTicks) + Borrowed;
                case ClassBend.TimeSnare: return "Trap sprung = half speed " + Sec(TimeSnareTicks) + ", wind ups " + X(TimeSnarePercent) + ", bleeds" + Borrowed;
                case ClassBend.Rewind: return "Every " + Sec(RewindEveryTicks) + " = most hurt ally back " + RewindSeconds + "s, striker open" + Borrowed;
                default: return string.Empty;
            }
        }

        // The three pacts, at the end of a combo's line.
        private static string Heavy => " | Heavy: " + (HeavyPactPercent - 100) + "% slower";
        private static string Unnerving => " | Unnerving: -1/" + UnnervingEighths + " nerve";
        private static string Borrowed => " | Borrowed time: -" + Pm(1000 - BorrowedTimePermille) + " health";

        // "x2", "x1.5", "x0.75" from a percent; "2s", "1.5s" from ticks; "40%", "12.5%" from a permille. Integers only.
        private static string X(int percent) => "x" + Decimal(percent, 100);
        private static string Sec(int ticks) => Decimal(ticks, SimClock.TicksPerSecond) + "s";
        private static string Pm(int permille) => Decimal(permille, 10) + "%";
        private static string Ord(int n) => n + (n == 1 ? "st" : n == 2 ? "nd" : n == 3 ? "rd" : "th");

        // a over b, to at most two decimals, with no trailing zero.
        private static string Decimal(int a, int b)
        {
            int hundredths = a * 100 / b;
            int fraction = hundredths % 100;
            string whole = (hundredths / 100).ToString();
            if (fraction == 0) return whole;
            return whole + "." + (fraction % 10 == 0 ? (fraction / 10).ToString() : fraction.ToString("00"));
        }

        /// How big a hit with this detail is: a spell's hit is heavy, a spell cashing in a mark is a finisher, the rest
        /// are light. Only the class spells' hits are sorted here.
        public static HitTier TierOf(string detail)
        {
            switch (detail)
            {
                case AimedPinDetail:
                case StrikeOpenDetail:
                case RipDetail:
                case FlurryDetail: return HitTier.Finisher;
                case AimedDetail:
                case StrikeDetail:
                case BlackHoleDetail:
                case EventHorizonDetail:
                case CorridorDetail: return HitTier.Heavy;
                default: return HitTier.Light;
            }
        }
    }
}
