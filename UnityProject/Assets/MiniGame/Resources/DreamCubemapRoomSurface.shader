Shader "MiniGame/Dream Cubemap Room Surface"
{
    Properties
    {
        _Tex ("Room cubemap", Cube) = "" {}
        _Yaw ("Cubemap yaw", Float) = 0
        _RoomHalfSize ("Room half size", Vector) = (1,1,1,0)
        _RoomCenter ("Room center", Vector) = (0,0,0,0)
        _ProjectionOrigin ("Fixed doorway eye", Vector) = (0,0,0,0)
        _EdgeColor ("Room edge color", Color) = (1,1,1,1)
        _EdgeWidth ("Room edge width (meters)", Range(0,0.1)) = 0.015
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Back ZWrite On ZTest LEqual
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            samplerCUBE _Tex;
            float _Yaw;
            float3 _RoomHalfSize;
            float3 _RoomCenter;
            float3 _ProjectionOrigin;
            float4 _EdgeColor;
            float _EdgeWidth;
            struct v2f
            {
                float4 vertex:SV_POSITION;
                float3 worldPosition:TEXCOORD0;
            };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(v.vertex);
                o.worldPosition=mul(unity_ObjectToWorld,v.vertex).xyz;
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float sine,cosine;sincos(_Yaw*UNITY_PI/180,sine,cosine);
                // Paint is fixed to the room. Only at the doorway eye does this
                // projection coincide with the viewer's rays and look like a skybox.
                float3 direction=normalize(i.worldPosition-_ProjectionOrigin);
                direction.xz=float2(cosine*direction.x-sine*direction.z,sine*direction.x+cosine*direction.z);
                float3 panorama=texCUBE(_Tex,direction).rgb;
                // On a face, one boundary distance is zero. The second-smallest
                // distance selects the twelve physical edges, never mesh diagonals.
                // Dynamic batching may supply world-space vertices, so derive the
                // room position from world coordinates rather than appdata.vertex.
                float3 roomPosition=i.worldPosition-_RoomCenter;
                float3 boundary=max(_RoomHalfSize-abs(roomPosition),0);
                float edgeDistance=min(max(boundary.x,boundary.y),
                    min(max(boundary.y,boundary.z),max(boundary.z,boundary.x)));
                float feather=max(fwidth(edgeDistance),0.0001);
                float edge=_EdgeWidth>0 ? 1-smoothstep(_EdgeWidth,_EdgeWidth+feather,edgeDistance) : 0;
                return float4(lerp(panorama,_EdgeColor.rgb,edge*_EdgeColor.a),1);
            }
            ENDCG
        }
    }
    Fallback Off
}
