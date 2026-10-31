using System.Collections.Generic;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        RectTransform _tribeScreen;
        readonly PxText[] _bleedReadouts = new PxText[2], _floodReadouts = new PxText[2];
        readonly Dictionary<string, PxText> _bleedMarks = new Dictionary<string, PxText>();
        bool _hasBleeding;
        readonly int[] _bleedingCounts = new int[2];

        static int ResultDeaths(EncounterResult r) => r.Fallen.Count + (r.Slain == null ? 0 : 1);

        // This is the loss policy of this encounter, including the sim's current ambush policy.
        string LossWarning(Encounter e) => LivesRun ? "Lose: one life. Squad and gear return; nobody captured." : e.Tribe.NoPrisoners
            ? "Lose: they kill your most hurt survivor. No prisoners."
            : "Lose: they capture your most hurt survivor.";

        void AddTribeBrief(PxLines lines, Encounter e)
        {
            if (HunterFight(e.Kind)) return; // Hunter combat has its own rules, even on these roads.
            if (e.Tribe.Id == CampaignTribes.StoneveilId)
                lines.Add("Their cuts cause bleeding. Wound timers show who is losing health.", PxRed);
            if (e.Tribe.Floods)
                lines.Add("Flood rises over time. Sides that cannot flood slow down and lose nerve; the flooding side's blows grow stronger.", PxBlue);
        }

        void DrawTribeMechanics(BattleSimulation sim)
        {
            if (_tribeScreen != _screen)
            {
                _tribeScreen = _screen;
                _bleedMarks.Clear();
                _hasBleeding = false;
                var setup = _playback != null ? _playback.Fight.Setup : _fight.Setup;
                for (int side = 0; side < 2; side++)
                {
                    foreach (var unit in setup.Side(side)) if (unit.BleedPercent > 0) _hasBleeding = true;
                    _bleedReadouts[side] = PxLabel(_stage, side == 0 ? 22 : 406, 176);
                    _floodReadouts[side] = PxLabel(_stage, side == 0 ? 22 : 406, 187);
                }
                foreach (var f in _figures) _bleedMarks[f.unit.Id] = PxLabel(f.readoutRoot, 0, -8, 1);
            }
            _bleedingCounts[0] = _bleedingCounts[1] = 0;
            foreach (var f in _figures)
            {
                bool hurt = !ViewOver && f.unit.Alive && f.unit.BleedTicks > 0;
                if (hurt) _bleedingCounts[f.unit.Side]++;
                SetPx(_bleedMarks[f.unit.Id], hurt && !ChiefCutInVisible && !sim.ClassSpellsOn ? "BLEED " + Seconds(f.unit.BleedTicks) : "", PxRed);
            }
            for (int side = 0; side < 2; side++)
            {
                string who = side == 0 ? "YOU" : "THEM";
                string effect = sim.Spell(1 - side) == ChiefSpell.BloodCall ? "BLOOD CALL TARGETS" : "HEALTH DRAIN";
                SetPx(_bleedReadouts[side], !ViewOver && !ChiefCutInVisible && _hasBleeding ? who + ": " + _bleedingCounts[side] + " BLEEDING / " + effect : "", _bleedingCounts[side] > 0 ? PxRed : PxMuted);
                SetPx(_floodReadouts[side], !ViewOver && !ChiefCutInVisible ? FloodReadout(sim, side) : "", PxBlue);
            }
        }

        static string FloodReadout(BattleSimulation sim, int side)
        {
            if (!sim.Floods(0) && !sim.Floods(1)) return "";
            string head = (side == 0 ? "YOU" : "THEM") + " / FLOOD " + sim.FloodLevel + "/" + FloodRules.MaxLevel + ": ";
            if (sim.Floods(0) && sim.Floods(1)) return head + "BOTH SIDES IMMUNE";
            if (sim.FloodLevel == 0) return head + "WATER RISING";
            return head + (sim.Floods(side) ? "STRONGER BLOWS" : "SLOWED + NERVE DRAIN");
        }
    }
}
