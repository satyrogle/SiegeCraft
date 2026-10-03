using System;
using System.Collections.Generic;
using System.Globalization;

namespace KindredSiege.Sim.Battle
{
    /// A deterministic auto battle between two sides. Units act on their own, steered by their
    /// side's tactics; the only outside input is a directive command. Learned reflexes (answer
    /// Unleash with Hold, cover whoever the enemy focuses, press with Unleash when the enemy falls
    /// back) fire inside the sim from the tactics, so they replay exactly. Integers only, one
    /// seeded random stream, units always processed in a fixed order. Numbers are provisional.
    public sealed class BattleSimulation : ISimulation
    {
        public const int HoldTicks = 150;
        public const int FallBackTicks = 75;
        public const int FocusTicks = 150;
        public const int UnleashTicks = 120;
        public const int HoldDamagePercent = 70;
        public const int UnleashDamagePercent = 150;
        public const int SacrificeHealPercent = 25;
        public const int MedicHealsBelowPermille = 900;
        public const int HabitThreshold = 3;
        public const int AvoidGuardedScore = 400;
        public const int OpenerTick = 15;
        public const string SacrificeDetail = "sacrifice";
        public const string TrapDetail = "trap";
        public const string GuardedDetail = "guarded";
        public const string CoverDetail = "Cover";
        public const string AutoActorPrefix = "auto";

        private const ulong FnvOffset = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        private readonly BattleSetup _setup;
        private readonly TacticsProfile[] _tactics;
        private readonly DeterministicRng _rng;
        private readonly List<BattleUnit> _units = new List<BattleUnit>();
        private readonly List<BattleEvent> _events = new List<BattleEvent>();
        private readonly SideState[] _sides = { new SideState(), new SideState() };
        private readonly int[] _trapReadyIn;
        private long _tick;
        private long _endTick = -1;
        private bool _finished;
        private int _winner = -1;
        private ulong _eventHash = FnvOffset;

        private sealed class SideState
        {
            public int Points;
            public int HoldTicks;
            public int FallBackTicks;
            public int FocusTicks;
            public int UnleashTicks;
            public string FocusTargetId = string.Empty;
            public bool TrapArmed;
            public string TrapOwnerId = string.Empty;
            public int ForcedWardTicks;
            public string ForcedWardId = string.Empty;
        }

        public BattleSimulation(BattleSetup setup)
        {
            _setup = setup ?? throw new ArgumentNullException(nameof(setup));
            _tactics = new[] { setup.CopyTactics(0), setup.CopyTactics(1) };
            _rng = DeterministicRng.ForStream(setup.Seed, SimStream.Battle);

            int index = 0;
            for (int side = 0; side < 2; side++)
            {
                IReadOnlyList<UnitSpec> specs = setup.Side(side);
                for (int i = 0; i < specs.Count; i++)
                {
                    UnitSpec spec = specs[i];
                    if (spec.Id.Length == 0) throw new ArgumentException("Every unit needs an id.");
                    if (FindById(spec.Id) != null) throw new ArgumentException("Duplicate unit id " + spec.Id + ".");
                    _units.Add(new BattleUnit(spec.Id, side, index++, spec.Class, spec.Row));
                }
            }
            _trapReadyIn = new int[_units.Count];

            _sides[0].Points = setup.StartingDirectivePoints;
            _sides[1].Points = setup.StartingDirectivePoints;

            // Stagger first actions so both sides do not open in lockstep.
            for (int i = 0; i < _units.Count; i++)
                _units[i].Cooldown = 1 + _rng.Next(_units[i].Stats.CooldownTicks);
        }

        public BattleSetup Setup => _setup;
        public long CurrentTick => _tick;
        public bool IsFinished => _finished;

        /// 0 or 1 once finished, -1 for a draw or while running.
        public int Winner => _winner;

        /// Tick on which the fight ended, or -1 while running.
        public long EndTick => _endTick;

        public IReadOnlyList<BattleUnit> Units => _units;
        public IReadOnlyList<BattleEvent> Events => _events;
        public ulong EventHash => _eventHash;

