using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // The accepted burial court is packaged with the player. An explicit study path remains a
        // review override; ordinary fights and recorded fights need no command-line art flag.
        readonly List<Texture2D> _bloodridgeTextures=new List<Texture2D>();
        readonly Dictionary<Figure,RawImage> _bloodridgeContacts=new Dictionary<Figure,RawImage>();
        RawImage _bloodridgeLinen,_bloodridgeDust;
        Texture2D _bloodridgeDustA,_bloodridgeDustB;
        bool _bloodridgeActive;
        string BloodridgeDirectory=>Argument("-ksBloodridgeReview")??
            Path.Combine(Application.streamingAssetsPath,"CampaignArt","Bloodridge");
        bool UseBloodridgeCourt=>!UseRegionalStage&&Argument("-ksBloodridgeBaseline")==null&&
            ViewKind!=EncounterKind.Tutorial&&ViewTribe==CampaignTribes.BloodridgeId;

        Texture2D BloodridgeTexture(string file)
        {
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            _bloodridgeTextures.Add(texture);
            if(!texture.LoadImage(File.ReadAllBytes(Path.Combine(BloodridgeDirectory,file)),true))
                throw new InvalidOperationException("Cannot load Bloodridge layer: "+file);
            texture.name=file;texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
            return texture;
        }
        RectTransform BloodridgeClip(string name)
        {
            var clip=Rect(_stage,name,StageX*Px,StageY*Px,StageW*Px,StageH*Px);
            clip.gameObject.AddComponent<RectMask2D>();return clip;
        }
        RawImage BloodridgeLayer(Transform parent,string file,int x,int y,int width,int height)
        {
            var image=Raw(parent,file,x-StageX*Px,y-StageY*Px,width,height);
            image.texture=BloodridgeTexture(file);return image;
        }
        void BuildBloodridgeReviewRear(RawImage original)
        {
            _bloodridgeActive=UseBloodridgeCourt;
            if(!_bloodridgeActive)return;
            original.enabled=false;
            var rear=BloodridgeClip("Bloodridge - floor and architecture");
            BloodridgeLayer(rear,"ground-underpaint.png",0,0,1440,900);
            BloodridgeLayer(rear,"rear-burial-architecture.png",0,0,1440,900);
            BloodridgeLayer(rear,"rooted-entrance.png",0,0,1440,900);
            _bloodridgeDust=BloodridgeLayer(rear,"ks-bloodridge-court-restrained-ash-review-1.png",208,140,1024,600);
            _bloodridgeDustA=(Texture2D)_bloodridgeDust.texture;
            _bloodridgeDustB=BloodridgeTexture("ks-bloodridge-court-restrained-ash-review-2.png");
        }
        void BuildBloodridgeReviewFront()
        {
            if(!_bloodridgeActive&&!_regionalActive)return;
            // A small pixel contact follows each actual ground anchor, including hit recoil. No captured feet.
            var shadow=new Texture2D(48,12,TextureFormat.RGBA32,false);
            var pixels=new Color32[48*12];
            for(int y=0;y<12;y++)for(int x=0;x<48;x++)
            {
                float d=(x-23.5f)*(x-23.5f)/(24*24)+(y-5.5f)*(y-5.5f)/(6*6);
                pixels[y*48+x]=new Color32(24,19,22,(byte)(d<.48f?104:d<.78f?68:d<1?34:0));
            }
            shadow.SetPixels32(pixels);shadow.Apply(false,true);shadow.filterMode=FilterMode.Point;
            _bloodridgeTextures.Add(shadow);
            foreach(var figure in _figures)
            {
                var contact=Raw(figure.root,"Live ground contact",-24,-6,48,12);
                contact.texture=shadow;contact.transform.SetAsFirstSibling();
                _bloodridgeContacts.Add(figure,contact);
            }
            if(_regionalActive){BuildRegionalStageFront();return;}
            // Near scenery below every bar, targeting label and combat number.
            var near=BloodridgeClip("Bloodridge - near roots and linen");
            BloodridgeLayer(near,"foreground.png",0,0,1440,900);
            _bloodridgeLinen=BloodridgeLayer(near,"linen-sheet.png",1186,515,226,562);
            DrawBloodridgeWind();
        }
        void DrawBloodridgeContact(Figure figure)
        {
            if(!_bloodridgeContacts.TryGetValue(figure,out RawImage contact))return;
            float fade=figure.unit.Alive?1:Mathf.Clamp01(1-(figure.poseTime-figure.deathAt)/1.5f);
            contact.color=new Color(1,1,1,fade);
        }
        static void BloodridgeSheetFrame(RawImage image,int frame,int columns,int width,int height)
        {
            image.uvRect=new UnityEngine.Rect(frame%columns*width/(float)image.texture.width,
                1-(frame/columns+1)*height/(float)image.texture.height,
                width/(float)image.texture.width,height/(float)image.texture.height);
        }
        void DrawBloodridgeWind()
        {
            if(!_bloodridgeActive||_bloodridgeLinen==null)return;
            int frame=Mathf.FloorToInt(_visualTime*8)%32;
            BloodridgeSheetFrame(_bloodridgeLinen,frame,8,226,562);
            _bloodridgeDust.texture=frame<24?_bloodridgeDustA:_bloodridgeDustB;
            BloodridgeSheetFrame(_bloodridgeDust,frame<24?frame:frame-24,6,1024,600);
        }
        void ClearBloodridgeReview()
        {
            foreach(var texture in _bloodridgeTextures)if(texture!=null)Destroy(texture);
            _bloodridgeTextures.Clear();_bloodridgeContacts.Clear();
            _bloodridgeLinen=_bloodridgeDust=null;_bloodridgeDustA=_bloodridgeDustB=null;_bloodridgeActive=false;
        }

        IEnumerator RecordBloodridgeReview()
        {
            yield return null;
            Campaign ready=null;
            for(ulong seed=1;seed<=20&&ready==null;seed++)
            {
                var campaign=Campaign.New(seed);
                if(!CampaignPilot.PlayTutorial(campaign,PilotStyle.Reference,seed,10))continue;
                var picker=new CampaignPilot(PilotStyle.Reference,seed);
                for(int run=0;run<3&&ready==null;run++)
                {
                    campaign.StartRun();
                    for(int n=0;n<16&&campaign.Stage==CampaignStage.InRun;n++)
                    {
                        var fight=campaign.StartEncounter(picker.PickOffer(campaign));
                        if(fight.Encounter.Tribe.Id==CampaignTribes.BloodridgeId&&fight.Encounter.Kind==EncounterKind.Chief)
                        {ready=campaign;break;}
                        CampaignPilot.PlayFight(campaign,fight,new CampaignPilot(PilotStyle.Reference,seed*31+(ulong)n));
                    }
                }
            }
            Check(ready!=null,"moving review reaches Bloodridge chief through real campaign progression");
            _campaign=ready;_fight=ready.Active;BeginFight();_speed=1;_pilotOn=false;
            File.WriteAllText(Path.Combine(_capture,"start-state.txt"),CampaignSave.Write(_campaign));
            var reference=CampaignSave.Read(CampaignSave.Write(_campaign)).Active;
            Check(reference!=null,"independent resumed simulation for every captured tick");
            bool baseline=Argument("-ksBloodridgeBaseline")!=null;
            Check(_bloodridgeActive!=baseline,"burial court is the default Bloodridge stage; legacy comparison is explicit");
            Check(_stageArt==null||baseline,"normal Bloodridge does not load the rejected tiled stage");
            CheckStagePresentation();
            Log("Bloodridge layers: "+BloodridgeDirectory);
            var layout=new StringBuilder("id,x,y,head,art\n");
            foreach(var f in _figures)layout.Append(f.unit.Id).Append(',').Append(f.feet.x).Append(',').Append(f.feet.y).Append(',').Append(f.head).Append(',').Append(f.artKey).Append('\n');
            File.WriteAllText(Path.Combine(_capture,"formations.csv"),layout.ToString());
            var trace=new StringBuilder("frame,tick,windFrame,events,numbers,attacks,alive\n");
            bool pauseChecked=false,targetChecked=false,sawAttack=false,sawNumbers=false;
            int frames=0,maxNumbers=0;
            System.Diagnostics.Process encoder=null;
            try
            {
                string encoderPath=Argument("-ksReviewEncoder");
                if(!baseline&&encoderPath!=null)
                {
                    encoder=new System.Diagnostics.Process();
                    // The MP4 is for watching. The FFV1 copy is every frame exactly as drawn, native 1440x900:
                    // the lossless master trailers are cut from (16 Oct 2026).
                    encoder.StartInfo=new System.Diagnostics.ProcessStartInfo(encoderPath,
                        "-y -loglevel error -f rawvideo -pixel_format rgb24 -video_size 1440x900 -framerate 30 -i pipe:0 -an -filter_complex [0:v]vflip,split=2[watch][master] -map [watch] -c:v libx264 -preset fast -crf 14 -pix_fmt yuv420p -movflags +faststart \""+Path.Combine(_capture,"bloodridge-moving-fight.mp4")+"\" -map [master] -c:v ffv1 -pix_fmt bgr0 \""+Path.Combine(_capture,"bloodridge-moving-fight-lossless.mkv")+"\"")
                    {UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardError=true};
                    encoder.Start();
                }
                for(int i=0;i<3600;i++)
                {
                    if(i==60||i==180||i==360||i==540)
                    {
                        int order=i==60?0:i==180?2:i==360?3:1;
                        if(!_fight.Over&&_buttons["order-"+order].interactable)
                        {
                            Check(Click("order-"+order),"real HUD order raycast at frame "+i);
                            string target=null;
                            if(order==2)
                            {
                                foreach(var f in _figures)if(f.unit.Side==1&&f.unit.Alive&&f.unit.Row==Row.Front){target=f.unit.Id;break;}
                                Check(target!=null&&Click("target-"+target),"live fighter target raycast through scenery");targetChecked=true;
                            }
                            Check(reference.TryOrder(Orders[order],target,out _),"reference receives the same HUD command");
                        }
                    }
                    if(i==120)
                    {
                        Check(Click("pause"),"pause control remains clickable");
                        long tick=_fight.Sim.CurrentTick;float clock=_visualTime;
                        var uv=_bloodridgeLinen==null?default(UnityEngine.Rect):_bloodridgeLinen.uvRect;
                        for(int p=0;p<4;p++)yield return null;
                        Check(tick==_fight.Sim.CurrentTick&&clock==_visualTime&&(_bloodridgeLinen==null||uv==_bloodridgeLinen.uvRect),"pause freezes fight, cloth and ash together");
                        Capture("paused");Check(Click("pause"),"resume control remains clickable");pauseChecked=true;
                    }
                    yield return null;
                    while(reference.Sim.CurrentTick<_fight.Sim.CurrentTick&&!reference.Over)reference.Tick();
                    if(reference.Sim.OrderedStateSnapshot!=_fight.Sim.OrderedStateSnapshot)
                        throw new InvalidOperationException("Scenery changed simulation at frame "+i);
                    int attacks=0,alive=0;
                    foreach(var f in _figures)
                    {
                        if(f.unit.Alive)alive++;
                        if(f.drawnClip==f.attack)attacks++;
                        if(f.body!=null&&f.drawnClip!=null&&f.body.rectTransform.sizeDelta!=new Vector2(f.drawnClip.sheet.frames[f.drawnFrame].frame.w,f.drawnClip.sheet.frames[f.drawnFrame].frame.h))
                            throw new InvalidOperationException("Fighter left native 1x size");
                    }
                    sawAttack|=attacks>0;sawNumbers|=_numbers.Count>0;maxNumbers=Math.Max(maxNumbers,_numbers.Count);
                    trace.Append(i).Append(',').Append(_fight.Sim.CurrentTick).Append(',').Append(Mathf.FloorToInt(_visualTime*8)%32).Append(',').Append(_fight.Sim.Events.Count).Append(',').Append(_numbers.Count).Append(',').Append(attacks).Append(',').Append(alive).Append('\n');
                    if(i==0||i==60||i==180||i==240||i==360||i==540||i==720||i%300==0)Capture("frame-"+i.ToString("D4"));
                    if(encoder!=null)
                    {
                        Canvas.ForceUpdateCanvases();Camera.main.Render();var previous=RenderTexture.active;RenderTexture.active=_captureTarget;
                        _capturePixels.ReadPixels(new UnityEngine.Rect(0,0,W,H),0,0);_capturePixels.Apply();RenderTexture.active=previous;
                        byte[] data=_capturePixels.GetRawTextureData();encoder.StandardInput.BaseStream.Write(data,0,data.Length);
                    }
                    frames++;
                    if(_fight.Over&&_endAge>=3)break;
                }
            }
            finally
            {
                if(encoder!=null)
                {
                    encoder.StandardInput.Close();string errors=encoder.StandardError.ReadToEnd();encoder.WaitForExit();
                    int code=encoder.ExitCode;encoder.Dispose();
                    File.WriteAllText(Path.Combine(_capture,"encoder.txt"),"exit="+code+"\n"+errors);
                    if(code!=0)throw new InvalidOperationException("Review encoder failed: "+errors);
                }
            }
            Capture("ending");
            File.WriteAllText(Path.Combine(_capture,"motion-trace.csv"),trace.ToString());
            File.WriteAllText(Path.Combine(_capture,"end-state.txt"),CampaignSave.Write(_campaign));
            Check(_fight.Over,"moving review captures the complete authoritative fight and three seconds of settling");
            Check(sawAttack&&sawNumbers&&pauseChecked&&targetChecked,"actual attacks, numbers, pause and live targeting exercised");
            Check(_stage.anchoredPosition==Vector2.zero,"stage shake settles; HUD remains on its original parent");
            Check(_bloodridgeTextures.Count==0||_bloodridgeTextures.Count==8,"review texture lifetime is bounded");
            Log("Moving review: "+frames+" frames at 30 fps; max simultaneous numbers="+maxNumbers+"; save="+_savePath);
            // Other regions and the tutorial retain their own stage and release these textures.
            _campaign=Campaign.New(21);_fight=_campaign.StartTutorial();BeginFight();_paused=true;
            Check(!_bloodridgeActive&&_bloodridgeTextures.Count==0,"leaving Bloodridge releases review textures and keeps tutorial stage unchanged");
            yield return null;Capture("unaffected-tutorial");
        }
    }
}
