using KindredSiege.Sim.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        RectTransform _meterScreen, _meterPanel;
        readonly Image[] _meterBars = new Image[MeterRules.Bars];
        PxText _meterLabel, _meterBonus;

        void DrawParryMeter(BattleSimulation sim)
        {
            if(_meterScreen!=_screen)
            {
                _meterScreen=_screen;
                _meterPanel=Rect(_screen,"Parry meter",330*Px,4*Px,144*Px,32*Px);
                for(int i=0;i<_meterBars.Length;i++)
                {
                    int x=i*48;
                    var edge=Panel(_meterPanel,x*Px,0,43*Px,12*Px,PxMuted);
                    edge.raycastTarget=true;OnHover(edge.gameObject,MeterHint);
                    Panel(_meterPanel,(x+1)*Px,Px,41*Px,10*Px,Ink);
                    _meterBars[i]=Panel(_meterPanel,(x+2)*Px,2*Px,39*Px,8*Px,PxGold);
                }
                _meterLabel=PxLabel(_meterPanel,69,12,1);
                _meterBonus=PxLabel(_meterPanel,69,22,1);
            }
            _meterPanel.gameObject.SetActive(sim.MeterOn);
            if(!sim.MeterOn)return;
            int bars=sim.ParryBars(MeterRules.Side);
            for(int i=0;i<_meterBars.Length;i++)_meterBars[i].enabled=i<bars;
            SetPx(_meterLabel,"PARRY "+bars+"/"+MeterRules.Bars,bars==MeterRules.Bars?PxGold:PxBone);
            SetPx(_meterBonus,"COMBO +"+(bars*MeterRules.PercentPerBar)+"%",bars>0?PxMint:PxMuted);
        }

        string MeterHint()
        {
            var sim=ViewSim;int bars=sim.ParryBars(MeterRules.Side);
            return bars+"/"+MeterRules.Bars+" parry bars / +"+(bars*MeterRules.PercentPerBar)+
                "% on Combo and Cash hits. A parry fills one; a miss loses none. Your chief spell spends all three, costs no order points, and remains once per fight.";
        }

        void MeterSpellPlate(BattleSimulation sim,bool usable,ref string effect,ref string cost,ref bool lit)
        {
            if(!sim.MeterOn)return;
            cost="";
            if(_fight.Assist==null||ViewOver||sim.AwaitingVerdict)return;
            if(sim.WindupTicks(0)>0){effect="CAST "+Seconds(sim.WindupTicks(0));return;}
            if(sim.WallTicks(0)>0){effect="WALL "+Seconds(sim.WallTicks(0));return;}
            if(_fight.AssistUsed)return;
            int bars=sim.ParryBars(MeterRules.Side);
            if(bars<MeterRules.Bars&&!sim.CanUse(0,Directive.Eclipse,out string why)&&why.Contains("parry bars"))effect=bars+"/"+MeterRules.Bars+" BARS";
            else if(usable){effect="READY";lit=true;}
        }
    }
}
