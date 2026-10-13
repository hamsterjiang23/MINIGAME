Shader "Shader Graphs/DeathSphere" {
	Properties {
		_NoiseScale ("NoiseScale", Float) = 15
		_NoisePower ("NoisePower", Float) = 7.16
		[HDR] _BaseColor ("BaseColor", Vector) = (0,23.6527,13.52117,0)
		_TwirlStrength ("TwirlStrength", Float) = 10
		_DistortionScale ("DistortionScale", Float) = 9
		_MoveSpeed ("MoveSpeed", Vector) = (0.01,0.01,0,0)
		_Metallic ("Metallic", Float) = 1
		_Smoothness ("Smoothness", Float) = 0
		_DisplaceAmplitudeSmall ("DisplaceAmplitudeSmall", Float) = -0.25
		_DisplaceBigAmplitude ("DisplaceBigAmplitude", Float) = 1
		_PosterizeColors ("PosterizeColors", Float) = 6
		_FreshnelPower ("FreshnelPower", Float) = 1
		_DisplaceNoiseScale ("DisplaceNoiseScale", Float) = 1
		[NoScaleOffset] _SampleTexture2DLOD_c0c41ff93f9a4615ac02e9f2858beb3e_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] _Triplanar_db662db3d9fe48679ff5545691fbb2a9_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
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

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}