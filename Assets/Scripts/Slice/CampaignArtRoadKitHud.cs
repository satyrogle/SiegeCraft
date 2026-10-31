using System;
using System.Collections.Generic;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        bool KitHud => (_fight!=null||_playback!=null)&&ViewSim.RoadKitOn;
        bool KitRun => _campaign!=null&&_campaign.Tuning.RulesCap>=Campaign.RoadKitRules&&
            (_campaign.Active==null||_campaign.Active.RulesVersion>=Campaign.RoadKitRules);
        const string KitStatus="FALL BACK: DODGE / KNOWN STEADY PIERCE BUG - DETAILS";
        const string KitGuide="RULES 12 / AUTOMATIC SQUAD KIT\n"+
            "Fall Back is intended to stop all attacks. AUDIT ISSUE: a Steady shot already aiming can still pierce during dodge. Sent to Claude; fix pending.\n"+
            "Boosts happen automatically when ready with a valid target. Blue strip = cooldown progress.\n"+
            "Warden: Shield Bash opens a target. Marksman: Steady makes the next aim unbreakable and piercing.\n"+
            "Skirmisher: Twin Cut leaves bleed. Trapper: Rearm restores a sprung trap. Medic: Stim readies a finisher when nobody needs urgent healing.\n"+
            "Burn: up to 3 stacks, 5 seconds since last hit. Aimed Shot flares the remaining Burn at once.\n"+
            "Road carriers: Cleave has a Hold beat; Focus the Ring carrier to interrupt; Fall Back dodges Ring or Brand.\n"+
            "Wins raise the materials multiplier up to x3. Losing a life resets it. Beast wins do not raise it.";
        sealed class KitReadout
        {
            public PxText burn, boost, brand;
            public Image cooldown;
            public readonly Image[] ring=new Image[12];
        }
        readonly Dictionary<string,KitReadout> _kitUnits=new Dictionary<string,KitReadout>();
        readonly Dictionary<string,BattleEvent> _kitBoosts=new Dictionary<string,BattleEvent>();
        readonly Dictionary<string,long> _kitFlares=new Dictionary<string,long>();
        static string BoostWord(string detail)
        {
            switch(detail)
            {
                case BoostRules.ShieldBash:return "SHIELD BASH";
                case BoostRules.Steady:return "STEADY";
                case BoostRules.TwinCut:return "TWIN CUT";
                case BoostRules.Rearm:return "REARM";
                case BoostRules.Stim:return "STIM";
                default:return DefenceWord(detail);
            }
        }
        void DrawKitThreat(BattleSimulation sim)
        {
            // Reuse the authoritative beat/cast presentation; it also keeps secondary casts visible.
            DrawRaidThreat(sim);
            var t=CurrentThreat;
            bool cast=false;foreach(var threat in _threatReads)cast|=threat.CastBar;
            var beat=Threat.None;
            foreach(var threat in _threatReads)if(!threat.CastBar&&threat.BeatTicks>=0){beat=threat;break;}
            var actor=!ViewOver&&!beat.IsNone?FindFigure(beat.ActorId):null;
            _raidBeatRing.enabled=actor!=null&&!ChiefCutInVisible;
            if(_raidBeatRing.enabled)
            {
                bool now=LiveParryCue(sim,beat);
                int size=now?36:48;
                Place(_raidBeatRing,Mathf.RoundToInt(actor.feet.x/Px)-size/2,
                    Mathf.Max(145,Mathf.RoundToInt((actor.feet.y-actor.head)/Px)-size),size,size);
                _raidBeatRing.color=now?PxGold:PxBone;
            }
            if(sim.ClearRoadOn){DrawClearThreat(sim,beat);return;}
            if(t.IsNone)SetPx(_threatHeading,"YOUR SQUAD FIGHTS AND COMBOS AUTOMATICALLY",PxBone);
            if(!cast)SetPx(_threatTarget,Fit(!beat.IsNone&&beat.ActorId!=t.ActorId?"ALSO: "+beat.Name+" / "+FighterLabel(beat.ActorId):sim.CurrentTick-_roadPayoffTick<2*SimClock.TicksPerSecond?_roadPayoff:
                t.IsNone?"BOOSTS ARE AUTOMATIC / SELECT A FIGHTER FOR DETAILS":FighterLabel(t.ActorId)+" TO "+FighterLabel(t.TargetId),566),PxGold);
            SetPx(_threatAnswers,Fit(_paused&&_playback==null?"PAUSED / SPACE TO RESUME":t.IsNone?
                "FOCUS A THREAT / UNLEASH AN OPENING / HOLD TO GUARD":ThreatAnswers(sim,t),566),PxBone);
            SetPx(_roadWarning,sim.FallBackTicksLeft(0)>0?"DODGING "+Seconds(sim.FallBackTicksLeft(0))+" / STEADY PIERCE BUG OPEN":KitStatus,PxLilac);
        }
        void DrawKitUnit(Figure f)
        {
            var u=f.unit;var sim=ViewSim;
            if(!_kitUnits.TryGetValue(u.Id,out var r))
            {
                r=new KitReadout();_kitUnits.Add(u.Id,r);
                r.cooldown=Panel(f.readoutRoot,-f.barWidth*Px/2,10*Px,f.barWidth*Px,Px,PxBlue);
                r.burn=PxLabel(f.readoutRoot,Mathf.RoundToInt(f.barWidth/2)+8,0);
                r.boost=PxLabel(f.readoutRoot,0,-30,1);
                r.brand=PxLabel(f.readoutRoot,0,-Mathf.RoundToInt(f.head/Px)-48,1);
                for(int i=0;i<12;i++)
                {
                    float a=(i*30-90)*Mathf.Deg2Rad;
                    r.ring[i]=Panel(f.readoutRoot,Mathf.Round(Mathf.Cos(a)*18)*Px,
                        (-f.head/Px-28+Mathf.Round(Mathf.Sin(a)*18))*Px,3*Px,3*Px,PxRed);
                }
            }
            bool live=u.Alive&&!u.Fled&&!ViewOver&&!ChiefCutInVisible;
            r.cooldown.enabled=live;
            r.cooldown.rectTransform.sizeDelta=new Vector2(Mathf.Round(f.barWidth*(1-Mathf.Clamp01(u.BoostTicks/(float)BoostRules.BoostCooldownTicks)))*Px,Px);
            bool flare=_kitFlares.TryGetValue(u.Id,out long flareTick)&&sim.CurrentTick-flareTick<SimClock.TicksPerSecond;
            SetPx(r.burn,sim.ClearRoadOn?"":!ViewOver&&!u.Fled&&flare?"FLARE":live&&u.BurnTicks>0?"BURN "+u.BurnStacks:"",flare?PxGold:PxOrange);
            SetPx(r.boost,sim.ClearRoadOn?"":live&&sim.FallBackTicksLeft(u.Side)>0?"DODGING":live&&_kitBoosts.TryGetValue(u.Id,out var e)&&sim.CurrentTick-e.Tick<SimClock.TicksPerSecond?BoostWord(e.Detail):"",PxBlue);
            // Brand is read from the marked fighter, not guessed from the current top threat.
            bool brand=live&&u.BrandTicks>0&&u.BrandById.Length>0;
            if(!sim.ClearRoadOn)SetPx(r.brand,brand?"BRAND "+Seconds(u.BrandTicks):"",PxRed);
            for(int i=0;i<12;i++)r.ring[i].enabled=brand&&u.BrandTicks>i*CarryRules.BrandTicks/12;
            if(sim.ClearRoadOn)DrawClearUnit(f,r);
        }
        string KitFighterHint(BattleUnit u)
        {
            if(!KitHud)return "";
            return (ViewSim.ClearRoadOn&&((ViewSim.UnitNerveOn&&u.Side!=0)||!ViewSim.BoostsAvailable)?" / BOOSTS NOT ACTIVE":" / AUTO BOOST "+(u.BoostTicks>0?Seconds(u.BoostTicks):"READY: NEEDS TARGET"))+
                (u.Steady?" / STEADY AIM":"")+(u.BurnTicks>0?" / BURN "+u.BurnStacks+" "+Seconds(u.BurnTicks):"")+
                (u.CarryMove!=ChiefMove.None?" / CARRIES "+u.CarryMove:"");
        }
    }
}
