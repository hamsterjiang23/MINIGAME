Shader "Hidden/MiniGame/Wall Surface Diagnostic"
{
    Properties
    {
        _DiagnosticColor ("Surface ID", Color) = (0.06, 0.06, 0.06, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull Off
        ZWrite On
        ZTest LEqual
        Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _DiagnosticColor;
            struct Input { float4 position : POSITION; };
            struct Varying { float4 position : SV_POSITION; };
            Varying vert(Input vertex)
            {
                Varying output;
                output.position = UnityObjectToClipPos(vertex.position);
                return output;
            }
            float4 frag(Varying input) : SV_Target { return _DiagnosticColor; }
            ENDCG
        }
    }
    Fallback Off
}
