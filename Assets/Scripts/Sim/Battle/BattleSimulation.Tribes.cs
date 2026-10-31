using System;

namespace KindredSiege.Sim.Battle
{
    /// The cave tribe's bleeding wounds (Stoneveil; Jacob, 18 Oct 2026: "Cave -2"). A blow from a fighter whose blades
    /// bleed (UnitSpec.BleedPercent) leaves whoever it hit bleeding for Ticks; each second they lose that percent of their
    /// health. A fresh cut starts the clock again. The blood call (ChiefSpell.BloodCall) rips it all out at once.
    /// Starting values.
    public static class BleedRules
    {
        public const int Ticks = 5 * SimClock.TicksPerSecond;
        public const int MaxPercent = 20;

        /// The damage detail of a second of bleeding.
        public const string Detail = "bleed";
    }

    /// The sea tribe's flood (Thundercoast; Jacob, 18 Oct 2026: "Sea - 6"). A side that floods the fight (SideRules.Flood)
    /// raises the water a level every StepTicks, up to MaxLevel. For each level the other side's fighters act SlowPercent
    /// slower and lose NervePerLevel nerve a second, and the flooding side's blows land PowerPercent harder. Starting
    /// values.
    public static class FloodRules
    {
        public const int StepTicks = 10 * SimClock.TicksPerSecond;
        public const int MaxLevel = 3;
        public const int SlowPercent = 10;
        public const int NervePerLevel = 3;
        public const int PowerPercent = 10;
    }

    /// The new tribes' rules (18 Oct 2026, DESIGN_LOCKS "The two new tribes"): bleeding wounds and the blood call, and
    /// the flood.
    public sealed partial class BattleSimulation
    {
        private readonly bool _bleedOn;
        private readonly bool _floodOn;

        /// The water's level in this fight, 0 to FloodRules.MaxLevel; 0 unless a side floods it. In the Thundercoast chief's
        /// fight it follows his tide (BattleSimulation.Thundercoast.cs). Read only, for the screen.
        public int FloodLevel => _tideOn ? TideLevel() : _floodOn ? (int)Math.Min(FloodRules.MaxLevel, _tick / FloodRules.StepTicks) : 0;

        /// Whether this side floods the fight (the sea tribe's side, or yours with three of them, or the Thundercoast chief's).
        public bool Floods(int side)
        {
            return side >= 0 && side <= 1 && (_rules[side].Flood || (_tideOn && side == _tideSide));
        }

        // Under the water: the side across from one that floods.
        private bool Flooded(int side)
        {
            return _floodOn && side >= 0 && side <= 1 && Floods(1 - side) && !Floods(side);
        }

        // How long a fighter waits before it acts again: the flood slows the side under it, and low water strands the
        // Thundercoast chief's side.
        private int NextCooldown(BattleUnit unit)
        {
            // The faster hook (rules 23, BattleSimulation.Shade.cs).
            return Clock(unit, CooldownTicks(unit));
        }

        private int CooldownTicks(BattleUnit unit)
        {
            int ticks = unit.Stats.CooldownTicks;
            // A Surgeon's treatments come more slowly (ClassBend.Surgeon).
            if (_classOn && HasBend(unit, ClassBend.Surgeon)) ticks = ticks * ClassSpells.SurgeonPercent / 100;
            int level = FloodLevel;
            if (_tideOn && level == 0 && unit.Side == _tideSide) return ticks * (100 + ChiefKits.StrandedSlowPercent) / 100;
            return level > 0 && Flooded(unit.Side) ? ticks * (100 + FloodRules.SlowPercent * level) / 100 : ticks;
        }

        // A blow from the flooding side lands harder as the water rises; at low water the Thundercoast chief's side's lands
        // lighter.
        private int FloodBlow(int attackerSide, int damage)
        {
            int level = FloodLevel;
            if (_tideOn && level == 0 && attackerSide == _tideSide) return Math.Max(1, damage * (100 - ChiefKits.StrandedWeakPercent) / 100);
            return level > 0 && Flooded(1 - attackerSide) ? damage * (100 + FloodRules.PowerPercent * level) / 100 : damage;
        }

