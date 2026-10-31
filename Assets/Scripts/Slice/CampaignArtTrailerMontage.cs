using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // The trailer's gameplay montage (Jacob, 18 Oct 2026: "go start the gameplay montage"), filmed like the
        // learned trick: one real campaign through the real buttons, every frame lossless at native 1440x900, fights
        // at one tick a frame with the reference pilot giving the orders. Run with -ksTrailerParry on
        // -ksTrailerMontage on. The parts in story order: the tutorial (Bloodridge comes for the cult), the road, a
        // road fight, the end of the Bloodridge chief's fight where his nerve breaks and he is captured, the result
        // where his band joins, the next squad page, that mixed fight, and a hunters' ambush from its red node.
        // The chief's fight is found headless first (save, play, read back), so only its last seconds are filmed.
        const int MontageFightFrames=210,MontageRoadFrames=96,MontageChiefTail=150,MontageAfterEnd=45,MontageStillFrames=90;

        IEnumerator RecordTrailerMontage()
        {
            yield return null;
            string encoderPath=Argument("-ksReviewEncoder");
            Check(encoderPath!=null&&File.Exists(encoderPath),"-ksReviewEncoder names the encoder for the trailer files");
            _savePath=Path.Combine(_capture,"trailer-montage-save.txt");
            var notes=new StringBuilder();
            bool done=false;
            for(ulong seed=1;seed<=40&&!done;seed++)
            {
                notes.Append("seed ").Append(seed).Append(": ");
                _campaign=Campaign.New(seed,EasyTribes());

                // The tutorial from its first tick.
                _fight=_campaign.StartTutorial();BeginFight();
                MontagePilot(seed*7919UL);
                var film=MontageFight("mtg-1-tutorial",encoderPath,MontageFightFrames);while(film.MoveNext())yield return film.Current;
                var rest=MontageFinish();while(rest.MoveNext())yield return rest.Current;
                if(_campaign.Stage==CampaignStage.Tutorial){notes.Append("tutorial lost\n");continue;}
                Check(Click("after-result"),"on from the tutorial");yield return null;
                if(_campaign.Stage==CampaignStage.ChooseTribe)_campaign.StartRun();
                if(_campaign.Stage!=CampaignStage.InRun||_campaign.Offers.Count==0){notes.Append("no run\n");continue;}

                // The road at the start of the run, the selection walking along its nodes.
                SetPage(Page.Offers);yield return null;
                var road=MontageRoad("mtg-2-road",encoderPath,-1);while(road.MoveNext())yield return road.Current;

                // The pilot's first pick on the road, through its squad page.
                int offer=NonAmbushOffer(seed);
                Check(Click("offer-"+offer),"the first road fight's node");yield return null;
                Check(Click("start-fight"),"the first road fight starts");
                MontagePilot(seed*31UL);
                film=MontageFight("mtg-3-fight",encoderPath,MontageFightFrames);while(film.MoveNext())yield return film.Current;
                rest=MontageFinish();while(rest.MoveNext())yield return rest.Current;
                Check(Click("after-result"),"on from the first road fight");yield return null;

                // Headless to the Bloodridge chief, then his fight played headless from a save until a pilot breaks
                // his nerve while he stands.
                var picker=new CampaignPilot(PilotStyle.Reference,seed);
                int n=1;
                int chief=TrailerChiefOffer(picker,seed,ref n);
                if(chief<0){notes.Append("no chief\n");continue;}
                string saved=CampaignSave.Write(_campaign);
                long endTick=-1;ulong pilotSeed=0;
                for(ulong k=0;k<8&&endTick<0;k++)
                {
                    var probe=CampaignSave.Read(saved,EasyTribes());
                    var trial=probe.StartEncounter(chief,MontageSquad(probe,chief));
                    var r=CampaignPilot.PlayFight(probe,trial,new CampaignPilot(PilotStyle.Reference,seed*977UL+k));
                    if(r.ChiefCaptured){endTick=trial.Sim.CurrentTick;pilotSeed=seed*977UL+k;}
                }
                if(endTick<0){notes.Append("no pilot captured the chief\n");continue;}

                // The same fight on screen: fast to its last seconds, then one tick a frame through his capture.
                _campaign=CampaignSave.Read(saved,EasyTribes());
                SetPage(Page.Offers);yield return null;
                Check(Click("offer-"+chief),"the Bloodridge chief's node");yield return null;
                Check(Click("start-fight"),"the chief's fight starts");
                MontagePilot(pilotSeed);_speed=24;
                for(int guard=0;guard<4000&&!_fight.Over&&_fight.Sim.CurrentTick<endTick-MontageChiefTail-30;guard++)yield return null;
                _speed=1;
                var end=MontageToEnd("mtg-4-chief",encoderPath);while(end.MoveNext())yield return end.Current;
                for(int i=0;i<76-MontageAfterEnd;i++)yield return null;
                Check(Click("see-result"),"the chief's result");yield return null;
                if(!_result.ChiefCaptured){notes.Append("on screen the chief was not captured\n");continue;}
                var still=TrailerStill("mtg-5-result",encoderPath,MontageStillFrames);while(still.MoveNext())yield return still.Current;
                Check(Click("after-result"),"on from the chief");yield return null;
                if(_campaign.Stage==CampaignStage.ChooseTribe)_campaign.StartRun();
                if(_campaign.Stage!=CampaignStage.InRun||_campaign.Offers.Count==0){notes.Append("the run ended at the chief\n");continue;}

                // The next fight with his band in the squad: its squad page, then the fight.
                SetPage(Page.Offers);yield return null;
                offer=NonAmbushOffer(seed+101UL);
                Check(Click("offer-"+offer),"the next road fight's node");yield return null;
                still=TrailerStill("mtg-6-squad",encoderPath,MontageStillFrames);while(still.MoveNext())yield return still.Current;
                Check(Click("start-fight"),"the mixed fight starts");
                MontagePilot(seed*53UL);
                film=MontageFight("mtg-7-mixed",encoderPath,MontageFightFrames);while(film.MoveNext())yield return film.Current;
                rest=MontageFinish();while(rest.MoveNext())yield return rest.Current;
                Check(Click("after-result"),"on from the mixed fight");yield return null;

                // On down the road, headless, until the hunters hold a node; then that node and its ambush.
                int ambush=-1;
                for(int guard=0;guard<20&&ambush<0&&_campaign.Stage==CampaignStage.InRun;guard++)
                {
                    for(int i=0;i<_campaign.Offers.Count;i++)if(_campaign.Offers[i].Kind==EncounterKind.Ambush)ambush=i;
                    if(ambush<0)CampaignPilot.PlayFight(_campaign,_campaign.StartEncounter(NonAmbushOffer(seed)),
                        new CampaignPilot(PilotStyle.Reference,seed*131UL+(ulong)guard));
                }
                if(ambush<0){notes.Append("no ambush\n");continue;}
                SetPage(Page.Offers);yield return null;
                road=MontageRoad("mtg-8-ambush-road",encoderPath,ambush);while(road.MoveNext())yield return road.Current;
                Check(Click("offer-"+ambush),"the ambush node");yield return null;
                Check(Click("start-fight"),"the ambush starts");
                MontagePilot(seed*59UL);
                film=MontageFight("mtg-9-ambush",encoderPath,MontageFightFrames);while(film.MoveNext())yield return film.Current;
                notes.Append("chief pilot ").Append(pilotSeed).Append(", chief over at tick ").Append(endTick).Append(", all filmed\n");
                done=true;
            }
            File.WriteAllText(Path.Combine(_capture,"trailer-montage.txt"),notes.ToString());
            Check(done,"the tutorial, the road, a road fight, the chief captured, his band joining, a mixed fight and an ambush");
        }

        // The squad the squad page would start with, as TakeOffer builds it.
        static List<string> MontageSquad(Campaign c,int offer)
        {
            var ids=new List<string>();
            foreach(var f in c.DefaultSquad(c.Offers[offer].SquadSize))ids.Add(f.Id);
            return ids;
        }

        // The fight now on screen, played by the reference pilot with this seed, one tick a frame.
        void MontagePilot(ulong seed)
        {
            _pilot=new CampaignPilot(PilotStyle.Reference,seed);_pilotOn=true;_paused=false;_speed=1;
        }

        // This many frames of the fight on screen, or fewer if it ends first.
        IEnumerator MontageFight(string name,string encoderPath,int frames)
        {
            var encoder=TrailerEncoder(encoderPath,name);
            int n=0;
            try{for(;n<frames&&!_fight.Over;n++){TrailerFrame(encoder);yield return null;}}
            finally{TrailerEncoderDone(encoder,name);}
            Log(name+": "+n+" frames, to tick "+_fight.Sim.CurrentTick);
        }

        // The fight on screen to its end and MontageAfterEnd frames past it.
        IEnumerator MontageToEnd(string name,string encoderPath)
        {
            var encoder=TrailerEncoder(encoderPath,name);
            int n=0,after=0;
            try
            {
                while(n<4000)
                {
                    TrailerFrame(encoder);n++;
                    if(_fight.Over&&after++>=MontageAfterEnd)break;
                    yield return null;
                }
            }
            finally{TrailerEncoderDone(encoder,name);}
            Log(name+": "+n+" frames, over at tick "+_fight.Sim.CurrentTick);
        }

        // The rest of the fight on screen, fast and unfilmed, then its result through its own button.
        IEnumerator MontageFinish()
        {
            _speed=24;
            for(int guard=0;guard<4000&&!_fight.Over;guard++)yield return null;
            Check(_fight.Over,"the fight plays out");
            for(int i=0;i<76;i++)yield return null;
            Check(Click("see-result"),"the result through its own button");yield return null;
        }

        // The road page, the selection walking from node to node; with a node given, it ends on that node.
        IEnumerator MontageRoad(string name,string encoderPath,int last)
        {
            var encoder=TrailerEncoder(encoderPath,name);
            int count=_campaign.Offers.Count;
            try
            {
                for(int i=0;i<MontageRoadFrames;i++)
                {
                    if(i%24==0&&count>0)
                    {
                        int pick=(i/24)%count;
                        if(last>=0&&i>=MontageRoadFrames-48)pick=last;
                        SelectControl("offer-"+pick);
                    }
                    TrailerFrame(encoder);yield return null;
                }
            }
            finally{TrailerEncoderDone(encoder,name);}
            Log(name+": "+MontageRoadFrames+" frames over "+count+" nodes");
        }
    }
}
