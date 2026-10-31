using System;
using System.Collections;
using System.IO;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Fixed save derivatives at the chief node, no pilot/seed search/tuning. Every action uses Campaign.
        IEnumerator ValidateChiefSpells()
        {
            string source = File.ReadAllText(Path.Combine(Argument("-ksChiefValidate"), "skullpeak-duel.txt"));
            for (int scenario = 0; scenario < 3; scenario++)
            {
                string tribe = scenario == 0 ? CampaignTribes.BloodridgeId : CampaignTribes.SkullpeakId;
                string tag = scenario == 0 ? "eclipse" : scenario == 1 ? "steel-enemy" : "steel-player";
                string fixture = source.Replace("run\tskullpeak\t0\t-1", "run\t" + tribe + "\t3\t-1");
                fixture = fixture.Replace("offer\t0", "offer\t4").Replace("offer\t2\r\n", "").Replace("offer\t3\r\n", "");
                if (scenario == 2) fixture = fixture.Replace("run\t", "assist\tskullpeak\t" + CampaignTribes.Skullpeak.AssistGrade + "\nrun\t");
                _playback = null; _fight = null;
                _savePath = Path.Combine(_capture, tag + "-save.txt");
                _campaign = CampaignSave.Read(fixture);
                SetPage(Page.Offers); yield return null;
                Capture(tag + "-road");
                int offer = _campaign.Offers.ToList().FindIndex(e => e.Kind == EncounterKind.Chief);
                Check(offer >= 0 && Click("offer-" + offer), tag + " chief selected from road"); yield return null;
                Capture(tag + "-squad");
                Check(Click("start-fight"), tag + " starts through squad UI");
                _pilotOn = false; _paused = true; _speed = 1; yield return null;
                var sim = ViewSim;
                Save(); File.Copy(_savePath, Path.Combine(_capture, tag + "-start.txt"), true);
                Check(sim.Spell(1) == (scenario == 0 ? ChiefSpell.Eclipse : ChiefSpell.SteelStorm), tag + " reads enemy spell");
                Check(sim.Spell(0) == (scenario == 2 ? ChiefSpell.SteelStorm : ChiefSpell.DustShark), tag + " reads player spell");
                Check(_orderEffect[4].key.Contains(ChiefEffect(sim.Spell(0))), tag + " fifth plate describes actual effect");
                Check(Click("order-4"), tag + " fifth order accepts UI click");
                DrainEvents(); DrawBattle(); RefreshFight();
                Check(sim.WindupTicks(0) > 0 && _fight.AssistUsed, tag + " fifth order starts authoritative spell once");
                Check(!_buttons["order-4"].interactable, tag + " spent fifth order is disabled");
                Capture(tag + "-our-cut-in");
                Check(_chiefPanels[0] != null && _chiefPanels[0].gameObject.activeSelf, tag + " actual player windup shows cut-in");
                Check(_chiefPanels[0].GetComponentsInChildren<Graphic>().All(g => !g.raycastTarget), tag + " cut-in cannot intercept input");
                Check(Click("order-2"), tag + " Focus Fire remains clickable during cut-in");
                var target = sim.Units.First(u => u.Side == 1 && u.Alive && !u.IsLeader);
                Check(Click("target-" + target.Id) && sim.FocusedId(0) == target.Id, tag + " actual enemy targets through cut-in");
                _notice = "";
                bool sawPlayerWall = false, sawWallExpire = false;
                for (int tick = 0; tick < 700 && !ViewOver && sim.WindupTicks(1) == 0; tick++)
                {
                    HeavyTestTick();
                    if (scenario == 2 && !sawPlayerWall && sim.WallTicks(0) > 0)
                    {
                        DrawBattle(); RefreshFight(); Capture("steel-player-own-wall");
                        Check(_wallPanels[0].gameObject.activeSelf && !_wallPanels[1].gameObject.activeSelf, "player wall protects the correct side");
                        Check(_orderEffect[4].key.Contains("Wall "), "fifth plate shows active player wall time");
                        sawPlayerWall = true;
                    }
                    if (scenario == 2 && sawPlayerWall && !sawWallExpire && sim.WallTicks(0) == 0)
                    {
                        DrawBattle(); RefreshFight(); Capture("steel-player-wall-ended");
                        Check(!_wallPanels[0].gameObject.activeSelf && _orderEffect[4].key.Contains("Used"), "expired player wall clears and assist remains spent");
                        Check(sim.Events.Any(e=>e.Kind==BattleEventKind.Wall&&e.ActorSide==0&&e.Detail==ChiefSpells.WallFell), "player wall naturally expires through sim event");
                        sawWallExpire = true;
                    }
                }
                if (scenario == 2) Check(sawPlayerWall && sawWallExpire, "player wall rise and natural expiry exercised");
                Check(!ViewOver && sim.WindupTicks(1) > 0, tag + " actual enemy cast begins");
                DrawBattle(); RefreshFight(); Capture(tag + "-their-cut-in");
                Check(_spellTimers[1].key.Contains(ChiefSpells.Name(sim.Spell(1))), tag + " spell timer survives simultaneous heavy");
                string snapshot = sim.OrderedStateSnapshot;
                for (int frame = 0; frame < 3; frame++) { DrawBattle(); RefreshFight(); yield return null; }
                Check(sim.OrderedStateSnapshot == snapshot, tag + " drawing and pause preserve sim");
                if (scenario > 0)
                {
                    for (int tick = 0; tick < 200 && !ViewOver && sim.WallTicks(1) == 0; tick++) HeavyTestTick();
                    Check(sim.WallTicks(1) > 0, tag + " wall rises from real cast");
                    DrawBattle(); RefreshFight(); Capture(tag + "-wall");
                    Check(_wallPanels[1].gameObject.activeSelf && _wallLabels[1].key.Contains(Seconds(sim.WallTicks(1))), tag + " enemy wall and timer follow sim");
                    Check(_orderEffect[3].key.Contains("Smash their wall"), tag + " Unleash plate names wall counter");
                    Save(); snapshot = sim.OrderedStateSnapshot;
                    File.Copy(_savePath, Path.Combine(_capture, tag + "-wall-save.txt"), true);
                    _campaign = CampaignSave.Read(File.ReadAllText(_savePath)); ResumeFight(); _pilotOn = false; sim = ViewSim;
                    DrawBattle(); RefreshFight();
                    Check(sim.OrderedStateSnapshot == snapshot && _wallPanels[1].gameObject.activeSelf, tag + " reload restores wall without changing state");
                    Check(_chiefPanels.All(p => p == null || !p.gameObject.activeSelf), tag + " reload does not replay old cut-ins");
                    yield return null;
                    RefreshFight(); Capture(tag + "-reloaded-wall");
                    Check(sim.CanUse(0,Directive.Unleash,out string unavailable),tag + " Unleash available after reload: " + unavailable);
                    Check(Click("order-3"), tag + " Unleash clicked against standing wall");
                    DrainEvents(); DrawBattle(); RefreshFight(); Capture(tag + "-smash");
                    Check(sim.WallTicks(1) == 0 && !_wallPanels[1].gameObject.activeSelf && _wallBreakWord.enabled, tag + " actual Unleash clears wall with smash feedback");
                    Check(_flashText[0] != "UNLEASH BLOCKED" && _pxBanner.key.Contains("WALL SMASHED"), tag + " successful wall smash is not presented as a failed order");
                }
                if (scenario == 2)
                {
                    Check(sim.Events.Any(e => e.Kind == BattleEventKind.Wall && e.ActorSide == 0 && e.Detail == ChiefSpells.WallUp), "player Steel Storm raises its own wall");
                    Check(!Click("order-4"), "used Steel Storm cannot be called again");
                }
                // Short actual motion sequence; no synthetic events or scripted pilot.
                _paused = false;
                for (int frame = 0; frame < 75 && !ViewOver; frame++)
                { yield return null; if (frame % 3 == 0) Capture(tag + "-motion-" + (frame / 3).ToString("D3")); }
                _paused = true;
                while (!ViewOver) HeavyTestTick();
                DrawBattle(); RefreshFight();
                Check(_wallPanels.All(p => !p.gameObject.activeSelf) && _chiefPanels.All(p => p == null || !p.gameObject.activeSelf), tag + " completed fight clears walls and cut-ins");
                string id = _fight.Setup.FightId;
                ShowResult(); yield return null;
                var record = _campaign.Archive.Find(id);
                string before = CampaignSave.Write(_campaign);
                WatchFight(record); SeekPlayback(_playback.Length); yield return null;
                Check(_playback.AtEnd && _playback.Matched, tag + " newly recorded fight replays to matching checksum");
                Check(CampaignSave.Write(_campaign) == before, tag + " playback preserves campaign");
            }
        }
    }
}