        public int DirectivePoints(int side)
        {
            return _sides[side].Points;
        }

        public BattleUnit FindById(string id)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                if (string.Equals(_units[i].Id, id, StringComparison.Ordinal)) return _units[i];
            }
            return null;
        }

        public int AliveCount(int side)
        {
            int count = 0;
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i].Side == side && _units[i].Alive) count++;
            }
            return count;
        }

        /// True while a Warden on the unit's own side is covering it.
        public bool IsGuarded(BattleUnit unit)
        {
            return GuardingWarden(unit) != null;
        }

        /// Runs ticks until the fight ends or the tick limit passes. Returns the tick count reached.
        public long RunUntilFinished()
        {
            while (!_finished && _tick < _setup.TickLimit) Tick();
            return _tick;
        }

        public void Tick()
        {
            if (!_finished)
            {
                if (_tick == OpenerTick)
                {
                    for (int s = 0; s < 2; s++) TryOpener(s);
                }

                // Alternate the scan direction each tick so neither side always acts first.
                bool forward = (_tick & 1L) == 0L;
                for (int n = 0; n < _units.Count && !_finished; n++)
                {
                    BattleUnit unit = _units[forward ? n : _units.Count - 1 - n];
                    if (!unit.Alive) continue;
                    unit.Cooldown--;
                    if (unit.Cooldown > 0) continue;
                    Act(unit);
                    if (unit.Alive) unit.Cooldown += unit.Stats.CooldownTicks;
                    CheckWipe();
                }

                // Counted down after the tick's actions, so an order given on tick t is in force
                // for exactly its length: ticks t to t + length - 1.
                for (int s = 0; s < 2; s++) CountDownEffects(_sides[s]);

                if (!_finished && _tick + 1 >= _setup.TickLimit) EndOnTime();
            }
            _tick++;
        }

        public bool TryApply(SimCommand command, out string failure)
        {
            failure = string.Empty;
            if (command == null)
            {
                failure = "Command is null.";
                return false;
            }
            if (!string.Equals(command.Kind, Directives.CommandKind, StringComparison.Ordinal))
            {
                failure = "Unknown command kind " + command.Kind + ".";
                return false;
            }
            if (_finished)
            {
                failure = "The battle is over.";
                return false;
            }

            int side;
            if (string.Equals(command.ActorId, "0", StringComparison.Ordinal)) side = 0;
            else if (string.Equals(command.ActorId, "1", StringComparison.Ordinal)) side = 1;
            else
            {
                failure = "Directive actor must be side 0 or 1.";
                return false;
            }

            if (!Directives.TryParse(command.TextArg, out Directive directive))
            {
                failure = "Unknown directive " + command.TextArg + ".";
                return false;
            }

            SideState state = _sides[side];
            int cost = Directives.Cost(directive);
            if (state.Points < cost)
            {
                failure = "Not enough directive points: need " + cost + ", have " + state.Points + ".";
                return false;
            }

            BattleUnit target = null;
            if (directive == Directive.FocusFire)
            {
                target = FindById(command.TargetId);
                if (target == null || !target.Alive || target.Side == side)
                {
                    failure = "Focus fire needs a living enemy target.";
                    return false;
                }
            }
            else if (directive == Directive.Sacrifice)
            {
                target = FindById(command.TargetId);
                if (target == null || !target.Alive || target.Side != side)
                {
                    failure = "Sacrifice needs a living unit on your side.";
                    return false;
                }
                if (AliveCount(side) < 2)
                {
                    failure = "Sacrifice needs another living ally to save.";
                    return false;
                }
            }

            ApplyDirective(side, directive, target, "side" + side.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        public string OrderedStateSnapshot
        {
            get
            {
                var b = new SnapshotBuilder();
                b.Section("battle");
                b.Field("fight", _setup.FightId);
                b.Field("tick", _tick);
                b.Field("finished", _finished);
                b.Field("winner", _winner);
                b.Field("endTick", _endTick);
                b.Field("rng", _rng.State);
                for (int s = 0; s < 2; s++)
                {
                    SideState state = _sides[s];
                    string name = "side" + s.ToString(CultureInfo.InvariantCulture);
                    b.Section(name);
                    b.Field("points", state.Points);
                    b.Field("hold", state.HoldTicks);
                    b.Field("fallBack", state.FallBackTicks);
                    b.Field("focus", state.FocusTicks);
                    b.Field("focusTarget", state.FocusTargetId);
                    b.Field("unleash", state.UnleashTicks);
                    b.Field("trapArmed", state.TrapArmed);
                    b.Field("trapOwner", state.TrapOwnerId);
                    b.Field("ward", state.ForcedWardTicks);
                    b.Field("wardId", state.ForcedWardId);
                    _tactics[s].AppendTo(b, name + ".");
                }
                b.Section("units");
                for (int i = 0; i < _units.Count; i++)
                {
                    BattleUnit u = _units[i];
                    b.Field(u.Id + ".hp", u.Hp);
                    b.Field(u.Id + ".alive", u.Alive);
                    b.Field(u.Id + ".cd", u.Cooldown);
                    b.Field(u.Id + ".guard", u.GuardTargetId);
                    b.Field(u.Id + ".trapIn", _trapReadyIn[i]);
                }
                b.Section("events");
                b.Field("count", _events.Count);
                b.Field("hash", _eventHash);
                return b.ToString();
            }
        }

        // ------------------------------------------------------------ directives and reflexes

        private void ApplyDirective(int side, Directive directive, BattleUnit target, string actorId)
        {
            SideState state = _sides[side];
            int cost = Directives.Cost(directive);
            state.Points -= cost;
            Emit(new BattleEvent(_tick, BattleEventKind.Directive, side, actorId, -1,
                target == null ? string.Empty : target.Id, target == null ? -1 : (int)target.Class, cost, Directives.Name(directive)));

            switch (directive)
            {
                case Directive.Hold:
                    state.HoldTicks = HoldTicks;
                    break;
                case Directive.FallBack:
                    state.FallBackTicks = FallBackTicks;
                    ReflexPressOnFallBack(1 - side);
                    break;
                case Directive.FocusFire:
                    state.FocusTargetId = target.Id;
                    state.FocusTicks = FocusTicks;
                    ReflexCoverFocused(1 - side, target);
                    break;
                case Directive.Unleash:
                    state.UnleashTicks = UnleashTicks;
                    ReflexHoldOnUnleash(1 - side);
                    break;
                case Directive.Sacrifice:
                    Sacrifice(target);
                    break;
            }
        }

        /// Learned: our Warden covers whoever the enemy just focused.
        private void ReflexCoverFocused(int side, BattleUnit focused)
        {
            if (_tactics[side].CoverFocused < HabitThreshold || !focused.Alive || focused.Class == UnitClass.Warden) return;
            BattleUnit warden = FirstLivingWarden(side);
            if (warden == null) return;
            SideState state = _sides[side];
            state.ForcedWardId = focused.Id;
            state.ForcedWardTicks = FocusTicks;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (u.Alive && u.Side == side && u.Class == UnitClass.Warden) u.GuardTargetId = focused.Id;
            }
            Emit(new BattleEvent(_tick, BattleEventKind.Directive, side, AutoActor(side), -1,
                focused.Id, (int)focused.Class, 0, CoverDetail));
        }

        /// Learned: answer the enemy's Unleash with Hold straight away.
        private void ReflexHoldOnUnleash(int side)
        {
            if (_tactics[side].HoldOnUnleash < HabitThreshold) return;
            if (_sides[side].Points < Directives.Cost(Directive.Hold) || _sides[side].HoldTicks > 0) return;
            ApplyDirective(side, Directive.Hold, null, AutoActor(side));
        }

        /// Learned: when the enemy falls back, nobody is in melee, so our ranged fighters unleash.
        private void ReflexPressOnFallBack(int side)
        {
            if (_tactics[side].PressOnFallBack < HabitThreshold) return;
            if (_sides[side].Points < Directives.Cost(Directive.Unleash) || _sides[side].UnleashTicks > 0) return;
            ApplyDirective(side, Directive.Unleash, null, AutoActor(side));
        }

        /// Open the fight by focusing a set enemy class. Not learned (see TacticsProfile.FocusOpener).
        private void TryOpener(int side)
        {
            int opener = _tactics[side].FocusOpener;
            if (opener < 0 || _sides[side].Points < Directives.Cost(Directive.FocusFire)) return;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (u.Alive && u.Side != side && (int)u.Class == opener)
                {
                    ApplyDirective(side, Directive.FocusFire, u, AutoActor(side));
                    return;
                }
            }
        }

        private static string AutoActor(int side)
        {
            return AutoActorPrefix + side.ToString(CultureInfo.InvariantCulture);
        }

        private static void CountDownEffects(SideState state)
        {
            if (state.HoldTicks > 0) state.HoldTicks--;
            if (state.FallBackTicks > 0) state.FallBackTicks--;
            if (state.UnleashTicks > 0) state.UnleashTicks--;
            if (state.FocusTicks > 0)
            {
                state.FocusTicks--;
                if (state.FocusTicks == 0) state.FocusTargetId = string.Empty;
            }
            if (state.ForcedWardTicks > 0)
            {
                state.ForcedWardTicks--;
                if (state.ForcedWardTicks == 0) state.ForcedWardId = string.Empty;
            }
        }

        // ------------------------------------------------------------ actions

        private void Act(BattleUnit unit)
        {
            if (unit.Class == UnitClass.Warden) UpdateGuard(unit);

            if (unit.Class == UnitClass.Medic)
            {
                BattleUnit patient = MostHurtAlly(unit, MedicHealsBelowPermille, true);
                if (patient != null)
                {
                    Heal(unit, patient, unit.Stats.Power, string.Empty);
                    return;
                }
                Attack(unit, SliceClasses.MedicFallbackAttack);
                return;
            }

            if (unit.Class == UnitClass.Trapper)
            {
                SideState own = _sides[unit.Side];
                if (!own.TrapArmed && _trapReadyIn[unit.Index] <= 0)
                {
                    own.TrapArmed = true;
                    own.TrapOwnerId = unit.Id;
                    _trapReadyIn[unit.Index] = SliceClasses.TrapRearmActions;
                    Emit(EventFor(BattleEventKind.TrapArmed, unit, null, 0, string.Empty));
                    return;
                }
                if (_trapReadyIn[unit.Index] > 0) _trapReadyIn[unit.Index]--;
            }

            Attack(unit, unit.Stats.Power);
        }

        private void UpdateGuard(BattleUnit warden)
        {
            SideState own = _sides[warden.Side];
            if (own.ForcedWardTicks > 0)
            {
                BattleUnit forced = FindById(own.ForcedWardId);
                if (forced != null && forced.Alive && !ReferenceEquals(forced, warden))
                {
                    warden.GuardTargetId = forced.Id;
                    return;
                }
            }
            BattleUnit ward = MostHurtAlly(warden, int.MaxValue, false);
            warden.GuardTargetId = ward == null ? string.Empty : ward.Id;
        }

        /// Living ally below the given health with the lowest health. Ties go to the lower index.
        private BattleUnit MostHurtAlly(BattleUnit unit, int belowPermille, bool includeSelf)
        {
            BattleUnit best = null;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive || u.Side != unit.Side) continue;
                if (!includeSelf && ReferenceEquals(u, unit)) continue;
                if (u.HpPermille >= belowPermille) continue;
                if (best == null || u.HpPermille < best.HpPermille) best = u;
            }
            return best;
        }

        private void Attack(BattleUnit attacker, int power)
        {
            BattleUnit target = ChooseTarget(attacker);
            if (target == null) return; // nothing it can or will strike this action; it waits

            int damage = power * (85 + _rng.Next(31)) / 100;
            if (damage < 1) damage = 1;

            SideState mine = _sides[attacker.Side];
            SideState theirs = _sides[target.Side];
            if (mine.UnleashTicks > 0) damage = damage * UnleashDamagePercent / 100;
            if (theirs.HoldTicks > 0 && target.Row == Row.Front) damage = damage * HoldDamagePercent / 100;

            // A melee attacker springs the defenders' armed trap before its blow lands.
            if (!attacker.Stats.Ranged && theirs.TrapArmed)
            {
                BattleUnit trapper = FindById(theirs.TrapOwnerId);
                theirs.TrapArmed = false;
                theirs.TrapOwnerId = string.Empty;
                Emit(new BattleEvent(_tick, BattleEventKind.TrapSprung, target.Side,
                    trapper == null ? string.Empty : trapper.Id, trapper == null ? -1 : (int)trapper.Class,
                    attacker.Id, (int)attacker.Class, SliceClasses.TrapDamage, string.Empty));
                attacker.Cooldown += SliceClasses.TrapSlowTicks;
                ApplyDamage(trapper, target.Side, attacker, SliceClasses.TrapDamage, TrapDetail);
                if (!attacker.Alive) return;
            }

            // A Warden covering this target takes the blow instead, lighter.
            BattleUnit warden = GuardingWarden(target);
            if (warden != null)
            {
                int taken = damage * (100 - SliceClasses.GuardReductionPercent) / 100;
                int prevented = damage - taken;
                Emit(EventFor(BattleEventKind.Guard, warden, target, prevented, string.Empty));
                ApplyDamage(attacker, attacker.Side, warden, taken, GuardedDetail);
                return;
            }

            ApplyDamage(attacker, attacker.Side, target, damage, string.Empty);
        }

        private BattleUnit ChooseTarget(BattleUnit attacker)
        {
            SideState mine = _sides[attacker.Side];
            SideState theirs = _sides[1 - attacker.Side];
            TacticsProfile tactics = _tactics[attacker.Side];
            bool melee = !attacker.Stats.Ranged;

            // Either side has fallen back: the lines are apart and no melee lands for now.
            if (melee && (theirs.FallBackTicks > 0 || mine.FallBackTicks > 0)) return null;

            // Learned habit: let our own Warden spring their trap first.
            if (melee && attacker.Class != UnitClass.Warden && theirs.TrapArmed &&
                tactics.TrapCaution >= HabitThreshold && FirstLivingWarden(attacker.Side) != null)
                return null;

            bool enemyFrontAlive = false;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (u.Alive && u.Side != attacker.Side && u.Row == Row.Front)
                {
                    enemyFrontAlive = true;
                    break;
                }
            }
            bool canReachBack = !melee || attacker.Class == UnitClass.Skirmisher || !enemyFrontAlive;

            if (mine.FocusTicks > 0 && mine.FocusTargetId.Length > 0)
            {
                BattleUnit focus = FindById(mine.FocusTargetId);
                if (focus != null && focus.Alive && focus.Side != attacker.Side &&
                    (focus.Row == Row.Front || canReachBack))
                    return focus;
            }

            // Priorities first, then the most hurt, then the lower index.
            BattleUnit best = null;
            int bestScore = int.MinValue;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit e = _units[i];
                if (!e.Alive || e.Side == attacker.Side) continue;
                if (e.Row == Row.Back && !canReachBack) continue;
                int score = tactics.TargetWeight(e.Class) * 1000 + (1000 - e.HpPermille);
                if (attacker.Class == UnitClass.Skirmisher && e.Row == Row.Back) score += 500;
                // Ranged only: melee that dodge the guarded waste their blows (measured 1 Oct 2026).
                if (!melee && tactics.AvoidGuarded >= HabitThreshold && GuardingWarden(e) != null)
                    score -= tactics.AvoidGuarded * AvoidGuardedScore;
                if (score > bestScore)
                {
                    best = e;
                    bestScore = score;
                }
            }
            return best;
        }

        private BattleUnit FirstLivingWarden(int side)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (u.Alive && u.Side == side && u.Class == UnitClass.Warden) return u;
            }
            return null;
        }

        private BattleUnit GuardingWarden(BattleUnit target)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (u.Alive && u.Side == target.Side && u.Class == UnitClass.Warden && !ReferenceEquals(u, target) &&
                    string.Equals(u.GuardTargetId, target.Id, StringComparison.Ordinal))
                    return u;
            }
            return null;
        }

        private void ApplyDamage(BattleUnit source, int sourceSide, BattleUnit victim, int amount, string detail)
        {
            if (!victim.Alive || amount <= 0) return;
            victim.Hp -= amount;
            Emit(new BattleEvent(_tick, BattleEventKind.Hit, sourceSide,
                source == null ? string.Empty : source.Id, source == null ? -1 : (int)source.Class,
                victim.Id, (int)victim.Class, amount, detail));
            if (victim.Hp <= 0)
            {
                victim.Hp = 0;
                victim.Alive = false;
                victim.GuardTargetId = string.Empty;
                Emit(new BattleEvent(_tick, BattleEventKind.Kill, sourceSide,
                    source == null ? string.Empty : source.Id, source == null ? -1 : (int)source.Class,
                    victim.Id, (int)victim.Class, 0, detail));
            }
        }

        private void Heal(BattleUnit healer, BattleUnit patient, int amount, string detail)
        {
            int healed = Math.Min(amount, patient.MaxHp - patient.Hp);
            if (healed <= 0) return;
            patient.Hp += healed;
            Emit(EventFor(BattleEventKind.Heal, healer, patient, healed, detail));
        }

        private void Sacrifice(BattleUnit victim)
        {
            victim.Hp = 0;
            victim.Alive = false;
            victim.GuardTargetId = string.Empty;
            Emit(EventFor(BattleEventKind.Kill, victim, victim, 0, SacrificeDetail));
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit ally = _units[i];
                if (!ally.Alive || ally.Side != victim.Side) continue;
                Heal(victim, ally, ally.MaxHp * SacrificeHealPercent / 100, SacrificeDetail);
            }
            CheckWipe();
        }

        private void CheckWipe()
        {
            if (_finished) return;
            int alive0 = AliveCount(0);
            int alive1 = AliveCount(1);
            if (alive0 > 0 && alive1 > 0) return;
            Finish(alive0 > 0 ? 0 : (alive1 > 0 ? 1 : -1), "wipe");
        }

        private void EndOnTime()
        {
            int total0 = 0;
            int total1 = 0;
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive) continue;
                if (u.Side == 0) total0 += u.HpPermille;
                else total1 += u.HpPermille;
            }
            Finish(total0 > total1 ? 0 : (total1 > total0 ? 1 : -1), "time");
        }

        private void Finish(int winner, string reason)
        {
            _finished = true;
            _winner = winner;
            _endTick = _tick;
            Emit(new BattleEvent(_tick, BattleEventKind.End, winner, string.Empty, -1, string.Empty, -1, 0, reason));
        }

        private BattleEvent EventFor(BattleEventKind kind, BattleUnit actor, BattleUnit target, int amount, string detail)
        {
            return new BattleEvent(_tick, kind, actor.Side, actor.Id, (int)actor.Class,
                target == null ? string.Empty : target.Id, target == null ? -1 : (int)target.Class, amount, detail);
        }

        private void Emit(BattleEvent e)
        {
            _events.Add(e);
            Hash(e.Tick.ToString(CultureInfo.InvariantCulture));
            Hash(((int)e.Kind).ToString(CultureInfo.InvariantCulture));
            Hash(e.ActorSide.ToString(CultureInfo.InvariantCulture));
            Hash(e.ActorId);
            Hash(e.ActorClass.ToString(CultureInfo.InvariantCulture));
            Hash(e.TargetId);
            Hash(e.TargetClass.ToString(CultureInfo.InvariantCulture));
            Hash(e.Amount.ToString(CultureInfo.InvariantCulture));
            Hash(e.Detail);
        }

        private void Hash(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                _eventHash ^= s[i];
                _eventHash *= FnvPrime;
            }
            _eventHash ^= '|';
            _eventHash *= FnvPrime;
        }
    }
}
