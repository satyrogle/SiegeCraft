using System;
using System.Collections.Generic;

namespace KindredSiege.Sim.Battle
{
    /// The floor under the three Pals (Jacob, 31 Oct 2026: "Go, sprint it"; DESIGN_LOCKS, "The floor under the three Pals";
    /// FightRules.Floor, campaign rules 23), the fight's half: a fighter's second piece (UnitSpec.Bend2, read through
    /// HasBend in BattleSimulation.Class.cs), the faster hook on a fighter's clocks (UnitSpec.ClockPercent), the cheaper hook on
    /// a side's order costs (PalKit.OrderCostPercent), and the Shade's three tricks built so far, each route's first form:
    /// Sanctuary's shield, Backstab and Fear (ShadeRules). Every number is a starting value.
    public sealed partial class BattleSimulation
    {
        private readonly bool _floorOn;

        /// Whether this fight plays the floor under the three Pals: campaign rules 23.
        public bool FloorOn => _floorOn;

        // What this side brings from the Pals' stores; nothing before rules 23.
        private PalKit KitOf(int side)
        {
            return _floorOn ? _rules[side].Pals : PalKit.None;
        }

        // The faster hook: a fighter's clocks (his spell's turn, his aim, his cooldown) pass his ClockPercent, never below a tick.
        private int Clock(BattleUnit unit, int ticks)
        {
            if (!_floorOn || unit == null) return ticks;
            int percent = SpecOf(unit).ClockPercent;
            return percent == 0 ? ticks : Math.Max(1, ticks * (100 + percent) / 100);
        }

        // The cheaper hook: a side's order costs pass its kit's OrderCostPercent.
        private int Cheaper(int side, int cost)
        {
            if (!_floorOn || cost <= 0) return cost;
            int percent = KitOf(side).OrderCostPercent;
            return percent == 0 ? cost : Math.Max(0, cost * (100 + percent) / 100);
        }

        // ------------------------------------------------------------ Sanctuary (Hold)

        // A clean parry by this side: with Sanctuary each of its standing fighters gets a shield.
        private void Sanctuary(int side)
        {
            if (!KitOf(side).Has(ShadeRoute.Sanctuary)) return;
            int shielded = 0;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive || u.Side != side || u.Benched) continue;
                u.Statuses.Apply(StatusKind.Shield, ShadeRules.ShieldTicks, Math.Max(1, u.MaxHp * ShadeRules.ShieldPercent / 100));
                shielded++;
            }
            if (shielded > 0) Emit(new BattleEvent(_tick, BattleEventKind.Trick, side, string.Empty, -1, string.Empty, -1, shielded, ShadeRules.SanctuaryDetail));
        }

        // A shield soaks what it can of a hit before the fighter's health, then breaks. What is left lands.
        private int Shielded(BattleUnit victim, int amount)
        {
            if (!_floorOn || amount <= 0) return amount;
            Status shield = victim.Statuses[StatusKind.Shield];
            if (!shield.On || shield.Size <= 0) return amount;
            int soaked = Math.Min(shield.Size, amount);
            shield.Size -= soaked;
            Emit(new BattleEvent(_tick, BattleEventKind.Shield, victim.Side, victim.Id, (int)victim.Class, victim.Id, (int)victim.Class, soaked,
                ShadeRules.SanctuaryDetail));
            if (shield.Size <= 0) victim.Statuses.Clear(StatusKind.Shield);
            return amount - soaked;
        }

        // ------------------------------------------------------------ Backstab (Fall Back)

        // A Fall Back by this side: with Backstab each of its standing fighters steps out behind them, his next hit doubling.
        private void Backstab(int side)
        {
            if (!KitOf(side).Has(ShadeRoute.Backstab)) return;
            int stepped = 0;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive || u.Side != side || u.Benched) continue;
                u.Statuses.Apply(StatusKind.Backstab, ShadeRules.BackstabTicks);
                stepped++;
            }
            if (stepped > 0) Emit(new BattleEvent(_tick, BattleEventKind.Trick, side, string.Empty, -1, string.Empty, -1, stepped, ShadeRules.BackstabDetail));
        }

        // Whether this blow is a backstab, from behind them. The blow spends it.
        private bool SpendBackstab(BattleUnit attacker, BattleUnit target)
        {
            if (!_floorOn || attacker.Side == target.Side || !attacker.Statuses[StatusKind.Backstab].On) return false;
            attacker.Statuses.Clear(StatusKind.Backstab);
            return true;
        }

        // ------------------------------------------------------------ Fear (Focus Fire)

        // An enemy fell to this side, killed or broken and fleeing (Flee): with Fear, if he was under its Focus Fire, the
        // enemies beside him lose nerve and the one of them left with the least panics.
        private void Fear(int side, BattleUnit fallen)
        {
            if (!KitOf(side).Has(ShadeRoute.Fear)) return;
            SideState mine = _sides[side];
            if (mine.FocusTicks <= 0 || !string.Equals(mine.FocusTargetId, fallen.Id, StringComparison.Ordinal)) return;
            List<BattleUnit> beside = BrandedAndBeside(fallen);
            BattleUnit weakest = null;
            int shaken = 0;
            for (int k = 1; k < beside.Count && !_finished; k++)
            {
                BattleUnit u = beside[k];
                if (!u.Alive || u.Side == side) continue;
                shaken++;
                UnitDrain(u, ShadeRules.FearNerve, side);
                if (u.Alive && (weakest == null || u.UnitNerve < weakest.UnitNerve)) weakest = u;
            }
            Emit(new BattleEvent(_tick, BattleEventKind.Trick, side, string.Empty, -1, fallen.Id, (int)fallen.Class, shaken, ShadeRules.FearDetail));
            if (!_finished && weakest != null && weakest.Alive) weakest.Statuses.Apply(StatusKind.Panic, ShadeRules.FearPanicTicks);
        }

        // ------------------------------------------------------------ the clocks

        // Shields and backstabs run out.
        private void CountDownFloor()
        {
            for (int i = 0; i < _units.Count; i++)
            {
                StatusList statuses = _units[i].Statuses;
                if (statuses.CountDown(StatusKind.Shield)) statuses.Clear(StatusKind.Shield);
                if (statuses.CountDown(StatusKind.Backstab)) statuses.Clear(StatusKind.Backstab);
            }
        }
    }
}
