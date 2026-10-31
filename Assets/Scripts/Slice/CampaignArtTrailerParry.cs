using System;
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
        // The trailer's learned trick (Jacob's go, 16 Oct 2026; reports/Pixel art trailers and story cinematics.md
        // in Desk 42). The Bloodridge chief's cast is parried, the result says what Bloodridge learned, and in a
        // later fight the chief waits out the same Hold and his cast lands. A real campaign through the real
        // buttons, one tick a frame. Every frame goes to a lossless native 1440x900 file; crops and scaling are
        // done afterwards, never here.
        const int TrailerHoldLead=5,TrailerTail=75,TrailerLessonFrames=120;
        string _trailerOutcome="";
        long _trailerHoldTick=-1,_trailerCastTick=-1;

        IEnumerator RecordTrailerParry()
        {
            // The gameplay montage rides on this flag (CampaignArtTrailerMontage.cs), so the dispatch stays as it is.
            if(Argument("-ksTrailerMontage")!=null){var montage=RecordTrailerMontage();while(montage.MoveNext())yield return montage.Current;yield break;}
            yield return null;
            string encoderPath=Argument("-ksReviewEncoder");
            Check(encoderPath!=null&&File.Exists(encoderPath),"-ksReviewEncoder names the encoder for the trailer files");
            _savePath=Path.Combine(_capture,"trailer-parry-save.txt");
            var notes=new StringBuilder();
            bool done=false;
            for(ulong seed=1;seed<=12&&!done;seed++)
            {
                _campaign=Campaign.New(seed);
                if(!CampaignPilot.PlayTutorial(_campaign,PilotStyle.Reference,seed,10))continue;
                var picker=new CampaignPilot(PilotStyle.Reference,seed);
                int n=0;
                // His first fight: no orders until his cast is TrailerHoldLead ticks out, then one Hold, which parries it.
                int offer=TrailerChiefOffer(picker,seed,ref n);
                if(offer<0)continue;
                var start=TrailerStartFight(offer);while(start.MoveNext())yield return start.Current;
                if(_fight.Sim.WaitsOutParry(1))continue;
                var first=TrailerCast("trailer-1-parried",encoderPath);while(first.MoveNext())yield return first.Current;
                notes.Append("seed ").Append(seed).Append(", run ").Append(_campaign.RunsStarted).Append(", first chief fight: Hold at tick ").Append(_trailerHoldTick)
                    .Append(", cast over at tick ").Append(_trailerCastTick).Append(", ").Append(_trailerOutcome).Append('\n');
                if(_trailerOutcome!="parried")continue;
                // The reference pilot plays the rest of it; then the result says what they learned.
                _pilot=new CampaignPilot(PilotStyle.Reference,seed*31+(ulong)n++);_pilotOn=true;_speed=24;
                for(int guard=0;!_fight.Over&&guard<4000;guard++)yield return null;
                Check(_fight.Over,"the first chief fight plays out");
                for(int i=0;i<76;i++)yield return null;
                Check(Click("see-result"),"the result through its own button");yield return null;
                Lesson parry=null;
                foreach(var lesson in _result.EnemyLessons)if(lesson.Rule==LessonRules.Parry)parry=lesson;
                // From rules 9 the chief learns into his own mind, which only fights against him teach (Campaign.ChiefMind).
                var learner=_result.Record.SquadFirst?_campaign.ChiefMind(CampaignTribes.BloodridgeId):_campaign.Mind(CampaignTribes.BloodridgeId);
                Check(parry!=null&&learner.HabitValue(Habit.WaitOutParry)>=BattleSimulation.HabitThreshold,
                    "Bloodridge learned to wait out your Hold from that one parry");
                notes.Append("lesson: ").Append(parry.Why).Append('\n');
                var still=TrailerStill("trailer-2-lesson",encoderPath,TrailerLessonFrames);while(still.MoveNext())yield return still.Current;
                Capture("trailer-2-lesson");
                AfterResult();
                // His next fight, down the road and over as many runs as it takes: the same Hold at the same moment.
                offer=TrailerChiefOffer(picker,seed,ref n);
                if(offer<0)continue;
                start=TrailerStartFight(offer);while(start.MoveNext())yield return start.Current;
                if(!_fight.Sim.WaitsOutParry(1)){notes.Append("the habit wore off before his next fight\n");continue;}
                var second=TrailerCast("trailer-3-waited",encoderPath);while(second.MoveNext())yield return second.Current;
                notes.Append("seed ").Append(seed).Append(", run ").Append(_campaign.RunsStarted).Append(", next chief fight: Hold at tick ").Append(_trailerHoldTick)
                    .Append(", cast over at tick ").Append(_trailerCastTick).Append(", ").Append(_trailerOutcome).Append('\n');
                done=_trailerOutcome=="waited";
            }
            File.WriteAllText(Path.Combine(_capture,"trailer-parry.txt"),notes.ToString());
            Check(done,"a parried cast, the lesson, then the same chief waiting out the same Hold");
        }

        // The road played headless by the reference pilots until the Bloodridge chief's fight is offered, leaving
        // camp on a new run whenever one ends. The offer's index, or -1.
        int TrailerChiefOffer(CampaignPilot picker,ulong seed,ref int n)
        {
            for(int guard=0;guard<80;guard++)
            {
                if(_campaign.Stage==CampaignStage.ChooseTribe)_campaign.StartRun();
                if(_campaign.Stage!=CampaignStage.InRun||_campaign.Offers.Count==0)return -1;
                for(int i=0;i<_campaign.Offers.Count;i++)
                {
                    var e=_campaign.Offers[i];
                    if(e.Kind==EncounterKind.Chief&&e.Tribe.Id==CampaignTribes.BloodridgeId&&e.EnemyCasts)return i;
                }
                CampaignPilot.PlayFight(_campaign,_campaign.StartEncounter(picker.PickOffer(_campaign)),
                    new CampaignPilot(PilotStyle.Reference,seed*31+(ulong)n++));
            }
            return -1;
        }

        // Starts an offered fight through the offers and squad pages, drawn at its first tick, nobody's orders on.
        IEnumerator TrailerStartFight(int offer)
        {
            SetPage(Page.Offers);yield return null;
            Check(Click("offer-"+offer),"the chief's fight on the road");yield return null;
            Check(Click("start-fight"),"the chief's fight starts from the squad page");
            _pilotOn=false;_paused=true;_speed=1;
            yield return null;
            Check(_fight!=null&&_fight.Sim.CurrentTick==0&&ViewTribe==CampaignTribes.BloodridgeId,"the Bloodridge chief's fight, drawn at its first tick");
        }

        // One fight at one tick a frame, from its first tick until TrailerTail frames after his cast is over. Your
        // only order is one Hold through the plate, TrailerHoldLead ticks before the cast is due. Sets
        // _trailerOutcome: parried, waited (he waited out the Hold, then cast), landed, or missed.
        IEnumerator TrailerCast(string name,string encoderPath)
        {
            var sim=_fight.Sim;
            _trailerOutcome="missed";_trailerHoldTick=_trailerCastTick=-1;
            bool held=false,waited=false;
            int tail=-1,seen=sim.Events.Count,frames=0;
            var trace=new StringBuilder("frame,tick,windup,points,callout\n");
            _paused=false;
            var encoder=TrailerEncoder(encoderPath,name);
            try
            {
                while(frames<3000&&!_fight.Over)
                {
                    TrailerFrame(encoder);
                    trace.Append(frames).Append(',').Append(sim.CurrentTick).Append(',').Append(sim.WindupTicks(1)).Append(',')
                        .Append(sim.DirectivePoints(0)).Append(',').Append(_flashText[1]).Append('\n');
                    frames++;
                    if(tail>=0&&tail--==0)break;
                    if(!held&&sim.WindupTicks(1)>0&&sim.WindupTicks(1)<=TrailerHoldLead&&sim.CanUse(0,Directive.Hold,out _))
                    {
                        Check(Click("order-0"),"Hold through its plate, "+sim.WindupTicks(1)+" ticks before the cast");
                        held=true;_trailerHoldTick=sim.CurrentTick;
                    }
                    yield return null;
                    bool release=false,parry=false;
                    for(;seen<sim.Events.Count;seen++)
                    {
                        var e=sim.Events[seen];
                        if(!held||tail>=0)continue;
                        if(e.Kind==BattleEventKind.Parry&&e.ActorSide==0)parry=true;
                        if(e.Kind==BattleEventKind.Reaction&&e.ActorSide==1&&e.Detail==BattleSimulation.WaitOutDetail)waited=true;
                        if(e.Kind==BattleEventKind.Release&&e.ActorSide==1){release=true;_trailerCastTick=e.Tick;}
                    }
                    if(release&&tail<0){_trailerOutcome=parry?"parried":waited?"waited":"landed";tail=TrailerTail;}
                }
            }
            finally{TrailerEncoderDone(encoder,name);}
            File.WriteAllText(Path.Combine(_capture,name+"-trace.csv"),trace.ToString());
            Log(name+": "+frames+" frames, Hold at tick "+_trailerHoldTick+", cast over at tick "+_trailerCastTick+", "+_trailerOutcome);
        }

        // A page that is not a fight, as it stands, for this many frames.
        IEnumerator TrailerStill(string name,string encoderPath,int frames)
        {
            var encoder=TrailerEncoder(encoderPath,name);
            try{for(int i=0;i<frames;i++){TrailerFrame(encoder);yield return null;}}
            finally{TrailerEncoderDone(encoder,name);}
            Log(name+": "+frames+" frames");
        }

        System.Diagnostics.Process TrailerEncoder(string encoderPath,string name)
        {
            var encoder=new System.Diagnostics.Process();
            encoder.StartInfo=new System.Diagnostics.ProcessStartInfo(encoderPath,
                "-y -loglevel error -f rawvideo -pixel_format rgb24 -video_size "+(int)W+"x"+(int)H+" -framerate 30 -i pipe:0 -an -vf vflip -c:v ffv1 -pix_fmt bgr0 \""+Path.Combine(_capture,name+".mkv")+"\"")
            {UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardError=true};
            encoder.Start();return encoder;
        }

        // Exactly what Capture draws, sent to the encoder in place of a PNG.
        void TrailerFrame(System.Diagnostics.Process encoder)
        {
            if(_page==Page.Fight&&_fight!=null){RefreshFight();DrawBattle();}
            Canvas.ForceUpdateCanvases();Camera.main.Render();var previous=RenderTexture.active;RenderTexture.active=_captureTarget;
            _capturePixels.ReadPixels(new UnityEngine.Rect(0,0,W,H),0,0);_capturePixels.Apply();RenderTexture.active=previous;
            byte[] data=_capturePixels.GetRawTextureData();encoder.StandardInput.BaseStream.Write(data,0,data.Length);
        }

        void TrailerEncoderDone(System.Diagnostics.Process encoder,string name)
        {
            encoder.StandardInput.Close();string errors=encoder.StandardError.ReadToEnd();encoder.WaitForExit();
            int code=encoder.ExitCode;encoder.Dispose();
            File.WriteAllText(Path.Combine(_capture,name+"-encoder.txt"),"exit="+code+"\n"+errors);
            if(code!=0)throw new InvalidOperationException("Trailer encoder failed on "+name+": "+errors);
        }
    }
}
