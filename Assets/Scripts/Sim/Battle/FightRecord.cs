using System;
using System.Collections.Generic;

namespace KindredSiege.Sim.Battle
{
    /// The finished history of one fight: every event in order, the result and the checksum.
    /// Lessons are computed from this and cite its numbers, so anyone can replay the fight and
    /// check the numbers again.
    public sealed class FightRecord
    {
        /// HP a sprung trap is worth beyond its damage, for the slow it causes. Provisional.
        public const int TrapSlowThreat = 12;

        private readonly List<BattleEvent> _events;
        private readonly UnitClass[][] _classes = new UnitClass[2][];

        private FightRecord(BattleSimulation sim)
        {
            FightId = sim.Setup.FightId;
            Seed = sim.Setup.Seed;
            StartingDirectivePoints = sim.Setup.StartingDirectivePoints;
            Winner = sim.Winner;
            EndTick = sim.EndTick;
            Checksum = StateChecksum.Compute(sim.OrderedStateSnapshot);
            _events = new List<BattleEvent>(sim.Events);
            for (int side = 0; side < 2; side++)
            {
                IReadOnlyList<UnitSpec> specs = sim.Setup.Side(side);
                _classes[side] = new UnitClass[specs.Count];
                for (int i = 0; i < specs.Count; i++) _classes[side][i] = specs[i].Class;
            }
            int survivors0 = 0;
            int survivors1 = 0;
            for (int i = 0; i < sim.Units.Count; i++)
            {
                if (!sim.Units[i].Alive) continue;
                if (sim.Units[i].Side == 0) survivors0++;
                else survivors1++;
            }
            Survivors0 = survivors0;
            Survivors1 = survivors1;
        }

        public static FightRecord From(BattleSimulation sim)
        {
            if (sim == null) throw new ArgumentNullException(nameof(sim));
            if (!sim.IsFinished) throw new InvalidOperationException("The fight has not finished.");
            return new FightRecord(sim);
        }

        public string FightId { get; }
        public ulong Seed { get; }

        /// Directive points each side started with. 0 means nobody could give orders, as in
        /// off screen wars between tribes.
        public int StartingDirectivePoints { get; }

        public int Winner { get; }
        public long EndTick { get; }
        public string Checksum { get; }
        public int Survivors0 { get; }
        public int Survivors1 { get; }
        public IReadOnlyList<BattleEvent> Events => _events;

        public int Survivors(int side)
        {
            return side == 0 ? Survivors0 : Survivors1;
        }

        public IReadOnlyList<UnitClass> Classes(int side)
        {
            return _classes[side];
        }

        /// True if the side started the fight with at least one unit of the class.
        public bool Fielded(int side, UnitClass unitClass)
        {
            UnitClass[] classes = _classes[side];
            for (int i = 0; i < classes.Length; i++)
            {
                if (classes[i] == unitClass) return true;
            }
            return false;
        }

