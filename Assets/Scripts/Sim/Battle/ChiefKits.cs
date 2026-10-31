using System;

namespace KindredSiege.Sim.Battle
{
    /// A chief's own moveset (Jacob, 20 Oct 2026, later; DESIGN_LOCKS, "Chief fights"): several moves, each on its own timer
    /// with its own tell, each with more than one answer that pays differently. Bloodridge's and Skullpeak's kits are written
    /// (22 Oct 2026, DESIGN_LOCKS "The Bloodridge chief finished and the Skullpeak chief's kit"), and Stoneveil's and
    /// Thundercoast's (later that day, DESIGN_LOCKS "The Stoneveil and Thundercoast chiefs"); every other chief gets his own
    /// before he is built.
    public enum ChiefKit
    {
        None = 0,

        /// Cleave every 6 s, Brand every 15 s, Eclipse every 25 s, Burning Ring every 12 s, Sun Mantle every 18 s, and Blood
        /// Sun at half health. His tactical piece: you see each of his moves, its target and how hard, 6 s before it is due.
        Bloodridge = 1,

        /// A slow grind behind walls: Steel Storm every 30 s, Hook every 9 s, Rifle Line every 15 s, Iron Advance every 20 s,
        /// Overclock every 25 s, and Lockdown at half health. Her tactical piece: her band has jobs.
        Skullpeak = 2,

        /// The hunters' captain at the end of a run (22 Oct 2026, DESIGN_LOCKS "The hunters' boss, the hunt"): the hunt, a bar
        /// of four steps over one of your fighters (HuntStep), which you break before it reaches the kill; two hunts at once
        /// from half health (the Pack). No moves of a pool, and no kneel: the hunters never join.
        Hunt = 3,

        /// The cave tribe's blood priest, the blood rite (22 Oct 2026, later, DESIGN_LOCKS "The Stoneveil and Thundercoast
        /// chiefs"): Blood Call every 20 s, the Basin every 15 s, Leeches every 12 s, Open Veins every 18 s, Transfusion every
        /// 16 s, and the Red Rite at half health. His tactical piece: Blood Price, every 6th order you give brings his next
        /// move at once.
        Stoneveil = 4,

        /// The sea tribe's chief, the tide's clock: in his fight the water rises and falls on a clock you can see. Tide every
        /// 25 s, Sluice Gate every 15 s, Riptide every 18 s, Tidal Bore every 20 s, Storm Surge every 30 s, and Spring Tide at
        /// half health. His tactical piece: moves that hit places, not people.
        Thundercoast = 5
    }

    /// The four steps of the hunt, in order. None while a hunt waits to start.
    public enum HuntStep
    {
        None = 0,

        /// His Trapper with the tracking eyes marks your most hurt fighter, the prey.
        Track = 1,

        /// His front row pushes in: his side's blows on the prey's row land a third harder.
        Drive = 2,

        /// A purple line glows, then his Trapper nets the prey: held, unable to dodge, and his Marksmen's shots crit them.
        Corner = 3,

        /// The captain's glaive comes down on the prey, a huge hit.
        Kill = 4
    }

    /// The moves a chief takes into a fight. Each run he takes ChiefKits.Takes of his pool (ChiefKits.Pool).
    [Flags]
    public enum ChiefMove
    {
        None = 0,

        /// His blade glows for 1 s, then a heavy hit on whoever stands across from him. A Hold as it falls parries it; his
        /// target's Warden Guard takes it; or you take it.
        Cleave = 1 << 0,

        /// A burning sigil over one of your fighters with a 4 s ring, then it bursts on that fighter and those beside them.
        /// Fall Back gets clear, a Warden covering them takes it halved, or your Medic heals through it.
        Brand = 1 << 1,

        /// His chief spell (ChiefSpell.Eclipse) on its own 25 s timer: parried, dodged or broken as before.
        Eclipse = 1 << 2,

        /// Bloodridge's fourth (22 Oct 2026): he spins his ring for 1.5 s, then throws it down one lane, through both your
        /// rows. Focus Fire him while it spins and your Marksman's shot knocks it out of his hand; Fall Back gets everyone
        /// clear; or you eat it.
        BurningRing = 1 << 3,

        /// Bloodridge's fifth: fire wraps him for 5 s and every blow on him burns whoever struck it. Switch your Focus Fire to
        /// his band, Fall Back so your blades stop, or you eat it.
        SunMantle = 1 << 4,

        /// Skullpeak's chief spell (ChiefSpell.SteelStorm) on its own 30 s timer: her Wardens lock shields over 2 s, then the
        /// wall stands 8 s and your blows do a tenth. Unleash smashes it; your Trapper's snare on one of her Wardens while
        /// they lock leaves it half built; or you wait it out.
        SteelStorm = 1 << 5,

        /// A chain glints at one of your back row for 1 s, then yanks them in front of her shields for 3 s, where every blow
        /// on them lands. A Hold as it bites snaps it and she staggers; your Warden's Guard on them anchors them; or they get
        /// dragged.
        Hook = 1 << 6,

        /// Three aim lines lock onto your most hurt fighter for 2 s, then a volley. Focus Fire a shooter and each one you hit
        /// loses his shot; Fall Back takes everyone out of sight; or your Medic tops them up first.
        RifleLine = 1 << 7,

