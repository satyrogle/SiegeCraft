using System;

namespace KindredSiege.Sim
{
    /// Named random streams. One per system, so a draw in one never moves another.
    /// The idea is from Desk 42's SeedEngine. The generator is PCG32 instead of
    /// System.Random so the sequence is identical on every platform and runtime.
    public enum SimStream
    {
        Draft = 1,
        Battle = 2,
        Lessons = 3,
        Events = 4,
        Rewards = 5,
        Mutations = 6
    }

    /// PCG32. Integer only. Same seed, same sequence, everywhere.
    public sealed class DeterministicRng
    {
        private const ulong Multiplier = 6364136223846793005UL;

        private ulong _state;
        private readonly ulong _increment;

        public DeterministicRng(ulong seed, ulong stream = 0)
        {
            _increment = (stream << 1) | 1UL;
            _state = 0UL;
            NextUInt();
            _state += seed;
            NextUInt();
        }

        private DeterministicRng(ulong state, ulong increment, bool raw)
        {
            _state = state;
            _increment = increment;
        }

        public ulong State => _state;
        public ulong Increment => _increment;

        public static DeterministicRng ForStream(ulong runSeed, SimStream stream)
        {
            return new DeterministicRng(runSeed, (ulong)stream);
        }

        public static DeterministicRng Restore(ulong state, ulong increment)
        {
            return new DeterministicRng(state, increment, true);
        }

        public DeterministicRng Clone()
        {
            return new DeterministicRng(_state, _increment, true);
        }

        public uint NextUInt()
        {
            ulong old = _state;
            _state = old * Multiplier + _increment;
            uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
            int rotation = (int)(old >> 59);
            return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
        }

        /// Uniform in [0, maxExclusive). Rejection sampling, no modulo bias.
        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            uint bound = (uint)maxExclusive;
            uint threshold = unchecked((uint)(0u - bound)) % bound;
            while (true)
            {
                uint r = NextUInt();
                if (r >= threshold) return (int)(r % bound);
            }
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            return minInclusive + Next(maxExclusive - minInclusive);
        }

        /// True with probability percent in 100. Integer percent, 0 to 100.
        public bool Chance(int percent)
        {
            if (percent <= 0) return false;
            if (percent >= 100) return true;
            return Next(100) < percent;
        }
    }
}
