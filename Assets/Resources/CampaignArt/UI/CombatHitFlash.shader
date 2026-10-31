Shader "KindredSiege/UI/CombatHitFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _HitWhite ("Hit white", Range(0,1)) = 0
        _BodyMask ("Resting body silhouette", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            sampler2D _MainTex;
            sampler2D _BodyMask;
            float4 _SpriteRect, _MaskRect, _MaskMap;
            float _HitWhite;
            v2f vert(appdata v)
            {
                v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 source=tex2D(_MainTex,i.uv);
                fixed4 c=source*i.color;
                float2 local=(i.uv-_SpriteRect.xy)/_SpriteRect.zw;
                float2 maskUV=local*_MaskMap.xy+_MaskMap.zw;
                float2 inside=step(_MaskRect.xy,maskUV)*step(maskUV,_MaskRect.xy+_MaskRect.zw);
                float mask=inside.x*inside.y*step(.5,tex2D(_BodyMask,maskUV).a);
                // Preserve pale baked hit/weapon effects instead of turning them into a white blade.
                mask*=1-step(.65,max(source.r,max(source.g,source.b)));
                // Keep the darkest outline pixels dark, retaining the sprite's internal drawing.
                mask*=smoothstep(.015,.12,max(source.r,max(source.g,source.b)));
                c.rgb=lerp(c.rgb,fixed3(1,1,1),_HitWhite*mask);
                return c;
            }
            ENDCG
        }
    }
}