        /// Her shield line marches for 4 s and crushes your front row each second. Hold braces against it but costs your own
        /// nerve; Unleash into the march staggers the line; Fall Back gives ground, so no damage, but her side gets nerve back.
        IronAdvance = 1 << 8,

        /// Her Medic jabs her band for 1.5 s, then they act twice as fast for 6 s. Focus Fire her Medic during the jab; Hold
        /// through the flurry; or your Trapper snares one of them.
        Overclock = 1 << 9,

        /// Stoneveil's chief spell (ChiefSpell.BloodCall) on its own 20 s timer: after a 3 s chant it rips the blood out of
        /// every bleeding fighter of yours at once. Fall Back gets clear; each treatment your Medic gives during the chant
        /// stitches one fighter's wound shut, so there is less to rip; or you eat it.
        BloodCall = 1 << 10,

        /// His acolyte carries a stone basin forward for 4 s, and it fills from every bleeding fighter of yours. Full, he
        /// drinks it: he heals a fifth, and his band hits a quarter harder for 6 s. Focus Fire the acolyte and it spills,
        /// Unleash kicks it over, or have nobody bleeding so it stays empty.
        Basin = 1 << 11,

        /// Cave leeches land on one of your back row and drink 3% of that fighter's health a second for 6 s, feeding it to his
        /// most hurt. Your Medic plucks them off, Fall Back shakes them loose, or you eat it.
        Leeches = 1 << 12,

        /// For 5 s every wound of yours bleeds twice as fast. A Hold presses the wounds and halves it while it runs; your
        /// Medic stitches; or you eat it.
        OpenVeins = 1 << 13,

        /// He channels for 3 s, drawing blood out of your most hurt fighter into his band. Focus Fire on him breaks the
        /// channel, your Warden covering them blocks the draw, or you eat it.
        Transfusion = 1 << 14,

        /// Thundercoast's chief spell (ChiefSpell.Tide) on its own 25 s timer: it drowns your back row, and hits harder the
        /// higher the water. Fall Back gets clear. At low water it barely lands.
        Tide = 1 << 15,

        /// His engineer, his band's Trapper, cranks a gate for 2 s; then the water jumps a level and the next low water comes
        /// late. Focus Fire the engineer while it grinds and it jams, Unleash slams it shut, or the water rises.
        SluiceGate = 1 << 16,

        /// For 5 s the current runs out, and anyone of yours who falls back is pulled into the deep: hurt, and doing nothing
        /// for 2 s. Hold and you stand fast, or do not Fall Back while it runs: it is the trap for the button that answers
        /// his Tide.
        Riptide = 1 << 17,

        /// A wall of water runs across your formation from one side to the other over 2 s, both rows, lane by lane, hitting
        /// and knocking down each fighter it reaches. A Warden it reaches breaks it there, so those past him are spared; Fall
        /// Back gets everyone clear; a Hold soaks it.
        TidalBore = 1 << 18,

        /// He sounds a horn for 3 s, then the water stands at its highest for 8 s. Focus Fire him during the horn and the call
        /// breaks, Hold through it to halve the nerve the water takes, or you eat it.
        StormSurge = 1 << 19
    }

    /// How a side answered one of a chief's moves. Each answer takes a chunk of his nerve and raises the side's Glory.
    public enum ChiefAnswer
    {
        None = 0,
        Parry = 1,
        Dodge = 2,
        Break = 3,
        Guard = 4,
        Heal = 5,

        /// Unleash smashed her wall or staggered her line (22 Oct 2026).
        Smash = 6,

        /// Your Trapper's snare held one of hers at the right moment.
        Snare = 7,

        /// A Hold braced against her march or through her flurry.
        Brace = 8,

        /// Your Focus Fire went to his band while his Sun Mantle burned, so nobody struck him.
        Switch = 9,

        /// Nobody of yours bled into his rite: the Stoneveil chief's Basin came back without filling (22 Oct 2026, later).
        Starve = 10
    }

    /// Glory, the style rank (Jacob, 20 Oct 2026: like Devil May Cry's): a letter from D to S.
    public enum GloryRank
    {
        D = 0,
        C = 1,
        B = 2,
        A = 3,
        S = 4
    }

    /// A job in a chief's band (Skullpeak's tactical piece, 22 Oct 2026): kill whoever holds it and one of her moves gets
    /// worse. Kill order matters.
    public enum BandJob
    {
        None = 0,

        /// Her first Warden. Once he is dead her walls come up slower: Steel Storm's lock takes twice as long, and in
        /// Lockdown your window after a smash lasts twice as long.
        Shieldbearer = 1,

        /// Her first Marksman. Once he is dead Rifle Line takes twice as long to aim.
        Spotter = 2
    }

    /// One of a chief's moves you can see coming (Bloodridge's tactical piece, 22 Oct 2026): which move, the ticks until it is
    /// due (0 once it is due and he only waits to be free), whom it marks (empty for a move that marks nobody; a second mark
    /// for a Brand under Blood Sun), and how hard: the damage before armour and answers on the one it marks, or for the
    /// Eclipse and the Sun Mantle, which mark nobody, a percent of each one's health (the Eclipse's hit, the Mantle's burn).
    public readonly struct ChiefForecast
    {
        public ChiefForecast(ChiefMove move, int ticksLeft, string targetId, string secondTargetId, int amount)
        {
            Move = move;
            TicksLeft = Math.Max(0, ticksLeft);
            TargetId = targetId ?? string.Empty;
            SecondTargetId = secondTargetId ?? string.Empty;
            Amount = amount;
        }

        public ChiefMove Move { get; }
        public int TicksLeft { get; }
        public string TargetId { get; }
        public string SecondTargetId { get; }
        public int Amount { get; }
    }

