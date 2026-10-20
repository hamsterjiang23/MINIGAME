Shader "PMS/Amber CRT"
{
    Properties { [PerRendererData] _MainTex("Native UI",2D)="black"{} _Mode("CRT",Float)=1 _Collapse("Power collapse",Float)=-1 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            sampler2D _MainTex; float _Mode, _Collapse;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o; }
            float3 sampleUI(float2 p) { return tex2D(_MainTex,clamp(p,.0001,.9999)).rgb; }
            fixed4 frag(v2f i):SV_Target
            {
                float aged=step(1.5,_Mode); float2 q=i.uv*2-1;
                float2 p=_Mode<.5?i.uv:(q*lerp(1.006,1.012,aged)*(1+lerp(.008,.018,aged)*dot(q,q))+1)*.5;
                float picture=1,beam=0,halfWidth=0,dotEnergy=0,radius=1.15;
                if(_Collapse>=0)
                {
                    float ms=saturate(_Collapse)*760;
                    if(ms<100) picture=1-.7*ms/100;
                    else if(ms<240)
                    {
                        float t=(ms-100)/140; float height=max(1.1,600*pow(1-t,3));
                        p=(p-.5)/float2(1-.035*t,height/600)+.5; picture=.30*(1-t); beam=.20*t*t; halfWidth=356;
                    }
                    else if(ms<390) { float t=(ms-240)/150; picture=0; beam=.24*exp(-2.7*t); halfWidth=356*exp(-6.2*t); dotEnergy=.085*t; }
                    else { float t=(ms-390)/370; picture=0; dotEnergy=.09*exp(-8*t); radius=1.15-.8*t; }
                }
                float mask=step(0,p.x)*step(p.x,1)*step(0,p.y)*step(p.y,1);
                float3 rgb=sampleUI(p);
                if(_Mode>.5)
                {
                    float2 px=float2(1.0/800,1.0/600);
                    float3 l=sampleUI(p-float2(px.x,0)),r=sampleUI(p+float2(px.x,0));
                    float3 near=(l+r+sampleUI(p-float2(0,px.y))+sampleUI(p+float2(0,px.y)))*.25;
                    rgb=lerp(rgb,(l+r)*.5,lerp(.055,.10,aged))+max(near-.5686,0)*lerp(.025,.045,aged);
                    float scan=1-lerp(.065,.12,aged)*(.5-.5*cos(p.y*600*6.283185));
                    float vignette=1-lerp(.11,.19,aged)*pow(dot(q,q)*.5,1.25);
                    float stripe=fmod(floor(i.vertex.x),3),grit=(fmod(fmod(floor(i.vertex.x),31)*13+fmod(floor(i.vertex.y),31)*7,31)/30-.5)*lerp(.0035,.008,aged);
                    rgb*=scan*vignette*(stripe<.5?1:1-lerp(.028,.055,aged))*(1+grit);
                }
                rgb=lerp(float3(.012,.008,.004),rgb,picture*mask);
                if(_Collapse>=0)
                {
                    float2 d=(i.uv-.5)*float2(800,600);
                    float beamCore=exp(-dot(d/float2(max(halfWidth,.1),.55),d/float2(max(halfWidth,.1),.55))*3);
                    float beamGlow=exp(-dot(d/float2(max(halfWidth,.1)*1.03,5.5),d/float2(max(halfWidth,.1)*1.03,5.5))*3);
                    float dotCore=exp(-dot(d/float2(radius,.65),d/float2(radius,.65))*3);
                    float dotGlow=exp(-dot(d/float2(9,7),d/float2(9,7))*3);
                    rgb+=float3(1,.82,.47)*(beam*(beamCore*1.8+beamGlow*.30)+dotEnergy*(dotCore*2+dotGlow*.65));
                }
                return fixed4(saturate(rgb)*i.color.rgb,i.color.a);
            }
            ENDCG
        }
    }
}
