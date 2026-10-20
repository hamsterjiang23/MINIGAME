Shader "Hidden/MiniGame/RetroScreen"
{
    Properties { _MainTex ("Source", 2D) = "white" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Strength, _PixelSize, _Levels, _GammaBefore, _GammaAfter, _FullscreenQuantization;
            float _ColorGradingEnabled, _NeutralTonemapping, _PerceptualQuantization;
            float _QuantizationStrength, _DitherStrength, _UseVirtualResolution, _VirtualHeight;
            float4 _ColorAdjustments, _ToneExposures, _ToneRanges;
            float4 _ShadowTint, _MidtoneTint, _HighlightTint;

            float Luminance709(float3 color) { return dot(color, float3(.2126, .7152, .0722)); }

            float Bayer2(float2 cell)
            {
                cell = fmod(cell, 2);
                return cell.y > .5 ? (cell.x > .5 ? 1 : 3) : (cell.x > .5 ? 2 : 0);
            }

            float Bayer4(float2 cell)
            {
                return (4 * Bayer2(cell) + Bayer2(floor(fmod(cell, 4) / 2)) + .5) / 16 - .5;
            }

            // Hable/Hejl/Frostbite Neutral curve, matching Unity Graphics Color.hlsl.
            // Copyright (c) 2020 Unity Technologies ApS. Unity Companion License:
            // License copy: Art/MainRoom/RoomLighting/UnityGraphics-LICENSE.md (under Assets/MiniGame).
            // https://github.com/Unity-Technologies/Graphics/blob/a7e4c051d256a781ab362c64316b125a1e104694/Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl#L489-L525
            float3 NeutralCurve(float3 color)
            {
                return ((color * (.2 * color + .24 * .29) + .272 * .02)
                    / (color * (.2 * color + .29) + .272 * .3)) - .02 / .3;
            }

            float3 NeutralTonemap(float3 color)
            {
                float3 whiteScale = 1 / NeutralCurve(float3(5.3, 5.3, 5.3));
                return NeutralCurve(min(max(color, 0), 435.18712) * whiteScale) * whiteScale;
            }

            float3 ExposureLinear(float3 color)
            {
                return float3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g), GammaToLinearSpaceExact(color.b));
            }

            float3 ExposureDisplay(float3 color)
            {
                return float3(LinearToGammaSpaceExact(color.r), LinearToGammaSpaceExact(color.g), LinearToGammaSpaceExact(color.b));
            }

            float3 Grade(float3 color)
            {
                // EV is a linear-light gain. Only zone selection and artistic controls use display values.
                float3 linearColor = ExposureLinear(color) * exp2(_ColorAdjustments.x);
                color = ExposureDisplay(linearColor);
                float luminance = Luminance709(color);
                float shadow = 1 - smoothstep(0, _ToneRanges.x, luminance);
                float highlight = smoothstep(_ToneRanges.y, 1, luminance);
                float midtone = max(0, 1 - shadow - highlight);
                float zoneGain = dot(float3(shadow, midtone, highlight), exp2(_ToneExposures.xyz));
                color = ExposureDisplay(linearColor * zoneGain);
                color *= shadow * _ShadowTint.rgb + midtone * _MidtoneTint.rgb + highlight * _HighlightTint.rgb;
                color = max(0, (color - .5) * (1 + _ColorAdjustments.y * .01) + .5);
                return max(0, lerp(Luminance709(color).xxx, color, 1 + _ColorAdjustments.z * .01));
            }

            float4 frag(v2f_img input) : SV_Target
            {
                // Blit's vert_img UVs sample the sole source consistently on each graphics API.
                float2 size = _MainTex_TexelSize.zw;
                float2 pixel = floor(input.uv * size);
                float2 blockSize = max(1, _PixelSize).xx;
                if (_UseVirtualResolution > .5)
                {
                    float height = min(size.y, max(1, _VirtualHeight));
                    float width = min(size.x, max(1, floor(height * size.x / size.y + .5)));
                    blockSize = size / float2(width, height);
                }
                float2 cell = floor(pixel / blockSize);
                // A fractional virtual grid still samples the centre of its actual source texels.
                float2 start = ceil(cell * blockSize);
                float2 end = min(ceil((cell + 1) * blockSize), size);
                // Snap even-sized blocks to an actual texel centre; clip the final partial block.
                float2 samplePixel = floor((start + end - 1) * .5) + .5;
                float2 uv = clamp(samplePixel / size, .5 / size, 1 - .5 / size);
                float4 original = tex2D(_MainTex, input.uv);
                float3 color = tex2D(_MainTex, uv).rgb;
                bool quantize = _FullscreenQuantization > .5 && _QuantizationStrength > 0;
                if (!quantize && _ColorGradingEnabled < .5 && _NeutralTonemapping < .5)
                    return float4(lerp(original.rgb, color, _Strength), original.a);
                color = max(color, 0);
                // Operate on display-like values in both Gamma and Linear projects.
                #ifndef UNITY_COLORSPACE_GAMMA
                    color = LinearToGammaSpace(color);
                #endif
                if (_ColorGradingEnabled > .5) color = Grade(color);
                if (_NeutralTonemapping > .5)
                    color = LinearToGammaSpace(NeutralTonemap(GammaToLinearSpace(color)));
                if (quantize)
                {
                    // Legacy max-channel quantization keeps its exact N-step/gamma behaviour.
                    // Perceptual mode uses N-1 steps and Rec.709 display-like luminance.
                    float hdr = max(1, max(color.r, max(color.g, color.b)));
                    float3 quantized = pow(color / hdr, _GammaBefore);
                    float value = _PerceptualQuantization > .5 ? Luminance709(quantized)
                        : max(quantized.r, max(quantized.g, quantized.b));
                    float steps = max(1, _Levels - (_PerceptualQuantization > .5 ? 1 : 0));
                    float noisyValue = saturate(value + Bayer4(cell) * _DitherStrength / steps);
                    float level = floor(noisyValue * steps + .5) / steps;
                    quantized *= level / max(value, 1e-6);
                    quantized = pow(max(quantized, 0), _GammaAfter) * hdr;
                    color = lerp(color, quantized, _QuantizationStrength);
                }
                #ifndef UNITY_COLORSPACE_GAMMA
                    color = GammaToLinearSpace(color);
                #endif
                return float4(lerp(original.rgb, color, _Strength), original.a);
            }
            ENDCG
        }
    }
    Fallback Off
}
