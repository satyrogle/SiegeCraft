using System;
using System.Collections.Generic;
using System.Globalization;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// A tribe on the campaign map: who it is at full strength, who is left, what it has learned,
    /// and where it stands with the player.
    public sealed class Tribe
    {
        /// Nerve rules: how much nerve a tribe gets back each time it regroups. Placeholder.
        public const int NerveRecoveryPerRegroup = 200;

        private readonly List<UnitClass> _home = new List<UnitClass>();
        private readonly List<UnitClass> _fighters = new List<UnitClass>();

        public Tribe(string id, string displayName, TribeMind mind, IReadOnlyList<UnitClass> homeRoster)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Mind = mind ?? throw new ArgumentNullException(nameof(mind));
            if (homeRoster == null || homeRoster.Count == 0) throw new ArgumentException("A tribe needs at least one fighter.");
            for (int i = 0; i < homeRoster.Count; i++)
            {
                _home.Add(homeRoster[i]);
                _fighters.Add(homeRoster[i]);
            }
        }

        public string Id { get; }
        public string DisplayName { get; }
        public TribeMind Mind { get; }

        /// The tribe at full strength, in deployment order. Hand made, never changes.
        public IReadOnlyList<UnitClass> HomeRoster => _home;

        /// Who is left, always in home roster order.
        public IReadOnlyList<UnitClass> Fighters => _fighters;

        public int Strength => _fighters.Count;
        public int HomeStrength => _home.Count;
        public int Wins { get; internal set; }
        public int Losses { get; internal set; }

        /// Old rules: lost a fight with fewer than half its home strength left. Nerve rules: its
        /// nerve ran out. A broken tribe no longer fights or regroups.
        public bool Broken { get; internal set; }

        /// Nerve rules: what the tribe has left, carried from one fight into the next, in
        /// thousandths of a full bar. Stays full under the old rules.
        public int Nerve { get; internal set; } = NerveRules.Full;

        /// Nerve rules: the player broke this tribe and every survivor left to join the player.
        public bool JoinedPlayer { get; internal set; }

        /// Set once the tribe lives under the nerve rules, so its snapshot carries its nerve.
        internal bool NerveTracked { get; set; }

        /// Broken with survivors: they will now listen to a recruitment offer from the player.
        public bool RecruitableByPlayer => Broken && _fighters.Count > 0;

        /// Broken with nobody left. A tribe whose survivors joined the player is not destroyed.
        public bool Destroyed => Broken && _fighters.Count == 0 && !JoinedPlayer;

        /// The first fighters in roster order, up to max, with stable ids.
        public List<UnitSpec> Deploy(int max, string idPrefix)
        {
            var specs = new List<UnitSpec>();
            int count = Math.Min(max, _fighters.Count);
            for (int i = 0; i < count; i++)
                specs.Add(new UnitSpec(idPrefix + "." + Id + "." + i.ToString(CultureInfo.InvariantCulture), _fighters[i]));
            return specs;
        }

        /// Takes this fight's dead off the roster. The side must have been deployed from the
        /// current roster, so the n-th fighter on that side is the n-th fighter. A Pal or Chief
        /// fighting alongside is not on the roster and is skipped.
        internal int RemoveDead(BattleSimulation sim, int side)
        {
            var dead = new List<int>();
            int k = 0;
            for (int i = 0; i < sim.Units.Count; i++)
            {
                BattleUnit u = sim.Units[i];
                if (u.Side != side || u.IsLeader) continue;
                if (k >= _fighters.Count || _fighters[k] != u.Class)
                    throw new InvalidOperationException("Side " + side + " was not deployed from " + Id + "'s current roster.");
                if (!u.Alive) dead.Add(k);
                k++;
            }
            for (int d = dead.Count - 1; d >= 0; d--) _fighters.RemoveAt(dead[d]);
            return dead.Count;
        }

        /// Brings back up to count missing fighters, filling the earliest gaps in the home roster.
        /// Broken tribes do not regroup. Returns how many came back.
        internal int Regroup(int count)
        {
            if (Broken || count <= 0) return 0;
            var have = new int[SliceClasses.Count];
            for (int i = 0; i < _fighters.Count; i++) have[(int)_fighters[i]]++;
            var rebuilt = new List<UnitClass>(_home.Count);
            int added = 0;
            for (int i = 0; i < _home.Count; i++)
            {
                int c = (int)_home[i];
                if (have[c] > 0)
                {
                    have[c]--;
                    rebuilt.Add(_home[i]);
                }
                else if (added < count)
                {
                    added++;
                    rebuilt.Add(_home[i]);
                }
            }
            _fighters.Clear();
            _fighters.AddRange(rebuilt);
            return added;
        }

        /// After a lost fight: breaks the tribe if fewer than half its home strength is left.
        internal bool CheckBreak()
        {
            if (Broken || _fighters.Count * 2 >= _home.Count) return false;
            Broken = true;
            return true;
        }

        /// Nerve rules: a regroup also steadies the tribe a little. Broken tribes do not recover.
        internal void RecoverNerve(int amount)
        {
            if (Broken || amount <= 0) return;
            Nerve = Math.Min(NerveRules.Full, Nerve + amount);
        }

        /// Nerve rules: breaks the tribe once its nerve has run out.
        internal bool CheckNerveBreak()
        {
            if (Broken || Nerve > 0) return false;
            Broken = true;
            return true;
        }

        /// Nerve rules: the player broke this tribe, so every survivor, in the field and at home,
        /// leaves to join the player. Returns who left, in roster order.
        internal List<UnitClass> LeaveToJoinPlayer()
        {
            var leaving = new List<UnitClass>(_fighters);
            _fighters.Clear();
            Broken = true;
            JoinedPlayer = true;
            return leaving;
        }

        public string Snapshot()
        {
            var b = new SnapshotBuilder();
            b.Section("tribe:" + Id);
            b.Field("home", _home.Count);
            b.Field("strength", _fighters.Count);
            for (int i = 0; i < _fighters.Count; i++)
                b.Field("f" + i.ToString(CultureInfo.InvariantCulture), SliceClasses.Name(_fighters[i]));
            b.Field("wins", Wins);
            b.Field("losses", Losses);
            b.Field("broken", Broken);
            if (NerveTracked)
            {
                b.Field("nerve", Nerve);
                b.Field("joined", JoinedPlayer);
            }
            return b.ToString() + Mind.Snapshot();
        }
    }

    public sealed class WarResult
    {
        internal WarResult(string fightId, string winnerId, string loserId, int deadA, int deadB,
            bool loserBroke, IReadOnlyList<Lesson> lessonsA, IReadOnlyList<Lesson> lessonsB)
        {
            FightId = fightId;
            WinnerId = winnerId;
            LoserId = loserId;
            DeadA = deadA;
            DeadB = deadB;
            LoserBroke = loserBroke;
            LessonsA = lessonsA;
            LessonsB = lessonsB;
        }

        public string FightId { get; }

        /// Empty on a draw.
        public string WinnerId { get; }
        public string LoserId { get; }
        public int DeadA { get; }
        public int DeadB { get; }
        public bool LoserBroke { get; }
        public IReadOnlyList<Lesson> LessonsA { get; }
        public IReadOnlyList<Lesson> LessonsB { get; }
    }

    /// Wars between tribes, fought off screen with the same battle rules the player fights under,
    /// but with no orders. Both tribes learn from the fight, the dead leave the rosters, and a
    /// loser left with fewer than half its home strength breaks and opens to the player's
    /// recruiters. Under the nerve rules a tribe breaks when its nerve runs out instead, and each
    /// tribe brings the nerve it has left into the fight. A tribe broken in a war between tribes
    /// stays recruitable. Provisional rules.
    public static class TribeWar
    {
        public const int DeployLimit = 5;

        public static WarResult Resolve(Tribe a, Tribe b, ulong seed, string fightId, FightArchive archive)
        {
            return Resolve(a, b, seed, fightId, archive, false);
        }

        /// The same, with the nerve rules on or off.
        public static WarResult Resolve(Tribe a, Tribe b, ulong seed, string fightId, FightArchive archive, bool nerve)
        {
            return Resolve(a, b, seed, fightId, archive, nerve, BattleSetup.NormalHealthPercent);
        }

        /// The same, with unit health in percent (the slice's slower pace). Wars have no orders,
        /// so refilling orders change nothing here.
        public static WarResult Resolve(Tribe a, Tribe b, ulong seed, string fightId, FightArchive archive, bool nerve,
            int healthPercent)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (a.Broken || b.Broken) throw new InvalidOperationException("A broken tribe cannot go to war.");
            if (a.Strength == 0 || b.Strength == 0) throw new InvalidOperationException("Both tribes need fighters.");

            var setup = new BattleSetup(fightId, seed, a.Deploy(DeployLimit, fightId), b.Deploy(DeployLimit, fightId),
                a.Mind.CopyTactics(), b.Mind.CopyTactics(), BattleSetup.DefaultTickLimit, 0,
                nerve ? NerveRules.On(a.Nerve, b.Nerve) : NerveRules.Off, 0, healthPercent);
            var sim = new BattleSimulation(setup);
            sim.RunUntilFinished();
            if (archive != null) archive.Add(setup, null, sim);
            FightRecord record = FightRecord.From(sim);

            int deadA = a.RemoveDead(sim, 0);
            int deadB = b.RemoveDead(sim, 1);
            IReadOnlyList<Lesson> lessonsA = LessonRules.Learn(a.Mind, record, 0);
            IReadOnlyList<Lesson> lessonsB = LessonRules.Learn(b.Mind, record, 1);

            Tribe winner = record.Winner == 0 ? a : (record.Winner == 1 ? b : null);
            Tribe loser = record.Winner == 0 ? b : (record.Winner == 1 ? a : null);
            if (winner != null)
            {
                winner.Wins++;
                loser.Losses++;
            }

            bool broke = false;
            if (nerve)
            {
                a.Nerve = sim.Nerve(0);
                b.Nerve = sim.Nerve(1);
                a.NerveTracked = true;
                b.NerveTracked = true;
                bool aBroke = a.CheckNerveBreak();
                bool bBroke = b.CheckNerveBreak();
                if (loser != null) broke = ReferenceEquals(loser, a) ? aBroke : bBroke;
            }
            else if (loser != null)
            {
                broke = loser.CheckBreak();
            }

            return new WarResult(fightId, winner == null ? string.Empty : winner.Id, loser == null ? string.Empty : loser.Id,
                deadA, deadB, broke, lessonsA, lessonsB);
        }
    }
}