    /// The chief kits' rules and numbers. Every number is a starting value that waits for Jacob's play.
    ///
    /// Breaking him: every move you answer takes a chunk of his side's nerve. When it runs out he kneels and stays down with
    /// no timer, and you choose. Hold spares him (his tribe joins you with his spell); Focus Fire on him executes him (you take
    /// his piece). His Wardens step back while he kneels. This kneel comes to every chief in a fight with
    /// FightRules.ChiefKits, kit or not, and replaces the 20 Oct quarter health yield; his second phase and his bodyguards
    /// stay (ChiefFight).
    ///
    /// Glory: answers raise it, a perfect parry more than a dodge, a different answer from the last one by half again, the
    /// same answer twice running by half as much. Eating a big move (a Brand, an Eclipse, a Burning Ring or a Sun Mantle in
    /// full, a Rifle Line volley, a whole Iron Advance) drops a rank. At A answers pay half again in points, at S double, and
    /// at S your 5th button goes off twice as hard. The rank you hold when the fight ends is its grade. From 22 Oct 2026,
    /// later, eating a Blood Call, Leeches, Open Veins or a Transfusion of the Stoneveil chief's, or the Thundercoast chief's
    /// Tide, Riptide, Tidal Bore or Storm Surge, drops it too.
    ///
    /// The tactical pieces (FightRules.ChiefTactics, 22 Oct 2026) are their own system: Bloodridge's forecast, Skullpeak's
    /// band jobs and Stoneveil's Blood Price. Thundercoast's, moves that hit places, is his moves themselves, so it needs no
    /// switch.
    public static class ChiefKits
    {
        /// How many moves of his pool a chief takes into each run (Jacob: "Each run he takes 3 of his 5 moves").
        public const int Takes = 3;

        /// The three Bloodridge moves written on 21 Oct 2026: every move a kit had before 22 Oct. The campaign keeps a fight
        /// begun under its rules version 3 or 4 to these.
        public const ChiefMove FirstWritten = ChiefMove.Cleave | ChiefMove.Brand | ChiefMove.Eclipse;

        /// Every move written, and the last kit, for reading saved fights.
        public const ChiefMove AllMoves = ChiefMove.Cleave | ChiefMove.Brand | ChiefMove.Eclipse | ChiefMove.BurningRing |
            ChiefMove.SunMantle | ChiefMove.SteelStorm | ChiefMove.Hook | ChiefMove.RifleLine | ChiefMove.IronAdvance |
            ChiefMove.Overclock | ChiefMove.BloodCall | ChiefMove.Basin | ChiefMove.Leeches | ChiefMove.OpenVeins |
            ChiefMove.Transfusion | ChiefMove.Tide | ChiefMove.SluiceGate | ChiefMove.Riptide | ChiefMove.TidalBore |
            ChiefMove.StormSurge;
        public const ChiefKit LastKit = ChiefKit.Thundercoast;

        // ------------------------------------------------------------ Bloodridge

        public const int CleaveEveryTicks = 6 * SimClock.TicksPerSecond;
        public const int CleaveWindupTicks = SimClock.TicksPerSecond;
        public const int BrandEveryTicks = 15 * SimClock.TicksPerSecond;
        public const int BrandRingTicks = 4 * SimClock.TicksPerSecond;
        public const int EclipseEveryTicks = 25 * SimClock.TicksPerSecond;

        /// A Brand bursts for this percent of each one's max health; a Warden covering the branded one takes this percent of
        /// their share.
        public const int BrandPercent = 20;
        public const int BrandGuardPercent = 50;

        /// Burning Ring: every RingEveryTicks he spins it for RingSpinTicks, then it hits the fighter nearest its lane in each
        /// of your rows for RingPercent of their max health.
        public const int RingEveryTicks = 12 * SimClock.TicksPerSecond;
        public const int RingSpinTicks = 45;
        public const int RingPercent = 25;

        /// Sun Mantle: every MantleEveryTicks fire wraps him for MantleTicks, and each blow that lands on him burns its
        /// striker for MantleBurnPercent of the striker's max health.
        public const int MantleEveryTicks = 18 * SimClock.TicksPerSecond;
        public const int MantleTicks = 5 * SimClock.TicksPerSecond;
        public const int MantleBurnPercent = 10;

        /// His tactical piece: each of his moves shows this long before it is due.
        public const int ForecastTicks = 6 * SimClock.TicksPerSecond;

        /// Blood Sun, from half health: a landed Cleave leaves its victim bleeding this percent a second for BleedRules.Ticks,
        /// each Brand marks this many, and each Eclipse after it hits this percent as hard as the one before.
        public const int BloodSunBleedPercent = 3;
        public const int BloodSunBrands = 2;
        public const int BloodSunEclipsePercent = 120;

        /// Sunbreaker, his hidden chain: parry ChainCleaves Cleaves in a row, then his Eclipse, all perfect, and your Warden
        /// throws the Eclipse back at him. It hits him as hard as it would have hit one of yours (never his last health: it
        /// breaks, it does not kill), costs his side SunbreakerNerve, and stuns him for SunbreakerStunTicks.
        public const int ChainCleaves = 2;
        public const int SunbreakerNerve = 350;
        public const int SunbreakerStunTicks = 4 * SimClock.TicksPerSecond;
        public const string Sunbreaker = "Sunbreaker";
        public const string SunbreakerDetail = "sunbreaker";

