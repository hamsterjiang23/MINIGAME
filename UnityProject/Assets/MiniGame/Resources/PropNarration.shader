Shader "MiniGame/Prop Narration"
{
    Properties
    {
        [PerRendererData] _MainTex ("Font atlas", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Backdrop ("Feathered backdrop", Float) = 0
        _PixelStep ("Subtitle pixel size at 1080p (0 disables)", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize, _TextureSampleAdd;
            fixed4 _Color;
            float _Backdrop, _PixelStep;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            Output vert(Input input)
            {
                Output output; output.vertex=UnityObjectToClipPos(input.vertex);
                output.uv=input.uv; output.color=input.color * _Color; return output;
            }
            fixed4 frag(Output input) : SV_Target
            {
                if (_Backdrop > .5)
                {
                    float2 edge = abs(input.uv * 2 - 1);
                    float feather = pow(saturate(1 - max(edge.x, edge.y)), .8);
                    return fixed4(input.color.rgb, input.color.a * feather);
                }
                float2 uv = input.uv;
                if (_PixelStep > 0)
                {
                    // Match the room's coarse image while keeping Chinese strokes readable.
                    // Quantize screen pixels rather than the entire shared font atlas.
                    float step = max(1, _PixelStep * _ScreenParams.y / 1080);
                    float2 offset = (floor(input.vertex.xy / step) + .5) * step - input.vertex.xy;
                    uv += ddx(input.uv) * offset.x + ddy(input.uv) * offset.y;
                }
                float2 d = abs(_MainTex_TexelSize.xy);
                float core = (tex2D(_MainTex,uv) + _TextureSampleAdd).a;
                float glow = tex2D(_MainTex,uv+d*float2(1,0)).a
                    + tex2D(_MainTex,uv+d*float2(-1,0)).a
                    + tex2D(_MainTex,uv+d*float2(0,1)).a
                    + tex2D(_MainTex,uv+d*float2(0,-1)).a;
                float alpha = saturate(core * .88 + glow * .065);
                return fixed4(input.color.rgb, alpha * input.color.a);
            }
            ENDCG
        }
    }
}
