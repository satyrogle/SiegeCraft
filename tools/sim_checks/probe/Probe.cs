using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

// Runs the same fights, campaigns and saves through whichever sim build this project references and prints what came out,
// so the build from before Black Hole Round (rules 14, save 22) and the build after it (rules 15, save 23) can be compared
// line by line. Everything here plays rules 14 or older, which the new build must leave exactly as it was.
public static class Probe
{
    private static UnitSpec U(string id, UnitClass c, UnitRole role = UnitRole.Fighter, int health = 400)
    {
        return new UnitSpec(id, c, SliceClasses.Stats(c).DefaultRow, role, 100, health);
    }

    private static List<UnitSpec> Ours(string p)
    {
        return new List<UnitSpec>
        {
            new UnitSpec(p + "pal", UnitClass.Warden, Row.Back, UnitRole.Pal, 120, 120),
            U(p + "w", UnitClass.Warden), U(p + "s", UnitClass.Skirmisher), U(p + "m", UnitClass.Marksman), U(p + "h", UnitClass.Medic),
            U(p + "t", UnitClass.Trapper)
        };
    }

    // Ours, every fighter wearing its class's bend number `which` (0 or 1): all ten class pieces across the two.
    private static List<UnitSpec> OursBent(string p, int which)
    {
        List<UnitSpec> list = Ours(p);
        for (int i = 0; i < list.Count; i++)
            if (list[i].Role == UnitRole.Fighter) list[i] = list[i].WithBend(ClassSpells.BendOf(list[i].Class, which));
        return list;
    }

    private static List<UnitSpec> Band(string p, UnitClass chief)
    {
        return new List<UnitSpec>
        {
            U(p + "x.w", UnitClass.Warden), U(p + "x.m", UnitClass.Marksman), U(p + "x.t", UnitClass.Trapper), U(p + "x.h", UnitClass.Medic),
            U(p + "x.s", UnitClass.Skirmisher), U(p + "chief", chief, UnitRole.Chief, 600)
        };
    }

    private static List<UnitSpec> Beast(string p, BeastKit kit)
    {
        var parts = new List<UnitSpec>();
        IReadOnlyList<BeastPart> order = BeastKits.Parts(kit);
        for (int i = 0; i < order.Count; i++)
            parts.Add(new UnitSpec(p + "beast." + BeastKits.Name(order[i]), UnitClass.Warden, BeastKits.RowOf(order[i]), UnitRole.Fighter, 100,
                BeastKits.HealthPercent(order[i])));
        return parts;
    }

    // The rules 14 stack a road fight plays on top of its kind's rules (Campaign.BuildSetup).
    private static FightRules Road14(FightRules f)
    {
        return f.WithParryCenter().WithSquadFirst().WithRaidChief().WithRoadCombat().WithRoadKit().WithClearRoad(4).WithSquadScreen();
    }