        // ------------------------------------------------------------ Skullpeak

        /// Steel Storm: every SteelStormEveryTicks her Wardens lock shields for LockTicks; then the wall stands
        /// ChiefSpells.WallTicks. Half built, your blows on her side do HalfWallPercent instead of a tenth.
        public const int SteelStormEveryTicks = 30 * SimClock.TicksPerSecond;
        public const int LockTicks = 2 * SimClock.TicksPerSecond;
        public const int HalfWallPercent = 50;

        /// Hook: every HookEveryTicks a chain glints for HookTellTicks; dragged, its mark stays in front of her shields for
        /// HookTicks.
        public const int HookEveryTicks = 9 * SimClock.TicksPerSecond;
        public const int HookTellTicks = SimClock.TicksPerSecond;
        public const int HookTicks = 3 * SimClock.TicksPerSecond;

        /// Rifle Line: every RifleEveryTicks up to RifleShots of her shooters aim for RifleAimTicks; each shot that is not
        /// lost hits for RifleShotPercent of its mark's max health.
        public const int RifleEveryTicks = 15 * SimClock.TicksPerSecond;
        public const int RifleAimTicks = 2 * SimClock.TicksPerSecond;
        public const int RifleShots = 3;
        public const int RifleShotPercent = 15;

        /// Iron Advance: every AdvanceEveryTicks her line marches for AdvanceTicks and crushes your front row each second for
        /// AdvanceCrushPercent of each one's max health. Each braced second costs your side AdvanceBraceNerve; each second you
        /// give ground gives her side AdvanceGroundNerve back.
        public const int AdvanceEveryTicks = 20 * SimClock.TicksPerSecond;
        public const int AdvanceTicks = 4 * SimClock.TicksPerSecond;
        public const int AdvanceCrushPercent = 8;
        public const int AdvanceBraceNerve = 30;
        public const int AdvanceGroundNerve = 40;

        /// Overclock: every OverclockEveryTicks her Medic jabs for JabTicks, then her band acts twice as fast for
        /// OverclockTicks.
        public const int OverclockEveryTicks = 25 * SimClock.TicksPerSecond;
        public const int JabTicks = 45;
        public const int OverclockTicks = 6 * SimClock.TicksPerSecond;

        /// Lockdown, from half health: her wall never drops. An Unleash smashes it open for LockdownOpenTicks, and her Wardens
        /// have it whole again LockdownRebuildTicks after the smash; until then another Unleash finds nothing to smash. While
        /// no Warden of hers stands, it is down for good.
        public const int LockdownOpenTicks = 6 * SimClock.TicksPerSecond;
        public const int LockdownRebuildTicks = 12 * SimClock.TicksPerSecond;

        /// Breach, her hidden chain: smash BreachSmashes walls in a row, each within QuickSmashTicks of it going up or being
        /// whole again, then parry a Hook. Your Skirmisher runs the chain back and takes out her steel leg: she is down for
        /// BreachStunTicks and cannot Hook again that fight.
        public const int QuickSmashTicks = SimClock.TicksPerSecond;
        public const int BreachSmashes = 2;
        public const int BreachStunTicks = 4 * SimClock.TicksPerSecond;
        public const string Breach = "Breach";

        /// A job's holder dead makes its move take this percent as long.
        public const int JobSlowPercent = 200;

        // ------------------------------------------------------------ the hunt

        /// The first hunt starts this far into the fight; after a hunt is broken or its kill lands, the next starts at once,
        /// on a new prey.
        public const int HuntStartTicks = 5 * SimClock.TicksPerSecond;

        /// How long each step runs if you do not break it. Corner's is its purple line; the net then holds the prey NetTicks.
        public const int TrackTicks = 5 * SimClock.TicksPerSecond;
        public const int DriveTicks = 4 * SimClock.TicksPerSecond;
        public const int LineTicks = 2 * SimClock.TicksPerSecond;
        public const int NetTicks = 4 * SimClock.TicksPerSecond;
        public const int KillTicks = 45;

        /// A treatment that leaves the prey over this share of their health turns the scent cold.
        public const int ColdScentPermille = 500;

        /// While the Drive runs, his side's blows on the prey's row hit this percent as hard.
        public const int DrivePercent = 133;

        /// The Kill hits for this percent of the prey's max health, before armour; a Warden who steps in takes it less his
        /// Guard.
        public const int KillPercent = 60;

        /// Turned Hunt, the hunt's hidden chain: for TurnedTicks his band cannot cover him and your blows on him hit
        /// TurnedPercent as hard, and his hunts wait.
        public const int TurnedTicks = 6 * SimClock.TicksPerSecond;
        public const int TurnedPercent = 200;
        public const string TurnedHunt = "Turned Hunt";

        /// The Mark event's detail for the net, and the Hit detail for the Kill.
        public const string NetMark = "net";
        public const string KillDetail = "kill";

        // ------------------------------------------------------------ Stoneveil, the blood rite

        /// Blood Call, his spell: every BloodCallEveryTicks a chant of Moves.EclipseWindupTicks (3 s), then it rips the blood
        /// out of every bleeding fighter of yours, as the cave tribe's blood call always has.
        public const int BloodCallEveryTicks = 20 * SimClock.TicksPerSecond;

