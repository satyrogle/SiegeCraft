using System;
using System.Collections.Generic;

namespace KindredSiege.Sim.Battle
{
    /// One fighter to place in a battle. Row defaults to the class's usual row.
    public readonly struct UnitSpec
    {
        public UnitSpec(string id, UnitClass unitClass)
            : this(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow)
        {
        }

        public UnitSpec(string id, UnitClass unitClass, Row row)
        {
            Id = id ?? string.Empty;
            Class = unitClass;
            Row = row;
        }

        public string Id { get; }
        public UnitClass Class { get; }
        public Row Row { get; }
    }

    /// How a side fights: which enemy classes it goes for first, and two learned habits.
    /// Tribes learn these numbers. A fight copies them at the start and never changes them.
    public sealed class TacticsProfile
    {
        public const int MinValue = 0;
        public const int MaxValue = 10;
        public const int StartWeight = 3;

        private readonly int[] _targetWeights = new int[SliceClasses.Count];

        public TacticsProfile()
        {
            for (int i = 0; i < _targetWeights.Length; i++) _targetWeights[i] = StartWeight;
        }

        /// 0 to 10. Higher means strike this enemy class first.
        public int TargetWeight(UnitClass unitClass)
        {
            return _targetWeights[(int)unitClass];
        }

        /// 0 to 10. At 3 or more, our ranged fighters stay off whoever an enemy Warden is covering.
        /// Melee ignore it: measured on 1 Oct 2026, melee that dodge the guarded lose more fights.
        public int AvoidGuarded { get; private set; }

        /// 0 to 10. At 3 or more, melee fighters other than the Warden wait while an enemy trap
        /// is armed, and let their own Warden spring it. Not learned: measured on 1 Oct 2026, the
        /// raiders with it won 64% of fights against the Clarth instead of 98%, and hunting the
        /// Trapper instead was worse still.
        public int TrapCaution { get; private set; }

        /// 0 to 10. At 3 or more, when the enemy focuses fire on one of ours, our Warden steps
        /// in to cover that unit for as long as the focus lasts.
        public int CoverFocused { get; private set; }

        /// 0 to 10. At 3 or more, the moment the enemy uses Unleash we answer with Hold, if we
        /// have a directive point left.
        public int HoldOnUnleash { get; private set; }

        /// 0 to 10. At 3 or more, when the enemy falls back we answer with Unleash, so our
        /// ranged fighters hit harder while nobody is in melee. Needs two directive points.
        public int PressOnFallBack { get; private set; }

        /// Enemy class to focus fire on at the start of a fight, or -1 for none. Costs a point.
        /// Not learned: measured on 1 Oct 2026, opening on a fixed class made the raiders worse.
        public int FocusOpener { get; private set; } = -1;

        public void SetPressOnFallBack(int value)
        {
            PressOnFallBack = Clamp(value);
        }

        public void SetTargetWeight(UnitClass unitClass, int value)
        {
            _targetWeights[(int)unitClass] = Clamp(value);
        }

        public void SetAvoidGuarded(int value)
        {
            AvoidGuarded = Clamp(value);
        }

        public void SetTrapCaution(int value)
        {
            TrapCaution = Clamp(value);
        }

        public void SetCoverFocused(int value)
        {
            CoverFocused = Clamp(value);
        }

        public void SetHoldOnUnleash(int value)
        {
            HoldOnUnleash = Clamp(value);
        }

        /// Pass -1 to clear.
        public void SetFocusOpener(int unitClass)
        {
            FocusOpener = unitClass < 0 || unitClass >= SliceClasses.Count ? -1 : unitClass;
        }

        public TacticsProfile Clone()
        {
            var clone = new TacticsProfile();
            Array.Copy(_targetWeights, clone._targetWeights, _targetWeights.Length);
            clone.AvoidGuarded = AvoidGuarded;
            clone.TrapCaution = TrapCaution;
            clone.CoverFocused = CoverFocused;
            clone.HoldOnUnleash = HoldOnUnleash;
            clone.PressOnFallBack = PressOnFallBack;
            clone.FocusOpener = FocusOpener;
            return clone;
        }

