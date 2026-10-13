Shader "Shader Graphs/NothernLight" {
	Properties {
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_NoiseTiling ("NoiseTiling", Vector) = (3,0.07,0,0)
		_NoiseSpeed ("NoiseSpeed", Vector) = (0,0,0,0)
		_OuterMaskRadius ("OuterMaskRadius", Float) = 1
		_InnerMaskRadius ("InnerMaskRadius", Float) = 0
		_DissolvePower ("DissolvePower", Float) = 1.5
		_WobbleSpeed ("WobbleSpeed", Float) = 0.5
		_WobbleFrequency ("WobbleFrequency", Float) = 2
		_WobbleDistance ("WobbleDistance", Float) = 0.05
		_WobbleAmount ("WobbleAmount", Range(0, 1)) = 0
		[NoScaleOffset] _SampleTexture2D_b8c1e01382f34790b2593126b7a61e56_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] _SampleTexture2D_d8cbab5a498b429b8cea7038a22db34f_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
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