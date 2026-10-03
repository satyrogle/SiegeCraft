using System;
using System.Collections.Generic;
using System.Globalization;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// The 8 December visa slice as a world: the Clarth, a raider tribe whose chief is the
    /// player's rival, and a third tribe at war with the raiders (Jacob, 1 Oct 2026).
    /// The raider and third tribe names are placeholders until Jacob names them.
    ///
    /// The loop it supports: the player fights the raiders with directives, both sides learn,
    /// the raiders then fight the third tribe off screen, and the result changes who the
    /// player can recruit. Every fight is archived and can be replayed exactly.
    ///
    /// Placeholder numbers, not design: a raider horde of 16 and a third tribe of 10, and every
    /// unbroken tribe but the Clarth gets 3 fighters back after each fight. Fights end in a wipe,
    /// so a loser loses all 5 it sent. Measured on 1 Oct 2026 with 12 raiders regrouping 2, the
    /// raiders broke in 179 to 196 of 200 worlds before a third rival fight, mostly in wars.
    public sealed class SliceWorld
    {
        public const string ClarthId = "clarth";
        public const string RaidersId = "raiders";
        public const string ThirdTribeId = "third_tribe";
        public const int PlayerSide = 0;
        public const int RegroupPerFight = 3;

        private readonly DeterministicRng _seeds;
        private int _fightNumber;

        private SliceWorld(ulong worldSeed)
        {
            WorldSeed = worldSeed;
            _seeds = DeterministicRng.ForStream(worldSeed, SimStream.Events);
            Archive = new FightArchive();

            Clarth = new Tribe(ClarthId, "Clarth", new TribeMind(ClarthId, "Clarth elder"), new[]
            {
                UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Medic
            });
            Raiders = new Tribe(RaidersId, "Raiders (placeholder)", new TribeMind(RaidersId, "Raider chief (placeholder)"), new[]
            {
                UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Medic,
                UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Medic,
                UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Marksman,
                UnitClass.Medic
            });
            ThirdTribe = new Tribe(ThirdTribeId, "Third tribe (placeholder)", new TribeMind(ThirdTribeId, "Third tribe chief (placeholder)"), new[]
            {
                UnitClass.Warden, UnitClass.Marksman, UnitClass.Trapper, UnitClass.Marksman, UnitClass.Medic,
                UnitClass.Warden, UnitClass.Trapper, UnitClass.Marksman, UnitClass.Medic, UnitClass.Marksman
            });
        }

        public static SliceWorld Create(ulong worldSeed)
        {
            return new SliceWorld(worldSeed);
        }

        public ulong WorldSeed { get; }

        /// The player's tribe. They recover between fights in this prototype.
        public Tribe Clarth { get; }

        /// The rival's tribe. Its mind is what the rival chief has learned.
        public Tribe Raiders { get; }

        public Tribe ThirdTribe { get; }
        public FightArchive Archive { get; }
        public int FightsFought => _fightNumber;

        public IReadOnlyList<Tribe> Tribes => new[] { Clarth, Raiders, ThirdTribe };

        /// The raiders and the third tribe can no longer go to war: one of them has broken.
        public bool WarOver => Raiders.Broken || ThirdTribe.Broken;

        /// Starts the next player fight against the raiders. Issue directives through a session on
        /// this setup, tick until the fight ends, then call FinishPlayerFight.
        public BattleSetup NextPlayerFight()
        {
            if (Raiders.Broken || Raiders.Strength == 0) throw new InvalidOperationException("The raiders cannot fight.");
            string fightId = NextFightId("rival");
            return new BattleSetup(fightId, NextSeed(),
                Clarth.Deploy(TribeWar.DeployLimit, fightId), Raiders.Deploy(TribeWar.DeployLimit, fightId),
                Clarth.Mind.CopyTactics(), Raiders.Mind.CopyTactics());
        }

        /// Archives the fight, lets both tribes learn, takes the raiders' dead off their roster and
        /// breaks them if they lost too many. Then every unbroken tribe but the Clarth regroups.
        public PlayerFightResult FinishPlayerFight(BattleSetup setup, SimSession session)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            if (session == null) throw new ArgumentNullException(nameof(session));
            var sim = session.Simulation as BattleSimulation;
            if (sim == null || !ReferenceEquals(sim.Setup, setup))
                throw new ArgumentException("The session must be running this setup.");
            if (!sim.IsFinished) throw new InvalidOperationException("The fight has not finished.");

            Archive.Add(setup, session.Log, sim);
            FightRecord record = Archive.Find(setup.FightId).Record;
            int raiderDead = Raiders.RemoveDead(sim, 1 - PlayerSide);
            bool raidersBroke = false;
            if (record.Winner == PlayerSide)
            {
                Clarth.Wins++;
                Raiders.Losses++;
                raidersBroke = Raiders.CheckBreak();
            }
            else if (record.Winner == 1 - PlayerSide)
            {
                Clarth.Losses++;
                Raiders.Wins++;
            }
            IReadOnlyList<Lesson> clarthLessons = LessonRules.Learn(Clarth.Mind, record, PlayerSide);
            IReadOnlyList<Lesson> raiderLessons = LessonRules.Learn(Raiders.Mind, record, 1 - PlayerSide);
            RegroupAll();
            return new PlayerFightResult(record, raiderDead, raidersBroke, clarthLessons, raiderLessons);
        }

        /// The raiders and the third tribe fight each other off screen.
        public WarResult RunRaiderWar()
        {
            if (WarOver) throw new InvalidOperationException("The war is over.");
            WarResult result = TribeWar.Resolve(Raiders, ThirdTribe, NextSeed(), NextFightId("war"), Archive);
            RegroupAll();
            return result;
        }

        public List<Tribe> RecruitableTribes()
        {
            var list = new List<Tribe>();
            IReadOnlyList<Tribe> tribes = Tribes;
            for (int i = 0; i < tribes.Count; i++)
            {
                if (tribes[i].RecruitableByPlayer) list.Add(tribes[i]);
            }
            return list;
        }

        public string Snapshot()
        {
            var b = new SnapshotBuilder();
            b.Section("world");
            b.Field("seed", WorldSeed);
            b.Field("fights", _fightNumber);
            b.Field("seedRng", _seeds.State);
            IReadOnlyList<ArchivedFight> fights = Archive.Fights;
            for (int i = 0; i < fights.Count; i++) b.Field(fights[i].FightId, fights[i].Record.Checksum);
            return b.ToString() + Clarth.Snapshot() + Raiders.Snapshot() + ThirdTribe.Snapshot();
        }

        private void RegroupAll()
        {
            Raiders.Regroup(RegroupPerFight);
            ThirdTribe.Regroup(RegroupPerFight);
        }

        private string NextFightId(string tag)
        {
            _fightNumber++;
            return "f" + _fightNumber.ToString(CultureInfo.InvariantCulture) + "." + tag;
        }

        private ulong NextSeed()
        {
            ulong high = _seeds.NextUInt();
            ulong low = _seeds.NextUInt();
            return (high << 32) | low;
        }
    }

    public sealed class PlayerFightResult
    {
        internal PlayerFightResult(FightRecord record, int raiderDead, bool raidersBroke,
            IReadOnlyList<Lesson> clarthLessons, IReadOnlyList<Lesson> raiderLessons)
        {
            Record = record;
            RaiderDead = raiderDead;
            RaidersBroke = raidersBroke;
            ClarthLessons = clarthLessons;
            RaiderLessons = raiderLessons;
        }

        public FightRecord Record { get; }
        public int RaiderDead { get; }

        /// This loss broke the raiders: the rivalry is over and their survivors can be recruited.
        public bool RaidersBroke { get; }

        public IReadOnlyList<Lesson> ClarthLessons { get; }

        /// What the rival chief took away from this fight.
        public IReadOnlyList<Lesson> RaiderLessons { get; }
    }
}
