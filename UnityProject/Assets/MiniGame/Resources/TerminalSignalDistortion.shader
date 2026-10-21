Shader "MiniGame/UI/Signal Distortion"
{
    Properties
    {
        [PerRendererData] _MainTex ("UI Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,1)) = 0.6
        _Tear ("Horizontal Tear (UV)", Range(0,0.12)) = 0.018
        _RgbSplit ("RGB Separation (UV)", Range(0,0.03)) = 0.004
        _Noise ("Noise", Range(0,0.25)) = 0.025
        _ScanlineStrength ("Scanline Strength", Range(0,0.8)) = 0.15
        _ScanlineDensity ("Scanline Count", Range(20,600)) = 220
        _ScanlineSpeed ("Scanline Phase Cycles / Second", Range(0,10)) = 1
        _RollStrength ("Rolling Band Strength", Range(0,0.15)) = 0.012
        _RollSpeed ("Rolling Band Traversals / Second", Range(0,3)) = 0.2
        _Rate ("Random Updates / Second", Range(1,30)) = 14
        _BurstFrequency ("Burst Events / Second", Range(0.1,10)) = 0.8
        _BurstDuration ("Burst Duration (Seconds)", Range(0.02,0.8)) = 0.18
        [Toggle] _LogoMode ("Transparent Logo Mode", Float) = 0
        [HideInInspector] _Clock ("Clock (Seconds)", Float) = 0
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
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 mask : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            sampler2D _MainTex;
            float4 _MainTex_ST, _MainTex_TexelSize, _ClipRect;
            fixed4 _Color, _TextureSampleAdd;
            int _UIVertexColorAlwaysGammaSpace;
            float _UIMaskSoftnessX, _UIMaskSoftnessY;
            float _Clock, _Intensity, _Tear, _RgbSplit, _Noise;
            float _ScanlineStrength, _ScanlineDensity, _ScanlineSpeed;
            float _RollStrength, _RollSpeed, _Rate, _BurstFrequency, _BurstDuration, _LogoMode;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace())
                    v.color.rgb = UIGammaToLinear(v.color.rgb);
                o.color = v.color * _Color;
                float2 pixelSize = o.vertex.w / abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 rect = clamp(_ClipRect, -2e10, 2e10);
                o.mask = float4(v.vertex.xy * 2 - rect.xy - rect.zw,
                    0.25 / (0.25 * float2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize)));
                return o;
            }

            float Hash(float2 p)
            {
                float3 q = frac(float3(p.xyx) * 0.1031);
                q += dot(q, q.yzx + 33.33);
                return frac((q.x + q.y) * q.z);
            }

            fixed4 ReadSignal(float2 uv)
            {
                // Clamp inside the edge texels even when the input texture uses Repeat.
                float2 inset = abs(_MainTex_TexelSize.xy) * 0.5;
                fixed4 c = tex2D(_MainTex, clamp(uv, inset, 1 - inset)) + _TextureSampleAdd;
                if (_LogoMode > 0.5)
                    c.a *= step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);
                return c;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Bypass all resampling and arithmetic at zero: ordinary uGUI sampling.
                fixed4 c = tex2D(_MainTex, i.uv) + _TextureSampleAdd;
                if (_Intensity > 0)
                {
                    float clock = max(0, _Clock);
                    float tick = floor(clock * max(1, _Rate));
                    float frequency = max(0.1, _BurstFrequency);
                    float eventTime = clock * frequency - 0.37;
                    float phase = frac(eventTime);
                    // Reserve at least 20% of each period for an undisturbed logo.
                    float duty = min(0.8, max(0.001, _BurstDuration * frequency));
                    float progress = phase / duty;
                    float burst = step(0, eventTime) * (1 - step(duty, phase));
                    burst *= smoothstep(0, 0.12, progress) * (1 - smoothstep(0.65, 1, progress));
                    burst *= lerp(0.45, 1, Hash(float2(tick, 19)));
                    float amount = saturate(_Intensity) * lerp(1, burst, step(0.5, _LogoMode));

                    if (amount > 0)
                    {
                        float rowNoise = Hash(float2(floor(i.uv.y * 48), tick));
                        float tear = (rowNoise - 0.84) * 6.25 * step(0.68, rowNoise) * _Tear * burst;
                        float bandDistance = abs(frac(i.uv.y - clock * _RollSpeed + 0.5) - 0.5);
                        float band = 1 - smoothstep(0.015, 0.11, bandDistance);
                        float2 uv = i.uv + float2((tear + band * _RollStrength) * amount, 0);
                        float split = _RgbSplit * burst * amount;
                        fixed4 r = ReadSignal(uv + float2(split, 0));
                        fixed4 g = ReadSignal(uv);
                        fixed4 b = ReadSignal(uv - float2(split, 0));
                        // Reconstruct straight alpha from the separate channel coverages.
                        // This retains red/blue fringes without painting transparent gaps.
                        float alpha = max(r.a, max(g.a, b.a));
                        c = fixed4(float3(r.r * r.a, g.g * g.a, b.b * b.a) / max(alpha, 0.00001), alpha);

                        // Speed is phase cycles/sec: 1 moves one stripe spacing per second.
                        float scanPhase = i.uv.y * _ScanlineDensity - clock * _ScanlineSpeed;
                        float scan = 0.5 + 0.5 * cos(scanPhase * 6.2831853);
                        // Fade unresolved stripes to their average to avoid moire.
                        scan = lerp(scan, 0.5, saturate(fwidth(scanPhase) - 0.5));
                        c.rgb *= 1 - _ScanlineStrength * scan * amount;
                        float grain = Hash(floor(i.uv * float2(640, 360)) + tick * float2(3.7, 7.1)) * 2 - 1;
                        c.rgb = saturate(c.rgb + (grain * _Noise + band * _RollStrength) * amount);
                    }
                }
                i.color.a = round(i.color.a * 255) / 255;
                c *= i.color;
                #ifdef UNITY_UI_CLIP_RECT
                float2 mask = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
                c.a *= mask.x * mask.y;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a - 0.001);
                #endif
                // Match UI/Default's premultiplied output, including render-target alpha.
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
