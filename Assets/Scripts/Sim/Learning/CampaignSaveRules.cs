using KindredSiege.Sim.Battle;
namespace KindredSiege.Sim.Learning
{
    public static partial class CampaignSave
    {
        // Explicit order; adding a tuning field requires adding it here. The coverage test enforces that.
        private static string ResumeRules(CampaignTuning t, string battleRules = ReplayEvidence.RulesIdentity)
        {
            var b = new SnapshotBuilder();
            b.Field("battleRules", battleRules);
            b.Field("Tier1Percent", t.Tier1Percent);
            b.Field("Tier2Percent", t.Tier2Percent);
            b.Field("Tier3Percent", t.Tier3Percent);
            b.Field("EnemyScaleTier1", t.EnemyScaleTier1);
            b.Field("EnemyScaleTier2", t.EnemyScaleTier2);
            b.Field("WarPartyPercent", t.WarPartyPercent);
            b.Field("HoldPercent", t.HoldPercent);
            b.Field("HoldPercentTier2", t.HoldPercentTier2);
            b.Field("DuelPercent", t.DuelPercent);
            b.Field("TrialPercent", t.TrialPercent);
            b.Field("PrecisionTrialPercent", t.PrecisionTrialPercent);
            b.Field("ChiefPercent", t.ChiefPercent);
            b.Field("HuntPercent", t.HuntPercent);
            b.Field("WarPartyCount", t.WarPartyCount);
            b.Field("HoldCount", t.HoldCount);
            b.Field("HoldTicks", t.HoldTicks);
            b.Field("FightTicks", t.FightTicks);
            b.Field("MaxSteps", t.MaxSteps);
            b.Field("PathFights", t.PathFights);
            b.Field("MaterialsPerFight", t.MaterialsPerFight);
            b.Field("MaterialsPerChief", t.MaterialsPerChief);
            b.Field("HoldForbidsFallBack", t.HoldForbidsFallBack);
            b.Field("TimeOutLoses", t.TimeOutLoses);
            b.Field("WarTaughtValue", t.WarTaughtValue);
            b.Field("WarTaughtPressOnFallBack", t.WarTaughtPressOnFallBack);
            b.Field("WarTaughtCover", t.WarTaughtCover);
            b.Field("TopUpTo", t.TopUpTo);
            b.Field("DuelChampionCasts", t.DuelChampionCasts);
            b.Field("DuelEclipseGrade", t.DuelEclipseGrade);
            b.Field("DuelEclipseTicks", t.DuelEclipseTicks);
            b.Field("TrialFighters", t.TrialFighters);
            b.Field("TrialEclipseTicks", t.TrialEclipseTicks);
            b.Field("LieutenantHealthPercent", t.LieutenantHealthPercent);
            b.Field("PrecisionTrialSquad", t.PrecisionTrialSquad);
            b.Field("PrecisionTrialEnemies", t.PrecisionTrialEnemies);
            b.Field("ChiefFighters", t.ChiefFighters);
            b.Field("ChiefPowerPercent", t.ChiefPowerPercent);
            b.Field("ChiefHealthPercent", t.ChiefHealthPercent);
            b.Field("ChiefEclipseTicks", t.ChiefEclipseTicks);
            b.Field("TutorialFighters", t.TutorialFighters);
            b.Field("TutorialPercent", t.TutorialPercent);
            b.Field("LeaderClass", (int)t.LeaderClass);
            b.Field("LeaderPercent", t.LeaderPercent);
            b.Field("UnleashNerve", t.UnleashNerve);
            b.Field("HoldDrainEveryTicks", t.HoldDrainEveryTicks);
            b.Field("SquadSize", t.SquadSize);
            for (int i = 0; i < SliceClasses.Count; i++)
            {
                var s = SliceClasses.Stats((UnitClass)i);
                b.Field("class", i).Field("hp", s.MaxHp).Field("power", s.Power).Field("cooldown", s.CooldownTicks)
                    .Field("ranged", s.Ranged).Field("row", (int)s.DefaultRow);
            }
            return StateChecksum.Compute(b.ToString());
        }
    }
}