        /// The Basin: every BasinEveryTicks his acolyte carries it forward for BasinTicks. Each second it fills by one drop for
        /// each bleeding fighter of yours, and at BasinFull drops he drinks it: he heals DrinkHealPercent of his health, and his
        /// band's blows land DrinkPercent as hard for DrinkTicks.
        public const int BasinEveryTicks = 15 * SimClock.TicksPerSecond;
        public const int BasinTicks = 4 * SimClock.TicksPerSecond;
        public const int BasinFull = 4;
        public const int DrinkHealPercent = 20;
        public const int DrinkPercent = 125;
        public const int DrinkTicks = 6 * SimClock.TicksPerSecond;

        /// Leeches: every LeechEveryTicks they land on one of your back row for LeechTicks and drink LeechPercent of that
        /// fighter's health each second, which heals his most hurt.
        public const int LeechEveryTicks = 12 * SimClock.TicksPerSecond;
        public const int LeechTicks = 6 * SimClock.TicksPerSecond;
        public const int LeechPercent = 3;

        /// Open Veins: every VeinsEveryTicks, for VeinsTicks, each second of bleeding on your side takes VeinsPercent as much;
        /// while you hold, PressPercent of that.
        public const int VeinsEveryTicks = 18 * SimClock.TicksPerSecond;
        public const int VeinsTicks = 5 * SimClock.TicksPerSecond;
        public const int VeinsPercent = 200;
        public const int PressPercent = 50;

        /// Transfusion: every TransfusionEveryTicks he channels for TransfusionTicks, and at each second of it draws
        /// TransfusionPercent of your most hurt fighter's health into his band's most hurt.
        public const int TransfusionEveryTicks = 16 * SimClock.TicksPerSecond;
        public const int TransfusionTicks = 3 * SimClock.TicksPerSecond;
        public const int TransfusionPercent = 6;

        /// Blood Price, his tactical piece: every PriceOrders orders you give, his next move comes at once.
        public const int PriceOrders = 6;
        public const string BloodPriceName = "Blood Price";

        /// The Red Rite, from half health: your wounds bleed RedRitePercent as long.
        public const int RedRitePercent = 200;

        /// Clean Hands, his hidden chain: CleanHandsLands of his moves land in a row with nobody of yours bleeding. The rite
        /// fails: he is stunned CleanHandsStunTicks, and his band's blades leave no more wounds that fight.
        public const int CleanHandsLands = 3;
        public const int CleanHandsStunTicks = 4 * SimClock.TicksPerSecond;
        public const string CleanHands = "Clean Hands";

        // ------------------------------------------------------------ Thundercoast, the tide's clock

        /// Tide, his spell, every TideEveryTicks.
        public const int TideEveryTicks = 25 * SimClock.TicksPerSecond;

        /// The tide table: the clock turns through TideSteps steps of FloodRules.StepTicks each, the water at 0 (low water),
        /// 1, 2, 3 (high water), 2 and 1, then round again. The fight starts at low water.
        public const int TideSteps = 6;
        public const int TideCycleTicks = TideSteps * FloodRules.StepTicks;
        public const int HighStep = 3;

        /// His Tide hits TidePercentPerLevel more for each level the water stands at, from TidePercentPerLevel at low water:
        /// a quarter of its full hit at low water, all of it at high water.
        public const int TidePercentPerLevel = 25;

        /// At low water his side is stranded: his fighters wait StrandedSlowPercent longer between actions, and their blows
        /// land StrandedWeakPercent lighter.
        public const int StrandedSlowPercent = 20;
        public const int StrandedWeakPercent = 20;

        /// Sluice Gate: every SluiceEveryTicks his engineer cranks for CrankTicks; then the water stands a level higher until
        /// the next low water, and the clock holds SluiceHoldTicks, so that low water comes late.
        public const int SluiceEveryTicks = 15 * SimClock.TicksPerSecond;
        public const int CrankTicks = 2 * SimClock.TicksPerSecond;
        public const int SluiceHoldTicks = 10 * SimClock.TicksPerSecond;

        /// Riptide: every RiptideEveryTicks, for RiptideTicks; a fighter it pulls takes RiptidePercent of their health and does
        /// nothing for PulledTicks.
        public const int RiptideEveryTicks = 18 * SimClock.TicksPerSecond;
        public const int RiptideTicks = 5 * SimClock.TicksPerSecond;
        public const int RiptidePercent = 10;
        public const int PulledTicks = 2 * SimClock.TicksPerSecond;

        /// Tidal Bore: every BoreEveryTicks it runs across your formation in BoreTicks. A fighter it reaches takes BorePercent of
        /// their health and is knocked down for KnockTicks; held, BoreSoakPercent of that and no knock down. A Warden it
        /// reaches breaks it and takes it less his Guard.
        public const int BoreEveryTicks = 20 * SimClock.TicksPerSecond;
        public const int BoreTicks = 2 * SimClock.TicksPerSecond;
        public const int BorePercent = 12;
        public const int KnockTicks = SimClock.TicksPerSecond;
        public const int BoreSoakPercent = 50;

        /// Storm Surge: every SurgeEveryTicks he sounds a horn for HornTicks, then the water stands at its highest for
        /// SurgeTicks. While you hold, the water takes SurgeHoldPercent of the nerve it would.
        public const int SurgeEveryTicks = 30 * SimClock.TicksPerSecond;
        public const int HornTicks = 3 * SimClock.TicksPerSecond;
        public const int SurgeTicks = 8 * SimClock.TicksPerSecond;
        public const int SurgeHoldPercent = 50;

