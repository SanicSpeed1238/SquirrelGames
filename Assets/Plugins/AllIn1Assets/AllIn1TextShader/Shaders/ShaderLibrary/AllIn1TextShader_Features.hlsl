#ifndef ALLIN1TEXTSHADER_FEATURES
#define ALLIN1TEXTSHADER_FEATURES

#if defined(INSTANCING_ON) && !defined(PROCEDURAL_INSTANCING_ON)
	#define ALLIN13D_GPU_INSTANCING
#endif

#if defined(URP_PASS)
	#define ALWAYS_DECLARE_PROPERTY
#endif

#if defined(ALLIN13D_GPU_INSTANCING)
	#define BATCHING_BUFFER_START	UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
	#define BATCHING_BUFFER_END		UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#else
	#if defined(URP_PASS) && !defined(PROCEDURAL_INSTANCING_ON)
		#define BATCHING_BUFFER_START	CBUFFER_START(UnityPerMaterial)
		#define BATCHING_BUFFER_END		CBUFFER_END
	#else
		#define BATCHING_BUFFER_START
		#define BATCHING_BUFFER_END	
	#endif
#endif

#if defined(URP_PASS) && defined(_LIGHT_LAYERS)
	#define ALLIN1_USE_LIGHT_LAYERS
#endif

#ifdef URP_PASS
	#define DECLARE_TEX2D(texName) \
		TEXTURE2D(texName); \
		SAMPLER(sampler##texName);
	#define DECLARE_TEXCUBE(texName) \
		TEXTURECUBE(texName); \
		SAMPLER(sampler##texName);

	#define SAMPLE_TEX2D(texName, uv)						SAMPLE_TEXTURE2D(texName, sampler##texName, uv)
	#define SAMPLE_TEX2D_DERIVATIVES(texName, uv, ddx, ddy) SAMPLE_TEXTURE2D_GRAD(texName, sampler##texName, uv, ddx, ddy)
	#define SAMPLE_TEX2D_LOD(texName, uv)					SAMPLE_TEXTURE2D_LOD(texName, sampler##texName, uv.xy, 0)
	#define SAMPLE_TEX2D_PROJ(texName, uv)					SAMPLE_TEXTURE2D(texName, sampler##texName, uv.xy/uv.w)
	#define ALLIN1_SAMPLE_TEXCUBE(texName, reflectedVector)	SAMPLE_TEXTURECUBE(texName, sampler##texName, reflectedVector)
#else
	#define DECLARE_TEX2D(texName)	sampler2D texName;
	#define DECLARE_TEXCUBE(texName) samplerCUBE texName;

	#define SAMPLE_TEX2D(texName, uv)						tex2D(texName, uv)
	#define SAMPLE_TEX2D_DERIVATIVES(texName, uv, ddx, ddy) tex2D(texName, uv, ddx, ddy)
	#define SAMPLE_TEX2D_LOD(texName, uv)					tex2Dlod(texName, uv)
	#define SAMPLE_TEX2D_PROJ(texName, uv)					tex2Dproj(texName, uv)
	#define ALLIN1_SAMPLE_TEXCUBE(texName, reflectedVector)	texCUBE(texName, reflectedVector)
#endif

#define DECLARE_PROPERTY_FLOAT4X4(name)	float4x4 name;

#if defined(PROCEDURAL_INSTANCING_ON)
	#define DECLARE_PROPERTY_FLOAT(name)	float name;
	#define DECLARE_PROPERTY_FLOAT2(name)	float2 name;
	#define DECLARE_PROPERTY_FLOAT3(name)	float3 name;
	#define DECLARE_PROPERTY_FLOAT4(name)	float4 name;
#else
	#if (defined(INSTANCING_ON) || defined(PROCEDURAL_INSTANCING_ON)) && !defined(URP_PASS)
		#define DECLARE_PROPERTY_FLOAT(name)	UNITY_DEFINE_INSTANCED_PROP(float, name)
		#define DECLARE_PROPERTY_FLOAT2(name)	UNITY_DEFINE_INSTANCED_PROP(float2, name)
		#define DECLARE_PROPERTY_FLOAT3(name)	UNITY_DEFINE_INSTANCED_PROP(float3, name)
		#define DECLARE_PROPERTY_FLOAT4(name)	UNITY_DEFINE_INSTANCED_PROP(float4, name)
	#else
		#define DECLARE_PROPERTY_FLOAT(name)	float name;
		#define DECLARE_PROPERTY_FLOAT2(name)	float2 name;
		#define DECLARE_PROPERTY_FLOAT3(name)	float3 name;
		#define DECLARE_PROPERTY_FLOAT4(name)	float4 name;
	#endif
#endif


//------ Shader features --------
#pragma shader_feature_local_fragment _ALLIN13D_SURFACE_TRANSPARENT
#if !defined(ALLIN1_SHADER_VARIANT)
	#include_with_pragmas "../ShaderLibrary/AllIn1TextShader_ShaderFeatures.hlsl"
#endif
//------------------------------

#include "../ShaderLibrary/AllIn1TextShader_GlobalPropertiesAndTexturesDeclaration.hlsl"


//-------- Properties Buffer -----------

BATCHING_BUFFER_START
	#include "../ShaderLibrary/AllIn1TextShader_BufferPropertiesDeclaration.hlsl"
BATCHING_BUFFER_END

//----------------------------------

#if defined(ALLIN13D_GPU_INSTANCING)
	#define ACCESS_PROP_FLOAT(name)		UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, name)
	#define ACCESS_PROP_FLOAT2(name)	UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, name)
	#define ACCESS_PROP_FLOAT3(name)	UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, name)
	#define ACCESS_PROP_FLOAT4(name)	UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, name)
	#define ACCESS_PROP_TILING_AND_OFFSET(name) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, name)

#else
	#define ACCESS_PROP_FLOAT(name)			name
	#define ACCESS_PROP_FLOAT2(name)		name
	#define ACCESS_PROP_FLOAT3(name)		name
	#define ACCESS_PROP_FLOAT4(name)		name
	#define ACCESS_PROP_TILING_AND_OFFSET(name)	name

#endif

#endif //ALLIN1TEXTSHADER_FEATURES