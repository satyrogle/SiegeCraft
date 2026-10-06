using System;
using System.Collections.Generic;

namespace KindredSiege.Sim.Battle
{
    /// What a unit is to its side (Jacob, 4 Oct 2026). A Pal leads the player's side and a Chief
    /// leads a tribe's: if either drops, its side breaks on the spot, and neither can be
    /// sacrificed. Everyone else is a Fighter, which is all the older fights ever had.
    public enum UnitRole
    {
        Fighter = 0,
        Pal = 1,
        Chief = 2
    }

    /// One fighter to place in a battle. Row defaults to the class's usual row. Power and health
    /// are in percent of the class's numbers (tiers, chiefs and gear raise them); 100 is plain.
    public readonly struct UnitSpec
    {
        public const int PlainPercent = 100;

        public UnitSpec(string id, UnitClass unitClass)
            : this(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow)
        {
        }

        public UnitSpec(string id, UnitClass unitClass, Row row)
            : this(id, unitClass, row, UnitRole.Fighter, PlainPercent, PlainPercent)
        {
        }

        public UnitSpec(string id, UnitClass unitClass, Row row, UnitRole role, int powerPercent, int healthPercent)
            : this(id, unitClass, row, role, powerPercent, healthPercent, 0)
        {
        }

        /// The same, with fear: a percent added to the courage this unit's hits drain (gear that
        /// frightens, like the raider chief's Lava Sun Tabard). 0 is plain.
        public UnitSpec(string id, UnitClass unitClass, Row row, UnitRole role, int powerPercent, int healthPercent, int fearPercent)
        {
            Id = id ?? string.Empty;
            Class = unitClass;
            Row = row;
            Role = role;
            PowerPercent = Math.Max(1, powerPercent);
            HealthPercent = Math.Max(1, healthPercent);
            FearPercent = Math.Max(0, fearPercent);
        }

        public string Id { get; }
        public UnitClass Class { get; }
        public Row Row { get; }
        public UnitRole Role { get; }
        public int PowerPercent { get; }
        public int HealthPercent { get; }

        /// Percent added to the courage this unit's hits drain. 0 unless its gear frightens.
        public int FearPercent { get; }
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

    /// The nerve bar (Jacob, 2 Oct 2026). Each side starts a fight with some nerve. Hits, deaths
    /// and scary orders drain it, Hold drains your own, and a side at zero breaks: the fight ends
    /// there and its survivors live. Off by default, so older fights and their tests keep their
    /// numbers. The numbers are placeholders, not balance.
    public sealed class NerveRules
    {
        /// Full nerve. Nerve is counted in thousandths of a full bar.
        public const int Full = 1000;

        /// The old rules: no nerve, fights end in a wipe or on time.
        public static readonly NerveRules Off = new NerveRules(false, Full, Full, 0, 0,
            BattleSimulation.UnleashNerveCost, 1);

        private readonly int _start0;
        private readonly int _start1;
        private readonly int _fear0;
        private readonly int _fear1;

        private NerveRules(bool enabled, int start0, int start1, int fear0, int fear1, int unleashCost, int holdDrainEveryTicks)
        {
            Enabled = enabled;
            _start0 = ClampStart(start0);
            _start1 = ClampStart(start1);
            _fear0 = Math.Max(0, fear0);
            _fear1 = Math.Max(0, fear1);
            UnleashCost = Math.Max(0, unleashCost);
            HoldDrainEveryTicks = Math.Max(1, holdDrainEveryTicks);
        }

        /// Nerve on. Pass each side's starting nerve (a tribe brings what it has left) and, for
        /// gear that frightens, a percent added to the nerve damage that side deals.
        public static NerveRules On(int startNerve0 = Full, int startNerve1 = Full, int fearBonusPercent0 = 0, int fearBonusPercent1 = 0)
        {
            return new NerveRules(true, startNerve0, startNerve1, fearBonusPercent0, fearBonusPercent1,
                BattleSimulation.UnleashNerveCost, 1);
        }

        /// The same rules with other prices on the two orders that cost nerve: what the enemy loses
        /// when you Unleash, and how often a running Hold takes one nerve from the holder (every
        /// tick by default, 150 for a full Hold). For balance work on the slice (Jacob, 4 Oct 2026).
        public NerveRules WithOrderCosts(int unleashCost, int holdDrainEveryTicks)
        {
            return new NerveRules(Enabled, _start0, _start1, _fear0, _fear1, unleashCost, holdDrainEveryTicks);
        }

        public bool Enabled { get; }

        /// Nerve the enemy loses when a side Unleashes. BattleSimulation.UnleashNerveCost unless set.
        public int UnleashCost { get; }

        /// A running Hold takes one nerve from the holder every this many ticks. 1 unless set.
        public int HoldDrainEveryTicks { get; }

        /// 1 to Full. A side cannot start a fight already broken.
        public int StartNerve(int side)
        {
            return side == 0 ? _start0 : _start1;
        }

        /// Percent added to the nerve damage this side deals to the enemy. 0 without gear.
        public int FearBonusPercent(int side)
        {
            return side == 0 ? _fear0 : _fear1;
        }

        private static int ClampStart(int nerve)
        {
            return Math.Max(1, Math.Min(Full, nerve));
        }
    }

    /// Everything needed to start the same fight again. Tactics are copied in, so a replay
    /// uses what each side knew at the time, not what it learned afterwards.
    public sealed class BattleSetup
    {
        public const int DefaultTickLimit = 3600;
        public const int DefaultDirectivePoints = 5;

        /// Unit health at the old toughness, in percent of each class's health.
        public const int NormalHealthPercent = 100;

        private readonly UnitSpec[] _side0;
        private readonly UnitSpec[] _side1;
        private readonly TacticsProfile _tactics0;
        private readonly TacticsProfile _tactics1;
        private readonly SideRules _rules0;
        private readonly SideRules _rules1;

        public BattleSetup(
            string fightId,
            ulong seed,
            IReadOnlyList<UnitSpec> side0,
            IReadOnlyList<UnitSpec> side1,
            TacticsProfile side0Tactics,
            TacticsProfile side1Tactics,
            int tickLimit = DefaultTickLimit,
            int startingDirectivePoints = DefaultDirectivePoints)
            : this(fightId, seed, side0, side1, side0Tactics, side1Tactics, tickLimit, startingDirectivePoints, NerveRules.Off)
        {
        }

        /// The same, with nerve rules. Pass NerveRules.Off for the old rules. With refillTicks above
        /// 0, each side gets one directive point back every that many ticks, up to its starting
        /// points; 0 never refills, as before. healthPercent scales every unit's health; 100 is the
        /// old toughness. Refilling orders and a slower pace are the slice's newer rules (Jacob,
        /// 2 and 3 Oct 2026). Side rules give a side its big moves (Eclipse, combos); null is none.
        public BattleSetup(
            string fightId,
            ulong seed,
            IReadOnlyList<UnitSpec> side0,
            IReadOnlyList<UnitSpec> side1,
            TacticsProfile side0Tactics,
            TacticsProfile side1Tactics,
            int tickLimit,
            int startingDirectivePoints,
            NerveRules nerve,
            int refillTicks = 0,
            int healthPercent = NormalHealthPercent,
            SideRules side0Rules = null,
            SideRules side1Rules = null)
        {
            if (side0 == null || side0.Count == 0) throw new ArgumentException("Side 0 needs at least one unit.");
            if (side1 == null || side1.Count == 0) throw new ArgumentException("Side 1 needs at least one unit.");
            if (tickLimit <= 0) throw new ArgumentOutOfRangeException(nameof(tickLimit));

            FightId = fightId ?? string.Empty;
            Seed = seed;
            TickLimit = tickLimit;
            StartingDirectivePoints = Math.Max(0, startingDirectivePoints);
            Nerve = nerve ?? NerveRules.Off;
            RefillTicks = Math.Max(0, refillTicks);
            HealthPercent = Math.Max(1, healthPercent);
            _side0 = Copy(side0);
            _side1 = Copy(side1);
            _tactics0 = side0Tactics == null ? new TacticsProfile() : side0Tactics.Clone();
            _tactics1 = side1Tactics == null ? new TacticsProfile() : side1Tactics.Clone();
            _rules0 = side0Rules ?? SideRules.None;
            _rules1 = side1Rules ?? SideRules.None;
        }

        public string FightId { get; }
        public ulong Seed { get; }
        public int TickLimit { get; }
        public int StartingDirectivePoints { get; }

        /// The nerve rules for this fight. NerveRules.Off unless set.
        public NerveRules Nerve { get; }

        /// Ticks between refilled directive points, or 0 when orders never refill.
        public int RefillTicks { get; }

        /// Every unit's health in percent of its class's health. NormalHealthPercent unless set.
        public int HealthPercent { get; }

        public IReadOnlyList<UnitSpec> Side(int side)
        {
            return side == 0 ? (IReadOnlyList<UnitSpec>)_side0 : _side1;
        }

        /// A side's big moves. SideRules.None unless set.
        public SideRules Rules(int side)
        {
            return side == 0 ? _rules0 : _rules1;
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
        internal BattleUnit(string id, int side, int index, UnitClass unitClass, Row row,
            int healthPercent = BattleSetup.NormalHealthPercent)
            : this(new UnitSpec(id, unitClass, row), side, index, healthPercent)
        {
        }

        /// healthPercent is the fight's toughness; the spec's own health percent goes on top.
        internal BattleUnit(UnitSpec spec, int side, int index, int healthPercent)
        {
            Id = spec.Id;
            Side = side;
            Index = index;
            Class = spec.Class;
            Row = spec.Row;
            Role = spec.Role;
            PowerPercent = spec.PowerPercent;
            Stats = SliceClasses.Stats(spec.Class);
            MaxHp = Math.Max(1, Stats.MaxHp * healthPercent / 100 * spec.HealthPercent / 100);
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
        public UnitRole Role { get; }

        /// Power in percent of the class's power. UnitSpec.PlainPercent unless raised.
        public int PowerPercent { get; }

        public ClassStats Stats { get; }
        public int MaxHp { get; }
        public int Hp { get; internal set; }
        public bool Alive { get; internal set; }
        public int Cooldown { get; internal set; }

        /// Warden only: the ally it is currently covering. Empty when none.
        public string GuardTargetId { get; internal set; }

        /// Health in thousandths, integer only.
        public int HpPermille => MaxHp <= 0 ? 0 : Hp * 1000 / MaxHp;

        /// Damage per attack (or healing per action for the Medic), with this unit's power percent.
        public int Power => Math.Max(1, Stats.Power * PowerPercent / 100);

        /// True for a Pal or a Chief: if it drops, its side breaks.
        public bool IsLeader => Role != UnitRole.Fighter;
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
        End = 7,

        /// Nerve rules only: a side's nerve ran out. Actor side is the side that broke, amount is
        /// how many of its fighters were still standing. Detail is "break", or "leader" when its
        /// Pal or Chief dropped.
        Break = 8,

        /// A big move started winding up. Actor is the caster, amount the windup in ticks, detail
        /// the move ("Eclipse", or "Pyre" when it finishes that combo), target the Pyre's target.
        Windup = 9,

        /// A caster was hit hard enough to lose its wound up move. Actor side is the side that
        /// interrupted (its next order is doubled), target is the caster, amount the damage taken.
        Interrupt = 10,

        /// Orders fused into a combo. Actor side is the side that made it, detail the combo.
        Combo = 11,

        /// A Pal transformed (Frenzy). Actor is the Pal, amount the ticks it lasts.
        Transform = 12,

        /// An empowered order was used: it runs twice as long or hits twice as hard. Detail is the order.
        Empowered = 13,

        /// A wound up move went off. Actor is the caster, amount the percent of max health it
        /// hits for, detail "full", "soaked" (the enemy held) or "dodged" (the enemy fell back).
        Release = 14
    }

    /// Player orders during an auto battle. Costs come from Jacob's UI mockup; Eclipse and
    /// Frenzy came with the slice's big moves (Jacob, 4 Oct 2026).
    public enum Directive
    {
        Hold = 0,
        FallBack = 1,
        FocusFire = 2,
        Unleash = 3,
        Sacrifice = 4,
        Eclipse = 5,
        Frenzy = 6
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
                case Directive.Eclipse: return 3;
                case Directive.Frenzy: return 0;
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
                case Directive.Eclipse: return "Eclipse";
                case Directive.Frenzy: return "Frenzy";
                default: return "Unknown";
            }
        }

        public static bool TryParse(string text, out Directive directive)
        {
            for (int i = 0; i <= (int)Directive.Frenzy; i++)
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
