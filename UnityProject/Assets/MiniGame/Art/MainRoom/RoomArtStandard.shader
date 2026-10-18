Shader "MiniGame/Room Art Standard"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Glossiness ("Smoothness", Range(0, 1)) = 0.2
        _MetallicGlossMap ("Metallic (R) Smoothness (A)", 2D) = "white" {}
        _GlossMapScale ("Smoothness Scale", Range(0, 1)) = 1
        [Normal] _BumpMap ("Normal", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1
        [HDR] _EmissionColor ("Emission", Color) = (0, 0, 0, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull [_Cull]
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        #pragma shader_feature_local _NORMALMAP
        #pragma shader_feature_local _METALLICGLOSSMAP
        #include "UnityStandardUtils.cginc"

        sampler2D _MainTex, _BumpMap, _MetallicGlossMap;
        fixed4 _Color;
        half _Metallic, _Glossiness, _GlossMapScale, _BumpScale;
        half4 _EmissionColor;

        struct Input
        {
            float2 uv_MainTex;
            float facing : VFACE;
        };

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            fixed4 albedo = tex2D(_MainTex, input.uv_MainTex) * _Color;
            output.Albedo = albedo.rgb;
            output.Alpha = albedo.a;
            output.Metallic = _Metallic;
            output.Smoothness = _Glossiness;
            #ifdef _METALLICGLOSSMAP
                half4 packed = tex2D(_MetallicGlossMap, input.uv_MainTex);
                output.Metallic = packed.r;
                output.Smoothness = packed.a * _GlossMapScale;
            #endif
            #ifdef _NORMALMAP
                output.Normal = UnpackScaleNormal(tex2D(_BumpMap, input.uv_MainTex), _BumpScale);
            #else
                output.Normal = half3(0, 0, 1);
            #endif
            output.Normal *= input.facing >= 0 ? 1 : -1;
            output.Emission = _EmissionColor.rgb;
        }
        ENDCG
    }
    Fallback "Standard"
}