        /// Healing done by a class on a side, not counting sacrifice heals.
        public int HealedBy(int side, UnitClass healerClass, out int heals, out long firstTick, out long lastTick)
        {
            int total = 0;
            heals = 0;
            firstTick = -1;
            lastTick = -1;
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind != BattleEventKind.Heal || e.ActorSide != side || e.ActorClass != (int)healerClass) continue;
                if (string.Equals(e.Detail, BattleSimulation.SacrificeDetail, StringComparison.Ordinal)) continue;
                total += e.Amount;
                heals++;
                if (firstTick < 0) firstTick = e.Tick;
                lastTick = e.Tick;
            }
            return total;
        }

        /// Damage a side's Wardens stopped by covering allies.
        public int GuardPrevented(int side, out int guards, out long firstTick, out long lastTick)
        {
            int total = 0;
            guards = 0;
            firstTick = -1;
            lastTick = -1;
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind != BattleEventKind.Guard || e.ActorSide != side) continue;
                total += e.Amount;
                guards++;
                if (firstTick < 0) firstTick = e.Tick;
                lastTick = e.Tick;
            }
            return total;
        }

        /// Traps sprung on a side's fighters, optionally leaving out Wardens.
        public int TrapsSprungOn(int victimSide, bool leaveOutWardens, out long firstTick, out long lastTick)
        {
            int count = 0;
            firstTick = -1;
            lastTick = -1;
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind != BattleEventKind.TrapSprung || e.ActorSide == victimSide) continue;
                if (leaveOutWardens && e.TargetClass == (int)UnitClass.Warden) continue;
                count++;
                if (firstTick < 0) firstTick = e.Tick;
                lastTick = e.Tick;
            }
            return count;
        }

        /// Traps a side armed during the fight.
        public int TrapsArmedBy(int side)
        {
            int count = 0;
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].Kind == BattleEventKind.TrapArmed && _events[i].ActorSide == side) count++;
            }
            return count;
        }

        /// Focus fire orders a side gave, and how many ended with the focused unit dead inside the
        /// focus window. Reports the first such kill: when it was ordered, the victim's class and
        /// how many ticks the kill took. With leaveOutWardens, orders on Wardens are not counted.
        public int FocusKills(int focuserSide, bool leaveOutWardens, out int focuses, out long firstOrderTick,
            out int firstVictimClass, out long firstKillTicks)
        {
            int kills = 0;
            focuses = 0;
            firstOrderTick = -1;
            firstVictimClass = -1;
            firstKillTicks = -1;
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent order = _events[i];
                if (order.Kind != BattleEventKind.Directive || order.ActorSide != focuserSide) continue;
                if (!string.Equals(order.Detail, Directives.Name(Directive.FocusFire), StringComparison.Ordinal)) continue;
                if (leaveOutWardens && order.TargetClass == (int)UnitClass.Warden) continue;
                focuses++;
                long windowEnd = order.Tick + BattleSimulation.FocusTicks;
                for (int j = i + 1; j < _events.Count && _events[j].Tick < windowEnd; j++)
                {
                    BattleEvent e = _events[j];
                    if (e.Kind != BattleEventKind.Kill || e.ActorSide != focuserSide) continue;
                    if (!string.Equals(e.TargetId, order.TargetId, StringComparison.Ordinal)) continue;
                    kills++;
                    if (firstOrderTick < 0)
                    {
                        firstOrderTick = order.Tick;
                        firstVictimClass = e.TargetClass;
                        firstKillTicks = e.Tick - order.Tick;
                    }
                    break;
                }
            }
            return kills;
        }

        /// Damage a side dealt inside its own Unleash windows. Overlapping windows count once.
        public int UnleashDamage(int side, out int unleashes, out long firstTick)
        {
            int damage = 0;
            unleashes = 0;
            firstTick = -1;
            long windowEnd = -1;
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind == BattleEventKind.Directive && e.ActorSide == side &&
                    string.Equals(e.Detail, Directives.Name(Directive.Unleash), StringComparison.Ordinal))
                {
                    unleashes++;
                    if (firstTick < 0) firstTick = e.Tick;
                    windowEnd = Math.Max(windowEnd, e.Tick + BattleSimulation.UnleashTicks);
                    continue;
                }
                if (e.Kind == BattleEventKind.Hit && e.ActorSide == side && e.Tick < windowEnd) damage += e.Amount;
            }
            return damage;
        }

        /// Fall back orders a side gave.
        public int FallBacksUsed(int side, out long firstTick)
        {
            int count = 0;
            firstTick = -1;
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind != BattleEventKind.Directive || e.ActorSide != side) continue;
                if (!string.Equals(e.Detail, Directives.Name(Directive.FallBack), StringComparison.Ordinal)) continue;
                count++;
                if (firstTick < 0) firstTick = e.Tick;
            }
            return count;
        }

        /// Damage a side took, split by the class that dealt it. Index is (int)UnitClass.
        public int[] DamageTakenByClass(int victimSide, out int total)
        {
            var byClass = new int[SliceClasses.Count];
            total = 0;
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind != BattleEventKind.Hit || e.ActorSide == victimSide || e.ActorClass < 0) continue;
                byClass[e.ActorClass] += e.Amount;
                total += e.Amount;
            }
            return byClass;
        }

        /// How much each enemy class hurt the learner's side, in HP: damage dealt, plus healing
        /// for Medics, damage stopped by guarding for Wardens, and the slow of each sprung trap
        /// for Trappers. Index is (int)UnitClass.
        public int[] ThreatByClass(int learnerSide, out int total)
        {
            int enemy = 1 - learnerSide;
            int[] threat = DamageTakenByClass(learnerSide, out _);
            threat[(int)UnitClass.Medic] += HealedBy(enemy, UnitClass.Medic, out _, out _, out _);
            threat[(int)UnitClass.Warden] += GuardPrevented(enemy, out _, out _, out _);
            threat[(int)UnitClass.Trapper] += TrapsSprungOn(learnerSide, false, out _, out _) * TrapSlowThreat;
            total = 0;
            for (int i = 0; i < threat.Length; i++) total += threat[i];
            return threat;
        }

        /// Class of the first unit a side struck with a normal attack, or -1 if it never struck.
        public int FirstTargetClass(int attackerSide)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind != BattleEventKind.Hit || e.ActorSide != attackerSide || e.Detail.Length != 0) continue;
                return e.TargetClass;
            }
            return -1;
        }

        /// Normal attacks a side landed on each target class. Index is (int)UnitClass.
        public int[] AttacksOnClass(int attackerSide)
        {
            var byClass = new int[SliceClasses.Count];
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind != BattleEventKind.Hit || e.ActorSide != attackerSide || e.Detail.Length != 0) continue;
                if (e.TargetClass >= 0) byClass[e.TargetClass]++;
            }
            return byClass;
        }

        /// Total damage a side dealt, counting everything that landed.
        public int DamageDealt(int side)
        {
            int total = 0;
            for (int i = 0; i < _events.Count; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind == BattleEventKind.Hit && e.ActorSide == side) total += e.Amount;
            }
            return total;
        }
    }
}
