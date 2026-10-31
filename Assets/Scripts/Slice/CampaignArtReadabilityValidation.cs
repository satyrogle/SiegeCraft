using System.Collections;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        IEnumerator ValidateReadability()
        {
            // Exercise the real reward UI, every choice, persistence and historical seeking first.
            yield return ValidateTheme();
            yield return ValidateCommandFeedback();
            int cap=Argument("-ksReadability21")!=null?21:20;
            CloseModal();IntegrationCap(cap);NewReviewRoute();yield return null;
            Check(Click("offer-0"),"readability encounter opens");yield return null;
            QaChooseDuelist();yield return null;
            Check(Click("start-fight"),"readability fight starts");yield return null;
            Check(_fight.RulesVersion==cap&&ViewSim.PayoffsOn&&ViewSim.DangerOn==(cap>=21),"released rules "+cap+" is active in the new route");
            _pilotOn=false;_paused=true;_speed=.5f;_hover="";_notice="";
            bool threat=false,parry=false;
            for(int tick=0;tick<600&&!ViewOver;tick++)
            {
                HudTicks(1);
                _paused=false;_hover="";RefreshFight();DrawBattle();
                if(tick==0)
                {
                    CaptureAsDrawn("compact-first-live-frame");
                    Check(_pxBanner2.key.Split('|')[0]=="","live fight has no default explanation strip");
                    Check(_pxLane.All(label=>label.key.Split('|')[0]==""),"live lanes have no duplicate prose");
                }
                if(!CurrentThreat.IsNone&&!threat)
                {
                    threat=true;Check(_threatHeading.key.Contains(CurrentThreat.Name.ToUpperInvariant()),"compact threat retains the authoritative move name");
                    CheckCleanTextBounds();CaptureAsDrawn("compact-live-threat");
                }
                if(LiveParryCue(ViewSim,CurrentThreat)&&!parry)
                {
                    parry=true;Check(_threatAnswers.key.Contains("PARRY"),"live gold timing has a short actionable parry cue");
                    CaptureAsDrawn("compact-live-parry");
                }
                _paused=true;
                if(threat&&parry)break;
                if(tick%60==0)yield return null;
            }
            Check(threat&&parry,"real fight reaches an incoming beat and an actionable parry window");
            long parryAt=ViewSim.CurrentTick;
            _paused=false;Check(Click("order-0"),"live Hold sent through its actual control");_paused=true;
            for(int tick=0;tick<60&&!ViewOver&&!ViewSim.Events.Any(e=>e.Kind==BattleEventKind.Parry&&e.ActorSide==0&&e.Tick>=parryAt);tick++)HudTicks(1);
            Check(ViewSim.Events.Any(e=>e.Kind==BattleEventKind.Parry&&e.ActorSide==0&&e.Tick>=parryAt),"timed Hold lands a real parry");
            RefreshFight();DrawBattle();
            Check(ViewSim.NextCombo(0).Combo==FightRules.Riposte&&_orderTitle[3].key.Contains("RIPOSTE"),"Unleash lights as Riposte only for its authoritative follow-up");
            Capture("compact-riposte-ready");
            Save();string savedState=ViewSim.OrderedStateSnapshot;ContinueSaved();_paused=true;_pilotOn=false;
            Check(_fight.RulesVersion==cap&&ViewSim.OrderedStateSnapshot==savedState,"route reload preserves exact active combat state at cap "+cap);
            ShowClarityDetails();yield return null;
            Check(_modal!=null,"full threat and command explanations remain available through Details");Capture("compact-paused-details");CloseModal();
            // A real learned counter must preserve the benefit, including when zero blows have yet landed.
            var setup=new BattleSetup("compact.unleash",21,
                new[]{new UnitSpec("clarth.front",UnitClass.Warden)},
                new[]{new UnitSpec("bloodridge.front",UnitClass.Warden)},
                new TacticsProfile(),new TacticsProfile(),600,BattleSetup.DefaultDirectivePoints,NerveRules.On());
            var counter=new BattleSimulation(setup);
            var session=new SimSession(counter);
            Check(session.TryIssue(Directives.CommandKind,"1","",0,0,Directives.Name(Directive.Hold),out _),"counter fixture accepts enemy Hold");
            Check(session.TryIssue(Directives.CommandKind,"0","",0,0,Directives.Name(Directive.Unleash),out _),"counter fixture accepts Unleash");
            var result=counter.OrderResults(0).Last();
            Check(result.Effects.Any(e=>e.Kind==OrderEffectKind.ScareBlocked),"real Unleash result records only scare blocked");
            string receipt=CompactReceipt(result);
            Check(receipt.Contains("+"+(BattleSimulation.UnleashDamagePercent-100)+"% DMG")&&receipt.Contains("SCARE BLOCKED"),"partial-success receipt names retained damage and blocked scare");
            Check(counter.UnleashTicksLeft(0)>0,"counter leaves authoritative Unleash damage window running");
            // A successful expired damage order has Miss.None; that enum must never become a negative-looking NONE receipt.
            var damageOnly=new OrderResult(Directive.FocusFire,0,150,"target",false,false,true,
                new[]{new OrderEffect(140,OrderEffectKind.Damage,"","target",37,false)},OrderMiss.None,"");
            Check(CompactReceipt(damageOnly).Contains("37")&&!CompactReceipt(damageOnly).Contains("NONE"),"completed damage-only success never displays NONE");
            Log("Readability checks complete. Rendered scripted QA; art handback and owner feel verdict remain pending.");
        }
    }
}
