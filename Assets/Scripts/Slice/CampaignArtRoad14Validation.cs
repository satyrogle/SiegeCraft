using System.Collections;
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
        IEnumerator ValidateRoad14Hud()
        {
            Log("Rules 14: original owner replay, protected leader, first-fight parry and visible teamwork.");
            _savePath=Path.Combine(_capture,"road14-save.txt");
            _squadReview=_clearReview=_kitReview=_roadReview=_gatesReview=true;
            NewReviewRoute();yield return null;
            Check(Click("offer-0"),"select road encounter");yield return null;
            _auto=false;Check(Click("start-fight"),"start real encounter");_auto=true;_pilotOn=false;_paused=true;yield return null;
            Check(ViewSim.SquadScreenOn&&ViewSim.RoadStage==1,"new campaign starts rules 14");
            Check(_speed==.5f,"normal learning pace stays half speed");
            Check(ViewSim.LeaderProtected(ViewSim.Leader(0))&&_leaderShield.enabled,"leader protection is shown");
            Capture("01-protected-leader");
            string active=CampaignSave.Write(_campaign);
            var back=CampaignSave.Read(active);
            Check(back.Active.Sim.OrderedStateSnapshot==ViewSim.OrderedStateSnapshot,"rules 14 active save restores exactly");
            bool windup=false,parried=false,combo=false;
            for(int i=0;i<3000&&!ViewOver;i++)
            {
                _paused=true;HudTicks(1);
                var sim=ViewSim;
                foreach(var hit in sim.Events.Where(e=>e.Tick==sim.CurrentTick-1&&e.Kind==BattleEventKind.Hit&&e.TargetId=="leader"))
                    Check(!sim.Units.Any(u=>u.Side==0&&!u.IsLeader&&u.Alive&&!u.Benched&&!u.Fled),"leader is hit only after squad falls");
                if(!windup&&_parryOuter.enabled)
                {windup=true;Capture("02-incoming-ring");}
                if(!parried&&_threatReads.Any(t=>t.ParryWindow))
                {
                    _paused=false;RefreshFight();DrawBattle();
                    Check(_parryOuter.enabled&&_parryInner.enabled&&_parryLink.enabled,"ring and attacker-to-victim link visible");
                    Check(_parryCall.key.Contains("HOLD NOW")&&_orderEffect[0].key.Contains("PARRY NOW"),"fighter and Hold button agree on real parry window");
                    Capture("03-hold-now");Check(Click("order-0"),"live Hold button clicked");
                    _paused=true;HudTicks(18);
                    Check(sim.Events.Any(e=>e.Kind==BattleEventKind.Parry),"live click actually parries");
                    parried=true;Capture("04-parried");
                }
                if(!combo&&_squadMoment!=null&&sim.CurrentTick-_squadMomentStart>=20)
                {
                    combo=true;Check(_comboHalos.All(r=>r.enabled)&&_comboLinks.All(l=>l.enabled),"both squadmates and victim connected");
                    Check(_comboCall.root.childCount>0&&_squadNames.root.childCount>0,"specific finisher and pair named");
                    Check(_squadMoment.origin!=_squadMoment.finisher,"combo has two distinct teammates");
                    Capture("05-squad-combo");
                }
                if(i%120==0)yield return null;
            }
            Check(windup&&parried&&combo,"all three presentation paths exercised in ordinary first fight");
            Check(ViewOver,"ordinary fight reaches an outcome");
            var final=ViewSim.OrderedStateSnapshot;ShowResult();yield return null;
            var latest=_campaign.Archive.Fights.Last();
            Check(_campaign.Archive.TryReplay(latest.FightId,out var again,out var why),"new archive verifies: "+why);
            Check(again.OrderedStateSnapshot==final,"new archived outcome is exact");
            WatchFight(latest,0,latest.Ticks);yield return null;
            var eventAt=again.Events.First(e=>e.Kind==BattleEventKind.ComboFinish);
            SeekPlayback(eventAt.Tick+1);yield return null;
            Check(_squadMoment!=null,"replay reconstructs confirmed combo");
            SeekPlayback(0);yield return null;
            Check(_squadMoment==null&&_squadMoments.Count==0&&_comboHalos.All(r=>!r.enabled),"rewind clears all teamwork cues");
            LeavePlayback();CloseModal();yield return null;
            // Stage-four overlap: Brand/cast instructions must survive a simultaneous closing parry ring.
            UnitSpec U(string id,UnitClass cls)=>new UnitSpec(id,cls,SliceClasses.Stats(cls).DefaultRow,UnitRole.Fighter,100,9000);
            var setup=new BattleSetup("camp.999.raiders.warparty",13,
                new[]{U("clarth.w",UnitClass.Warden),U("clarth.s",UnitClass.Skirmisher),U("clarth.t",UnitClass.Trapper),U("clarth.m",UnitClass.Marksman),U("clarth.h",UnitClass.Medic)},
                new[]{U("enemy.w",UnitClass.Warden).WithRoadKit(1,ChiefMove.Cleave|ChiefMove.Brand),U("enemy.s",UnitClass.Skirmisher).WithRoadKit(1,ChiefMove.BurningRing),U("enemy.m",UnitClass.Marksman)},
                new TacticsProfile(),new TacticsProfile(),2400,8,NerveRules.On(),10,400,
                new SideRules(1,0,true),new SideRules(0,0,false,150),FightRules.Campaign.WithClearRoad().WithSquadScreen());
            var carrier=new BattleSimulation(setup);var session=new SimSession(carrier);while(!carrier.IsFinished)session.Tick();
            var archive=_campaign.Archive.Add(setup,session.Log,carrier);WatchFight(archive,0,archive.Ticks);yield return null;
            bool brandSeen=false,castSeen=false,carrierBeat=false;
            for(long tick=0;tick<archive.Ticks;tick+=5)
            {
                SeekPlayback(tick);
                bool brand=ViewSim.Units.Any(u=>u.Side==0&&u.Alive&&u.BrandTicks>0);
                if(!brandSeen&&brand&&!_threatReads.Any(t=>t.ParryWindow))
                {brandSeen=true;Check(_threatAnswers.key.Contains("BRAND"),"Brand dodge instruction survives other windups");Capture("06-brand-overlap");}
                if(!castSeen&&CurrentThreat.CastBar&&!brand&&!_threatReads.Any(t=>t.ParryWindow))
                {castSeen=true;Check(_threatAnswers.key.Contains("FOCUS"),"interruptible cast keeps Focus instruction");Capture("07-cast");}
                if(!carrierBeat&&_threatReads.Any(t=>t.Name==ChiefKits.CleaveName&&t.ParryWindow))
                {carrierBeat=true;Check(_parryOuter.enabled,"carried Cleave uses visible attacker ring");Capture("08-carried-cleave");}
                if(brandSeen&&castSeen&&carrierBeat)break;
                if(tick%150==0)yield return null;
            }
            Check(brandSeen&&castSeen&&carrierBeat,"stage-four Brand, cast and Cleave inspected");
            SeekPlayback(archive.Ticks);yield return null;Check(ViewSim.OrderedStateSnapshot==carrier.OrderedStateSnapshot,"stage-four archive remains exact");
            LeavePlayback();CloseModal();yield return null;
            string owner=Argument("-ksOwner13");
            Check(CampaignSave.TryReadFile(owner,null,out var old,out why),"owner rules 13 copy loads: "+why);
            var oldFight=old.Archive.Fights.Last();
            Check(old.Archive.TryReplay(oldFight.FightId,out var oldSim,out why),"original owner archive still exact: "+why);
            Check(!oldSim.SquadScreenOn&&oldSim.Units.First(u=>u.Id=="leader").Hp==0,"historical exposed-leader outcome preserved");
            Check(oldSim.Units.Count(u=>u.Side==0&&u.Alive)==5,"original five surviving squadmates preserved");
            Log("Rules 14 packaged validation complete. Scripted input and captures; owner feel remains a separate verdict.");
        }
    }
}
