using System;
using System.Collections.Generic;
using System.Globalization;

namespace KindredSiege.Sim.Battle
{
    /// The floor under the three Pals (Jacob, 31 Oct 2026: "Go, sprint it"; DESIGN_LOCKS, "The floor under the three Pals";
    /// FightRules.Floor, campaign rules 23), the fight's half: a fighter's second piece (UnitSpec.Bend2, read through
    /// HasBend in BattleSimulation.Class.cs), the faster hook on a fighter's clocks (UnitSpec.ClockPercent), the cheaper hook on
    /// a side's order costs (PalKit.OrderCostPercent), and the Shade's three tricks built so far, each route's first form:
    /// Sanctuary's shield, Backstab and Fear (ShadeRules). From campaign rules 25 (FightRules.ShadeForms; Jacob in chat, 31 Oct
    /// 2026: "go" on Claude's Shade design) their second and third forms and each route's price: Sanctuary's Relay and Last
    /// Refuge (its parries do not stagger), Backstab's Twin Knives and Shadow Strike (Fall Back waits longer), and Fear's
    /// Spreading Dread and Turncoat (Focus Fire is shorter). Every number is a starting value.
    public sealed partial class BattleSimulation
    {
        private readonly bool _floorOn;
        private readonly bool _shadeOn;

        /// Whether this fight plays the floor under the three Pals: campaign rules 23.
        public bool FloorOn => _floorOn;

        /// Whether this fight plays the Shade's forms: campaign rules 25.
        public bool ShadeFormsOn => _shadeOn;

        /// The ticks Sanctuary's field still runs on this side, 0 when it is down (rules 25).
        public int FieldTicksLeft(int side) => _shadeOn && side >= 0 && side <= 1 ? _sides[side].FieldTicks : 0;

        /// What Relay's pool can still soak on this side (rules 25).
        public int RelayLeft(int side) => _shadeOn && side >= 0 && side <= 1 ? _sides[side].RelaySize : 0;

        /// Whether this side spent its Last Refuge this fight (rules 25).
        public bool RefugeSpent(int side) => _shadeOn && side >= 0 && side <= 1 && _sides[side].RefugeUsed;

        /// The ticks until this side can Fall Back again, under Backstab's price (rules 25).
        public int FallBackLockLeft(int side) => _shadeOn && side >= 0 && side <= 1 ? _sides[side].FallBackLockTicks : 0;

        /// How many of his next hits still land double from behind (rules 25; before it, 1 while his Backstab is on).
        public int BackstabHitsLeft(BattleUnit unit)
        {
            if (unit == null || !unit.Statuses[StatusKind.Backstab].On) return 0;
            return _shadeOn ? Math.Max(1, unit.Statuses[StatusKind.Backstab].Store) : 1;
        }

