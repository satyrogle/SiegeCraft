using System;

namespace KindredSiege.Sim.Battle
{
    /// The slice's big moves (Jacob, 4 Oct 2026). Every number here is a placeholder for tuning.
    ///
    /// Eclipse: the raider chief's signature. It winds up for a moment you can see, then hits the
    /// enemy front row for a share of each unit's health and shakes their nerve. Hold soaks half
    /// of it, Fall Back dodges it, and hitting the caster hard enough during the windup breaks it
    /// off and doubles your next order. Break the chief and it is yours: its grade (how hard it
    /// hits) comes from how cleanly you won.
    ///
    /// Frenzy: the Pal transforms once a fight, for free. The Pal hits twice as hard and every
    /// ally a little harder while it lasts.
    ///
    /// Combos: certain orders in a row fuse into one big attack, each once a fight. Ambush is Fall
    /// Back then Unleash: the enemy back row is caught in the open. Pyre is Focus Fire, Unleash,
    /// then Eclipse: when the Eclipse goes off, the focused target burns.
    public static class Moves
    {
        public const int EclipseWindupTicks = 90;

        /// Measured on 4 Oct 2026: at 100 nerve and 15/20/25 percent, a player who never answered
        /// the chief's Eclipse won as often as one who did, so it now hurts enough to answer. Raised from 200 on
        /// 18 Oct 2026 (Jacob: "go, tune the chief spells to those numbers"): a chief spell must be a real threat.
        public const int EclipseNerveCost = 350;
        public const int EclipseHoldPercent = 50;
        public const int MaxEclipseGrade = 3;

        /// A caster that loses this percent of its max health during the windup is interrupted.
        /// Measured on 4 Oct 2026: at 10, and at 6, focus fire on the chief never broke his cast
        /// (about 60 damage reached him in a windup). At this it can, unless his Warden has
        /// learned to cover whoever you focus. Since 6 Oct 2026 only blows from a side that is
        /// focusing the caster count (Jacob's playtest: stray hits on the Butcher broke his Eclipse).
        public const int InterruptPercent = 4;

        /// A chief is worn out for this long after his own Eclipse goes off, dodged or not, and
        /// takes this percent of every blow meanwhile (Jacob's playtest, 6 Oct 2026: the rematch
        /// chief was a wall). So a dodge can be punished.
        public const int WornOutTicks = 120;
        public const int WornOutDamagePercent = 200;

        /// An empowered order runs this percent as long, or hits this percent as hard.
        public const int EmpoweredPercent = 200;

        public const int FrenzyTicks = 240;
        public const int FrenzyPalPowerPercent = 200;
        public const int FrenzyAllyPowerPercent = 130;

        public const string Ambush = "Ambush";
        public const int AmbushWindowTicks = 90;
        public const int AmbushPercent = 10;
        public const int AmbushNerveCost = 80;

        public const string Pyre = "Pyre";
        public const int PyreWindowTicks = 180;
        public const int PyrePercent = 30;
        public const int PyreNerveCost = 150;

        /// Hit details for the moves' damage, so lessons that count plain hits leave them out.
        public const string EclipseDetail = "eclipse";
        public const string AmbushDetail = "ambush";
        public const string PyreDetail = "pyre";

        /// Break and finish reason when a side's Pal or Chief drops.
        public const string LeaderDetail = "leader";

        /// Release outcomes.
        public const string Full = "full";
        public const string Soaked = "soaked";
        public const string Dodged = "dodged";

        /// Percent of each target's max health an Eclipse of this grade hits for. 0 for no Eclipse. Doubled from
        /// 15/20/25 on 18 Oct 2026 (Jacob: "go, tune the chief spells to those numbers"); every spell that hits a row
        /// uses it, the Dust Shark's bite and the hunters' Firebomb too.
        public static int EclipsePercent(int grade)
        {
            switch (grade)
            {
                case 1: return 30;
                case 2: return 40;
                case 3: return 50;
                default: return 0;
            }
        }
    }

    /// What a side can do beyond the five orders: its Eclipse and grade (0 for none), how often
    /// its chief casts Eclipse on his own (0 for never; the player casts by order), whether
    /// its orders fuse into combos, how often its fighters start a heavy swing (0 for never,
    /// see HeavyRules), its chief spell, and what a band of hunters brings (BandTrait). None is the old rules.
    /// A chief's kit (ChiefKits, 21 Oct 2026) and the moves he took from it this run come last; only fights with
    /// FightRules.ChiefKits play them. After them, the beast this side is (BeastKits, 21 Oct 2026); only fights with
    /// FightRules.Beasts play it.
    public sealed class SideRules
    {
        public static readonly SideRules None = new SideRules(0, 0, false);

