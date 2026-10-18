Shader "MiniGame/Bedtime/WingedInsects"
{
    Properties { _Strength ("Visibility", Range(0,1)) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
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
                float distance = length(input.uv);
                float silhouette = 1 - smoothstep(.74, 1, distance);
                // Neutral edge reflection on thin wings; ordinary alpha, never an additive glow sprite.
                float edge = smoothstep(.50, .78, distance) * (1 - smoothstep(.78, .98, distance));
                float isWing = step(.3, input.color.r);
                fixed3 color = input.color.rgb + edge * .12 * isWing;
                return fixed4(color, silhouette * input.color.a * _Strength);
            }
            ENDCG
        }
    }
}
