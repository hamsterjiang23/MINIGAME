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
            float _Strength, _PixelSize, _Levels, _GammaBefore, _GammaAfter;

            float4 frag(v2f_img input) : SV_Target
            {
                // Blit's vert_img UVs sample the sole source consistently on each graphics API.
                float2 size = _MainTex_TexelSize.zw;
                float2 pixel = floor(input.uv * size);
                float blockSize = max(1, _PixelSize);
                float2 start = floor(pixel / blockSize) * blockSize;
                float2 end = min(start + blockSize, size);
                // Snap even-sized blocks to an actual texel centre; clip the final partial block.
                float2 samplePixel = floor((start + end - 1) * .5) + .5;
                float2 uv = clamp(samplePixel / size, .5 / size, 1 - .5 / size);
                float4 original = tex2D(_MainTex, input.uv);
                float3 color = max(tex2D(_MainTex, uv).rgb, 0);
                // Operate on display-like values in both Gamma and Linear projects.
                #ifndef UNITY_COLORSPACE_GAMMA
                    color = LinearToGammaSpace(color);
                #endif
                // Separate HDR magnitude so powers stay finite and bright emission isn't clipped.
                float peak = max(color.r, max(color.g, color.b));
                float hdr = max(1, peak);
                color = pow(color / hdr, _GammaBefore);
                float value = max(color.r, max(color.g, color.b));
                float level = floor(value * _Levels + .5) / _Levels;
                color *= level / max(value, 1e-6);
                color = pow(max(color, 0), _GammaAfter) * hdr;
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
