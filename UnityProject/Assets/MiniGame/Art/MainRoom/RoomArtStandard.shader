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
        _CrtPower ("CRT Power", Range(0, 1)) = 1
        [HideInInspector] _DreamClipEnabled ("Dream head capture clipping", Float) = 0
        [HideInInspector] _DreamClipPlane ("Dream wall plane", Vector) = (0,0,1,0)
        [HideInInspector] _LiveDisplay ("Live terminal display", Float) = 0
        [HideInInspector] _LiveDisplayTex ("Native terminal frame", 2D) = "black" {}
        [Toggle] _LightingBandsEnabled ("Lighting Bands Enabled", Float) = 0
        _LightingBands ("Lighting Bands", Range(2, 32)) = 8
        _LightingBandsStrength ("Lighting Bands Strength", Range(0, 1)) = 1
        _LightingDither ("Lighting Dither", Range(0, 1)) = 0
        _AlbedoGain ("Albedo Gain", Range(0, 3)) = 1
        _AlbedoContrast ("Albedo Contrast", Range(0, 2)) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull [_Cull]
        CGPROGRAM
        #pragma surface surf RoomStandard fullforwardshadows addshadow exclude_path:deferred exclude_path:prepass
        #pragma target 3.0
        #pragma shader_feature_local _NORMALMAP
        #pragma shader_feature_local _METALLICGLOSSMAP
        #include "UnityPBSLighting.cginc"

        sampler2D _MainTex, _BumpMap, _MetallicGlossMap;
        sampler2D _LiveDisplayTex;
        half _LiveDisplay;
        fixed4 _Color;
        half _Metallic, _Glossiness, _GlossMapScale, _BumpScale;
        half4 _EmissionColor;
        half _CrtPower;
        half _DreamClipEnabled;
        float4 _DreamClipPlane;
        half _LightingBandsEnabled, _LightingBands, _LightingBandsStrength, _LightingDither;
        half _AlbedoGain, _AlbedoContrast;

        struct SurfaceOutputRoom
        {
            fixed3 Albedo;
            float3 Normal;
            half3 Emission;
            half Metallic;
            half Smoothness;
            half Occlusion;
            fixed Alpha;
            half LightingNoise;
        };

        SurfaceOutputStandard StandardSurface(SurfaceOutputRoom surface)
        {
            SurfaceOutputStandard standard;
            standard.Albedo = surface.Albedo;
            standard.Normal = surface.Normal;
            standard.Emission = surface.Emission;
            standard.Metallic = surface.Metallic;
            standard.Smoothness = surface.Smoothness;
            standard.Occlusion = surface.Occlusion;
            standard.Alpha = surface.Alpha;
            return standard;
        }

        half4 LightingRoomStandard(SurfaceOutputRoom surface, float3 viewDir, UnityGI gi)
        {
            if (_LightingBandsEnabled > .5 && _LightingBands >= 2 && _LightingBandsStrength > 0)
            {
                // Unity's GI setup has already applied cone/range attenuation and shadows.
                // Quantize incident diffuse illumination, never the textured surface colour.
                float ndotl = saturate(dot(normalize(surface.Normal), gi.light.dir));
                float peak = max(gi.light.color.r, max(gi.light.color.g, gi.light.color.b));
                float illumination = max(0, ndotl * peak);
                float steps = max(1, floor(_LightingBands + .5) - 1);
                float band = max(0, floor(illumination * steps
                    + surface.LightingNoise * saturate(_LightingDither) + .5)) / steps;
                // Partial strength keeps smooth falloff between levels instead of flat rings.
                float shaped = lerp(illumination, band, saturate(_LightingBandsStrength));
                // Zero light and back-facing surfaces stay unlit, including at shadow edges.
                gi.light.color *= illumination > 1e-5 ? shaped / illumination : 0;
            }
            half4 lit = LightingStandard(StandardSurface(surface), viewDir, gi);
            lit.rgb *= lerp(.02, 1, saturate(_CrtPower));
            return lit;
        }

        void LightingRoomStandard_GI(SurfaceOutputRoom surface, UnityGIInput data, inout UnityGI gi)
        {
            LightingStandard_GI(StandardSurface(surface), data, gi);
        }

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float facing : VFACE;
        };

        void surf(Input input, inout SurfaceOutputRoom output)
        {
            if (_DreamClipEnabled > .5) clip(dot(float4(input.worldPos, 1), _DreamClipPlane));
            fixed4 albedo = tex2D(_MainTex, input.uv_MainTex) * _Color;
            output.Albedo = max(0, (albedo.rgb - .5) * _AlbedoContrast + .5) * _AlbedoGain;
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
            output.Emission = _EmissionColor.rgb * saturate(_CrtPower);
            if (_LiveDisplay > .5)
            {
                output.Albedo = 0;
                output.Metallic = 0;
                output.Emission = tex2D(_LiveDisplayTex, input.uv_MainTex).rgb * saturate(_CrtPower);
            }
            output.Occlusion = 1;
            // World-anchored noise is stable as the camera moves, unlike temporal film grain.
            float3 noiseCell = floor(input.worldPos * 200);
            output.LightingNoise = frac(sin(dot(noiseCell, float3(12.9898, 78.233, 37.719))) * 43758.5453) - .5;
        }
        ENDCG
    }
    Fallback "Standard"
}
