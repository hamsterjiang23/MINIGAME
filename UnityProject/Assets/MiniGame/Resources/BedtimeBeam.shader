Shader "MiniGame/Bedtime/SoftCeilingBeam"
{
    Properties
    {
        _Color ("Warm Air Color", Color) = (.66,.64,.51,1)
        _Strength ("Visibility", Range(0,1)) = 0
        _Radius ("Bed Footprint Radius", Float) = 1
        _Height ("Ceiling To Bed Distance", Float) = 2
        _EmitterWS ("Ceiling Origin", Vector) = (0,2,0,0)
        _AxisWS ("Downward Axis", Vector) = (0,-1,0,0)
        _VisualTime ("Owner Clock", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent-10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Front
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float4 _Color, _EmitterWS, _AxisWS;
            float _Strength, _Radius, _Height, _VisualTime;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 position : SV_POSITION; float3 worldPosition : TEXCOORD0; float4 screenPosition : TEXCOORD1; };
            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.screenPosition = ComputeScreenPos(output.position);
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                float3 ray = input.worldPosition - _WorldSpaceCameraPos;
                float endDistance = length(ray);
                ray /= max(.0001, endDistance);
                float depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(input.screenPosition));
                float eyeDepth = LinearEyeDepth(depth);
                float eyeDirection = max(.001, -mul((float3x3)UNITY_MATRIX_V, ray).z);
                endDistance = min(endDistance, eyeDepth / eyeDirection);
                float boundLength = sqrt(_Height * _Height + 4 * _Radius * _Radius) * 1.02;
                float startDistance = max(0, endDistance - boundLength);
                float stepLength = (endDistance - startDistance) / 24;
                float density = 0;
                [unroll] for (int stepIndex = 0; stepIndex < 24; stepIndex++)
                {
                    float distance = startDistance + (stepIndex + .5) * stepLength;
                    float3 samplePosition = _WorldSpaceCameraPos + ray * distance - _EmitterWS.xyz;
                    float down = dot(samplePosition, _AxisWS.xyz);
                    float h = down / max(.01, _Height);
                    float3 radial = samplePosition - _AxisWS.xyz * down;
                    float width = max(.001, _Radius * h);
                    float softRadius = 1 - smoothstep(.46, 1, length(radial) / width);
                    float ends = smoothstep(.015, .12, h) * (1 - smoothstep(.85, 1, h));
                    float air = .96 + .04 * sin(down * 5.3 + radial.x * 3.1 + _VisualTime * .13);
                    density += softRadius * ends * air * stepLength;
                }
                float alpha = (1 - exp(-density * .18)) * _Strength;
                return fixed4(_Color.rgb, saturate(alpha));
            }
            ENDCG
        }
    }
}
