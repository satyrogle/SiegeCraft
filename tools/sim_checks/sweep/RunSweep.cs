using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

// Whole runs for the Butcher's hooked combos (29 Oct 2026, later). Each build is worn from the run's start, so the bonds
// one fight forms carry into the next, and the run plays to its end taking no favour and no spoils, so the combos are the
// only gear. Every build starts from the same campaign seed, tutorial and run start, with the same route picker and fight
// pilot seeds. One CSV line per run: seed,build,runWon,fightsWon,fights,bonds.
// The fights mode writes one line per fight instead (FightLine).
// The duels mode does the same with a chosen duelist (Run's duelist).
//   args: mode (calib|pairs|fights|duels) seedStart seedCount enemyScale style
public static class RunSweep
{
    private static CampaignTuning Tuning(int scale) => new CampaignTuning { EnemyScaleTier1 = scale, EnemyScaleTier2 = scale * 86 / 95 };

    // How often in a thousand the fight pilots parry a champion's beat in a duel (CampaignPilot.DuelParryPermille); the
    // optional seventh argument, 0 by default.
    private static int _parryPermille;

    private static readonly ClassBend[] All =
        Enumerable.Range((int)ClassBend.BlackHoleRound, 15).Select(i => (ClassBend)i).ToArray();

    // The five classes, in the order each augment's combos come.
    private static readonly UnitClass[] DuelClasses =
        { UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Trapper, UnitClass.Medic };

    // The run's start with these combos in its gear, each worn by the first fighter of its class. Null if one can't be.
    private static Campaign Equip(string save, CampaignTuning t, IList<ClassBend> combos)
    {
        if (combos.Count == 0) return CampaignSave.Read(save, t);
        var lines = save.Split('\n').ToList();
        int at = lines.FindLastIndex(l => l.StartsWith("fighter\t"));
        if (at < 0) return null;
        foreach (ClassBend bend in combos)
            lines.Insert(at + 1, string.Join("\t", "piece", Campaign.ComboGearId(bend), CampaignTribes.ClarthId, "0", "0", "0",
                ClassSpells.Name(bend), ((int)ClassSpells.ClassOf(bend)).ToString(), ((int)bend).ToString()));
        Campaign c = CampaignSave.Read(string.Join("\n", lines), t);
        if (c.LoadNote.Length > 0) return null;
        foreach (ClassBend bend in combos)
        {
            CampaignFighter f = c.Reserve.FirstOrDefault(x => x.Class == ClassSpells.ClassOf(bend));
            if (f == null || !c.TryEquip(f.Id, Campaign.ComboGearId(bend), out _)) return null;
        }
        return c;
    }

    // One line per fight for the fights mode: seed,build,n,kind,tribe,won,ticks,livesAfter, then our side's damage on
    // theirs: all of it, on their chief, by bleeding, by rips, bleeds and rips on their chief, Event Horizon's cleave, and
    // how many Event Horizon pulls.
    private static string FightLine(ulong seed, string build, int n, CampaignFight f, EncounterResult r, Campaign c)
    {
        string[] id = f.Setup.FightId.Split('.');
        string kind = id[id.Length - 1], tribe = id.Length >= 4 ? id[2] : "-";
        long all = 0, chief = 0, bleed = 0, rip = 0, chiefWound = 0, cleave = 0;
        int pulls = 0;
        string horizon = ClassSpells.Name(ClassBend.EventHorizon);
        foreach (BattleEvent e in f.Sim.Events)
        {
            if (e.Kind == BattleEventKind.Pull && e.Detail == horizon) pulls++;
            if (e.Kind != BattleEventKind.Hit || e.ActorSide != 0) continue;
            BattleUnit v = f.Sim.FindById(e.TargetId);
            if (v == null || v.Side != 1) continue;
            bool isChief = v.Role == UnitRole.Chief;
            all += e.Amount;
            if (isChief) chief += e.Amount;
            bool wound = e.Detail == BleedRules.Detail || e.Detail == ClassSpells.RipDetail;
            if (e.Detail == BleedRules.Detail) bleed += e.Amount;
            if (e.Detail == ClassSpells.RipDetail) rip += e.Amount;
            if (wound && isChief) chiefWound += e.Amount;
            if (e.Detail == ClassSpells.EventHorizonDetail) cleave += e.Amount;
        }
        return string.Join(",", seed, build, n, kind, tribe, r.Won ? 1 : 0, f.Sim.CurrentTick, c.Lives, all, chief, bleed, rip, chiefWound, cleave, pulls);
    }

