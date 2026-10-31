using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

// The strength and synergy sweep for the Butcher's 15 combos on the real sim (29 Oct 2026). It walks real runs with the
// reference pilot, saves the campaign just before each fight, then replays that same fight once per build: the plain
// squad, or the squad with combos put on the first fighter of each combo's class. Same save, same fight seed, same pilot
// seed, so the only difference is the build. One CSV line per fight played.
//   args: mode (calib|singles|pairs|builds) seedStart seedCount enemyScale style fights
public static class Sweep
{
    private static CampaignTuning Tuning(int scale) => new CampaignTuning { EnemyScaleTier1 = scale, EnemyScaleTier2 = scale * 86 / 95 };

    // The fights a run meets, as the save just before each and the offer picked.
    private static List<(string save, int offer, string kind)> Road(ulong seed, CampaignTuning t, int fights)
    {
        var list = new List<(string, int, string)>();
        Campaign c = Campaign.New(seed, t);
        if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) return list;
        c.StartRun();
        var picker = new CampaignPilot(PilotStyle.Reference, seed);
        for (int n = 0; n < fights && c.Stage == CampaignStage.InRun; n++)
        {
            int offer = picker.PickOffer(c);
            Encounter e = c.Offers[offer];
            string kind = e.Kind + (e.Node != null && e.Node.Pal != Pal.None ? ".pal" : string.Empty);
            list.Add((CampaignSave.Write(c), offer, kind));
            CampaignPilot.PlayFight(c, c.StartEncounter(offer), new CampaignPilot(PilotStyle.Reference, seed * 31UL + (ulong)n));
        }
        return list;
    }

    // The save with these combos in the run's gear, each worn by the first fighter of its class. Null if one can't be.
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

    private static int Share(BattleSimulation sim, int side)
    {
        long hp = 0, max = 0;
        foreach (BattleUnit u in sim.Units)
        {
            if (u.Side != side || u.Benched) continue;
            hp += Math.Max(0, u.Hp);
            max += u.MaxHp;
        }
        return max <= 0 ? 0 : (int)(hp * 1000 / max);
    }

    // Plays the fight at this save with these combos; null when they can't be worn or a wearer sits this fight out.
    private static string Play(string save, int offer, CampaignTuning t, IList<ClassBend> combos, PilotStyle style, ulong pilotSeed)
    {
        Campaign c = Equip(save, t, combos);
        if (c == null) return null;
        CampaignFight f = c.StartEncounter(offer);
        foreach (ClassBend bend in combos)
        {
            string wearer = c.Reserve.First(x => x.Class == ClassSpells.ClassOf(bend)).Id;
            if (!f.Setup.Side(0).Any(s => s.Id == wearer && s.Bend == bend)) return null;
        }
        EncounterResult r = CampaignPilot.PlayFight(c, f, new CampaignPilot(style, pilotSeed));
        int fx = f.Sim.Events.Count(e => (int)e.Kind >= 44 && (int)e.Kind <= 46);
        return (r.Won ? 1 : 0) + "," + (Share(f.Sim, 0) - Share(f.Sim, 1)) + "," + f.Sim.CurrentTick + "," + fx;
    }

    private static readonly ClassBend[] All =
        Enumerable.Range((int)ClassBend.BlackHoleRound, 15).Select(i => (ClassBend)i).ToArray();

    public static int Main(string[] args)
    {
        string mode = args[0];
        ulong seed0 = ulong.Parse(args[1]);
        int count = int.Parse(args[2]);
        int scale = int.Parse(args[3]);
        PilotStyle style = (PilotStyle)Enum.Parse(typeof(PilotStyle), args[4]);
        int fights = int.Parse(args[5]);
        CampaignTuning t = Tuning(scale);
        TextWriter o = Console.Out;
        for (ulong seed = seed0; seed < seed0 + (ulong)count; seed++)
        {
            List<(string save, int offer, string kind)> road = Road(seed, t, fights);
            for (int n = 0; n < road.Count; n++)
            {
                var (save, offer, kind) = road[n];
                ulong pilotSeed = seed * 1000003UL + (ulong)n;
                var builds = new List<(string name, ClassBend[] combos)> { ("plain", new ClassBend[0]) };
                if (mode == "singles" || mode == "pairs")
                    foreach (ClassBend b in All) builds.Add((b.ToString(), new[] { b }));
                if (mode == "pairs")
                    for (int i = 0; i < All.Length; i++)
                        for (int j = i + 1; j < All.Length; j++)
                            if (ClassSpells.ClassOf(All[i]) != ClassSpells.ClassOf(All[j]))
                                builds.Add((All[i] + "+" + All[j], new[] { All[i], All[j] }));
                if (mode == "builds")
                {
                    // Random whole builds: each class wears one of its three combos or none, from the fight's own dice.
                    var dice = new DeterministicRng(pilotSeed, 99UL);
                    for (int k = 0; k < 40; k++)
                    {
                        var combos = new List<ClassBend>();
                        for (int cls = 0; cls < 5; cls++)
                        {
                            int pick = dice.Next(4);
                            if (pick < 3) combos.Add(All.Where(b => (int)ClassSpells.ClassOf(b) == cls).ElementAt(pick));
                        }
                        builds.Add((string.Join("+", combos), combos.ToArray()));
                    }
                }
                foreach (var (name, combos) in builds)
                {
                    string line = Play(save, offer, t, combos, style, pilotSeed);
                    if (line != null) o.WriteLine(seed + "," + n + "," + kind + "," + name + "," + line);
                }
                o.Flush();
            }
        }
        return 0;
    }
}
