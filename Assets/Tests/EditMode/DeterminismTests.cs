using System.Collections.Generic;
using NUnit.Framework;
using KindredSiege.Sim;

namespace KindredSiege.Sim.Tests
{
    /// A tiny simulation used only to prove the core: it draws from the Battle stream every
    /// tick and adds command arguments. Order of commands matters, so replay must preserve it.
    internal sealed class CounterSimulation : ISimulation
    {
        private readonly DeterministicRng _rng;
        private readonly List<int> _appliedSequences = new List<int>();
        private long _tick;
        private long _value;

        public CounterSimulation(ulong seed)
        {
            _rng = DeterministicRng.ForStream(seed, SimStream.Battle);
        }

        public long CurrentTick => _tick;
        public long Value => _value;

        public void Tick()
        {
            _value += _rng.Next(10);
            _tick++;
        }

        public bool TryApply(SimCommand command, out string failure)
        {
            failure = string.Empty;
            if (command.Kind != "add")
            {
                failure = "Unknown kind " + command.Kind;
                return false;
            }
            _value = _value * 3 + command.Arg0;
            _appliedSequences.Add(command.Sequence);
            return true;
        }

        public string OrderedStateSnapshot
        {
            get
            {
                var b = new SnapshotBuilder();
                b.Section("counter");
                b.Field("tick", _tick);
                b.Field("value", _value);
                b.Field("rngState", _rng.State);
                b.Section("applied");
                for (int i = 0; i < _appliedSequences.Count; i++)
                    b.Field("s" + i, _appliedSequences[i]);
                return b.ToString();
            }
        }
    }

    public sealed class DeterminismTests
    {
        [Test]
        public void Rng_SameSeedAndStream_SameSequence()
        {
            var a = DeterministicRng.ForStream(1234, SimStream.Battle);
            var b = DeterministicRng.ForStream(1234, SimStream.Battle);
            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(a.NextUInt(), b.NextUInt(), "diverged at draw " + i);
        }

        [Test]
        public void Rng_DifferentStreams_DoNotMoveEachOther()
        {
            var battleOnly = DeterministicRng.ForStream(77, SimStream.Battle);
            var battle = DeterministicRng.ForStream(77, SimStream.Battle);
            var draft = DeterministicRng.ForStream(77, SimStream.Draft);
            for (int i = 0; i < 50; i++) draft.NextUInt();
            for (int i = 0; i < 100; i++)
                Assert.AreEqual(battleOnly.NextUInt(), battle.NextUInt());
        }

        [Test]
        public void Rng_Next_StaysInBounds()
        {
            var rng = DeterministicRng.ForStream(9, SimStream.Events);
            for (int i = 0; i < 10000; i++)
            {
                int v = rng.Next(7);
                Assert.That(v, Is.InRange(0, 6));
            }
        }

        [Test]
        public void Clock_CatchUpIsBounded()
        {
            var clock = new SimClock();
            int ticks = 0;
            int executed = clock.Advance(1.0d, () => ticks++);
            Assert.AreEqual(SimClock.DefaultMaximumCatchUpTicks, executed);
            Assert.AreEqual(SimClock.DefaultMaximumCatchUpTicks, ticks);
        }

        [Test]
        public void Clock_PausedRunsNothing()
        {
            var clock = new SimClock();
            clock.SetPaused(true);
            int ticks = 0;
            Assert.AreEqual(0, clock.Advance(1.0d, () => ticks++));
            Assert.AreEqual(0, ticks);
        }

        [Test]
        public void Log_OrdersByTickThenSequence()
        {
            var log = new SimCommandLog();
            Assert.IsTrue(log.TryRecord(Cmd(5, 2), out _));
            Assert.IsTrue(log.TryRecord(Cmd(1, 3), out _));
            Assert.IsTrue(log.TryRecord(Cmd(5, 1), out _));
            Assert.AreEqual(3, log.Commands[0].Sequence);
            Assert.AreEqual(1, log.Commands[1].Sequence);
            Assert.AreEqual(2, log.Commands[2].Sequence);
        }

        [Test]
        public void Log_RejectsDuplicateSequence()
        {
            var log = new SimCommandLog();
            Assert.IsTrue(log.TryRecord(Cmd(0, 1), out _));
            Assert.IsFalse(log.TryRecord(Cmd(3, 1), out string failure));
            StringAssert.Contains("Duplicate", failure);
        }

