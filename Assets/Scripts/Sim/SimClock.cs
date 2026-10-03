using System;

namespace KindredSiege.Sim
{
    /// Fixed 30 Hz tick clock. Ported from Desk 42's OfficeSimulationClock.
    /// The accumulator takes frame time from presentation; the simulation never sees it.
    public sealed class SimClock
    {
        public const int TicksPerSecond = 30;
        public const int DefaultMaximumCatchUpTicks = 4;
        public const double TickDurationSeconds = 1d / TicksPerSecond;
        private const double TickComparisonEpsilon = 0.000000000001d;

        private double _accumulator;

        public SimClock(int maximumCatchUpTicks = DefaultMaximumCatchUpTicks)
        {
            if (maximumCatchUpTicks <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumCatchUpTicks));
            MaximumCatchUpTicks = maximumCatchUpTicks;
        }

        public int MaximumCatchUpTicks { get; }
        public long CurrentTick { get; private set; }
        public bool Paused { get; private set; }

        public int Advance(double unscaledDeltaTime, Action tick)
        {
            if (tick == null) throw new ArgumentNullException(nameof(tick));
            if (Paused) return 0;

            _accumulator += Math.Max(0d, unscaledDeltaTime);
            int executed = 0;
            while (_accumulator + TickComparisonEpsilon >= TickDurationSeconds &&
                executed < MaximumCatchUpTicks)
            {
                tick();
                CurrentTick++;
                _accumulator -= TickDurationSeconds;
                if (_accumulator < 0d && _accumulator > -TickComparisonEpsilon)
                    _accumulator = 0d;
                executed++;
            }
            if (executed == MaximumCatchUpTicks && _accumulator > TickDurationSeconds)
                _accumulator = TickDurationSeconds;
            return executed;
        }

        public bool Step(Action tick)
        {
            if (tick == null) throw new ArgumentNullException(nameof(tick));
            tick();
            CurrentTick++;
            _accumulator = 0d;
            return true;
        }

        public void SetPaused(bool paused)
        {
            Paused = paused;
            if (paused) _accumulator = 0d;
        }
    }
}
