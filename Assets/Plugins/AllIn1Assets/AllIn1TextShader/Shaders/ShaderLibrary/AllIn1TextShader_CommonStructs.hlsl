#ifndef ALLIN1TEXTSHADER_COMMON_STRUCTS
#define ALLIN1TEXTSHADER_COMMON_STRUCTS

struct AllIn1LightData
{
	float3 lightColor;
	float3 lightDir;
	float realtimeShadow;
	float4 shadowColor;
	float distanceAttenuation;
	uint layerMask;
};

struct ShadowCoordStruct
{
	float4 _ShadowCoord : TEXCOORD0;
	float4 pos : TEXCOORD1;
};

struct FogStruct
{
	float fogCoord : TEXCOORD0;
};

struct VertexData
{
	UNITY_VERTEX_INPUT_INSTANCE_ID

	float4 vertex : POSITION;
	uint vid : SV_VertexID;
	float3 normal : NORMAL;
	float4 color : COLOR;
	float4 texcoord0 : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
};

//The toolkit stamps each vertex's index into TMP's uv2.x as -(index + 1): SV_VertexID includes the draw's base vertex on Vulkan, OpenGL and WebGPU
uint GetVertexIndex(VertexData v)
{
	uint res = v.texcoord1.x < -0.5 ? (uint)(-v.texcoord1.x - 0.5) : v.vid;
	return res;
}

#define INITIALIZE_FRAGMENT_DATA(input) \
	input.pos = 0; \
	input.interpolator_00 = 0; \
	input.interpolator_01 = 0; \
	input.interpolator_02 = 0; \
	input.interpolator_03 = 0; \
	input.interpolator_04 = 0; \
	input.interpolator_05 = 0; \
	input.interpolator_06 = 0;
	
struct FragmentData
{
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
	
	float4 pos : SV_POSITION;
	float4 interpolator_00 : TEXCOORD0;
	float4 interpolator_01 : TEXCOORD1;
	float4 interpolator_02 : TEXCOORD2;
	float4 interpolator_03 : TEXCOORD3;
	float4 interpolator_04 : TEXCOORD4;
	float4 interpolator_05 : TEXCOORD5;
	float4 interpolator_06 : TEXCOORD6;
};

struct ExtraInfo
{
	float4 uvCorners;
	float4 uvCornersInAtlas;
	float4 characterCenter;
	float characterAspectRatio;
	float4 diagonalVector; //xy: quad edge lengths in rect space, zw: the glyph's right axis
	float2 uvNormalized;
};

StructuredBuffer<ExtraInfo> extraInfoBuffer;
StructuredBuffer<float> timingSeedsBuffer;

struct EffectsData
{
	float pixelSizeMask;
	
	float2 texelSize;

	float3 vertexOS;
	float3 vertexVS;
	float3 viewDirWS;
	
	float4 characterUVCorners;
	float4 uvCornersInAtlas;
	float4 faceColor;

	float2 glyphRight; //The glyph's right axis in rect space, unit length
	float quadKind; //characterCenter.w: 1 glyph, 2 line slice, 3 mark slice
	float pixelTexels; //A screen pixel in atlas texels, from the quad's UV before any UV effect snaps or bends it
};
	
struct CharacterData
{
	float2 uvNormalized; //0-1 from bottom-left quad to top-right quad
	float2 unclampedUV;
	float2 mainUV;
	float characterMask;
	float sdf;
	float sdfBlurred;
	float bias;
	float faceDilate; //Material dilate plus the bold weight, per glyph
	float timingSeed;
	float3 shaderTime;
	float characterAspectRatio;
	float2 rectUV;
	float rectAspectRatio;
	float pixelSDF; //A screen pixel in remapped SDF units, the narrowest edge ramp. 0 leaves edges hard
};


#define CREATE_CHARACTER_DATA_WITH_BIAS(param_variable_name, param_uv_normalized, param_bias, param_vertexID) \
	CharacterData param_variable_name; \
	param_variable_name.uvNormalized = param_uv_normalized; \
	param_variable_name.unclampedUV = 0; \
	param_variable_name.mainUV = 0; \
	param_variable_name.characterMask = 0; \
	param_variable_name.sdf = 0; \
	param_variable_name.sdfBlurred = 0; \
	param_variable_name.bias = param_bias; \
	param_variable_name.faceDilate = GET_FACE_DILATE(i); \
	param_variable_name.timingSeed = timingSeedsBuffer[param_vertexID]; \
	param_variable_name.shaderTime = GET_SHADER_TIME(i); \
	param_variable_name.characterAspectRatio = extraInfoBuffer[param_vertexID].characterAspectRatio; \
	param_variable_name.rectUV = GET_RECT_UV(i); \
	param_variable_name.rectAspectRatio = ACCESS_PROP_FLOAT4(rectSize).y / max(ACCESS_PROP_FLOAT4(rectSize).x, 0.00001); \
	param_variable_name.pixelSDF = 0;
	
#define CREATE_CHARACTER_DATA(param_variable_name, param_uv_normalized, param_vertexID) CREATE_CHARACTER_DATA_WITH_BIAS(param_variable_name, param_uv_normalized, 0, param_vertexID)


#endif //ALLIN1TEXTSHADER_COMMON_STRUCTS