        public void AppendTo(SnapshotBuilder b, string prefix)
        {
            for (int i = 0; i < SliceClasses.All.Length; i++)
                b.Field(prefix + "w." + SliceClasses.Name(SliceClasses.All[i]), _targetWeights[i]);
            b.Field(prefix + "avoidGuarded", AvoidGuarded);
            b.Field(prefix + "trapCaution", TrapCaution);
            b.Field(prefix + "coverFocused", CoverFocused);
            b.Field(prefix + "holdOnUnleash", HoldOnUnleash);
            b.Field(prefix + "pressOnFallBack", PressOnFallBack);
            b.Field(prefix + "focusOpener", FocusOpener);
        }

        private static int Clamp(int value)
        {
            return Math.Max(MinValue, Math.Min(MaxValue, value));
        }
    }

    /// Everything needed to start the same fight again. Tactics are copied in, so a replay
    /// uses what each side knew at the time, not what it learned afterwards.
    public sealed class BattleSetup
    {
        public const int DefaultTickLimit = 3600;
        public const int DefaultDirectivePoints = 5;

        private readonly UnitSpec[] _side0;
        private readonly UnitSpec[] _side1;
        private readonly TacticsProfile _tactics0;
        private readonly TacticsProfile _tactics1;

        public BattleSetup(
            string fightId,
            ulong seed,
            IReadOnlyList<UnitSpec> side0,
            IReadOnlyList<UnitSpec> side1,
            TacticsProfile side0Tactics,
            TacticsProfile side1Tactics,
            int tickLimit = DefaultTickLimit,
            int startingDirectivePoints = DefaultDirectivePoints)
        {
            if (side0 == null || side0.Count == 0) throw new ArgumentException("Side 0 needs at least one unit.");
            if (side1 == null || side1.Count == 0) throw new ArgumentException("Side 1 needs at least one unit.");
            if (tickLimit <= 0) throw new ArgumentOutOfRangeException(nameof(tickLimit));

            FightId = fightId ?? string.Empty;
            Seed = seed;
            TickLimit = tickLimit;
            StartingDirectivePoints = Math.Max(0, startingDirectivePoints);
            _side0 = Copy(side0);
            _side1 = Copy(side1);
            _tactics0 = side0Tactics == null ? new TacticsProfile() : side0Tactics.Clone();
            _tactics1 = side1Tactics == null ? new TacticsProfile() : side1Tactics.Clone();
        }

        public string FightId { get; }
        public ulong Seed { get; }
        public int TickLimit { get; }
        public int StartingDirectivePoints { get; }

        public IReadOnlyList<UnitSpec> Side(int side)
        {
            return side == 0 ? (IReadOnlyList<UnitSpec>)_side0 : _side1;
        }

        public int TargetWeight(int side, UnitClass unitClass)
        {
            return Tactics(side).TargetWeight(unitClass);
        }

        public int AvoidGuarded(int side)
        {
            return Tactics(side).AvoidGuarded;
        }

        public int TrapCaution(int side)
        {
            return Tactics(side).TrapCaution;
        }

        /// A copy of a side's tactics for this fight.
        public TacticsProfile CopyTactics(int side)
        {
            return Tactics(side).Clone();
        }

        private TacticsProfile Tactics(int side)
        {
            return side == 0 ? _tactics0 : _tactics1;
        }

        private static UnitSpec[] Copy(IReadOnlyList<UnitSpec> specs)
        {
            var copy = new UnitSpec[specs.Count];
            for (int i = 0; i < specs.Count; i++) copy[i] = specs[i];
            return copy;
        }
    }

