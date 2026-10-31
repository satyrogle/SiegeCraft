using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // The tutorial walks you through one order at a time (Jacob, 11 Oct 2026: "tutorial doesn't actually
        // say it's a tutorial ... it just puts you right in the game"). Each step stops the fight, says the
        // order and lights its plate, and the fight carries on once you give it. Off in the automatic checks
        // unless a check asks for it.
        static readonly Directive[] CoachOrders={Directive.Hold,Directive.FocusFire,Directive.Unleash,Directive.FallBack};
        static readonly string[] CoachAsk={"Press 1 or click HOLD.","Press 3, then click an enemy or choose with Tab and Enter.","Press 4 or click UNLEASH.","Press 2 or click FALL BACK."};
        static readonly string[] CoachWhen={"Their raiders charge your front line.","Everyone who can reach hits that one target.",
            "Break their nerve and they run.","Use it when they wind up a big hit."};
        int _coachStep=-1;
        bool _coachShown,_coachTest;
        float _coachDoneAt=-100;
        RawImage _coachPlate;

        bool Coaching=>_fight!=null&&_fight.Encounter.Kind==EncounterKind.Tutorial&&(!_auto||_coachTest);

        void CoachBegin()
        {
            _coachStep=Coaching?0:-1;
            _coachShown=false;
            _coachDoneAt=-100;
        }

        /// The order the step asks for was given, by key or by plate: the fight carries on.
        void CoachHeard(BattleEvent e)
        {
            if(_coachStep<0||_coachStep>=CoachOrders.Length||e.Kind!=BattleEventKind.Directive||e.ActorSide!=0)return;
            if(e.Detail!=Directives.Name(CoachOrders[_coachStep]))return;
            if(_coachShown)_paused=false;
            _coachShown=false;
            _coachStep++;
            _coachDoneAt=_visualTime;
        }

        /// Called every frame before the banner is drawn: starts the next step when its order can be paid
        /// for and the last one has run its course, and lights the plate it asks for.
        void Coach()
        {
            for(int i=0;i<Orders.Length;i++)_buttons["order-"+i].targetGraphic.color=Color.white;
            if(_coachPlate!=null)_coachPlate.enabled=false;
            if(_coachStep<0||_fight.Over){_coachStep=-1;return;}
            var sim=_fight.Sim;
            if(_coachStep<CoachOrders.Length)
            {
                Directive d=CoachOrders[_coachStep];
                bool settled=_coachStep==0||(_visualTime-_coachDoneAt>1.5f&&sim.HoldTicksLeft(0)==0&&sim.FocusTicksLeft(0)==0&&
                    sim.UnleashTicksLeft(0)==0&&sim.FallBackTicksLeft(0)==0);
                if(!_coachShown&&settled&&sim.DirectivePoints(0)>=DisplayOrderCost(sim,d)){_coachShown=true;_paused=true;}
                if(!_coachShown)return;
                // The plate it asks for glows gold and pulses, paused or not.
                int plate=System.Array.IndexOf(Orders,d);
                float pulse=Mathf.PingPong(Time.unscaledTime*2f,1f);
                _buttons["order-"+plate].targetGraphic.color=Color.Lerp(new Color(1f,.86f,.5f),new Color(1f,.62f,.15f),pulse);
                if(_coachPlate!=null)_coachPlate.enabled=true;
            }
            else if(_visualTime-_coachDoneAt>8f){_coachStep=-1;return;}
            else if(_coachPlate!=null)_coachPlate.enabled=true;
        }

        /// The banner's two lines while a step is up.
        bool CoachText(out string first,out string second)
        {
            first=second="";
            if(_coachStep<0)return false;
            if(_coachStep>=CoachOrders.Length)
            {
                first="That's every order. Key 5 is a chief's order: break a chief and it's yours.";
                second="Win this fight and the Clarth join you.";
                return true;
            }
            if(!_coachShown)return false;
            Directive d=CoachOrders[_coachStep];
            first="Tutorial "+(_coachStep+1)+" of "+CoachOrders.Length+": "+OrderName(d)+". "+CoachAsk[_coachStep];
            second=d==Directive.Hold && ViewSim.ParryCenterOn ? "This paused Hold is ordinary guard. Resume, then use fresh Hold at a beat to parry." : Effect(d)+". "+CoachWhen[_coachStep];
            if(d==Directive.FocusFire&&_targeting)second=TargetPrompt();
            return true;
        }
    }
}
