Shader "MiniGame/Dream Dynamic Cubemap"
{
    Properties
    {
        _Tex ("Beach panorama", Cube) = "" {}
        _Yaw ("Panorama yaw", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            samplerCUBE _Tex;
            float _Yaw;
            struct v2f { float4 vertex:SV_POSITION; float3 direction:TEXCOORD0; };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.direction = v.vertex.xyz;
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float sine, cosine;
                sincos(_Yaw * UNITY_PI / 180, sine, cosine);
                float3 direction = normalize(i.direction);
                // Inverse sample rotation makes positive yaw rotate the visible panorama clockwise.
                direction.xz = float2(cosine * direction.x - sine * direction.z,
                    sine * direction.x + cosine * direction.z);
                return float4(texCUBE(_Tex, direction).rgb, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
