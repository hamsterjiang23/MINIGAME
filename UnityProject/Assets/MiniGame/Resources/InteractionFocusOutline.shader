Shader "MiniGame/Interaction Focus Outline"
{
    Properties
    {
        _FocusOutlineCenter ("Mesh center", Vector) = (0,0,0,0)
        _FocusOutlineWidth ("Outline width in pixels", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off
        CGINCLUDE
        #include "UnityCG.cginc"
        float4 _FocusOutlineCenter;
        float _FocusOutlineWidth;
        struct Vertex { float4 vertex : POSITION; };
        struct Varying { float4 position : SV_POSITION; };
        Varying Original(Vertex input)
        {
            Varying output;
            output.position = UnityObjectToClipPos(input.vertex);
            return output;
        }
        Varying Expanded(Vertex input)
        {
            Varying output = Original(input);
            float4 center = UnityObjectToClipPos(float4(_FocusOutlineCenter.xyz, 1));
            float2 direction = (output.position.xy / max(abs(output.position.w), .00001)
                - center.xy / max(abs(center.w), .00001)) * _ScreenParams.xy;
            direction /= max(length(direction), .00001);
            // A shared projected centre avoids cracks at hard normals and needs no readable mesh copy.
            output.position.xy += direction * (2.0 * _FocusOutlineWidth / _ScreenParams.xy) * output.position.w;
            return output;
        }
        fixed4 White(Varying input) : SV_Target { return fixed4(1, 1, 1, 1); }
        ENDCG

        Pass
        {
            Name "Mask"
            ColorMask 0 ZTest Always
            Stencil { Ref 1 ReadMask 1 WriteMask 1 Comp Always Pass Replace }
            CGPROGRAM
            #pragma vertex Original
            #pragma fragment White
            ENDCG
        }
        Pass
        {
            Name "VisibleOutline"
            ZTest LEqual
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            CGPROGRAM
            #pragma vertex Expanded
            #pragma fragment White
            ENDCG
        }
        Pass
        {
            Name "ReleaseMask"
            ColorMask 0 ZTest Always
            Stencil { Ref 0 ReadMask 1 WriteMask 1 Comp Always Pass Replace }
            CGPROGRAM
            #pragma vertex Original
            #pragma fragment White
            ENDCG
        }
    }
    Fallback Off
}