        /// Spring Tide, from half health: the clock turns SpringSpeed times as fast and the water never drops below
        /// SpringFloor; his Tide drowns both your rows.
        public const int SpringSpeed = 2;
        public const int SpringFloor = 1;

        /// Turn of the Tide, his hidden chain: TurnAnswers of his moves answered in a row while the water stands at its
        /// highest. The water drains to nothing for DrainTicks, so his fishmen are stranded.
        public const int TurnAnswers = 3;
        public const int DrainTicks = 10 * SimClock.TicksPerSecond;
        public const string TurnOfTheTide = "Turn of the Tide";

        /// Why the water changed, the detail of a Tide event: the tide table turned, his Sluice Gate, his Storm Surge (up, and
        /// back down when it ends), your Turn of the Tide (down, and back when it ends), or his Spring Tide's floor.
        public const string TideClock = "clock";
        public const string TideGate = "gate";
        public const string TideSurge = "surge";
        public const string TideDrain = "drain";
        public const string TideSpring = "spring";

        // ------------------------------------------------------------ answers

        /// The chunk of his nerve each answer takes.
        public const int ParryNerve = 100;
        public const int BreakNerve = 80;
        public const int DodgeNerve = 60;
        public const int GuardNerve = 40;
        public const int HealNerve = 40;
        public const int SmashNerve = 80;
        public const int SnareNerve = 60;
        public const int BraceNerve = 40;
        public const int SwitchNerve = 60;
        public const int StarveNerve = 60;

        /// Points a Warden's guard, a heal through, a snare at the right moment, a switch or a starved Basin earns, before
        /// Glory. A brace earns none: it already costs your nerve or soaks.
        public const int GuardPoints = 1;
        public const int HealPoints = 1;
        public const int SnarePoints = 1;
        public const int SwitchPoints = 1;
        public const int StarvePoints = 1;

        // ------------------------------------------------------------ Glory

        public const int GloryMax = 1000;
        public const int GloryPerRank = 200;
        public const int ParryGlory = 150;
        public const int BreakGlory = 120;
        public const int DodgeGlory = 80;
        public const int GuardGlory = 60;
        public const int HealGlory = 60;
        public const int SmashGlory = 120;
        public const int SnareGlory = 100;
        public const int BraceGlory = 60;
        public const int SwitchGlory = 80;
        public const int StarveGlory = 80;
        public const int MixedPercent = 150;
        public const int RepeatPercent = 50;

        /// Each grade above D sends home this percent more of a chief's materials.
        public const int GradeMaterialsPercent = 25;

        /// The Earn reason of the points Glory adds.
        public const string GloryReason = "glory";

        /// The Glory detail when a side ate a big move.
        public const string EatenDetail = "eaten";

        // ------------------------------------------------------------ names and details

        public const string CleaveName = "Cleave";
        public const string BrandName = "Brand";
        public const string RingName = "Burning Ring";
        public const string MantleName = "Sun Mantle";
        public const string HookName = "Hook";
        public const string RifleName = "Rifle Line";
        public const string AdvanceName = "Iron Advance";
        public const string OverclockName = "Overclock";
        public const string BasinName = "Basin";
        public const string LeechName = "Leeches";
        public const string VeinsName = "Open Veins";
        public const string TransfusionName = "Transfusion";
        public const string SluiceName = "Sluice Gate";
        public const string RiptideName = "Riptide";
        public const string BoreName = "Tidal Bore";
        public const string SurgeName = "Storm Surge";

        /// Hit details.
        public const string CleaveDetail = "cleave";
        public const string BrandDetail = "brand";
        public const string ExecuteDetail = "execute";
        public const string RingDetail = "ring";
        public const string MantleDetail = "mantle";
        public const string RifleDetail = "rifle";
        public const string AdvanceDetail = "advance";
        public const string LeechDetail = "leech";
        public const string TransfusionDetail = "transfusion";
        public const string RiptideDetail = "riptide";
        public const string BoreDetail = "bore";

        /// Heal details: the Stoneveil chief drinking his Basin, and his Red Rite's Blood Call healing him (ChiefSpells.BloodDetail);
        /// what his Leeches and Transfusion take goes to his side under their hit details.
        public const string BasinDetail = "basin";

        /// The Cash event's detail when your Medic's treatment stitches a bleeding wound shut in the Stoneveil chief's fight.
        public const string StitchDetail = "stitch";

        /// How a move landed, after its name and a dot on a Land event.
        public const string Parried = "parried";
        public const string Guarded = "guarded";
        public const string Dodged = "dodged";
        public const string Healed = "healed";
        public const string Soaked = "soaked";
        public const string Full = "full";
        public const string Broken = "broken";
        public const string Braced = "braced";
        public const string Staggered = "staggered";
        public const string Switched = "switched";
        public const string Clear = "clear";
        public const string Torn = "torn";
        public const string Smashed = "smashed";
        public const string Empty = "empty";

        /// Wall event details for Skullpeak (with ChiefSpells' "up", "smashed" and "fell"): her Steel Storm went up half built;
        /// her Lockdown wall went up for good; her Wardens have it whole again.
        public const string WallHalf = "half";
        public const string WallLockdown = "lockdown";
        public const string WallRebuilt = "rebuilt";

