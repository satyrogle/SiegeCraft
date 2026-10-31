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
        IEnumerator ValidateRoad12Hud()
        {
            yield return null;
            _savePath=Path.Combine(_capture,"road12-save.txt");_kitReview=_roadReview=_gatesReview=true;
            NewReviewRoute();yield return null;Capture("01-road");
            Check(_campaign.Lives==3&&LivesText()=="Lives: 3/3","road exposes authoritative three lives");
            Check(Click("road-details"),"road details clickable");yield return null;Capture("02-road-details");
            Check(Click("modal-close"),"close road details");yield return null;
            Check(Click("offer-0"),"road node opens squad");yield return null;
            Check(Click("start-fight"),"squad starts current encounter");_pilotOn=false;_paused=true;_speed=.5f;yield return null;
            Check(RoadHud&&ViewSim.UnitNerveOn,"rules 12 personal nerve HUD active");
            Check(!_nerveFill[0].enabled&&!_nerveBack[1].enabled,"shared nerve bars absent");
            Check(ViewSim.RoadKitOn&&_roadWarning.key.Contains("PIERCE BUG"),"rules 12 remaining Fall Back issue visible");
            Check(_nerveLabel[1].key.Contains(ViewSim.StillFighting(1).ToString()),"enemies left is authoritative");
            Check(_orderCost[0].key.Contains("FREE"),"Hold remains free");
            string fresh=CampaignSave.Write(_campaign),state=ViewSim.OrderedStateSnapshot;
            File.WriteAllText(Path.Combine(_capture,"preview-start.txt"),fresh);
            for(int i=0;i<3;i++){RefreshFight();DrawBattle();yield return null;}
            Check(ViewSim.OrderedStateSnapshot==state,"paused drawing cannot change combat");
            Check(_roadUnits.Count==ViewSim.Units.Count(u=>!u.Benched),"each visible fighter has personal nerve cue");
            Check(Click("fight-read"),"combat Details opens");yield return null;Capture("03-details");
            Check(Click("modal-close"),"close combat Details");yield return null;
            Capture("04-start");CheckCleanTextBounds();
            _canvas.scaleFactor=.75f;CheckCleanTextBounds();_canvas.scaleFactor=1;
            Check(Click("order-1"),"Fall Back UI raycast works");
            RefreshFight();DrawBattle();
            Check(ViewSim.FallBackTicksLeft(0)>0&&_roadWarning.key.Contains("DODGING"),"Fall Back active receipt");
            foreach(var f in _figures.Where(f=>f.unit.Side==0&&f.unit.Alive))Check(f.root.anchoredPosition.x<f.feet.x,"whole squad visibly steps back");
            Check(Click("order-0"),"Hold UI raycast works");
            Check(Click("order-2"),"Focus targeting opens");HandleUiKey(KeyCode.Return);
            Check(ViewSim.FocusedId(0).Length>0,"keyboard confirms Focus target");
            GateRestore(fresh);yield return null;_speed=.5f;_paused=false;
            long tick=ViewSim.CurrentTick;for(int i=0;i<24;i++)yield return null;
            _paused=true;Check(ViewSim.CurrentTick>tick,"live player advances at half speed");
            GateRestore(fresh);yield return null;
            bool chain=false,defence=false,frenzy=false,fled=false,saved=false,burn=false,boost=false,brand=false,cleave=false;
            var checkedFled=new HashSet<string>();
            for(int i=0;i<7200&&!ViewOver;i++)
            {
                HudTicks(1);
                if(!burn&&ViewSim.Units.Any(u=>u.Alive&&u.BurnTicks>0)){burn=true;Check(_kitUnits.Values.Any(r=>r.burn.key.Contains("BURN")),"burn stacks visible");Capture("05-burn");}
                if(!boost&&ViewSim.Events.Any(e=>e.Kind==BattleEventKind.Boost)){boost=true;Check(_kitBoosts.Count>0,"automatic boost events recorded for HUD");Capture("05-boost");}
                if(!brand&&ViewSim.Units.Any(u=>u.Alive&&u.BrandTicks>0)){brand=true;Check(_kitUnits.Values.Any(r=>r.ring.Any(p=>p.enabled)),"Brand countdown ring visible");Capture("05-brand");}
                if(!cleave&&_threatReads.Any(t=>t.Name==ChiefKits.CleaveName)){cleave=true;Check(_raidBeatRing.enabled,"carrier Cleave beat ring visible including secondary threat");Capture("05-cleave");}
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
            Check(burn&&boost&&brand,"fixed road encounter exposes Burn, boosts and Brand");
            Log("Cleave observed in campaign fixture="+cleave+"; a carrier may be interrupted before telling. Dedicated fixture below requires it.");
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
            Check(_roadPayoff.Length==0&&_kitBoosts.Count==0,"backward seek clears stale combo and boost stamps");
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
            Check(_resultUsesKit&&_result.Streak==1&&_campaign.Streak==1,"result exposes authoritative first-win multiplier");

            // A deliberately durable mechanics fixture, not campaign tuning or natural victory evidence.
            UnitSpec U(string id,UnitClass c)=>new UnitSpec(id,c,SliceClasses.Stats(c).DefaultRow,UnitRole.Fighter,100,6000);
            var ours=new List<UnitSpec>{U("clarth.w",UnitClass.Warden).WithRoadKit(1,ChiefMove.None),U("clarth.s",UnitClass.Skirmisher),
                U("clarth.t",UnitClass.Trapper),U("clarth.m",UnitClass.Marksman),U("clarth.h",UnitClass.Medic)};
            var theirs=new List<UnitSpec>{U("enemy.w",UnitClass.Warden).WithRoadKit(1,ChiefMove.Cleave|ChiefMove.Brand),
                U("enemy.s",UnitClass.Skirmisher).WithRoadKit(1,ChiefMove.BurningRing),U("enemy.m",UnitClass.Marksman)};
            var setup=new BattleSetup("camp.999.raiders.warparty",13,ours,theirs,new TacticsProfile(),new TacticsProfile(),
                BattleSetup.DefaultTickLimit,BattleSetup.DefaultDirectivePoints,NerveRules.On(NerveRules.Full,NerveRules.Full),
                10,400,new SideRules(1,0,true),new SideRules(0,0,false),FightRules.Campaign.WithRoadKit());
            var fixture=new BattleSimulation(setup);var session=new SimSession(fixture);
            while(!fixture.IsFinished){session.Tick();if(fixture.CurrentTick%300==0)yield return null;}
            var recorded=_campaign.Archive.Add(setup,session.Log,fixture);
            WatchFight(recorded,0,recorded.Ticks);yield return null;
            foreach(var word in new[]{BoostRules.ShieldBash,BoostRules.Steady,BoostRules.TwinCut,BoostRules.Rearm,BoostRules.Stim})
            {
                var evt=fixture.Events.FirstOrDefault(e=>e.Kind==BattleEventKind.Boost&&e.Detail==word);
                Check(evt.Kind==BattleEventKind.Boost,"durable fixture exercises automatic "+word);
                SeekPlayback(evt.Tick+1);yield return null;
                Check(_kitBoosts.ContainsKey(evt.ActorId)&&_kitUnits[evt.ActorId].boost.key.Contains(BoostWord(word)),"recorded boost has visible cue: "+word);
                Capture("12-boost-"+word);
            }
            var flare=fixture.Events.FirstOrDefault(e=>e.Kind==BattleEventKind.Cash&&e.Detail==BurnRules.FlareDetail);
            Check(flare.Kind==BattleEventKind.Cash,"fixture really flares Burn");SeekPlayback(flare.Tick+1);yield return null;
            Check(_kitUnits[flare.TargetId].burn.key.Contains("FLARE"),"flare stays visible on its victim alongside simultaneous combo hits");Capture("12-flare");
            var ring=fixture.Events.FirstOrDefault(e=>e.Kind==BattleEventKind.Tell&&e.Detail==ChiefKits.RingName);
            Check(ring.Kind==BattleEventKind.Tell,"fixture really casts a road Ring");SeekPlayback(ring.Tick+1);yield return null;
            Check(_beatTrack.enabled&&_beatWindow.enabled&&_threatReads.Any(t=>t.CastBar),"road Ring cast bar visible even beside another threat");
            Check(_threatTarget.key.Contains("CASTING")||_threatHeading.key.Contains("INTERRUPT"),"road Ring identified on screen");Capture("12-ring");
            var cleaveTell=fixture.Events.FirstOrDefault(e=>e.Kind==BattleEventKind.Tell&&e.Detail==ChiefKits.CleaveName);
            Check(cleaveTell.Kind==BattleEventKind.Tell,"durable fixture really tells Cleave");SeekPlayback(cleaveTell.Tick+1);yield return null;
            Check(_raidBeatRing.enabled&&_threatReads.Any(t=>t.Name==ChiefKits.CleaveName),"durable fixture Cleave has a visible beat ring");Capture("12-cleave");
            SeekPlayback(recorded.Ticks);yield return null;
            Check(ViewSim.OrderedStateSnapshot==fixture.OrderedStateSnapshot,"durable mechanics fixture replays exactly");
            SeekPlayback(0);yield return null;
            Check(_kitBoosts.Count==0&&_kitFlares.Count==0&&_roadPayoff==""&&!_beatWindow.enabled&&_kitUnits.Values.All(r=>r.ring.All(p=>!p.enabled)),"rewind removes boosts, flare, payoff, cast and Brand ring");
            LeavePlayback();CloseModal();yield return null;

            string legacy11=Argument("-ksLegacy11");
            Check(legacy11!=null,"cross-build rules 11 fixture supplied");
            GateRestore(File.ReadAllText(legacy11));yield return null;
            Check(RoadHud&&!KitHud&&_kitUnits.Count==0&&_roadWarning.key.Contains("SPAM IS TOO STRONG"),"rules 11 active save retains historical HUD and Fall Back warning");Capture("13-rules11");
            GateRestore(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","chief.txt")));
            yield return null;
            Check(!RoadHud&&!ViewSim.RoadCombatOn&&_nerveFill[1].enabled,"historical chief retains old shared nerve HUD");
            Check(DisplayOrderCost(ViewSim,Directive.Hold)==1,"historical Hold cost preserved");
            Capture("11-legacy");
            Log("Rules 12 HUD checks complete; scripted checks are not human feel approval.");
        }
    }
}