    // Plays the whole run from its start with these combos worn; null when they can't be worn. With a fight log, every
    // fight's line goes there too. With a duelist class, every duel sends that class's first fighter (the one a combo of
    // that class is worn by) instead of the default.
    private static string Run(string save, CampaignTuning t, IList<ClassBend> combos, PilotStyle style, ulong seed,
        string build = null, TextWriter fightLog = null, UnitClass? duelist = null)
    {
        Campaign c = Equip(save, t, combos);
        if (c == null) return null;
        var picker = new CampaignPilot(PilotStyle.Reference, seed);
        int won = 0, fights = 0, bonds = 0;
        bool runWon = false;
        for (int n = 0; c.Stage == CampaignStage.InRun && n < 80; n++)
        {
            int offer = picker.PickOffer(c);
            string[] squad = null;
            if (duelist != null && c.Offers[offer].Kind == EncounterKind.Duel)
            {
                CampaignFighter sent = c.Reserve.FirstOrDefault(x => x.Class == duelist.Value);
                if (sent != null) squad = new[] { sent.Id };
            }
            CampaignFight f = c.StartEncounter(offer, squad);
            EncounterResult r = CampaignPilot.PlayFight(c, f, new CampaignPilot(style, seed * 31UL + (ulong)n) { DuelParryPermille = _parryPermille });
            if (fightLog != null) fightLog.WriteLine(FightLine(seed, build, n, f, r, c));
            fights++;
            if (r.Won) won++;
            bonds = Math.Max(bonds, c.Bonds.Count);
            if (r.RunOver)
            {
                runWon = r.RunWon;
                break;
            }
        }
        return (runWon ? 1 : 0) + "," + won + "," + fights + "," + bonds;
    }

    public static int Main(string[] args)
    {
        string mode = args[0];
        ulong seed0 = ulong.Parse(args[1]);
        int count = int.Parse(args[2]);
        int scale = int.Parse(args[3]);
        PilotStyle style = (PilotStyle)Enum.Parse(typeof(PilotStyle), args[4]);
        if (args.Length > 5) _parryPermille = int.Parse(args[5]);
        // The tune mode's two duel numbers, the 7th and 8th args, on a scratch sim where they are fields, not consts.
        if (args.Length > 7)
        {
            typeof(HeavyRules).GetField("DuelParryNerve").SetValue(null, int.Parse(args[6]));
            typeof(HeavyRules).GetField("DuelDamagePercent").SetValue(null, int.Parse(args[7]));
        }
        CampaignTuning t = Tuning(scale);
        TextWriter o = Console.Out;
        for (ulong seed = seed0; seed < seed0 + (ulong)count; seed++)
        {
            Campaign start = Campaign.New(seed, t);
            if (!CampaignPilot.PlayTutorial(start, PilotStyle.Reference, seed, 10)) continue;
            start.StartRun();
            string save = CampaignSave.Write(start);
            var builds = new List<(string name, ClassBend[] combos)> { ("plain", new ClassBend[0]) };
            // The duels mode: as the fights mode, but every duel sends a chosen fighter. The plain squad with the default
            // duelist, the plain squad sending each class's first fighter, and each combo with its wearer sent.
            if (mode == "duels")
            {
                Run(save, t, new ClassBend[0], style, seed, "plain", o);
                foreach (UnitClass cls in DuelClasses) Run(save, t, new ClassBend[0], style, seed, "plain." + cls, o, cls);
                foreach (ClassBend b in All) Run(save, t, new[] { b }, style, seed, b.ToString(), o, ClassSpells.ClassOf(b));
                o.Flush();
                continue;
            }
            // The tune mode: only the plain squad sending each class's first fighter to every duel.
            if (mode == "tune")
            {
                foreach (UnitClass cls in DuelClasses) Run(save, t, new ClassBend[0], style, seed, "plain." + cls, o, cls);
                o.Flush();
                continue;
            }
            // The fights mode: the plain squad and each combo alone, every fight of every run on its own line.
            if (mode == "fights")
            {
                foreach (ClassBend b in All) builds.Add((b.ToString(), new[] { b }));
                foreach (var (name, combos) in builds) Run(save, t, combos, style, seed, name, o);
                o.Flush();
                continue;
            }
            if (mode == "pairs")
            {
                foreach (ClassBend b in All) builds.Add((b.ToString(), new[] { b }));
                for (int i = 0; i < All.Length; i++)
                    for (int j = i + 1; j < All.Length; j++)
                        if (ClassSpells.ClassOf(All[i]) != ClassSpells.ClassOf(All[j]))
                            builds.Add((All[i] + "+" + All[j], new[] { All[i], All[j] }));
            }
            foreach (var (name, combos) in builds)
            {
                string line = Run(save, t, combos, style, seed);
                if (line != null) o.WriteLine(seed + "," + name + "," + line);
            }
            o.Flush();
        }
        return 0;
    }
}
