Shader "Hidden/MiniGame/Patient Monitor Signal"
{
    Properties { _MainTex ("Video",2D)="white" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _Clock, _Exposure, _Saturation, _Noise;
            fixed4 frag(v2f_img i):SV_Target
            {
                float2 p=i.uv*2-1;
                float2 warped=.5+.5*p*(1+.025*dot(p,p));
                float mask=step(0,warped.x)*step(warped.x,1)*step(0,warped.y)*step(warped.y,1);
                float3 c=tex2D(_MainTex,warped).rgb;
                float l=dot(c,float3(.2126,.7152,.0722))*_Exposure;
                c=saturate(l*float3(.921112,.828809,.536175)-float3(.147709,.125097,.066043));
                c=lerp(dot(c,float3(.2126,.7152,.0722)).xxx,c,_Saturation);
                c*=1-.12*dot(p,p);
                c*=.97+.03*sin(i.uv.y*420*3.14159);
                float n=frac(sin(dot(floor(i.uv*float2(864,350)),float2(12.9898,78.233))+floor(_Clock*12)*31.1)*43758.5453)-.5;
                return fixed4(saturate(c+n*_Noise)*mask,1);
            }
            ENDCG
        }
    }
}
