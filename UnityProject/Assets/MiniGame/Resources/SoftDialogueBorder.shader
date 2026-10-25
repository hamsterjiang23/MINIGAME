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
                // A borderless scrim: no rectangular edge over the actor or the room.
                float horizontal = smoothstep(0, .18, input.uv.x) * smoothstep(0, .18, 1 - input.uv.x);
                float vertical = smoothstep(0, .24, input.uv.y) * smoothstep(0, .40, 1 - input.uv.y);
                float coverage = horizontal * vertical;
                return fixed4(input.color.rgb, input.color.a * coverage);
            }
            ENDCG
        }
    }
}
