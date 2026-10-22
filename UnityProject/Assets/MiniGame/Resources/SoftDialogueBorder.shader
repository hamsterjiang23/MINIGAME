Shader "MiniGame/Soft Dialogue Border"
{
    Properties { [PerRendererData] _MainTex ("UI texture", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            Output vert(Input input)
            {
                Output output; output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv; output.color = input.color; return output;
            }
            fixed4 frag(Output input):SV_Target
            {
                // A thin ring with softly fading sides, measured in screen pixels.
                float2 size = 1 / max(fwidth(input.uv), .00001);
                float2 q = abs((input.uv - .5) * size) - (size * .5 - 3);
                float distance = length(max(q, 0)) + min(max(q.x, q.y), 0) - 3;
                float coverage = 1 - smoothstep(.25, 1.4, abs(distance + 1.4));
                return fixed4(input.color.rgb, input.color.a * coverage);
            }
            ENDCG
        }
    }
}
