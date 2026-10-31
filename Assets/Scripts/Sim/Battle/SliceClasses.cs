namespace KindredSiege.Sim.Battle
{
    /// The five classes in the 8 December visa slice (Jacob, 1 Oct 2026).
    /// The numbers below are provisional starting values for the prototype, not balance.
    public enum UnitClass
    {
        Warden = 0,
        Marksman = 1,
        Skirmisher = 2,
        Trapper = 3,
        Medic = 4
    }

    public enum Row
    {
        Front = 0,
        Back = 1
    }

    public readonly struct ClassStats
    {
        public ClassStats(int maxHp, int power, int cooldownTicks, bool ranged, Row defaultRow)
        {
            MaxHp = maxHp;
            Power = power;
            CooldownTicks = cooldownTicks;
            Ranged = ranged;
            DefaultRow = defaultRow;
        }

        public int MaxHp { get; }

        /// Damage per attack. For the Medic, the amount healed per action.
        public int Power { get; }

        /// Ticks between actions. The sim runs at 30 ticks a second.
        public int CooldownTicks { get; }

        public bool Ranged { get; }
        public Row DefaultRow { get; }
    }

    public static class SliceClasses
    {
        public const int Count = 5;

        /// Every class in enum order. Loops over classes use this, never a dictionary.
        public static readonly UnitClass[] All =
        {
            UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Medic
        };

        public const int MedicFallbackAttack = 5;
        public const int TrapDamage = 16;
        public const int TrapSlowTicks = 24;

        /// Actions a Trapper spends attacking before it can arm another trap.
        public const int TrapRearmActions = 3;

        /// A guarded hit lands on the Warden instead, reduced by this much.
        public const int GuardReductionPercent = 30;

        /// Provisional. Warden covers the most hurt ally: hits on it land on the Warden instead,
        /// 30 percent lighter. Skirmisher can reach the back row. Trapper arms one trap, then
        /// attacks three times before arming again; the next enemy melee attack on its side
        /// springs the trap. Medic heals the most hurt ally below 90 percent.
        public static ClassStats Stats(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Warden: return new ClassStats(160, 8, 36, false, Row.Front);
                case UnitClass.Marksman: return new ClassStats(90, 18, 45, true, Row.Back);
                case UnitClass.Skirmisher: return new ClassStats(100, 10, 30, false, Row.Front);
                case UnitClass.Trapper: return new ClassStats(95, 9, 39, true, Row.Back);
                case UnitClass.Medic: return new ClassStats(85, 16, 42, true, Row.Back);
                default: return new ClassStats(100, 10, 40, false, Row.Front);
            }
        }

        /// Aim in fights with misses (FightRules.Misses, 13 Oct 2026): a blow lands when a roll comes
        /// up under the attacker's Hit minus the target's Avoid, both in percent. Starting values.
        public static int Hit(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Warden: return 88;
                case UnitClass.Marksman: return 95;
                case UnitClass.Skirmisher: return 92;
                case UnitClass.Trapper: return 90;
                case UnitClass.Medic: return 85;
                default: return 90;
            }
        }

        public static int Avoid(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Warden: return 4;
                case UnitClass.Marksman: return 6;
                case UnitClass.Skirmisher: return 14;
                case UnitClass.Trapper: return 10;
                case UnitClass.Medic: return 8;
                default: return 5;
            }
        }

        public static string Name(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Warden: return "Warden";
                case UnitClass.Marksman: return "Marksman";
                case UnitClass.Skirmisher: return "Skirmisher";
                case UnitClass.Trapper: return "Trapper";
                case UnitClass.Medic: return "Medic";
                default: return "Unknown";
            }
        }
    }
}
