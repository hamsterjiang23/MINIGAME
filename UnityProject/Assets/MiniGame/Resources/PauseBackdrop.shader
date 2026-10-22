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
                // Preserve the room's coarse pixels; the pause layer only quiets their contrast.
                float radius = .22;
                float2 stepUV = _MainTex_TexelSize.xy * radius;
                fixed3 color = tex2D(_MainTex, uv).rgb * .2;
                color += tex2D(_MainTex, uv + stepUV * float2(1, 1)).rgb * .2;
                color += tex2D(_MainTex, uv + stepUV * float2(-1, 1)).rgb * .2;
                color += tex2D(_MainTex, uv + stepUV * float2(1, -1)).rgb * .2;
                color += tex2D(_MainTex, uv + stepUV * float2(-1, -1)).rgb * .2;
                color *= lerp(1, .96, _Finish);
                return fixed4(color, 1);
            }
            ENDCG
        }
    }
}
