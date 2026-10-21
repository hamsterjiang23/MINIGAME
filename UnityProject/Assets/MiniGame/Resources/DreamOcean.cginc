#ifndef MINIGAME_DREAM_OCEAN_INCLUDED
#define MINIGAME_DREAM_OCEAN_INCLUDED

// Original implementation informed by Catlike Coding's Gerstner-wave derivation
// and Roystan's depth-masked, moving/distorted foam. This fixed beach uses its
// authored analytical seabed instead of a screen-space scene-depth texture.
float _DreamTime, _UseDreamTime;
float4 _DreamOrigin;
float _WaveHeight, _WavePeriod, _WaveLength, _ShoreWash, _FoamAmount;
static const float DREAM_SEA_LEVEL = -.03;
static const float DREAM_TAU = 6.28318530718;

float DreamOceanTime() { return _UseDreamTime > .5 ? _DreamTime : _Time.y; }
float DreamOceanHash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7))) * 43758.5453); }
float DreamOceanNoise(float2 p)
{
    float2 cell = floor(p), f = frac(p); f = f*f*(3-2*f);
    return lerp(lerp(DreamOceanHash(cell),DreamOceanHash(cell+float2(1,0)),f.x),
        lerp(DreamOceanHash(cell+float2(0,1)),DreamOceanHash(cell+1),f.x),f.y);
}

float DreamShoreOffset(float2 p)
{
    float fade = 1-smoothstep(8,15,abs(p.y+1));
    return .075 * (.65*sin(p.x*.13)+.22*sin(p.x*.41)) * fade;
}
float2 DreamShoreOffsetGradient(float2 p)
{
    float u = saturate((abs(p.y+1)-8)/7);
    float fade = 1-u*u*(3-2*u);
    float shape = .075 * (.65*sin(p.x*.13)+.22*sin(p.x*.41));
    float dx = .075 * (.65*.13*cos(p.x*.13)+.22*.41*cos(p.x*.41)) * fade;
    float dz = -shape * 6*u*(1-u)/7 * sign(p.y+1);
    return float2(dx,dz);
}
float DreamSandHeight(float2 p) { return -.095-.075*p.y + DreamShoreOffset(p); }
float2 DreamSandGradient(float2 p) { return float2(0,-.075)+DreamShoreOffsetGradient(p); }
float DreamMeanShore(float x)
{
    return (-.065 + .075*(.65*sin(x*.13)+.22*sin(x*.41))) / .075;
}

// A faster incoming sheet and a longer retreat, with gently staggered lobes.
float DreamSwashPulse(float x, float t)
{
    float phase = frac(t/max(_WavePeriod,.5) + .018*sin(x*.17)+.007*sin(x*.43));
    return phase < .36 ? smoothstep(0,.36,phase) : 1-smoothstep(.36,1,phase);
}
float DreamShoreFront(float x, float t)
{
    float wash = max(0,_ShoreWash);
    float lobe = sin(x*1.35+t*.22)*.055 + sin(x*3.7-t*.31)*.025;
    return DreamMeanShore(x) + wash*(.18-1.18*DreamSwashPulse(x,t)+lobe);
}
float DreamSandWetness(float2 p, float t)
{
    float now = smoothstep(-.2,.4,p.y-DreamShoreFront(p.x,t));
    float recent = smoothstep(-.28,.5,p.y-DreamShoreFront(p.x,t-1.2))*.85;
    float older = smoothstep(-.32,.6,p.y-DreamShoreFront(p.x,t-2.5))*.55;
    return max(now,max(recent,older));
}

void DreamAddWave(float2 anchor, float t, float2 direction, float wavelength, float period,
    float amplitude, float horizontalBudget, float phaseOffset, float attenuation, float2 attenuationGradient,
    inout float3 position, inout float3 tangent, inout float3 binormal, inout float crest)
{
    float2 d = normalize(direction);
    float k = DREAM_TAU/max(wavelength,1);
    float phase = k*dot(anchor,d) + DREAM_TAU*t/max(period,.5) + phaseOffset;
    float s = sin(phase), c = cos(phase);
    float horizontal = min(amplitude*2.1,horizontalBudget/k);
    float a = amplitude*attenuation;
    float h = horizontal*attenuation;
    position += float3(d.x*h*c,a*s,d.y*h*c);
    float2 phaseGradient = k*d;
    float2 horizontalGradient = horizontal*attenuationGradient*c - h*s*phaseGradient;
    float2 verticalGradient = amplitude*attenuationGradient*s + a*c*phaseGradient;
    tangent += float3(d.x*horizontalGradient.x,verticalGradient.x,d.y*horizontalGradient.x);
    binormal += float3(d.x*horizontalGradient.y,verticalGradient.y,d.y*horizontalGradient.y);
    crest += pow(saturate(s),6) * amplitude/max(_WaveHeight,.001);
}

