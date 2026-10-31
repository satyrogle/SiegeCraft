using System;
using System.Collections.Generic;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        const string ClearGuide="RULES 13 / YOUR SQUAD MAKES THE COMBOS\n"+
            "Road: combos, then defence, then boosts, then carried moves. Each step keeps the previous tools.\n"+
            "Enemy road fighters use ordinary attacks and Burn; their carried moves arrive on step 4.\n"+
            "Fighter icons: purple mesh = Snare; red drop = Bleed; orange break = Opening; blue shield = Guard.\n"+
            "Flame = Burn; lightning = boost; blue shield = defence; orange horns = Frenzy; blue snowflake = Frozen; gold crosshair = your Focus target. Select a fighter for full names and timers.\n"+
            "Gold stamp = your combo actually hit. Blue strip = your boost charging.\n"+
            "Cleave: HOLD when the large ring turns gold. Brand: FALL BACK before the red countdown ends. Focus a Ring caster to interrupt.\n"+
            "Fall Back stops your attacks while dodging. Hold is free. Space pauses; half speed gives you time to read.\n"+
            "Wins raise the materials multiplier up to x3; losing a life resets it.";
        static string RoadLesson(int stage) => stage==1?"1 / YOUR SQUAD MAKES COMBOS":stage==2?"2 / AUTOMATIC DEFENCE":stage==3?"3 / AUTOMATIC BOOSTS":"4 / CLEAVE AND BRAND";
        RectTransform _clearScreen;
        PxText _clearStamp;
        readonly Dictionary<string,RawImage[]> _clearUnits=new Dictionary<string,RawImage[]>();
        readonly List<Texture2D> _clearGlyphs=new List<Texture2D>();
        // Small HUD symbols, drawn in code. No character or background asset changes.
        static readonly string[] ClearMasks={
            "1010101/1111111/1010101/1111111/1010101/1111111/1010101",
            "0001000/0001000/0011100/0011100/0111110/0111110/0011100",
            "1110011/1001010/1001010/0100100/0100100/0011000/0001000",
            "1111111/1111111/1111111/0111110/0111110/0011100/0001000",
            "0001000/0011000/0011010/0111110/1111111/1110111/0111110",
            "0001100/0011000/0111110/0001100/0011000/0110000/0100000",
            "1111111/1001001/1011101/0101010/0100010/0010100/0001000",
            "1000001/1100011/0111110/0101010/0111110/0011100/0001000",
            "0001000/0111110/0101010/1110111/0101010/0111110/0001000",
            "1001001/0101010/0011100/1111111/0011100/0101010/1001001"};
        static Color32 ClearColour(int i)=>i==0?PxLilac:i==1?PxRed:i==2||i==4||i==7?PxOrange:i==5||i==8?PxGold:PxBlue;
        void EnsureClearHud()
        {
            if(_clearScreen==_screen)return;
            _clearScreen=_screen;_clearUnits.Clear();_clearGlyphs.Clear();
            _clearStamp=PxLabel(_readoutLayer,0,0,1);
            _clearStamp.root.anchoredPosition=new Vector2(360*Px,-58*Px);
            _clearStamp.root.localScale=Vector3.one*1.5f;
            foreach(string mask in ClearMasks)
            {
                var texture=new Texture2D(7,7,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};
                var pixels=new Color32[49];var rows=mask.Split('/');
                for(int y=0;y<7;y++)for(int x=0;x<7;x++)pixels[(6-y)*7+x]=rows[y][x]=='1'?new Color32(255,255,255,255):new Color32(0,0,0,0);
                texture.SetPixels32(pixels);texture.Apply();_screenTextures.Add(texture);_clearGlyphs.Add(texture);
            }
        }
        void DrawClearThreat(BattleSimulation sim,Threat beat)
        {
            EnsureClearHud();
            if(RoadHud)SetPx(_roadWarning,"",PxMuted);
            bool payoff=!ViewOver&&sim.CurrentTick-_roadPayoffTick<2*SimClock.TicksPerSecond;
            SetPx(_clearStamp,payoff?(_roadPayoff.Contains("FLARE")?"FLARE!":_roadPayoff.Contains("BOND")?"BOND FORMED!":"COMBO HIT!"):"",PxGold);
            var brand=(BattleUnit)null;
            foreach(var u in sim.Units)if(u.Side==0&&u.Alive&&u.BrandTicks>0&&(brand==null||u.BrandTicks<brand.BrandTicks))brand=u;
            var actor=!beat.IsNone?FindFigure(beat.ActorId):null;
            if(actor!=null&&_raidBeatRing.enabled)
            {
                int size=LiveParryCue(sim,beat)?60:72;
                Place(_raidBeatRing,Mathf.RoundToInt(actor.feet.x/Px)-size/2,Mathf.Max(145,Mathf.RoundToInt((actor.feet.y-actor.head)/Px)-size),size,size);
            }
            if(CurrentThreat.IsNone)
            {
                SetPx(_threatHeading,RoadLesson(sim.RoadStage),PxBone);
                SetPx(_threatTarget,"",PxBone);
                SetPx(_threatAnswers,"SQUAD FIGHTS AUTOMATICALLY / 3 FOCUS AN ENEMY",PxMuted);
            }
            else if(LiveParryCue(sim,beat))
                SetPx(_threatAnswers,"1 HOLD NOW / "+beat.Name.ToUpperInvariant(),PxGold);
            if(brand!=null&&!LiveParryCue(sim,beat))
                SetPx(_threatAnswers,Fit((_playback!=null?"REPLAY / BRAND: DODGE / ":"2 FALL BACK / BRAND ")+Seconds(brand.BrandTicks)+" / "+NameOf(brand.Id),566),PxRed);
            if(sim.FallBackTicksLeft(0)>0)SetPx(_threatAnswers,"DODGING / YOUR ATTACKS PAUSED / "+Seconds(sim.FallBackTicksLeft(0)),PxBlue);
            if(_paused&&_playback==null)SetPx(_threatAnswers,"PAUSED / SPACE TO RESUME",PxMuted);
            if(sim.SquadScreenOn)DrawSquadScreen(sim,beat);
        }
        void DrawClearUnit(Figure f,KitReadout kit)
        {
            EnsureClearHud();var sim=ViewSim;var u=f.unit;
            if(!_clearUnits.TryGetValue(u.Id,out var icons))
            {
                icons=new RawImage[10];_clearUnits.Add(u.Id,icons);
                for(int i=0;i<icons.Length;i++)
                {icons[i]=PxImage(f.readoutRoot,"Status icon "+i,_clearGlyphs[i],0,0);icons[i].raycastTarget=false;}
            }
            bool live=u.Alive&&!u.Fled&&!ViewOver&&!ChiefCutInVisible;
            bool boosted=_kitBoosts.TryGetValue(u.Id,out var boost)&&sim.CurrentTick-boost.Tick<SimClock.TicksPerSecond;
            bool defended=_roadDefences.TryGetValue(u.Id,out var defence)&&sim.CurrentTick-defence.Tick<SimClock.TicksPerSecond;
            int active=(u.SnareTicks>0?1:0)|(u.BleedTicks>0?2:0)|(u.OpeningTicks>0?4:0)|(sim.CoveredBy(u)!=null?8:0)|
                (u.BurnTicks>0?16:0)|(boosted?32:0)|(defended?64:0)|(u.Frenzied?128:0)|(u.Side==1&&sim.FocusedId(0)==u.Id?256:0)|(u.PanicTicks>0?512:0);
            int count=0;for(int i=0;i<icons.Length;i++)if((active&(1<<i))!=0)count++;
            int slot=0;
            for(int i=0;i<icons.Length;i++)
            {
                icons[i].enabled=live&&(active&(1<<i))!=0;icons[i].color=ClearColour(i);
                if(icons[i].enabled)Place(icons[i],slot++*10-count*5,14,7,7);
            }
            kit.cooldown.enabled=live&&(!sim.UnitNerveOn||u.Side==0)&&sim.BoostsAvailable;
            SetPx(kit.burn,"",PxOrange);SetPx(kit.boost,"",PxBlue);
            if(_roadUnits.TryGetValue(u.Id,out var road))
            {
                SetPx(road.defence,"",PxBlue);
                SetPx(road.state,live&&u.Frenzied?Seconds(u.RiseTicks):live&&u.PanicTicks>0?Seconds(u.PanicTicks):"",u.Frenzied?PxOrange:PxBlue);
            }
            // Only the urgent countdown keeps text at a fighter; regular status names live in Details.
            int centreY=-Mathf.Min(30,Mathf.RoundToInt(f.head/(2*Px)));
            if(kit.brand.y!=centreY-4){kit.brand.y=centreY-4;kit.brand.key="";}
            SetPx(kit.brand,live&&u.BrandTicks>0?Seconds(u.BrandTicks):"",PxBone);
            for(int i=0;i<12;i++)
            {
                float a=(i*30-90)*Mathf.Deg2Rad;
                Place(kit.ring[i],Mathf.RoundToInt(Mathf.Cos(a)*24),centreY+Mathf.RoundToInt(Mathf.Sin(a)*24),4,4);
            }
        }
    }
}