    private static string Fight(string name, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, SideRules rules, FightRules fight,
        TacticsProfile tactics)
    {
        var setup = new BattleSetup(name, seed, ours, theirs, new TacticsProfile(), tactics ?? new TacticsProfile(),
            BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.On(NerveRules.Full, NerveRules.Full), 10, 300,
            new SideRules(2, 0, true), rules, fight);
        var sim = new BattleSimulation(setup);
        var session = new SimSession(sim);
        string[] cycle = { "Hold", "FocusFire", "Unleash", "Hold", "FallBack", "Hold", "Eclipse", "Hold" };
        int step = 0;
        while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
        {
            if (sim.CurrentTick % 23 == 7)
            {
                string order = cycle[step % cycle.Length];
                string target = string.Empty;
                if (order == "FocusFire")
                    for (int i = sim.Units.Count - 1; i >= 0; i--)
                        if (sim.Units[i].Alive && sim.Units[i].Side == 1) { target = sim.Units[i].Id; break; }
                // Every third Hold is given as if paused.
                int arg0 = order == "Hold" && step % 3 == 0 ? HeavyRules.PausedOrderArg : 0;
                session.TryIssue(Directives.CommandKind, "0", target, arg0, 0, order, out _);
                step++;
            }
            if (sim.AwaitingVerdict) session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, "Hold", out _);
            session.Tick();
        }
        return name + " end " + sim.EndTick + " winner " + sim.Winner + " events " + sim.Events.Count + " hash " + sim.EventHash.ToString("x16") +
            " state " + StateChecksum.Compute(sim.OrderedStateSnapshot);
    }

    private static void Fights(TextWriter o)
    {
        var habits = new TacticsProfile();
        habits.SetWaitOutParry(5); habits.SetCoverFocused(5); habits.SetHoldOnUnleash(5); habits.SetPressOnFallBack(5); habits.SetAvoidGuarded(5);
        habits.SetRally(5); habits.SetGuardCaster(5); habits.SetHoldFormation(5); habits.SetKite(3);
        for (ulong seed = 1; seed <= 3; seed++)
        {
            o.WriteLine(Fight("off" + seed, seed, Ours("off" + seed + "."), Band("o", UnitClass.Warden), SideRules.None, FightRules.Off, null));
            o.WriteLine(Fight("skill" + seed, seed, Ours("skill" + seed + "."), Band("k", UnitClass.Warden),
                new SideRules(2, 300, false, HeavyRules.EveryTicks), FightRules.Skill, habits));
            for (int b = 0; b < 15; b++)
                o.WriteLine(Fight("band" + b + "." + seed, seed, Ours("band" + b + "." + seed + "."), Band("b", UnitClass.Skirmisher),
                    new SideRules(2, 300, false, HeavyRules.EveryTicks, ChiefSpell.Eclipse, (BandTrait)(1 << b)), FightRules.Skill, null));
            for (int sp = 0; sp < ChiefSpells.Count; sp++)
                o.WriteLine(Fight("spell" + sp + "." + seed, seed, Ours("spell" + sp + "." + seed + "."), Band("p", UnitClass.Marksman),
                    new SideRules(2, 300, false, HeavyRules.EveryTicks, (ChiefSpell)sp), FightRules.Campaign, habits));
            ChiefKit[] kits = { ChiefKit.Bloodridge, ChiefKit.Skullpeak, ChiefKit.Stoneveil, ChiefKit.Thundercoast, ChiefKit.Hunt };
            ChiefSpell[] spells = { ChiefSpell.Eclipse, ChiefSpell.SteelStorm, ChiefSpell.BloodCall, ChiefSpell.Tide, ChiefSpell.Eclipse };
            for (int k = 0; k < kits.Length; k++)
                o.WriteLine(Fight("kit" + kits[k] + "." + seed, seed, Ours("kit" + k + "." + seed + "."), Band("r" + k, UnitClass.Warden),
                    new SideRules(2, ChiefKits.SpellEveryTicks(kits[k]), false, HeavyRules.EveryTicks, spells[k], flood: kits[k] == ChiefKit.Thundercoast,
                        kit: kits[k], moves: ChiefKits.Pool(kits[k])), FightRules.CampaignChiefTactics, habits));
            o.WriteLine(Fight("shark." + seed, seed, Ours("shark." + seed + "."), Beast("s.", BeastKit.SandShark),
                new SideRules(0, 0, false, beast: BeastKit.SandShark), FightRules.CampaignBeast, null));
            o.WriteLine(Fight("bear." + seed, seed, Ours("bear." + seed + "."), Beast("c.", BeastKit.CageBear),
                new SideRules(0, 0, false, beast: BeastKit.CageBear), FightRules.CampaignBeast, null));

            // The class pieces (the aim clock, the chains) under the 20 Oct rules and under the whole rules 14 stack.
            for (int which = 0; which < 2; which++)
            {
                o.WriteLine(Fight("bent" + which + "." + seed, seed, OursBent("bent" + which + "." + seed + ".", which), Band("e", UnitClass.Warden),
                    new SideRules(2, 300, false, HeavyRules.EveryTicks), FightRules.Campaign, habits));
                o.WriteLine(Fight("road14bent" + which + "." + seed, seed, OursBent("rb" + which + "." + seed + ".", which), Band("f", UnitClass.Warden),
                    new SideRules(2, 300, false, HeavyRules.EveryTicks), Road14(FightRules.Campaign), habits));
                o.WriteLine(Fight("road14kit" + which + "." + seed, seed, OursBent("rk" + which + "." + seed + ".", which), Band("g", UnitClass.Warden),
                    new SideRules(2, ChiefKits.SpellEveryTicks(ChiefKit.Bloodridge), false, HeavyRules.EveryTicks, ChiefSpell.Eclipse,
                        kit: ChiefKit.Bloodridge, moves: ChiefKits.Pool(ChiefKit.Bloodridge)), Road14(FightRules.CampaignChiefTactics), habits));
            }
            o.WriteLine(Fight("road14plain." + seed, seed, Ours("rp." + seed + "."), Band("h", UnitClass.Warden),
                new SideRules(2, 300, false, HeavyRules.EveryTicks), Road14(FightRules.Campaign), habits));
        }
    }

    // FNV-1a over a save with its first line (the header and version) cut off.
    private static string SaveHash(string save)
    {
        int cut = save.IndexOf('\n');
        string body = cut < 0 ? save : save.Substring(cut + 1);
        ulong h = 14695981039346656037UL;
        for (int i = 0; i < body.Length; i++)
        {
            h ^= body[i];
            h *= 1099511628211UL;
        }
        return h.ToString("x16");
    }

    // Whole runs under a rules cap: every fight's outcome and hashes, the pick taken after it, and the run's end.
    private static void Runs(TextWriter o, int cap)
    {
        for (ulong seed = 5100; seed < 5112; seed++)
        {
            var tuning = new CampaignTuning { EnemyScaleTier1 = 40, RulesCap = cap };
            Campaign c = Campaign.New(seed, tuning);
            if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) { o.WriteLine(seed + " no tutorial"); continue; }
            c.StartRun();
            var picker = new CampaignPilot(PilotStyle.Reference, seed);
            for (int n = 0; c.Stage == CampaignStage.InRun && n < 80; n++)
            {
                CampaignFight fight = c.StartEncounter(picker.PickOffer(c));
                EncounterResult r = CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Reference, seed * 31UL + (ulong)n));
                string pick = "-";
                if (c.FavourOffer != null)
                {
                    pick = "favour " + string.Join(",", c.FavourOffer.Choices.Select(f => ((int)f).ToString()));
                    c.TryTakeFavour(0, out _);
                }
                else if (c.SpoilsChoices.Count > 0 && c.TryTakeSpoils(0, out _))
                {
                    CampaignGear piece = c.Gear[c.Gear.Count - 1];
                    CampaignFighter wearer = c.Reserve.FirstOrDefault(f => piece.Fits(f.Class));
                    pick = "piece " + piece.Id + " bend " + (int)piece.Bend + (wearer != null && c.TryEquip(wearer.Id, piece.Id, out _) ? " on " + wearer.Id : string.Empty);
                }
                o.WriteLine(seed + "." + n + " " + fight.Setup.FightId + " won " + r.Won + " rules " + fight.RulesVersion + " end " + fight.Sim.EndTick +
                    " hash " + fight.Sim.EventHash.ToString("x16") + " state " + StateChecksum.Compute(fight.Sim.OrderedStateSnapshot) + " " + pick +
                    " pulls " + fight.Sim.Events.Count(e => (int)e.Kind == 44));
            }
            o.WriteLine(seed + " end stage " + c.Stage + " materials " + c.Materials + " gear " + c.Gear.Count + " favours " + c.Favours.Count +
                " save " + SaveHash(CampaignSave.Write(c)));
        }
    }

    // Plays into a chief fight of each kit's tribe, and into the first Pal road fight (a Butcher one when the first row has
    // it), saves each mid fight, and prints each save and its fight's state.
    private static void SaveWrite(string dir, TextWriter o)
    {
        string[] tribes = { CampaignTribes.BloodridgeId, CampaignTribes.SkullpeakId, CampaignTribes.StoneveilId, CampaignTribes.ThundercoastId };
        for (int t = 0; t < tribes.Length; t++)
            for (ulong seed = 4600; seed < 4603; seed++)
            {
                var c = Campaign.New(seed, new CampaignTuning { PathFights = 0 });
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) continue;
                c.ChooseTribe(tribes[t]);
                CampaignFight fight = c.StartEncounter(0);
                Play(fight, 400);
                Save(dir, "chief." + tribes[t] + "." + seed, c, fight, o);
            }
        for (ulong seed = 5100; seed < 5120; seed++)
        {
            var c = Campaign.New(seed, new CampaignTuning { EnemyScaleTier1 = 40 });
            if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) continue;
            c.StartRun();
            int index = c.Offers.ToList().FindIndex(e => e.Kind != EncounterKind.Ambush && e.Node != null && e.Node.Pal == Pal.Butcher);
            if (index < 0) index = c.Offers.ToList().FindIndex(e => e.Kind != EncounterKind.Ambush && e.Node != null && e.Node.Pal != Pal.None);
            if (index < 0) continue;
            string pal = c.Offers[index].Node.Pal.ToString();
            CampaignFight fight = c.StartEncounter(index);
            Play(fight, 300);
            Save(dir, "road." + pal + "." + seed, c, fight, o);
        }
    }

    private static void Play(CampaignFight fight, int ticks)
    {
        for (int i = 0; i < ticks && !fight.Sim.IsFinished && !fight.Sim.AwaitingVerdict; i++)
        {
            if (i % 37 == 11) fight.TryOrder(i % 2 == 0 ? Directive.Hold : Directive.Unleash, string.Empty, i % 3 != 0, out _);
            fight.Session.Tick();
        }
    }

    private static void Save(string dir, string name, Campaign c, CampaignFight fight, TextWriter o)
    {
        string file = Path.Combine(dir, name + ".save");
        File.WriteAllText(file, CampaignSave.Write(c));
        o.WriteLine(Path.GetFileName(file) + " tick " + fight.Sim.CurrentTick + " state " + StateChecksum.Compute(fight.Sim.OrderedStateSnapshot));
    }

    // Reads every save SaveWrite left and prints its resumed fight's state, as SaveWrite printed it.
    private static void SaveRead(string dir, TextWriter o)
    {
        string[] files = Directory.GetFiles(dir, "*.save");
        Array.Sort(files, StringComparer.Ordinal);
        foreach (string file in files)
        {
            // The same tuning SaveWrite played it under.
            CampaignTuning tuning = Path.GetFileName(file).StartsWith("chief.") ? new CampaignTuning { PathFights = 0 }
                : new CampaignTuning { EnemyScaleTier1 = 40 };
            if (!CampaignSave.TryRead(File.ReadAllText(file), tuning, out Campaign c, out string failure))
            {
                o.WriteLine(Path.GetFileName(file) + " FAILED " + failure);
                continue;
            }
            CampaignFight fight = c.Active;
            o.WriteLine(Path.GetFileName(file) + (fight == null ? " no fight, note " + c.LoadNote
                : " tick " + fight.Sim.CurrentTick + " state " + StateChecksum.Compute(fight.Sim.OrderedStateSnapshot) +
                  (c.LoadNote.Length > 0 ? " note " + c.LoadNote : string.Empty)));
        }
    }


    // ------------------------------------------------------------ the status list's extra modes (30 Oct 2026)

    private static readonly ClassBend[] Combos =
        Enumerable.Range((int)ClassBend.BlackHoleRound, 15).Select(i => (ClassBend)i).ToArray();

    private static readonly UnitClass[] DuelClasses =
        { UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Trapper, UnitClass.Medic };

    private static ClassBend ComboOf(UnitClass c, int k) => Combos.Where(b => ClassSpells.ClassOf(b) == c).ElementAt(k);

    // Ours, every fighter wearing the k-th Butcher combo of its class (k 0 to 2): all fifteen across the three.
    private static List<UnitSpec> OursCombo(string p, int k)
    {
        List<UnitSpec> list = Ours(p);
        for (int i = 0; i < list.Count; i++)
            if (list[i].Role == UnitRole.Fighter) list[i] = list[i].WithBend(ComboOf(list[i].Class, k));
        return list;
    }

    // A band whose fighters' blows burn.
    private static List<UnitSpec> Burners(string p, UnitClass chief)
    {
        List<UnitSpec> list = Band(p, chief);
        for (int i = 0; i < list.Count; i++)
            if (list[i].Role == UnitRole.Fighter) list[i] = list[i].WithRoadKit(BurnRules.MaxPercent, ChiefMove.None);
        return list;
    }

    // One of ours against a champion who swings a heavy every 2.5 s, as DuelTests plays it, under the champion duel or as
    // the road fight before it. Ours holds before every other beat coming at him, and runs the probe's order cycle.
    private static string Duel(string name, ulong seed, UnitSpec ours, bool duel)
    {
        var rules = new FightRules(true, true, true, true, true, roadCombat: true);
        var setup = new BattleSetup(name, seed, new List<UnitSpec> { ours },
            new List<UnitSpec> { new UnitSpec("x", UnitClass.Warden, Row.Front, UnitRole.Fighter, 100, 1000) },
            new TacticsProfile(), new TacticsProfile(), BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints,
            NerveRules.On(NerveRules.Full, NerveRules.Full), 10, 400, null, new SideRules(0, 0, false, HeavyRules.EveryTicks),
            duel ? rules.WithChampionDuel() : rules);
        var sim = new BattleSimulation(setup);
        var session = new SimSession(sim);
        int beats = 0;
        while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
        {
            if (sim.HoldTicksLeft(0) <= 0 && sim.Threats(0).Any(t => t.BeatTicks == 1) && beats++ % 2 == 0)
                session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, "Hold", out _);
            else if (sim.CurrentTick % 41 == 13)
                session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, sim.CurrentTick % 3 == 0 ? "Unleash" : "FallBack", out _);
            if (sim.AwaitingVerdict) session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, "Hold", out _);
            session.Tick();
        }
        return name + " end " + sim.EndTick + " winner " + sim.Winner + " events " + sim.Events.Count + " hash " + sim.EventHash.ToString("x16") +
            " state " + StateChecksum.Compute(sim.OrderedStateSnapshot);
    }

    // The Butcher's combos, burn, the chains, the chief kits and duels in direct fights.
    private static void ComboFights(TextWriter o)
    {
        var habits = new TacticsProfile();
        habits.SetWaitOutParry(5); habits.SetCoverFocused(5); habits.SetHoldOnUnleash(5); habits.SetPressOnFallBack(5); habits.SetAvoidGuarded(5);
        habits.SetRally(5); habits.SetGuardCaster(5); habits.SetHoldFormation(5); habits.SetKite(3);
        ChiefKit[] kits = { ChiefKit.Bloodridge, ChiefKit.Skullpeak, ChiefKit.Stoneveil, ChiefKit.Thundercoast, ChiefKit.Hunt };
        ChiefSpell[] spells = { ChiefSpell.Eclipse, ChiefSpell.SteelStorm, ChiefSpell.BloodCall, ChiefSpell.Tide, ChiefSpell.Eclipse };
        for (ulong seed = 1; seed <= 4; seed++)
            for (int k = 0; k < 3; k++)
            {
                string s = k + "." + seed;
                o.WriteLine(Fight("combo" + s, seed, OursCombo("c" + s + ".", k), Band("e", UnitClass.Warden),
                    new SideRules(2, 300, false, HeavyRules.EveryTicks), FightRules.Campaign, habits));
                o.WriteLine(Fight("roadcombo" + s, seed, OursCombo("rc" + s + ".", k), Band("f", UnitClass.Skirmisher),
                    new SideRules(2, 300, false, HeavyRules.EveryTicks), Road14(FightRules.Campaign), habits));
                o.WriteLine(Fight("burncombo" + s, seed, OursCombo("bc" + s + ".", k), Burners("g", UnitClass.Warden),
                    new SideRules(2, 300, false, HeavyRules.EveryTicks), Road14(FightRules.Campaign), habits));
                for (int q = 0; q < kits.Length; q++)
                    o.WriteLine(Fight("kitcombo" + kits[q] + "." + s, seed, OursCombo("kc" + q + "." + s + ".", k), Burners("r" + q, UnitClass.Warden),
                        new SideRules(2, ChiefKits.SpellEveryTicks(kits[q]), false, HeavyRules.EveryTicks, spells[q], flood: kits[q] == ChiefKit.Thundercoast,
                            kit: kits[q], moves: ChiefKits.Pool(kits[q])), Road14(FightRules.CampaignChiefTactics), habits));
                o.WriteLine(Fight("beastcombo" + s, seed, OursCombo("sc" + s + ".", k), Beast("s.", BeastKit.SandShark),
                    new SideRules(0, 0, false, beast: BeastKit.SandShark), FightRules.CampaignBeast, null));
                foreach (UnitClass c in DuelClasses)
                    foreach (bool duel in new[] { true, false })
                        o.WriteLine(Duel("duel" + c + "." + duel + "." + s, seed,
                            new UnitSpec("d", c, SliceClasses.Stats(c).DefaultRow, UnitRole.Fighter, 100, 1000).WithBend(ComboOf(c, k)), duel));
            }
    }

    // The run's start with these combos in its gear, each worn by the first fighter of its class (RunSweep.Equip).
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

    // A whole run from its start with these combos worn, every fight on its own line. With a duelist class, every duel
    // sends that class's first fighter, and the pilots parry half the champion's beats.
    private static void ComboRun(TextWriter o, string save, CampaignTuning t, ulong seed, string build, IList<ClassBend> combos, UnitClass? duelist)
    {
        Campaign c = Equip(save, t, combos);
        if (c == null) { o.WriteLine(seed + " " + build + " not worn"); return; }
        var picker = new CampaignPilot(PilotStyle.Reference, seed);
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
            EncounterResult r = CampaignPilot.PlayFight(c, f, new CampaignPilot(PilotStyle.Reference, seed * 31UL + (ulong)n)
                { DuelParryPermille = duelist != null ? 500 : 0 });
            o.WriteLine(seed + " " + build + " " + n + " " + f.Setup.FightId + " won " + r.Won + " rules " + f.RulesVersion + " end " + f.Sim.EndTick +
                " hash " + f.Sim.EventHash.ToString("x16") + " state " + StateChecksum.Compute(f.Sim.OrderedStateSnapshot));
            if (r.RunOver) break;
        }
        o.WriteLine(seed + " " + build + " end stage " + c.Stage + " save " + SaveHash(CampaignSave.Write(c)));
    }

    // Whole runs at the current rules: the plain squad and each combo alone, then the duels with each class sent and each
    // combo's wearer sent.
    private static void ComboRuns(TextWriter o, ulong seed0, int count, int cap = 0)
    {
        var t = new CampaignTuning { EnemyScaleTier1 = 110, EnemyScaleTier2 = 110 * 86 / 95 };
        if (cap > 0) t.RulesCap = cap;
        for (ulong seed = seed0; seed < seed0 + (ulong)count; seed++)
        {
            Campaign start = Campaign.New(seed, t);
            if (!CampaignPilot.PlayTutorial(start, PilotStyle.Reference, seed, 10)) { o.WriteLine(seed + " no tutorial"); continue; }
            start.StartRun();
            string save = CampaignSave.Write(start);
            ComboRun(o, save, t, seed, "plain", new ClassBend[0], null);
            foreach (ClassBend b in Combos) ComboRun(o, save, t, seed, b.ToString(), new[] { b }, null);
            foreach (UnitClass cls in DuelClasses) ComboRun(o, save, t, seed, "plain." + cls, new ClassBend[0], cls);
            foreach (ClassBend b in Combos) ComboRun(o, save, t, seed, b + ".sent", new[] { b }, ClassSpells.ClassOf(b));
            o.Flush();
        }
    }


    private static void MixFights(TextWriter o)
    {
        var habits = new TacticsProfile();
        habits.SetWaitOutParry(5); habits.SetCoverFocused(5); habits.SetHoldOnUnleash(5); habits.SetPressOnFallBack(5); habits.SetAvoidGuarded(5);
        habits.SetRally(5); habits.SetGuardCaster(5); habits.SetHoldFormation(5); habits.SetKite(3);
        for (ulong seed = 1; seed <= 4; seed++)
            for (int a = 0; a < 3; a++)
                for (int b = 0; b < 3; b++)
                {
                    string s = a + "" + b + "." + seed;
                    List<UnitSpec> ours = OursCombo("x" + s + ".", (int)(seed % 3));
                    for (int i = 0; i < ours.Count; i++)
                    {
                        if (ours[i].Role != UnitRole.Fighter) continue;
                        if (ours[i].Class == UnitClass.Trapper) ours[i] = ours[i].WithBend(ComboOf(UnitClass.Trapper, a));
                        if (ours[i].Class == UnitClass.Warden) ours[i] = ours[i].WithBend(ComboOf(UnitClass.Warden, b));
                    }
                    o.WriteLine(Fight("mix" + s, seed, ours, Band("e", UnitClass.Warden),
                        new SideRules(2, 300, false, HeavyRules.EveryTicks), FightRules.Campaign, habits));
                    o.WriteLine(Fight("roadmix" + s, seed, ours, Band("f", UnitClass.Skirmisher),
                        new SideRules(2, 300, false, HeavyRules.EveryTicks), Road14(FightRules.Campaign), habits));
                    o.WriteLine(Fight("ritemix" + s, seed, ours, Burners("v", UnitClass.Medic),
                        new SideRules(2, ChiefKits.SpellEveryTicks(ChiefKit.Stoneveil), false, HeavyRules.EveryTicks, ChiefSpell.BloodCall,
                            kit: ChiefKit.Stoneveil, moves: ChiefKits.Pool(ChiefKit.Stoneveil)), Road14(FightRules.CampaignChiefTactics), habits));
                }
    }

    public static int Main(string[] args)
    {
        TextWriter o = Console.Out;
        if (args[0] == "fights") Fights(o);
        else if (args[0] == "runs") Runs(o, int.Parse(args[1]));
        else if (args[0] == "savewrite") SaveWrite(args[1], o);
        else if (args[0] == "combofights") ComboFights(o);
        else if (args[0] == "mixfights") MixFights(o);
        else if (args[0] == "comboruns") ComboRuns(o, ulong.Parse(args[1]), int.Parse(args[2]), args.Length > 3 ? int.Parse(args[3]) : 0);
        else SaveRead(args[1], o);
        return 0;
    }
}