    /// A fighter during a battle. Only BattleSimulation changes it.
    public sealed class BattleUnit
    {
        internal BattleUnit(string id, int side, int index, UnitClass unitClass, Row row)
        {
            Id = id;
            Side = side;
            Index = index;
            Class = unitClass;
            Row = row;
            Stats = SliceClasses.Stats(unitClass);
            MaxHp = Stats.MaxHp;
            Hp = MaxHp;
            Alive = true;
            GuardTargetId = string.Empty;
        }

        public string Id { get; }
        public int Side { get; }

        /// Position in the battle's fixed unit order. Ties always break on the lower index.
        public int Index { get; }

        public UnitClass Class { get; }
        public Row Row { get; }
        public ClassStats Stats { get; }
        public int MaxHp { get; }
        public int Hp { get; internal set; }
        public bool Alive { get; internal set; }
        public int Cooldown { get; internal set; }

        /// Warden only: the ally it is currently covering. Empty when none.
        public string GuardTargetId { get; internal set; }

        /// Health in thousandths, integer only.
        public int HpPermille => MaxHp <= 0 ? 0 : Hp * 1000 / MaxHp;
    }

    public enum BattleEventKind
    {
        Hit = 0,
        Heal = 1,
        Guard = 2,
        TrapArmed = 3,
        TrapSprung = 4,
        Kill = 5,
        Directive = 6,
        End = 7
    }

    /// Player orders during an auto battle. Costs come from Jacob's UI mockup.
    public enum Directive
    {
        Hold = 0,
        FallBack = 1,
        FocusFire = 2,
        Unleash = 3,
        Sacrifice = 4
    }

    public static class Directives
    {
        public const string CommandKind = "directive";

        public static int Cost(Directive directive)
        {
            switch (directive)
            {
                case Directive.Hold: return 1;
                case Directive.FallBack: return 1;
                case Directive.FocusFire: return 1;
                case Directive.Unleash: return 2;
                case Directive.Sacrifice: return 3;
                default: return 99;
            }
        }

        public static string Name(Directive directive)
        {
            switch (directive)
            {
                case Directive.Hold: return "Hold";
                case Directive.FallBack: return "FallBack";
                case Directive.FocusFire: return "FocusFire";
                case Directive.Unleash: return "Unleash";
                case Directive.Sacrifice: return "Sacrifice";
                default: return "Unknown";
            }
        }

        public static bool TryParse(string text, out Directive directive)
        {
            for (int i = 0; i <= (int)Directive.Sacrifice; i++)
            {
                var candidate = (Directive)i;
                if (string.Equals(Name(candidate), text, StringComparison.Ordinal))
                {
                    directive = candidate;
                    return true;
                }
            }
            directive = Directive.Hold;
            return false;
        }
    }

    /// Something that happened in a fight. The full list is the fight's history, and lessons
    /// cite it, so it must be complete and in the order things happened.
    public readonly struct BattleEvent
    {
        public BattleEvent(
            long tick,
            BattleEventKind kind,
            int actorSide,
            string actorId,
            int actorClass,
            string targetId,
            int targetClass,
            int amount,
            string detail)
        {
            Tick = tick;
            Kind = kind;
            ActorSide = actorSide;
            ActorId = actorId ?? string.Empty;
            ActorClass = actorClass;
            TargetId = targetId ?? string.Empty;
            TargetClass = targetClass;
            Amount = amount;
            Detail = detail ?? string.Empty;
        }

        public long Tick { get; }
        public BattleEventKind Kind { get; }

        /// Side of the unit or player that caused the event.
        public int ActorSide { get; }

        public string ActorId { get; }

        /// Class of the actor as an int, or -1 when the actor is a player order.
        public int ActorClass { get; }

        public string TargetId { get; }

        /// Class of the target as an int, or -1 when there is no unit target.
        public int TargetClass { get; }

        public int Amount { get; }
        public string Detail { get; }
    }
}
