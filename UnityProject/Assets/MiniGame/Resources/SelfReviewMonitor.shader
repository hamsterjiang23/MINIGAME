Shader "Hidden/MiniGame/SelfReviewMonitor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            struct Input { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float shade : TEXCOORD1; };
            Output vert(Input input)
            {
                Output output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                float3 normal = UnityObjectToWorldNormal(input.normal);
                output.shade = .55 + .45 * saturate(dot(normal, normalize(float3(.35,.8,.5))));
                return output;
            }
            fixed4 frag(Output input) : SV_Target
            {
                fixed3 color = tex2D(_MainTex, input.uv).rgb * _Color.rgb * input.shade;
                float luminance = dot(color, float3(.299,.587,.114));
                return fixed4(lerp(color, luminance * fixed3(.87,1,.9), .3), 1);
            }
            ENDCG
        }
    }
}
