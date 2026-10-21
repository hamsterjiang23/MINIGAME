Shader "MiniGame/Dream Sand"
{
    Properties
    {
        _Color ("Sand", Color) = (.78,.70,.55,1)
        _DreamTime ("Playback time", Float) = 0
        _UseDreamTime ("Use explicit playback time", Float) = 0
        _DreamOrigin ("Isolated source origin", Vector) = (0,0,0,0)
        _WaveHeight ("Wave height (m)", Float) = .24
        _WavePeriod ("Wave period (s)", Float) = 6
        _WaveLength ("Wave length (m)", Float) = 7
        _ShoreWash ("Shore wash (m)", Float) = 1.8
        _FoamAmount ("Foam amount", Range(0,1.5)) = .85
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0
        #include "UnityCG.cginc"
        #include "DreamOcean.cginc"
        float4 _Color;
        struct Input { float3 worldPos; };
        void vert(inout appdata_full v)
        {
            float3 world = mul(unity_ObjectToWorld,v.vertex).xyz-_DreamOrigin.xyz;
            float2 gradient = DreamShoreOffsetGradient(world.xz);
            world.y += DreamShoreOffset(world.xz);
            float3 normal = normalize(UnityObjectToWorldNormal(v.normal)-float3(gradient.x,0,gradient.y));
            v.vertex = mul(unity_WorldToObject,float4(world+_DreamOrigin.xyz,1));
            v.normal = normalize(mul(normal,(float3x3)unity_ObjectToWorld));
        }
        void surf(Input i, inout SurfaceOutputStandard o)
        {
            float3 world = i.worldPos-_DreamOrigin.xyz;
            float grain=DreamOceanHash(floor(world.xz*65));
            float ripple=sin(world.z*23+sin(world.x*.5)*2)*.014;
            float wet=DreamSandWetness(world.xz,DreamOceanTime());
            float detail=1-saturate(max(length(ddx(world.xz)),length(ddy(world.xz)))*12);
            o.Albedo=_Color.rgb*.82*(.97+(grain-.5)*.10*detail+ripple*detail)*(1-wet*.38);
            o.Smoothness=lerp(.10,.58,wet);
            o.Metallic=0;
            o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