        /// The Yield event's detail for a kneel, and the actor id of a Spare when the fight's clock ran out first.
        public const string KneelDetail = "kneel";
        public const string TimeActor = "time";

        /// Line keys (BattleEventKind.Line).
        public const string ParryLine = "parry";
        public const string EclipseLine = "eclipse";
        public const string BloodSunLine = "bloodsun";
        public const string LockdownLine = "lockdown";
        public const string PackLine = "pack";
        public const string RedRiteLine = "redrite";
        public const string SpringTideLine = "springtide";
        public const string KneelLine = "kneel";

        /// The answer to a kneeling chief's verdict for the screen and a refused order.
        public const string KneelFailure = "He kneels. Hold spares him; Focus Fire on him executes him.";

        /// The moves a kit's chief can take: five each for Bloodridge, Skullpeak, Stoneveil and Thundercoast.
        public static ChiefMove Pool(ChiefKit kit)
        {
            switch (kit)
            {
                case ChiefKit.Bloodridge: return FirstWritten | ChiefMove.BurningRing | ChiefMove.SunMantle;
                case ChiefKit.Skullpeak:
                    return ChiefMove.SteelStorm | ChiefMove.Hook | ChiefMove.RifleLine | ChiefMove.IronAdvance | ChiefMove.Overclock;
                case ChiefKit.Stoneveil:
                    return ChiefMove.BloodCall | ChiefMove.Basin | ChiefMove.Leeches | ChiefMove.OpenVeins | ChiefMove.Transfusion;
                case ChiefKit.Thundercoast:
                    return ChiefMove.Tide | ChiefMove.SluiceGate | ChiefMove.Riptide | ChiefMove.TidalBore | ChiefMove.StormSurge;
                default: return ChiefMove.None;
            }
        }

        /// The move that is a kit's chief spell, cast through the 5th button's slot on its own clock: Bloodridge's Eclipse,
        /// Skullpeak's Steel Storm, Stoneveil's Blood Call, Thundercoast's Tide.
        public static ChiefMove SpellMove(ChiefKit kit)
        {
            switch (kit)
            {
                case ChiefKit.Bloodridge: return ChiefMove.Eclipse;
                case ChiefKit.Skullpeak: return ChiefMove.SteelStorm;
                case ChiefKit.Stoneveil: return ChiefMove.BloodCall;
                case ChiefKit.Thundercoast: return ChiefMove.Tide;
                default: return ChiefMove.None;
            }
        }

        /// How often a kit's chief casts his spell when he took it: SideRules.AutoEclipseTicks. 0 without one.
        public static int SpellEveryTicks(ChiefKit kit)
        {
            switch (kit)
            {
                case ChiefKit.Bloodridge: return EclipseEveryTicks;
                case ChiefKit.Skullpeak: return SteelStormEveryTicks;
                case ChiefKit.Stoneveil: return BloodCallEveryTicks;
                case ChiefKit.Thundercoast: return TideEveryTicks;
                default: return 0;
            }
        }

        /// What his second phase is called in his kit, for the Phase event's detail. Empty without a kit.
        public static string PhaseName(ChiefKit kit)
        {
            switch (kit)
            {
                case ChiefKit.Bloodridge: return "Blood Sun";
                case ChiefKit.Skullpeak: return "Lockdown";
                case ChiefKit.Hunt: return "Pack";
                case ChiefKit.Stoneveil: return "Red Rite";
                case ChiefKit.Thundercoast: return "Spring Tide";
                default: return string.Empty;
            }
        }

        /// The line key a kit's chief says at his second phase.
        public static string PhaseLine(ChiefKit kit)
        {
            switch (kit)
            {
                case ChiefKit.Bloodridge: return BloodSunLine;
                case ChiefKit.Skullpeak: return LockdownLine;
                case ChiefKit.Hunt: return PackLine;
                case ChiefKit.Stoneveil: return RedRiteLine;
                case ChiefKit.Thundercoast: return SpringTideLine;
                default: return string.Empty;
            }
        }

        /// The name of the hidden chain each kit's chief hides (the Combo event's detail when it goes off). Empty without one.
        public static string ChainName(ChiefKit kit)
        {
            switch (kit)
            {
                case ChiefKit.Bloodridge: return Sunbreaker;
                case ChiefKit.Skullpeak: return Breach;
                case ChiefKit.Hunt: return TurnedHunt;
                case ChiefKit.Stoneveil: return CleanHands;
                case ChiefKit.Thundercoast: return TurnOfTheTide;
                default: return string.Empty;
            }
        }

        /// A hunt step's name, as on its Tell and Land events: "Track", "Drive", "Corner" or "Kill".
        public static string Name(HuntStep step)
        {
            switch (step)
            {
                case HuntStep.Track: return "Track";
                case HuntStep.Drive: return "Drive";
                case HuntStep.Corner: return "Corner";
                case HuntStep.Kill: return "Kill";
                default: return string.Empty;
            }
        }

