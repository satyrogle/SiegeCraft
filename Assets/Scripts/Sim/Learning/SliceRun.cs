using System;
using System.Collections.Generic;
using System.Globalization;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// Gear the Butcher forges (Jacob, 4 Oct 2026). Breaking a chief teaches him that chief's
    /// recipe (the lock: "the Pal who broke a chief learns that chief's gear recipe") and he forges
    /// it on the spot: the raider chief's Lava Sun Tabard, the Skullpeak chief's Steel Rib Plate.
    /// The Chief's Fang is the trophy from catching the raider chief.
    public enum GearKind
    {
        None = 0,
        LavaSunTabard = 1,
        SteelRibPlate = 2,
        ChiefsFang = 3
    }

    /// What each piece of gear does. Placeholders for tuning.
    public static class Gear
    {
        public static string Name(GearKind kind)
        {
            switch (kind)
            {
                case GearKind.LavaSunTabard: return "Lava Sun Tabard";
                case GearKind.SteelRibPlate: return "Steel Rib Plate";
                case GearKind.ChiefsFang: return "Chief's Fang";
                default: return "None";
            }
        }

        /// Percent added to the wearer's power.
        public static int PowerBonus(GearKind kind)
        {
            return kind == GearKind.ChiefsFang ? 25 : 0;
        }

        /// Percent added to the wearer's health.
        public static int HealthBonus(GearKind kind)
        {
            return kind == GearKind.SteelRibPlate ? 20 : 0;
        }

        /// Percent added to the courage the wearer's hits drain.
        public static int FearBonus(GearKind kind)
        {
            return kind == GearKind.LavaSunTabard ? 40 : 0;
        }

        public static string Describe(GearKind kind)
        {
            int power = PowerBonus(kind);
            int health = HealthBonus(kind);
            int fear = FearBonus(kind);
            if (power > 0) return "+" + power.ToString(CultureInfo.InvariantCulture) + "% power";
            if (health > 0) return "+" + health.ToString(CultureInfo.InvariantCulture) + "% health";
            if (fear > 0) return "hits drain " + fear.ToString(CultureInfo.InvariantCulture) + "% more courage";
            return string.Empty;
        }
    }

    /// A fighter you own in a run. The id stays the same from fight to fight, so the reserve,
    /// the squad, the fight and the gear all name the same fighter.
    public sealed class RunFighter
    {
        internal RunFighter(string id, string tribeId, string tribeName, UnitClass unitClass, int tier, bool chief,
            int basePowerPercent, int baseHealthPercent)
        {
            Id = id;
            TribeId = tribeId;
            TribeName = tribeName;
            Class = unitClass;
            Tier = tier;
            Chief = chief;
            BasePowerPercent = basePowerPercent;
            BaseHealthPercent = baseHealthPercent;
        }

        public string Id { get; }

        /// The tribe it came from. Its art comes from here too.
        public string TribeId { get; }
        public string TribeName { get; }
        public UnitClass Class { get; }

        /// The tier of the tribe it came from. A fighter keeps its tier's strength when it joins you.
        public int Tier { get; }

        /// A chief you caught: chief strength, chief art.
        public bool Chief { get; }

        /// Strength from tier (and chiefhood) before gear, in percent.
        public int BasePowerPercent { get; }
        public int BaseHealthPercent { get; }

        public GearKind Gear { get; internal set; }

        public int PowerPercent => BasePowerPercent + Learning.Gear.PowerBonus(Gear);
        public int HealthPercent => BaseHealthPercent + Learning.Gear.HealthBonus(Gear);
        public int FearPercent => Learning.Gear.FearBonus(Gear);

        public string DisplayName => Chief ? TribeName + " chief" : TribeName + " " + SliceClasses.Name(Class);
    }

    /// What outlives a run (Jacob, 4 Oct 2026): every tribe's mind and the codex. When a run is
    /// lost the army, its gear and everyone who joined are gone; what the chiefs learned about
    /// you is not. RunSave writes it down so it outlives closing the game too.
    public sealed class RunMemory
    {
        private readonly List<string> _codex = new List<string>();

        public RunMemory()
        {
            ClarthMind = new TribeMind(SliceRun.ClarthId, "Clarth elder");
            RaiderMind = new TribeMind(SliceRun.RaidersId, "Raider chief");
            SkullpeakMind = new TribeMind(SliceRun.SkullpeakId, "Skullpeak chief");
        }

        /// What your own fighters learned (their reflexes).
        public TribeMind ClarthMind { get; }

        /// The raider chief's mind. His splinter crew fights with it too.
        public TribeMind RaiderMind { get; }

        public TribeMind SkullpeakMind { get; }
        public int RunsStarted { get; internal set; }
        public int RunsWon { get; internal set; }

        /// Everything met, seen or caught across every run, in the order it was first found.
        public IReadOnlyList<string> Codex => _codex;

        /// Adds an entry the first time. Returns true when it was new.
        internal bool Discover(string entry)
        {
            if (_codex.Contains(entry)) return false;
            _codex.Add(entry);
            return true;
        }

        public string Snapshot()
        {
            var b = new SnapshotBuilder();
            b.Section("memory");
            b.Field("runs", RunsStarted);
            b.Field("won", RunsWon);
            for (int i = 0; i < _codex.Count; i++) b.Field("codex" + i.ToString(CultureInfo.InvariantCulture), _codex[i]);
            return b.ToString() + ClarthMind.Snapshot() + RaiderMind.Snapshot() + SkullpeakMind.Snapshot();
        }
    }

    /// The run's numbers. Every one is a placeholder; the slice plays with the defaults and the
    /// balance harness tries others. A run copies its tuning when it starts.
    ///
    /// Defaults measured on 4 Oct 2026 over 150 first runs each, with the autopilot: answering
    /// every Eclipse (Smart) won 63% of runs, 68% of rematches; never answering (Passive) won
    /// fight 1 93% of the time and no rematch. Before this, ignoring the chief won almost always.
    ///
    /// Measured again on 6 Oct 2026, after Codex's audit showed that pressing Unleash whenever
    /// it was affordable won all 300 of its runs. Now a Hold blocks Unleash's scare and a fighter
    /// who dies is gone for the run. Over the audit's 300 seeds, with fresh memory each run, full
    /// runs won with an Eclipse every 480 ticks: dodging every Eclipse while Unleashing whenever
    /// affordable, with Frenzy, 300; the same but waiting for the chief's shields to drop 298;
    /// the Smart autopilot 284 (98 before it stopped trying to break the cast and stopped
    /// focusing their Medic); Hold always plus Unleash and Frenzy 98; ignoring the chief with
    /// Unleash and Frenzy 15; Unleash whenever affordable 0; Passive 0. Answering the chief's
    /// Eclipse is what wins. At 540 ticks, ignoring him with Unleash and Frenzy still won 278.
    public sealed class RunTuning
    {
        /// Strength of each tier in percent, for power and health alike: tier 1, 2, 3.
        /// Measured on 6 Oct 2026: at a tier 3 of 130 or 135 every plan but Passive won almost
        /// every rematch, and at 160 even dodging every Eclipse won only 32 of 300 runs.
        public int Tier1Percent { get; set; } = 110;
        public int Tier2Percent { get; set; } = 130;
        public int Tier3Percent { get; set; } = 140;

        /// The Butcher (Jacob's Pal, placeholder art): a front row Warden who leads every fight.
        public int ButcherPowerPercent { get; set; } = 175;
        public int ButcherHealthPercent { get; set; } = 175;

        /// A chief's strength on top of its tier.
        public int ChiefPowerPercent { get; set; } = 150;
        public int ChiefHealthPercent { get; set; } = 300;

        /// The raider chief's Eclipse grade, and in the rematch.
        public int ChiefEclipseGrade { get; set; } = 2;
        public int RematchEclipseGrade { get; set; } = 3;

        /// The raider chief casts Eclipse this often, in ticks (16 seconds). Measured on 6 Oct
        /// 2026, see above: at 540 ignoring him still won, and at 450 waiting for his shields won 77.
        public int AutoEclipseTicks { get; set; } = 480;

        /// Fighters a tribe sends besides its chief.
        public int EnemyDeploy { get; set; } = 5;

        /// A tribe never leaves a war between tribes with less nerve than this, so a war weakens
        /// it but never breaks it, and never sends it into your fight half beaten.
        public int WarNerveFloor { get; set; } = 800;

        /// Nerve the enemy loses when either side Unleashes in your fights, unless the enemy holds.
        public int UnleashNerve { get; set; } = BattleSimulation.UnleashNerveCost;

        /// A running Hold takes one nerve from the holder every this many ticks in your fights.
        /// Measured on 4 Oct 2026: at every tick (150 for a full Hold) the chief's learned "Hold
        /// when you Unleash" burned his own courage, and a player who never answered him won
        /// every rematch; at every 3 ticks that player won none.
        public int HoldDrainEveryTicks { get; set; } = 3;

        public int TierPercent(int tier)
        {
            if (tier <= 1) return Tier1Percent;
            return tier == 2 ? Tier2Percent : Tier3Percent;
        }

        public RunTuning Clone()
        {
            return (RunTuning)MemberwiseClone();
        }

        public void AppendTo(SnapshotBuilder b)
        {
            b.Field("tiers", Tier1Percent.ToString(CultureInfo.InvariantCulture) + "/" +
                Tier2Percent.ToString(CultureInfo.InvariantCulture) + "/" + Tier3Percent.ToString(CultureInfo.InvariantCulture));
            b.Field("butcher", ButcherPowerPercent.ToString(CultureInfo.InvariantCulture) + "/" + ButcherHealthPercent.ToString(CultureInfo.InvariantCulture));
            b.Field("chief", ChiefPowerPercent.ToString(CultureInfo.InvariantCulture) + "/" + ChiefHealthPercent.ToString(CultureInfo.InvariantCulture));
            b.Field("grades", ChiefEclipseGrade.ToString(CultureInfo.InvariantCulture) + "/" + RematchEclipseGrade.ToString(CultureInfo.InvariantCulture));
            b.Field("autoEclipse", AutoEclipseTicks);
            b.Field("deploy", EnemyDeploy);
            b.Field("warFloor", WarNerveFloor);
            b.Field("unleashNerve", UnleashNerve);
            b.Field("holdDrainEvery", HoldDrainEveryTicks);
        }
    }

    public enum RunStage
    {
        Raiders = 0,
        Skullpeak = 1,
        Rematch = 2,
        Won = 3,
        Lost = 4
    }

    /// The 8 December slice as a run (Jacob, 4 Oct 2026: "Kingdom Rush with Pokemon").
    ///
    /// You lead with the Butcher, your Pal, plus a squad of 5 you pick from your reserve before
    /// every fight. Break a tribe and its survivors join your reserve. The run goes: the
    /// Bloodridge raiders (tier 1); off screen, the raider chief's splinter crew raids Skullpeak;
    /// Skullpeak (tier 2); then the raider chief's rematch with his splinter crew (tier 3).
    ///
    /// The raider chief escapes the first break with the crew who stayed home. As he runs you
    /// steal his Eclipse (the cleaner the win, the higher its grade) and the Butcher learns his
    /// gear recipe and forges it on the spot; you choose who wears it. He comes back smarter,
    /// having learned from your fight and his war, and the second break catches him. Breaking
    /// Skullpeak catches their chief and teaches the Butcher her recipe. Lose a fight and the run
    /// is over: start again with a fresh army against chiefs who remember you.
    ///
    /// Fighters who live recover between fights. A fighter who dies is gone for the rest of the
    /// run, Sacrifice included (6 Oct 2026: Claude's call after Codex's audit found the dead back
    /// at full health; Jacob let it stand but has not confirmed it word for word). It is what
    /// makes the Medic and Fall Back matter. The numbers live in RunTuning.
    public sealed class SliceRun
    {
        public const string ClarthId = "clarth";
        public const string RaidersId = "raiders";
        public const string SkullpeakId = "skullpeak";
        public const string SplinterId = "splinter";
        public const string ButcherId = "butcher";
        public const int PlayerSide = 0;
        public const int SquadSize = 5;
        public const int RegroupPerFight = 3;

        public const UnitClass ButcherClass = UnitClass.Warden;
        public const UnitClass RaiderChiefClass = UnitClass.Marksman;
        public const UnitClass SkullpeakChiefClass = UnitClass.Trapper;

        /// The stolen Eclipse's grade: 3 for a win this fast with no losses, 2 for one this fast
        /// with at most one loss, else 1.
        public const int CleanWinTicks = 1200;
        public const int QuickWinTicks = 1800;

        private static readonly UnitClass[] ClarthRoster =
        {
            UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Medic
        };

        // The first ones fight; the rest stay home. The raiders who stay home become the chief's splinter crew.
        private static readonly UnitClass[] RaiderRoster =
        {
            UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Medic,
            UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Medic
        };

        private static readonly UnitClass[] SkullpeakRoster =
        {
            UnitClass.Warden, UnitClass.Trapper, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Medic,
            UnitClass.Trapper, UnitClass.Marksman, UnitClass.Warden, UnitClass.Medic, UnitClass.Skirmisher
        };

        private readonly DeterministicRng _seeds;
        private readonly List<RunFighter> _reserve = new List<RunFighter>();
        private readonly RunTuning _tuning;
        private int _fightNumber;
        private int _joined;

        private SliceRun(RunMemory memory, ulong seed, RunTuning tuning)
        {
            Memory = memory;
            Seed = seed;
            _tuning = tuning == null ? new RunTuning() : tuning.Clone();
            memory.RunsStarted++;
            RunNumber = memory.RunsStarted;
            _seeds = DeterministicRng.ForStream(seed, SimStream.Events);
            Archive = new FightArchive();
            Raiders = new Tribe(RaidersId, "Bloodridge", memory.RaiderMind, RaiderRoster) { NerveTracked = true };
            Skullpeak = new Tribe(SkullpeakId, "Skullpeak", memory.SkullpeakMind, SkullpeakRoster) { NerveTracked = true };
            int tier1 = _tuning.TierPercent(1);
            for (int i = 0; i < ClarthRoster.Length; i++)
            {
                UnitClass c = ClarthRoster[i];
                _reserve.Add(new RunFighter(ClarthId + "." + SliceClasses.Name(c).ToLowerInvariant(), ClarthId, "Clarth", c, 1, false, tier1, tier1));
            }
            Stage = RunStage.Raiders;
            memory.Discover("Tribe: Bloodridge raiders");
        }

        /// A new run with the default numbers. The memory carries over from earlier runs; pass a
        /// new RunMemory for the first.
        public static SliceRun Start(RunMemory memory, ulong seed)
        {
            return Start(memory, seed, null);
        }

        /// A new run with these numbers (null for the defaults).
        public static SliceRun Start(RunMemory memory, ulong seed, RunTuning tuning)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));
            return new SliceRun(memory, seed, tuning);
        }

        public RunMemory Memory { get; }
        public ulong Seed { get; }

        /// A copy of the numbers this run plays with.
        public RunTuning Tuning => _tuning.Clone();

        /// 1 for the first run, then up by one with every new run on the same memory.
        public int RunNumber { get; }

        public RunStage Stage { get; private set; }
        public bool Over => Stage == RunStage.Won || Stage == RunStage.Lost;
        public Tribe Raiders { get; }
        public Tribe Skullpeak { get; }

        /// The raider chief's splinter crew: null until he escapes the first break.
        public Tribe Splinter { get; private set; }

        public FightArchive Archive { get; }

        /// Everyone you can put in a squad, in the order they joined. The dead are not in it.
        public IReadOnlyList<RunFighter> Reserve => _reserve;

        /// Your Eclipse's grade, 0 until you steal it.
        public int PlayerEclipseGrade { get; private set; }

        /// The splinter crew's war on Skullpeak happens before the next fight.
        public bool WarPending { get; private set; }

        public int FightsWon { get; private set; }

        /// The piece the Butcher forged after the last win, while you can still say who wears it,
        /// or None. The choice closes when the next war or fight starts.
        public GearKind ForgedGear { get; private set; }

        /// Who wears ForgedGear now: the Butcher's own pick until you choose (the living squad
        /// fighter without gear who did the most damage), or null.
        public RunFighter ForgedWearer { get; private set; }

        /// The tribe you fight next, or null once the run is over.
        public Tribe NextEnemy
        {
            get
            {
                switch (Stage)
                {
                    case RunStage.Raiders: return Raiders;
                    case RunStage.Skullpeak: return Skullpeak;
                    case RunStage.Rematch: return Splinter;
                    default: return null;
                }
            }
        }

        /// The next enemy's tier: 1 raiders, 2 Skullpeak, 3 the rematch.
        public int NextTier => Stage == RunStage.Raiders ? 1 : Stage == RunStage.Skullpeak ? 2 : 3;

        public string NextChiefName => Stage == RunStage.Skullpeak ? "the Skullpeak chief" : "the raider chief";

        public RunFighter FindFighter(string id)
        {
            for (int i = 0; i < _reserve.Count; i++)
            {
                if (string.Equals(_reserve[i].Id, id, StringComparison.Ordinal)) return _reserve[i];
            }
            return null;
        }

        /// Who can wear the forged piece: whoever wears it now first, then everyone in the
        /// reserve without gear, in the order they joined. Empty when nothing is waiting.
        public List<RunFighter> GearCandidates()
        {
            var candidates = new List<RunFighter>();
            if (ForgedGear == GearKind.None || ForgedWearer == null) return candidates;
            candidates.Add(ForgedWearer);
            for (int i = 0; i < _reserve.Count; i++)
            {
                if (_reserve[i].Gear == GearKind.None) candidates.Add(_reserve[i]);
            }
            return candidates;
        }

        /// Gives the forged piece to this fighter instead of the Butcher's pick. Only someone
        /// without gear can take it, and only until the next war or fight starts.
        public bool GiveForgedGear(string fighterId, out string failure)
        {
            failure = string.Empty;
            if (ForgedGear == GearKind.None || ForgedWearer == null)
            {
                failure = "No forged gear is waiting for a wearer.";
                return false;
            }
            RunFighter f = FindFighter(fighterId);
            if (f == null)
            {
                failure = fighterId + " is not in your reserve.";
                return false;
            }
            if (ReferenceEquals(f, ForgedWearer)) return true;
            if (f.Gear != GearKind.None)
            {
                failure = f.DisplayName + " already wears the " + Gear.Name(f.Gear) + ".";
                return false;
            }
            ForgedWearer.Gear = GearKind.None;
            f.Gear = ForgedGear;
            ForgedWearer = f;
            return true;
        }

        /// The autopilot's squad: the strongest Warden, Skirmisher, Marksman, Medic and Trapper,
        /// then the strongest left if a class is missing. Strongest is power plus health, ties to
        /// whoever joined first.
        public List<string> DefaultSquad()
        {
            var picked = new List<RunFighter>();
            UnitClass[] wanted = { UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Marksman, UnitClass.Medic, UnitClass.Trapper };
            int size = Math.Min(SquadSize, _reserve.Count);
            for (int w = 0; w < wanted.Length && picked.Count < size; w++)
            {
                RunFighter best = Strongest(picked, wanted[w], true);
                if (best != null) picked.Add(best);
            }
            while (picked.Count < size) picked.Add(Strongest(picked, UnitClass.Warden, false));
            var ids = new List<string>(picked.Count);
            for (int i = 0; i < picked.Count; i++) ids.Add(picked[i].Id);
            return ids;
        }

        private RunFighter Strongest(List<RunFighter> taken, UnitClass unitClass, bool matchClass)
        {
            RunFighter best = null;
            for (int i = 0; i < _reserve.Count; i++)
            {
                RunFighter f = _reserve[i];
                if (taken.Contains(f) || (matchClass && f.Class != unitClass)) continue;
                if (best == null || f.PowerPercent + f.HealthPercent > best.PowerPercent + best.HealthPercent) best = f;
            }
            return best;
        }

        /// Starts the next fight with the Butcher and this squad. The squad must be exactly 5
        /// different fighters from the reserve (all of it, if it holds fewer). Issue orders
        /// through a session on the setup, tick until it ends, then call FinishFight.
        public BattleSetup NextFight(IReadOnlyList<string> squadIds)
        {
            if (Over) throw new InvalidOperationException("The run is over.");
            if (WarPending) throw new InvalidOperationException("The war comes first: call RunWar.");
            if (squadIds == null) throw new ArgumentNullException(nameof(squadIds));
            int size = Math.Min(SquadSize, _reserve.Count);
            if (squadIds.Count != size) throw new ArgumentException("Pick exactly " + size + " fighters.");
            var squad = new List<RunFighter>(size);
            for (int i = 0; i < squadIds.Count; i++)
            {
                RunFighter f = FindFighter(squadIds[i]);
                if (f == null) throw new ArgumentException(squadIds[i] + " is not in your reserve.");
                if (squad.Contains(f)) throw new ArgumentException(squadIds[i] + " is picked twice.");
                squad.Add(f);
            }
            CloseGearChoice();

            Tribe enemy = NextEnemy;
            int tier = NextTier;
            string fightId = NextFightId(enemy.Id);
            var side0 = new List<UnitSpec>(size + 1)
            {
                new UnitSpec(ButcherId, ButcherClass, SliceClasses.Stats(ButcherClass).DefaultRow, UnitRole.Pal,
                    _tuning.ButcherPowerPercent, _tuning.ButcherHealthPercent)
            };
            for (int i = 0; i < squad.Count; i++)
            {
                RunFighter f = squad[i];
                side0.Add(new UnitSpec(f.Id, f.Class, SliceClasses.Stats(f.Class).DefaultRow, UnitRole.Fighter,
                    f.PowerPercent, f.HealthPercent, f.FearPercent));
            }
            UnitClass chiefClass = Stage == RunStage.Skullpeak ? SkullpeakChiefClass : RaiderChiefClass;
            List<UnitSpec> side1 = DeployEnemy(enemy, fightId, tier, chiefClass, true);
            int enemyGrade = Stage == RunStage.Raiders ? _tuning.ChiefEclipseGrade : Stage == RunStage.Rematch ? _tuning.RematchEclipseGrade : 0;
            Memory.Discover(Stage == RunStage.Skullpeak ? "Tribe: Skullpeak" : "Chief: the raider chief");

            return new BattleSetup(fightId, NextSeed(), side0, side1,
                Memory.ClarthMind.CopyTactics(), enemy.Mind.CopyTactics(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints,
                NerveRules.On(NerveRules.Full, enemy.Nerve).WithOrderCosts(_tuning.UnleashNerve, _tuning.HoldDrainEveryTicks),
                SliceRules.RefillTicks, SliceRules.HealthPercent,
                new SideRules(PlayerEclipseGrade, 0, true),
                new SideRules(enemyGrade, enemyGrade > 0 ? _tuning.AutoEclipseTicks : 0, false));
        }

        /// Up to EnemyDeploy of the tribe's fighters at its tier's strength, then its chief in the
        /// back row when it brings one.
        private List<UnitSpec> DeployEnemy(Tribe tribe, string fightId, int tier, UnitClass chiefClass, bool withChief)
        {
            int percent = _tuning.TierPercent(tier);
            var specs = new List<UnitSpec>();
            int count = Math.Min(_tuning.EnemyDeploy, tribe.Strength);
            for (int i = 0; i < count; i++)
            {
                UnitClass c = tribe.Fighters[i];
                specs.Add(new UnitSpec(fightId + "." + tribe.Id + "." + i.ToString(CultureInfo.InvariantCulture), c,
                    SliceClasses.Stats(c).DefaultRow, UnitRole.Fighter, percent, percent));
            }
            if (withChief)
                specs.Add(new UnitSpec(fightId + "." + tribe.Id + ".chief", chiefClass, Row.Back, UnitRole.Chief,
                    percent * _tuning.ChiefPowerPercent / 100, percent * _tuning.ChiefHealthPercent / 100));
            return specs;
        }

        /// Archives the fight and lets both sides learn. Your fighters who died leave the reserve
        /// for good. A loss ends the run. A win breaks the tribe: its survivors join you (the
        /// raider chief escapes the first time with the crew who stayed home, and his Eclipse is
        /// yours), the Butcher learns the broken chief's gear recipe and forges it (or a trophy,
        /// once he knows it) for a living fighter you can change before the next fight, and the
        /// tribes still standing regroup.
        public RunFightResult FinishFight(BattleSetup setup, SimSession session)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (Over) throw new InvalidOperationException("The run is over.");
            var sim = session.Simulation as BattleSimulation;
            if (sim == null || !ReferenceEquals(sim.Setup, setup))
                throw new ArgumentException("The session must be running this setup.");
            if (!sim.IsFinished) throw new InvalidOperationException("The fight has not finished.");

            Archive.Add(setup, session.Log, sim);
            FightRecord record = Archive.Find(setup.FightId).Record;
            RunStage fought = Stage;
            Tribe enemy = NextEnemy;
            int fieldSurvivors = 0;
            for (int i = 0; i < sim.Units.Count; i++)
            {
                BattleUnit u = sim.Units[i];
                if (u.Side == 1 && !u.IsLeader && u.Alive) fieldSurvivors++;
            }
            int enemyDead = enemy.RemoveDead(sim, 1);
            int playerDead = 0;
            var fallen = new List<RunFighter>();
            for (int i = 0; i < sim.Units.Count; i++)
            {
                BattleUnit u = sim.Units[i];
                if (u.Side != PlayerSide || u.Alive) continue;
                playerDead++;
                RunFighter f = u.Role == UnitRole.Fighter ? FindFighter(u.Id) : null;
                if (f == null) continue;
                _reserve.Remove(f);
                fallen.Add(f);
            }
            BattleUnit butcher = sim.FindById(ButcherId);
            bool butcherDown = butcher != null && !butcher.Alive;
            IReadOnlyList<Lesson> playerLessons = LessonRules.Learn(Memory.ClarthMind, record, PlayerSide);
            IReadOnlyList<Lesson> enemyLessons = LessonRules.Learn(enemy.Mind, record, 1);
            for (int i = 0; i < record.Events.Count; i++)
            {
                BattleEvent e = record.Events[i];
                if (e.Kind == BattleEventKind.Combo && e.ActorSide == PlayerSide) Memory.Discover("Combo: " + e.Detail);
            }

            bool won = record.Winner == PlayerSide;
            if (!won)
            {
                enemy.Wins++;
                Stage = RunStage.Lost;
                return new RunFightResult(record, fought, false, enemyDead, playerDead, sim.BrokenSide == PlayerSide, butcherDown,
                    new List<RunFighter>(), fallen, false, false, 0, GearKind.None, null, GearKind.None, playerLessons, enemyLessons,
                    true, false);
            }

            FightsWon++;
            enemy.Losses++;
            var joined = new List<RunFighter>();
            bool chiefEscaped = false;
            bool chiefCaught = false;
            int stolenGrade = 0;
            GearKind learned = GearKind.None;
            GearKind forged;
            List<UnitClass> survivors = enemy.LeaveToJoinPlayer();
            switch (fought)
            {
                case RunStage.Raiders:
                {
                    // Those who fought and lived join you; the chief runs with those who stayed home.
                    var home = new List<UnitClass>();
                    for (int i = 0; i < survivors.Count; i++)
                    {
                        if (i < fieldSurvivors) joined.Add(Join(enemy, survivors[i], 1, false));
                        else home.Add(survivors[i]);
                    }
                    if (home.Count == 0) home.Add(UnitClass.Skirmisher);
                    Splinter = new Tribe(SplinterId, "Bloodridge", Memory.RaiderMind, home) { NerveTracked = true };
                    chiefEscaped = true;
                    stolenGrade = GradeFor(record.EndTick, playerDead);
                    PlayerEclipseGrade = Math.Max(PlayerEclipseGrade, stolenGrade);
                    Memory.Discover("Move: Eclipse");
                    learned = GearKind.LavaSunTabard;
                    forged = GearKind.LavaSunTabard;
                    WarPending = true;
                    Stage = RunStage.Skullpeak;
                    break;
                }
                case RunStage.Skullpeak:
                    for (int i = 0; i < survivors.Count; i++) joined.Add(Join(enemy, survivors[i], 2, false));
                    joined.Add(Join(enemy, SkullpeakChiefClass, 2, true));
                    chiefCaught = true;
                    Memory.Discover("Chief caught: Skullpeak");
                    learned = GearKind.SteelRibPlate;
                    forged = GearKind.SteelRibPlate;
                    Stage = RunStage.Rematch;
                    break;
                default:
                    // His crew are Bloodridge raiders, and so is he: they join under the raiders' name.
                    for (int i = 0; i < survivors.Count; i++) joined.Add(Join(Raiders, survivors[i], 3, false));
                    joined.Add(Join(Raiders, RaiderChiefClass, 3, true));
                    chiefCaught = true;
                    Memory.Discover("Chief caught: the raider chief");
                    forged = GearKind.ChiefsFang;
                    Stage = RunStage.Won;
                    Memory.RunsWon++;
                    break;
            }
            _reserve.AddRange(joined);
            if (learned != GearKind.None) Memory.Discover("Recipe: " + Gear.Name(learned));

            // The dead have already left the reserve, so the gear always goes to someone alive.
            RunFighter wearer = TopDamageWithoutGear(record, setup) ?? FirstWithoutGear();
            if (wearer != null)
            {
                wearer.Gear = forged;
                Memory.Discover("Gear: " + Gear.Name(forged));
                ForgedGear = forged;
                ForgedWearer = wearer;
            }
            else forged = GearKind.None;

            RegroupStanding();
            return new RunFightResult(record, fought, true, enemyDead, playerDead, false, butcherDown, joined, fallen,
                chiefEscaped, chiefCaught, stolenGrade, learned, wearer, forged, playerLessons, enemyLessons,
                Stage == RunStage.Won, Stage == RunStage.Won);
        }

        /// The stolen Eclipse's grade from how fast you won and how many you lost.
        public static int GradeFor(long endTick, int playerDead)
        {
            if (endTick <= CleanWinTicks && playerDead == 0) return 3;
            if (endTick <= QuickWinTicks && playerDead <= 1) return 2;
            return 1;
        }

        /// The splinter crew raids Skullpeak off screen. Chiefs stay out of it. Both sides learn,
        /// the dead leave the rosters, and each keeps its nerve, never below the war floor.
        public RunWarResult RunWar()
        {
            if (!WarPending) throw new InvalidOperationException("There is no war to fight.");
            WarPending = false;
            CloseGearChoice();
            Tribe a = Splinter;
            Tribe b = Skullpeak;
            string fightId = NextFightId("war");
            if (a.Strength == 0 || b.Strength == 0)
            {
                RegroupStanding();
                return new RunWarResult(fightId, -1, 0, 0, new List<Lesson>(), new List<Lesson>(), false);
            }

            var setup = new BattleSetup(fightId, NextSeed(), DeployEnemy(a, fightId, 3, RaiderChiefClass, false),
                DeployEnemy(b, fightId, 2, SkullpeakChiefClass, false),
                a.Mind.CopyTactics(), b.Mind.CopyTactics(), BattleSetup.DefaultTickLimit, 0,
                NerveRules.On(a.Nerve, b.Nerve), 0, SliceRules.HealthPercent);
            var sim = new BattleSimulation(setup);
            sim.RunUntilFinished();
            Archive.Add(setup, null, sim);
            FightRecord record = Archive.Find(fightId).Record;
            int deadA = a.RemoveDead(sim, 0);
            int deadB = b.RemoveDead(sim, 1);
            IReadOnlyList<Lesson> lessonsA = LessonRules.Learn(a.Mind, record, 0);
            IReadOnlyList<Lesson> lessonsB = LessonRules.Learn(b.Mind, record, 1);
            if (record.Winner == 0)
            {
                a.Wins++;
                b.Losses++;
            }
            else if (record.Winner == 1)
            {
                b.Wins++;
                a.Losses++;
            }
            a.Nerve = Math.Max(_tuning.WarNerveFloor, sim.Nerve(0));
            b.Nerve = Math.Max(_tuning.WarNerveFloor, sim.Nerve(1));
            RegroupStanding();
            return new RunWarResult(fightId, record.Winner, deadA, deadB, lessonsA, lessonsB, true);
        }

        public string Snapshot()
        {
            var b = new SnapshotBuilder();
            b.Section("run");
            b.Field("seed", Seed);
            b.Field("run", RunNumber);
            b.Field("stage", (int)Stage);
            b.Field("fights", _fightNumber);
            b.Field("seedRng", _seeds.State);
            b.Field("eclipse", PlayerEclipseGrade);
            b.Field("war", WarPending);
            b.Field("won", FightsWon);
            b.Field("forged", Gear.Name(ForgedGear));
            b.Field("forgedWearer", ForgedWearer == null ? string.Empty : ForgedWearer.Id);
            _tuning.AppendTo(b);
            IReadOnlyList<ArchivedFight> fights = Archive.Fights;
            for (int i = 0; i < fights.Count; i++) b.Field(fights[i].FightId, fights[i].Record.Checksum);
            for (int i = 0; i < _reserve.Count; i++)
            {
                RunFighter f = _reserve[i];
                b.Field("r" + i.ToString(CultureInfo.InvariantCulture), f.Id + ":" + SliceClasses.Name(f.Class) + ":" +
                    f.Tier.ToString(CultureInfo.InvariantCulture) + ":" + (f.Chief ? "chief" : "fighter") + ":" +
                    f.PowerPercent.ToString(CultureInfo.InvariantCulture) + ":" + f.HealthPercent.ToString(CultureInfo.InvariantCulture) + ":" +
                    Gear.Name(f.Gear));
            }
            return b.ToString() + Raiders.Snapshot() + Skullpeak.Snapshot() +
                (Splinter == null ? string.Empty : Splinter.Snapshot()) + Memory.Snapshot();
        }

        private RunFighter Join(Tribe from, UnitClass unitClass, int tier, bool chief)
        {
            _joined++;
            string id = chief
                ? from.Id + ".chief"
                : from.Id + "." + SliceClasses.Name(unitClass).ToLowerInvariant() + "." + _joined.ToString(CultureInfo.InvariantCulture);
            int percent = _tuning.TierPercent(tier);
            int power = chief ? percent * _tuning.ChiefPowerPercent / 100 : percent;
            int health = chief ? percent * _tuning.ChiefHealthPercent / 100 : percent;
            return new RunFighter(id, from.Id, from.DisplayName, unitClass, tier, chief, power, health);
        }

        /// The living squad fighter who did the most damage this fight and has no gear yet. Ties
        /// go to the one picked first. The Butcher forges; he does not wear.
        private RunFighter TopDamageWithoutGear(FightRecord record, BattleSetup setup)
        {
            IReadOnlyList<UnitSpec> side = setup.Side(PlayerSide);
            RunFighter best = null;
            int bestDamage = -1;
            for (int s = 0; s < side.Count; s++)
            {
                if (side[s].Role != UnitRole.Fighter) continue;
                RunFighter f = FindFighter(side[s].Id);
                if (f == null || f.Gear != GearKind.None) continue;
                int damage = 0;
                for (int i = 0; i < record.Events.Count; i++)
                {
                    BattleEvent e = record.Events[i];
                    if (e.Kind == BattleEventKind.Hit && e.ActorSide == PlayerSide &&
                        string.Equals(e.ActorId, f.Id, StringComparison.Ordinal))
                        damage += e.Amount;
                }
                if (damage > bestDamage)
                {
                    best = f;
                    bestDamage = damage;
                }
            }
            return best;
        }

        /// The first fighter in the reserve without gear, for when nobody from the squad can take it.
        private RunFighter FirstWithoutGear()
        {
            for (int i = 0; i < _reserve.Count; i++)
            {
                if (_reserve[i].Gear == GearKind.None) return _reserve[i];
            }
            return null;
        }

        private void CloseGearChoice()
        {
            ForgedGear = GearKind.None;
            ForgedWearer = null;
        }

        private void RegroupStanding()
        {
            Tribe[] tribes = { Raiders, Skullpeak, Splinter };
            for (int i = 0; i < tribes.Length; i++)
            {
                if (tribes[i] == null || tribes[i].Broken) continue;
                tribes[i].Regroup(RegroupPerFight);
                tribes[i].RecoverNerve(Tribe.NerveRecoveryPerRegroup);
            }
        }

        private string NextFightId(string tag)
        {
            _fightNumber++;
            return "run" + RunNumber.ToString(CultureInfo.InvariantCulture) + ".f" +
                _fightNumber.ToString(CultureInfo.InvariantCulture) + "." + tag;
        }

        private ulong NextSeed()
        {
            ulong high = _seeds.NextUInt();
            ulong low = _seeds.NextUInt();
            return (high << 32) | low;
        }
    }

    public sealed class RunFightResult
    {
        internal RunFightResult(FightRecord record, RunStage fought, bool won, int enemyDead, int playerDead,
            bool playerBroke, bool butcherDown, IReadOnlyList<RunFighter> joined, IReadOnlyList<RunFighter> fallen,
            bool chiefEscaped, bool chiefCaught, int stolenGrade, GearKind learnedRecipe, RunFighter gearWearer, GearKind forged,
            IReadOnlyList<Lesson> playerLessons, IReadOnlyList<Lesson> enemyLessons, bool runOver, bool runWon)
        {
            Record = record;
            Fought = fought;
            Won = won;
            EnemyDead = enemyDead;
            PlayerDead = playerDead;
            PlayerBroke = playerBroke;
            ButcherDown = butcherDown;
            Joined = joined;
            Fallen = fallen;
            ChiefEscaped = chiefEscaped;
            ChiefCaught = chiefCaught;
            StolenGrade = stolenGrade;
            LearnedRecipe = learnedRecipe;
            GearWearer = gearWearer;
            Forged = forged;
            PlayerLessons = playerLessons;
            EnemyLessons = enemyLessons;
            RunOver = runOver || !won;
            RunWon = runWon;
        }

        public FightRecord Record { get; }

        /// Which fight of the run this was.
        public RunStage Fought { get; }

        public bool Won { get; }
        public int EnemyDead { get; }

        /// Everyone on your side who died, the Butcher included.
        public int PlayerDead { get; }

        /// Your line broke: courage gone, or the Butcher dropped.
        public bool PlayerBroke { get; }

        public bool ButcherDown { get; }

        /// Who joined your reserve after this fight, in order.
        public IReadOnlyList<RunFighter> Joined { get; }

        /// Squad fighters who died this fight and left the reserve for good, in squad order.
        public IReadOnlyList<RunFighter> Fallen { get; }

        public bool ChiefEscaped { get; }
        public bool ChiefCaught { get; }

        /// The grade of the Eclipse you stole this fight, or 0.
        public int StolenGrade { get; }

        /// The broken chief's gear recipe the Butcher learned this fight, or None.
        public GearKind LearnedRecipe { get; }

        /// The Butcher's pick for the forged gear, or null. SliceRun.GiveForgedGear can change it
        /// before the next fight; SliceRun.ForgedWearer says who wears it now.
        public RunFighter GearWearer { get; }

        public GearKind Forged { get; }
        public IReadOnlyList<Lesson> PlayerLessons { get; }
        public IReadOnlyList<Lesson> EnemyLessons { get; }
        public bool RunOver { get; }
        public bool RunWon { get; }
    }

    public sealed class RunWarResult
    {
        internal RunWarResult(string fightId, int winner, int splinterDead, int skullpeakDead,
            IReadOnlyList<Lesson> splinterLessons, IReadOnlyList<Lesson> skullpeakLessons, bool fought)
        {
            FightId = fightId;
            Winner = winner;
            SplinterDead = splinterDead;
            SkullpeakDead = skullpeakDead;
            SplinterLessons = splinterLessons;
            SkullpeakLessons = skullpeakLessons;
            Fought = fought;
        }

        public string FightId { get; }

        /// 0 the splinter crew won, 1 Skullpeak won, -1 a draw or no war.
        public int Winner { get; }

        public int SplinterDead { get; }
        public int SkullpeakDead { get; }

        /// What the raider chief took from the war.
        public IReadOnlyList<Lesson> SplinterLessons { get; }

        public IReadOnlyList<Lesson> SkullpeakLessons { get; }

        /// False when one side had nobody to send.
        public bool Fought { get; }
    }
}
