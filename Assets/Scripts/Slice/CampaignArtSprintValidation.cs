using System;
using System.Collections;
using System.IO;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        IEnumerator ValidateSprint()
        {
            yield return null;
            Check(Click("new"),"new campaign through UI");yield return null;
            Check(Click("start-tutorial"),"tutorial through UI");yield return null;
            _paused=true;_pilotOn=false;RefreshFight();
            _soundSettings.muted=false;_soundSettings.volume=.65f;
            for(int i=0;i<6;i++)
            {
                var clip=_soundClips[i];var pcm=new float[clip.samples*clip.channels];clip.GetData(pcm,0);
                float peak=0;double energy=0;bool finite=true;
                foreach(float x in pcm){finite&=!float.IsNaN(x)&&!float.IsInfinity(x);peak=Mathf.Max(peak,Mathf.Abs(x));energy+=x*x;}
                Check(finite&&peak>.05f&&peak<.7f&&energy>1,"cue "+i+" has bounded non-silent PCM");
                using(var output=new BinaryWriter(File.Create(Path.Combine(_capture,((Cue)i)+".wav"))))
                {
                    output.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));output.Write(36+pcm.Length*2);
                    output.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));output.Write(16);output.Write((short)1);output.Write((short)clip.channels);
                    output.Write(clip.frequency);output.Write(clip.frequency*clip.channels*2);output.Write((short)(clip.channels*2));output.Write((short)16);
                    output.Write(System.Text.Encoding.ASCII.GetBytes("data"));output.Write(pcm.Length*2);
                    foreach(float x in pcm)output.Write((short)(x*32767));
                }
                _cueAt[i]=-100;PlayCue((Cue)i);Check(_cuePlayed[i]>0,"cue dispatch "+i);
                int voices=0;foreach(var source in _soundVoices)if(source.isPlaying)voices++;
                Check(voices>0&&voices<=4,"AudioSource plays with bounded voices "+i);
                yield return new WaitForSecondsRealtime(.8f);
            }
            int accepted=_cuePlayed[(int)Cue.Accepted];_cueAt[(int)Cue.Accepted]=-100;
            Order(Directive.Hold);Check(_cuePlayed[(int)Cue.Accepted]==accepted+1,"accepted order sounds from actual Directive event");
            int unavailable=_cuePlayed[(int)Cue.Unavailable];_cueAt[(int)Cue.Unavailable]=-100;
            // The tutorial may begin with every visible order available. Spend real points through
            // the order API until Unleash is genuinely gated; never assume a chief call is locked.
            for(int n=0;n<64&&_fight.Sim.CanUse(0,Directive.Unleash,out _);n++)Order(Directive.Unleash);
            Check(!_fight.Sim.CanUse(0,Directive.Unleash,out string reason)&&reason.Length>0,
                "real tutorial orders reach an unavailable Unleash state");
            int refusalPoints=_fight.Sim.DirectivePoints(0),refusalCommands=_fight.Session.Log.Commands.Count;
            Order(Directive.Unleash);
            Check(_cuePlayed[(int)Cue.Unavailable]==unavailable+1&&_fight.Sim.DirectivePoints(0)==refusalPoints&&_fight.Session.Log.Commands.Count==refusalCommands,
                "unavailable order sounds without spending or entering the command log");
            int points=_fight.Sim.DirectivePoints(0);SelectControl("order-3");HandleUiKey(KeyCode.Space);
            Check(!_paused&&_fight.Sim.DirectivePoints(0)==points,"Space does not issue selected order");HandleUiKey(KeyCode.Space);
            ShowSound();yield return null;Capture("01-sound");
            Check(Click("sound-mute"),"mute button raycast");yield return null;
            int count=_cuePlayed[0];_cueAt[0]=-100;PlayCue(Cue.Accepted);
            Check(_cuePlayed[0]==count,"mute suppresses dispatch");
            LoadSoundSettings();Check(_soundSettings.muted,"mute persists through settings reload");
            Check(Click("sound-volume"),"volume button raycast");yield return null;float volume=_soundSettings.volume;
            LoadSoundSettings();Check(Mathf.Approximately(volume,_soundSettings.volume),"volume persists through reload");
            Check(Click("sound-mute"),"unmute button raycast");yield return null;CloseModal();

            // All records are produced by Campaign, with no invented rewards or hand-built simulations.
            _fight=null;
            string fixedArchive=Argument("-ksSprintFixture");
            if(fixedArchive!=null)_campaign=CampaignSave.Read(File.ReadAllText(fixedArchive));
            else
            {
                _campaign=Campaign.New(21);
                Check(CampaignPilot.PlayTutorial(_campaign,PilotStyle.Reference,21,10),"played tutorial archive fixture");
                var picker=new CampaignPilot(PilotStyle.Reference,21);
                for(int run=0;run<3;run++)
                {
                    _campaign.StartRun();
                    for(int n=0;n<12&&_campaign.Stage==CampaignStage.InRun;n++)
                    {
                        var f=_campaign.StartEncounter(picker.PickOffer(_campaign));
                        CampaignPilot.PlayFight(_campaign,f,new CampaignPilot(PilotStyle.Reference,21*31UL+(ulong)n));
                    }
                }
            }
            SetPage(Page.ChooseTribe);Save();string before=CampaignSave.Write(_campaign);
            File.WriteAllText(Path.Combine(_capture,"playback-before.txt"),before);
            ArchivedFight chief=null;
            foreach(var record in _campaign.Archive.Fights)if(record.FightId.EndsWith(".chief",StringComparison.Ordinal)){chief=record;break;}
            Check(chief!=null,"campaign reaches archived chief");
            ShowArchivedFight(chief);yield return null;Capture("02-history");
            Check(Click("replay-watch"),"Watch Fight button raycast");yield return null;
            Check(_page==Page.Playback&&_paused&&_playback.Tick==0,"replay opens paused at first tick");
            CheckStagePresentation();
            foreach(var figure in _figures)
            {
                string saved=chief.NameOf(figure.unit.Id);
                if(saved.Length>0)Check(UnitName(figure.unit).Contains(saved),"historical name kept: "+saved);
                if(!figure.unit.Id.EndsWith(".champion")&&!figure.unit.Id.EndsWith(".shaman"))Check(figure.body!=null,"known chief fight art bound: "+figure.unit.Id);
            }
            Order(Directive.Unleash);Check(_playback.Tick==0&&CampaignSave.Write(_campaign)==before,"replay refuses live orders without changing campaign");
            long tick=_playback.Tick;for(int i=0;i<4;i++)yield return null;
            Check(_playback.Tick==tick,"paused replay holds tick");
            Check(Click("pause"),"replay Play raycast");
            for(int i=0;i<180;i++){yield return null;if(i%2==0)Capture("replay-"+i.ToString("D4"));}
            Check(_playback.Tick>0,"rendered playback advances via Step");
            Check(Click("pause"),"replay Pause raycast");tick=_playback.Tick;
            for(int i=0;i<4;i++)yield return null;Check(_playback.Tick==tick,"replay pause freezes tick and visuals");
            Check(Click("speed"),"replay speed raycast");Check(_speed==2,"replay speed updates");
            Check(Click("playback-prev"),"back five seconds raycast");yield return null;Check(_playback.Tick==Math.Max(0,tick-5*SimClock.TicksPerSecond),"backward seek lands exactly");
            foreach(var f in _figures)Check(ReferenceEquals(f.unit,_playback.Sim.FindById(f.unit.Id)),"seek rebuilt current unit references");
            Check(Click("playback-next"),"forward five seconds raycast");yield return null;
            int casts=_cuePlayed[(int)Cue.Cast],breaks=_cuePlayed[(int)Cue.Break],shields=_cuePlayed[(int)Cue.Shield];
            _speed=4;_paused=false;int playGuard=0;
            while(!_playback.AtEnd&&_playback.Failure.Length==0&&playGuard++<1800)yield return null;
            Check(_playback.AtEnd&&_playback.Matched,"continuous playback reaches verified end");
            Check(_cuePlayed[(int)Cue.Cast]>casts&&_cuePlayed[(int)Cue.Break]>breaks&&_cuePlayed[(int)Cue.Shield]>shields,"real replay events sound cast warning nerve break and shield hits");
            SeekPlayback(0);yield return null;
            int soundsBefore=0;foreach(int played in _cuePlayed)soundsBefore+=played;
            Check(Click("playback-end"),"end seek raycast");yield return null;
            int soundsAfter=0;foreach(int played in _cuePlayed)soundsAfter+=played;
            Check(soundsBefore==soundsAfter,"seek does not burst historical audio");
            Check(_playback.AtEnd&&_playback.Matched,"rendered chief replay matches checksum and event history");Capture("03-replay-verified");
            Check(Click("playback-start"),"restart raycast");yield return null;Check(_playback.Tick==0&&!_playback.Matched,"restart clears end state");
            Check(Click("playback-back"),"back from playback raycast");
            Check(_page==Page.ChooseTribe&&_modal!=null&&_playback==null,"back restores camp and archived fight panel");
            Check(CampaignSave.Write(_campaign)==before,"watching seeking and returning changes no campaign byte");
            Check(CampaignSave.TryReadFile(_savePath,null,out Campaign disk,out _)&&CampaignSave.Write(disk)==before,"watching does not alter saved campaign");
            var clue=RematchClue.For(_campaign,CampaignTribes.BloodridgeId);
            Check(clue!=null&&clue.Source!=null,"lesson source exists");ShowRematch(clue);yield return null;
            Check(Click("lesson-watch"),"Watch Moment raycast");
            Check(_playback.Tick==clue.Lesson.FromTick,"lesson seeks to recorded start");Capture("04-lesson-moment");
            _speed=4;_paused=false;int guard=0;
            while(!_paused&&!_playback.AtEnd&&guard++<1800)yield return null;
            Check(_playback.Tick==clue.Lesson.ToTick||_playback.AtEnd,"lesson stops at recorded end");LeavePlayback();
            foreach(var record in _campaign.Archive.Fights)
            {
                WatchFight(record);SeekPlayback(record.Ticks);
                CheckStagePresentation();
                Check(_playback.Matched,"all archived encounter kinds render and verify: "+record.FightId);LeavePlayback();yield return null;
            }
            Check(CampaignSave.Write(_campaign)==before,"whole archive playback leaves campaign byte identical");
            File.WriteAllText(Path.Combine(_capture,"playback-after.txt"),CampaignSave.Write(_campaign));
            // A pre-names archive still plays, but must use role labels rather than today's roster names.
            var legacy=new System.Text.StringBuilder();
            foreach(string line in before.Split('\n'))if(!line.StartsWith("names\t",StringComparison.Ordinal))legacy.Append(line).Append('\n');
            _campaign=CampaignSave.Read(legacy.ToString());SetPage(Page.ChooseTribe);
            WatchFight(_campaign.Archive.Fights[0]);
            foreach(var f in _figures)Check(!string.IsNullOrEmpty(UnitName(f.unit)),"pre-names record has readable fallback");
            SeekPlayback(_playback.Length);Check(_playback.Matched,"pre-names archive still matches");LeavePlayback();
            Check(!FightPlayback.TryOpen(_campaign.Archive,"missing",out _,out string missing)&&missing.Length>0,"missing archive is refused with a reason");
        }
    }
}
