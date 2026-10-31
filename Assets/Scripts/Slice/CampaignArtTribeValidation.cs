using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        int _tribeRouteCount;
        IEnumerator ValidateTribes()
        {
            string dir = Argument("-ksTribesValidate");
            string source = File.ReadAllText(Path.Combine(dir, "skullpeak-duel.txt")).Replace("\r", "");
            _savePath = Path.Combine(_capture, "tribes-save.txt");
            foreach (string tribe in new[] { "stoneveil", "thundercoast" })
            {
                // Inspect every delivered clip, including roles a particular fixed squad may not contain.
                foreach (string role in new[] { "warden", "marksman", "skirmisher", "trapper", "medic", "chief", "champion" }.Concat(tribe == "stoneveil" ? new[] { "shaman" } : new string[0]))
                foreach (string facing in role == "chief" || role == "champion" || role == "shaman" ? new[] { "sw" } : new[] { "sw", "se" })
                foreach (string action in new[] { "idle", "attack", "hit", "death" })
                {
                    string key = tribe + "_" + role + "_" + facing + "_" + action;
                    var clip = LoadClip(key);
                    Check(clip.sheet.frames.Length > 0 && clip.seconds > 0 && clip.texture != null, key + " loads with duration");
                }
                string data = source.Replace("run\tskullpeak\t0\t-1", "run\t" + tribe + "\t3\t-1");
                _playback = null; _fight = null; _result = null; _notice = "";
                _campaign = CampaignSave.Read(data);
                SetPage(Page.Offers); yield return null;
                Check(Click("offer-0"), tribe + " chief selected"); yield return null;
                Capture(tribe + "-briefing");
                Check(Click("start-fight"), tribe + " chief starts through UI");
                _pilotOn = false; _paused = true; _speed = 1; yield return null;
                Check(_figures.Where(f => f.unit.Side == 1).All(f => f.body != null && f.artKey.StartsWith(tribe + "_")), tribe + " all enemy figures use delivered SW sheets");
                Save(); File.Copy(_savePath, Path.Combine(_capture, tribe + "-review.txt"), true);
                bool bleedSeen = false, floodSeen = false;
                while (!ViewOver && ViewSim.WindupTicks(1) == 0)
                {
                    HeavyTestTick();
                    if (!bleedSeen && ViewSim.Units.Any(u => u.Alive && u.BleedTicks > 0))
                    {
                        DrawBattle(); RefreshFight(); CheckTribeReadouts(); Capture(tribe + "-bleeding"); bleedSeen = true;
                        CheckTribeReload(tribe + " bleeding");
                    }
                    if (!floodSeen && ViewSim.FloodLevel > 0)
                    {
                        DrawBattle(); RefreshFight(); CheckTribeReadouts(); Capture(tribe + "-flood"); floodSeen = true;
                        CheckTribeReload(tribe + " flood");
                    }
                }
                Check(!ViewOver && ViewSim.WindupTicks(1) > 0, tribe + " real enemy spell winds up");
                DrawBattle(); RefreshFight();
                var sim = ViewSim;
                Check(sim.SpellAnswer(1) == Directive.FallBack, tribe + " counter is authoritative Fall Back");
                Check(_chiefPanels[1] != null && _chiefPanels[1].GetComponentsInChildren<RawImage>(true).Any(i => i.name == "Chief portrait" && i.texture != null), tribe + " approved portrait bound to real cast");
                Check(_chiefPanels[1].GetComponentsInChildren<Graphic>(true).All(g => !g.raycastTarget), tribe + " cut-in cannot intercept controls");
                Capture(tribe + "-cut-in");
                // The tell is longer than an ordinary Fall Back; give the order near release.
                while (!ViewOver && sim.WindupTicks(1) > SimClock.TicksPerSecond) HeavyTestTick();
                Check(Click("order-1"), tribe + " Fall Back clicks through cut-in");
                DrainEvents(); DrawBattle(); RefreshFight();
                while (!ViewOver && sim.WindupTicks(1) > 0) HeavyTestTick();
                Check(sim.Events.Any(e => e.Kind == BattleEventKind.Release && e.ActorSide == 1 && e.Detail == Moves.Dodged), tribe + " Fall Back actually dodges spell");
                while (!ViewOver)
                {
                    HeavyTestTick();
                    if (!floodSeen && sim.FloodLevel > 0) { DrawBattle(); CheckTribeReadouts(); Capture(tribe + "-flood"); floodSeen = true; CheckTribeReload(tribe + " flood"); sim = ViewSim; }
                }
                Check(tribe == "stoneveil" ? bleedSeen : floodSeen, tribe + " mechanic occurred in actual fight");
                DrawBattle(); Check(_bleedMarks.Values.All(t => t.key.StartsWith("|")) && _floodReadouts.All(t => t.key.StartsWith("|")), tribe + " finished fight clears status readouts");
                string id = _fight.Setup.FightId;
                ShowResult(); yield return null; Capture(tribe + "-result");
                CheckTribeResult(tribe);
                string saved = CampaignSave.Write(_campaign);
                WatchFight(_campaign.Archive.Find(id)); SeekPlayback(_playback.Length); yield return null;
                Check(_playback.AtEnd && _playback.Matched, tribe + " new replay checksum matches");
                Check(CampaignSave.Write(_campaign) == saved, tribe + " playback leaves campaign unchanged");

                // A saved joined band is a UI fixture, not a claim of winning the preceding fight.
                _playback = null; _fight = null; _notice = "";
                string joined = source.Replace("\tclarth\t", "\t" + tribe + "\t")
                    .Replace("run\t", "assist\t" + tribe + "\t2\nrun\t");
                _campaign = CampaignSave.Read(joined); SetPage(Page.Offers); yield return null;
                int pick = _campaign.Offers.ToList().FindIndex(e => e.SquadSize == 5);
                Check(pick >= 0 && Click("offer-" + pick), tribe + " joined band selected"); yield return null;
                Capture(tribe + "-joined-squad");
                Check(Click("start-fight"), tribe + " joined band starts"); _paused = true; _pilotOn = false;
                Check(_figures.Where(f => f.unit.Side == 0 && !f.unit.IsLeader).All(f => f.artKey.StartsWith(tribe + "_") && f.artKey.EndsWith("_se") && f.body != null), tribe + " joined band uses SE sheets");
                CheckTribeReload(tribe + " joined band"); Capture(tribe + "-joined-fight");
                CheckFloodSides();
                Check(Click("order-4"), tribe + " saved captured order is usable"); DrainEvents(); DrawBattle(); RefreshFight();
                Check(_fight.AssistUsed && ViewSim.Spell(0) == (tribe == "stoneveil" ? ChiefSpell.BloodCall : ChiefSpell.Tide), tribe + " fifth order calls correct spell once");
                Capture(tribe + "-own-cut-in");
                _paused = false;
                for (int frame = 0; frame < 90 && !ViewOver; frame++)
                { yield return null; if (frame % 6 == 0) Capture(tribe + "-motion-" + (frame / 6).ToString("D2")); }
                _paused = true; CheckTribeReload(tribe + " after captured cast");
                if (tribe == "stoneveil")
                {
                    _fight = null; _playback = null; _notice = "";
                    string road = source.Replace("run\tskullpeak\t0\t-1", "run\tstoneveil\t0\t-1");
                    _campaign = CampaignSave.Read(road);
                    var trial = _campaign.Road.All.First(n => n.Kind == RoadNodeKind.Fight && n.Fight == EncounterKind.Trial);
                    _campaign = CampaignSave.Read(road.Replace("run\tstoneveil\t0\t-1", "run\tstoneveil\t" + trial.Row + "\t-1"));
                    int trialPick = _campaign.Offers.ToList().FindIndex(e => e.Kind == EncounterKind.Trial);
                    SetPage(Page.Offers); yield return null;
                    Check(trialPick >= 0 && Click("offer-" + trialPick), "Stoneveil blood trial selected"); yield return null;
                    Check(Click("start-fight"), "Stoneveil blood trial started"); _paused = true; _pilotOn = false; _speed = 1;
                    var priest = _figures.Single(f => f.unit.Id.EndsWith(".shaman"));
                    Check(priest.artKey == "stoneveil_shaman_sw" && priest.body != null, "actual trial caster uses approved priest instead of Medic");
                    Capture("stoneveil-priest-trial"); CheckTribeReload("Stoneveil priest trial");
                }
            }
            var routes = ValidateTribeRoutes(source); while (routes.MoveNext()) yield return routes.Current;
            var recruits = ValidateTribeRecruitment(dir); while (recruits.MoveNext()) yield return recruits.Current;
            Check(_resultScrolls == 0, "new tribe result screens fit without scrolling");
            // Retain the full four-combination acceptance gate.
            Check(_tribeRouteCount == 4, "all four early route combinations are reachable (sim-owned Gate 6)");
        }

        void CheckFloodSides()
        {
            // Long-lived isolated status fixtures exercise the cap and mutual immunity without tuning the campaign.
            for (int mode = 0; mode < 3; mode++)
            {
                var ours = new[] { new UnitSpec("flood.ours", UnitClass.Warden, Row.Front, UnitRole.Fighter, 1, 10000) };
                var theirs = new[] { new UnitSpec("flood.theirs", UnitClass.Warden, Row.Front, UnitRole.Fighter, 1, 10000) };
                var sim = new BattleSimulation(new BattleSetup("flood-status", 1, ours, theirs, null, null, 1800, 5,
                    NerveRules.Off, side0Rules: new SideRules(0, 0, true, flood: mode != 1),
                    side1Rules: new SideRules(0, 0, true, flood: mode != 0)));
                for (int level = 0; level <= FloodRules.MaxLevel; level++)
                {
                    while (sim.CurrentTick < level * FloodRules.StepTicks) sim.Tick();
                    string state = sim.OrderedStateSnapshot; DrawTribeMechanics(sim);
                    for (int side = 0; side < 2; side++)
                    {
                        string expected = mode == 2 ? "BOTH SIDES IMMUNE" : level == 0 ? "WATER RISING" : sim.Floods(side) ? "STRONGER BLOWS" : "SLOWED + NERVE DRAIN";
                        Check(_floodReadouts[side].key.Contains("FLOOD " + level + "/" + FloodRules.MaxLevel) && _floodReadouts[side].key.Contains(expected), "flood mode " + mode + " level " + level + " side " + side + " displayed correctly");
                    }
                    Check(sim.OrderedStateSnapshot == state, "flood rendering is read-only");
                }
            }
            DrawBattle();
        }

        IEnumerator ValidateTribeRecruitment(string dir)
        {
            foreach (string tribe in new[] { "stoneveil", "thundercoast" })
            {
            // Fixed max-favour fixtures: Stoneveil's authored heavy/parry counter sequence and the
            // archived chief schedule for Thundercoast. No tuning changes or outcome/seed search.
            string data = File.ReadAllText(Path.Combine(Argument("-ksTribeFixtures"), tribe + "-recruit-fixture.txt"));
            var recorded = CampaignSave.Read(File.ReadAllText(Path.Combine(dir, "fixture-clue.txt"))).Archive.Find("camp.5.raiders.chief");
            _playback = null; _fight = null; _result = null; _notice = "";
            _campaign = CampaignSave.Read(data); SetPage(Page.Offers); yield return null;
            Check(Click("offer-0"), tribe + " earned recruitment selects chief"); yield return null;
            Check(Click("start-fight"), tribe + " earned recruitment starts"); _paused = true; _pilotOn = false;
            int cursor = 0;
            int[] stoneTicks = { 100, 105, 175, 180, 250, 255, 325, 330, 400, 405 };
            while (!ViewOver)
            {
                if (tribe == "stoneveil")
                {
                    while (cursor < stoneTicks.Length && stoneTicks[cursor] <= ViewSim.CurrentTick)
                    {
                        var command = cursor++ % 2 == 0 ? Directive.Hold : Directive.Unleash;
                        Check(_fight.TryOrder(command, null, out _), "Stoneveil authored live counter " + command);
                    }
                }
                else while (cursor < recorded.Log.Commands.Count && recorded.Log.Commands[cursor].Tick <= ViewSim.CurrentTick)
                {
                    var order = recorded.Log.Commands[cursor++];
                    if (Directives.TryParse(order.TextArg, out Directive command)) _fight.TryOrder(command, order.TargetId.Replace("raiders", tribe), out _);
                }
                HeavyTestTick();
            }
            ShowResult(); yield return null;
            Check(_result.ChiefCaptured && _result.JoinedFighters.Count > 0 && _result.AssistUnlocked != null, tribe + " real chief capture awards band and order");
            CheckTribeResult(tribe + " earned recruitment"); Capture(tribe + "-earned-recruits");
            Check(Click("after-result"), tribe + " earned recruits continue"); yield return null;
            Check(Click("offer-0"), "earned recruits open next squad"); yield return null;
            foreach (var f in _campaign.Reserve.Where(f => f.TribeId == tribe))
                Check(_buttons.ContainsKey("fighter-" + f.Id), "earned recruit selectable " + f.Id);
            Capture(tribe + "-earned-next-squad");
            }
        }

        void CheckTribeReadouts()
        {
            var sim = ViewSim;
            string before = sim.OrderedStateSnapshot;
            for (int i = 0; i < 3; i++)
            {
                DrawBattle();
                if (_playback != null) RefreshPlayback(); else RefreshFight();
            }
            Check(sim.OrderedStateSnapshot == before, "paused repeated rendering changes no status or sim state");
            foreach (var f in _figures)
            {
                bool bleeding = f.unit.Alive && f.unit.BleedTicks > 0 && !ViewOver && !ChiefCutInVisible;
                if (sim.ClassSpellsOn)
                {
                    Check(_classReadouts[f.unit.Id].marks[1].key.StartsWith(bleeding ? "BLEED|" : "|", StringComparison.Ordinal),
                        "class bleed marker follows actual unit " + f.unit.Id);
                    Check(_bleedMarks[f.unit.Id].key.StartsWith("|", StringComparison.Ordinal), "class bleed has no duplicate legacy label " + f.unit.Id);
                    if (bleeding && !f.unit.Yielded)
                        Check(ClassFighterHint(f.unit).Contains("Bleed " + Seconds(f.unit.BleedTicks)), "class wound hint timer matches sim " + f.unit.Id);
                }
                else
                {
                    Check(_bleedMarks[f.unit.Id].key.StartsWith(bleeding ? "BLEED " : "|", StringComparison.Ordinal),
                        "legacy bleed marker follows actual unit " + f.unit.Id);
                    if (bleeding) Check(_bleedMarks[f.unit.Id].key.Contains(Seconds(f.unit.BleedTicks)), "legacy wound timer matches sim " + f.unit.Id);
                }
            }
            for (int side = 0; side < 2; side++)
            {
                string text = !ViewOver && !ChiefCutInVisible ? FloodReadout(sim, side) : "";
                Check(_floodReadouts[side].key.StartsWith(text + "|", StringComparison.Ordinal), "flood readout agrees with side " + side);
                Check(_tinyFont.Measure(text, 1) <= 292, "flood readout fits side " + side);
                if (!ViewOver && !ChiefCutInVisible && sim.FloodLevel > 0 && sim.Floods(1) && !sim.Floods(0))
                    Check(text.Contains(side == 0 ? "SLOWED + NERVE DRAIN" : "STRONGER BLOWS"), "enemy flood describes actual affected side " + side);
            }
        }

        void CheckTribeReload(string tag)
        {
            string state = ViewSim.OrderedStateSnapshot;
            Save(); _campaign = CampaignSave.Read(File.ReadAllText(_savePath)); ResumeFight(); _paused = true; _pilotOn = false; _speed = 1;
            Check(ViewSim.OrderedStateSnapshot == state, tag + " active save restores exact state");
            DrawBattle(); RefreshFight(); CheckTribeReadouts();
        }

        void CheckTribeResult(string tag)
        {
            var r = _result;
            string text = string.Join(" ", ResultFighterLines(r, false).texts);
            Check(text.Contains("Died: " + (r.Fallen.Count + (r.Slain == null ? 0 : 1))), tag + " death total includes post-defeat killing");
            foreach (var f in r.Fallen) Check(text.Contains(FighterName(f)), tag + " names fallen " + f.Id);
            if (r.Slain != null)
            {
                Check(r.Captured == null && text.Contains("Killed after defeat: " + FighterName(r.Slain)) && text.Contains("Captured: none"), tag + " names slain without inventing prisoner");
                Check(!_campaign.Reserve.Any(f => f.Id == r.Slain.Id) && !_campaign.Grudges.Contains(r.Encounter.Tribe.Id), tag + " slain absent and no capture grudge");
            }
            if (r.Captured != null) Check(text.Contains(FighterName(r.Captured)) && r.Slain == null, tag + " names prisoner separately");
            foreach (var f in r.JoinedFighters) Check(text.Contains(FighterName(f)) && _campaign.Reserve.Any(x => x.Id == f.Id), tag + " actual recruit named " + f.Id);
            Save();
            Check(CampaignSave.Write(CampaignSave.Read(File.ReadAllText(_savePath))) == CampaignSave.Write(_campaign), tag + " result roster materials orders and route survive reload");
        }

        IEnumerator ValidateTribeRoutes(string source)
        {
            // Route-only fixture catalogue: no fights, bot runs or balance/seed optimisation here.
            var seeds = new Dictionary<string, string>();
            string camp = string.Join("\n", source.Split('\n').Where(line => !line.StartsWith("run\t") && !line.StartsWith("road\t") && !line.StartsWith("offer\t")));
            for (ulong seed = 1; seed <= 32; seed++)
            {
                var c = CampaignSave.Read(camp.Replace("seed\t1\n", "seed\t" + seed + "\n"));
                string preview = string.Join("/", c.RunRoute.Select(t => t.Id));
                c.StartRun();
                string key = string.Join("/", c.RunRoute.Select(t => t.Id));
                Check(preview == key && c.Target.Id == c.RunRoute[0].Id, "new run keeps preview for seed " + seed);
                if (!seeds.ContainsKey(key)) seeds.Add(key, CampaignSave.Write(c));
            }
            _tribeRouteCount = seeds.Count;
            Log("AUDIT reachable route combinations: " + seeds.Count + " / 4: " + string.Join(", ", seeds.Keys));
            foreach (var route in seeds)
            {
                string routeData = route.Value;
                string start = "run\t" + route.Key.Split('/')[0] + "\t0\t-1";
                foreach (string tribe in route.Key.Split('/'))
                {
                    _playback = null; _fight = null; _result = null; _notice = "";
                    _campaign = CampaignSave.Read(routeData.Replace(start, "run\t" + tribe + "\t3\t-1"));
                    SetPage(Page.Offers); yield return null;
                    Check(string.Join("/", Route().Select(t => t.Id)) == route.Key, route.Key + " preview keeps both selected tiers at " + tribe);
                    Check(Click("offer-0"), route.Key + " selects " + tribe); yield return null;
                    Check(Click("start-fight"), route.Key + " starts " + tribe); _paused = true; _pilotOn = false;
                    // Fixed no-order encounter. Outcome is measured, never imposed by the UI.
                    while (!ViewOver) HeavyTestTick();
                    ShowResult(); yield return null;
                    CheckTribeResult(route.Key + " " + tribe);
                    Capture("route-" + route.Key.Replace('/', '-') + "-" + tribe + "-result");
                }
            }
        }
    }
}
