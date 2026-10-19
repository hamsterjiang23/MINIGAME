Shader "MiniGame/Dream Projection Surface"
{
    Properties { _MainTex ("Dream image", 2D) = "black" {} _Opacity ("Brightness", Range(0,1)) = 1 }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _Opacity;
            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 edge = min(input.uv,1-input.uv);
                float border = smoothstep(0,.009,min(edge.x,edge.y));
                return fixed4(tex2D(_MainTex,input.uv).rgb * _Opacity,border);
            }
            ENDCG
        }
    }
    Fallback Off
}
