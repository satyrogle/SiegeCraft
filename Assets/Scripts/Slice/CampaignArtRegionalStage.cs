using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Opt-in regional art review. No default stage or simulation changes.
        string RegionalStageKey=>Argument("-ksRegionalStageReview")==null?null:
            HunterFight(ViewKind)?"hunters":ViewTribe==KindredSiege.Sim.Learning.CampaignTribes.SkullpeakId?"skullpeak":null;
        bool UseRegionalStage=>RegionalStageKey!=null;
        bool _regionalActive;
        RectTransform _regionalRear,_regionalFront;
        RawImage _regionalDust;
        Texture2D _regionalDustA,_regionalDustB;

        RawImage RegionalLayer(Transform parent,string file)
        {
            string folder=Argument("-ksRegionalStageReview");
            if(folder=="packaged")folder=Path.Combine(Application.streamingAssetsPath,"CampaignArt","RegionalReview");
            string path=Path.Combine(folder,RegionalStageKey,file);
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            _bloodridgeTextures.Add(texture);
            if(!texture.LoadImage(File.ReadAllBytes(path),true)||texture.width!=StageW*Px||texture.height!=StageH*Px)
                throw new InvalidOperationException("Regional layer must retain native stage size: "+path);
            texture.name=RegionalStageKey+"/"+file;texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
            var image=Raw(parent,file,0,0,StageW*Px,StageH*Px);image.texture=texture;return image;
        }
        void BuildRegionalStageRear(RawImage original)
        {
            _regionalActive=UseRegionalStage;
            if(!_regionalActive)return;
            original.enabled=false;
            _regionalRear=BloodridgeClip("Regional - ground and rear scenery");
            RegionalLayer(_regionalRear,"ground.png");RegionalLayer(_regionalRear,"rear.png");
            // Rebind the existing atmosphere clip. Confine it to the rear scenery, clear of fighting lanes.
            var atmosphere=Rect(_regionalRear,"Distant atmosphere only",0,0,StageW*Px,285);
            atmosphere.gameObject.AddComponent<RectMask2D>();
            _regionalDust=Raw(atmosphere,"Existing restrained dust",176,-180,1024,600);
            _regionalDustA=BloodridgeTexture("ks-bloodridge-court-restrained-ash-review-1.png");
            _regionalDustB=BloodridgeTexture("ks-bloodridge-court-restrained-ash-review-2.png");
            _regionalDust.texture=_regionalDustA;
            _regionalDust.color=RegionalStageKey=="skullpeak"?new Color(.82f,.91f,1,.38f):new Color(.88f,.82f,.92f,.3f);
            DrawRegionalWind();
        }
        void BuildRegionalStageFront()
        {
            _regionalFront=BloodridgeClip("Regional - near edge framing");
            RegionalLayer(_regionalFront,"foreground.png");
        }
        void DrawRegionalWind()
        {
            if(!_regionalActive||_regionalDust==null)return;
            int frame=Mathf.FloorToInt(_visualTime*8)%32;
            _regionalDust.texture=frame<24?_regionalDustA:_regionalDustB;
            BloodridgeSheetFrame(_regionalDust,frame<24?frame:frame-24,6,1024,600);
        }
        void ClearRegionalStage()
        {
            // Shared contact/texture ownership is cleared by ClearBloodridgeReview first.
            _regionalActive=false;_regionalRear=_regionalFront=null;
            _regionalDust=null;_regionalDustA=_regionalDustB=null;
        }
    }
}
