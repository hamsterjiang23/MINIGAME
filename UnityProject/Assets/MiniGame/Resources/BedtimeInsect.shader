Shader "MiniGame/Bedtime/GlowParticles"
{
    Properties { _Strength ("Visibility", Range(0,1)) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            float _Strength;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 position : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                float radiusSquared = dot(input.uv, input.uv);
                float edge = 1 - smoothstep(.72, 1, sqrt(radiusSquared));
                float core = exp2(-110 * radiusSquared);
                float halo = .14 * exp2(-5.5 * radiusSquared);
                float glow = (core + halo) * edge * input.color.a * saturate(_Strength);
                // A fine centre with a faint halo; additive RGB leaves the captured destination alpha unchanged.
                return fixed4(input.color.rgb * glow, 0);
            }
            ENDCG
        }
    }
}
