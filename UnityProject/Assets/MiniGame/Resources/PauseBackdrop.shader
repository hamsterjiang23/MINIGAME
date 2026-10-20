Shader "MiniGame/PauseBackdrop"
{
    Properties { _MainTex ("Scene", 2D) = "white" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Finish;
            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 centered = uv * 2 - 1;
                // A restrained lens curve makes the frozen room recede at the edges.
                uv += centered * dot(centered, centered) * .004 * _Finish;
                float radius = lerp(1.2, 2.8, smoothstep(.12, .9, abs(centered.x)));
                float2 stepUV = _MainTex_TexelSize.xy * radius;
                fixed3 color = tex2D(_MainTex, uv).rgb * .2;
                color += tex2D(_MainTex, uv + stepUV * float2(1, 1)).rgb * .2;
                color += tex2D(_MainTex, uv + stepUV * float2(-1, 1)).rgb * .2;
                color += tex2D(_MainTex, uv + stepUV * float2(1, -1)).rgb * .2;
                color += tex2D(_MainTex, uv + stepUV * float2(-1, -1)).rgb * .2;
                float gray = dot(color, float3(.299, .587, .114));
                float vignette = 1 - smoothstep(.3, 1.45, dot(centered, centered)) * .62;
                color = lerp(color, lerp(gray.xxx, color, .8) * float3(.80, .88, .84) * vignette, _Finish);
                return fixed4(color, 1);
            }
            ENDCG
        }
    }
}
