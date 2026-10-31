using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// How each tribe reacts under threat (Jacob, 13 Oct 2026: "repetition is a chore ... we need to find
    /// how diff tribes act differently to threat"). Set on a tribe's side at the start of each fight and
    /// never learned, so it is who they are, not what they picked up from you. Claude's starting set; Jacob
    /// can change any of it. The rules themselves are in BattleSimulation.Skill.cs.
    ///   Bloodridge  rally when one of theirs falls; their Wardens guard whoever is casting.
    ///   Skullpeak   hunt your most hurt fighter; hold formation once their nerve falls below half.
    ///   Clarth      fall back out of your Unleash; their ranged pick off your back row.
    ///   Stoneveil   hunt your most hurt fighter; their Wardens guard whoever is casting (18 Oct 2026).
    ///   Thundercoast their ranged pick off your back row; hold formation once their nerve falls below half (18 Oct 2026).
    public static class TribeDoctrines
    {
        /// How strongly a tribe holds its doctrines, 0 to 10. A starting value.
        public const int Strength = 6;

        /// Gives a tribe's side its doctrines. A tribe without any is left as it is.
        public static void Apply(string tribeId, TacticsProfile tactics)
        {
            if (tactics == null) return;
            switch (tribeId)
            {
                case CampaignTribes.BloodridgeId:
                    tactics.SetRally(Strength);
                    tactics.SetGuardCaster(Strength);
                    break;
                case CampaignTribes.SkullpeakId:
                    tactics.SetHuntWounded(Strength);
                    tactics.SetHoldFormation(Strength);
                    break;
                case CampaignTribes.ClarthId:
                    tactics.SetKite(Strength);
                    tactics.SetPickOffBack(Strength);
                    break;
                case CampaignTribes.StoneveilId:
                    tactics.SetHuntWounded(Strength);
                    tactics.SetGuardCaster(Strength);
                    break;
                case CampaignTribes.ThundercoastId:
                    tactics.SetPickOffBack(Strength);
                    tactics.SetHoldFormation(Strength);
                    break;
            }
        }

        /// Your side's share of a tribe's doctrine when three of its fighters stand in your squad (TribeSets,
        /// Jacob, 13 Oct 2026): the one that needs no order. The Clarth pick off the back row, Bloodridge rally,
        /// Skullpeak hunt the most hurt.
        public static void ApplySet(string tribeId, TacticsProfile tactics)
        {
            if (tactics == null) return;
            switch (tribeId)
            {
                case CampaignTribes.ClarthId:
                    tactics.SetPickOffBack(Strength);
                    break;
                case CampaignTribes.BloodridgeId:
                    tactics.SetRally(Strength);
                    break;
                case CampaignTribes.SkullpeakId:
                case CampaignTribes.StoneveilId:
                    tactics.SetHuntWounded(Strength);
                    break;
                // Three Thundercoast bring their flood instead, a rule of the fight, not a habit (Campaign.SquadFloods).
            }
        }

        /// One line per doctrine, for a card or a tag over the fight.
        public static string Describe(string detail)
        {
            switch (detail)
            {
                case BattleSimulation.RallyDetail: return "When one of theirs falls, the rest hit harder for a while.";
                case BattleSimulation.GuardCasterDetail: return "Their Wardens cover whoever is casting.";
                case BattleSimulation.HuntWoundedDetail: return "They go for your most hurt fighter.";
                case BattleSimulation.HoldFormationDetail: return "Once their nerve falls below half, they Hold.";
                case BattleSimulation.KiteDetail: return "When you Unleash, they Fall Back out of its way.";
                case BattleSimulation.PickOffBackDetail: return "Their ranged fighters pick off your back row.";
                default: return string.Empty;
            }
        }
    }
}
