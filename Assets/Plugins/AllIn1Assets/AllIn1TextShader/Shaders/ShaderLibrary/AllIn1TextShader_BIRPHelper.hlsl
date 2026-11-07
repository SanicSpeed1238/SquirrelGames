#ifndef ALLIN1TEXTSHADER_BIRPHELPER
#define ALLIN1TEXTSHADER_BIRPHELPER

#define OBJECT_TO_WORLD_MATRIX unity_ObjectToWorld
#define OBJECT_TO_CLIP_SPACE(v) UnityObjectToClipPos(v.vertex)
#define OBJECT_TO_CLIP_SPACE_FLOAT4(pos) UnityObjectToClipPos(pos)

float3 GetViewDirWS(float3 vertexWS)
{
	float3 res = UnityWorldSpaceViewDir(vertexWS);
	return res;
}

#endif //AllIn1TextShader_BIRPHelper