using System;
using System.Globalization;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Presentation pace only. Every command and tick still goes through the same campaign API.
        float _learningSpeed=.5f;
        string _holdRejected="";
        float _holdRejectedUntil;
        RectTransform _learningThreatScreen;
        RawImage _beatTrack,_beatWindow,_beatCursor;

        string LearningSpeedLabel=>_speed.ToString("0.##",CultureInfo.InvariantCulture)+"X";

        void CycleLearningSpeed()
        {
            // The first click from the learning default slows down further, rather than speeding up.
            _speed=_speed==.5f?.25f:_speed<1?1:_speed<2?2:_speed<4?4:.5f;
            _learningSpeed=_speed;
        }

        void ResetHoldFeedback(){_holdRejected="";_holdRejectedUntil=0;_rejectedOrder=Directive.Hold;}
        void HoldAccepted(){ResetHoldFeedback();_receiptTick=-1;}
        Directive _rejectedOrder=Directive.Hold;
        void OrderRejected()
        {
            var rejected=_fight.LastRejection;
            if(rejected.IsNone)return;
            HoldRejected(rejected.Reason);_rejectedOrder=rejected.Order;
        }
        void HoldRejected(string reason)
        {
            _holdRejected=reason;_holdRejectedUntil=Time.unscaledTime+4;
            _notice="";
        }

        OrderResult LatestHold()
        {
            for(int i=_orderReads.Count-1;i>=0;i--)
                if(_orderReads[i].Order==Directive.Hold)return _orderReads[i];
            return OrderResult.None;
        }

        string RealBeatTime(Threat t)=>(Math.Max(0,t.BeatTicks-1)/(float)SimClock.TicksPerSecond/Mathf.Max(.25f,_speed)).ToString("0.0",CultureInfo.InvariantCulture)+"s";

        static bool HasUnmitigatedHoldHit(BattleSimulation sim,OrderResult hold)
        {
            return hold.Effects.Count>0&&hold.Effects[hold.Effects.Count-1].Kind==OrderEffectKind.OutOfReach;
        }

        string BeatTime(int ticks)=>(Math.Max(0,ticks-1)/(float)SimClock.TicksPerSecond/Mathf.Max(.25f,_speed)).ToString("0.0",CultureInfo.InvariantCulture)+"s";
        static string BeatCue(Threat t)
        {
            if(t.IsNone)return "YOUR SQUAD FIGHTS AUTOMATICALLY";
            if(t.FakeFirst)return "FAKE FIRST / WAIT FOR THE REAL ATTACK";
            if(t.WaitsOutHold)return "THIS MOVE WAITS OUT A FRESH HOLD";
            if(t.BeatTicks<0)return "MOVE ACTIVE / ANSWERS IN DETAILS";
            if(!t.Duel)return "SMALL BEAT / HOLD GUARDS YOUR FRONT ROW";
            return t.ParryWindow?"PARRY WINDOW / OR USE ANOTHER ANSWER":"BIG BEAT / WATCH THE WINDUP";
        }

        void DrawLearningThreat(BattleSimulation sim,Threat t)
        {
            if(_learningThreatScreen!=_screen)
            {
                _learningThreatScreen=_screen;
                int y=sim.BeastOn?161:102;
                _beatTrack=Raw(_threatPanel,"Time to impact",28*Px,(y+29)*Px,578*Px,3*Px);
                _beatTrack.color=new Color(.22f,.20f,.24f,1);
                _beatWindow=Raw(_threatPanel,"Final half second",28*Px,(y+29)*Px,144*Px,3*Px);
                _beatWindow.color=PxGold;
                _beatCursor=Raw(_threatPanel,"Impact cursor",28*Px,(y+28)*Px,3*Px,5*Px);
                _beatCursor.color=PxBone;
            }
            bool show=sim.ParryCenterOn&&t.BeatTicks>=0;
            _beatTrack.enabled=_beatWindow.enabled=_beatCursor.enabled=show;
            _beatWindow.color=t.FakeFirst||t.WaitsOutHold?PxMuted:PxGold;
            if(show)
            {
                // The final two simulation seconds, shrinking towards the gold parry zone.
                var pos=_beatCursor.rectTransform.anchoredPosition;
                pos.x=(28+Mathf.Clamp01(Math.Max(0,t.RealBeatTicks-1)/(2f*SimClock.TicksPerSecond))*575)*Px;
                _beatCursor.rectTransform.anchoredPosition=pos;
            }
            if(!sim.ParryCenterOn||_playback!=null)return;
            string cue=BeatCue(t);
            if(_paused)cue="PAUSED - SPACE TO RESUME";
            else if(!t.IsNone&&sim.HoldRefreshTicksLeft(0)>0)cue=HoldRecoveryCue(sim);
            if(!t.IsNone&&!sim.CanUse(0,Directive.Hold,out _))cue="HOLD UNAVAILABLE - CLICK FOR REASON";
            if(!t.IsNone&&(t.Answers&ThreatAnswer.Parry)==0)cue="WATCH THEIR MOVE";
            string beat=t.BeatTicks>=0?(_paused?" / "+Seconds(t.RealBeatTicks)+" fight time":(t.FakeFirst?" / REAL IN ":" / BEAT IN ")+BeatTime(t.RealBeatTicks)):"";
            SetPx(_threatHeading,Fit((t.IsNone?"NO BIG ATTACK INCOMING":t.Name)+beat,578),LiveParryCue(sim,t)?PxGold:PxBone);
            SetPx(_threatAnswers,Fit(cue,660),LiveParryCue(sim,t)?PxGold:PxMint);
            if(t.IsNone)SetPx(_threatTarget,"Fighters attack automatically. Save Hold for an incoming attack.",PxBone);
            // Other available answers, estimates and class help remain in DETAILS.
            if(_orderReads.Count==0)SetPx(_orderReceipt,_speed<1?"LEARNING "+LearningSpeedLabel+": slower time, same rules. Click speed above to change.":
                "SPACE pauses / 1 Hold / 2 Fall Back / 3 Focus / 4 Unleash",PxMuted);
            else
            {
                var last=_orderReads[_orderReads.Count-1];
                if(last.Order==Directive.Hold&&HasUnmitigatedHoldHit(sim,last))
                    SetPx(_orderReceipt,"HOLD: target out of reach / hit landed in full.",PxRed);
            }
        }

        void HoldFeedback(BattleSimulation sim,out string title,out string detail,out Color32 colour)
        {
            ReadClarity(sim);
            var hold=LatestHold();colour=PxBone;
            title=sim.SquadFirstOn?"YOUR SQUAD FIGHTS / CHOOSE AN ORDER":"HOLD: press 1 or click it when the attack reaches the gold zone.";
            detail=_paused?"PAUSED: Space resumes. A Hold ordered while paused cannot parry.":
                "Watch the incoming attack above. Hold is a timed order, not a held-down button.";
            if(!hold.IsNone)
            {
                title=hold.Paused?"HOLD ACCEPTED WHILE PAUSED - cannot parry":"HOLD ACCEPTED - shields raised";
                detail=hold.Paused?"Resume, then issue a fresh Hold at the beat to parry.":"Waiting for impact. This confirms the order, not a successful parry.";
                colour=PxBlue;
                if(hold.Effects.Count>0)
                {
                    var effect=hold.Effects[hold.Effects.Count-1];
                    switch(effect.Kind)
                    {
                        case OrderEffectKind.Parried:
                            title="PARRIED: "+effect.Move+" - attack negated!";
                            detail=sim.NextCombo(0).Combo==FightRules.Riposte?"RIPOSTE OPEN: press 4 to punish, or focus the exposed enemy.":
                                "Attack was negated and the enemy staggered. Watch for the next attack.";
                            colour=PxMint;break;
                        case OrderEffectKind.Soaked:
                            title="BLOCKED: "+effect.Move+" - damage reduced";
                            detail=sim.SquadFirstOn?"Your front row soaked the hit. No block nerve cost.":"Your front row soaked the hit. Details lists any block nerve cost.";
                            colour=PxOrange;break;
                        case OrderEffectKind.Shattered:
                            title="HOLD BROKEN: "+effect.Move;detail="Their attack broke your guard; it was not a parry.";colour=PxRed;break;
                        case OrderEffectKind.OutOfReach:
                            title="HOLD: HIT LANDED IN FULL";detail=effect.Move+" hit outside your guard / "+effect.Amount+" health taken.";colour=PxRed;break;
                        case OrderEffectKind.StaleBlock:
                            title="HOLD: BLOCK NERVE COST";detail=effect.Move+" / "+effect.Amount+" nerve lost under these historical rules.";colour=PxOrange;break;
                        case OrderEffectKind.Feinted:
                            title="HOLD FAKED: "+effect.Move;detail="Fake met your Hold. Real attack followed "+Seconds(effect.Amount)+" later.";colour=PxOrange;break;
                        case OrderEffectKind.WaitedOut:
                            title="HOLD WAITED OUT: "+effect.Move;detail="Their move waited "+Seconds(effect.Amount)+" for your guard to age.";colour=PxOrange;break;
                        default:
                            title="HOLD: "+effect.Kind+" "+effect.Move;
                            detail="Recorded result. This was not a parry.";break;
                    }
                }
                else if(hold.Final)
                {
                    title=hold.Miss==OrderMiss.TooEarly?"HOLD TOO EARLY: "+hold.MissMove:
                        hold.Miss==OrderMiss.TooLate?"HOLD TOO LATE: "+hold.MissMove:
                        hold.Miss==OrderMiss.NothingCame?"HOLD EXPIRED - no attack met it":"HOLD: "+hold.Miss;
                    detail=Receipt(hold);colour=PxOrange;
                }
                else if(!hold.Running)
                {title="HOLD ENDED - result still resolving";detail="Waiting for the simulation to settle the outcome.";}

            }
            if(Time.unscaledTime<_holdRejectedUntil)
            {
                detail=hold.IsNone?(sim.RaidChiefOn?"No Hold was sent. Hold costs no points.":"No Hold was sent. Save a point for the next incoming attack."):"Last accepted order: "+title;
                title=OrderName(_rejectedOrder)+" NOT SENT: "+_holdRejected;colour=PxRed;
            }
        }
    }
}
