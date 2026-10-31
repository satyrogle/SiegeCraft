using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        IEnumerator ValidateRoad11Hud()
        {
            yield return null;
            _savePath=Path.Combine(_capture,"road11-save.txt");_roadReview=_gatesReview=true;
            NewReviewRoute();yield return null;Capture("01-road");
            Check(_campaign.Lives==3&&LivesText()=="Lives: 3/3","road exposes authoritative three lives");
            Check(Click("road-details"),"road details clickable");yield return null;Capture("02-road-details");
            Check(Click("modal-close"),"close road details");yield return null;
            Check(Click("offer-0"),"road node opens squad");yield return null;
            Check(Click("start-fight"),"squad starts current encounter");_pilotOn=false;_paused=true;_speed=.5f;yield return null;
            Check(RoadHud&&ViewSim.UnitNerveOn,"rules 11 personal nerve HUD active");
            Check(!_nerveFill[0].enabled&&!_nerveBack[1].enabled,"shared nerve bars absent");
            Check(_roadWarning.key.Contains("FALL BACK"),"known Fall Back failure stays visible on combat screen");
            Check(_nerveLabel[1].key.Contains(ViewSim.StillFighting(1).ToString()),"enemies left is authoritative");
            Check(_orderCost[0].key.Contains("FREE"),"Hold remains free");
            string fresh=CampaignSave.Write(_campaign),state=ViewSim.OrderedStateSnapshot;
            for(int i=0;i<3;i++){RefreshFight();DrawBattle();yield return null;}
            Check(ViewSim.OrderedStateSnapshot==state,"paused drawing cannot change combat");
            Check(_roadUnits.Count==ViewSim.Units.Count(u=>!u.Benched),"each visible fighter has personal nerve cue");
            Check(Click("fight-read"),"combat Details opens");yield return null;Capture("03-details");
            Check(Click("modal-close"),"close combat Details");yield return null;
            Capture("04-start");CheckCleanTextBounds();
            _canvas.scaleFactor=.75f;CheckCleanTextBounds();_canvas.scaleFactor=1;
            Check(Click("order-0"),"Hold UI raycast works");
            Check(Click("order-2"),"Focus targeting opens");HandleUiKey(KeyCode.Return);
            Check(ViewSim.FocusedId(0).Length>0,"keyboard confirms Focus target");
            GateRestore(fresh);yield return null;_speed=.5f;_paused=false;
            long tick=ViewSim.CurrentTick;for(int i=0;i<24;i++)yield return null;
            _paused=true;Check(ViewSim.CurrentTick>tick,"live player advances at half speed");
            GateRestore(fresh);yield return null;
            bool chain=false,defence=false,frenzy=false,fled=false,saved=false;
            var checkedFled=new HashSet<string>();
            for(int i=0;i<7200&&!ViewOver;i++)
            {
                HudTicks(1);
                if(!chain&&_roadPayoff.Length>0){chain=true;Capture("05-combo");}
                if(!defence&&ViewSim.Events.Any(e=>e.Kind==BattleEventKind.Defence))
                {defence=true;Capture("06-defence");}
                if(!frenzy&&ViewSim.Units.Any(u=>u.Frenzied&&u.Alive))
                {frenzy=true;Capture("07-frenzy");Check(_roadUnits.Values.Any(r=>r.ring.Any(p=>p.enabled)),"frenzy timer ring visible");}
                foreach(var u in ViewSim.Units.Where(u=>u.Fled))
                {fled=true;if(checkedFled.Add(u.Id))Check(!FindFigure(u.Id).root.gameObject.activeSelf,"fled enemy removed from field");}
                if(i==300)
                {
                    state=ViewSim.OrderedStateSnapshot;var save=CampaignSave.Write(_campaign);
                    GateRestore(save);yield return null;
                    Check(state==ViewSim.OrderedStateSnapshot,"mid-fight chains and nerve reload exactly");saved=true;
                }
                if(i%90==0)yield return null;
            }
            Check(chain&&defence&&saved&&ViewOver,"one fixed road fight exercises chains, defence, reload and finish");
            Log("Observed frenzy="+frenzy+", fled="+fled+". No balance sweep.");
            Capture("08-end");CheckCleanTextBounds();
            string final=ViewSim.OrderedStateSnapshot;
            int reserve=_campaign.Reserve.Count;
            ShowResult();yield return null;Capture("09-result");
            Check(_resultUsesLives&&_resultLives==_campaign.Lives,"results use actual lives");
            Check(_campaign.Reserve.Count==reserve,"fight-local deaths retain roster");
            var bonded=CampaignSave.Read(CampaignSave.Write(_campaign));
            Check(_campaign.Bonds.Count==bonded.Bonds.Count,"earned bonds persist after the result");
            Check(Click("why"),"result Why opens recorded road summary");yield return null;Capture("09-why");
            Check(Click("modal-close"),"result Why closes");yield return null;
            var archive=_campaign.Archive.Fights.Last();
            WatchFight(archive,0,archive.Ticks);yield return null;SeekPlayback(archive.Ticks);yield return null;
            Check(ViewSim.OrderedStateSnapshot==final,"new archived fight replays exact state");
            Check(RoadHud&&_nerveLabel[0].key.Contains("REPLAY"),"historical replay does not invent current lives");
            SeekPlayback(0);yield return null;
            Check(_roadPayoff.Length==0,"backward seek clears stale combo stamp");
            LeavePlayback();CloseModal();yield return null;

            // Explicit loss fixture, not gameplay tuning: exercise the same result/restore UI.
            _campaign=CampaignSave.Read(CampaignSave.Write(FreshRoadPreview()),new CampaignTuning{EnemyScaleTier1=300});
            reserve=_campaign.Reserve.Count;
            _fight=_campaign.StartEncounter(0);BeginFight();_pilotOn=false;_paused=true;yield return null;
            for(int i=0;i<7200&&!ViewOver;i++){HudTicks(1);if(i%120==0)yield return null;}
            Check(ViewOver&&!FightWon(),"explicit high-enemy-strength loss fixture settles");
            ShowResult();yield return null;Capture("10-loss");
            Check(_resultLives==2&&_campaign.Reserve.Count==reserve&&_result.Captured==null,"loss costs one life and restores whole squad");
            string lost=CampaignSave.Write(_campaign);
            _campaign=CampaignSave.Read(lost);Check(_campaign.Lives==2,"life loss persists in save");

            _campaign=CampaignSave.Read(lost.Replace("lives\t2","lives\t1"),new CampaignTuning{EnemyScaleTier1=300});
            Check(_campaign.Lives==1,"last-life fixture starts with one life");
            _fight=_campaign.StartEncounter(0);BeginFight();_pilotOn=false;_paused=true;yield return null;
            for(int i=0;i<7200&&!ViewOver;i++){HudTicks(1);if(i%120==0)yield return null;}
            Check(ViewOver&&!FightWon(),"last-life defeat settles");
            ShowResult();yield return null;Capture("10-last-life");
            Check(_result.RunOver&&_resultLives==0&&ResultLine(_result).Contains("no lives left"),"final life ends run with correct reason");

            _campaign=CampaignSave.Read(CampaignSave.Write(FreshRoadPreview()),new CampaignTuning{EnemyScaleTier1=1});
            _fight=_campaign.StartEncounter(0);BeginFight();_pilotOn=false;_paused=true;yield return null;
            for(int i=0;i<7200&&!ViewOver;i++){HudTicks(1);if(i%120==0)yield return null;}
            Check(ViewOver&&FightWon(),"explicit low-enemy-strength victory fixture settles");
            ShowResult();yield return null;Capture("10-victory");
            Check(_resultLives==3&&_resultUsesLives,"victory shows unchanged actual lives rather than result default zero");

            GateRestore(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","chief.txt")));
            yield return null;
            Check(!RoadHud&&!ViewSim.RoadCombatOn&&_nerveFill[1].enabled,"historical chief retains old shared nerve HUD");
            Check(DisplayOrderCost(ViewSim,Directive.Hold)==1,"historical Hold cost preserved");
            Capture("11-legacy");
            Log("KNOWN FAIL: Fall Back spam regression retained; technical HUD checks are not balance or human feel approval.");
        }
    }
}
