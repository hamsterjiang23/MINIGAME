Shader "MiniGame/Interaction Cursor Mask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Approved cursor artwork", 2D) = "white" {}
        _PixelSize ("Pixel block size", Float) = 1.5
        _IconSize ("Icon size", Vector) = (44,44,0,0)
        _AtlasRect ("Icon atlas rectangle", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _PixelSize;
            float4 _IconSize, _AtlasRect;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            Output vert(Input input)
            {
                Output output; output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv; output.color = input.color; return output;
            }
            fixed4 frag(Output input):SV_Target
            {
                // Sample on a coarser local grid: 50% larger blocks, unchanged icon bounds.
                float2 local = (input.uv - _AtlasRect.xy) / _AtlasRect.zw;
                float2 cell = (floor(local * _IconSize.xy / _PixelSize) + .5) * _PixelSize;
                float2 uv = _AtlasRect.xy + saturate(cell / _IconSize.xy) * _AtlasRect.zw;
                fixed3 source = tex2D(_MainTex, uv).rgb;
                // Reject the neutral backdrop, retain the yellow artwork as coverage.
                // Tinting here gives all four symbols the exact same flat reticle yellow.
                float coverage = smoothstep(0, .97, saturate((source.r - source.b - .02) / .65));
                return fixed4(input.color.rgb, input.color.a * coverage);
            }
            ENDCG
        }
    }
}
