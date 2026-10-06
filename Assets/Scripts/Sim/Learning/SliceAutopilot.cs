using System;
using System.Collections.Generic;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// A scripted player for the slice. The recorded demo, the story seed search, the tests and
    /// the balance numbers all play with it. It only reads the fight and gives orders through the
    /// session, the way a player would, so everything it does replays exactly.
    public sealed class SliceAutopilot
    {
        public enum Style
        {
            /// Answers each of the chief's Eclipses by dodging it with Fall Back, keeps Unleashing
            /// whenever it can spare the points, Frenzies, and makes Ambush and Pyre, stopping
            /// Ambush once the chief has learned to punish Fall Back. Measured on 6 Oct 2026 (see
            /// RunTuning): it used to try to break the cast, keep their Medic focused and wait for
            /// the chief's shields, and together those cost it most of its wins.
            Smart = 0,

            /// Shows each answer once: soaks the first Eclipse with Hold, dodges the second with
            /// Fall Back, then plays as Smart. For the recorded demo.
            Showcase = 1,

            /// The old plan: keeps their Medic focused and Unleashes whenever two points are free.
            /// Never answers an Eclipse, never makes a combo, never Frenzies.
            Passive = 2
        }

        public const int StartTick = 20;

        /// Frenzy goes once the enemy's courage is at or below this.
        public const int FrenzyAtNerve = 700;

        /// Ambush waits this long into a fight, so the lines have met.
        public const int AmbushFromTick = 300;

        /// Fall Back lasts 75 ticks, so to dodge it must go out with this many windup ticks left or fewer.
        public const int DodgeAtWindup = 60;

        private readonly Style _style;
        private long _focusUntil = -1;
        private long _unleashUntil = -1;
        private int _lastEnemyWindup;
        private int _windupsSeen;
        private bool _soaked;
        private bool _dodged;
        private long _ambushFallBackTick = -1;
        private bool _pyreRunning;
        private long _pyreFocusTick = -1;

        public SliceAutopilot(Style style)
        {
            _style = style;
        }

        public Style PlayStyle => _style;

        /// Call once before every tick of the fight.
        public void Step(SimSession session, BattleSimulation sim)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (sim == null) throw new ArgumentNullException(nameof(sim));
            if (sim.IsFinished || sim.CurrentTick < StartTick) return;
            long t = sim.CurrentTick;

            int enemyWindup = sim.WindupTicks(1);
            if (enemyWindup > 0 && _lastEnemyWindup == 0)
            {
                _windupsSeen++;
                _soaked = false;
                _dodged = false;
            }
            _lastEnemyWindup = enemyWindup;

            if (_style != Style.Passive)
            {
                if (!sim.FrenzyUsed(0) && sim.Nerve(1) <= FrenzyAtNerve) Order(session, Directive.Frenzy, string.Empty);

                // Ambush: Unleash straight after the Fall Back, inside the combo's window.
                if (_ambushFallBackTick >= 0)
                {
                    if (t - _ambushFallBackTick > Moves.AmbushWindowTicks || Order(session, Directive.Unleash, string.Empty))
                    {
                        _ambushFallBackTick = -1;
                        _unleashUntil = t + BattleSimulation.UnleashTicks;
                    }
                    return;
                }

                // A Pyre in progress takes every point; the chief's Eclipse is taken on the chin.
                if (_pyreRunning)
                {
                    ContinuePyre(session, sim, t);
                    return;
                }

                if (enemyWindup > 0)
                {
                    AnswerEclipse(session, sim, enemyWindup);
                    return;
                }

                int points = sim.DirectivePoints(0);
                if (sim.Rules(0).EclipseGrade > 0 && !sim.ComboUsed(0, Moves.Pyre) && sim.WindupTicks(0) == 0 &&
                    points >= BattleSetup.DefaultDirectivePoints)
                {
                    StartPyre(session, sim, t);
                    return;
                }
                if (!sim.ComboUsed(0, Moves.Ambush) && t >= AmbushFromTick && !PunishesFallBack(sim) &&
                    points >= Directives.Cost(Directive.FallBack) + Directives.Cost(Directive.Unleash) &&
                    Order(session, Directive.FallBack, string.Empty))
                {
                    _ambushFallBackTick = t;
                    return;
                }
            }

            // The old plan. Passive keeps their Medic focused; the others spend nothing on focus
            // and keep a point spare for dodging the chief's Eclipse.
            int left = sim.DirectivePoints(0);
            if (_style == Style.Passive && t >= _focusUntil && left >= Directives.Cost(Directive.FocusFire))
            {
                BattleUnit target = FirstAlive(sim, 1, UnitClass.Medic, false) ?? MostHurt(sim, 1);
                if (target != null && Order(session, Directive.FocusFire, target.Id))
                {
                    _focusUntil = t + BattleSimulation.FocusTicks;
                    left -= Directives.Cost(Directive.FocusFire);
                }
            }
            int spare = _style == Style.Passive ? 0 : 1;

            // It keeps the pressure on, even into the chief's shields: every Hold he answers with
            // costs him a point and courage, and starves his Eclipse. Waiting for the shields to
            // drop won fewer runs (6 Oct 2026, see RunTuning).
            if (t >= _unleashUntil && left >= Directives.Cost(Directive.Unleash) + spare &&
                Order(session, Directive.Unleash, string.Empty))
                _unleashUntil = t + BattleSimulation.UnleashTicks;
        }

        /// True while the enemy's shields are up, or when he has learned to raise them the moment
        /// you Unleash and has the point to do it. What a player reads off the chief's shields,
        /// his order points and what he is known to answer.
        public static bool EnemyCanBlock(BattleSimulation sim)
        {
            if (sim == null) throw new ArgumentNullException(nameof(sim));
            if (sim.HoldTicksLeft(1) > 0) return true;
            return sim.Setup.CopyTactics(1).HoldOnUnleash >= BattleSimulation.HabitThreshold &&
                sim.DirectivePoints(1) >= Directives.Cost(Directive.Hold);
        }

        /// True once the enemy chief has learned to answer Fall Back with Unleash: then Ambush
        /// hands him a free Unleash, so the autopilot stops making it.
        private static bool PunishesFallBack(BattleSimulation sim)
        {
            return sim.Setup.CopyTactics(1).PressOnFallBack >= BattleSimulation.HabitThreshold;
        }

        /// Soaks the first Eclipse with Hold when showing off, otherwise dodges it with Fall Back
        /// near the end of the windup. It used to focus fire on the chief first to try to break
        /// his cast; measured on 6 Oct 2026, that won no more runs.
        private void AnswerEclipse(SimSession session, BattleSimulation sim, int windup)
        {
            if (_style == Style.Showcase && _windupsSeen == 1)
            {
                if (!_soaked && Order(session, Directive.Hold, string.Empty)) _soaked = true;
                return;
            }
            if (!_dodged && windup <= DodgeAtWindup && Order(session, Directive.FallBack, string.Empty)) _dodged = true;
        }

        /// Focus Fire on their chief (or Medic), Unleash, then Eclipse once three points are back.
        private void StartPyre(SimSession session, BattleSimulation sim, long t)
        {
            BattleUnit target = sim.Leader(1) ?? FirstAlive(sim, 1, UnitClass.Medic, false) ?? MostHurt(sim, 1);
            if (target == null || !Order(session, Directive.FocusFire, target.Id)) return;
            Order(session, Directive.Unleash, string.Empty);
            _pyreRunning = true;
            _pyreFocusTick = t;
            _focusUntil = t + BattleSimulation.FocusTicks;
            _unleashUntil = t + BattleSimulation.UnleashTicks;
        }

        private void ContinuePyre(SimSession session, BattleSimulation sim, long t)
        {
            if (t - _pyreFocusTick > Moves.PyreWindowTicks)
            {
                _pyreRunning = false;
                return;
            }
            if (sim.CanUse(0, Directive.Eclipse, out _) && Order(session, Directive.Eclipse, string.Empty)) _pyreRunning = false;
        }

        private static bool Order(SimSession session, Directive directive, string targetId)
        {
            return session.TryIssue(Directives.CommandKind, "0", targetId, 0, 0, Directives.Name(directive), out _);
        }

        private static BattleUnit FirstAlive(BattleSimulation sim, int side, UnitClass unitClass, bool leaders)
        {
            for (int i = 0; i < sim.Units.Count; i++)
            {
                BattleUnit u = sim.Units[i];
                if (u.Side == side && u.Alive && u.Class == unitClass && u.IsLeader == leaders) return u;
            }
            return null;
        }

        private static BattleUnit MostHurt(BattleSimulation sim, int side)
        {
            BattleUnit best = null;
            for (int i = 0; i < sim.Units.Count; i++)
            {
                BattleUnit u = sim.Units[i];
                if (u.Side == side && u.Alive && (best == null || u.HpPermille < best.HpPermille)) best = u;
            }
            return best;
        }

        // ------------------------------------------------------------ whole runs, headless

        /// Plays a whole run with the autopilot and the default squads, wars included, and counts
        /// what happened. Uses and changes the memory, like a real run.
        public static RunOutcome PlayRun(RunMemory memory, ulong seed, Style style)
        {
            return PlayRun(memory, seed, style, null);
        }

        /// The same with these numbers (null for the defaults).
        public static RunOutcome PlayRun(RunMemory memory, ulong seed, Style style, RunTuning tuning)
        {
            SliceRun run = SliceRun.Start(memory, seed, tuning);
            var outcome = new RunOutcome(run);
            while (!run.Over)
            {
                if (run.WarPending) run.RunWar();
                BattleSetup setup = run.NextFight(run.DefaultSquad());
                var sim = new BattleSimulation(setup);
                var session = new SimSession(sim);
                var pilot = new SliceAutopilot(style);
                while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
                {
                    pilot.Step(session, sim);
                    session.Tick();
                }
                outcome.Add(run.FinishFight(setup, session));
            }
            return outcome;
        }
    }

    /// What one headless run did, counted from its fights' events.
    public sealed class RunOutcome
    {
        private readonly List<RunFightResult> _fights = new List<RunFightResult>();
        private readonly List<long> _fightTicks = new List<long>();

        internal RunOutcome(SliceRun run)
        {
            Run = run;
        }

        public SliceRun Run { get; }
        public IReadOnlyList<RunFightResult> Fights => _fights;
        public IReadOnlyList<long> FightTicks => _fightTicks;
        public bool Won => Run.Stage == RunStage.Won;
        public int ChiefEclipses { get; private set; }
        public int Soaked { get; private set; }
        public int Dodged { get; private set; }
        public int TakenFull { get; private set; }
        public int Interrupts { get; private set; }
        public int Frenzies { get; private set; }
        public int Ambushes { get; private set; }
        public int Pyres { get; private set; }
        public int PlayerEclipses { get; private set; }
        public int LeaderBreaks { get; private set; }

        internal void Add(RunFightResult result)
        {
            _fights.Add(result);
            _fightTicks.Add(result.Record.EndTick);
            IReadOnlyList<BattleEvent> events = result.Record.Events;
            for (int i = 0; i < events.Count; i++)
            {
                BattleEvent e = events[i];
                switch (e.Kind)
                {
                    case BattleEventKind.Windup:
                        if (e.ActorSide == 1) ChiefEclipses++;
                        else PlayerEclipses++;
                        break;
                    case BattleEventKind.Release:
                        if (e.ActorSide != 1) break;
                        if (e.Detail == Moves.Soaked) Soaked++;
                        else if (e.Detail == Moves.Dodged) Dodged++;
                        else TakenFull++;
                        break;
                    case BattleEventKind.Interrupt:
                        if (e.ActorSide == 0) Interrupts++;
                        break;
                    case BattleEventKind.Transform:
                        if (e.ActorSide == 0) Frenzies++;
                        break;
                    case BattleEventKind.Combo:
                        if (e.ActorSide != 0) break;
                        if (e.Detail == Moves.Ambush) Ambushes++;
                        else if (e.Detail == Moves.Pyre) Pyres++;
                        break;
                    case BattleEventKind.Break:
                        if (e.Detail == Moves.LeaderDetail) LeaderBreaks++;
                        break;
                }
            }
        }
    }
}
