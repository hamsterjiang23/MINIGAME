Shader "MiniGame/Dream Clouds"
{
    Properties
    {
        _DreamTime ("Playback time", Float) = 0
        _CloudSpeed ("Cloud speed", Float) = 0.006
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
            float _DreamTime, _CloudSpeed;
            struct v2f { float4 vertex:SV_POSITION; float3 direction:TEXCOORD0; };
            v2f vert(appdata_base v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.direction=v.vertex.xyz; return o; }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
            }
            float fbm(float2 p)
            {
                float f=0, a=.5;
                for(int j=0;j<5;j++) { f+=a*noise(p); p=p*2.03+float2(11.3,7.1); a*=.5; }
                return f;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float3 ray=normalize(i.direction);
                float h=saturate(ray.y);
                float3 sky=lerp(float3(.78,.9,.95),float3(.23,.60,.87),pow(h,.45));
                float2 p=ray.xz/max(ray.y+.27,.1)*3.1;
                p+=float2(_DreamTime*_CloudSpeed,_DreamTime*_CloudSpeed*.34);
                float shape=fbm(p+fbm(p*.47)*1.6);
                float clouds=smoothstep(.48,.69,shape)*smoothstep(-.015,.16,ray.y);
                float shade=fbm(p+float2(.13,.22));
                float3 white=lerp(float3(.77,.84,.89),float3(1,.99,.96),smoothstep(.35,.65,shade));
                sky=lerp(sky,white,clouds);
                float sun=pow(saturate(dot(ray,normalize(float3(-.45,.8,.3)))),260);
                sky+=sun*.5;
                return float4(sky,1);
            }
            ENDCG
        }
    }
}
