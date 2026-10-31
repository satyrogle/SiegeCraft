using System.Collections;
using System.IO;
using System.Text;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        IEnumerator RecordCombatFeedback()
        {
            yield return null;
            // Locate a chief through played campaign progression, never by inventing a battle setup.
            Campaign ready=null;
            for(ulong seed=1;seed<=20&&ready==null;seed++)
            {
                var c=Campaign.New(seed);
                if(!CampaignPilot.PlayTutorial(c,PilotStyle.Reference,seed,10))continue;
                var picker=new CampaignPilot(PilotStyle.Reference,seed);
                for(int run=0;run<3&&ready==null;run++)
                {
                    c.StartRun();
                    for(int n=0;n<12&&c.Stage==CampaignStage.InRun;n++)
                    {
                        var fight=c.StartEncounter(picker.PickOffer(c));
                        if(fight.Encounter.Kind==EncounterKind.Chief){ready=c;break;}
                        CampaignPilot.PlayFight(c,fight,new CampaignPilot(PilotStyle.Reference,seed*31+(ulong)n));
                    }
                }
            }
            Check(ready!=null,"comparison fixture reaches a real chief encounter");
            _campaign=ready;_fight=ready.Active;BeginFight();
            _speed=1;_pilotOn=false;
            File.WriteAllText(Path.Combine(_capture,"start-state.txt"),CampaignSave.Write(_campaign));
            int flashes=0,shakes=0,holds=0,maxFlashes=0,flashStreak=0,maxFlashStreak=0;
            bool tintBounded=true,attackEffectsUntinted=true;
            var trace=new StringBuilder("frame,tick,id,clip,clipFrame,white\n");
            for(int i=0;i<720;i++)
            {
                yield return null;
                int activeFlashes=0;
                foreach(var f in _figures)
                {
                    float white=f.hitMaterial!=null?f.hitMaterial.GetFloat(HitWhite):0;
                    if(white>0){flashes++;activeFlashes++;}
                    tintBounded&=white>=0&&white<=.22f;
                    attackEffectsUntinted&=f.drawnClip!=f.attack||white==0;
                    if(f.poseHold>0)holds++;
                    string clip=f.drawnClip==null?"token":f.drawnClip==f.attack?"attack":f.drawnClip==f.hit?"hit":f.drawnClip==f.death?"death":"other";
                    trace.Append(i).Append(',').Append(_fight.Sim.CurrentTick).Append(',').Append(f.unit.Id).Append(',').Append(clip).Append(',').Append(f.drawnFrame).Append(',').Append(white.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append('\n');
                }
                maxFlashes=Mathf.Max(maxFlashes,activeFlashes);
                flashStreak=activeFlashes>0?flashStreak+1:0;maxFlashStreak=Mathf.Max(maxFlashStreak,flashStreak);
                if(_stage.anchoredPosition!=Vector2.zero)shakes++;
                Capture("motion-"+i.ToString("D4"));
            }
            File.WriteAllText(Path.Combine(_capture,"end-state.txt"),CampaignSave.Write(_campaign));
            File.WriteAllText(Path.Combine(_capture,"attack-trace.csv"),trace.ToString());
            File.WriteAllText(Path.Combine(_capture,"feedback-counts.txt"),"enabled="+_combatFeedback+" flashes="+flashes+" holds="+holds+" shakeFrames="+shakes+" maxConcurrentFlashes="+maxFlashes+" maxFlashFrames="+maxFlashStreak);
            Check(maxFlashes<=1,"no more than one fighter flashes in a rendered frame");
            Check(maxFlashStreak<=2&&tintBounded,"flash lasts at most two capture frames and blends at most 22 percent white");
            Check(attackEffectsUntinted,"attack sheets never receive the white tint");
            Check(!_combatFeedback||flashes>0&&holds>0,"chief fight shows the enabled impact effects");
            if(!_combatFeedback)Check(flashes==0&&holds==0&&shakes==0,"comparison disables only the added presentation effects");
            if(_combatFeedback)Check(shakes>0,"real chief hit triggers bounded stage shake");
        }

        // Opt-in player proof using the actual Campaign and rendered UI. Saves stay in _capture.
        IEnumerator ValidateCombatFeedback()
        {
            yield return null;
            Check(Click("new"),"feedback: start through title button");yield return null;
            Check(Click("start-tutorial"),"feedback: tutorial starts through its button");
            _paused=true;_pilotOn=false;_speed=1;
            var reference=CampaignSave.Read(CampaignSave.Write(_campaign)).Active;
            Check(reference!=null,"feedback: independent resumed reference fight exists");
            Check(_combatFeedback,"feedback enabled in delivered player");
            foreach(var f in _figures)if(f.body!=null)
                Check(f.hitMaterial!=null&&f.hitMaterial.shader.isSupported,"hit shader compiled for target graphics API");
            Capture("00-before");
            Check(Click("pause"),"feedback: resume button raycast");
            bool sawMotion=false,sawWhite=false,sawHold=false,checkedPause=false,checkedLocalHold=false,sawLunge=false;
            float maxLunge=0;int visibleLungeFrames=0;
            int frame=0;
            while(frame<240&&!_fight.Over)
            {
                yield return null;
                while(reference.Sim.CurrentTick<_fight.Sim.CurrentTick&&!reference.Over)reference.Tick();
                Check(_fight.Sim.OrderedStateSnapshot==reference.Sim.OrderedStateSnapshot,"feedback leaves authoritative state unchanged at tick "+_fight.Sim.CurrentTick);
                foreach(var f in _figures)
                {
                    float x=f.root.anchoredPosition.x-f.feet.x;
                    if(Mathf.Abs(x)>.5f)sawMotion=true;
                    Check(Mathf.Approximately(x,Mathf.Round(x))&&Mathf.Abs(x)<=FrontLungePixels,"bounded whole-pixel movement");
                    if(f.unit.Row==Row.Front&&Mathf.Abs(x)>=32){sawLunge=true;visibleLungeFrames++;maxLunge=Mathf.Max(maxLunge,Mathf.Abs(x));}
                    Check(Mathf.Abs(f.readoutRoot.anchoredPosition.x-f.root.anchoredPosition.x)<=1,"health bar follows the moving fighter on the 2x grid");
                    if(f.hitMaterial!=null&&f.hitMaterial.GetFloat(HitWhite)>0)sawWhite=true;
                    if(f.poseHold>0)sawHold=true;
                }
                if(!checkedPause&&sawWhite)
                {
                    Check(Click("pause"),"feedback: pause during impact");
                    long tick=_fight.Sim.CurrentTick;float clock=_visualTime;
                    var positions=new Vector2[_figures.Count];var poses=new float[_figures.Count];
                    for(int i=0;i<_figures.Count;i++){positions[i]=_figures[i].root.anchoredPosition;poses[i]=_figures[i].poseTime;}
                    for(int i=0;i<3;i++)
                    {
                        DrawBattle();yield return null;
                        foreach(var f in _figures)if(f.body!=null)
                            Check(f.hitMaterial.GetFloat(HitWhite)==0&&f.body.color==Color.white,"paused frame clears shader flash and legacy colour tint");
                    }
                    Check(_fight.Sim.CurrentTick==tick&&_visualTime==clock,"pause holds the simulation and feedback clock");
                    for(int i=0;i<_figures.Count;i++)Check(_figures[i].poseTime==poses[i]&&_figures[i].root.anchoredPosition==positions[i],"redrawing paused impact does not consume its pose or movement");
                    Capture("01-impact-paused");
                    Check(Click("order-2"),"Focus Fire can be armed during hit pause");
                    Figure enemy=null;foreach(var f in _figures)if(f.unit.Side==1&&f.unit.Alive){enemy=f;break;}
                    int points=_fight.Sim.DirectivePoints(0);
                    Check(enemy!=null&&Click("target-"+enemy.unit.Id),"moving fighter remains a raycast target");
                    Check(_fight.Sim.DirectivePoints(0)==points-Directives.Cost(Directive.FocusFire),"impact does not consume or duplicate the order");
                    Check(reference.TryOrder(Directive.FocusFire,enemy.unit.Id,out _),"reference receives the identical player command");
                    Check(Click("pause"),"feedback resumes without losing the queued order");checkedPause=true;
                }
                if(!checkedLocalHold)
                {
                    Figure held=null,other=null;
                    foreach(var f in _figures){if(f.poseHold>=1f/30f)held=f;else if(f.poseHold==0&&f.unit.Alive)other=f;}
                    if(held!=null&&other!=null)
                    {
                        float a=held.poseTime,b=other.poseTime,action=held.attackAt;
                        yield return null;
                        Check(held.attackAt!=action||Mathf.Approximately(held.poseTime,a),"participant holds its pose unless its next action begins");
                        Check(other.poseTime>b,"uninvolved fighter continues animating during impact");
                        checkedLocalHold=true;
                    }
                }
                Capture("motion-"+(frame++).ToString("D4"));
            }
            Check(sawMotion&&sawWhite&&sawHold&&checkedPause&&checkedLocalHold,"actual hits showed movement, white flash, local hold and live targeting");
            Check(sawLunge&&visibleLungeFrames>=6,"front attacks visibly travel at least 32 native pixels across multiple frames");
            File.WriteAllText(Path.Combine(_capture,"lunge-measurements.txt"),"maxNativePixels="+maxLunge+"\nvisibleFighterFrames="+visibleLungeFrames);
            // Finish at 4x, then verify death/settling and that save/reopen clears transient presentation.
            _speed=4;int guard=0;while(!_fight.Over&&guard++<3000)yield return null;
            Check(_fight.Over,"feedback fight reaches its authoritative ending at 4x");
            for(int i=0;i<100;i++)yield return null;
            foreach(var f in _figures)Check(f.root.anchoredPosition==new Vector2(f.feet.x,-f.feet.y),"fighter settles back onto its original ground anchor");
            Check(_stage.anchoredPosition==Vector2.zero,"camera shake settles without moving HUD");
            Capture("02-ended");
            Save();string saved=CampaignSave.Write(_campaign);
            _campaign=null;_fight=null;SetPage(Page.Title);yield return null;
            Check(Click("continue"),"continue reopens the isolated feedback save");yield return null;
            Check(CampaignSave.Write(_campaign)==saved,"feedback fields do not leak into persisted campaign state");
            foreach(var f in _figures)Check(f.poseHold==0&&f.strikeAt<0&&f.recoilAt<0,"reopen starts with no stale impact offsets or pose holds");
            Capture("03-reopened");
            // Exercise the real coach stopping the fight in the same frame as an active flash.
            _campaign=Campaign.New(21);_fight=_campaign.StartTutorial();_coachTest=true;BeginFight();
            _coachStep=3;_coachShown=false;_coachDoneAt=-100;_paused=false;
            Figure marked=null;foreach(var f in _figures)if(f.body!=null){marked=f;break;}
            marked.whiteAt=_visualTime;marked.flashAt=_visualTime;marked.flash=Color.red;
            RefreshFight();DrawBattle();
            Check(_paused&&_coachShown,"real tutorial stop pauses in the current render frame");
            foreach(var f in _figures)if(f.body!=null)
                Check(f.hitMaterial.GetFloat(HitWhite)==0&&f.body.color==Color.white,"tutorial stop has no white or legacy colour flash");
            Capture("04-coach-paused");
        }
    }
}
