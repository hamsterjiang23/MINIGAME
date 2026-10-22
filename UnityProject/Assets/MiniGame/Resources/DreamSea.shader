Shader "MiniGame/Dream Sea"
{
    Properties
    {
        _Shallow ("Shallow water", Color) = (0.29,0.76,0.78,1)
        _Deep ("Deep water", Color) = (0.035,0.29,0.45,1)
        _DreamTime ("Playback time", Float) = 0
        _UseDreamTime ("Use explicit playback time", Float) = 0
        _DreamOrigin ("Isolated source origin", Vector) = (0,0,0,0)
        _WaveHeight ("Wave height (m)", Float) = .24
        _WavePeriod ("Wave period (s)", Float) = 6
        _WaveLength ("Wave length (m)", Float) = 7
        _ShoreWash ("Shore wash (m)", Float) = 1.8
        _FoamAmount ("Foam amount", Range(0,1.5)) = .85
        _UsePanoramaMask ("Clip to panorama water", Float) = 0
        _PanoramaWaterMask ("Panorama water visibility", Cube) = "white" {}
        _CaptureOrigin ("Fixed panorama eye", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Cull Off ZWrite Off ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "DreamOcean.cginc"
            float4 _Shallow, _Deep;
            samplerCUBE _PanoramaWaterMask;
            float _UsePanoramaMask;
            float3 _CaptureOrigin;
            struct v2f { float4 vertex:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; float2 anchor:TEXCOORD2; float crest:TEXCOORD3; };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.anchor = (mul(unity_ObjectToWorld,v.vertex).xyz-_DreamOrigin.xyz).xz;
                DreamOceanSurface(o.anchor,DreamOceanTime(),o.world,o.normal,o.crest);
                o.vertex = mul(UNITY_MATRIX_VP,float4(o.world+_DreamOrigin.xyz,1));
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float t = DreamOceanTime();
                float edge = i.world.z-DreamShoreFront(i.world.x,t);
                float visibility = 1;
                if (_UsePanoramaMask > .5)
                {
                    visibility = texCUBE(_PanoramaWaterMask, i.world + _DreamOrigin.xyz - _CaptureOrigin).r;
                    clip(visibility-.01);
                }
                else clip(edge+.04);
                float depth = max(0,i.world.y-DreamSandHeight(i.world.xz));
                float footprint = max(length(ddx(i.world.xz)),length(ddy(i.world.xz)));
                float detail = (1-smoothstep(.08,.35,footprint))*saturate(_WaveHeight/.24);
                float ripple = sin(i.world.x*2.2+i.world.z*3.9+t*2.1);
                float3 eye = _WorldSpaceCameraPos-_DreamOrigin.xyz;
                float distanceToEye = distance(i.world,eye);
                float resolvedWaves = (1-smoothstep(22,70,distanceToEye))
                    * (1-smoothstep(max(1,_WaveLength)*.10,max(1,_WaveLength)*.42,footprint));
                float3 n = normalize(lerp(float3(0,1,0),i.normal,resolvedWaves)
                    + float3(ripple*.035,0,cos(i.world.z*4.3+t*2.6)*.025)*detail*resolvedWaves);
                float3 view = normalize(eye-i.world);
                float fresnel = .04+.96*pow(1-saturate(dot(n,view)),5);
                float3 c = lerp(_Shallow.rgb,_Deep.rgb,1-exp(-depth*.45));
                float lighting = .78+.22*saturate(dot(n,normalize(float3(-.45,.8,.3))));
                c *= lighting;
                c = lerp(c,float3(.65,.81,.87),fresnel*.66);
                float sun = pow(saturate(dot(reflect(-normalize(float3(-.45,.8,.3)),n),view)),110);
                c += sun*.32*detail;
                float foam = DreamOceanFoam(i.world.xz,i.anchor,t,depth,i.crest);
                c = lerp(c,float3(.73,.88,.92),smoothstep(120,800,distanceToEye)*.55);
                float waterAlpha = (.18+.80*(1-exp(-depth*2.8)))*smoothstep(.002,.035,depth);
                // Foam is a white coverage layer, not a second attenuation of shallow-water tint.
                float foamAlpha = smoothstep(.08,.60,foam);
                float opacity = foamAlpha+waterAlpha*(1-foamAlpha);
                c = (float3(.94,.97,.93)*foamAlpha+c*waterAlpha*(1-foamAlpha))/max(opacity,.0001);
                // A masked capture replaces the photographed water, so baked crests do not show through.
                return float4(c, _UsePanoramaMask > .5 ? visibility : opacity*smoothstep(-.02,.20,edge));
            }
            ENDCG
        }
    }
}
