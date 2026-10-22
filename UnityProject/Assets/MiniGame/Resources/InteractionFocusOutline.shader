Shader "MiniGame/Interaction Focus Outline"
{
    Properties
    {
        _FocusOutlineWidth ("Outline width in pixels", Float) = 3
        _FocusOutlineColor ("Muted outline", Color) = (.42,.42,.39,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off
        CGINCLUDE
        #include "UnityCG.cginc"
        float4 _FocusOutlineOffset;
        float _FocusOutlineWidth;
        fixed4 _FocusOutlineColor;
        sampler2D _FocusOutlineExterior;
        float _FocusOutlineHasExterior;
        float _FocusOutlineMaskScale;
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
            output.position.xy += _FocusOutlineOffset.xy * (2.0 * _FocusOutlineWidth / _ScreenParams.xy) * output.position.w;
            return output;
        }
        fixed4 White(Varying input) : SV_Target { return fixed4(1, 1, 1, 1); }
        Varying Silhouette(Vertex input)
        {
            Varying output = Original(input);
            output.position.xy *= _FocusOutlineMaskScale;
            return output;
        }
        fixed4 Outline(Varying input) : SV_Target
        {
            if (_FocusOutlineHasExterior > .5)
                clip(tex2D(_FocusOutlineExterior,
                    (input.position.xy / _ScreenParams.xy - .5) * _FocusOutlineMaskScale + .5).r - .5);
            return _FocusOutlineColor;
        }
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
            #pragma fragment Outline
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
        Pass
        {
            Name "Silhouette"
            ZTest Always
            CGPROGRAM
            #pragma vertex Silhouette
            #pragma fragment White
            ENDCG
        }
    }
    Fallback Off
}
