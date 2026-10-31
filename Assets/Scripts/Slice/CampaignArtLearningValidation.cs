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
        IEnumerator ValidateLearningCombat()
        {
            yield return null;
            _savePath=Path.Combine(_capture,"learning-save.txt");_gatesReview=true;
            NewReviewRoute();yield return null;
            Check(Click("tribe-"+CampaignTribes.BloodridgeId),"learning route opens Bloodridge");yield return null;
            Check(Click("offer-0"),"learning route picks first offer");yield return null;
            _auto=false;
            Check(Click("start-fight"),"manual fight starts through button");
            _auto=true;_pilotOn=false;_paused=true;
            Check(_speed==.5f,"manual fight defaults to half speed");
            string start=CampaignSave.Write(_campaign);
            yield return null;
            foreach(float expected in new[]{.25f,1f,2f,4f,.5f})
            {
                Check(Click("speed"),"speed button is reachable");RefreshFight();
                Check(_speed==expected,"speed cycle reaches "+expected);
                Check(CampaignSave.Write(_campaign)==start,"changing speed never alters authoritative state");
            }
            // Measure actual Update scheduling at each learning pace, not just the stored setting.
            foreach(float pace in new[]{.25f,.5f,1f})
            {
                GateRestore(start);_speed=pace;_auto=false;
                yield return null;yield return null;
                _paused=false;long tick=ViewSim.CurrentTick;float began=Time.unscaledTime,eligible=0;
                while(Time.unscaledTime-began<1f){yield return null;eligible+=Mathf.Min(Time.unscaledDeltaTime,.1f);}
                _paused=true;_auto=true;
                float elapsed=Time.unscaledTime-began;
                long advanced=ViewSim.CurrentTick-tick;
                Check(Math.Abs(advanced-eligible*pace*SimClock.TicksPerSecond)<2,"runtime ticks match elapsed frame time at "+pace+"x: "+advanced+" ticks / "+elapsed+"s (eligible "+eligible+"s; existing 0.1s frame cap)");
            }
            GateRestore(start);_speed=.5f;
            int guard=0;
            while((CurrentThreat.IsNone||CurrentThreat.BeatTicks<=FightRules.PerfectHoldTicks+2)&&!ViewOver&&guard++<2000)GateTicks(1);
            Check(!CurrentThreat.IsNone&&CurrentThreat.BeatTicks>FightRules.PerfectHoldTicks+2,"real road fight exposes an early windup");
            string early=CampaignSave.Write(_campaign);Capture("01-wait");
            _paused=false;Check(Click("order-0"),"early live Hold pointer press accepted");_paused=true;RefreshFight();
            Check(_pxBanner.key.Contains("HOLD ACCEPTED"),"immediate Hold acknowledgment is visible");Capture("02-accepted");
            GateTicks(90);var blocked=LatestHold();
            Check(ViewSim.SquadFirstOn&&!blocked.Effects.Any(e=>e.Kind==OrderEffectKind.StaleBlock),"rules 9 early Hold carries no old block nerve penalty");
            Check(blocked.Effects.Any(e=>e.Kind==OrderEffectKind.OutOfReach),"back-row hit has authoritative out-of-reach receipt");
            Check(_pxBanner.key.Contains("HIT LANDED IN FULL")&&_pxBanner2.key.Contains("OUTSIDE YOUR GUARD"),"unmitigated early Hold is explained from its authoritative receipt");Capture("03-too-early");
            GateRestore(early);_speed=.5f;
            guard=0;while(!CurrentThreat.ParryWindow&&!ViewOver&&guard++<1000)GateTicks(1);
            string beat=CampaignSave.Write(_campaign);
            File.WriteAllText(Path.Combine(_capture,"beat-save.txt"),beat);
            _paused=false;RefreshFight();DrawBattle();Capture("04-hold-now");_paused=true;
            Check(_threatAnswers.key.Contains("SMALL BEAT"),"small beat has optional guard cue instead of compulsory parry coaching");
            Check(Click("order-0"),"paused Hold accepted through pointer");RefreshFight();
            Check(_pxBanner.key.Contains("WHILE PAUSED")&&_pxBanner2.key.Contains("FRESH HOLD"),"paused Hold warning is immediate");Capture("05-paused-accepted");
            GateTicks(30);
            Check(LatestHold().Paused&&!LatestHold().Effects.Any(e=>e.Kind==OrderEffectKind.Parried),"paused Hold cannot become a parry");
            Check(!_pxBanner.key.StartsWith("PARRIED"),"paused outcome is not mislabeled");
            GateRestore(beat);_speed=.5f;yield return null;_paused=false;
            Check(Click("order-0"),"live timed pointer Hold accepted");_paused=true;GateTicks(30);
            Check(LatestHold().Effects.Any(e=>e.Kind==OrderEffectKind.Parried),"live timed Hold truly parried in sim");
            Check(_pxBanner.key.StartsWith("PARRIED"),"persistent Hold banner reports actual parry");Capture("06-parried");
            if(ViewSim.CanUse(0,Directive.Unleash,out _))
            {
                Check(Click("order-3"),"follow-up order accepted");RefreshFight();
                Check(_pxBanner.key.StartsWith("UNLEASH")&&LatestHold().Effects.Any(e=>e.Kind==OrderEffectKind.Parried),"latest order replaces banner while Hold history survives");
            }
            string after=CampaignSave.Write(_campaign);
            GateRestore(after);yield return null;
            Check(LatestHold().Effects.Any(e=>e.Kind==OrderEffectKind.Parried),"reload reconstructs actual Hold history");
            GateRestore(beat);yield return null;
            Check(!_pxBanner.key.StartsWith("PARRIED"),"rewind clears stale Hold outcome");
            // Exhaust real points through accepted commands; the next click must explain rejection.
            guard=0;while(ViewSim.CanUse(0,Directive.Hold,out _)&&guard++<20){Order(Directive.Hold);RefreshFight();}
            Check(!ViewSim.CanUse(0,Directive.Hold,out string reason),"fixture runs out of points normally");
            string noPoints=CampaignSave.Write(_campaign);
            Check(Click("order-0"),"unavailable Hold still receives pointer input");RefreshFight();
            Check(_pxBanner.key.StartsWith(("HOLD NOT SENT: "+reason).ToUpperInvariant()),"rejected pointer press shows authoritative reason");
            Check(CampaignSave.Write(_campaign)==noPoints,"rejected press spends nothing and adds no command");Capture("07-unavailable");
            Order(Directive.Hold);RefreshFight();
            Check(_pxBanner.key.StartsWith(("HOLD NOT SENT: "+reason).ToUpperInvariant()),"keyboard command path reports same rejection");
            _speed=.25f;yield return new WaitForSecondsRealtime(4.1f);RefreshFight();
            Check(!_pxBanner.key.Contains("NOT SENT")&&_paused,"rejection notice expires in real time even while paused at quarter speed");
            Check(CampaignSave.Write(_campaign)==noPoints,"waiting on rejected feedback never advances paused campaign");
            GateRestore(early);RefreshFight();
            Check(!_pxBanner.key.Contains("NOT SENT"),"replacing battle clears rejected-input notice");
            _auto=false;_learningSpeed=.25f;BeginFight();_auto=true;_pilotOn=false;_paused=true;
            Check(_speed==.25f,"chosen learning speed survives entering another fight");
            // Separate from the back-row gap: this fixed chief Cleave really IS mitigated by an early Hold.
            GateRestore(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","chief.txt")));
            _speed=.5f;yield return null;guard=0;
            while(!(CurrentThreat.Name=="Cleave"&&CurrentThreat.BeatTicks==25)&&!ViewOver&&guard++<900)GateTicks(1);
            Check(CurrentThreat.Name=="Cleave"&&CurrentThreat.BeatTicks==25,"chief checkpoint reaches a reproducible early Cleave");
            _paused=false;Check(Click("order-0"),"early chief Hold sent through pointer");_paused=true;
            guard=0;while(!LatestHold().Effects.Any(e=>e.Kind==OrderEffectKind.Soaked)&&!ViewOver&&guard++<90)GateTicks(1);
            Check(LatestHold().Effects.Any(e=>e.Kind==OrderEffectKind.Soaked),"chief Cleave is actually soaked");
            Check(_pxBanner.key.Contains("BLOCKED")&&_pxBanner.key.Contains("DAMAGE REDUCED"),"mitigated early Hold says blocked, not parried");
            Capture("08-blocked-cleave");
            var old=TacticsFixture(0,false);WatchFight(old);yield return null;SeekPlayback(220);yield return null;
            Check(!ViewSim.ParryCenterOn&&!_beatTrack.enabled,"old replay does not acquire rules 8 timing meter");
            Check(!_threatAnswers.key.Contains("HOLD NOW"),"replay does not ask for live Hold input");
            LeavePlayback();CloseModal();yield return null;
            Log("Learning combat validation: actual player rendering and UI pointer dispatch; not native input or human comprehension proof.");
        }
    }
}

