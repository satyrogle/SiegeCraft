using System;
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
        void GateTicks(int count)
        {
            for(int i=0;i<count&&!_fight.Over&&!_fight.Sim.AwaitingVerdict;i++)_fight.Tick();
            DrainEvents(); RefreshFight(); DrawBattle();
        }

        void GateRestore(string save)
        {
            _campaign=CampaignSave.Read(save);_playback=null;_fight=_campaign.Active;
            BeginFight();_pilotOn=false;_paused=true;_speed=1;_accum=0;
        }

        IEnumerator ValidateClarity()
        {
            yield return null;
            _savePath=Path.Combine(_capture,"review-save.txt");_gatesReview=true;
            NewReviewRoute();yield return null;Capture("01-camp");
            Check(_campaign.Stage==CampaignStage.ChooseTribe&&_campaign.Active==null,"review starts at camp after authentic tutorial");
            Check(_campaign.Tuning.PathFights==new CampaignTuning().PathFights,"review retains default road length");
            Check(Click("tribe-"+CampaignTribes.BloodridgeId),"camp opens fixed Bloodridge road");yield return null;Capture("02-road");
            string road=CampaignSave.Write(_campaign);
            Check(Click("road-details"),"road details raycast");yield return null;
            Check(Click("modal-close"),"road details close");yield return null;
            Check(CampaignSave.Write(_campaign)==road,"road inspection mutates no state");
            Check(Click("offer-0"),"road choice opens squad");yield return null;Capture("03-squad");
            Check(Click("squad-details"),"squad full briefing remains accessible");yield return null;Capture("04-squad-details");
            Check(Click("modal-close"),"squad details close");yield return null;
            Check(Click("start-fight"),"selected squad starts real encounter");_pilotOn=false;_paused=true;_speed=1;yield return null;
            Check(ViewSim.ParryCenterOn,"new battle uses current parry rules");
            long tick=ViewSim.CurrentTick;
            _paused=false;for(int i=0;i<30;i++)yield return null;_paused=true;
            Check(ViewSim.CurrentTick>tick&&ViewSim.CurrentTick-tick<=SimClock.TicksPerSecond+2,"normal speed advances within one second over 30 validation frames");
            int guard=0;
            while(CurrentThreat.IsNone&&!ViewOver&&guard++<1000)GateTicks(1);
            Check(!CurrentThreat.IsNone,"real road fight produces a ranked threat");
            var top=ViewSim.TopThreat(0);
            Check(CurrentThreat.Name==top.Name&&CurrentThreat.ActorId==top.ActorId,"HUD follows sim top priority");
            Capture("05-threat");
            string state=ViewSim.OrderedStateSnapshot;int eventCount=ViewSim.Events.Count;
            for(int i=0;i<10;i++){DrawClarity(ViewSim);yield return null;}
            Check(state==ViewSim.OrderedStateSnapshot&&eventCount==ViewSim.Events.Count,"paused threat and outcome reads are side effect free");
            Check(Click("fight-read"),"fight details raycast");yield return null;Capture("06-fight-details");
            Check(Click("modal-close"),"fight details close");yield return null;
            Check(_paused,"closing details leaves combat paused");
            while(!CurrentThreat.ParryWindow&&!ViewOver&&guard++<1600)GateTicks(1);
            Check(CurrentThreat.ParryWindow,"normal road fight reaches beat window");
            string beforeHold=CampaignSave.Write(_campaign);
            File.WriteAllText(Path.Combine(_capture,"beat-save.txt"),beforeHold);
            Check(Click("order-0"),"paused Hold uses real order button");GateTicks(30);
            var paused=ViewSim.OrderResults(0).Last();
            Check(paused.Paused&&!paused.Effects.Any(e=>e.Kind==OrderEffectKind.Parried),"paused Hold never reported as parry");
            Capture("07-paused-hold");
            GateRestore(beforeHold);yield return null;
            _paused=false;Check(Click("order-0"),"live timed Hold uses real order button");_paused=true;GateTicks(30);
            var parry=ViewSim.OrderResults(0).Last();
            Check(!parry.Paused&&parry.Effects.Any(e=>e.Kind==OrderEffectKind.Parried),"live timed Hold produces authoritative parry effect");
            Check(_orderReceipt.key.ToUpperInvariant().Contains("PARRIED"),"visible receipt says parried");Capture("08-perfect-hold");
            string after=CampaignSave.Write(_campaign),snapshot=ViewSim.OrderedStateSnapshot;
            GateRestore(after);yield return null;
            Check(ViewSim.OrderedStateSnapshot==snapshot,"save reload reproduces active battle snapshot");
            Check(_orderReceipt.key.ToUpperInvariant().Contains("PARRIED"),"save reload reconstructs outcome feedback");
            GateRestore(beforeHold);yield return null;
            Check(!_orderReceipt.key.ToUpperInvariant().Contains("PARRIED"),"rewind to pre-order state clears stale outcome");
            // Resolve an empty Hold and observe the delayed Final state even if no command is added.
            _paused=false;Check(Click("order-1"),"Fall Back uses real control");_paused=true;GateTicks(190);
            ReadClarity(ViewSim);
            Check(_orderReads.Count==ViewSim.OrderResults(0).Count,"outcome cache refreshes as orders expire");
            if(!ViewOver)
            {
                Check(Click("order-2"),"Focus arms targeting");yield return null;
                var foe=ViewSim.Units.First(u=>u.Side==1&&u.Alive&&!u.Benched);
                Check(Click("target-"+foe.Id),"Focus reaches fighter through HUD");
                Check(ViewSim.FocusedId(0)==foe.Id,"targeting sends correct target to sim");Capture("09-focus");
            }
            // A real defeat or victory must remain inspectable and saveable; no outcome is forced.
            guard=0;while(!ViewOver&&guard++<10000)GateTicks(10);
            if(ViewSim.AwaitingVerdict)Order(Directive.Hold);
            Check(ViewOver,"fixed battle settles under normal rules");_endAge=3;RefreshFight();yield return null;
            Check(Click("see-result"),"settled fight opens real reward / loss screen");yield return null;Capture("10-result");
            Check(Click("result-tactics"),"result tactics remain accessible");yield return null;
            Check(Click("modal-close"),"result tactics close");yield return null;
            File.WriteAllText(Path.Combine(_capture,"settled-save.txt"),CampaignSave.Write(_campaign));
            var archived=_campaign.Archive.Fights.Last();
            WatchFight(archived,0,archived.Ticks);yield return null;
            Check(_playback!=null,"recorded fight opens playback");Capture("11-replay");
            LeavePlayback();CloseModal();yield return null;
            // This checkpoint was reached from the shipped camp through default-rule commands.
            GateRestore(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","chief.txt")));yield return null;
            Check(_fight.Encounter.Kind==EncounterKind.Chief&&ViewSim.ParryCenterOn,"authentic chief checkpoint is current rules");
            Capture("12-chief-start");long lastHold=-100;guard=0;
            while(!ViewOver&&!ViewSim.AwaitingVerdict&&guard++<1800)
            {
                _paused=false;RefreshFight();
                if(_fight.CanCallAssist)Check(Click("order-4"),"chief route assist command");
                var threat=ViewSim.TopThreat(0);
                if(threat.BeatTicks==1&&ViewSim.CurrentTick-lastHold>15&&ViewSim.CanUse(0,Directive.Hold,out _))
                { Check(Click("order-0"),"chief route timed Hold command");lastHold=ViewSim.CurrentTick; }
                else if(ViewSim.NextCombo(0).Combo==FightRules.Riposte&&ViewSim.CanUse(0,Directive.Unleash,out _)&&ViewSim.UnleashTicksLeft(0)==0)
                    Check(Click("order-3"),"chief route Riposte command");
                else if(ViewSim.DirectivePoints(0)>=4&&ViewSim.FocusTicksLeft(0)==0)
                {
                    var target=ViewSim.Units.Where(u=>u.Side==1&&u.Alive&&!u.Benched).OrderBy(u=>u.Row==Row.Front?0:1).ThenBy(u=>u.Hp).First();
                    Check(Click("order-2"),"chief route Focus command");
                    Check(Click("target-"+target.Id),"chief route Focus target");
                }
                _paused=true;GateTicks(1);
                if(guard%60==0)yield return null;
            }
            Check(ViewSim.OrderResults(0).Any(r=>r.Effects.Any(e=>e.Kind==OrderEffectKind.Parried)),"normal-rule chief fight contains an actual parry");
            Check(ViewSim.AwaitingVerdict,"normal-rule chief kneels");Capture("13-chief-kneels");
            Check(Click("order-0"),"Spare goes through actual verdict control");_endAge=3;RefreshFight();yield return null;
            Check(Click("see-result"),"chief reward screen opens");yield return null;
            Check(_result.Won&&_result.Joined&&_result.MaterialsEarned>0,"chief reward and recruits are awarded by Campaign.Finish");
            Capture("14-chief-reward");File.WriteAllText(Path.Combine(_capture,"chief-result-save.txt"),CampaignSave.Write(_campaign));
            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","rematch.txt")));
            _fight=null;GoToStage();yield return null;
            Check(_campaign.Target.Id==CampaignTribes.BloodridgeId&&_campaign.Mind(CampaignTribes.BloodridgeId).Lessons.Count>0,"rematch checkpoint retains authentic learned history");
            Check(Click("rematch"),"rematch clue opens");yield return null;Capture("15-rematch-memory");
            Check(Click("lesson-check"),"rematch lesson verification control");yield return null;
            Check(Click("modal-close"),"rematch closes");yield return null;
            Check(Click("offer-0"),"rematch road can select encounter");yield return null;
            Check(Click("start-fight"),"rematch starts a playable encounter");_pilotOn=false;_paused=true;yield return null;
            Check(ViewSim.ParryCenterOn,"rematch uses current rules");Capture("16-rematch-fight");
            // Controlled pre-existing presentation fixture, not a balance or natural-run result.
            _fight=null;SetPage(Page.Offers);yield return null;
            foreach(bool current in new[]{false,true})
            {
                var chief=TacticsFixture(0,current);
                WatchFight(chief);yield return null;
                SeekPlayback(220);yield return null;DrawBattle();
                Check(ViewSim.ParryCenterOn==current,"chief replay retains its own rules "+current);
                Check(!_tacticsPanel.gameObject.activeSelf,"dense chief panel yields to priority threat "+current);
                Check(Click("chief-moves"),"chief moves guide remains accessible "+current);yield return null;
                Check(Click("modal-close"),"chief guide closes "+current);yield return null;
                Check(Click("fight-read"),"chief detail panel works "+current);yield return null;
                Capture(current?"13-chief-current-details":"12-chief-legacy-details");
                Check(Click("modal-close"),"chief detail closes "+current);yield return null;
                SeekPlayback(0);yield return null;DrawBattle();
                Check(CurrentThreat.IsNone,"seek to start clears threats "+current);
                SeekPlayback(250);yield return null;Capture(current?"15-chief-current":"14-chief-legacy");
                SeekPlayback(chief.Ticks);yield return null;
                Check(!_threatPanel.gameObject.activeSelf,"completed replay hides threat panel "+current);
                LeavePlayback();CloseModal();yield return null;
            }
            foreach(var tribe in new[]{CampaignTribes.BloodridgeId,CampaignTribes.SkullpeakId})
            {
                _playback=null;_fight=null;_campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","camp.txt")));
                _campaign.ChooseTribe(tribe);SetPage(Page.Offers);yield return null;
                Check(Click("beast-offer"),"optional beast remains accessible "+tribe);yield return null;
                Capture("beast-brief-"+tribe);
                Check(Click("squad-details"),"beast details accessible "+tribe);yield return null;
                Check(Click("modal-close"),"beast details close "+tribe);yield return null;
                Check(Click("beast-guide"),"beast move guide accessible "+tribe);yield return null;
                Check(Click("modal-close"),"beast guide closes "+tribe);yield return null;
            }
            Log("Gates 0-2 focused player validation complete. Automated interactions; not human comprehension or balance proof.");
        }
    }
}
