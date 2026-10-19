Shader "MiniGame/Readable Prop Outline"
{
    Properties { _Color ("Color", Color) = (.76,.85,.68,.95) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Cull Off ZWrite Off ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct Vertex { float4 vertex : POSITION; fixed4 color : COLOR; };
            struct Varying { float4 position : SV_POSITION; fixed4 color : COLOR; };
            Varying vert(Vertex input)
            {
                Varying output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.color = input.color * _Color;
                return output;
            }
            fixed4 frag(Varying input) : SV_Target { return input.color; }
            ENDCG
        }
    }
}
