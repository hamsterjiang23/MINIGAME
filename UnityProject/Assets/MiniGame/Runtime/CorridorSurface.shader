Shader "MiniGame/Corridor Surface"
{
    Properties
    {
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Four quadrant albedo", 2D) = "white" {}
        _AtlasRect ("Atlas region (x, y, width, height)", Vector) = (0, .5, .5, .5)
        _TileSize ("Metres per repeat", Float) = 1
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Glossiness ("Smoothness", Range(0, 1)) = .2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        sampler2D _MainTex;
        float4 _MainTex_TexelSize, _AtlasRect;
        fixed4 _Color;
        half _Metallic, _Glossiness;
        float _TileSize;
        struct Input { float3 worldPos; float3 worldNormal; };

        fixed3 SampleRegion(float2 plane)
        {
            // Keep filtering within this quadrant, even when the world-space UV wraps.
            float2 guard = _MainTex_TexelSize.xy * 2.5;
            float2 uv = _AtlasRect.xy + guard
                + frac(plane / max(.02, _TileSize)) * (_AtlasRect.zw - guard * 2);
            return tex2D(_MainTex, uv).rgb;
        }

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float3 weight = pow(abs(input.worldNormal), 4);
            weight /= max(dot(weight, float3(1, 1, 1)), .0001);
            fixed3 albedo = SampleRegion(input.worldPos.zy) * weight.x
                + SampleRegion(input.worldPos.xz) * weight.y
                + SampleRegion(input.worldPos.xy) * weight.z;
            output.Albedo = albedo * _Color.rgb;
            output.Metallic = _Metallic;
            output.Smoothness = _Glossiness;
            output.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
