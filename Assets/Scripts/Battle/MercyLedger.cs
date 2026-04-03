using UnityEngine;
using KindredSiege.Core;

namespace KindredSiege.Battle
{
    /// <summary>
    /// GDD Cross-Run Persistence: The Mercy Ledger
    /// 
    /// Tracks lifetime mercy decisions across ALL runs using PlayerPrefs.
    /// Data persists even through New Game. Displayed on the Main Menu.
    /// 
    /// Counters:
    ///   • UnitsSaved   — Total units saved via Mercy Tokens (across all campaigns)
    ///   • UnitsLetDie  — Total units the player chose to let die
    ///   • TotalTokensUsed — Lifetime Mercy Tokens spent
    /// </summary>
    public static class MercyLedger
    {
        // PlayerPrefs keys — persist across runs
        private const string KeySaved     = "KS_MercyLedger_Saved";
        private const string KeyLetDie    = "KS_MercyLedger_LetDie";
        private const string KeyTokens    = "KS_MercyLedger_TokensUsed";

        // ════════════════════════════════════════════
        // READ
        // ════════════════════════════════════════════

        public static int UnitsSaved    => PlayerPrefs.GetInt(KeySaved, 0);
        public static int UnitsLetDie   => PlayerPrefs.GetInt(KeyLetDie, 0);
        public static int TotalTokens   => PlayerPrefs.GetInt(KeyTokens, 0);

        /// <summary>Total mercy decisions made (saved + let die).</summary>
        public static int TotalDecisions => UnitsSaved + UnitsLetDie;

        /// <summary>Mercy ratio: 0.0 (heartless) to 1.0 (saint). Returns 0.5 if no decisions.</summary>
        public static float MercyRatio => TotalDecisions > 0
            ? (float)UnitsSaved / TotalDecisions
            : 0.5f;

        /// <summary>Description of player disposition based on mercy ratio.</summary>
        public static string Disposition
        {
            get
            {
                if (TotalDecisions == 0) return "Unknown";
                float r = MercyRatio;
                if (r >= 0.80f) return "Compassionate";
                if (r >= 0.60f) return "Merciful";
                if (r >= 0.40f) return "Pragmatic";
                if (r >= 0.20f) return "Ruthless";
                return "Heartless";
            }
        }

        // ════════════════════════════════════════════
        // WRITE
        // ════════════════════════════════════════════

        /// <summary>Record that a unit was saved by a Mercy Token.</summary>
        public static void RecordSaved()
        {
            PlayerPrefs.SetInt(KeySaved, UnitsSaved + 1);
            PlayerPrefs.SetInt(KeyTokens, TotalTokens + 1);
            PlayerPrefs.Save();
            Debug.Log($"[MercyLedger] Unit SAVED. Lifetime: {UnitsSaved} saved, {UnitsLetDie} lost. ({Disposition})");
        }

        /// <summary>Record that a unit was allowed to die.</summary>
        public static void RecordLetDie()
        {
            PlayerPrefs.SetInt(KeyLetDie, UnitsLetDie + 1);
            PlayerPrefs.Save();
            Debug.Log($"[MercyLedger] Unit LET DIE. Lifetime: {UnitsSaved} saved, {UnitsLetDie} lost. ({Disposition})");
        }

        /// <summary>Reset the ledger (for debug / testing only).</summary>
        public static void ResetLedger()
        {
            PlayerPrefs.DeleteKey(KeySaved);
            PlayerPrefs.DeleteKey(KeyLetDie);
            PlayerPrefs.DeleteKey(KeyTokens);
            PlayerPrefs.Save();
            Debug.Log("[MercyLedger] Ledger reset.");
        }
    }
}
