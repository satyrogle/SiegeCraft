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
        // Geometric HUD receipts, not fighter animation or replacement effect art. Every line connects
        // the event's actual wearer and target; it never invents a setup partner or nearby victim.
        sealed class ButcherEffectReadout
        {
            public BattleEvent receipt;
            public readonly Image[] corners=new Image[8];
            public Image link;
            public PxText label;
        }
        sealed class ButcherUnitReadout
        {
            public PxText state;
            public Image held;
            public readonly Image[] marks=new Image[10];
        }
        readonly ButcherEffectReadout[] _butcherEffects=new ButcherEffectReadout[2];
        readonly Dictionary<string,ButcherUnitReadout> _butcherUnits=new Dictionary<string,ButcherUnitReadout>();
        readonly List<BattleEvent> _butcherReceipts=new List<BattleEvent>();
        readonly List<BattleEvent> _butcherOutcomes=new List<BattleEvent>();
        readonly HashSet<string> _butcherWearers=new HashSet<string>();
        RectTransform _butcherScreen;
        PxText _butcherFooterTitle,_butcherFooterDetail;
        bool _butcherFooterShown;
        BattleEvent _butcherFooterReceipt;
        BattleSimulation _butcherSim;
        long _butcherTick=-1;
        int _butcherEvents=-1;
        bool _butcherActive;
        bool _butcherControlsReady;
        const int ButcherReceiptTicks=30;

        static ClassBend ButcherBend(BattleSimulation sim,string id)
        {
            for(int side=0;side<2;side++)foreach(var spec in sim.Setup.Side(side))
                if(spec.Id==id)
                {
                    if(ClassSpells.IsFor(spec.Bend,spec.Class))return spec.Bend;
                    if(sim.FloorOn&&ClassSpells.IsFor(spec.Bend2,spec.Class))return spec.Bend2;
                }
            return ClassBend.None;
        }

        static string ButcherStateText(BattleSimulation sim,BattleUnit unit)
        {
            if(unit==null||!unit.Alive||unit.Fled||unit.Benched)return "";
            if(unit.EvacTicks>0)return "OUT OF REACH "+Seconds(unit.EvacTicks);
            if(unit.FlurryTicks>0)return "FROZEN "+Seconds(unit.FlurryTicks)+" / STORED "+unit.FlurryStore;
            if(unit.PinTicks>0)return "HELD "+Seconds(unit.PinTicks)+(unit.SlowTicks>0?" / SLOW":"");
            if(unit.SlowTicks>0)return "SLOWED "+Seconds(unit.SlowTicks);
            if(ButcherHasBend(sim,unit.Id,ClassBend.StoppedBullets))
                return "SHOTS "+unit.ComboCount+" / "+ClassSpells.StoppedShots;
            return "";
        }

        static string ButcherReceiptText(BattleSimulation sim,BattleEvent e)
        {
            if(e.Kind==BattleEventKind.Burst)
                return e.Detail==MarkRules.Crush?"CRUSH BURST / HIT + PIN":e.Detail==MarkRules.Exposed?
                    "EXPOSED BURST / NEXT BLOW WIDE OPEN":"STASIS BURST / FREEZE "+Seconds(e.Amount)+" + WIND-UP RESET";
            if(e.Kind==BattleEventKind.Cash&&e.Detail==MarkRules.WideOpenDetail)return "WIDE OPEN / +"+e.Amount+" DAMAGE";
            if(e.Kind==BattleEventKind.Pull)
                return e.Detail+(e.Detail==ClassSpells.Name(ClassBend.MercyWell)?" / "+e.Amount+" ALLIES":
                    e.Detail==ClassSpells.Name(ClassBend.Collapse)?" / PIN":" / PULL "+e.Amount);
            if(e.Kind==BattleEventKind.Portal)
                return e.Detail+(e.Detail==ClassSpells.Name(ClassBend.Evac)?" / SAVED":
                    e.Detail==ClassSpells.Name(ClassBend.AroundTheCorner)?" / AIM":"");
            if(e.Kind==BattleEventKind.Freeze)
            {
                if(e.Detail==ClassSpells.Name(ClassBend.StoppedBullets))return e.Detail+" / SHOTS HELD "+e.Amount;
                if(e.Detail==ClassSpells.Name(ClassBend.Rewind))return e.Detail; // Amount is intended healing, never a timer.
                return e.Detail+(e.Detail==ClassSpells.Name(ClassBend.TimeSnare)?" / SLOW ":" / FREEZE ")+Seconds(e.Amount);
            }
            if(e.Kind==BattleEventKind.Cash&&e.Detail==ClassSpells.MercyDetail)return "MERCY WELL / BLOOD-FED TREATMENT";
            if(ButcherFlurryCash(sim,e))
                return "FROZEN FLURRY / OPENING CASHED";
            if(e.Kind==BattleEventKind.Heal&&e.Detail==ClassSpells.RewindDetail)return "REWIND / +"+e.Amount+" HP";
            if(e.Kind==BattleEventKind.Heal&&e.Detail==ClassSpells.MercyDetail)return "MERCY WELL / +"+e.Amount+" HP";
            if(e.Kind==BattleEventKind.Hit&&e.Detail==ClassSpells.FlurryDetail)return "FROZEN FLURRY / "+e.Amount+" HIT";
            return "";
        }

        static bool ButcherFlurryCash(BattleSimulation sim,BattleEvent cash)
        {
            if(cash.Kind!=BattleEventKind.Cash||cash.Detail!=ClassSpells.OpeningMark)return false;
            // Distinguish the actual thaw from this wearer's ordinary Opening strike on another turn.
            for(int i=sim.Events.Count-1;i>=0;i--)
            {
                var hit=sim.Events[i];if(hit.Tick<cash.Tick)break;
                if(hit.Tick==cash.Tick&&hit.Kind==BattleEventKind.Hit&&hit.Detail==ClassSpells.FlurryDetail&&
                    hit.ActorId==cash.ActorId&&hit.TargetId==cash.TargetId)return true;
            }
            return false;
        }

        static Color32 ButcherColour(BattleSimulation sim,BattleEvent e)
        {
            if(e.Kind==BattleEventKind.Burst)return e.Detail==MarkRules.Crush?PxLilac:e.Detail==MarkRules.Exposed?PxBlue:PxGold;
            if(e.Kind==BattleEventKind.Cash&&e.Detail==MarkRules.WideOpenDetail)return PxBlue;
            if(e.Kind==BattleEventKind.Heal||e.Kind==BattleEventKind.Cash&&e.Detail==ClassSpells.MercyDetail)return PxMint;
            if(e.Kind==BattleEventKind.Pull)return PxLilac;
            if(e.Kind==BattleEventKind.Portal)return PxBlue;
            return PxGold;
        }

        void EnsureButcherEffects(BattleSimulation sim)
        {
            if(_butcherScreen==_screen&&_butcherSim==sim)return;
            bool newScreen=_butcherScreen!=_screen;
            _butcherScreen=_screen;_butcherSim=sim;_butcherTick=-1;_butcherEvents=-1;_butcherActive=false;
            _butcherReceipts.Clear();_butcherOutcomes.Clear();_butcherWearers.Clear();
            foreach(var unit in sim.Units)if(ButcherHasCombo(sim,unit)){_butcherActive=true;break;}
            if(newScreen){_butcherUnits.Clear();_butcherControlsReady=false;_butcherFooterShown=false;}
            if(!_butcherActive)return;
            if(!_butcherControlsReady)
            {
                _butcherControlsReady=true;
                // Reuse the established feedback positions. The original labels remain intact and
                // are restored whenever an order, threat or hover needs the existing feedback area.
                _butcherFooterTitle=PxLabel(_pxBanner.root.parent,_pxBanner.x,_pxBanner.y,_pxBanner.align);
                _butcherFooterDetail=PxLabel(_pxBanner2.root.parent,_pxBanner2.x,_pxBanner2.y,_pxBanner2.align);
                _butcherFooterTitle.root.name="Butcher latest confirmed effect";
                _butcherFooterDetail.root.name="Butcher actual wearer and target";
                _butcherFooterTitle.root.gameObject.SetActive(false);
                _butcherFooterDetail.root.gameObject.SetActive(false);
                for(int i=0;i<_butcherEffects.Length;i++)
                {
                    var read=new ButcherEffectReadout();_butcherEffects[i]=read;
                    read.link=Panel(_readoutLayer,0,0,1,1,PxLilac);read.link.name="Butcher actual wearer to target";
                    for(int j=0;j<read.corners.Length;j++)
                    {read.corners[j]=Panel(_readoutLayer,0,0,1,1,PxLilac);read.corners[j].name="Butcher receipt bracket "+j;}
                    read.label=PxLabel(_readoutLayer,0,0,1);
                }
                foreach(var figure in _figures)
                {
                    var read=new ButcherUnitReadout();
                    read.held=Panel(figure.readoutRoot,0,0,4*Px,4*Px,PxLilac);
                    read.held.name="Butcher held or slowed state";
                    read.state=PxLabel(figure.readoutRoot,0,29,1);
                    for(int m=0;m<read.marks.Length;m++)
                    {
                        read.marks[m]=Panel(figure.readoutRoot,0,0,4,4,PxMuted);
                        read.marks[m].name="Butcher mark stack "+m;
                    }
                    _butcherUnits.Add(figure.unit.Id,read);
                }
            }
        }

        void ReadButcherReceipts(BattleSimulation sim)
        {
            if(_butcherTick==sim.CurrentTick&&_butcherEvents==sim.Events.Count)return;
            _butcherTick=sim.CurrentTick;_butcherEvents=sim.Events.Count;
            _butcherReceipts.Clear();_butcherOutcomes.Clear();_butcherWearers.Clear();
            // A seek's sim contains only events up to that moment. Rebuild from it instead of reviving
            // a view-local queue, and use simulation time so pause freezes the receipt too.
            for(int i=sim.Events.Count-1;i>=0;i--)
            {
                var e=sim.Events[i];long age=sim.CurrentTick-e.Tick;
                if(age>=ButcherReceiptTicks)break;
                if(age<0)continue;
                bool primary=e.Kind==BattleEventKind.Pull||e.Kind==BattleEventKind.Portal||e.Kind==BattleEventKind.Freeze||e.Kind==BattleEventKind.Burst;
                if(primary)
                {
                    if(_butcherWearers.Add(e.ActorId))_butcherReceipts.Add(e);
                }
                else if(ButcherReceiptText(sim,e).Length>0)_butcherOutcomes.Add(e);
            }
            foreach(var e in _butcherOutcomes)if(_butcherWearers.Add(e.ActorId))_butcherReceipts.Add(e);
            // Most recent wearer first; a tie keeps the event order from the backward read.
            for(int i=1;i<_butcherReceipts.Count;i++)
            {
                var e=_butcherReceipts[i];int j=i;
                while(j>0&&_butcherReceipts[j-1].Tick<e.Tick){_butcherReceipts[j]=_butcherReceipts[j-1];j--;}
                _butcherReceipts[j]=e;
            }
        }

        void DrawButcherEffects(BattleSimulation sim)
        {
            EnsureButcherEffects(sim);
            if(!_butcherActive){HideButcherEffects();return;}
            ReadButcherReceipts(sim);
            bool visible=!ViewOver&&!sim.AwaitingVerdict&&!ChiefCutInVisible;
            foreach(var figure in _figures)
            {
                if(!_butcherUnits.TryGetValue(figure.unit.Id,out var read))continue;
                string state=visible?ButcherStateText(sim,figure.unit):"";
                read.held.enabled=state.Length>0;
                var colour=figure.unit.EvacTicks>0?PxBlue:figure.unit.SlowTicks>0||figure.unit.FlurryTicks>0||
                    ButcherHasBend(sim,figure.unit.Id,ClassBend.StoppedBullets)?PxGold:PxLilac;
                read.held.color=colour;
                // A compact status square belongs to this exact HP bar. Full timers and stored
                // amounts stay in its fighter hint, rather than running down into the next row.
                Place(read.held,Mathf.RoundToInt(figure.barWidth/2)+4,0,4,4);
                SetPx(read.state,"",colour);
                for(int m=0;m<read.marks.Length;m++)
                {
                    int row=m/3,col=m%3;var kind=row==0?StatusKind.Crush:row==1?StatusKind.Exposed:row==2?StatusKind.Stasis:StatusKind.WideOpen;
                    var status=figure.unit.Statuses[kind];var pip=read.marks[m];
                    pip.enabled=visible&&sim.MarksOn&&figure.unit.Alive&&!figure.unit.Fled&&!figure.unit.Benched&&status.On;
                    pip.color=col<status.Stacks?(row==0?PxLilac:row==1||row==3?PxBlue:PxGold):PxMuted;
                    Place(pip,Mathf.RoundToInt(figure.barWidth/2)+10+col*5,row*6,4,4);
                }
            }
            for(int i=0;i<_butcherEffects.Length;i++)
            {
                var read=_butcherEffects[i];bool show=visible&&i<_butcherReceipts.Count;
                read.receipt=show?_butcherReceipts[i]:default;
                var actor=show?FindFigure(read.receipt.ActorId):null;
                var target=show?FindFigure(read.receipt.TargetId):null;
                show&=target!=null;
                foreach(var corner in read.corners)corner.enabled=show;
                read.link.enabled=false;
                if(!show){SetPx(read.label,"",PxBone);continue;}
                var colour=ButcherColour(sim,read.receipt);var body=Body(target);
                int radius=23+Mathf.RoundToInt(4*(1-Mathf.Clamp01((sim.CurrentTick-read.receipt.Tick)/(float)ButcherReceiptTicks)));
                for(int j=0;j<4;j++)
                {
                    bool right=(j&1)!=0,bottom=(j&2)!=0;
                    int x=Mathf.RoundToInt(body.x)+(right?radius:-radius),y=Mathf.RoundToInt(body.y)+(bottom?22:-22);
                    Place(read.corners[j*2],right?x-8:x,y,8,2);
                    Place(read.corners[j*2+1],x,bottom?y-6:y,2,8);
                    read.corners[j*2].color=read.corners[j*2+1].color=colour;
                }
                Trace(read.link,actor,target,1,new Color(colour.r/255f,colour.g/255f,colour.b/255f,.45f),1);
                SetPx(read.label,"",colour);
            }
            DrawButcherFooter(sim);
        }

        bool ButcherCanShowFooter(BattleSimulation sim)
        {
            ReadClarity(sim);
            if(ViewOver||sim.AwaitingVerdict||ChiefCutInVisible||!CurrentThreat.IsNone||_targeting||
                _hover.Length>0||_notice.Length>0||_modal!=null||_coachStep>=0||Time.unscaledTime<_holdRejectedUntil)return false;
            if(_playback!=null&&_playback.Failure.Length>0)return false;
            return !OrderReceiptPriority(sim);
        }

        bool OrderReceiptPriority(BattleSimulation sim)
        {
            ReadClarity(sim);
            // A running order or its recent outcome takes priority. This is display priority,
            // not a new expiry for the actual order or its recorded effect.
            if(_orderReads.Count>0)
            {
                var last=_orderReads[_orderReads.Count-1];
                long latest=Math.Max(last.Tick,last.EndTick);
                foreach(var effect in last.Effects)latest=Math.Max(latest,effect.Tick);
                if(!last.Final||last.Running||sim.CurrentTick-latest<2*SimClock.TicksPerSecond||
                    (last.Order==Directive.Hold&&sim.NextCombo(0).Combo==FightRules.Riposte))return true;
            }
            return false;
        }

        void DrawButcherFooter(BattleSimulation sim)
        {
            ButcherEffectReadout newest=null;
            foreach(var read in _butcherEffects)
                if(read!=null&&read.corners[0].enabled){newest=read;break;}
            bool show=newest!=null&&ButcherCanShowFooter(sim);
            _butcherFooterReceipt=show?newest.receipt:default;
            if(_butcherFooterShown&&!show)
            {
                _pxBanner.root.gameObject.SetActive(true);_pxBanner2.root.gameObject.SetActive(true);
            }
            _butcherFooterShown=show;
            _butcherFooterTitle.root.gameObject.SetActive(show);
            _butcherFooterDetail.root.gameObject.SetActive(show);
            if(!show)return;
            _pxBanner.root.gameObject.SetActive(false);_pxBanner2.root.gameObject.SetActive(false);
            var colour=ButcherColour(sim,newest.receipt);
            SetPx(_butcherFooterTitle,Fit(ButcherReceiptText(sim,newest.receipt),660),colour);
            SetPx(_butcherFooterDetail,Fit(NameOf(newest.receipt.ActorId)+" > "+NameOf(newest.receipt.TargetId)+
                (_playback!=null?" / RECORDED":""),660),PxBone);
        }

        void HideButcherEffects()
        {
            if(_butcherFooterShown)
            {
                if(_pxBanner!=null&&_pxBanner.root!=null)_pxBanner.root.gameObject.SetActive(true);
                if(_pxBanner2!=null&&_pxBanner2.root!=null)_pxBanner2.root.gameObject.SetActive(true);
            }
            _butcherFooterShown=false;_butcherFooterReceipt=default;
            if(_butcherFooterTitle!=null&&_butcherFooterTitle.root!=null)_butcherFooterTitle.root.gameObject.SetActive(false);
            if(_butcherFooterDetail!=null&&_butcherFooterDetail.root!=null)_butcherFooterDetail.root.gameObject.SetActive(false);
            foreach(var read in _butcherEffects)
            {
                if(read==null)continue;read.receipt=default;
                if(read.link!=null)read.link.enabled=false;
                foreach(var corner in read.corners)if(corner!=null)corner.enabled=false;
                if(read.label!=null&&read.label.root!=null)SetPx(read.label,"",PxBone);
            }
            foreach(var read in _butcherUnits.Values)
            {
                if(read.held!=null)read.held.enabled=false;
                if(read.state!=null&&read.state.root!=null)SetPx(read.state,"",PxBone);
                foreach(var pip in read.marks)if(pip!=null)pip.enabled=false;
            }
        }

        string ButcherFighterHint(BattleSimulation sim,BattleUnit unit)
        {
            string state=ButcherStateText(sim,unit);var bend=ButcherBend(sim,unit.Id);
            if(ButcherHasBend(sim,unit.Id,ClassBend.Rewind)&&unit.ComboTicks>0)state+=(state.Length>0?" / ":"")+"NEXT REWIND "+Seconds(unit.ComboTicks);
            if(ButcherHasBend(sim,unit.Id,ClassBend.StillWall))state+=(state.Length>0?" / ":"")+(unit.ComboTicks>0?"WALL RECOVERS "+Seconds(unit.ComboTicks):"STILL WALL READY");
            if(ButcherHasBend(sim,unit.Id,ClassBend.Evac))state+=(state.Length>0?" / ":"")+(unit.EvacSpent?"EVAC USED":"EVAC READY");
            if(sim.MarksOn)
            {
                if(ButcherHasCombo(sim,unit))foreach(var spec in sim.Setup.Side(unit.Side))
                    if(spec.Id==unit.Id)
                    {
                        if(ClassSpells.IsCombo(spec.Bend))state+=(state.Length>0?" / ":"")+ClassSpells.Name(spec.Bend)+" RANK "+spec.ComboRank;
                        if(sim.FloorOn&&ClassSpells.IsCombo(spec.Bend2))state+=(state.Length>0?" / ":"")+"SLOT 2 "+ClassSpells.Name(spec.Bend2)+" RANK "+spec.ComboRank2;
                        break;
                    }
                for(var kind=StatusKind.Crush;kind<=StatusKind.WideOpen;kind++)
                {
                    var mark=unit.Statuses[kind];if(!mark.On)continue;
                    state+=(state.Length>0?" / ":"")+(kind==StatusKind.WideOpen?"WIDE OPEN":MarkRules.Name(kind)+" "+mark.Stacks+"/"+MarkRules.MaxStacks)+" "+Seconds(mark.Ticks);
                }
            }
            return state.Length>0?"   "+state:"";
        }
    }
}