void DreamOceanSurface(float2 anchor, float t, out float3 position, out float3 normal, out float crest)
{
    float depth = DREAM_SEA_LEVEL-DreamSandHeight(anchor);
    float u = saturate(depth/.45);
    float attenuation = u*u*(3-2*u);
    float2 attenuationGradient = -DreamSandGradient(anchor)*(6*u*(1-u)/.45);
    float distanceToEye = distance(anchor,(_WorldSpaceCameraPos-_DreamOrigin.xyz).xz);
    float height = max(0,_WaveHeight);
    float length = max(1,_WaveLength), period = max(.5,_WavePeriod);
    position = float3(anchor.x,DREAM_SEA_LEVEL,anchor.y);
    // Keep the identity derivatives, including when all wave amplitudes are zero.
    float3 tangent = float3(1,0,0), binormal = float3(0,0,1);
    crest = 0;
    // All phases use the original anchor; horizontal steepness budgets sum to .61.
    DreamAddWave(anchor,t,float2(.08,1),length,period,
        height*.70*(1-smoothstep(25,75,distanceToEye)),.38,0,attenuation,attenuationGradient,position,tangent,binormal,crest);
    DreamAddWave(anchor,t,float2(-.27,.96),length*.69,period*.79,
        height*.22*(1-smoothstep(12,40,distanceToEye)),.16,1.4,attenuation,attenuationGradient,position,tangent,binormal,crest);
    DreamAddWave(anchor,t,float2(.38,.92),length*.38,period*.57,
        height*.08*(1-smoothstep(8,24,distanceToEye)),.07,3.1,attenuation,attenuationGradient,position,tangent,binormal,crest);
    normal = normalize(cross(binormal,tangent));
    // The shallow incoming sheet rides just above the same displaced sand surface.
    float sand = DreamSandHeight(position.xz);
    if (position.y < sand+.014)
    {
        position.y = sand+.014;
        float2 slope = DreamSandGradient(position.xz);
        normal = normalize(float3(-slope.x,1,-slope.y));
    }
}

float DreamOceanFoam(float2 p, float2 anchor, float t, float depth, float crest)
{
    float frontDistance = p.y-DreamShoreFront(p.x,t);
    float2 flow = p*float2(1.7,2.3) + float2(t*.13,t*.63);
    float2 warp = float2(DreamOceanNoise(p*.7+float2(t*.08,0)),
        DreamOceanNoise(p*.63+float2(19,-t*.1)))-.5;
    float grain = DreamOceanNoise(flow+warp*1.6);
    float ridge = 1-abs(grain*2-1);
    float breakup = smoothstep(.22,.67,DreamOceanNoise(p*.68+float2(.12*t,-.16*t)));
    float leading = exp(-pow((frontDistance-.13)/.28,2));
    float breaking = smoothstep(.30,.62,crest) * (1-smoothstep(.35,1.3,depth));
    float phase = frac((dot(anchor,normalize(float2(.08,1)))/max(_WaveLength,1)) + t/max(_WavePeriod,.5));
    float wake = smoothstep(.27,.34,phase)*(1-smoothstep(.34,.72,phase));
    wake *= (1-smoothstep(.25,1.1,depth)) * smoothstep(.1,.8,frontDistance);
    float density = saturate(leading*1.05 + breaking*.80 + wake*.28);
    float aa = max(.035,fwidth(ridge)*1.2);
    float lace = smoothstep(.87-density*.12-aa,.87-density*.12+aa,ridge);
    float footprint = max(length(ddx(p)),length(ddy(p)));
    float distantFade = 1-smoothstep(.2,.85,footprint);
    return saturate(max(0,_FoamAmount)*density*lace*(.35+.65*breakup)*distantFade);
}
#endif