        public SideRules(int eclipseGrade, int autoEclipseTicks, bool combos, int heavyEveryTicks = 0,
            ChiefSpell spell = ChiefSpell.Eclipse, BandTrait band = BandTrait.None, SideFavours favours = null, bool flood = false,
            ChiefKit kit = ChiefKit.None, ChiefMove moves = ChiefMove.None, BeastKit beast = BeastKit.None, PalKit pals = null)
        {
            EclipseGrade = Math.Max(0, Math.Min(Moves.MaxEclipseGrade, eclipseGrade));
            AutoEclipseTicks = Math.Max(0, autoEclipseTicks);
            Combos = combos;
            HeavyEveryTicks = Math.Max(0, heavyEveryTicks);
            Spell = spell;
            Band = band & (BandTrait)BandRules.All;
            Favours = favours ?? SideFavours.None;
            Flood = flood;
            Kit = kit;
            KitMoves = kit == ChiefKit.None ? ChiefMove.None : moves & ChiefKits.Pool(kit);
            Beast = beast;
            Pals = pals ?? PalKit.None;
        }

        /// What this side brings from the Pals' stores (rules 23): the Shade's routes and the cheaper hook. None unless set.
        public PalKit Pals { get; }

        /// These rules with this Pal kit.
        public SideRules WithPals(PalKit pals)
        {
            return new SideRules(EclipseGrade, AutoEclipseTicks, Combos, HeavyEveryTicks, Spell, Band, Favours, Flood, Kit, KitMoves, Beast, pals);
        }

        public int EclipseGrade { get; }
        public int AutoEclipseTicks { get; }
        public bool Combos { get; }
        public int HeavyEveryTicks { get; }

        /// What the side's chief order does when it goes off (ChiefSpells). The Eclipse unless set.
        public ChiefSpell Spell { get; }

        /// What a band of hunters brings to this side (BandTrait, 18 Oct 2026). None unless set.
        public BandTrait Band { get; }

        /// What the Pals' favours bend in this side's orders (SideFavours, 18 Oct 2026). None unless set.
        public SideFavours Favours { get; }

        /// This side floods the fight (FloodRules; the sea tribe, 18 Oct 2026). False unless set.
        public bool Flood { get; }

        /// This side's chief fights with his own kit (ChiefKits, 21 Oct 2026), taking these moves from it. None unless set.
        public ChiefKit Kit { get; }
        public ChiefMove KitMoves { get; }

        /// This side is a beast (BeastKits, 21 Oct 2026): its units are the beast's parts. None unless set.
        public BeastKit Beast { get; }

        public bool IsNone => EclipseGrade == 0 && AutoEclipseTicks == 0 && !Combos && HeavyEveryTicks == 0 && Band == BandTrait.None &&
            Favours.IsNone && !Flood && Kit == ChiefKit.None && Beast == BeastKit.None && Pals.IsNone;

        public void AppendTo(SnapshotBuilder b, string prefix)
        {
            b.Field(prefix + "eclipseGrade", EclipseGrade);
            b.Field(prefix + "autoEclipse", AutoEclipseTicks);
            b.Field(prefix + "combos", Combos);
            // Written only when on, so every older fight keeps its checksum.
            if (HeavyEveryTicks > 0) b.Field(prefix + "heavyEvery", HeavyEveryTicks);
            if (Spell != ChiefSpell.Eclipse) b.Field(prefix + "spell", (int)Spell);
            if (Band != BandTrait.None) b.Field(prefix + "band", (int)Band);
            Favours.AppendTo(b, prefix + "favour.");
            if (Flood) b.Field(prefix + "flood", true);
            if (Kit != ChiefKit.None)
            {
                b.Field(prefix + "kit", (int)Kit);
                b.Field(prefix + "moves", (int)KitMoves);
            }
            if (Beast != BeastKit.None) b.Field(prefix + "beast", (int)Beast);
            Pals.AppendTo(b, prefix + "pals.");
        }
    }

    /// Every chief's order is a question one of your buttons answers (Jacob, 18 Oct 2026), so a player learns
    /// four answers, not 24 spells. Each spell keeps the Eclipse order's slot: the 5th button, and the chief's own
    /// casts. Its windup is the tell.
    public enum ChiefSpell
    {
        /// Bloodridge: a big hit on the front row. Answered by Hold, the parry.
        Eclipse = 0,

