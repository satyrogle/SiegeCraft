using System;

namespace KindredSiege.Sim
{
    public readonly struct ReplayResult
    {
        public ReplayResult(bool success, string failure, string checksum, long ticks, int commandsApplied)
        {
            Success = success;
            Failure = failure ?? string.Empty;
            Checksum = checksum ?? string.Empty;
            Ticks = ticks;
            CommandsApplied = commandsApplied;
        }

        public bool Success { get; }
        public string Failure { get; }
        public string Checksum { get; }
        public long Ticks { get; }
        public int CommandsApplied { get; }
    }

    /// Feeds a command log into a fresh simulation, tick by tick, and returns the final checksum.
    /// Commands on tick T are applied before tick T runs, in log order.
    public static class ReplayRunner
    {
        public static ReplayResult Run(ISimulation simulation, SimCommandLog log, long tickCount)
        {
            if (simulation == null) throw new ArgumentNullException(nameof(simulation));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (tickCount < 0) throw new ArgumentOutOfRangeException(nameof(tickCount));

            var commands = log.Commands;
            int index = 0;
            for (long t = 0; t < tickCount; t++)
            {
                while (index < commands.Count && commands[index].Tick <= simulation.CurrentTick)
                {
                    SimCommand command = commands[index];
                    if (command.Tick < simulation.CurrentTick)
                    {
                        return new ReplayResult(false,
                            "Command " + command.Sequence + " is stamped tick " + command.Tick +
                            " but the simulation is already at tick " + simulation.CurrentTick + ".",
                            string.Empty, simulation.CurrentTick, index);
                    }
                    if (!simulation.TryApply(command, out string failure))
                    {
                        return new ReplayResult(false,
                            "Command " + command.Sequence + " rejected: " + failure,
                            string.Empty, simulation.CurrentTick, index);
                    }
                    index++;
                }
                simulation.Tick();
            }

            if (index < commands.Count)
            {
                return new ReplayResult(false,
                    "Log has " + (commands.Count - index) + " command(s) beyond tick " + tickCount + ".",
                    string.Empty, simulation.CurrentTick, index);
            }

            return new ReplayResult(true, string.Empty,
                StateChecksum.Compute(simulation.OrderedStateSnapshot),
                simulation.CurrentTick, index);
        }
    }

    /// Live play wrapper: issues commands on the current tick and records them.
    public sealed class SimSession
    {
        private int _nextSequence = 1;

        public SimSession(ISimulation simulation, SimCommandLog log = null)
        {
            Simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            Log = log ?? new SimCommandLog();
        }

        public ISimulation Simulation { get; }
        public SimCommandLog Log { get; }
        public string Checksum => StateChecksum.Compute(Simulation.OrderedStateSnapshot);

        public bool TryIssue(
            string kind,
            string actorId,
            string targetId,
            int arg0,
            int arg1,
            string textArg,
            out string failure)
        {
            var command = new SimCommand(
                SimCommandLog.CurrentSchemaVersion,
                Simulation.CurrentTick,
                _nextSequence,
                kind, actorId, targetId, arg0, arg1, textArg);

            if (!Simulation.TryApply(command, out failure)) return false;
            if (!Log.TryRecord(command, out failure))
                throw new InvalidOperationException("Command applied but not recorded: " + failure);
            _nextSequence++;
            return true;
        }

        public void Tick()
        {
            Simulation.Tick();
        }
    }
}
