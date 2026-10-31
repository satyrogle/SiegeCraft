using System;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// One trait per tribe, shown on its fighters' cards (Jacob's playtest, 6 Oct 2026: every
    /// fighter showed the same power and health). Converts keep their tribe's trait, so who you
    /// recruit changes how your squad fights. Claude's proposal from the playtest, not a lock;
    /// the numbers are placeholders.
    ///   Hunters      the Clarth hunt sand sharks: they hit anyone below half health harder.
    ///   Dread        the Bloodridge raiders are truly scary: their hits drain more courage.
    ///   Steel bones  Skullpeak grew steel into their bodies: they take less from every hit.
    ///   Bloodletters the cave tribe (Stoneveil, 18 Oct 2026): their blows leave wounds that bleed.
    ///   Deepborn     the sea tribe (Thundercoast, 18 Oct 2026): machine eyes, so they hit more often.
    public enum TribeTrait
    {
        None = 0,
        Hunters = 1,
        Dread = 2,
        SteelBones = 3,
        Bloodletters = 4,
        Deepborn = 5
    }

    public static class TribeTraits
    {
        public const int HuntersPercent = 25;
        public const int DreadPercent = 25;
        public const int SteelBonesPercent = 15;

        /// Aim (13 Oct 2026, only in fights with FightRules.Misses): Skullpeak are precise, the Clarth
        /// are hard to hit, the Bloodridge swing wild. Points on Hit and on Avoid; starting values.
        public const int SteelBonesHitBonus = 8;
        public const int HuntersAvoidBonus = 6;
        public const int DreadHitPenalty = 6;

        /// The new tribes' traits (18 Oct 2026), starting values: health a second a Bloodletter's victim bleeds, and the
        /// Deepborn's Hit.
        public const int BloodlettersBleedPercent = 2;
        public const int DeepbornHitBonus = 8;

        /// The trait of a tribe by its id. The raider chief's splinter crew are raiders too.
        public static TribeTrait For(string tribeId)
        {
            switch (tribeId)
            {
                case SliceRun.ClarthId: return TribeTrait.Hunters;
                case SliceRun.RaidersId:
                case SliceRun.SplinterId: return TribeTrait.Dread;
                case SliceRun.SkullpeakId: return TribeTrait.SteelBones;
                case CampaignTribes.StoneveilId: return TribeTrait.Bloodletters;
                case CampaignTribes.ThundercoastId: return TribeTrait.Deepborn;
                default: return TribeTrait.None;
            }
        }

        public static string Name(TribeTrait trait)
        {
            switch (trait)
            {
                case TribeTrait.Hunters: return "Hunters";
                case TribeTrait.Dread: return "Dread";
                case TribeTrait.SteelBones: return "Steel bones";
                case TribeTrait.Bloodletters: return "Bloodletters";
                case TribeTrait.Deepborn: return "Deepborn";
                default: return string.Empty;
            }
        }

        /// One line for a fighter's card.
        public static string Describe(TribeTrait trait)
        {
            switch (trait)
            {
                case TribeTrait.Hunters: return "Hit " + HuntersPercent + "% harder on anyone below half health.";
                case TribeTrait.Dread: return "Their hits drain " + DreadPercent + "% more courage.";
                case TribeTrait.SteelBones: return "Take " + SteelBonesPercent + "% less from every hit.";
                case TribeTrait.Bloodletters: return "Their blows leave wounds that bleed " + BloodlettersBleedPercent + "% of health a second.";
                case TribeTrait.Deepborn: return "+" + DeepbornHitBonus + " Hit.";
                default: return string.Empty;
            }
        }

        /// What a tribe's trait adds to its fighters in battle (UnitSpec): fear, the bonus on the
        /// wounded, and armour, each in percent.
        public static int FearPercent(string tribeId)
        {
            return For(tribeId) == TribeTrait.Dread ? DreadPercent : 0;
        }

        public static int WoundedBonusPercent(string tribeId)
        {
            return For(tribeId) == TribeTrait.Hunters ? HuntersPercent : 0;
        }

        public static int ArmorPercent(string tribeId)
        {
            return For(tribeId) == TribeTrait.SteelBones ? SteelBonesPercent : 0;
        }

        public static int HitBonus(string tribeId)
        {
            switch (For(tribeId))
            {
                case TribeTrait.SteelBones: return SteelBonesHitBonus;
                case TribeTrait.Dread: return -DreadHitPenalty;
                case TribeTrait.Deepborn: return DeepbornHitBonus;
                default: return 0;
            }
        }

        public static int AvoidBonus(string tribeId)
        {
            return For(tribeId) == TribeTrait.Hunters ? HuntersAvoidBonus : 0;
        }

        /// Health a second this tribe's blows make their victims bleed (Battle.BleedRules), in percent.
        public static int BleedPercent(string tribeId)
        {
            return For(tribeId) == TribeTrait.Bloodletters ? BloodlettersBleedPercent : 0;
        }

        /// A fighter spec with its tribe's trait added on top of the fear it already has, and its tribe's aim.
        internal static UnitSpec Spec(string id, UnitClass unitClass, Row row, UnitRole role, int powerPercent,
            int healthPercent, int fearPercent, string tribeId)
        {
            return Spec(id, unitClass, row, role, powerPercent, healthPercent, fearPercent, tribeId, 0, 0, 0, 0, 0);
        }

        /// The same, with a fighter's own extras on top of the tribe's (Sprint 2: quirk, gear and set, see
        /// CampaignGear): Hit and Avoid points, the rise bonus, nerve per kill and the big hit guard, and since 18 Oct 2026
        /// the chief pieces' bonus on the wounded and bleeding.
        internal static UnitSpec Spec(string id, UnitClass unitClass, Row row, UnitRole role, int powerPercent,
            int healthPercent, int fearPercent, string tribeId, int hitBonus, int avoidBonus, int riseBonus, int killNerve,
            int bigHitArmorPercent, int woundedBonusExtra = 0, int bleedPercentExtra = 0)
        {
            return new UnitSpec(id, unitClass, row, role, powerPercent, healthPercent,
                fearPercent + FearPercent(tribeId), WoundedBonusPercent(tribeId) + woundedBonusExtra, ArmorPercent(tribeId),
                HitBonus(tribeId) + hitBonus, AvoidBonus(tribeId) + avoidBonus, riseBonus, killNerve, bigHitArmorPercent,
                Math.Max(BleedPercent(tribeId), bleedPercentExtra));
        }
    }
}
