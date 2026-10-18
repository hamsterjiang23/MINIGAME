Shader "MiniGame/Room Window Glass"
{
    Properties
    {
        _Color ("Dust Tint", Color) = (.16, .20, .19, .12)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct VertexInput { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct VertexOutput { float4 position : SV_POSITION; float2 uv : TEXCOORD0; };
            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }
            fixed4 frag(VertexOutput input) : SV_Target
            {
                float2 edgeDistance = min(input.uv, 1 - input.uv);
                half dust = 1 - saturate(min(edgeDistance.x, edgeDistance.y) * 10);
                return fixed4(_Color.rgb, _Color.a * lerp(.35, 1, dust));
            }
            ENDCG
        }
    }
}
