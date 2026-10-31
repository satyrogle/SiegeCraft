using System;

namespace KindredSiege.Sim.Battle
{
    /// What the Pals' favours change about one side's orders for a fight (Jacob, 18 Oct 2026: "favours from the Pals on
    /// road nodes"). The campaign turns the run's favours into these numbers (Learning.CampaignFavours) and the fight
    /// only reads them. Every field bends a number the fight already has; 0 leaves it alone. Starting values.
    public sealed class SideFavours
    {
        public const int MaxPercent = 300;
        public const int MaxCut = 3;
        public const int MaxPoints = 10;
        public const int MaxTicks = 300;

        /// How many fields ToArray writes. ReplayEvidence keeps the count, so later fields can be added.
        public const int FieldCount = 12;

        /// The detail of the Reaction event when a dodged cast or heavy swing leaves its thrower staggering.
        public const string WrongfootDetail = "wrongfooted";

        public static readonly SideFavours None = new SideFavours();

        public SideFavours(int unleashLongerPercent = 0, int unleashDrainCut = 0, int unleashScarePercent = 0,
            int holdDrainCut = 0, int holdAvoid = 0, int holdSoakCut = 0,
            int dodgePoints = 0, int fallBackLongerPercent = 0, int wrongfootTicks = 0,
            int focusHit = 0, int focusLongerPercent = 0, int focusFallNervePercent = 0)
        {
            UnleashLongerPercent = Clamp(unleashLongerPercent, MaxPercent);
            UnleashDrainCut = Clamp(unleashDrainCut, MaxCut);
            UnleashScarePercent = Clamp(unleashScarePercent, MaxPercent);
            HoldDrainCut = Clamp(holdDrainCut, MaxCut);
            HoldAvoid = Clamp(holdAvoid, 100);
            HoldSoakCut = Clamp(holdSoakCut, BattleSimulation.HoldDamagePercent);
            DodgePoints = Clamp(dodgePoints, MaxPoints);
            FallBackLongerPercent = Clamp(fallBackLongerPercent, MaxPercent);
            WrongfootTicks = Clamp(wrongfootTicks, MaxTicks);
            FocusHit = Clamp(focusHit, 100);
            FocusLongerPercent = Clamp(focusLongerPercent, MaxPercent);
            FocusFallNervePercent = Clamp(focusFallNervePercent, MaxPercent);
        }

        /// Unleash lasts this much longer, in percent.
        public int UnleashLongerPercent { get; }

        /// Quarters of Unleash's drain on your own nerve that never come: 0 to 3.
        public int UnleashDrainCut { get; }

        /// Unleash breaks this much more of the enemy's nerve, in percent.
        public int UnleashScarePercent { get; }

        /// Quarters of Hold's drain on your own nerve that never come: 0 to 3.
        public int HoldDrainCut { get; }

        /// Avoid a Hold gives your front row on top of FightRules.HoldAvoidBonus.
        public int HoldAvoid { get; }

        /// Points off BattleSimulation.HoldDamagePercent: your holding front row takes less from blows and heavy swings.
        public int HoldSoakCut { get; }

        /// Points a cast or heavy swing you dodge with Fall Back earns, on top of FightRules.DodgePoints.
        public int DodgePoints { get; }

        /// Fall Back lasts this much longer, in percent.
        public int FallBackLongerPercent { get; }

        /// Ticks whoever threw a cast or heavy swing you dodge with Fall Back staggers for.
        public int WrongfootTicks { get; }

        /// Hit Focus Fire gives on top of FightRules.FocusHitBonus.
        public int FocusHit { get; }

        /// Focus Fire lasts this much longer, in percent.
        public int FocusLongerPercent { get; }

        /// When the one you focus falls, their side loses this much more than the usual nerve for a death, in percent.
        public int FocusFallNervePercent { get; }

        public bool IsNone
        {
            get
            {
                int[] all = ToArray();
                for (int i = 0; i < all.Length; i++) if (all[i] != 0) return false;
                return true;
            }
        }

        /// The fields in a fixed order, for ReplayEvidence.
        public int[] ToArray()
        {
            return new[]
            {
                UnleashLongerPercent, UnleashDrainCut, UnleashScarePercent, HoldDrainCut, HoldAvoid, HoldSoakCut,
                DodgePoints, FallBackLongerPercent, WrongfootTicks, FocusHit, FocusLongerPercent, FocusFallNervePercent
            };
        }

        /// Reads ToArray back. Missing fields read as 0.
        public static SideFavours FromArray(int[] v)
        {
            if (v == null || v.Length == 0) return None;
            int F(int i) => i < v.Length ? v[i] : 0;
            var f = new SideFavours(F(0), F(1), F(2), F(3), F(4), F(5), F(6), F(7), F(8), F(9), F(10), F(11));
            return f.IsNone ? None : f;
        }

        /// Written only when a field is set, so every fight without favours keeps its checksum.
        public void AppendTo(SnapshotBuilder b, string prefix)
        {
            int[] all = ToArray();
            for (int i = 0; i < all.Length; i++)
                if (all[i] != 0) b.Field(prefix + Names[i], all[i]);
        }

        private static readonly string[] Names =
        {
            "unleashLonger", "unleashDrainCut", "unleashScare", "holdDrainCut", "holdAvoid", "holdSoakCut",
            "dodgePoints", "fallBackLonger", "wrongfoot", "focusHit", "focusLonger", "focusFallNerve"
        };

        private static int Clamp(int value, int max)
        {
            return Math.Max(0, Math.Min(max, value));
        }
    }
}
