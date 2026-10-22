Shader "MiniGame/Pause Lettering"
{
    Properties
    {
        [PerRendererData] _MainTex ("Font atlas", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; float2 local:TEXCOORD1; };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize, _ClipRect;
            fixed4 _Color, _TextureSampleAdd;
            Output vert(Input input)
            {
                Output output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv; output.local = input.vertex.xy;
                output.color = input.color * _Color;
                return output;
            }
            fixed4 frag(Output input):SV_Target
            {
                // Static sub-texel wear, never animated: legibility and keyboard focus stay steady.
                float2 cell = floor(input.local * 1.4);
                float noise = frac(sin(dot(cell, float2(12.9898,78.233))) * 43758.5453);
                float2 d = abs(_MainTex_TexelSize.xy) * .65;
                float core = (tex2D(_MainTex, input.uv) + _TextureSampleAdd).a;
                float neighbors = tex2D(_MainTex, input.uv + float2(d.x, 0)).a
                    + tex2D(_MainTex, input.uv - float2(d.x, 0)).a
                    + tex2D(_MainTex, input.uv + float2(0, d.y)).a
                    + tex2D(_MainTex, input.uv - float2(0, d.y)).a;
                // The shared room font atlas is point sampled. A small coverage fringe keeps
                // scaled Song strokes from turning into harsh, disconnected white pixels.
                float alpha = saturate(core * .88 + neighbors * .04) * lerp(.97, 1, noise);
                fixed4 color = fixed4(input.color.rgb, input.color.a * alpha);
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.local, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - .001);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