        [Test]
        public void Log_JsonRoundTripIsExact()
        {
            var log = new SimCommandLog();
            log.TryRecord(new SimCommand(1, 2, 1, "add", "player", "unit\"7\"", 5, -3, "line one\nline \"two\" \\ end"), out _);
            log.TryRecord(new SimCommand(1, 2, 2, "add", "player", "", 1, 0, ""), out _);
            log.TryRecord(new SimCommand(1, 9, 3, "add", "system", "x", 0, 0, "ü"), out _);

            string json = log.ToJson();
            Assert.IsTrue(SimCommandLog.TryParse(json, out SimCommandLog parsed, out string failure), failure);
            Assert.AreEqual(log.Count, parsed.Count);
            for (int i = 0; i < log.Count; i++)
                Assert.IsTrue(log.Commands[i].SameAs(parsed.Commands[i]), "command " + i + " differs");
            Assert.AreEqual(json, parsed.ToJson());
        }

        [Test]
        public void Replay_FromLog_MatchesLivePlay()
        {
            const ulong seed = 42;
            const long ticks = 50;

            var live = new SimSession(new CounterSimulation(seed));
            RunLive(live, ticks);
            string liveChecksum = live.Checksum;

            var replayed = new CounterSimulation(seed);
            ReplayResult result = ReplayRunner.Run(replayed, live.Log.CloneForReplay(), ticks);

            Assert.IsTrue(result.Success, result.Failure);
            Assert.AreEqual(liveChecksum, result.Checksum);
            Assert.AreEqual(live.Log.Count, result.CommandsApplied);
        }

        [Test]
        public void Replay_TwoLiveRuns_SameInputs_SameChecksum()
        {
            var a = new SimSession(new CounterSimulation(7));
            var b = new SimSession(new CounterSimulation(7));
            RunLive(a, 80);
            RunLive(b, 80);
            Assert.AreEqual(a.Checksum, b.Checksum);
        }

        [Test]
        public void Replay_DifferentSeed_DifferentChecksum()
        {
            var a = new SimSession(new CounterSimulation(1));
            var b = new SimSession(new CounterSimulation(2));
            RunLive(a, 80);
            RunLive(b, 80);
            Assert.AreNotEqual(a.Checksum, b.Checksum);
        }

        [Test]
        public void Replay_RejectsCommandStampedInThePast()
        {
            var log = new SimCommandLog();
            log.TryRecord(Cmd(0, 1), out _);
            var sim = new CounterSimulation(3);
            sim.Tick();
            ReplayResult result = ReplayRunner.Run(sim, log.CloneForReplay(), 5);
            Assert.IsFalse(result.Success);
            StringAssert.Contains("already at tick", result.Failure);
        }

        [Test]
        public void Checksum_ChangesWhenCommandOrderChanges()
        {
            var a = new CounterSimulation(5);
            var b = new CounterSimulation(5);
            a.TryApply(new SimCommand(1, 0, 1, "add", "p", "", 2, 0, ""), out _);
            a.TryApply(new SimCommand(1, 0, 2, "add", "p", "", 9, 0, ""), out _);
            b.TryApply(new SimCommand(1, 0, 2, "add", "p", "", 9, 0, ""), out _);
            b.TryApply(new SimCommand(1, 0, 1, "add", "p", "", 2, 0, ""), out _);
            Assert.AreNotEqual(
                StateChecksum.Compute(a.OrderedStateSnapshot),
                StateChecksum.Compute(b.OrderedStateSnapshot));
        }

        private static SimCommand Cmd(long tick, int sequence)
        {
            return new SimCommand(1, tick, sequence, "add", "player", "", 1, 0, "");
        }

        private static void RunLive(SimSession session, long ticks)
        {
            for (long t = 0; t < ticks; t++)
            {
                if (t == 3) Assert.IsTrue(session.TryIssue("add", "player", "", 4, 0, "", out _));
                if (t == 7)
                {
                    Assert.IsTrue(session.TryIssue("add", "player", "", 1, 0, "", out _));
                    Assert.IsTrue(session.TryIssue("add", "player", "u2", 2, 1, "note", out _));
                }
                if (t == 40) Assert.IsTrue(session.TryIssue("add", "system", "", 11, 0, "", out _));
                session.Tick();
            }
        }
    }
}