        /// Skullpeak: their Wardens lock shields into a steel wall and your hits barely land while it stands.
        /// Answered by Unleash, which smashes it open (Jacob, 18 Oct 2026: "i like 3 unleash its simple").
        SteelStorm = 1,

        /// A hunter's (Learning.HunterTraits, 18 Oct 2026): fire on your front row. A Hold can neither parry nor soak
        /// it and nothing breaks the cast. Answered by Fall Back, which dodges it.
        Firebomb = 2,

        /// A hunter's: a cry that puts nerve back into their band. It cannot be dodged or parried; a Focus Fire on
        /// the caster breaks it off, like an Eclipse. Answered by Focus Fire.
        WarCry = 3,

        /// The Clarth's, so yours from the first fight (Jacob, 18 Oct 2026: "2 Dust Shark"): while it winds up, red sand
        /// whirls round the caster's side and blows on it mostly miss; then a sand shark bites the enemy's back row. A
        /// Hold can neither parry nor soak the bite and nothing breaks the cast. Answered by Fall Back, which dodges it.
        DustShark = 4,

        /// The cave tribe's (Stoneveil; Jacob, 18 Oct 2026: "Cave -2"): their blades leave wounds that keep bleeding
        /// (BleedRules), and the blood call rips the blood out of every bleeding fighter on the other side at once. A Hold
        /// can neither parry nor soak it and nothing breaks the cast. Answered by Fall Back, which gets clear of it.
        BloodCall = 5,

        /// The sea tribe's (Thundercoast; Jacob, 18 Oct 2026: "Sea - 6"): the tide drowns the enemy's back row. A Hold can
        /// neither parry nor soak it and nothing breaks the cast. Answered by Fall Back, which gets clear of it.
        Tide = 6
    }

    public static class ChiefSpells
    {
        public const int Count = 7;

        /// While a Dust Shark winds up, blows on the caster's side land this many percent less often (never below
        /// FightRules.MinHitPercent). A starting value.
        public const int DustAvoid = 40;

        /// The damage detail of a Dust Shark's bite.
        public const string BiteDetail = "shark";

        /// The damage details of a blood call and a tide (18 Oct 2026).
        public const string BloodDetail = "blood";
        public const string TideDetail = "tide";

        /// Nerve a War Cry gives back to its side, never above full.
        public const int WarCryNerve = 250;

        /// The Release detail of a War Cry that went off.
        public const string Cried = "cried";

        /// How long a Steel Storm wall stands, in ticks (eight seconds; six until Jacob's 18 Oct tune).
        public const int WallTicks = 240;

        /// While the wall stands, the enemy's blows on its side hit this percent as hard (30 until 18 Oct 2026).
        public const int WallDamagePercent = 10;

        /// Nerve a side loses when an Unleash smashes its wall open (120 until 18 Oct 2026).
        public const int WallSmashNerve = 300;

        /// Points a side gets back for smashing a wall: half the Unleash.
        public const int SmashPoints = 1;
        public const string SmashReason = "smash";

        /// Details of a Wall event: it went up, an Unleash smashed it, or it fell on its own.
        public const string WallUp = "up";
        public const string WallSmashed = "smashed";
        public const string WallFell = "fell";

        /// The one button that answers this spell, for the screen's cut in and its colour.
        public static Directive Answer(ChiefSpell spell)
        {
            switch (spell)
            {
                case ChiefSpell.SteelStorm: return Directive.Unleash;
                case ChiefSpell.Firebomb: return Directive.FallBack;
                case ChiefSpell.DustShark: return Directive.FallBack;
                case ChiefSpell.BloodCall: return Directive.FallBack;
                case ChiefSpell.Tide: return Directive.FallBack;
                case ChiefSpell.WarCry: return Directive.FocusFire;
                default: return Directive.Hold;
            }
        }

        /// The spell's name, as on the Windup event.
        public static string Name(ChiefSpell spell)
        {
            switch (spell)
            {
                case ChiefSpell.SteelStorm: return "Steel Storm";
                case ChiefSpell.Firebomb: return "Firebomb";
                case ChiefSpell.WarCry: return "War Cry";
                case ChiefSpell.DustShark: return "Dust Shark";
                case ChiefSpell.BloodCall: return "Blood Call";
                case ChiefSpell.Tide: return "Tide";
                default: return "Eclipse";
            }
        }

        /// Whether a Focus Fire on the caster can break this spell off in its windup: the Eclipse and the War Cry.
        public static bool CanBreak(ChiefSpell spell)
        {
            return spell == ChiefSpell.Eclipse || spell == ChiefSpell.WarCry;
        }
    }
}
