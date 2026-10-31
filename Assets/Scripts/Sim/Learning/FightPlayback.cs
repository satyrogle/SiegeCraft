using System.Collections.Generic;
using System.Globalization;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// Plays a recorded fight back one tick at a time, for a screen to draw (Jacob's go, 12 Oct 2026). It
    /// starts the fight again from its setup and gives every order on the tick it was given, the way
    /// FightArchive.TryReplay does, so play, pause, speed and restart are only how often the screen calls
    /// Step. Seek jumps to any tick, such as a lesson's window. At the end it checks the fight came out as
    /// recorded. It plays its own copy of the battle, so watching never changes the campaign.
    public sealed class FightPlayback
    {
        private readonly ArchivedFight _fight;
        private IReadOnlyList<SimCommand> _orders;
        private int _next;
        private BattleSimulation _sim;

        private FightPlayback(ArchivedFight fight)
        {
            _fight = fight;
            Restart();
        }

        /// Opens a recorded fight at its first tick. False, with the reason, when the archive has no such
        /// fight or it was fought under battle rules this build does not have.
        public static bool TryOpen(FightArchive archive, string fightId, out FightPlayback playback, out string failure)
        {
            playback = null;
            failure = string.Empty;
            ArchivedFight fight = archive == null ? null : archive.Find(fightId);
            if (fight == null)
            {
                failure = "No recorded fight " + fightId + ".";
                return false;
            }
            if (!ReplayEvidence.Replayable(fight))
            {
                failure = "This fight uses different battle rules. Its history is kept, but it cannot be played back in this build.";
                return false;
            }
            // A rules identity alone cannot prove compatibility: older builds reused one after a
            // spell-strength change. Verify before drawing any reconstructed events as history.
            if (!archive.TryReplay(fightId, out _, out _))
            {
                failure = "This fight no longer replays exactly in this build. Its recorded history is kept, but playback is unavailable.";
                return false;
            }
            playback = new FightPlayback(fight);
            return true;
        }

        public ArchivedFight Fight => _fight;
        public string FightId => _fight.FightId;

        /// A unit's name in this fight, the way the fight named them. Empty for a fight recorded before names
        /// were kept, for your leader and for the hunters' unnamed blades. Their captain keeps their name.
        public string NameOf(string unitId)
        {
            return _fight.NameOf(unitId);
        }

        /// The battle as it stands at Tick: units, health, nerve, orders running and every event so far.
        /// Read it to draw. Never give it orders or ticks; Step does that.
        public BattleSimulation Sim => _sim;

        public long Tick => _sim.CurrentTick;

        /// How many ticks the fight ran.
        public long Length => _fight.Ticks;

        public bool AtEnd => _sim.CurrentTick >= _fight.Ticks;

        /// Set when the fight came out differently from its record, saying how. Playback stops there.
        public string Failure { get; private set; } = string.Empty;

        /// At the end: the fight came out exactly as recorded, the same state and the same history.
        public bool Matched { get; private set; }

        /// Plays one tick, giving the orders due on it first. False at the end, or once something failed.
        public bool Step()
        {
            if (AtEnd || Failure.Length > 0) return false;
            if (!GiveOrders()) return false;
            _sim.Tick();
            // An order on the last tick can end the fight on that tick (an Ambush or a Riposte that breaks their
            // nerve): it was given before the fight stopped, so it is given here before the check.
            if (AtEnd && GiveOrders()) Check();
            return true;
        }

        // Gives every order due on the current tick. False, with the reason, if one is out of order or refused.
        private bool GiveOrders()
        {
            while (_next < _orders.Count && _orders[_next].Tick <= _sim.CurrentTick)
            {
                SimCommand order = _orders[_next];
                if (order.Tick < _sim.CurrentTick)
                    return Stop("Order " + N(order.Sequence) + " is stamped tick " + L(order.Tick) + " but the fight is already at tick " +
                        L(_sim.CurrentTick) + ".");
                if (!_sim.TryApply(order, out string why)) return Stop("Order " + N(order.Sequence) + " was refused: " + why);
                _next++;
            }
            return true;
        }

        /// Back to the first tick, as the fight began.
        public void Restart()
        {
            _sim = new BattleSimulation(_fight.Setup);
            _orders = _fight.Log.Commands;
            _next = 0;
            Failure = string.Empty;
            Matched = false;
            // A fight can end on orders at tick zero. There is no Step to give those terminal
            // orders, so opening and restarting must apply and verify them without advancing time.
            if (AtEnd && GiveOrders()) Check();
        }

        /// Jumps to a tick: forward by playing on, back by starting again. False if the tick is outside the
        /// fight, or something failed on the way.
        public bool Seek(long tick)
        {
            if (tick < 0 || tick > Length) return false;
            if (tick < Tick) Restart();
            while (Tick < tick)
                if (!Step()) return false;
            return Failure.Length == 0;
        }

        // The same checks as FightArchive.TryReplay: every order given, finished on the same tick, the same
        // state and the same history.
        private void Check()
        {
            if (_next < _orders.Count)
            {
                Stop(N(_orders.Count - _next) + " order(s) were given after the fight ended.");
                return;
            }
            if (!_sim.IsFinished || StateChecksum.Compute(_sim.OrderedStateSnapshot) != _fight.Record.Checksum)
            {
                Stop("The fight came out differently from its record.");
                return;
            }
            if (!ReplayEvidence.SameRecord(_fight.Record, FightRecord.From(_sim)))
            {
                Stop("The fight's history came out differently from its record.");
                return;
            }
            Matched = true;
        }

        private bool Stop(string why)
        {
            Failure = why;
            return false;
        }

        private static string N(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string L(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
