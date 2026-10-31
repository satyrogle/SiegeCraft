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
        // Bounded presentation checks over fixed saves and recorded fights. No pilot or tuning.
        IEnumerator ValidateJobs()
        {
            string fixtures=Argument("-ksJobsValidate");
            _savePath=Path.Combine(_capture,"sprint-test-save.txt");
            _pilotOn=false;_paused=true;
            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"parry-0.txt")));
            SetPage(Page.ChooseTribe);yield return null;Capture("01-camp");
            string state=CampaignSave.Write(_campaign);
            Check(_screen.GetComponentsInChildren<RawImage>().Count(r=>r.name.EndsWith(" at work"))==4,"camp shows four approved Pal loops");
            Check(Click("camp-keeps"),"camp keeps control raycasts");yield return null;
            Check(Click("modal-close"),"camp keeps closes");yield return null;
            Check(Click("camp-learned"),"camp habits control raycasts");yield return null;
            Capture("02-camp-habits");Check(Click("modal-close"),"camp habits closes");yield return null;
            Check(CampaignSave.Write(_campaign)==state,"reading camp changes no saved state");
            Check(Click("tribe-"+_campaign.Choosable()[0].Id),"start run uses camp control");yield return null;
            Capture("03-road");
            // The selected tribe's first row need not contain a War Party. This probe needs a full squad.
            int offer=_campaign.Offers.ToList().FindIndex(e=>e.SquadSize>=2);
            Check(offer>=0,"fixed camp road offers a squad encounter");
            Log("AUDIT road probe: "+_campaign.Target.Id+" / "+_campaign.Offers[offer].Kind);
            Check(Click("offer-"+offer),"road choice raycasts");yield return null;
            Check(_screen.GetComponentsInChildren<RawImage>().Count(r=>r.name=="Fighter portrait")==_campaign.Reserve.Count,"all squad rows have native portraits");
            Capture("04-squad");
            Check(Click("start-fight"),"squad starts fight");_pilotOn=false;_paused=true;yield return null;
            Check(ArtScale==1&&Px==2,"native world and UI scales retained");
            Check(!_pxStats.key.Contains("NERVE"),"top strip no longer repeats nerve bars");
            var actor=_figures.First(f=>f.unit.Side==0);var target=_figures.First(f=>f.unit.Side==1);
            var other=_figures.First(f=>f!=actor&&f!=target);
            state=ViewSim.OrderedStateSnapshot;
            // Synthetic event probes test presentation duration and crowd dimming, not sim outcomes.
            foreach(var kind in new[]{BattleEventKind.Parry,BattleEventKind.Combo,BattleEventKind.Kill,BattleEventKind.Release})
            {
                _paused=false;_visualTime+=1;AdvanceCombatPoses(1);DrawBattle();
                foreach(var f in _figures){f.majorFreeze=false;f.poseHold=0;}
                string detail=kind==BattleEventKind.Combo?FightRules.Riposte:kind==BattleEventKind.Release?Moves.Full:"";
                _windupName[actor.unit.Id]="Eclipse";
                var e=new BattleEvent(ViewSim.CurrentTick,kind,actor.unit.Side,actor.unit.Id,(int)actor.unit.Class,
                    kind==BattleEventKind.Release?"":target.unit.Id,(int)target.unit.Class,0,detail);
                MajorMoment(e,actor,kind==BattleEventKind.Release?null:target);
                Check(Mathf.Approximately(actor.poseHold,kind==BattleEventKind.Kill?.33f:.15f),kind+" uses bounded local pose hold");
                var clip=actor.drawnClip;int frame=actor.drawnFrame;
                _visualTime+=.03f;AdvanceCombatPoses(.03f);DrawBattle();
                Check(actor.drawnClip==clip&&actor.drawnFrame==frame,kind+" holds the actual rendered frame");
                if(kind!=BattleEventKind.Release)Check(MomentTint(other,Color.white).r<.5f&&MomentTint(actor,Color.white).r==1,kind+" keeps pair lit and dims others");
                DrawCombatFeedback(target);
                Check(target.hitMaterial.GetFloat(HitWhite)==1,kind+" has a brief body white flash");
                if(kind==BattleEventKind.Parry)Capture("05-major-event-probe");
                _visualTime+=.5f;AdvanceCombatPoses(.5f);DrawBattle();
                Check(!actor.majorFreeze&&MomentTint(other,Color.white).r==1,kind+" presentation settles");
            }
            _paused=true;DrawBattle();
            Check(ViewSim.OrderedStateSnapshot==state,"all event probes leave authoritative sim unchanged");
            _selected="";int numbers=_numbers.Count;
            var ordinary=_figures.First(f=>!f.unit.IsLeader);
            FloatMiss(ordinary);FloatNumber(ordinary,1,false);
            Check(_numbers.Count==numbers,"unselected ordinary hit and miss clutter suppressed");
            _selected=ordinary.unit.Id;FloatNumber(ordinary,1,false);
            Check(_numbers.Count>numbers,"selected fighter retains small damage feedback");
            Save();state=ViewSim.OrderedStateSnapshot;
            _campaign=CampaignSave.Read(File.ReadAllText(_savePath));ResumeFight();_pilotOn=false;_paused=true;
            Check(ViewSim.OrderedStateSnapshot==state,"isolated save reload preserves fight exactly");

            // Genuine historical recordings: preserve unavailable history rather than dereferencing a failed playback.
            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"fixture-clue.txt")));
            string campaignBefore=CampaignSave.Write(_campaign);
            foreach(string id in new[]{"camp.3.raiders.trial","camp.5.raiders.chief","camp.11.alpha.alphahunt"})
            {
                _fight=null;_playback=null;SetPage(Page.ChooseTribe);
                var historical=_campaign.Archive.Find(id);
                Check(historical!=null,"historical recording remains in archive "+id);
                bool playable=FightPlayback.TryOpen(_campaign.Archive,id,out _,out string why);
                WatchFight(historical);yield return null;
                if(!playable)
                {
                    Check(_playback==null&&_modal!=null&&_page!=Page.Playback,
                        "incompatible historical fight stays in recorded history "+id);
                    Check(why.Contains("history is kept"),"unavailable replay explains preserved history "+id);
                    Check(CampaignSave.Write(_campaign)==campaignBefore,"unavailable replay preserves full campaign "+id);
                    Capture(id+"-history");CloseModal();continue;
                }
                Check(_playback!=null,"compatible historical replay opens "+id);
                _paused=false;_speed=1;
                for(int i=0;i<240&&!_playback.AtEnd;i++)
                {yield return null;if(i%3==0)Capture(id+"-motion-"+(i/3).ToString("D3"));}
                _paused=true;Capture(id+"-paused");
                long tick=_playback.Tick;for(int i=0;i<3;i++)yield return null;
                Check(_playback.Tick==tick,"pause holds recorded fight "+id);
                SeekPlayback(_playback.Length);
                Check(_playback.AtEnd&&_playback.Matched,"fixed replay checksum matches "+id);
            }
            Check(CampaignSave.Write(_campaign)==campaignBefore,"all replays leave campaign unchanged");
            _playback=null;_fight=null;
            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"skullpeak-duel.txt")));
            SetPage(Page.Offers);yield return null;
            offer=_campaign.Offers.ToList().FindIndex(e=>e.Kind==EncounterKind.Duel);
            Check(Click("offer-"+offer),"Skullpeak duel choice");yield return null;
            QaChooseDuelist();yield return null;
            Check(Click("start-fight"),"Skullpeak duel starts");_pilotOn=false;_paused=false;_speed=1;
            Check(_figures.Count==2&&Mathf.Abs(_figures[0].feet.x-_figures[1].feet.x)==300,"duel uses closer 300-pixel separation at 1x");
            for(int i=0;i<240&&!ViewOver;i++){yield return null;if(i%3==0)Capture("skullpeak-motion-"+(i/3).ToString("D3"));}
            _paused=true;Save();Capture("06-skullpeak-duel");
            var results=ValidateCurrentResultPages(fixtures);
            while(results.MoveNext())yield return results.Current;
        }

        IEnumerator ValidateCurrentResultPages(string fixtures)
        {
            var source=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"fixture-clue.txt")));
            int movingReplays=0,gearReceipts=0;
            for(int scenario=0;scenario<4;scenario++)
            {
                _fight=null;_playback=null;_paused=true;_pilotOn=false;
                if(scenario==0){_campaign=Campaign.New(1);_fight=_campaign.StartTutorial();}
                else if(scenario==1)
                {
                    AfterResult();_campaign.StartRun();
                    int offer=_campaign.Offers.ToList().FindIndex(e=>e.Kind==EncounterKind.WarParty);
                    Check(offer>=0,"current rules offer fixed war party");_fight=_campaign.StartEncounter(offer);
                }
                else
                {
                    _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,scenario==2?"fixture-recipe.txt":"fixture-hunt-win.txt")));
                    _fight=_campaign.StartEncounter(0);
                }
                string id=new[]{"camp.1.raiders.tutorial","camp.2.raiders.warparty","camp.5.raiders.chief","camp.11.alpha.alphahunt"}[scenario];
                var record=source.Archive.Find(id);int next=0;
                // Reapply the fixed old command schedule under today's rules, without claiming its old checksum matches.
                while(!_fight.Over)
                {
                    while(next<record.Log.Commands.Count&&record.Log.Commands[next].Tick<=_fight.Sim.CurrentTick)
                    {
                        var order=record.Log.Commands[next++];
                        Check(Directives.TryParse(order.TextArg,out Directive directive),"fixed order parses for result "+scenario);
                        Check(_fight.TryOrder(directive,order.TargetId,out string why),"fixed order accepted for result "+scenario+": "+why);
                    }
                    _fight.Tick();
                }
                string currentId=_fight.Setup.FightId;
                ShowResult();yield return null;Capture("result-"+scenario);
                Check(_screen.Find("DIED value")!=null&&_screen.Find("RECRUITED value")!=null,"result "+scenario+" displays prominent actual roster totals");
                string before=CampaignSave.Write(_campaign);
                Check(Click("result-tactics"),"result "+scenario+" tactics opens");yield return null;
                Check(Click("modal-close"),"result "+scenario+" tactics closes");yield return null;
                Check(CampaignSave.Write(_campaign)==before,"result "+scenario+" details leave rewards unchanged");
                Log("AUDIT current result "+scenario+": won="+_result.Won+", died="+_result.Fallen.Count+", recruited="+_result.JoinedFighters.Count+", materials="+_result.MaterialsEarned);
                if(_buttons.ContainsKey("take-spoils")&&_buttons["take-spoils"].interactable)
                {
                    int materials=_campaign.Materials,price=_result.MaterialsEarned;
                    string gear=_result.SpoilsOffered.Id;
                    Check(Click("take-spoils"),"result "+scenario+" exchange through real control");yield return null;
                    Check(_modal!=null&&_campaign.Materials==materials&&_campaign.FindGear(gear)==null,"result "+scenario+" preview does not spend");
                    Check(Click("strip-0"),"result "+scenario+" strip first piece through real control");yield return null;
                    Check(_campaign.Materials==materials-price&&_campaign.FindGear(gear)!=null&&!_buttons.ContainsKey("take-spoils"),"result "+scenario+" exchanges exactly once");
                    Check(_screen.Find("MATERIALS EXCHANGED value")!=null,"result "+scenario+" summary labels exchanged materials truthfully");
                    Save();var restored=CampaignSave.Read(File.ReadAllText(_savePath));
                    Check(restored.Materials==_campaign.Materials&&restored.FindGear(gear)!=null,"result "+scenario+" exchange survives save reload");
                    Capture("receipt-"+scenario);
                    gearReceipts++;
                }
                if(scenario==0)continue;
                // Archive the actual current-rule outcome, then verify moving playback independently.
                // These are fixed recorded command schedules, never a bot or a seed/outcome search.
                string resultState=CampaignSave.Write(_campaign);
                SetPage(Page.ChooseTribe);WatchFight(_campaign.Archive.Find(currentId));
                Check(_playback!=null,"current result replay opens "+currentId);
                _paused=false;_speed=1;
                long startTick=_playback.Tick;
                int sampledFrames=0;
                for(int frame=0;frame<240&&!_playback.AtEnd;frame++)
                {
                    yield return null;
                    // Auto playback advances one 30 Hz tick per frame at 1x. Check before
                    // SeekPlayback restarts the replay and can erase a stall or failure.
                    long expectedTick=Math.Min(startTick+frame+1,_playback.Length);
                    Check(!_paused&&_playback.Failure.Length==0&&_playback.Tick==expectedTick,
                        "current replay progresses without error "+currentId+" frame "+frame+
                        " (expected tick "+expectedTick+", actual "+_playback.Tick+", failure: "+_playback.Failure+")");
                    sampledFrames++;
                    if(frame%3==0)Capture("current-"+scenario+"-motion-"+(frame/3).ToString("D3"));
                }
                Check(sampledFrames>0,"current replay samples player updates "+currentId);
                _paused=true;Capture("current-"+scenario+"-paused");
                long tick=_playback.Tick;for(int frame=0;frame<3;frame++)yield return null;
                Check(_playback.Tick==tick,"pause holds current recorded fight "+currentId);
                SeekPlayback(_playback.Length);
                Check(_playback.AtEnd&&_playback.Matched,"current replay checksum matches "+currentId);
                Check(CampaignSave.Write(_campaign)==resultState,"current replay leaves rewards and history unchanged "+currentId);
                _playback=null;SetPage(Page.Result);movingReplays++;
            }
            Check(movingReplays==3,"path chief and hunt all receive verified moving replay coverage");
            Log("AUDIT current-result gear receipts: "+gearReceipts+"; class loot and duel exchange coverage is the separate -ksClassValidate gate.");
            Check(_resultScrolls==0,"current result pages fit without scrolling");
        }
    }
}
