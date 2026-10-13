Shader "Shader Graphs/LightBeamPolar" {
	Properties {
		_CenterOffsetX ("CenterOffsetX", Range(0, 1)) = 0.5
		_CenterOffsetY ("CenterOffsetY", Range(0, 1)) = 0.5
		_ScaleX ("ScaleX", Float) = 1
		_ScaleY ("ScaleY", Float) = 1
		_RotationAngle ("RotationAngle", Range(0, 360)) = 0
		[HDR] _Color ("Color", Vector) = (1,1,1,0)
		_LightIntensity ("LightIntensity", Float) = 1
		_OpeningAngle ("OpeningAngle", Range(0, 180)) = 45
		_NoiseScale ("NoiseScale", Float) = 30
		_NoiseTiling ("NoiseTiling", Vector) = (0.2,5,0,0)
		_NoiseSpeed ("NoiseSpeed", Vector) = (-0.02,0,0,0)
		_InnerTransparensyRadius ("InnerTransparensyRadius", Range(0, 1)) = 0.2
		_OuterTransparencyRadius ("OuterTransparencyRadius", Range(0, 1)) = 0
		_DissolveAngleDelta ("DissolveAngleDelta", Range(0.001, 1)) = 0.05
		_ParallelDissolvePower ("ParallelDissolvePower", Range(0, 1)) = 0.2
		_DistanceStartsFadeOut ("DistanceStartsFadeOut", Float) = 0.7
		_DistanceFullyTransparent ("DistanceFullyTransparent", Float) = 0.2
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}