using System;
using System.Collections;
using System.IO;
using System.Linq;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Two fixed fights, no pilot, tuning or searches. Images alone cannot prove the layer ordering.
        IEnumerator ValidateRegionalStages()
        {
            string fixtures=Argument("-ksRegionalStageValidate");
            _savePath=Path.Combine(_capture,"isolated-stage-save.txt");
            foreach(string region in new[]{"skullpeak","hunters"})
            {
                _playback=null;_fight=null;
                _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,region=="skullpeak"?"skullpeak-duel.txt":"fixture-hunt-win.txt")));
                int offer=region=="hunters"?0:_campaign.Offers.ToList().FindIndex(e=>e.Kind==EncounterKind.WarParty);
                Check(offer>=0,region+" fixed save offers a representative squad fight");
                _fight=_campaign.StartEncounter(offer);BeginFight();_pilotOn=false;_paused=true;_speed=1;
                Save();File.Copy(_savePath,Path.Combine(_capture,region+"-start-save.txt"),true);
                var reference=CampaignSave.Read(CampaignSave.Write(_campaign)).Active;
                yield return null;DrawBattle();Capture(region+"-start");
                Check(_regionalActive&&!_bloodridgeActive&&_stageArt==null,region+" selects only the opt-in layered stage");
                Check(_bloodridgeContacts.Count==_figures.Count,region+" every actual fighter has a contact shadow");
                Check(_bloodridgeTextures.Count==6,region+" loads three static layers, two existing atmosphere sheets and one contact texture");
                foreach(var texture in _bloodridgeTextures)Check(texture.filterMode==FilterMode.Point&&texture.mipmapCount==1,region+" native texture "+texture.name);
                foreach(var figure in _figures)
                {
                    Check(_regionalRear.GetSiblingIndex()<figure.root.GetSiblingIndex()&&_regionalFront.GetSiblingIndex()>figure.root.GetSiblingIndex(),region+" fighter sorted between rear and foreground: "+figure.unit.Id);
                    Check(figure.readoutRoot.parent.GetSiblingIndex()>_regionalFront.GetSiblingIndex(),region+" readouts remain above scenery: "+figure.unit.Id);
                }
                var uv=_regionalDust.uvRect;var dust=_regionalDust.texture;float clock=_visualTime;long tick=ViewSim.CurrentTick;
                for(int i=0;i<5;i++)yield return null;
                Check(tick==ViewSim.CurrentTick&&clock==_visualTime&&uv==_regionalDust.uvRect&&dust==_regionalDust.texture,region+" pause freezes fight and atmosphere together");
                Check(Click("pause"),region+" resume button raycasts");
                bool sawAttack=false,sawLunge=false,sawWind=false;
                var encoder=new System.Diagnostics.Process();
                encoder.StartInfo=new System.Diagnostics.ProcessStartInfo(Argument("-ksReviewEncoder"),
                    "-y -loglevel error -f rawvideo -pixel_format rgb24 -video_size 1440x900 -framerate 30 -i pipe:0 -an -vf vflip -c:v libx264 -preset fast -crf 14 -pix_fmt yuv420p -movflags +faststart \""+Path.Combine(_capture,region+"-moving.mp4")+"\"")
                    {UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardError=true};
                Time.captureFramerate=30;
                try
                {
                    encoder.Start();
                    for(int i=0;i<420;i++)
                    {
                        if(i==60||i==120||i==210||i==300)
                        {
                            int order=i==60?0:i==120?2:i==210?3:1;
                            if(!_fight.Over&&_buttons["order-"+order].interactable)
                            {
                                Check(Click("order-"+order),region+" order control raycasts at frame "+i);
                                string target=null;
                                if(order==2)
                                {
                                    target=_figures.First(f=>f.unit.Side==1&&f.unit.Alive).unit.Id;
                                    Check(Click("target-"+target),region+" targets a live fighter through scenery");
                                }
                                Check(reference.TryOrder(Orders[order],target,out _),region+" reference receives identical order");
                            }
                        }
                        yield return null;
                        while(reference.Sim.CurrentTick<ViewSim.CurrentTick&&!reference.Over)reference.Tick();
                        if(reference.Sim.OrderedStateSnapshot!=ViewSim.OrderedStateSnapshot)throw new InvalidOperationException(region+" scenery altered simulation at frame "+i);
                        foreach(var f in _figures)
                        {
                            sawAttack|=f.drawnClip==f.attack;
                            sawLunge|=Mathf.Abs(f.root.anchoredPosition.x-f.feet.x)>1;
                            if(f.body!=null&&f.drawnClip!=null)
                            {
                                var frame=f.drawnClip.sheet.frames[f.drawnFrame].frame;
                                if(f.body.rectTransform.sizeDelta!=new Vector2(frame.w,frame.h))throw new InvalidOperationException("Fighter resampled");
                            }
                        }
                        sawWind|=_regionalDust.uvRect!=uv||_regionalDust.texture!=dust;
                        if(i==60||i==180||i==300)Capture(region+"-frame-"+i);
                        Canvas.ForceUpdateCanvases();Camera.main.Render();var previous=RenderTexture.active;RenderTexture.active=_captureTarget;
                        _capturePixels.ReadPixels(new UnityEngine.Rect(0,0,W,H),0,0);_capturePixels.Apply();RenderTexture.active=previous;
                        byte[] pixels=_capturePixels.GetRawTextureData();encoder.StandardInput.BaseStream.Write(pixels,0,pixels.Length);
                        if(_fight.Over&&_endAge>=2)break;
                    }
                }
                finally
                {
                    Time.captureFramerate=0;_paused=true;
                    encoder.StandardInput.Close();string errors=encoder.StandardError.ReadToEnd();encoder.WaitForExit();
                    int code=encoder.ExitCode;encoder.Dispose();
                    File.WriteAllText(Path.Combine(_capture,region+"-encoder.txt"),"exit="+code+"\n"+errors);
                    if(code!=0)throw new InvalidOperationException(errors);
                }
                Check(sawAttack&&sawLunge,region+" actual attacks and positional lunges render at native scale");
                Check(sawWind,region+" existing atmosphere clip advances");
                Check(ViewSim.OrderedStateSnapshot==reference.Sim.OrderedStateSnapshot,region+" matches independent simulation throughout moving capture");
                Save();string saved=ViewSim.OrderedStateSnapshot;
                _campaign=CampaignSave.Read(File.ReadAllText(_savePath));ResumeFight();_paused=true;_pilotOn=false;yield return null;
                Check(ViewSim.OrderedStateSnapshot==saved&&_regionalActive,region+" isolated save reload retains state and regional binding");
                Capture(region+"-reload");
                _fight=null;SetPage(Page.ChooseTribe);yield return null;
                Check(!_regionalActive&&_regionalDust==null&&_bloodridgeTextures.Count==0&&_bloodridgeContacts.Count==0,region+" leaving fight releases layers and contacts");
            }
            // Existing Bloodridge and tutorial selection remain separate from the two review regions.
            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"parry-0.txt")));
            _campaign.StartRun();_fight=_campaign.StartEncounter(0);BeginFight();_pilotOn=false;_paused=true;yield return null;
            CheckStagePresentation();Capture("bloodridge-unchanged");
            _fight=null;_campaign=Campaign.New(21);_fight=_campaign.StartTutorial();BeginFight();_pilotOn=false;_paused=true;yield return null;
            CheckStagePresentation();Capture("tutorial-unchanged");
        }
    }
}
