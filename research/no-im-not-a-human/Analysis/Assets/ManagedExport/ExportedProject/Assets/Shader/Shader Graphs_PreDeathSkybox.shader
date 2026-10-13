Shader "Shader Graphs/PreDeathSkybox" {
	Properties {
		_ZenithBlend ("ZenithBlend", Range(0.01, 50)) = 1
		_NadirBlend ("NadirBlend", Range(0.01, 50)) = 1
		_HorizonBlend ("HorizonBlend", Range(0.01, 50)) = 30
		[HDR] _SkyColor ("SkyColor", Vector) = (0.0242969,0.0450251,0.245283,0)
		[HDR] _GroundColor ("GroundColor", Vector) = (0.008514151,0.008514151,0.019,0)
		[HDR] _HorizonColor ("HorizonColor", Vector) = (0.02636364,0.02636364,0.058,0)
		_StarHeight ("StarHeight", Float) = 100
		_StarPower ("StarPower", Float) = 30
		_StarIntensity ("StarIntensity", Float) = 50
		[NoScaleOffset] _SampleTexture2D_eedc1f8e77a14ab18a64a25f0c81c2c9_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
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