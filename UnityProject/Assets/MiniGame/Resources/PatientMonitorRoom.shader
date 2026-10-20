Shader "MiniGame/Patient Monitor Room"
{
    Properties
    {
        _Color ("Surface", Color) = (.7,.73,.68,1)
        _LightTint ("Fluorescent tint", Color) = (.90,.97,.94,1)
        _Grime ("Wear", Range(0,1)) = .25
        _Specular ("Metal reflection", Range(0,1)) = .1
        _Emission ("Diffuser emission", Range(0,2)) = 0
        _LampStrength ("Lamp", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="Always" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            float4 _Color, _LightTint;
            float _Grime, _Specular, _Emission, _LampStrength;
            float4 _LampPosition;
            float4x4 _WorldToRoom;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 position : SV_POSITION;
                float3 world : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 room : TEXCOORD2;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.room = mul(_WorldToRoom, float4(o.world, 1)).xyz;
                o.color = v.color;
                return o;
            }

            float hash(float3 p)
            {
                p = frac(p * .1031);
                p += dot(p, p.zyx + 31.32);
                return frac((p.x + p.y) * p.z);
            }

            float noise(float3 p)
            {
                float3 i = floor(p), f = frac(p);
                f = f * f * (3 - 2 * f);
                return lerp(lerp(lerp(hash(i), hash(i + float3(1,0,0)), f.x),
                    lerp(hash(i + float3(0,1,0)), hash(i + float3(1,1,0)), f.x), f.y),
                    lerp(lerp(hash(i + float3(0,0,1)), hash(i + float3(1,0,1)), f.x),
                    lerp(hash(i + float3(0,1,1)), hash(i + 1), f.x), f.y), f.z);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 n = normalize(i.normal);
                float3 toLamp = _LampPosition.xyz - i.world;
                float distanceSquared = max(dot(toLamp, toLamp), .05);
                float3 l = toLamp * rsqrt(distanceSquared);
                float diffuse = max(dot(n, l), 0);
                float attenuation = 1.35 / (1 + distanceSquared * .10);
                float softFill = .16 * saturate(dot(n, normalize(float3(-.2,.6,-1))) * .5 + .5);
                float hemisphere = lerp(.21, .36, n.y * .5 + .5);
                float roomCorner = saturate((3.3 - abs(i.room.x)) * 2.5)
                    * saturate((2.3 - i.room.z) * 3.0);
                float nearFloor = smoothstep(0, .4, i.room.y);
                float ao = lerp(.67, 1, roomCorner) * lerp(.72, 1, nearFloor);
                // Soft contact under the fixed furnishings, independent of the host scene's shadow settings.
                float2 bedEdge = abs(i.room.xz - float2(1.43,.72)) - float2(.82,1.1);
                float bedContact = 1 - smoothstep(-.03,.19,max(bedEdge.x,bedEdge.y));
                float2 deskEdge = abs(i.room.xz - float2(-2.35,-.93)) - float2(.56,.34);
                float deskContact = 1 - smoothstep(-.03,.19,max(deskEdge.x,deskEdge.y));
                float floorSurface = (1 - smoothstep(.06,.16,i.room.y)) * saturate(n.y);
                ao *= 1 - floorSurface * max(bedContact * .23, deskContact * .13);

                float mottling = noise(i.room * 5.4) * .65 + noise(i.room * 17.1) * .35;
                float lowDirt = (1 - smoothstep(.05, 1.7, i.room.y)) * .22;
                float stain = smoothstep(.49, .79, noise(i.room * float3(3.1,1.15,3.1))) * .24;
                float wear = _Grime * (lowDirt + stain + mottling * .12);
                float grain = noise(i.room * 103) * .036 - .018;
                float3 albedo = _Color.rgb * (1 - wear + grain) * i.color.rgb;
                albedo = lerp(albedo, albedo * float3(.85,.81,.69), wear * .5);
                float3 v = normalize(_WorldSpaceCameraPos.xyz - i.world);
                float3 h = normalize(l + v);
                float specular = pow(max(0, dot(n, h)), 38) * _Specular * attenuation;
                float3 lighting = (hemisphere + softFill) * float3(.83,.89,.87)
                    + diffuse * attenuation * .67 * _LightTint.rgb * _LampStrength;
                float3 color = albedo * lighting * ao + specular * _LightTint.rgb;
                color += _Color.rgb * _Emission * _LampStrength;
                return float4(max(color, 0), 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
