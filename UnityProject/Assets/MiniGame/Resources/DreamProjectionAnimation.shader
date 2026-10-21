Shader "MiniGame/Dream Projection Animation"
{
    Properties
    {
        _Artwork ("Artwork", 2D) = "black" {}
        _ArtworkNext ("Next artwork", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _Artwork;
            sampler2D _ArtworkNext;
            float _SequenceMode, _FrameBlend, _PostIntensity;
            float _Day, _PlaybackSeconds, _Progress, _MotionIntensity;
            float hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float noise(float2 p)
            {
                float2 cell = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(cell), hash(cell + float2(1,0)), f.x),
                    lerp(hash(cell + float2(0,1)), hash(cell + 1), f.x), f.y);
            }
            float field(float2 p) { return noise(p) * .6 + noise(p * 2.17) * .28 + noise(p * 4.31) * .12; }
            float ellipse(float2 p, float2 radius) { return 1 - smoothstep(.75, 1, length(p / radius)); }
            float segment(float2 p, float2 a, float2 b)
            {
                float2 v = b - a;
                return length(p - a - v * saturate(dot(p - a, v) / max(dot(v,v), .00001)));
            }
            float3 artwork(float2 uv) { return tex2D(_Artwork, clamp(uv, .001, .999)).rgb; }

            float3 beach(float2 uv, float t)
            {
                float water = (1 - smoothstep(.51, .59, uv.y)) * (1 - smoothstep(.62, 1, uv.x));
                float posts = max(ellipse(uv - float2(.112,.24),float2(.075,.255)),
                    ellipse(uv - float2(.30,.30),float2(.055,.20)));
                water *= 1 - posts;
                float2 drift = float2(sin(uv.y * 61 - t * 1.1), cos(uv.x * 32 + t * .73)) * .003;
                float3 color = artwork(uv + drift * water * _MotionIntensity);
                float foamPhase = frac(uv.y * 5.2 + sin(uv.x * 13 + t * .27) * .15 - t * .10);
                float foam = smoothstep(.88, .97, foamPhase) * (1 - smoothstep(.97, 1, foamPhase));
                foam *= smoothstep(.25, .8, field(uv * float2(74,38) + float2(t * .6, -t * .24)));
                color = lerp(color, float3(.72,.88,.84), foam * water * .26 * _MotionIntensity);
                float2 stain = (uv - float2(.46,.50)) / float2(.41,.36);
                stain.y += sin(stain.x * 9 + t * .45) * .05;
                float spreading = 1 - smoothstep(.25 + _Progress * .45, .65 + _Progress * .5,
                    length(stain) + field(uv * 19 - float2(0,t * .065)) * .25);
                float blood = spreading * water * smoothstep(.22,.72,field(uv * 24 + float2(t * .07,0)));
                color = lerp(color, color * float3(1.12,.38,.32), blood * .35 * _MotionIntensity);
                [unroll] for (int i = 0; i < 6; i++)
                {
                    float seed = i * 1.37;
                    float2 center = float2(frac(.11 + i * .163 + t * (.012 + i * .001)),
                        .72 + frac(seed * .31) * .19 + sin(t * .55 + seed) * .012);
                    float size = .012 + frac(seed * .7) * .008;
                    float2 p = (uv - center) * float2(1.7778,1) / size;
                    float flap = sin(t * (3.4 + i * .27) + seed) * .33;
                    float d = min(segment(p,float2(-.85,flap),float2(0,-.12)),segment(p,float2(0,-.12),float2(.85,flap)));
                    float bird = 1 - smoothstep(.035,.12,d);
                    color = lerp(color,float3(.10,.15,.15),bird * .83 * saturate(_MotionIntensity));
                }
                return color;
            }

            float3 road(float2 uv, float t)
            {
                const float horizon = .43;
                float depth = max(horizon - uv.y,.002);
                float ground = 1 - smoothstep(horizon - .01,horizon + .015,uv.y);
                float roadWidth = depth * 1.12;
                float roadMask = 1 - smoothstep(roadWidth * .95,roadWidth * 1.05 + .001,abs(uv.x - .5));
                float shift = min(t * .045,3) * _MotionIntensity;
                float sourceDepth = 1 / (1 / depth + shift);
                float2 sampleUv = float2(.5 + (uv.x - .5) * sourceDepth / depth, horizon - sourceDepth);
                sampleUv = lerp(uv,sampleUv,ground);
                float sky = smoothstep(.43,.62,uv.y);
                sampleUv.x += sin(uv.y * 7 + t * .11) * .006 * sky * _MotionIntensity;
                sampleUv.y += sin(uv.x * 5 - t * .075) * .003 * sky * _MotionIntensity;
                float3 color = artwork(sampleUv);
                float clouds = field(uv * float2(6,8) + float2(t * .018,-t * .009));
                color *= 1 - (clouds - .45) * .23 * sky * _MotionIntensity;
                float worldDepth = 1 / max(depth,.018);
                float dashed = smoothstep(.15,.21,frac(worldDepth - t * 1.4))
                    * (1 - smoothstep(.62,.7,frac(worldDepth - t * 1.4)));
                float centerLine = 1 - smoothstep(.002 + depth * .008,.004 + depth * .013,abs(uv.x - .5));
                color = lerp(color,float3(.77,.75,.62),centerLine * dashed * ground * .42 * saturate(_MotionIntensity));
                float rush = noise(float2(uv.x * 230,worldDepth * 3 - t * 6));
                color += (rush - .5) * .035 * ground * roadMask * _MotionIntensity;
                return color;
            }

            float3 judgment(float2 uv, float t)
            {
                float hands = max(ellipse(uv - float2(.18,.23),float2(.23,.28)),ellipse(uv - float2(.82,.23),float2(.23,.28)));
                float2 tremor = float2(sin(t * 3.1) + .3 * sin(t * 7.7),cos(t * 2.7)) * .003;
                float band = step(.84,noise(float2(floor(uv.y * 58),floor(t * 2.2))));
                float2 sampleUv = uv + tremor * hands * _MotionIntensity;
                sampleUv.x += band * sin(t * 9 + uv.y * 60) * .005 * _MotionIntensity;
                float3 color = artwork(sampleUv);
                float judges = 0;
                [unroll] for (int i = 0; i < 3; i++)
                {
                    float2 center = float2(.31 + i * .19 + sin(t * .43 + i) * .003,.66);
                    judges = max(judges,ellipse(uv-center,float2(i == 1 ? .077 : .06,.14)));
                }
                color *= 1 - judges * (.10 + .08 * sin(t * .81)) * _MotionIntensity;
                float light = ellipse(uv - float2(.5,.82),float2(.30,.48));
                color *= 1 + light * (.035 * sin(t * 7.1) + .025 * sin(t * 2.3)) * _MotionIntensity;
                [unroll] for (int side = 0; side < 2; side++)
                [unroll] for (int link = 0; link < 4; link++)
                {
                    float2 center = float2(lerp(.225,.775,side) + sin(t * 2.7 + link * .3) * .003,
                        .12 + link * .033);
                    float ring = abs(length((uv - center) / float2(.008,.025)) - 1);
                    float chain = 1 - smoothstep(.08,.24,ring);
                    color = lerp(color,float3(.18,.20,.17),chain * .40 * saturate(_MotionIntensity));
                    color += chain * max(0,sin(t * 1.7 + link)) * .10 * _MotionIntensity;
                }
                float scratches = band * smoothstep(.63,.82,noise(float2(uv.x * 32,floor(uv.y * 140) + t * .2)));
                color = lerp(color,float3(.58,.075,.025),scratches * .22 * saturate(_MotionIntensity));
                return color;
            }

            float3 sequenceSample(float2 uv)
            {
                uv=clamp(uv,.001,.999);
                return lerp(tex2D(_Artwork,uv).rgb,tex2D(_ArtworkNext,uv).rgb,_FrameBlend);
            }

            float3 memorySequence(float2 uv,float t)
            {
                float motion=_MotionIntensity;
                float2 q=(uv-.5)/(1+(.012+.028*_Progress)*motion)+.5;
                q.x+=sin(t*.19)*.0015*motion;
                float post=_PostIntensity;
                float tension=smoothstep(.05,.95,_Progress);
                float a=(_Progress-.31)/.025,b=(_Progress-.66)/.022,c=(_Progress-.88)/.018;
                float interruption=exp(-a*a)+exp(-b*b)+exp(-c*c);
                float2 centered=q-.5;
                float radius=dot(centered,centered);
                float curve=(_Day<1.5 ? .035 : _Day<2.5 ? .11 : .085)*post*(.5+.5*tension);
                q=centered*(1+curve*radius)/(1+curve*.55)+.5;
                if(_Day<1.5)
                {
                    // Let the authored waves and gulls carry the scene; add only a low sea sway.
                    float water=1-smoothstep(.45,.67,uv.y);
                    q.y+=sin(uv.x*22-t*.65)*.0012*water*motion;
                    q.x+=sin(uv.y*31-t*.8)*.004*water*post*(.3+.7*tension);
                }
                else if(_Day<2.5)
                {
                    // The inverted architecture drifts almost imperceptibly above the flower field.
                    float angle=sin(t*.15)*(.002*motion+.011*post);
                    float2 p=q-.5;
                    q=float2(cos(angle)*p.x-sin(angle)*p.y,sin(angle)*p.x+cos(angle)*p.y)+.5;
                    q.y+=sin(t*.23)*.0012*motion;
                    q.y+=sin(uv.x*8+t*.4)*.003*post;
                }
                else
                {
                    q.x+=(sin(t*2.1)+.3*sin(t*3.7))*.00065*motion;
                    float band=(uv.y-frac(t*.13))/.035;
                    q.x+=exp(-band*band)*sin(t*17)*(.008+.025*interruption)*post;
                    q.x+=(hash(float2(floor(uv.y*28),floor(t*7)))-.5)*.012*interruption*post;
                }
                float3 color=sequenceSample(q);
                float2 split=(uv-.5)*(.003+.011*tension)*post*(_Day<1.5 ? .4 : _Day<2.5 ? .75 : 1);
                color.r=sequenceSample(q+split).r;
                color.b=sequenceSample(q-split).b;
                float ghost=(_Day<1.5 ? .025 : .05+.14*interruption)*post;
                color=lerp(color,sequenceSample(q+float2((.009+.025*interruption)*post,.002*sin(t*.3)*post)),saturate(ghost));
                float2 blur=float2(.0025,.0015)*post;
                float3 glow=(sequenceSample(q+blur)+sequenceSample(q-blur)
                    +sequenceSample(q+float2(blur.x,-blur.y))+sequenceSample(q+float2(-blur.x,blur.y)))*.25;
                color+=max(glow-.65,0)*.16*post;
                if(_Day>2.5)
                {
                    color.r=lerp(color.r,sequenceSample(q+float2(.0007*motion,0)).r,.35*motion);
                    color.b=lerp(color.b,sequenceSample(q-float2(.0007*motion,0)).b,.35*motion);
                    color*=1-(.012*motion+.13*post)*(.5+.5*sin(uv.y*900+t*.6));
                    color*=1-.22*interruption*post;
                }
                color*=1-smoothstep(.12,.5,radius)*.2*post;
                color+=(hash(floor(uv*float2(1920,1080))+floor(t*18))-.5)*.014*post;
                return color*(1+.009*sin(t*.8)*motion);
            }

            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 uv = input.uv;
                float t = _PlaybackSeconds;
                float3 color = _SequenceMode>.5 ? memorySequence(uv,t)
                    : _Day < 1.5 ? beach(uv,t) : _Day < 2.5 ? road(uv,t) : judgment(uv,t);
                float vignette = saturate(16 * uv.x * (1-uv.x) * uv.y * (1-uv.y));
                color *= lerp(_SequenceMode>.5 ? .92 : .84,1,pow(vignette,.2));
                float grain = hash(floor(uv * float2(1920,1080)) + floor(t * 12)) - .5;
                color += grain * (_SequenceMode>.5 ? .004 : .012) * _MotionIntensity;
                return fixed4(max(color,0),1);
            }
            ENDCG
        }
    }
    Fallback Off
}
