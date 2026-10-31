using System;
using System.Collections;
using System.Collections.Generic;
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
        IEnumerator ValidateRules9Hud()
        {
            var hud=ValidateCombatHud();while(hud.MoveNext())yield return hud.Current;
            Check(!Tip("heavy",false,false,false,true,true).Contains("costs nerve"),"rules 9 aftermath never teaches old block cost");
            Check(Tip("heavy",false,false,false,true,false).Contains("costs nerve"),"rules 8 aftermath retains its own block cost");
            foreach(var kind in new[]{OrderEffectKind.OutOfReach,OrderEffectKind.StaleBlock,OrderEffectKind.Feinted,OrderEffectKind.WaitedOut})
            {
                var result=new OrderResult(Directive.Hold,0,100,"",false,false,true,new[]{new OrderEffect(30,kind,"Cleave","chief",30,false)},OrderMiss.None,"");
                string text=Receipt(result);
                Check(!text.Contains(kind.ToString()),"new receipt has readable wording: "+kind);
                Check(text.Contains("Cleave"),"receipt names actual move: "+kind);
            }
            // Actual sim fixtures: same high-health units for long observation, never campaign tuning.
            foreach(int phase in new[]{0,1,2})
            {
                bool fake=phase==1,normal=phase==2;
                var tactics=new TacticsProfile();
                if(fake){tactics.SetFeint(5);tactics.SetFeintMove("Cleave");}
                else if(!normal) {tactics.SetWaitOutParry(5);tactics.SetWaitOutMove("Cleave");}
                var ours=new List<UnitSpec>{new UnitSpec("w",UnitClass.Warden,Row.Front,UnitRole.Fighter,100,1000),new UnitSpec("m",UnitClass.Marksman,Row.Back,UnitRole.Fighter,100,1000)};
                var theirs=new List<UnitSpec>{new UnitSpec("s",UnitClass.Skirmisher,Row.Front,UnitRole.Fighter,100,1000),new UnitSpec("chief",UnitClass.Warden,Row.Front,UnitRole.Chief,100,1000)};
                var sim=new BattleSimulation(new BattleSetup("hud-phase",3,ours,theirs,new TacticsProfile(),tactics,
                    BattleSetup.DefaultTickLimit,BattleSetup.DefaultDirectivePoints,NerveRules.On(1000,1000),10,400,
                    new SideRules(1,0,true),new SideRules(2,0,false,HeavyRules.EveryTicks,ChiefSpell.Eclipse,kit:ChiefKit.Bloodridge,moves:ChiefMove.Cleave),FightRules.CampaignChiefTactics.WithSquadFirst()));
                var session=new SimSession(sim);Threat beat=Threat.None;bool small=false;
                while(!sim.IsFinished&&sim.CurrentTick<1200)
                {
                    foreach(var t in sim.Threats(0))
                    {
                        if(t.Name=="Heavy") {small=true;Check(!t.Duel&&!t.FakeFirst&&!t.WaitsOutHold,"Cleave lesson does not colour a small Heavy");}
                        if(t.Name=="Cleave"&&t.BeatTicks==(normal?25:1)){beat=t;break;}
                    }
                    if(!beat.IsNone)break;session.Tick();
                }
                Check(!beat.IsNone&&beat.Duel&&!beat.ParryWindow,"actual learned chief beat has honest unavailable parry window");
                Check(normal?BeatCue(beat).Contains("BIG BEAT"):fake?BeatCue(beat).Contains("FAKE FIRST"):BeatCue(beat).Contains("WAITS OUT"),"actual phase selects exact HUD cue");
                Check(beat.RealBeatTicks==beat.BeatTicks+(fake?ParryRules.FeintTicks:0),"real beat timer follows sim");
                Check(session.TryIssue(Directives.CommandKind,"0","",0,0,"Hold",out _),"fixture Hold accepted through command API");session.Tick();
                if(normal)for(int i=0;i<30;i++)session.Tick();
                var hold=sim.OrderResults(0).Last();
                Check(hold.Effects.Any(e=>e.Kind==(normal?OrderEffectKind.Soaked:fake?OrderEffectKind.Feinted:OrderEffectKind.WaitedOut)),"actual Hold outcome records "+(normal?"soak":fake?"fake":"wait"));
                ResetHoldFeedback();HoldFeedback(sim,out string title,out string detail,out _);
                Check(title.Contains(normal?"BLOCKED":fake?"FAKED":"WAITED OUT"),"live Hold presenter consumes actual phase outcome");
                Check(!title.Contains("PARRIED"),"unanswered beat never claims a parry");
                Check(!hold.Effects.Any(e=>e.Kind==OrderEffectKind.StaleBlock),"rules 9 outcome has no stale block penalty");
                Check(small,"phase fixture also observes unaffected small beat");
            }
            // One fixed default route integration check, not a tuning sweep or human victory claim.
            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","camp.txt")));
            _campaign.ChooseTribe(CampaignTribes.BloodridgeId);_playback=null;
            var pilot=new CampaignPilot(PilotStyle.Reference,21);bool chiefSeen=false;
            for(int n=0;n<5&&_campaign.Stage==CampaignStage.InRun;n++)
            {
                _campaign.TryTakeSpoils(out _);_campaign.TryTakeFavour(0,out _);
                _fight=_campaign.StartEncounter(pilot.PickOffer(_campaign));BeginFight();_pilotOn=false;_paused=true;yield return null;
                Check(ViewSim.SquadFirstOn,"new fixed-route encounter uses rules 9: "+_fight.Encounter.Kind);
                if(_fight.Encounter.Kind==EncounterKind.Chief)
                {
                    chiefSeen=true;Check(ReferenceEquals(_campaign.MindFor(_fight.Encounter),_campaign.ChiefMind(CampaignTribes.BloodridgeId)),"chief briefing reads his own mind");
                    Check(!_fight.Sim.WaitsOutParry(1),"road play has not armed fresh Bloodridge chief with wait-out");
                    File.WriteAllText(Path.Combine(_capture,"rules9-chief-save.txt"),CampaignSave.Write(_campaign));Capture("20-rules9-chief-start");
                }
                int guard=0;while(!ViewOver&&guard++<10000)
                {pilot.Step(_fight);HudTicks(1);if(ViewSim.AwaitingVerdict)Order(Directive.Hold);if(guard%90==0)yield return null;}
                Check(ViewOver,"fixed route encounter settles");Capture("21-route-"+n);
                var result=_campaign.Finish(_fight);Log("Fixed route "+result.Encounter.Kind+": won="+result.Won+", parries="+result.Record.Events.Count(e=>e.Kind==BattleEventKind.Parry&&e.ActorSide==0));
                _fight=null;if(!result.Won)break;
            }
            Log("Default-route chief reached: "+chiefSeen+". A failure to reach him remains a play/balance risk, not repaired here.");
            // CampaignSave writes the camp as it stood BEFORE the active fight. Restore that historical
            // camp as an isolated fixture, then start a NEW rules 9 chief through the public API.
            // The shipped active rules 8 save is never upgraded or overwritten.
            string oldChief=File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","chief.txt"));
            int fightLine=oldChief.IndexOf("fight\t",StringComparison.Ordinal);
            Check(fightLine>0,"historical checkpoint contains its active-fight section");
            _campaign=CampaignSave.Read(oldChief.Substring(0,fightLine)+"end\n");
            Check(_campaign.Active==null&&_campaign.Offers[0].Kind==EncounterKind.Chief,"isolated pre-fight camp restores a chief offer");
            _fight=_campaign.StartEncounter(0);BeginFight();_pilotOn=false;_paused=true;yield return null;
            Check(ViewSim.SquadFirstOn&&CleanCombatHud,"new chief from historical camp uses rules 9 clean HUD");
            Check(ReferenceEquals(_campaign.MindFor(_fight.Encounter),_campaign.ChiefMind(CampaignTribes.BloodridgeId)),"new chief uses his own mind");
            Check(!ViewSim.WaitsOutParry(1),"historical road learning does not arm this fresh chief");
            string fresh=CampaignSave.Write(_campaign);File.WriteAllText(Path.Combine(_capture,"rules9-chief-save.txt"),fresh);
            string state=ViewSim.OrderedStateSnapshot;GateRestore(fresh);yield return null;
            Check(ViewSim.SquadFirstOn&&ViewSim.OrderedStateSnapshot==state,"rules 9 chief reload retains state and rule version");
            int g=0;while((CurrentThreat.IsNone||!CurrentThreat.Duel)&&!ViewOver&&g++<900)HudTicks(1);
            Check(!CurrentThreat.IsNone&&CurrentThreat.Duel,"new chief presents a big beat");
            _paused=false;RefreshFight();DrawBattle();Capture("22-rules9-chief");_paused=true;yield return null;
            CheckCleanTextBounds();Check(Click("chief-moves"),"rules 9 chief guide receives pointer");yield return null;Capture("23-rules9-chief-guide");
            Check(Click("modal-close"),"rules 9 chief guide closes");yield return null;
            pilot=new CampaignPilot(PilotStyle.Reference,21);g=0;
            while(!ViewOver&&g++<10000){pilot.Step(_fight);HudTicks(1);if(ViewSim.AwaitingVerdict)Order(Directive.Hold);if(g%90==0)yield return null;}
            Check(ViewOver,"rules 9 chief fixture settles without changing balance");Capture("24-rules9-chief-end");
            var chiefResult=_campaign.Finish(_fight);Log("Isolated chief fixture won="+chiefResult.Won+", joined="+chiefResult.Joined);
            var archive=_campaign.Archive.Fights.Last();WatchFight(archive,0,archive.Ticks);yield return null;SeekPlayback(archive.Ticks);yield return null;
            Check(ViewSim.SquadFirstOn,"new chief archive replays with rules 9");
            Check(ViewSim.OrderedStateSnapshot==_fight.Sim.OrderedStateSnapshot,"new chief replay matches final authoritative state");
            LeavePlayback();CloseModal();yield return null;
            Log("One fixed scripted route is integration evidence, not balance tuning or a human playtest.");
        }
    }
}