        /// A move's name, as on its Tell, Land and Forecast events.
        public static string Name(ChiefMove move)
        {
            switch (move)
            {
                case ChiefMove.Cleave: return CleaveName;
                case ChiefMove.Brand: return BrandName;
                case ChiefMove.Eclipse: return "Eclipse";
                case ChiefMove.BurningRing: return RingName;
                case ChiefMove.SunMantle: return MantleName;
                case ChiefMove.SteelStorm: return ChiefSpells.Name(ChiefSpell.SteelStorm);
                case ChiefMove.Hook: return HookName;
                case ChiefMove.RifleLine: return RifleName;
                case ChiefMove.IronAdvance: return AdvanceName;
                case ChiefMove.Overclock: return OverclockName;
                case ChiefMove.BloodCall: return ChiefSpells.Name(ChiefSpell.BloodCall);
                case ChiefMove.Basin: return BasinName;
                case ChiefMove.Leeches: return LeechName;
                case ChiefMove.OpenVeins: return VeinsName;
                case ChiefMove.Transfusion: return TransfusionName;
                case ChiefMove.Tide: return ChiefSpells.Name(ChiefSpell.Tide);
                case ChiefMove.SluiceGate: return SluiceName;
                case ChiefMove.Riptide: return RiptideName;
                case ChiefMove.TidalBore: return BoreName;
                case ChiefMove.StormSurge: return SurgeName;
                default: return string.Empty;
            }
        }

        /// An answer's name, as on a Glory event: "parry", "dodge", "break", "guard", "heal", "smash", "snare", "brace",
        /// "switch" or "starve".
        public static string Name(ChiefAnswer answer)
        {
            switch (answer)
            {
                case ChiefAnswer.Parry: return "parry";
                case ChiefAnswer.Dodge: return "dodge";
                case ChiefAnswer.Break: return "break";
                case ChiefAnswer.Guard: return "guard";
                case ChiefAnswer.Heal: return "heal";
                case ChiefAnswer.Smash: return "smash";
                case ChiefAnswer.Snare: return "snare";
                case ChiefAnswer.Brace: return "brace";
                case ChiefAnswer.Switch: return "switch";
                case ChiefAnswer.Starve: return "starve";
                default: return string.Empty;
            }
        }

        /// A band job's name: "Shieldbearer" or "Spotter".
        public static string Name(BandJob job)
        {
            switch (job)
            {
                case BandJob.Shieldbearer: return "Shieldbearer";
                case BandJob.Spotter: return "Spotter";
                default: return string.Empty;
            }
        }

        /// The rank this much Glory holds.
        public static GloryRank Rank(int glory)
        {
            return (GloryRank)Math.Max(0, Math.Min((int)GloryRank.S, glory / GloryPerRank));
        }

        /// The letter for a rank: "D" to "S".
        public static string Letter(GloryRank rank)
        {
            return rank.ToString();
        }

        /// The points Glory adds to an answer that earned these: half again at A (rounded up), double at S, none below A.
        public static int GloryBonus(GloryRank rank, int earned)
        {
            if (earned <= 0) return 0;
            return rank == GloryRank.S ? earned : rank == GloryRank.A ? (earned + 1) / 2 : 0;
        }

        /// What an answer adds to Glory before mixing.
        public static int Glory(ChiefAnswer answer)
        {
            switch (answer)
            {
                case ChiefAnswer.Parry: return ParryGlory;
                case ChiefAnswer.Break: return BreakGlory;
                case ChiefAnswer.Dodge: return DodgeGlory;
                case ChiefAnswer.Guard: return GuardGlory;
                case ChiefAnswer.Heal: return HealGlory;
                case ChiefAnswer.Smash: return SmashGlory;
                case ChiefAnswer.Snare: return SnareGlory;
                case ChiefAnswer.Brace: return BraceGlory;
                case ChiefAnswer.Switch: return SwitchGlory;
                case ChiefAnswer.Starve: return StarveGlory;
                default: return 0;
            }
        }

        /// The chunk of his nerve an answer takes.
        public static int Nerve(ChiefAnswer answer)
        {
            switch (answer)
            {
                case ChiefAnswer.Parry: return ParryNerve;
                case ChiefAnswer.Break: return BreakNerve;
                case ChiefAnswer.Dodge: return DodgeNerve;
                case ChiefAnswer.Guard: return GuardNerve;
                case ChiefAnswer.Heal: return HealNerve;
                case ChiefAnswer.Smash: return SmashNerve;
                case ChiefAnswer.Snare: return SnareNerve;
                case ChiefAnswer.Brace: return BraceNerve;
                case ChiefAnswer.Switch: return SwitchNerve;
                case ChiefAnswer.Starve: return StarveNerve;
                default: return 0;
            }
        }

        /// What a chief says at a big moment, from his kit and the line's key. His kneel line changes once you have met him
        /// before. Empty when there are no words yet: the Line event still comes, so the screen can hold the moment.
        /// Bloodridge's lines are from the page Jacob approved on 20 Oct 2026; his first kneel line, "Finish it.", is from the
        /// 22 Oct lock, Jacob's to rewrite. Skullpeak talks at the same moments (her first parried move, her Steel Storm going
        /// up whole, her Lockdown, her kneel), and her words are Jacob's to write. So do the Stoneveil and Thundercoast chiefs
        /// (a spell you eat, the Red Rite or Spring Tide, the kneel), and theirs are Jacob's too.
        public static string Line(ChiefKit kit, string key, bool metBefore)
        {
            if (kit != ChiefKit.Bloodridge) return string.Empty;
            switch (key)
            {
                case ParryLine: return "Lucky.";
                case EclipseLine: return "Kneel.";
                case BloodSunLine: return "Now you burn.";
                case KneelLine: return metBefore ? "You again." : "Finish it.";
                default: return string.Empty;
            }
        }
    }
}
