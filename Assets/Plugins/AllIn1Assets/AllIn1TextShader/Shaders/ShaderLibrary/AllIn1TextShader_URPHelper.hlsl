#ifndef ALLIN1TEXT_URPHELPER
#define ALLIN1TEXT_URPHELPER

#define OBJECT_TO_WORLD_MATRIX GetObjectToWorldMatrix()
#define OBJECT_TO_CLIP_SPACE(v) TransformObjectToHClip(v.vertex.xyz)
#define OBJECT_TO_CLIP_SPACE_FLOAT4(pos) TransformObjectToHClip(pos.xyz)

float3 GetViewDirWS(float3 vertexWS)
{
	float3 res = GetWorldSpaceViewDir(vertexWS);
	return res;
}

#endif //ALLIN1TEXT_URPHELPER