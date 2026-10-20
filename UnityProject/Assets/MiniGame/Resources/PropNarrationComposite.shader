Shader "Hidden/MiniGame/Prop Narration Composite"
{
    Properties
    {
        _MainTex ("Scene", 2D) = "white" {}
        _OverlayTex ("Narration overlay", 2D) = "black" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex, _OverlayTex;
            fixed4 frag(v2f_img input) : SV_Target
            {
                fixed4 scene = tex2D(_MainTex, input.uv);
                fixed4 overlay = tex2D(_OverlayTex, input.uv);
                return fixed4(scene.rgb * (1 - overlay.a) + overlay.rgb, scene.a);
            }
            ENDCG
        }
    }
}
