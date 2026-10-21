Shader "PMS/Amber CRT"
{
    Properties
    {
        [PerRendererData] _MainTex("Native UI RenderTexture",2D)="white"{}
        _Color("Tint",Color)=(1,1,1,1)
        _Mode("CRT mode: 0 off / 1 standard / 2 aged",Float)=1
        _Collapse("Host power collapse",Float)=-1
        [HideInInspector] _StencilComp("Stencil Comparison",Float)=8
        [HideInInspector] _Stencil("Stencil ID",Float)=0
        [HideInInspector] _StencilOp("Stencil Operation",Float)=0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask",Float)=255
        [HideInInspector] _StencilReadMask("Stencil Read Mask",Float)=255
        [HideInInspector] _ColorMask("Color Mask",Float)=15
    }
    SubShader
    {
        Tags {"Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
        Cull Off Lighting Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
            struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
            sampler2D _MainTex;float4 _MainTex_TexelSize,_Color;float _Mode,_Collapse;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
            float3 sampleUI(float2 p){return tex2D(_MainTex,clamp(p,float2(.0001,.0001),float2(.9999,.9999))).rgb;}
            float3 displayColor(float3 gammaColor)
            {
                #ifndef UNITY_COLORSPACE_GAMMA
                return float3(GammaToLinearSpaceExact(gammaColor.r),GammaToLinearSpaceExact(gammaColor.g),GammaToLinearSpaceExact(gammaColor.b));
                #else
                return gammaColor;
                #endif
            }
            fixed4 frag(v2f i):SV_Target
            {
                float aged=step(1.5,_Mode),curve=lerp(.008,.018,aged),zoom=lerp(1.006,1.012,aged);
                float2 q=i.uv*2-1,p=_Mode<.5?i.uv:(q*zoom*(1+curve*dot(q,q))+1)*.5;
                // Preserve the host's daily power timeline; the supplied package only owns normal CRT optics.
                float picture=1,beam=0,halfWidth=0,dotEnergy=0,radius=1.15;
                if(_Collapse>=0)
                {
                    float ms=saturate(_Collapse)*760;
                    if(ms<100)picture=1-.7*ms/100;
                    else if(ms<240)
                    {
                        float t=(ms-100)/140,height=max(1.1,600*pow(1-t,3));
                        p=(p-.5)/float2(1-.035*t,height/600)+.5;picture=.30*(1-t);beam=.20*t*t;halfWidth=356;
                    }
                    else if(ms<390){float t=(ms-240)/150;picture=0;beam=.24*exp(-2.7*t);halfWidth=356*exp(-6.2*t);dotEnergy=.085*t;}
                    else{float t=(ms-390)/370;picture=0;dotEnergy=.09*exp(-8*t);radius=1.15-.8*t;}
                }
                float inside=step(0,p.x)*step(p.x,1)*step(0,p.y)*step(p.y,1);
                if(_Collapse<0&&inside<.5)return float4(displayColor(float3(.0275,.0196,.0078)),1)*i.color;
                float2 pixel=float2(1.0/800.0,1.0/600.0);
                float3 base=sampleUI(p),left=sampleUI(p-float2(pixel.x,0)),right=sampleUI(p+float2(pixel.x,0));
                float3 up=sampleUI(p+float2(0,pixel.y)),down=sampleUI(p-float2(0,pixel.y));
                float blur=lerp(.055,.10,aged),glow=lerp(.025,.045,aged);
                float3 rgb=_Mode<.5?base:lerp(base,(left+right)*.5,blur)+max(0,(left+right+up+down)*.25-.5686)*glow;
                float scan=1-lerp(.065,.12,aged)*(.5-.5*cos(p.y*600*6.283185));
                float vignette=1-lerp(.11,.19,aged)*pow(dot(q,q)*.5,1.25);
                float mask=1-lerp(.028,.055,aged)*step(.5,fmod(floor(i.vertex.x),3));
                float2 fc=i.vertex.xy;
                #if UNITY_UV_STARTS_AT_TOP
                fc.y=_ScreenParams.y-fc.y;
                #endif
                float grain=(fmod(fmod(floor(fc.x),31)*13+fmod(floor(fc.y),31)*7,31)/30-.5)*lerp(.0035,.008,aged);
                if(_Mode>.5)rgb*=scan*vignette*mask*(1+grain);
                if(_Collapse>=0)
                {
                    rgb=lerp(float3(.012,.008,.004),rgb,picture*inside);
                    float2 d=(i.uv-.5)*float2(800,600);
                    float beamCore=exp(-dot(d/float2(max(halfWidth,.1),.55),d/float2(max(halfWidth,.1),.55))*3);
                    float beamGlow=exp(-dot(d/float2(max(halfWidth,.1)*1.03,5.5),d/float2(max(halfWidth,.1)*1.03,5.5))*3);
                    float dotCore=exp(-dot(d/float2(radius,.65),d/float2(radius,.65))*3);
                    float dotGlow=exp(-dot(d/float2(9,7),d/float2(9,7))*3);
                    rgb+=float3(1,.82,.47)*(beam*(beamCore*1.8+beamGlow*.30)+dotEnergy*(dotCore*2+dotGlow*.65));
                }
                return float4(displayColor(saturate(rgb)),1)*i.color;
            }
            ENDCG
        }
    }
}
