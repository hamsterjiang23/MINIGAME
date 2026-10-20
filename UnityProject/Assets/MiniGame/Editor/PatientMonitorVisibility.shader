// Editor-only depth diagnostic. This asset is not included in Player builds.
Shader "Hidden/MiniGame/Patient Visibility Diagnostic"
{
    Properties { _MainTex ("Sprite alpha", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite On ZTest LEqual
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 position : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata value)
            {
                v2f output;
                output.position = UnityObjectToClipPos(value.vertex);
                output.uv = value.uv;
                return output;
            }
            fixed4 frag(v2f value) : SV_Target
            {
                clip(tex2D(_MainTex, value.uv).a - .1);
                return fixed4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
