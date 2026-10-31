using System.Globalization;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Eligibility comes from the sim, including its historical anti-mash timing. No input history is kept in the HUD.
        bool LiveParryCue(BattleSimulation sim, Threat threat) =>
            !ViewOver && !sim.AwaitingVerdict && !_paused && _playback == null && !_pilotOn && !ChiefCutInVisible &&
            !threat.IsNone && threat.ParryWindow && (threat.Answers & ThreatAnswer.Parry) != 0 &&
            sim.HoldRefreshTicksLeft(0) == 0 && sim.CanUse(0, Directive.Hold, out _);

        static string HoldRecoveryTime(int ticks)
        {
            int tenths = (ticks * 10 + SimClock.TicksPerSecond - 1) / SimClock.TicksPerSecond;
            return (tenths / 10f).ToString("0.0", CultureInfo.InvariantCulture) + "s";
        }

        string HoldRecoveryCue(BattleSimulation sim) => "HOLD RECOVERING / " + HoldRecoveryTime(sim.HoldRefreshTicksLeft(0));

        Threat TimerParryThreat(BattleSimulation sim, string actorId, bool heavy)
        {
            ReadClarity(sim);
            foreach (var threat in _threatReads)
                if (threat.ActorId == actorId && (heavy ? threat.Name == HeavyRules.Heavy : threat.Tier == ThreatTier.Spell))
                    return threat;
            return Threat.None;
        }
    }
}
