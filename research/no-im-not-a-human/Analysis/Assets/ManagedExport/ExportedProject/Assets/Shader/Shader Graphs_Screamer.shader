Shader "Shader Graphs/Screamer" {
	Properties {
		_Alpha ("Alpha", Range(0, 1)) = 1
		_ColorTint ("ColorTint", Vector) = (0.7735849,0.003648994,0.003648994,1)
		_Saturation ("Saturation", Range(0, 1)) = 1
		[NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
		_ShapeDistortAmount ("ShapeDistortAmount", Float) = 0.1
		_ShapeDistortScale ("ShapeDistortScale", Float) = 10
		_ShapeDistortSpeed ("ShapeDistortSpeed", Float) = 1
		_ColorDistortAmount ("ColorDistortAmount", Float) = 0.01
		_ColorEdge ("ColorEdge", Float) = 0.05
		_EdgeSaturation ("EdgeSaturation", Float) = 2
		_FaceOpacity ("FaceOpacity", Float) = 1
		_FaceSize ("FaceSize", Float) = 0.5
		_FacePosition ("FacePosition", Vector) = (0.5,0.5,0,0)
		_FaceSaturation ("FaceSaturation", Range(0, 1)) = 1
		_NoiseMirror ("NoiseMirror", Vector) = (-0.5,-0.5,0,0)
		_NoiseEdge ("NoiseEdge", Vector) = (0.3,0.95,0,0)
		_NoiseScale ("NoiseScale", Float) = 50
		_NoiseColorCount ("NoiseColorCount", Float) = 17
		_MoveSpeed ("MoveSpeed", Float) = 1
		_BackgroundOpacity ("BackgroundOpacity", Float) = 0.1
		[NoScaleOffset] _NoiseTexture ("NoiseTexture", 2D) = "white" {}
		_NoiseTiling ("NoiseTiling", Vector) = (1,1,0,0)
		_NoiseSpeed ("NoiseSpeed", Vector) = (0,0.1,0,0)
		[HDR] _VignetteColor ("VignetteColor", Vector) = (0.05856176,0.2641509,0.1342816,0)
		_VignettePower ("VignettePower", Float) = 5
		_VignetteIntensity ("VignetteIntensity", Float) = 4
		_PulsateFrequency ("PulsateFrequency", Float) = 2
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("ColorMask", Float) = 15
		[HideInInspector] _ClipRect ("ClipRect", Vector) = (0,0,0,0)
		[HideInInspector] _UIMaskSoftnessX ("UIMaskSoftnessX", Float) = 1
		[HideInInspector] _UIMaskSoftnessY ("UIMaskSoftnessY", Float) = 1
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
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}