        // A route's form on this side: below rules 25 only first forms play, whatever the kit holds.
        private int FormOf(int side, ShadeRoute route)
        {
            int form = KitOf(side).FormOf(route);
            return form > 0 && !_shadeOn ? 1 : form;
        }

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
            long given = 0;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive || u.Side != side || u.Benched) continue;
                int size = Math.Max(1, u.MaxHp * ShadeRules.ShieldPercent / 100);
                u.Statuses.Apply(StatusKind.Shield, ShadeRules.ShieldTicks, size);
                shielded++;
                given += size;
            }
            if (shielded > 0) Emit(new BattleEvent(_tick, BattleEventKind.Trick, side, string.Empty, -1, string.Empty, -1, shielded, ShadeRules.SanctuaryDetail));
            // From rules 25 the shields are one field for their length, and from Relay it also makes a pool of a share of them.
            if (!_shadeOn || shielded == 0) return;
            SideState s = _sides[side];
            s.FieldTicks = ShadeRules.ShieldTicks;
            s.RelaySize = FormOf(side, ShadeRoute.Sanctuary) >= 2 ? (int)Math.Max(1, given * ShadeRules.RelayPercent / 100) : 0;
        }

        // Sanctuary's price (rules 25): while it is equipped this side's parries do not stagger.
        private bool SanctuaryPriced(int side)
        {
            return _shadeOn && side >= 0 && side <= 1 && KitOf(side).Has(ShadeRoute.Sanctuary);
        }

        // Relay (rules 25): a hit broke his shield, and the field's pool soaks what it can of the rest. What it soaked.
        private int Relayed(BattleUnit victim, int rest)
        {
            SideState s = _sides[victim.Side];
            if (rest <= 0 || s.RelaySize <= 0 || s.FieldTicks <= 0) return 0;
            int soaked = Math.Min(s.RelaySize, rest);
            s.RelaySize -= soaked;
            Emit(new BattleEvent(_tick, BattleEventKind.Shield, victim.Side, victim.Id, (int)victim.Class, victim.Id, (int)victim.Class, soaked,
                ShadeRules.RelayDetail));
            return soaked;
        }

        // Last Refuge (rules 25): once a fight, a hit that would kill one of the side while its field is up leaves him at 1
        // health, and the field shatters, every shield of it with it. What lands of the hit.
        private int LastRefuge(BattleUnit victim, int amount)
        {
            if (amount < victim.Hp) return amount;
            int side = victim.Side;
            if (side < 0 || side > 1) return amount;
            SideState s = _sides[side];
            if (s.FieldTicks <= 0 || s.RefugeUsed || FormOf(side, ShadeRoute.Sanctuary) < 3) return amount;
            s.RefugeUsed = true;
            s.FieldTicks = 0;
            s.RelaySize = 0;
            for (int i = 0; i < _units.Count; i++) if (_units[i].Side == side) _units[i].Statuses.Clear(StatusKind.Shield);
            int lands = Math.Max(0, victim.Hp - ShadeRules.RefugeHealth);
            Emit(new BattleEvent(_tick, BattleEventKind.Trick, side, string.Empty, -1, victim.Id, (int)victim.Class, amount - lands,
                ShadeRules.RefugeDetail));
            return lands;
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
            if (shield.Size <= 0)
            {
                victim.Statuses.Clear(StatusKind.Shield);
                // Relay (rules 25): the field's pool soaks what is left.
                if (_shadeOn) soaked += Relayed(victim, amount - soaked);
            }
            return amount - soaked;
        }

        // ------------------------------------------------------------ Backstab (Fall Back)

        // A Fall Back by this side: with Backstab each of its standing fighters steps out behind them, his next hit doubling.
        private void Backstab(int side)
        {
            if (!KitOf(side).Has(ShadeRoute.Backstab)) return;
            // From rules 25 his hits from behind are counted: Twin Knives gives two.
            int hits = FormOf(side, ShadeRoute.Backstab) >= 2 ? ShadeRules.TwinKnivesHits : 1;
            int stepped = 0;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive || u.Side != side || u.Benched) continue;
                u.Statuses.Apply(StatusKind.Backstab, ShadeRules.BackstabTicks);
                if (_shadeOn) u.Statuses[StatusKind.Backstab].Store = hits;
                stepped++;
            }
            if (stepped > 0) Emit(new BattleEvent(_tick, BattleEventKind.Trick, side, string.Empty, -1, string.Empty, -1, stepped, ShadeRules.BackstabDetail));
        }

        // Whether this blow is a backstab, from behind them. The blow spends it.
        private bool SpendBackstab(BattleUnit attacker, BattleUnit target)
        {
            if (!_floorOn || attacker.Side == target.Side || !attacker.Statuses[StatusKind.Backstab].On) return false;
            Status stab = attacker.Statuses[StatusKind.Backstab];
            // Twin Knives (rules 25): a hit from behind left over stays for his next blow.
            if (_shadeOn && stab.Store > 1)
            {
                stab.Store--;
                return true;
            }
            attacker.Statuses.Clear(StatusKind.Backstab);
            if (_shadeOn) stab.Store = 0;
            return true;
        }

        // Shadow Strike, Backstab's last form (rules 25): this side keeps attacking while it falls back.
        private bool ShadowStrike(int side)
        {
            return _shadeOn && side >= 0 && side <= 1 && FormOf(side, ShadeRoute.Backstab) >= 3;
        }

        // Fear's price (rules 25): with Fear equipped Focus Fire runs a quarter shorter.
        private int FearPriced(int side, int ticks)
        {
            if (!_shadeOn || side < 0 || side > 1 || !KitOf(side).Has(ShadeRoute.Fear)) return ticks;
            return Math.Max(1, ticks * ShadeRules.FearFocusPercent / 100);
        }

        // "2.0 s": what a rejection shows for a clock, rounded up to a tenth.
        private static string SecondsLeft(int ticks)
        {
            int tenths = (ticks * 10 + SimClock.TicksPerSecond - 1) / SimClock.TicksPerSecond;
            return (tenths / 10).ToString(CultureInfo.InvariantCulture) + "." + (tenths % 10).ToString(CultureInfo.InvariantCulture) + " s";
        }

        // ------------------------------------------------------------ Fear (Focus Fire)

        // An enemy fell to this side, killed or broken and fleeing (Flee): with Fear, if he was under its Focus Fire, the
        // enemies beside him lose nerve and the one of them left with the least panics.
        private void Fear(int side, BattleUnit fallen)
        {
            if (!KitOf(side).Has(ShadeRoute.Fear)) return;
            SideState mine = _sides[side];
            if (mine.FocusTicks <= 0 || !string.Equals(mine.FocusTargetId, fallen.Id, StringComparison.Ordinal)) return;
            int form = FormOf(side, ShadeRoute.Fear);
            int spreads = form >= 2 ? ShadeRules.DreadSpreads : 0;
            FearAround(side, fallen, form, ShadeRules.FearDetail, ref spreads);
        }

        // Fear around one who fell: those beside him lose nerve and the one of them left with the least panics. From rules 25,
        // Spreading Dread: each of them it breaks (he flees or makes his last stand) sets it off again around him, while spreads
        // last; and Turncoat: the one it panics hits the one beside him once.
        private void FearAround(int side, BattleUnit center, int form, string detail, ref int spreads)
        {
            List<BattleUnit> beside = BrandedAndBeside(center);
            List<BattleUnit> broke = form >= 2 ? new List<BattleUnit>() : null;
            BattleUnit weakest = null;
            int shaken = 0;
            for (int k = 1; k < beside.Count && !_finished; k++)
            {
                BattleUnit u = beside[k];
                if (!u.Alive || u.Side == side) continue;
                shaken++;
                bool snapped = u.NerveSnapped;
                UnitDrain(u, ShadeRules.FearNerve, side);
                if (broke != null && !snapped && u.NerveSnapped) broke.Add(u);
                if (u.Alive && (weakest == null || u.UnitNerve < weakest.UnitNerve)) weakest = u;
            }
            Emit(new BattleEvent(_tick, BattleEventKind.Trick, side, string.Empty, -1, center.Id, (int)center.Class, shaken, detail));
            if (!_finished && weakest != null && weakest.Alive)
            {
                weakest.Statuses.Apply(StatusKind.Panic, ShadeRules.FearPanicTicks);
                if (form >= 3) Turncoat(side, weakest);
            }
            for (int i = 0; broke != null && i < broke.Count && spreads > 0 && !_finished; i++)
            {
                spreads--;
                FearAround(side, broke[i], form, ShadeRules.DreadDetail, ref spreads);
            }
        }

        // Turncoat (rules 25): the one Fear panicked hits the first of his own side beside him once, for his power. The blow is
        // the trick's, so it counts for the side whose Fear it was.
        private void Turncoat(int side, BattleUnit turned)
        {
            List<BattleUnit> beside = BrandedAndBeside(turned);
            BattleUnit victim = null;
            for (int k = 1; k < beside.Count && victim == null; k++)
                if (beside[k].Alive && beside[k].Side == turned.Side) victim = beside[k];
            if (victim == null) return;
            int power = Math.Max(1, turned.Power);
            Emit(new BattleEvent(_tick, BattleEventKind.Trick, side, turned.Id, (int)turned.Class, victim.Id, (int)victim.Class, power,
                ShadeRules.TurncoatDetail));
            ApplyDamage(turned, side, victim, power, ShadeRules.TurncoatDetail);
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

        // From rules 25 the field and Backstab's lock run out, and a backstab that ran out keeps no hits.
        private void CountDownShade()
        {
            for (int s = 0; s < 2; s++)
            {
                SideState state = _sides[s];
                if (state.FieldTicks > 0 && --state.FieldTicks == 0) state.RelaySize = 0;
                if (state.FallBackLockTicks > 0) state.FallBackLockTicks--;
            }
            for (int i = 0; i < _units.Count; i++)
            {
                Status stab = _units[i].Statuses[StatusKind.Backstab];
                if (!stab.On && stab.Store != 0) stab.Store = 0;
            }
        }

        private void AppendShadeSide(SnapshotBuilder b, SideState state)
        {
            b.Field("field", state.FieldTicks);
            b.Field("relay", state.RelaySize);
            b.Field("refugeUsed", state.RefugeUsed);
            b.Field("fallBackLock", state.FallBackLockTicks);
        }
    }
}
