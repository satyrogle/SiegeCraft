using KindredSiege.Sim.Battle;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Presentation only. Each participant can hold its pose while the authoritative fight,
        // other fighters, order buttons and numbers continue on their existing clock.
        bool _combatFeedback=true;
        const float FrontLungePixels=48,RearLungePixels=12;
        const float StrikeHoldSeconds=.08f,StrikeReturnSeconds=.28f;
        float _shakeAt=-100, _lastShakeAt=-100;
        float _lastWhiteAt=-100;
        const float WhiteSeconds=1f/30f, WhiteStrength=.08f;
        static readonly int HitWhite=Shader.PropertyToID("_HitWhite");
        static readonly int BodyMask=Shader.PropertyToID("_BodyMask");
        static readonly int SpriteRect=Shader.PropertyToID("_SpriteRect");
        static readonly int MaskRect=Shader.PropertyToID("_MaskRect");
        static readonly int MaskMap=Shader.PropertyToID("_MaskMap");

        void LateUpdate()
        {
            SoundPauseTick();
            // Pointer buttons can pause after this component's Update. Clear before rendering too,
            // so that first paused frame is readable regardless of EventSystem update ordering.
            if(!_paused||(_page!=Page.Fight&&_page!=Page.Playback))return;
            foreach(var f in _figures)
            {
                f.whiteAt=f.flashAt=-100;
                f.majorWhiteUntil=-100;
                if(f.hitMaterial!=null)f.hitMaterial.SetFloat(HitWhite,0);
                if(f.body!=null)f.body.color=Color.white;
            }
        }

        void PrepareCombatFeedback(Figure f)
        {
            if(f.body==null)return;
            var shader=Resources.Load<Shader>("CampaignArt/UI/CombatHitFlash");
            if(shader==null)throw new System.InvalidOperationException("Combat hit flash shader missing");
            f.hitMaterial=new Material(shader);
            f.hitMaterial.SetTexture(BodyMask,f.idle.texture);
            f.body.material=f.hitMaterial;
        }

        void AdvanceCombatPoses(float dt)
        {
            foreach(var f in _figures)
            {
                float held=Mathf.Min(dt,f.poseHold);
                f.poseHold-=held;
                if(f.poseHold<=0)f.majorFreeze=false;
                f.poseTime+=dt-held;
            }
        }

        void Impact(BattleEvent e,Figure actor,Figure target)
        {
            if(!_combatFeedback||e.Kind!=BattleEventKind.Hit||e.Amount<=0||target==null)return;
            bool heavy=ClassSpells.TierOf(e.Detail)!=HitTier.Light||e.Amount>=Mathf.Max(1,target.unit.MaxHp/5);
            float hold=heavy?.075f:.05f;
            float direction=actor!=null?Mathf.Sign(target.feet.x-actor.feet.x):(target.unit.Side==0?-1:1);
            if(direction==0)direction=target.unit.Side==0?-1:1;
            if(actor!=null&&actor!=target)
            {
                actor.strikeAt=actor.poseTime;
                actor.strikePixels=(actor.unit.Row==Row.Front?FrontLungePixels:RearLungePixels)*direction;
                if(actor.poseHold<=0)actor.poseHold=hold;
            }
            target.recoilAt=target.poseTime;
            target.recoilPixels=(heavy?10:5)*direction;
            // Repeated hits must not extend one freeze into the next action.
            if(target.poseHold<=0)target.poseHold=hold;
            // One subtle flash at a time. Recoil/numbers still show every hit in a crowded exchange.
            if(!_paused&&_visualTime-_lastWhiteAt>=.12f)
            {target.whiteAt=_visualTime;_lastWhiteAt=_visualTime;}
            if(heavy&&_visualTime-_lastShakeAt>=.25f)_shakeAt=_lastShakeAt=_visualTime;
        }

        static float Settle(float age,float duration)
        {
            if(age<0||age>=duration)return 0;
            float left=1-age/duration;
            return left*left;
        }

        void DrawCombatFeedback(Figure f)
        {
            float age=f.poseTime-f.strikeAt;
            float strike=age>=0?f.strikePixels*Settle(Mathf.Max(0,age-StrikeHoldSeconds),StrikeReturnSeconds):0;
            float recoil=f.recoilPixels*Settle(f.poseTime-f.recoilAt,.23f);
            // A simultaneous incoming hit must not subtract away the attack's step-in. Let recoil
            // resume once the strike returns; every hit still has its number, bar chip and pose hold.
            float offset=_combatFeedback?(strike!=0?strike:recoil):0;
            if(ViewSim.RoadKitOn&&f.unit.Alive&&ViewSim.FallBackTicksLeft(f.unit.Side)>0)
                offset=f.unit.Side==0?-FrontLungePixels:FrontLungePixels;
            f.root.anchoredPosition=new Vector2(f.feet.x+Mathf.Round(Mathf.Clamp(offset,-FrontLungePixels,FrontLungePixels)),-f.feet.y);
            if(f.readoutRoot!=null)
                f.readoutRoot.anchoredPosition=new Vector2(Mathf.Round(f.root.anchoredPosition.x/Px)*Px,-f.feet.y);
            if(f.hitMaterial!=null)
            {
                if(_paused)f.whiteAt=-100;
                float whiteAge=_visualTime-f.whiteAt;
                float white=_visualTime<f.majorWhiteUntil?1: f.unit.Alive&&whiteAge>=0&&whiteAge<WhiteSeconds?WhiteStrength:0;
                f.hitMaterial.SetFloat(HitWhite,_combatFeedback&&!_paused?white:0);
            }
        }

        void SetFlashMask(Figure f,Clip c,FrameRect frame)
        {
            // Use the approved resting body silhouette, aligned by ground anchors. Baked strike
            // shapes outside the body are never tinted; bright effect pixels are excluded in shader.
            var idle=f.idle;var mask=idle.sheet.frames[0].frame;
            float mw=idle.texture.width,mh=idle.texture.height;
            var uv=f.body.uvRect;
            f.hitMaterial.SetVector(SpriteRect,new Vector4(uv.x,uv.y,uv.width,uv.height));
            f.hitMaterial.SetVector(MaskRect,new Vector4(mask.x/mw,1-(mask.y+mask.h)/mh,mask.w/mw,mask.h/mh));
            f.hitMaterial.SetVector(MaskMap,new Vector4(frame.w/mw,frame.h/mh,
                (mask.x+idle.sheet.anchorX-c.sheet.anchorX)/mw,
                1-(mask.y+frame.h+idle.sheet.anchorY-c.sheet.anchorY)/mh));
            // Attack effects remain entirely untouched, even if a fighter is hit mid-swing.
            if(c==f.attack)f.hitMaterial.SetFloat(HitWhite,0);
        }

        void DrawCombatShake()
        {
            float age=_visualTime-_shakeAt;Vector2 offset=Vector2.zero;
            if(_combatFeedback&&age>=0&&age<.12f)
            {
                int step=Mathf.FloorToInt(age/.024f);
                switch(step)
                {
                    case 0:offset=new Vector2(2,0);break;
                    case 1:offset=new Vector2(-2,1);break;
                    case 2:offset=new Vector2(1,-1);break;
                    case 3:offset=new Vector2(-1,0);break;
                }
            }
            _stage.anchoredPosition=offset;
        }
    }
}