        // The same through the damage layers (BattleSimulation.Layers.cs): the rising water, or the Thundercoast chief's
        // stranded side's cut, in the kit layer.
        private void FloodHit(int attackerSide, Hit hit)
        {
            int level = FloodLevel;
            if (_tideOn && level == 0 && attackerSide == _tideSide) hit.Add(HitLayer.Kit, -ChiefKits.StrandedWeakPercent);
            else if (level > 0 && Flooded(1 - attackerSide)) hit.Add(HitLayer.Kit, FloodRules.PowerPercent * level);
        }

        // Every second under the water costs nerve; holding through the Thundercoast chief's Storm Surge, half of it.
        private void TickFlood()
        {
            int level = FloodLevel;
            if (level <= 0 || _tick % SimClock.TicksPerSecond != 0) return;
            for (int s = 0; s < 2 && !_finished; s++)
            {
                if (!Flooded(s)) continue;
                int nerve = FloodRules.NervePerLevel * level;
                if (_tideOn) nerve = SurgeNerve(s, nerve);
                DrainNerve(s, nerve, 1 - s);
            }
        }

        // A blow from blades that bleed leaves whoever it hit bleeding.
        private void Bleed(BattleUnit attacker, BattleUnit victim)
        {
            if (!_bleedOn || attacker == null || victim == null || !victim.Alive || _finished) return;
            int percent = SpecOf(attacker).BleedPercent;
            if (percent <= 0) return;
            // The Stoneveil chief's rite (BattleSimulation.Stoneveil.cs): once your Clean Hands broke it his band's blades leave
            // no wounds, and his Red Rite makes yours bleed twice as long.
            if (_riteOn && !RiteWounds(attacker.Side)) return;
            victim.Statuses.Apply(StatusKind.Bleed, _riteOn ? WoundTicks(victim) : BleedRules.Ticks, percent);
        }

        // Every bleeding fighter loses its share each second, and the wound closes when its clock runs out. The Stoneveil
        // chief's Open Veins make each second take more (BattleSimulation.Stoneveil.cs).
        private void TickBleeding()
        {
            for (int i = 0; i < _units.Count && !_finished; i++)
            {
                BattleUnit u = _units[i];
                Status bleed = u.Statuses[StatusKind.Bleed];
                if (!u.Alive || bleed.Ticks <= 0) continue;
                bleed.Ticks--;
                int percent = bleed.Size;
                if (bleed.Ticks <= 0) bleed.Size = 0;
                if (bleed.Ticks % StatusRules.Of(StatusKind.Bleed).Period != 0) continue;
                // Through the damage layers a second of bleeding is its exact share, Open Veins a kit layer on it
                // (BattleSimulation.Layers.cs).
                if (_layersOn)
                {
                    // From rules 20 a big target's share counts at most PayoffRules.PercentHealthCap (BattleSimulation.Payoffs.cs).
                    Hit second = Hit.Percent(ShareHealth(u), percent);
                    if (_riteOn) VeinsHit(u, second);
                    ApplyHit(null, 1 - u.Side, u, second, BleedRules.Detail);
                    continue;
                }
                int amount = Math.Max(1, ShareHealth(u) * percent / 100);
                if (_riteOn) amount = VeinsBleed(u, amount);
                ApplyDamage(null, 1 - u.Side, u, amount, BleedRules.Detail);
            }
            if (!_finished) CheckWipe();
        }

        // The blood call: every bleeding fighter on the other side loses this share of its health at once and stops
        // bleeding, and their nerve takes the spell's hit. Nobody bleeding, nothing happens. Returns the health it took.
        private int CallBlood(int side, BattleUnit caster, int percent)
        {
            int enemy = 1 - side;
            int hit = 0;
            int ripped = 0;
            for (int i = 0; i < _units.Count && !_finished; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive || u.Side != enemy || u.BleedTicks <= 0) continue;
                u.Statuses.Clear(StatusKind.Bleed);
                hit++;
                int before = u.Hp;
                ApplyDamage(caster, side, u, Math.Max(1, u.MaxHp * percent / 100), ChiefSpells.BloodDetail);
                ripped += before - u.Hp;
            }
            if (hit > 0 && !_finished) DrainNerve(enemy, Moves.EclipseNerveCost, side);
            if (!_finished) CheckWipe();
            return ripped;
        }
    }
}
