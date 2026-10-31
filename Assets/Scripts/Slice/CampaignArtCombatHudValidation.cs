using System;
using System.Collections;
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
        // Exercise the same presentation clock and poses as Update while advancing deterministic ticks.
        void HudTicks(int count)
        {
            for(int i=0;i<count&&!_fight.Over&&!ViewSim.AwaitingVerdict;i++)
            {
                _visualTime+=1f/SimClock.TicksPerSecond;
                AdvanceCombatPoses(1f/SimClock.TicksPerSecond);
                Anticipate();_fight.Tick();DrainEvents();RefreshFight();DrawBattle();
            }
        }
        void CheckCleanTextBounds()
        {
            Canvas.ForceUpdateCanvases();
            if(CleanCombatHud&&_heavyTargetBack.enabled)Check(_heavyTargetLabel.y<=365,"target marker remains above feedback strip");
            foreach(var label in new[]{_threatHeading,_threatTarget,_threatAnswers,_pxTitle,_pxStats,_pxBanner,_pxBanner2})
                foreach(var part in label.root.GetComponentsInChildren<RawImage>())
                {
                    var corners=new Vector3[4];part.rectTransform.GetWorldCorners(corners);
                    var lo=_root.InverseTransformPoint(corners[0]);var hi=_root.InverseTransformPoint(corners[2]);
                    Check(lo.x>=-W/2&&hi.x<=W/2,"HUD text stays inside canvas: "+label.key.Split('|')[0]);
                }
            for(int i=0;i<5;i++)
                foreach(var label in new[]{_orderTitle[i],_orderEffect[i],_orderCost[i]})
                    foreach(var part in label.root.GetComponentsInChildren<RawImage>())
                    {
                        var r=part.rectTransform;
                        Check(r.anchoredPosition.x>=0&&r.anchoredPosition.x+r.sizeDelta.x<=134*Px,
                            "order "+(i+1)+" text fits its button: "+label.key.Split('|')[0]);
                    }
        }

        IEnumerator ValidateCombatHud()
        {
            yield return null;
            _savePath=Path.Combine(_capture,"hud-save.txt");_gatesReview=true;
            NewReviewRoute();yield return null;
            Check(Click("tribe-"+CampaignTribes.BloodridgeId),"HUD route opens Bloodridge");yield return null;
            Check(Click("offer-0"),"HUD route picks real road offer");yield return null;
            _auto=false;Check(Click("start-fight"),"manual fight starts");_auto=true;_pilotOn=false;_paused=true;yield return null;
            Check(ViewSim.SquadFirstOn,"fresh review road uses rules 9");
            Check(CleanCombatHud&&_speed==.5f,"Bloodridge cleanup retains half-speed manual default");
            Check(_nerveFill[0].rectTransform.sizeDelta.y==24,"nerve bar is 24 screen pixels high");
            Check(_buttons["order-0"].GetComponent<RectTransform>().sizeDelta.y==84,"order controls have taller hit areas");
            Check(!_orderReceipt.root.gameObject.activeSelf&&!_threatOthers.root.gameObject.activeSelf,"duplicate live text strips are removed");
            string start=CampaignSave.Write(_campaign);
            Capture("01-ready");yield return null;CheckCleanTextBounds();
            foreach(float expected in new[]{.25f,1f,2f,4f,.5f})
            {
                Check(Click("speed"),"speed control receives pointer");RefreshFight();
                Check(_speed==expected,"pace reaches "+expected);
            }
            Check(CampaignSave.Write(_campaign)==start,"pace and rendering do not mutate campaign");
            long beganTick=ViewSim.CurrentTick;float beganPose=_figures[0].poseTime;
            _paused=false;for(int i=0;i<60;i++)yield return null;_paused=true;
            Check(ViewSim.CurrentTick>beganTick&&_figures[0].poseTime>beganPose,"actual Update advances moving fighters and combat at selected pace");
            GateRestore(start);yield return null;_speed=.5f;
            int guard=0;while(CurrentThreat.IsNone&&guard++<900)HudTicks(1);
            Check(!CurrentThreat.IsNone,"real fight exposes an incoming threat");
            Check(_castLabel.key.StartsWith("ATTACKING"),"source is identified at its fighter");
            Check(_castLabel.y>=143&&_castBack.enabled,"source label is backed and below threat panel");
            Check(_heavyTargetLabel.key.StartsWith("INCOMING"),"target is identified at its fighter");
            Check(_beatTrack.rectTransform.sizeDelta.y==14,"primary timing track is readable at 14 screen pixels");
            string early=CampaignSave.Write(_campaign);
            _paused=false;RefreshFight();DrawBattle();Capture("02-incoming");_paused=true;
            string state=ViewSim.OrderedStateSnapshot;
            for(int i=0;i<5;i++){DrawBattle();RefreshFight();yield return null;}
            Check(state==ViewSim.OrderedStateSnapshot,"paused HUD reads do not change simulation");
            Check(Click("fight-read"),"Details is clickable");yield return null;Capture("03-details");
            Check(Click("modal-close"),"Details closes");yield return null;
            Check(_paused,"inspection retains pause");
            Check(Click("order-2"),"Focus arms");RefreshFight();yield return null;
            Check(_targeting&&_pxBanner.key.Contains("CHOOSE AN ENEMY"),"Focus selection has its own prominent prompt");
            Capture("04-targeting");
            _fight.TryOrder(Directive.FocusFire,"missing-target",true,out _);OrderRejected();RefreshFight();
            Check(_fight.LastRejection.Order==Directive.FocusFire,"campaign retains specific Focus refusal");
            string beforeCancel=CampaignSave.Write(_campaign);HandleUiKey(KeyCode.Escape);
            Check(!_targeting&&CampaignSave.Write(_campaign)==beforeCancel,"Escape cancels targeting without spending");
            Check(Click("order-2"),"Focus rearms");yield return null;
            var foe=ViewSim.Units.First(u=>u.Side==1&&u.Alive&&!u.Benched);
            Check(Click("target-"+foe.Id),"fighter hitbox remains reachable beneath HUD");RefreshFight();DrawBattle();yield return null;
            Check(Time.unscaledTime>=_holdRejectedUntil,"accepted Focus clears stale rejection");
            Check(ViewSim.FocusedId(0)==foe.Id,"selected target reaches authoritative Focus API");
            Check(_pxBanner.key.ToUpperInvariant().Contains("FOCUS"),"latest feedback acknowledges Focus rather than coaching Hold");
            // The same controls must still receive pointer input when the fixed canvas is scaled down.
            _canvas.scaleFactor=.8f;
            Check(Click("fight-read"),"Details receives pointer at smaller canvas scale");
            yield return null;_canvas.scaleFactor=.8f;
            Check(Click("modal-close"),"scaled Details closes");_canvas.scaleFactor=1;yield return null;
            Capture("05-focused");CheckCleanTextBounds();
            GateRestore(early);yield return null;_paused=false;
            Check(Click("order-0"),"early Hold pointer accepted");_paused=true;HudTicks(90);
            Check(HasUnmitigatedHoldHit(ViewSim,LatestHold()),"existing full-hit back-row case remains reproducible");
            Check(_pxBanner.key.Contains("HIT LANDED IN FULL"),"full-hit result stays honest");Capture("06-early-hold");
            GateRestore(early);yield return null;guard=0;
            while(!CurrentThreat.ParryWindow&&guard++<900)HudTicks(1);
            string beat=CampaignSave.Write(_campaign);
            _paused=false;Check(Click("order-0"),"timed live Hold accepted");_paused=true;HudTicks(30);
            Check(LatestHold().Effects.Any(e=>e.Kind==OrderEffectKind.Parried),"timed command actually parries");
            Check(_pxBanner.key.StartsWith("PARRIED"),"actual parry is shown");Capture("07-parried");
            if(ViewSim.CanUse(0,Directive.Unleash,out _))
            {
                Check(Click("order-3"),"Unleash follow-up accepted");RefreshFight();
                Check(_pxBanner.key.ToUpperInvariant().Contains("UNLEASH"),"latest Unleash feedback replaces older Hold result");
                Check(LatestHold().Effects.Any(e=>e.Kind==OrderEffectKind.Parried),"previous parry remains in order history");
            }
            GateRestore(beat);yield return null;Check(Click("order-0"),"paused Hold accepted");RefreshFight();
            Check(_pxBanner.key.Contains("WHILE PAUSED"),"paused Hold warning survives cleanup");
            HudTicks(30);Check(!LatestHold().Effects.Any(e=>e.Kind==OrderEffectKind.Parried),"paused Hold is never reported as parry");
            guard=0;while(ViewSim.CanUse(0,Directive.Hold,out _)&&guard++<20)Order(Directive.Hold);
            string exhausted=CampaignSave.Write(_campaign);
            Check(Click("order-0"),"unavailable Hold accepts click for explanation");RefreshFight();
            Check(_pxBanner.key.StartsWith("HOLD NOT SENT"),"rejection is prominent");
            Check(CampaignSave.Write(_campaign)==exhausted,"rejected order spends no points");Capture("08-unavailable");
            Order(Directive.Unleash);RefreshFight();
            Check(_fight.LastRejection.Order==Directive.Unleash&&_pxBanner.key.StartsWith("UNLEASH NOT SENT"),"generic rejection names authoritative refused order");
            Check(CampaignSave.Write(_campaign)==exhausted,"generic refusal spends no points");
            yield return new WaitForSecondsRealtime(4.1f);RefreshFight();
            Check(!_pxBanner.key.Contains("NOT SENT"),"rejection expires while paused");
            GateRestore(start);yield return null;
            Check(!_pxBanner.key.Contains("NOT SENT")&&!_pxBanner.key.StartsWith("PARRIED"),"reload clears old outcomes");

            GateRestore(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","chief.txt")));yield return null;
            Check(!ViewSim.SquadFirstOn,"unfinished rules 8 chief retains historical rules");
            Check(CleanCombatHud,"chief shares cleaned Bloodridge UI");
            Check(Click("chief-moves"),"chief Moves is clickable");yield return null;
            Check(Click("modal-close"),"chief Moves closes");yield return null;
            // Real existing rules, no synthetic stats: early Holds drain nerve over this fixed fight.
            guard=0;bool low=false,shiftSeen=false;
            while(!ViewOver&&!ViewSim.AwaitingVerdict&&guard++<1600)
            {
                if(ViewSim.CanUse(0,Directive.Hold,out _)&&CurrentThreat.BeatTicks>FightRules.PerfectHoldTicks)
                {_paused=false;Order(Directive.Hold);_paused=true;}
                HudTicks(1);
                if(!shiftSeen&&ShiftingBeat(ViewSim)&&!CurrentThreat.IsNone)
                {
                    _paused=false;DrawBattle();RefreshFight();
                    Check(!_threatAnswers.key.Contains("HOLD NOW")&&(_threatAnswers.key.Contains("FAKE FIRST")||_threatAnswers.key.Contains("WAITS OUT")),"learned counters never receive a guaranteed timing command");
                    Capture("09-chief-shifting");_paused=true;shiftSeen=true;
                }
                if(!low&&ViewSim.Nerve(0)>0&&ViewSim.Nerve(0)<=NerveRules.Full/4)
                {
                    Check(_nerveLabel[0].key.Contains("NEAR BREAK"),"actual low nerve has textual danger warning");
                    Capture("10-low-nerve");low=true;
                }
                if(guard%60==0)yield return null;
            }
            Check(low,"real chief encounter reaches visible near-break state");
            Check(ViewOver||ViewSim.AwaitingVerdict,"chief encounter settles under unchanged rules");
            yield return null;Capture("11-chief-end");
            var old=TacticsFixture(0,false);WatchFight(old);yield return null;SeekPlayback(220);yield return null;
            Check(!CleanCombatHud&&!ViewSim.ParryCenterOn,"legacy playback retains its original presentation branch");
            Check(!_beatTrack.enabled,"historical fight gets no new beat meter");
            LeavePlayback();CloseModal();yield return null;
            Log("HUD validation uses actual player rendering and pointer/key handler dispatch. Native input and human enjoyment are not established.");
        }
    }
}
