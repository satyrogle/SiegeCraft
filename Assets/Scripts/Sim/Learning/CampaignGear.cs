using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// Run gear in three tiers (Jacob, 13 Oct 2026; DESIGN_LOCKS, "Sprint 2 calls"): war parties and hold the line
    /// drop Pauper, trials and duels drop Mid, and a chief you kill gives his Princess piece. One slot per fighter.
    /// Run gear is lost when the run ends; materials come home.
    public enum GearTier
    {
        Pauper = 0,
        Mid = 1,
        Princess = 2
    }

    /// The one stat a piece raises.
    public enum GearStat
    {
        Damage = 0,
        Health = 1,
        Hit = 2,
        Avoid = 3
    }

    /// A Princess piece's own effect, from the chief who wore it.
    public enum GearSpecial
    {
        None = 0,

        /// The Bloodridge chief's: the wearer's kills break more of their nerve.
        KillsBreakNerve = 1,

        /// The Skullpeak chief's: big hits (an Eclipse) on the wearer are halved.
        BigHitGuard = 2,

        /// The cave chief's (18 Oct 2026): the wearer's blows leave bleeding wounds (Battle.BleedRules).
        Bleeding = 3,

        /// The sea chief's (18 Oct 2026): the wearer hits the wounded harder.
        Predator = 4
    }

    /// One piece of run gear. Since 20 Oct 2026 a won fight's pieces and an event's are class pieces (Jacob: "Gear
    /// becomes class pieces that bend that class's own spell"): only that class can wear one, and its bend changes that
    /// class's spell in a fight with class spells (Battle.ClassBend). A piece from before, and a chief's own Princess
    /// piece, fit anyone and bend nothing. From campaign rules 15 a Butcher combo taken on the road is a piece too (Jacob,
    /// 29 Oct 2026; DESIGN_LOCKS: "A combo takes that fighter's one gear slot and keeps its pact"): its class's, with the
    /// combo as its bend and no stat.
    public sealed class CampaignGear
    {
        /// The ClassIndex of a piece anyone can wear.
        public const int AnyClass = -1;

        internal CampaignGear(string id, string tribeId, GearTier tier, GearStat stat, GearSpecial special, string name)
            : this(id, tribeId, tier, stat, special, name, AnyClass, ClassBend.None)
        {
        }

        internal CampaignGear(string id, string tribeId, GearTier tier, GearStat stat, GearSpecial special, string name,
            int classIndex, ClassBend bend)
        {
            Id = id ?? string.Empty;
            TribeId = tribeId ?? string.Empty;
            Tier = tier;
            Stat = stat;
            Special = special;
            Name = name ?? string.Empty;
            ClassIndex = classIndex >= 0 && classIndex < SliceClasses.Count ? classIndex : AnyClass;
            Bend = ClassIndex == AnyClass ? ClassBend.None : bend;
        }

        public string Id { get; }

        /// The tribe it was taken from.
        public string TribeId { get; }
        public GearTier Tier { get; }
        public GearStat Stat { get; }
        public GearSpecial Special { get; }
        public string Name { get; }

        /// The class that can wear it, as a UnitClass number, or AnyClass.
        public int ClassIndex { get; }

        /// What it does to its class's spell, or None.
        public ClassBend Bend { get; }

        /// A combo's rank (campaign rules 18, DESIGN_LOCKS "The Butcher's theme"): 1, and one more each time the Butcher's
        /// offer of it is taken again, up to MarkRules.MaxStacks. It lays that many of its mark's stacks at once. Only a
        /// combo goes above 1.
        public int Rank { get; internal set; } = 1;

        /// True when this class can wear it.
        public bool Fits(UnitClass unitClass)
        {
            return ClassIndex == AnyClass || ClassIndex == (int)unitClass;
        }

        /// How much it raises its stat: percent for damage and health, points for Hit and Avoid. A Butcher combo raises
        /// nothing: its move and its pact are the trade (29 Oct 2026).
        public int Bonus => ClassSpells.IsCombo(Bend) ? 0 : CampaignGearRules.Bonus(Tier);

        /// One line for a card, as x + y = z (Jacob, 31 Oct 2026; HANDOFF_LOG 12817): "Mid: +10% damage", then a class piece's
        /// class and bend, or a Princess piece's special. A combo: "Black Hole Round (Marksman): Aimed Shot = ...".
        public string Describe()
        {
            if (ClassSpells.IsCombo(Bend))
                return ClassSpells.Name(Bend) + " (" + SliceClasses.Name((UnitClass)ClassIndex) + "): " + ClassSpells.Describe(Bend);
            string line = Tier + ": " + CampaignGearRules.StatText(Stat, Bonus);
            if (ClassIndex != AnyClass)
            {
                line += ", " + SliceClasses.Name((UnitClass)ClassIndex) + " only";
                if (Bend != ClassBend.None) line += " | " + ClassSpells.Name(Bend) + ": " + ClassSpells.Describe(Bend);
            }
            string special = CampaignGearRules.SpecialText(Special);
            return special.Length == 0 ? line : line + " | " + special;
        }
    }

    /// The numbers and names behind run gear. Starting values (Jacob's rule 8, 12 Oct 2026: build first, tune
    /// later). The names are placeholders.
    public static class CampaignGearRules
    {
        public const int PauperBonus = 5;
        public const int MidBonus = 10;
        public const int PrincessBonus = 15;

        /// How many pieces a won fight offers to strip one of (Jacob, 20 Oct 2026).
        public const int SpoilsOffered = 3;

        /// Nerve a kill by the wearer of the Bloodridge chief's piece takes from the victim's side, on top of the
        /// usual cost. Out of NerveRules.Full, so 100 is a tenth.
        public const int KillNerve = 100;

        /// Percent the Skullpeak chief's piece takes off a big hit on its wearer.
        public const int BigHitGuardPercent = 50;

        /// Health a second the cave chief's piece makes the wearer's victims bleed, and how much harder the sea chief's
        /// makes the wearer hit anyone below half health, in percent (18 Oct 2026). Starting values.
        public const int BleedingPercent = 2;
        public const int PredatorPercent = 25;

        public static int Bonus(GearTier tier)
        {
            switch (tier)
            {
                case GearTier.Princess: return PrincessBonus;
                case GearTier.Mid: return MidBonus;
                default: return PauperBonus;
            }
        }

        /// The tier a won fight drops: war parties and hold the line Pauper, trials and duels Mid, chiefs Princess.
        public static GearTier DropTier(EncounterKind kind)
        {
            switch (kind)
            {
                case EncounterKind.Trial:
                case EncounterKind.Duel: return GearTier.Mid;
                case EncounterKind.Chief:
                case EncounterKind.AlphaHunt: return GearTier.Princess;
                default: return GearTier.Pauper;
            }
        }

        public static string StatText(GearStat stat, int bonus)
        {
            switch (stat)
            {
                case GearStat.Damage: return "+" + bonus + "% damage";
                case GearStat.Health: return "+" + bonus + "% health";
                case GearStat.Hit: return "+" + bonus + " Hit";
                default: return "+" + bonus + " Avoid";
            }
        }

        public static string SpecialText(GearSpecial special)
        {
            switch (special)
            {
                case GearSpecial.KillsBreakNerve: return "Kill = -" + KillNerve * 100 / NerveRules.Full + "% their nerve";
                case GearSpecial.BigHitGuard: return "Big hit on you = halved";
                case GearSpecial.Bleeding: return "Blow = bleed " + BleedingPercent + "% health a second";
                case GearSpecial.Predator: return "Target under half health = +" + PredatorPercent + "% damage";
                default: return string.Empty;
            }
        }

        /// A chief's Princess piece: the stat it raises and its special.
        public static GearStat ChiefStat(string tribeId)
        {
            switch (tribeId)
            {
                case CampaignTribes.SkullpeakId: return GearStat.Health;
                case CampaignTribes.ClarthId: return GearStat.Avoid;
                case CampaignTribes.StoneveilId: return GearStat.Hit;
                default: return GearStat.Damage;
            }
        }

        public static GearSpecial ChiefSpecial(string tribeId)
        {
            switch (tribeId)
            {
                case CampaignTribes.BloodridgeId: return GearSpecial.KillsBreakNerve;
                case CampaignTribes.SkullpeakId: return GearSpecial.BigHitGuard;
                case CampaignTribes.StoneveilId: return GearSpecial.Bleeding;
                case CampaignTribes.ThundercoastId: return GearSpecial.Predator;
                default: return GearSpecial.None;
            }
        }

        // A piece's name: the tribe and a word for its stat. A Princess piece is named for the chief who wore it.
        internal static string Name(CampaignTribe tribe, GearStat stat, GearTier tier, string chiefName)
        {
            string noun = Noun(stat);
            if (tier == GearTier.Princess && !string.IsNullOrEmpty(chiefName)) return chiefName + "'s " + noun;
            return (tribe == null ? "Tribe" : tribe.Name) + " " + noun;
        }

        // A class piece's name: the tribe, its bend and a word for its stat, as "Bloodridge Bastion mail". Placeholders.
        internal static string ClassName(CampaignTribe tribe, GearStat stat, ClassBend bend)
        {
            return (tribe == null ? "Tribe" : tribe.Name) + " " + ClassSpells.Name(bend) + " " + Noun(stat);
        }

        private static string Noun(GearStat stat)
        {
            switch (stat)
            {
                case GearStat.Damage: return "blade";
                case GearStat.Health: return "mail";
                case GearStat.Hit: return "eye charm";
                default: return "cloak";
            }
        }
    }

    /// One quirk per fighter (Jacob, 13 Oct 2026; DESIGN_LOCKS, "Sprint 2 calls"). Starting values.
    public enum Quirk
    {
        None = 0,

        /// More damage on a tribe that captured one of yours this run.
        Grudge = 1,
        Brute = 2,
        Tough = 3,
        SharpEye = 4,
        Quick = 5,

        /// Far likelier to rise than panic when his nerve snaps.
        Steady = 6
    }

    public static class Quirks
    {
        public const int GrudgePercent = 25;
        public const int BrutePercent = 15;
        public const int ToughPercent = 15;
        public const int SharpEyeHit = 10;
        public const int QuickAvoid = 10;

        /// Points added to the chance a Steady fighter rises rather than panics: FightRules.BaseRisePercent plus
        /// this.
        public const int SteadyRise = 40;

        private static readonly Quirk[] Pool = { Quirk.Grudge, Quirk.Brute, Quirk.Tough, Quirk.SharpEye, Quirk.Quick, Quirk.Steady };

        /// A fighter's quirk: the same every time for this campaign and this fighter. Anyone can roll Grudge, the
        /// Clarth too, since it is about one of yours taken (13 Oct 2026).
        public static Quirk For(ulong campaignSeed, string fighterId)
        {
            return Pool[(int)(Hash(campaignSeed, fighterId ?? string.Empty) % (ulong)Pool.Length)];
        }

        public static string Name(Quirk quirk)
        {
            switch (quirk)
            {
                case Quirk.Grudge: return "Grudge";
                case Quirk.Brute: return "Brute";
                case Quirk.Tough: return "Tough";
                case Quirk.SharpEye: return "Sharp eye";
                case Quirk.Quick: return "Quick";
                case Quirk.Steady: return "Steady";
                default: return string.Empty;
            }
        }

        /// One line for a fighter's card.
        public static string Describe(Quirk quirk)
        {
            switch (quirk)
            {
                case Quirk.Grudge: return "Tribe that took one of yours = +" + GrudgePercent + "% damage";
                case Quirk.Brute: return "+" + BrutePercent + "% damage";
                case Quirk.Tough: return "+" + ToughPercent + "% health";
                case Quirk.SharpEye: return "+" + SharpEyeHit + " Hit";
                case Quirk.Quick: return "+" + QuickAvoid + " Avoid";
                case Quirk.Steady: return "Nerve snaps = far likelier to rise";
                default: return string.Empty;
            }
        }

        // FNV-1a over the seed's bytes and the id's characters: the same on every machine.
        private static ulong Hash(ulong seed, string id)
        {
            ulong h = 14695981039346656037UL;
            for (int i = 0; i < 8; i++)
            {
                h ^= (seed >> (8 * i)) & 0xFF;
                h *= 1099511628211UL;
            }
            for (int i = 0; i < id.Length; i++)
            {
                h ^= id[i];
                h *= 1099511628211UL;
            }
            return h;
        }
    }

    /// Two or three fighters of one tribe in the squad (Jacob, 13 Oct 2026; DESIGN_LOCKS, "Sprint 2 calls"): at
    /// two each of them gets the tribe's stat, at three your side also fights to one of the tribe's doctrines
    /// (TribeDoctrines.ApplySet). Starting values.
    public static class TribeSets
    {
        public const int StatAt = 2;
        public const int DoctrineAt = 3;
        public const int ClarthAvoid = 8;
        public const int BloodridgeDamagePercent = 10;
        public const int SkullpeakHealthPercent = 10;

        /// The two new tribes' sets (18 Oct 2026), starting values: two Stoneveil give +8 Hit each, three hunt the most
        /// hurt; two Thundercoast give +8 Avoid each, three flood the fight for your side.
        public const int StoneveilHit = 8;
        public const int ThundercoastAvoid = 8;

        /// One line for the squad page.
        public static string Describe(string tribeId)
        {
            switch (tribeId)
            {
                case CampaignTribes.ClarthId:
                    return StatAt + " Clarth = +" + ClarthAvoid + " Avoid each | " + DoctrineAt + " = ranged hit their back row";
                case CampaignTribes.BloodridgeId:
                    return StatAt + " Bloodridge = +" + BloodridgeDamagePercent + "% damage each | " + DoctrineAt + " = one falls, the rest hit harder";
                case CampaignTribes.SkullpeakId:
                    return StatAt + " Skullpeak = +" + SkullpeakHealthPercent + "% health each | " + DoctrineAt + " = hunt their most hurt";
                case CampaignTribes.StoneveilId:
                    return StatAt + " Stoneveil = +" + StoneveilHit + " Hit each | " + DoctrineAt + " = hunt their most hurt";
                case CampaignTribes.ThundercoastId:
                    return StatAt + " Thundercoast = +" + ThundercoastAvoid + " Avoid each | " + DoctrineAt + " = your side floods the fight";
                default: return string.Empty;
            }
        }
    }
}
