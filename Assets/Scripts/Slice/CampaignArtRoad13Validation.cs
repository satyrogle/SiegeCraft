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
        IEnumerator ValidateRoad13Hud()
        {
            Log("Rules 13: real UI, staged road, icon HUD, carriers, save and replay. Scripted evidence, not owner feel approval.");
            _savePath=Path.Combine(_capture,"road13-save.txt");_clearReview=_kitReview=_roadReview=_gatesReview=true;
            NewReviewRoute();yield return null;Capture("01-road");
            Check(_campaign.Road.Rows==4&&_campaign.StagedBloodridgeRoad,"four-step road is authoritative");
            Check(Click("offer-0"),"select a road squad");yield return null;
            Check(Click("start-fight"),"start stage one");_pilotOn=false;_paused=true;_speed=.5f;yield return null;
            Check(ViewSim.ClearRoadOn&&ViewSim.RoadStage==1,"first encounter uses rules 13 combos stage");
            Check(!_nerveFill[0].enabled,"personal nerve replaces shared bar");
            Check(_roadWarning.root.childCount==0,"historical bug warning absent in fixed rules");
            Check(_clearUnits.Count==ViewSim.Units.Count(u=>!u.Benched),"every fighter has icon readout");
            Check(Click("fight-read"),"Details opens");yield return null;Capture("02-details");
            Check(Click("modal-close"),"Details closes");yield return null;
            string fresh=CampaignSave.Write(_campaign),state=ViewSim.OrderedStateSnapshot;
            File.WriteAllText(Path.Combine(_capture,"start13.txt"),fresh);
            RefreshFight();DrawBattle();Check(ViewSim.OrderedStateSnapshot==state,"drawing leaves authoritative sim unchanged");
            Check(Click("order-1"),"Fall Back button accepts input");RefreshFight();DrawBattle();
            Check(ViewSim.FallBackTicksLeft(0)>0,"dodge sent to sim");
            _paused=false;DrawBattle();Check(_threatAnswers.key.Contains("DODGING"),"dodge receipt visible");_paused=true;
            GateRestore(fresh);yield return null;
            Check(Click("order-2"),"Focus targeting opens");HandleUiKey(KeyCode.Return);
            Check(ViewSim.FocusedId(0).Length>0,"Focus target confirmed through keyboard handler");
            RefreshFight();DrawBattle();Check(_clearUnits[ViewSim.FocusedId(0)][8].enabled,"Focus target keeps a gold crosshair");
            GateRestore(fresh);yield return null;Capture("03-start");CheckCleanTextBounds();
            _speed=.5f;_paused=false;long tick=ViewSim.CurrentTick;for(int i=0;i<30;i++)yield return null;
            _paused=true;Check(ViewSim.CurrentTick>tick,"half-speed player actually advances");
            GateRestore(fresh);yield return null;
            bool combo=false;int startEvents=ViewSim.Events.Count;
            for(int i=0;i<7200&&!ViewOver;i++)
            {
                HudTicks(1);
                if(!combo&&_roadPayoff.Length>0){combo=true;Capture("04-player-combo");Check(!_roadPayoff.StartsWith("ENEMY"),"only player receives combo stamp");}
                if(i==200){state=ViewSim.OrderedStateSnapshot;GateRestore(CampaignSave.Write(_campaign));yield return null;Check(state==ViewSim.OrderedStateSnapshot,"active rules 13 save restores exact state");}
                if(i%120==0)yield return null;
            }
            Check(combo,"natural first fight exercises a player payoff");
            Check(!ViewSim.Events.Skip(startEvents).Any(e=>e.Kind==BattleEventKind.Boost||e.Kind==BattleEventKind.Defence),"first fight does not introduce defence or boosts early");
            Check(ViewOver,"default first fight settles");Log("Default first fight outcome: "+(FightWon()?"WIN":"LOSS"));
            ShowResult();yield return null;Capture("05-result");
            Check(_campaign.Archive.TryReplay(_campaign.Archive.Fights.Last().FightId,out _,out var failure),"default first fight archived replay: "+failure);

            // Controlled strong-squad progression fixture, not a balance change or natural run victory.
            _campaign=CampaignSave.Read(CampaignSave.Write(FreshRoadPreview()),new CampaignTuning{EnemyScaleTier1=1});
            for(int stage=1;stage<=4;stage++)
            {
                _fight=_campaign.StartEncounter(0);BeginFight();_pilotOn=false;_paused=true;yield return null;
                Check(ViewSim.RoadStage==stage,"campaign starts stage "+stage);
                var restored=CampaignSave.Read(CampaignSave.Write(_campaign),_campaign.Tuning);
                Check(restored.Active!=null&&restored.Active.Sim.RoadStage==stage&&restored.Active.Sim.OrderedStateSnapshot==ViewSim.OrderedStateSnapshot,"stage "+stage+" survives an active-save reload");
                Check(ViewSim.DefencesAvailable==(stage>=2)&&ViewSim.BoostsAvailable==(stage>=3)&&ViewSim.CarriersAvailable==(stage>=4),"authoritative availability at stage "+stage);
                Capture("06-stage-"+stage);
                for(int i=0;i<7200&&!ViewOver;i++){HudTicks(1);if(i%180==0)yield return null;}
                Check(ViewOver&&FightWon(),"controlled progression fixture wins stage "+stage);
                ShowResult();yield return null;
            }
            Check(_campaign.Offers.Any(e=>e.Kind==EncounterKind.Chief),"chief offered after four fights");
            foreach(var archived in _campaign.Archive.Fights)Check(_campaign.Archive.TryReplay(archived.FightId,out _,out failure),"staged archive replays "+archived.FightId+": "+failure);

            // Durable units keep simultaneous carrier cues alive long enough to inspect in replay.
            UnitSpec U(string id,UnitClass c)=>new UnitSpec(id,c,SliceClasses.Stats(c).DefaultRow,UnitRole.Fighter,100,9000);
            var ours=new List<UnitSpec>{U("clarth.w",UnitClass.Warden).WithRoadKit(1,ChiefMove.None),U("clarth.s",UnitClass.Skirmisher),U("clarth.t",UnitClass.Trapper),U("clarth.m",UnitClass.Marksman),U("clarth.h",UnitClass.Medic)};
            var theirs=new List<UnitSpec>{U("enemy.w",UnitClass.Warden).WithRoadKit(1,ChiefMove.Cleave|ChiefMove.Brand),U("enemy.s",UnitClass.Skirmisher).WithRoadKit(1,ChiefMove.BurningRing),U("enemy.m",UnitClass.Marksman)};
            var setup=new BattleSetup("camp.999.raiders.warparty",13,ours,theirs,new TacticsProfile(),new TacticsProfile(),2400,8,
                NerveRules.On(NerveRules.Full,NerveRules.Full),10,400,new SideRules(1,0,true),new SideRules(0,0,false,150),FightRules.Campaign.WithClearRoad());
            var fixture=new BattleSimulation(setup);var session=new SimSession(fixture);
            while(!fixture.IsFinished)session.Tick();
            var recorded=_campaign.Archive.Add(setup,session.Log,fixture);
            WatchFight(recorded,0,recorded.Ticks);yield return null;
            var boost=fixture.Events.First(e=>e.Kind==BattleEventKind.Boost&&e.ActorSide==0);
            SeekPlayback(boost.Tick+1);yield return null;
            Check(_clearUnits[boost.ActorId][5].enabled,"boost is a lightning icon");
            Check(_kitUnits[boost.ActorId].boost.root.childCount==0,"boost word removed from body");Capture("07-icons");
            var cleave=fixture.Events.First(e=>e.Kind==BattleEventKind.Tell&&e.Detail==ChiefKits.CleaveName);
            SeekPlayback(cleave.Tick+1);yield return null;
            Check(_raidBeatRing.enabled&&_raidBeatRing.rectTransform.sizeDelta.x>=60*Px,"large Cleave timing ring visible");Capture("08-cleave");
            Check(!_castBack.enabled&&!_heavyTargetBack.enabled,"no empty word backplates remain");
            long branded=-1;
            for(long t=0;t<recorded.Ticks;t+=10){SeekPlayback(t);if(ViewSim.Units.Any(u=>u.Side==0&&u.Alive&&u.BrandTicks>0)){branded=t;break;}}
            Check(branded>=0,"durable fixture reaches player Brand");yield return null;
            Check(_kitUnits.Values.Any(r=>r.ring.Any(p=>p.enabled)),"Brand countdown has ring");
            foreach(var u in ViewSim.Units.Where(u=>u.Alive&&u.BrandTicks>0))
            {
                var r=_kitUnits[u.Id];Check(r.ring[0].rectTransform.anchoredPosition.y>0,"Brand ring is above its fighter's feet");
                Check(r.brand.y>=-34&&r.brand.y<=-4,"countdown sits at fighter body, clear of header");
            }
            Check(_threatAnswers.key.Contains("BRAND")||_threatAnswers.key.Contains("HOLD NOW"),"urgent Brand or simultaneous parry gets the main instruction");Capture("09-brand");
            SeekPlayback(recorded.Ticks);yield return null;Check(ViewSim.OrderedStateSnapshot==fixture.OrderedStateSnapshot,"new format replay finishes exactly");
            SeekPlayback(0);yield return null;
            Check(_kitBoosts.Count==0&&_roadPayoff==""&&_kitUnits.Values.All(r=>r.ring.All(p=>!p.enabled)),"rewind clears transient cues");
            LeavePlayback();CloseModal();yield return null;
            string legacy=Argument("-ksLegacy12");Check(legacy!=null,"cross-build rules 12 fixture supplied");
            GateRestore(File.ReadAllText(legacy));yield return null;
            Check(!ViewSim.ClearRoadOn&&ViewSim.RoadKitOn&&_roadWarning.key.Contains("PIERCE BUG"),"historical rules 12 retains truthful legacy HUD");Capture("10-legacy12");
            Log("Rules 13 player validation complete.");
        }
    }
}
