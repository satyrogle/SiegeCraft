using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        Image _heavyTargetBack;
        PxText _heavyTargetLabel;
        RawImage _heavyParryWord, _heavyRiposteWord;
        string _heavyMomentText = "";
        float _heavyMomentUntil = -100;

        void EnsureHeavyReadouts()
        {
            _heavyTargetBack = Panel(_readoutLayer, 0, 0, 1, 1, BarBack);
            _heavyTargetLabel = PxLabel(_readoutLayer, 0, 0, 1);
            var parry = _logoFont.Render("PARRIED!", PxGold, PxOrange, 1, true);
            var riposte = _logoFont.Render("RIPOSTE!", PxGold, PxOrange, 1, true);
            _screenTextures.Add(parry); _screenTextures.Add(riposte);
            _heavyParryWord = PxImage(_readoutLayer,"Parry moment",parry,((int)W/Px-parry.width)/2,StageY+39);
            _heavyRiposteWord = PxImage(_readoutLayer,"Riposte moment",riposte,((int)W/Px-riposte.width)/2,StageY+39);
            _heavyParryWord.enabled = _heavyRiposteWord.enabled = false;
            _heavyTargetBack.enabled = false;
            _heavyMomentUntil = -100;
        }

        bool HeavyBanner(BattleSimulation sim, out string first, out string second, out Color32 colour)
        {
            first = second = ""; colour = PxYellow;
            int left = sim.HeavyTicks(1);
            if (left <= 0) return false;
            string actor = sim.HeavySwingerId(1);
            first = NameOf(actor) + " -> " + NameOf(sim.HeavyTargetId(1)) + ": heavy swing " + Seconds(left);
            if (!sim.HeavyParryable(1))
            {
                second = "This heavy cannot be parried. Check TACTICS for their counters.";
                colour = PxRed;
            }
            else if (_paused)
                second = "PAUSED: resume before timing Hold. A paused Hold cannot parry.";
            else if (_playback != null || _pilotOn)
                second = _playback != null ? "REPLAY: recorded defence." : "AUTO: timed defence.";
            else if (!sim.CanUse(0, Directive.Hold, out string why))
                second = "Hold unavailable: " + why;
            else if (sim.HoldRefreshTicksLeft(0) > 0)
                second = HoldRecoveryCue(sim);
            else if (left <= FightRules.PerfectHoldTicks && (!sim.ParryCenterOn || LiveParryCue(sim, TimerParryThreat(sim, sim.HeavySwingerId(1), true))))
            {
                second = sim.HeavyWaited(1) ? "DELAYED SWING: Hold again now!" : "HOLD NOW: parry the heavy swing!";
                colour = PxGold;
            }
            else
                second = "Wait for the gold mark, then Hold.";
            if (!_paused && sim.WaitsOutParry(1) && !sim.HeavyWaited(1) && sim.HeavyParryable(1))
                second += " They may delay once; watch the timer.";
            return true;
        }

        void DrawHeavyTarget(BattleSimulation sim)
        {
            bool moment = !ViewOver && _visualTime < _heavyMomentUntil;
            _heavyParryWord.enabled = moment && _heavyMomentText == "PARRIED!";
            _heavyRiposteWord.enabled = moment && _heavyMomentText == "RIPOSTE!";
            var f = sim.HeavyTicks(1) > 0 && !ViewOver ? FindFigure(sim.HeavyTargetId(1)) : null;
            _heavyTargetBack.enabled = f != null && f.unit.Alive;
            if (!_heavyTargetBack.enabled) { SetPx(_heavyTargetLabel, "", PxRed); return; }
            int x = Mathf.RoundToInt(f.feet.x / Px), y = Mathf.RoundToInt(f.feet.y / Px) + (sim.ClassSpellsOn ? 50 : 20);
            Place(_heavyTargetBack, x - 26, y - 2, 52, 12);
            if (_heavyTargetLabel.x != x || _heavyTargetLabel.y != y)
            { _heavyTargetLabel.x = x; _heavyTargetLabel.y = y; _heavyTargetLabel.key = ""; }
            SetPx(_heavyTargetLabel, "TARGETED", PxRed);
        }

        void HeavyPresentationEvent(BattleEvent e, Figure actor)
        {
            if (e.Kind == BattleEventKind.Release && !string.IsNullOrEmpty(e.TargetId) && actor != null && actor.attack != null)
            {
                // The same impact point as ordinary blows, even when this heavy is dodged or parried.
                actor.poseHold = 0; actor.attackAt = _visualTime - actor.attackLength * .4f;
                actor.anticipated = false;
            }
            if (_seeking) return;
            if (e.Kind == BattleEventKind.Parry && e.ActorSide == 0)
            { _heavyMomentText = "PARRIED!"; _heavyMomentUntil = _visualTime + .8f; }
            if (e.Kind == BattleEventKind.Combo && e.ActorSide == 0 && e.Detail == FightRules.Riposte)
            { _heavyMomentText = "RIPOSTE!"; _heavyMomentUntil = _visualTime + .8f; }
        }

        // Drive the lead-in from remaining authoritative ticks, so pause, speed and a late swing
        // cannot make the clip strike before Release. Original sheets and ordinary attacks stay intact.
        bool HeavyAttackPose(Figure f, BattleSimulation sim, out float time)
        {
            time = 0;
            int left = sim.HeavyTicks(f.unit.Side);
            if (f.attack == null || left <= 0 || sim.HeavySwingerId(f.unit.Side) != f.unit.Id) return false;
            float lead = f.attackLength * .4f;
            time = Mathf.Clamp01((lead - left / (float)SimClock.TicksPerSecond) / f.attackLength) * f.attack.seconds;
            return true;
        }
    }
}
