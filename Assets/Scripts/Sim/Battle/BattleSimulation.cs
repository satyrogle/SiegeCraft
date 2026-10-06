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
    ///
    /// Under the nerve rules (BattleSetup.Nerve, Jacob 2 Oct 2026) each side also has a nerve
    /// bar: hits and deaths drain it, the enemy's Unleash and Sacrifice drain it, and Hold drains
    /// the holder's own. A side at zero breaks and the fight ends there with its survivors alive.
    /// The nerve rules use no randomness, so with them off every fight is exactly as before.
    ///
    /// The slice's newer rules (Jacob, 2 and 3 Oct 2026) add two more switches on the setup:
    /// orders refill during the fight (a point back every RefillTicks ticks, up to the starting
    /// points) so the player stays busy, and HealthPercent makes units tougher so a fight lasts
    /// longer. Both are off by default and use no randomness either.
    ///
    /// The slice's big moves (Jacob, 4 Oct 2026, see Moves) come from the setup's side rules and
    /// unit roles: Eclipse winds up and can be soaked, dodged or interrupted, a Pal can Frenzy
    /// once a fight, orders fuse into combos, and a side whose Pal or Chief drops breaks on the
    /// spot. A unit whose gear frightens (UnitSpec.FearPercent) drains more courage with its hits.
    /// None of it draws on the random stream, and a fight without side rules, roles or fear
    /// never meets any of it.
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

        /// Nerve rules, placeholders. Losing a side's whole starting health to hits costs this
        /// percent of a full bar; each death costs NerveDeathCost on top.
        public const int NerveHitPercent = 70;
        public const int NerveDeathCost = 200;

        /// Scary orders: what the enemy's nerve loses when you Unleash or Sacrifice. A fight's
        /// nerve rules can price Unleash differently (NerveRules.UnleashCost).
        public const int UnleashNerveCost = 100;
        public const int SacrificeNerveCost = 150;

        /// Holding costs the holder this much nerve every tick it runs (150 for a full Hold), or
        /// every NerveRules.HoldDrainEveryTicks ticks when the fight's nerve rules say so.
        public const int HoldNervePerTick = 1;

        public const string BreakDetail = "break";

        private const ulong FnvOffset = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        private readonly BattleSetup _setup;
        private readonly TacticsProfile[] _tactics;
        private readonly SideRules[] _rules;
        private readonly DeterministicRng _rng;
        private readonly List<BattleUnit> _units = new List<BattleUnit>();
        private readonly List<BattleEvent> _events = new List<BattleEvent>();
        private readonly SideState[] _sides = { new SideState(), new SideState() };
        private readonly int[] _trapReadyIn;
        private readonly int[] _fear;
        private readonly bool _nerveOn;
        private readonly bool _movesOn;
        private readonly int _refillTicks;
        private readonly int _pointCap;
        private long _tick;
        private long _endTick = -1;
        private bool _finished;
        private int _winner = -1;
        private int _brokenSide = -1;
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
            public int Nerve;
            public int StartHp;
            public int NerveRemainder;

            // Big moves (4 Oct 2026).
            public int Windup;
            public string CasterId = string.Empty;
            public int WindupDamage;
            public int WindupPercent;
            public bool WindupPyre;
            public string PyreTargetId = string.Empty;
            public long NextAutoEclipse;
            public bool Empowered;
            public int FrenzyTicks;
            public bool FrenzyUsed;
            public bool AmbushUsed;
            public bool PyreUsed;
            public long LastFallBackTick = -1;
            public long LastFocusTick = -1;
            public string LastFocusId = string.Empty;
            public long LastUnleashTick = -1;
        }

        public BattleSimulation(BattleSetup setup)
        {
            _setup = setup ?? throw new ArgumentNullException(nameof(setup));
            _tactics = new[] { setup.CopyTactics(0), setup.CopyTactics(1) };
            _rules = new[] { setup.Rules(0), setup.Rules(1) };
            _rng = DeterministicRng.ForStream(setup.Seed, SimStream.Battle);

            int index = 0;
            bool special = false;
            var fear = new List<int>();
            for (int side = 0; side < 2; side++)
            {
                IReadOnlyList<UnitSpec> specs = setup.Side(side);
                for (int i = 0; i < specs.Count; i++)
                {
                    UnitSpec spec = specs[i];
                    if (spec.Id.Length == 0) throw new ArgumentException("Every unit needs an id.");
                    if (FindById(spec.Id) != null) throw new ArgumentException("Duplicate unit id " + spec.Id + ".");
                    _units.Add(new BattleUnit(spec, side, index++, setup.HealthPercent));
                    fear.Add(spec.FearPercent);
                    if (spec.Role != UnitRole.Fighter || spec.PowerPercent != UnitSpec.PlainPercent ||
                        spec.HealthPercent != UnitSpec.PlainPercent || spec.FearPercent != 0)
                        special = true;
                }
            }
            _trapReadyIn = new int[_units.Count];
            _fear = fear.ToArray();
            _movesOn = special || !_rules[0].IsNone || !_rules[1].IsNone;

            _sides[0].Points = setup.StartingDirectivePoints;
            _sides[1].Points = setup.StartingDirectivePoints;
            _refillTicks = setup.RefillTicks;
            _pointCap = setup.StartingDirectivePoints;

            _nerveOn = setup.Nerve.Enabled;
            for (int i = 0; i < _units.Count; i++) _sides[_units[i].Side].StartHp += _units[i].MaxHp;
            _sides[0].Nerve = setup.Nerve.StartNerve(0);
            _sides[1].Nerve = setup.Nerve.StartNerve(1);
            _sides[0].NextAutoEclipse = _rules[0].AutoEclipseTicks;
            _sides[1].NextAutoEclipse = _rules[1].AutoEclipseTicks;

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

        /// True when this fight runs under the nerve rules.
        public bool NerveOn => _nerveOn;

        /// The side whose nerve broke, or -1. Only a fight under the nerve rules can end in a break,
        /// unless a side's Pal or Chief drops.
        public int BrokenSide => _brokenSide;

        /// A side's nerve in thousandths of a full bar. Stays full when the nerve rules are off.
        public int Nerve(int side)
        {
            return _sides[side].Nerve;
        }

        public int DirectivePoints(int side)
        {
            return _sides[side].Points;
        }

        /// Percent added to the courage this unit's hits drain. 0 unless its gear frightens.
        public int FearPercent(BattleUnit unit)
        {
            return _fear[unit.Index];
        }

        /// A side's big moves for this fight.
        public SideRules Rules(int side)
        {
            return _rules[side];
        }

        /// Ticks left before a side's wound up move goes off, or 0 when nothing is winding up.
        public int WindupTicks(int side)
        {
            return _sides[side].Windup;
        }

        /// The unit winding up a side's move, or empty.
        public string WindupCasterId(int side)
        {
            return _sides[side].CasterId;
        }

        /// True when the move winding up finishes a Pyre.
        public bool WindupIsPyre(int side)
        {
            return _sides[side].WindupPyre;
        }

        /// The Pyre's target while one is winding up, or empty.
        public string PyreTargetId(int side)
        {
            return _sides[side].PyreTargetId;
        }

        /// The side interrupted a move and its next order is doubled.
        public bool IsEmpowered(int side)
        {
            return _sides[side].Empowered;
        }

        /// Ticks of Frenzy left, or 0.
        public int FrenzyTicksLeft(int side)
        {
            return _sides[side].FrenzyTicks;
        }

        public bool FrenzyUsed(int side)
        {
            return _sides[side].FrenzyUsed;
        }

        /// True once the side has made this combo (Moves.Ambush or Moves.Pyre) this fight.
        public bool ComboUsed(int side, string combo)
        {
            if (string.Equals(combo, Moves.Ambush, StringComparison.Ordinal)) return _sides[side].AmbushUsed;
            if (string.Equals(combo, Moves.Pyre, StringComparison.Ordinal)) return _sides[side].PyreUsed;
            return false;
        }

        /// The side's living Pal or Chief, or null. Eclipse is cast by this unit.
        public BattleUnit Leader(int side)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (u.Alive && u.Side == side && u.IsLeader) return u;
            }
            return null;
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

        /// Whether a side could give this order now, targets aside. The same checks TryApply makes.
        public bool CanUse(int side, Directive directive, out string failure)
        {
            failure = string.Empty;
            if (_finished)
            {
                failure = "The battle is over.";
                return false;
            }
            // What the order needs comes before what it costs, so the reason given is the real one.
            SideState state = _sides[side];
            if (directive == Directive.Eclipse)
            {
                if (_rules[side].EclipseGrade <= 0)
                {
                    failure = "Eclipse is not yours yet.";
                    return false;
                }
                if (Leader(side) == null)
                {
                    failure = "Eclipse needs a living leader to cast it.";
                    return false;
                }
                if (state.Windup > 0)
                {
                    failure = "Eclipse is already winding up.";
                    return false;
                }
            }
            else if (directive == Directive.Frenzy)
            {
                if (LivingPal(side) == null)
                {
                    failure = "Frenzy needs a living Pal.";
                    return false;
                }
                if (state.FrenzyUsed)
                {
                    failure = "Frenzy is used up this fight.";
                    return false;
                }
            }
            int cost = Directives.Cost(directive);
            if (state.Points < cost)
            {
                failure = "Not enough directive points: need " + cost + ", have " + state.Points + ".";
                return false;
            }
            return true;
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

                // A chief casts his Eclipse on his own schedule, once he has the points.
                for (int s = 0; s < 2 && !_finished; s++) TryAutoEclipse(s);

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

                // Holding costs nerve: the longer a side digs in, the shakier it gets.
                if (_nerveOn)
                {
                    for (int s = 0; s < 2 && !_finished; s++)
                    {
                        if (_sides[s].HoldTicks > 0 && _tick % _setup.Nerve.HoldDrainEveryTicks == 0)
                            DrainNerve(s, HoldNervePerTick, s);
                    }
                }

                // Refilling orders: a point back every RefillTicks ticks, up to the starting points.
                if (_refillTicks > 0 && !_finished && (_tick + 1) % _refillTicks == 0)
                {
                    for (int s = 0; s < 2; s++)
                    {
                        if (_sides[s].Points < _pointCap) _sides[s].Points++;
                    }
                }

                // Wound up moves go off once their windup has run, before this tick's effects
                // count down, so an order in force on this tick still counts against them.
                for (int s = 0; s < 2 && !_finished; s++) AdvanceWindup(s);

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

            if (!CanUse(side, directive, out failure)) return false;

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
                if (target.IsLeader)
                {
                    failure = "A leader cannot be sacrificed.";
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
                if (_nerveOn)
                {
                    b.Field("nerveOn", true);
                    b.Field("brokenSide", _brokenSide);
                    if (_setup.Nerve.UnleashCost != UnleashNerveCost || _setup.Nerve.HoldDrainEveryTicks != 1)
                    {
                        b.Field("unleashNerve", _setup.Nerve.UnleashCost);
                        b.Field("holdDrainEvery", _setup.Nerve.HoldDrainEveryTicks);
                    }
                }
                if (_refillTicks > 0) b.Field("refill", _refillTicks);
                if (_setup.HealthPercent != BattleSetup.NormalHealthPercent) b.Field("health", _setup.HealthPercent);
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
                    if (_nerveOn)
                    {
                        b.Field("nerve", state.Nerve);
                        b.Field("nerveRem", state.NerveRemainder);
                    }
                    if (_movesOn)
                    {
                        _rules[s].AppendTo(b, name + ".");
                        b.Field("windup", state.Windup);
                        b.Field("caster", state.CasterId);
                        b.Field("windupDamage", state.WindupDamage);
                        b.Field("windupPercent", state.WindupPercent);
                        b.Field("windupPyre", state.WindupPyre);
                        b.Field("pyreTarget", state.PyreTargetId);
                        b.Field("nextAutoEclipse", state.NextAutoEclipse);
                        b.Field("empowered", state.Empowered);
                        b.Field("frenzy", state.FrenzyTicks);
                        b.Field("frenzyUsed", state.FrenzyUsed);
                        b.Field("ambushUsed", state.AmbushUsed);
                        b.Field("pyreUsed", state.PyreUsed);
                        b.Field("lastFallBack", state.LastFallBackTick);
                        b.Field("lastFocus", state.LastFocusTick);
                        b.Field("lastFocusId", state.LastFocusId);
                        b.Field("lastUnleash", state.LastUnleashTick);
                    }
                    _tactics[s].AppendTo(b, name + ".");
                }
                b.Section("units");
                for (int i = 0; i < _units.Count; i++)
                {
                    BattleUnit u = _units[i];
                    // Who each unit is, not only how it is doing, so two different line ups never
                    // share a checksum (Codex audit, 2 Oct 2026, finding 8).
                    b.Field(u.Id + ".class", SliceClasses.Name(u.Class));
                    b.Field(u.Id + ".row", (int)u.Row);
                    b.Field(u.Id + ".role", (int)u.Role);
                    b.Field(u.Id + ".power", u.PowerPercent);
                    b.Field(u.Id + ".fear", _fear[i]);
                    b.Field(u.Id + ".maxHp", u.MaxHp);
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
            if (_finished) return;
            SideState state = _sides[side];
            int cost = Directives.Cost(directive);
            state.Points -= cost;
            Emit(new BattleEvent(_tick, BattleEventKind.Directive, side, actorId, -1,
                target == null ? string.Empty : target.Id, target == null ? -1 : (int)target.Class, cost, Directives.Name(directive)));

            // Only orders the player gives count toward combos; reflexes and the chief's own casts do not.
            bool ordered = !actorId.StartsWith(AutoActorPrefix, StringComparison.Ordinal);

            // An interrupt doubled this order.
            int percent = 100;
            if (state.Empowered)
            {
                state.Empowered = false;
                percent = Moves.EmpoweredPercent;
                Emit(new BattleEvent(_tick, BattleEventKind.Empowered, side, actorId, -1,
                    target == null ? string.Empty : target.Id, target == null ? -1 : (int)target.Class, percent, Directives.Name(directive)));
            }

            switch (directive)
            {
                case Directive.Hold:
                    state.HoldTicks = HoldTicks * percent / 100;
                    break;
                case Directive.FallBack:
                    state.FallBackTicks = FallBackTicks * percent / 100;
                    if (ordered) state.LastFallBackTick = _tick;
                    ReflexPressOnFallBack(1 - side);
                    break;
                case Directive.FocusFire:
                    state.FocusTargetId = target.Id;
                    state.FocusTicks = FocusTicks * percent / 100;
                    if (ordered)
                    {
                        state.LastFocusTick = _tick;
                        state.LastFocusId = target.Id;
                    }
                    ReflexCoverFocused(1 - side, target);
                    break;
                case Directive.Unleash:
                    state.UnleashTicks = UnleashTicks * percent / 100;
                    if (ordered)
                    {
                        TryAmbush(side);
                        state.LastUnleashTick = _tick;
                    }
                    ReflexHoldOnUnleash(1 - side);
                    if (_nerveOn) UnleashScare(side);
                    break;
                case Directive.Sacrifice:
                    Sacrifice(target, percent);
                    if (_nerveOn) DrainNerve(1 - side, SacrificeNerveCost, side);
                    break;
                case Directive.Eclipse:
                    StartEclipse(side, percent, ordered);
                    break;
                case Directive.Frenzy:
                    BattleUnit pal = LivingPal(side);
                    state.FrenzyTicks = Moves.FrenzyTicks * percent / 100;
                    state.FrenzyUsed = true;
                    Emit(new BattleEvent(_tick, BattleEventKind.Transform, side, pal.Id, (int)pal.Class,
                        string.Empty, -1, state.FrenzyTicks, Directives.Name(Directive.Frenzy)));
                    break;
            }
        }

        /// Detail of the event a side gets when its raised shields block an Unleash: actor side
        /// is the side that held, amount the courage it kept.
        public const string BlockedDetail = "blocked";

        /// Ticks of Hold left on a side, or 0. Its shields are up while this runs.
        public int HoldTicksLeft(int side)
        {
            return _sides[side].HoldTicks;
        }

        /// Ticks of Unleash left on a side, or 0.
        public int UnleashTicksLeft(int side)
        {
            return _sides[side].UnleashTicks;
        }

        /// Unleash's scare (Jacob, 6 Oct 2026). It drains the enemy's courage, unless the enemy is
        /// holding: an Unleash into raised shields scares nobody, and against the held front row
        /// its extra damage is all but cancelled by the Hold. So it pays to wait for the shields
        /// to drop, and a chief who learned to Hold the moment you Unleash blocks you while he
        /// has a point to spend. Measured against Codex's audit of the same day, where pressing
        /// Unleash whenever it was affordable won every run.
        private void UnleashScare(int side)
        {
            int enemy = 1 - side;
            if (_sides[enemy].HoldTicks > 0)
            {
                Emit(new BattleEvent(_tick, BattleEventKind.Directive, enemy, AutoActor(enemy), -1,
                    string.Empty, -1, _setup.Nerve.UnleashCost, BlockedDetail));
                return;
            }
            DrainNerve(enemy, _setup.Nerve.UnleashCost, side);
        }

        /// Learned: our Warden covers whoever the enemy just focused.
        private void ReflexCoverFocused(int side, BattleUnit focused)
        {
            if (_finished) return;
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
            if (_finished || _tactics[side].HoldOnUnleash < HabitThreshold) return;
            if (_sides[side].Points < Directives.Cost(Directive.Hold) || _sides[side].HoldTicks > 0) return;
            ApplyDirective(side, Directive.Hold, null, AutoActor(side));
        }

        /// Learned: when the enemy falls back, nobody is in melee, so our ranged fighters unleash.
        private void ReflexPressOnFallBack(int side)
        {
            if (_finished || _tactics[side].PressOnFallBack < HabitThreshold) return;
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
            if (state.FrenzyTicks > 0) state.FrenzyTicks--;
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

        // ------------------------------------------------------------ big moves

        /// The chief's own Eclipse: due every AutoEclipseTicks, cast as soon as it can be.
        private void TryAutoEclipse(int side)
        {
            SideRules rules = _rules[side];
            SideState state = _sides[side];
            if (rules.AutoEclipseTicks <= 0 || rules.EclipseGrade <= 0 || _tick < state.NextAutoEclipse) return;
            if (!CanUse(side, Directive.Eclipse, out _)) return;
            ApplyDirective(side, Directive.Eclipse, null, AutoActor(side));
            state.NextAutoEclipse = _tick + rules.AutoEclipseTicks;
        }

        /// The side's leader starts winding up. If the player's last orders were Focus Fire then
        /// Unleash, close enough together, this Eclipse finishes a Pyre on the focused target.
        private void StartEclipse(int side, int percent, bool ordered)
        {
            SideState state = _sides[side];
            BattleUnit caster = Leader(side);
            state.Windup = Moves.EclipseWindupTicks;
            state.CasterId = caster.Id;
            state.WindupDamage = 0;
            state.WindupPercent = Moves.EclipsePercent(_rules[side].EclipseGrade) * percent / 100;
            state.WindupPyre = false;
            state.PyreTargetId = string.Empty;
            if (ordered && _rules[side].Combos && !state.PyreUsed && state.LastFocusTick >= 0 &&
                state.LastUnleashTick >= state.LastFocusTick && _tick - state.LastFocusTick <= Moves.PyreWindowTicks)
            {
                BattleUnit focused = FindById(state.LastFocusId);
                if (focused != null && focused.Alive)
                {
                    state.WindupPyre = true;
                    state.PyreTargetId = focused.Id;
                    state.PyreUsed = true;
                }
            }
            BattleUnit pyreTarget = state.WindupPyre ? FindById(state.PyreTargetId) : null;
            Emit(new BattleEvent(_tick, BattleEventKind.Windup, side, caster.Id, (int)caster.Class,
                pyreTarget == null ? string.Empty : pyreTarget.Id, pyreTarget == null ? -1 : (int)pyreTarget.Class,
                Moves.EclipseWindupTicks, state.WindupPyre ? Moves.Pyre : Directives.Name(Directive.Eclipse)));
        }

        private void AdvanceWindup(int side)
        {
            SideState state = _sides[side];
            if (state.Windup <= 0) return;
            BattleUnit caster = FindById(state.CasterId);
            if (caster == null || !caster.Alive)
            {
                ClearWindup(state);
                return;
            }
            state.Windup--;
            if (state.Windup == 0) ReleaseEclipse(side, caster);
        }

        /// The Eclipse goes off on the enemy front row (the back row once the front has fallen).
        /// Fall Back dodges it, Hold soaks half of it and half of its nerve cost.
        private void ReleaseEclipse(int side, BattleUnit caster)
        {
            SideState state = _sides[side];
            int enemy = 1 - side;
            SideState theirs = _sides[enemy];
            int percent = state.WindupPercent;
            bool pyre = state.WindupPyre;
            string pyreTargetId = state.PyreTargetId;
            ClearWindup(state);

            bool dodged = theirs.FallBackTicks > 0;
            bool soaked = !dodged && theirs.HoldTicks > 0;
            Emit(new BattleEvent(_tick, BattleEventKind.Release, side, caster.Id, (int)caster.Class, string.Empty, -1,
                percent, dodged ? Moves.Dodged : soaked ? Moves.Soaked : Moves.Full));
            if (dodged) return;

            Row row = AnyAlive(enemy, Row.Front) ? Row.Front : Row.Back;
            for (int i = 0; i < _units.Count && !_finished; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive || u.Side != enemy || u.Row != row) continue;
                int damage = u.MaxHp * percent / 100;
                if (soaked) damage = damage * Moves.EclipseHoldPercent / 100;
                ApplyDamage(caster, side, u, Math.Max(1, damage), Moves.EclipseDetail);
            }
            if (!_finished)
                DrainNerve(enemy, soaked ? Moves.EclipseNerveCost * Moves.EclipseHoldPercent / 100 : Moves.EclipseNerveCost, side);

            if (pyre && !_finished)
            {
                BattleUnit target = FindById(pyreTargetId);
                if (target != null && target.Alive)
                {
                    int burn = Math.Max(1, target.MaxHp * Moves.PyrePercent / 100);
                    Emit(new BattleEvent(_tick, BattleEventKind.Combo, side, caster.Id, (int)caster.Class,
                        target.Id, (int)target.Class, burn, Moves.Pyre));
                    ApplyDamage(caster, side, target, burn, Moves.PyreDetail);
                    if (!_finished) DrainNerve(enemy, Moves.PyreNerveCost, side);
                }
            }
            CheckWipe();
        }

        /// Fall Back then Unleash, close together: the enemy back row (the front row once the back
        /// has fallen) is caught in the open.
        private void TryAmbush(int side)
        {
            SideState state = _sides[side];
            if (!_rules[side].Combos || state.AmbushUsed || state.LastFallBackTick < 0 ||
                _tick - state.LastFallBackTick > Moves.AmbushWindowTicks)
                return;
            state.AmbushUsed = true;
            int enemy = 1 - side;
            Row row = AnyAlive(enemy, Row.Back) ? Row.Back : Row.Front;
            Emit(new BattleEvent(_tick, BattleEventKind.Combo, side, string.Empty, -1, string.Empty, -1,
                Moves.AmbushPercent, Moves.Ambush));
            for (int i = 0; i < _units.Count && !_finished; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive || u.Side != enemy || u.Row != row) continue;
                ApplyDamage(null, side, u, Math.Max(1, u.MaxHp * Moves.AmbushPercent / 100), Moves.AmbushDetail);
            }
            if (!_finished) DrainNerve(enemy, Moves.AmbushNerveCost, side);
            CheckWipe();
        }

        /// The caster was hit hard enough: the move is lost and the other side's next order is doubled.
        private void Interrupt(BattleUnit caster, BattleUnit source)
        {
            SideState state = _sides[caster.Side];
            int taken = state.WindupDamage;
            bool pyre = state.WindupPyre;
            ClearWindup(state);
            int interrupter = 1 - caster.Side;
            _sides[interrupter].Empowered = true;
            Emit(new BattleEvent(_tick, BattleEventKind.Interrupt, interrupter,
                source == null ? string.Empty : source.Id, source == null ? -1 : (int)source.Class,
                caster.Id, (int)caster.Class, taken, pyre ? Moves.Pyre : Directives.Name(Directive.Eclipse)));
        }

        private static void ClearWindup(SideState state)
        {
            state.Windup = 0;
            state.CasterId = string.Empty;
            state.WindupDamage = 0;
            state.WindupPercent = 0;
            state.WindupPyre = false;
            state.PyreTargetId = string.Empty;
        }

        private BattleUnit LivingPal(int side)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (u.Alive && u.Side == side && u.Role == UnitRole.Pal) return u;
            }
            return null;
        }

        private bool AnyAlive(int side, Row row)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (u.Alive && u.Side == side && u.Row == row) return true;
            }
            return false;
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
                    Heal(unit, patient, unit.Power, string.Empty);
                    return;
                }
                Attack(unit, Math.Max(1, SliceClasses.MedicFallbackAttack * unit.PowerPercent / 100));
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

            Attack(unit, unit.Power);
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
            if (mine.FrenzyTicks > 0)
                damage = damage * (attacker.Role == UnitRole.Pal ? Moves.FrenzyPalPowerPercent : Moves.FrenzyAllyPowerPercent) / 100;
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
                // Under the nerve rules the trap can break the attacker's side; then the blow never lands.
                if (!attacker.Alive || _finished) return;
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
            if (_finished || !victim.Alive || amount <= 0) return;
            int hpLost = Math.Min(amount, victim.Hp);
            victim.Hp -= amount;
            Emit(new BattleEvent(_tick, BattleEventKind.Hit, sourceSide,
                source == null ? string.Empty : source.Id, source == null ? -1 : (int)source.Class,
                victim.Id, (int)victim.Class, amount, detail));
            bool killed = false;
            if (victim.Hp <= 0)
            {
                victim.Hp = 0;
                victim.Alive = false;
                victim.GuardTargetId = string.Empty;
                Emit(new BattleEvent(_tick, BattleEventKind.Kill, sourceSide,
                    source == null ? string.Empty : source.Id, source == null ? -1 : (int)source.Class,
                    victim.Id, (int)victim.Class, 0, detail));
                killed = true;
            }

            // A caster hit hard during its windup loses the move.
            SideState victimState = _sides[victim.Side];
            if (!killed && victimState.Windup > 0 && string.Equals(victimState.CasterId, victim.Id, StringComparison.Ordinal))
            {
                victimState.WindupDamage += hpLost;
                if (victimState.WindupDamage * 100 >= Moves.InterruptPercent * victim.MaxHp) Interrupt(victim, source);
            }

            if (_nerveOn)
            {
                // A frightening attacker's hits shake the victim's side harder.
                int shaken = source == null ? hpLost : hpLost * (100 + _fear[source.Index]) / 100;
                DrainForHit(victim.Side, shaken, sourceSide);
                if (killed) DrainNerve(victim.Side, NerveDeathCost, sourceSide);
            }

            // A side whose Pal or Chief drops breaks on the spot.
            if (killed && victim.IsLeader) LeaderDown(victim.Side);
        }

        private void Heal(BattleUnit healer, BattleUnit patient, int amount, string detail)
        {
            int healed = Math.Min(amount, patient.MaxHp - patient.Hp);
            if (healed <= 0) return;
            patient.Hp += healed;
            Emit(EventFor(BattleEventKind.Heal, healer, patient, healed, detail));
        }

        /// A chosen death: it heals the allies and costs your own side no nerve. An empowered
        /// Sacrifice heals twice as much.
        private void Sacrifice(BattleUnit victim, int percent)
        {
            victim.Hp = 0;
            victim.Alive = false;
            victim.GuardTargetId = string.Empty;
            Emit(EventFor(BattleEventKind.Kill, victim, victim, 0, SacrificeDetail));
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit ally = _units[i];
                if (!ally.Alive || ally.Side != victim.Side) continue;
                Heal(victim, ally, ally.MaxHp * SacrificeHealPercent / 100 * percent / 100, SacrificeDetail);
            }
            CheckWipe();
        }

        // ------------------------------------------------------------ nerve

        /// Hits drain nerve in proportion to the side's starting health: losing all of it costs
        /// NerveHitPercent of a full bar. The remainder carries, so small hits add up exactly.
        private void DrainForHit(int side, int hpLost, int causedBySide)
        {
            SideState state = _sides[side];
            if (hpLost <= 0 || state.StartHp <= 0) return;
            int divisor = 100 * state.StartHp;
            int scaled = hpLost * NerveHitPercent * NerveRules.Full + state.NerveRemainder;
            state.NerveRemainder = scaled % divisor;
            DrainNerve(side, scaled / divisor, causedBySide);
        }

        /// Takes nerve from a side. What the enemy causes is raised by the enemy's fear bonus.
        /// At zero the side breaks, unless nobody is left standing: that is a wipe, not a break.
        private void DrainNerve(int side, int amount, int causedBySide)
        {
            if (!_nerveOn || _finished || amount <= 0) return;
            if (causedBySide == 1 - side)
                amount = amount * (100 + _setup.Nerve.FearBonusPercent(causedBySide)) / 100;
            SideState state = _sides[side];
            state.Nerve -= amount;
            if (state.Nerve > 0) return;
            state.Nerve = 0;
            if (AliveCount(side) == 0) return;
            BreakSide(side, BreakDetail);
        }

        /// The side's Pal or Chief dropped: its nerve is gone and it breaks, unless nobody is left.
        private void LeaderDown(int side)
        {
            if (_finished) return;
            if (AliveCount(side) == 0)
            {
                CheckWipe();
                return;
            }
            _sides[side].Nerve = 0;
            BreakSide(side, Moves.LeaderDetail);
        }

        private void BreakSide(int side, string detail)
        {
            _brokenSide = side;
            Emit(new BattleEvent(_tick, BattleEventKind.Break, side, string.Empty, -1, string.Empty, -1,
                AliveCount(side), detail));
            Finish(1 - side, detail);
        }

        // ------------------------------------------------------------ endings

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
