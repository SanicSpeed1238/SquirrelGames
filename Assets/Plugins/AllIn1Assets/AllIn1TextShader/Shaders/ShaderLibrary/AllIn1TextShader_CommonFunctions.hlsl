#ifndef ALLIN1TEXTSHADER_COMMON_FUNCTIONS
#define ALLIN1TEXTSHADER_COMMON_FUNCTIONS

#define FLOAT_4_ZERO	float4(0, 0, 0, 0)

#define ALLIN13DSHADER_PI				3.14159265359f
#define ALLIN13DSHADER_TWO_PI			6.28318530718f
#define ALLIN13DSHADER_FOUR_PI			12.56637061436f
#define ALLIN13DSHADER_INV_PI			0.31830988618f
#define ALLIN13DSHADER_INV_TWO_PI		0.15915494309f
#define ALLIN13DSHADER_INV_FOUR_PI		0.07957747155f
#define ALLIN13DSHADER_HALF_PI			1.57079632679f
#define ALLIN13DSHADER_INV_HALF_PI		0.636619772367f
#define ALLIN1_TAU						6.283185307179586
#define ALLIN1_DEG_TO_RAD				0.01745329

#define MIN_SCALE_BLENDING				0.4
#define MAX_SCALE_BLENDING				2.0
#define SCALE_THRESHOLD_TO_CLIP			3.0
#define MESH_MARGIN_TEXEL_GUARD			1.0
#define MESH_MARGIN_UV_BUDGET_CAP		1.0
#define SDF_FLOOR_ALPHA					(1.5 / 255.0)
#define SDF_FAR_ALPHA					-1.0

#define CUSTOM_TRANSFORM_TEX(uv, increment, name) ((uv.xy + increment.xy) * ACCESS_PROP_FLOAT4(name##_ST).xy + ACCESS_PROP_FLOAT4(name##_ST).zw)
#define SIMPLE_CUSTOM_TRANSFORM_TEX(uv, name) uv.xy * ACCESS_PROP_FLOAT4(name##_ST).xy + ACCESS_PROP_TILING_AND_OFFSET(name##_ST).zw

#define DEFAULT_NORMAL_MAP_VALUE float4(1, 0.5, 0.5, 0.5)

//Textures read at the letter's own UV: Pixelate snaps it, so implicit derivatives jump at every block border and would pick a blurry mip there
#if defined(_PIXELATE_ON)
	#define SAMPLE_GLYPH_TEX(texName, uv) SAMPLE_TEX2D_LOD(texName, float4(uv, 0.0, 0.0))
#else
	#define SAMPLE_GLYPH_TEX(texName, uv) SAMPLE_TEX2D(texName, uv)
#endif

#define QUANTIZE_ALL_UVS(data, quantizeFactor) \
	data.mainUV = floor(data.mainUV * quantizeFactor) / quantizeFactor;


float SafeDivisor(float x)
{
	return abs(x) < 0.0001 ? 0.0001 : x;
}

// smoothstep is undefined on GLSL and SPIR-V when edge0 >= edge1, this gives the D3D result everywhere
float SafeSmoothstep(float edge0, float edge1, float x)
{
	float t = saturate((x - edge0) / SafeDivisor(edge1 - edge0));
	return t * t * (3.0 - 2.0 * t);
}

float CalculateBias(float effectSize, float charWeight, float scale, float clampValue)
{
	float size = (charWeight + effectSize * 0.5);
	float res = ((0.5 - size) * scale) - 0.5;
	
	res = max(res, clampValue);
	
	return res;
}


//-2 covers max dilate plus max outline/glow/shadow size; SampleAtlasSDF rebuilds the values below -1 the atlas can't hold
float CalculateMainBias(float dilate)
{
	float bias = -dilate;
	bias = clamp(bias, -2.0, 0.5);

	return bias;
}

//Without the far field rebuild the atlas floor is a flat -1 over the glyph's block, and a ramp starting below it would fill the block.
//A threshold already past the floor starts at itself. Continuous in bias, so an edge never jumps where a varying dilate crosses the floor
float ClampToAtlasFloor(float rampStart, float bias)
{
	float atlasFloor = SDF_FLOOR_ALPHA * 2.0 - 1.0;
	return max(rampStart, min(bias, atlasFloor));
}

//The text is drawn at this mask to the 4th power, so the narrowest ramp is two pixels starting 3/4 of it outside the edge, where that power crosses half
float CalculateCharacterMask(float sdf, float bias, float smoothness, float pixelSDF)
{
	float sdfRemaped = (sdf - 0.5) * 2.0;
	float width = max(smoothness, 2.0 * pixelSDF);
	float start = ClampToAtlasFloor(bias - 0.75 * (width - smoothness), bias);

	float res = SafeSmoothstep(start, start + width, sdfRemaped);
	res = saturate(res);

	return res;
}

//Drop shadows are composited at their own coverage, so the narrowest ramp is one pixel centred on the edge
float CalculateCoverageMask(float sdf, float bias, float smoothness, float pixelSDF)
{
	float sdfRemaped = (sdf - 0.5) * 2.0;
	float width = max(smoothness, pixelSDF);
	float start = ClampToAtlasFloor(bias - 0.5 * (width - smoothness), bias);

	float res = SafeSmoothstep(start, start + width, sdfRemaped);
	res = saturate(res);

	return res;
}

//RectMask2D as uGUI and TMP do it: a one pixel antialiased edge, widened by the mask's Softness. Fragment stage only
float UIRectClip(float2 positionOS)
{
	float4 clipRect = clamp(_ClipRect, -2e10, 2e10);
	float2 pixelSize = max(fwidth(positionOS), 0.00001);
	float2 inside = clipRect.zw - clipRect.xy - abs(positionOS * 2.0 - clipRect.xy - clipRect.zw);
	float2 res = saturate(inside * (0.25 / (0.25 * float2(_UIMaskSoftnessX, _UIMaskSoftnessY) + pixelSize)));

	return res.x * res.y;
}

float CalculateCharacterMaskNoSmoothness(float sdf, float bias)
{
	float sdfRemaped = (sdf - 0.5) * 2.0;
	
	float res = step(bias, sdfRemaped);
	
	return res;
}

float GammaToLinear(float value)
{
	return (value <= 0.04045) ? value / 12.92 : pow((value + 0.055) / 1.055, 2.4);
}

float3 UIGammaToLinear(float3 color)
{
	return float3(GammaToLinear(color.r), GammaToLinear(color.g), GammaToLinear(color.b));
}

float4 UIGammaToLinear(float4 color)
{
	return float4(GammaToLinear(color.r), GammaToLinear(color.g), GammaToLinear(color.b), color.a);
}	
	
float Pow_5(float x)
{
	float x2 = x * x;
	float res = x2 * x2 * x;
	return res;
}

float Pow_4(float x)
{
	float res = x * x * x * x;
	return res;
}

float Pow_16(float x)
{
	float auxPow4 = Pow_4(x);
	float res = auxPow4 * auxPow4;
	return res;
}

float EaseOutQuint(float x) 
{
	return 1 - Pow_5(1 - x);
}

float RemapFloat(float inValue, float inMin, float inMax, float outMin, float outMax)
{
	return outMin + (inValue - inMin) * (outMax - outMin) / (inMax - inMin);
}

float3 RemapFloat3(float3 inValue, float3 inMin, float3 inMax, float3 outMin, float3 outMax)
{
	float3 res = 
		float3
		(
			RemapFloat(inValue.x, inMin.x, inMax.x, outMin.x, outMax.x),
			RemapFloat(inValue.y, inMin.y, inMax.y, outMin.y, outMax.y),
			RemapFloat(inValue.z, inMin.z, inMax.z, outMin.z, outMax.z)
		);

	return res;
}

float GetLuminanceRaw(float4 col)
{
	float res = 0.3 * col.r + 0.59 * col.g + 0.11 * col.b;
	return res;
}

float GetLuminance(float4 col)
{
	return GetLuminanceRaw(col);
}

float GetLuminance(float3 col)
{
	return GetLuminance(float4(col, 1.0));
}

float OverlayFloat(float base, float blend)
{
	float res = 0.0;
	if(base <= 0.5)
	{
		res = 2.0 * base * blend;
	}
	else
	{
		res = 1.0 - 2.0 * (1.0 - base) * (1.0 - blend);
	}
	
	return res;
}

float3 OverlayColor(float3 inputCol, float3 overlayCol)
{
	float3 res = inputCol;
	
	res.rgb = (
		OverlayFloat(inputCol.r, overlayCol.r),
		OverlayFloat(inputCol.g, overlayCol.g),
		OverlayFloat(inputCol.b, overlayCol.b)
	);
	
	return res;
}

float noise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float noise2D(float2 p)
{
    float2 ip = floor(p);
    float2 fp = frac(p);
    fp = fp * fp * (3 - 2 * fp);
                
    float n00 = noise(ip);
    float n01 = noise(ip + float2(0, 1));
    float n10 = noise(ip + float2(1, 0));
    float n11 = noise(ip + float2(1, 1));
                
    return lerp(lerp(n00, n01, fp.y), lerp(n10, n11, fp.y), fp.x);
}

float3 BlendingUnpackedNormals(float3 unpackedNormal0, float3 unpackedNormal1)
{
	float2 unpackedBlendedNormalXY	= unpackedNormal0.xy + unpackedNormal1.xy;
	float unpackedBlendedNormalZ	= unpackedNormal0.z * unpackedNormal1.z;

	float3 res	= normalize(float3(unpackedBlendedNormalXY.x, unpackedBlendedNormalXY.y, unpackedBlendedNormalZ));
	return res;
}

#ifdef REQUIRE_SCENE_DEPTH
float ComputeEyeDepth(float3 vertexWS)
{
	float3 positionVS = mul(UNITY_MATRIX_V, float4(vertexWS, 1.0)).xyz;
	float res = -positionVS.z;
	
	return res;
}

float GetNormalizedDepth(float4 projPos)
{
	float nearClip = _ProjectionParams.y + global_MinDepth;
	float farClip = nearClip + global_DepthZoneLength;

	float cameraRange = farClip - nearClip;
		
	float distanceToNearClip = projPos.z - global_MinDepth;
		
	float res = distanceToNearClip / cameraRange;
	
	res = saturate(res);
	
	return res;
}

float GetEyeDepth(float3 vertexVS)
{
	return -vertexVS.z;
}

#endif

float AllIn1_GetOddNegativeScale()
{
	return unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0;
}

float rand(float2 seed, float offset) {
	return (frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453) + offset) % 1.0;
}

float rand2(float2 seed, float offset, float speed) {
	return (frac(sin(dot(seed * floor(50 + (_Time.x % 1.0) * 12. * speed), float2(127.1, 311.7))) * 43758.5453123) + offset) % 1.0;
}

float rand2CustomTime(float2 seed, float offset, float customTime, float speed) {
	return (frac(sin(dot(seed * floor(50 + (customTime % 1.0) * 12. * speed), float2(127.1, 311.7))) * 43758.5453123) + offset) % 1.0;
}

float2 GetRectUV(float3 localPos, float2 rectOrigin)
{
	float4 _rectSize = ACCESS_PROP_FLOAT4(rectSize); 
	float2 initRectPos = rectOrigin;

	//RS = Rect Space
	float3 localPosRS = mul(matrix_canvasToRect, float4(localPos.xyz, 1.0)).xyz; 
	float2 rectUV = localPosRS.xy - initRectPos;
	rectUV /= max(_rectSize.xy, 0.00001);

	return rectUV;
}

float2 CalculateClampedUV(float2 characterUVNormalized, float2 uv, float4 uvCornersInAtlas, float texelSize)
{
	float2 charUVNormalized = characterUVNormalized;
	
	float2 minUV = float2(uvCornersInAtlas.x, uvCornersInAtlas.z);
	float2 maxUV = float2(uvCornersInAtlas.y, uvCornersInAtlas.w);
	
	float2 inset = texelSize * 0.25;

	float2 res = clamp(uv, minUV + inset, maxUV - inset);
	return res;
}

//TMP's SDF ramp spans its gradient scale, padding + 1 texels, from the glyph edge down to 0. One SDF unit is that many texels
float GetAtlasSpreadTexels()
{
	return max(_AtlasPadding, 1.0) + 1.0;
}

//Outlines are measured from the face and do not stack, so the widest one is the edge the glows start from
float CalculateMaxOutsideOutlineWidth()
{
	float res = 0.0;

#if defined(OUTLINE_01_ENABLED) && !defined(_OUTLINE01POSITION_INSIDE)
	res = max(res, _Outline01Width);
#endif

#if defined(OUTLINE_02_ENABLED) && !defined(_OUTLINE02POSITION_INSIDE)
	res = max(res, _Outline02Width);
#endif

#if defined(OUTLINE_03_ENABLED) && !defined(_OUTLINE03POSITION_INSIDE)
	res = max(res, _Outline03Width);
#endif

	return res;
}

float CalculateMaxInsideOutlineWidth()
{
	float res = 0.0;

#if defined(OUTLINE_01_ENABLED) && defined(_OUTLINE01POSITION_INSIDE)
	res = max(res, _Outline01Width);
#endif

#if defined(OUTLINE_02_ENABLED) && defined(_OUTLINE02POSITION_INSIDE)
	res = max(res, _Outline02Width);
#endif

#if defined(OUTLINE_03_ENABLED) && defined(_OUTLINE03POSITION_INSIDE)
	res = max(res, _Outline03Width);
#endif

	return res;
}

//The widest outside outline sits on the dilated face and the outer glow starts past it, so the three add up
float CalculateOutsideStackReach(float faceDilate)
{
	float res = max(0.0, faceDilate) + CalculateMaxOutsideOutlineWidth();

#ifdef _OUTER_GLOW_ON
	res += _OuterGlowSize;
#endif

	return res;
}

//The size slider maps to a dilate of lerp(-1, 1, size) in DropShadow
float CalculateDropShadowReach(float dropShadowSize, float faceDilate)
{
	return max(0.0, lerp(-1.0, 1.0, dropShadowSize) + faceDilate);
}

#ifdef _EXTRUDE_ON
//The extruded shape is the face with its outside outlines, and the extrusion outline sits on top of that
float CalculateExtrudeOutlineReach(float faceDilate)
{
	return max(0.0, faceDilate) + CalculateMaxOutsideOutlineWidth() + ACCESS_PROP_FLOAT(_ExtrudeOutlineWidth);
}

//The angle is in rect space so every glyph extrudes the same way however it is rotated; returned in the glyph's own (atlas) axes
float2 GetExtrudeDirectionGlyph(float2 glyphRight)
{
	float angle = ACCESS_PROP_FLOAT(_ExtrudeAngle) * ALLIN1_DEG_TO_RAD;
	float2 directionRS = float2(cos(angle), sin(angle));
	float2 glyphUp = float2(-glyphRight.y, glyphRight.x);

	return float2(dot(directionRS, glyphRight), dot(directionRS, glyphUp));
}
#endif

//The atlas only encodes distance up to its spread; past the glyph block the field is extended analytically.
//sdfDrop decays the clamped border sample. sdfFarBound bounds the far field through the ink rect where the atlas
//floors at 0, keeping it decreasing. Thresholds past the spread need RebuildSDFPastSpread instead.
float2 ClampUVToGlyphRect(float2 unclampedUV, float4 uvCornersInAtlas, float2 texelSize, out float sdfDrop, out float sdfFarBound)
{
	float2 minUV = float2(uvCornersInAtlas.x, uvCornersInAtlas.z);
	float2 maxUV = float2(uvCornersInAtlas.y, uvCornersInAtlas.w);

	float2 inset = texelSize * 0.25;
	float2 clampedUV = clamp(unclampedUV, minUV + inset, maxUV - inset);

	float padding = max(_AtlasPadding, 1.0);
	float spread = GetAtlasSpreadTexels();

	float2 overshootTexels = (unclampedUV - clampedUV) / texelSize;
	sdfDrop = length(overshootTexels) * (0.5 / spread);

	//uvCornersInAtlas is glyphRect +- (padding - 1), so the ink rect sits (padding - 1) texels inside it
	float2 inkMinUV = minUV + (padding - 1.0) * texelSize;
	float2 inkMaxUV = maxUV - (padding - 1.0) * texelSize;
	float2 inkDistTexels = max(float2(0.0, 0.0), max(inkMinUV - unclampedUV, unclampedUV - inkMaxUV)) / texelSize;
	float farBound = 0.5 * (1.0 - length(inkDistTexels) / spread);
	sdfFarBound = (farBound < 0.0) ? farBound : 1.0;

	return clampedUV;
}

//p is at most |p - c| further from the ink than any texel c the atlas still resolves, so this never overestimates
float FarFieldCandidate(float2 pTexels, float angle, float radius, float4 uvCornersInAtlas, float2 texelSize)
{
	float2 dir;
	sincos(angle, dir.y, dir.x);

	float2 minUV = float2(uvCornersInAtlas.x, uvCornersInAtlas.z);
	float2 maxUV = float2(uvCornersInAtlas.y, uvCornersInAtlas.w);
	float2 inset = texelSize * 0.25;
	float2 sampleUV = clamp((pTexels + dir * radius) * texelSize, minUV + inset, maxUV - inset);

	float raw = SAMPLE_TEX2D_LOD(_MainTex, float4(sampleUV, 0.0, 0.0)).a;
	float res = raw - length(pTexels - sampleUV / texelSize) * (0.5 / GetAtlasSpreadTexels());

	return raw > SDF_FLOOR_ALPHA ? res : SDF_FAR_ALPHA;
}

//Past the spread the atlas reads 0: a coarse ring finds the direction to the ink, a ring landing inside the spread refines it
float RebuildSDFPastSpread(float2 unclampedUV, float4 uvCornersInAtlas, float2 texelSize)
{
	float spread = GetAtlasSpreadTexels();
	float2 pTexels = unclampedUV / texelSize;

	float best = SDF_FAR_ALPHA;
	float bestAngle = 0.0;

	[unroll]
	for (int i = 0; i < 8; i++)
	{
		float angle = i * (ALLIN1_TAU / 8.0);
		float candidate = FarFieldCandidate(pTexels, angle, 1.25 * spread, uvCornersInAtlas, texelSize);
		if (candidate > best)
		{
			best = candidate;
			bestAngle = angle;
		}
	}

	float res = SDF_FAR_ALPHA;
	[branch]
	if (best > SDF_FAR_ALPHA)
	{
		float coarseDistance = (0.5 - best) * 2.0 * spread;
		float refineRadius = clamp(coarseDistance - 0.75 * spread, 0.25 * spread, 1.25 * spread);

		float refineAngle = bestAngle;
		float refineBest = FarFieldCandidate(pTexels, refineAngle, refineRadius, uvCornersInAtlas, texelSize);
		float angleStep = ALLIN1_TAU / 16.0;

		[unroll]
		for (int j = 0; j < 2; j++)
		{
			float angleA = refineAngle - angleStep;
			float angleB = refineAngle + angleStep;
			float candidateA = FarFieldCandidate(pTexels, angleA, refineRadius, uvCornersInAtlas, texelSize);
			float candidateB = FarFieldCandidate(pTexels, angleB, refineRadius, uvCornersInAtlas, texelSize);

			if (candidateA > refineBest)
			{
				refineBest = candidateA;
				refineAngle = angleA;
			}

			if (candidateB > refineBest)
			{
				refineBest = candidateB;
				refineAngle = angleB;
			}

			angleStep *= 0.5;
		}

		res = max(best, refineBest);
	}
	
	return res;
}

//Reach r, in SDF units past the glyph edge, thresholds at 0.5 - r / 2: below the atlas floor of 0 once r passes 1, which the
//inspector's AtlasRangeNote reports. Skipped where the ink rect bound already keeps every threshold dark
bool NeedsSDFRebuild(float raw, float sdfFarBound, float reach)
{
	float lowestThreshold = 0.5 - 0.5 * reach;
	return raw <= SDF_FLOOR_ALPHA && lowestThreshold < 0.0 && sdfFarBound > lowestThreshold;
}

//reach: how far past the glyph edge, in SDF units, the caller's lowest threshold sits
void SampleAtlasSDF(float2 unclampedUV, float4 uvCornersInAtlas, float2 texelSize, float reach, out float2 clampedUV, out float d, out float dBlurred)
{
	float sdfDrop;
	float sdfFarBound;
	clampedUV = ClampUVToGlyphRect(unclampedUV, uvCornersInAtlas, texelSize, sdfDrop, sdfFarBound);

	float raw = SAMPLE_GLYPH_TEX(_MainTex, clampedUV).a;
	float rawBlurred = SAMPLE_GLYPH_TEX(_BlurredAtlas, clampedUV).a;

	d = min(raw - sdfDrop, sdfFarBound);
	dBlurred = min(rawBlurred - sdfDrop, sdfFarBound);

	[branch]
	if (NeedsSDFRebuild(raw, sdfFarBound, reach))
	{
		d = RebuildSDFPastSpread(unclampedUV, uvCornersInAtlas, texelSize);
		//The blurred atlas is the blur of the floored field, so its spill past the floor rides on the rebuilt value
		dBlurred = d + rawBlurred - raw;
	}
}

//SampleAtlasSDF for loops, where implicit derivatives are undefined. No blurred value
float SampleAtlasSDFLod(float2 unclampedUV, float4 uvCornersInAtlas, float2 texelSize, float reach)
{
	float sdfDrop;
	float sdfFarBound;
	float2 clampedUV = ClampUVToGlyphRect(unclampedUV, uvCornersInAtlas, texelSize, sdfDrop, sdfFarBound);

	float raw = SAMPLE_TEX2D_LOD(_MainTex, float4(clampedUV, 0.0, 0.0)).a;
	float res = min(raw - sdfDrop, sdfFarBound);

	[branch]
	if (NeedsSDFRebuild(raw, sdfFarBound, reach))
	{
		res = RebuildSDFPastSpread(unclampedUV, uvCornersInAtlas, texelSize);
	}

	return res;
}

float SampleBlurredAtlasSDF(float2 unclampedUV, float4 uvCornersInAtlas, float2 texelSize)
{
	float sdfDrop;
	float sdfFarBound;
	float2 clampedUV = ClampUVToGlyphRect(unclampedUV, uvCornersInAtlas, texelSize, sdfDrop, sdfFarBound);

	float res = min(SAMPLE_GLYPH_TEX(_BlurredAtlas, clampedUV).a - sdfDrop, sdfFarBound);
	return res;
}

float CalculateOutlineBiasOutside(float faceDilate, float outlineWidth)
{
	float outlineDilate = faceDilate + outlineWidth;
	outlineDilate = clamp(outlineDilate, -2.0, 2.0);

	float res = CalculateMainBias(outlineDilate);
	return res;
}

float CalculateOutlineBiasInside(float faceDilate, float outlineWidth)
{
	float outlineDilate = faceDilate - outlineWidth;
	outlineDilate = clamp(outlineDilate, -2.0, 2.0);
	float res = CalculateMainBias(outlineDilate);

	return res;
}

float CalculateOutlineMaskOutside(float outlineBias, float outlineWidth, float smoothness, float characterMask, float sdf, float accumulatedMask, float pixelSDF)
{
	float res = CalculateCharacterMask(sdf, outlineBias, smoothness, pixelSDF) - characterMask - accumulatedMask;
	res = saturate(res);
	
	return res;
}

float CalculateOutlineMaskInside(float outlineBias, float outlineWidth, float smoothness, float characterMask, float sdf, float accumulatedMask, float pixelSDF)
{
	float res = characterMask - accumulatedMask - CalculateCharacterMask(sdf, outlineBias, smoothness, pixelSDF);
	res = saturate(res);
	
	return res;
}

void ConfigureCharacterData(float4 characterUVCorners, float4 uvCornersInAtlas, float bias, float2 texelSize, float smoothness, inout CharacterData res)
{
	float2 minUV = float2(characterUVCorners.x, characterUVCorners.z);
	float2 maxUV = float2(characterUVCorners.y, characterUVCorners.w);

	float2 unclampedUV = lerp(minUV, maxUV, res.uvNormalized);

	float2 mainUV;
	float d;
	float dBlurred;
	SampleAtlasSDF(unclampedUV, uvCornersInAtlas, texelSize, CalculateOutsideStackReach(res.faceDilate), mainUV, d, dBlurred);

	res.unclampedUV = unclampedUV;
	res.mainUV = mainUV;
	res.characterMask = CalculateCharacterMask(d, bias, smoothness, res.pixelSDF);
	
	res.sdf = d;
	res.sdfBlurred = dBlurred;
	
	res.bias = bias;
}

#define INIT_SDF_AND_UV_DATA_FROM_MAIN_CHARACTER(param_mainCharacterData, param_outlineData) \
	param_outlineData.unclampedUV = param_mainCharacterData.unclampedUV; \
	param_outlineData.mainUV = param_mainCharacterData.mainUV; \
	param_outlineData.sdf = param_mainCharacterData.sdf; \
	param_outlineData.sdfBlurred = param_mainCharacterData.sdfBlurred;

#define INIT_SDF_AND_UV_DATA_ISOLATED(param_characterUVCorners, param_uvCornersInAtlas, param_texelSize, param_mainCharacterData, param_outlineData) \
	float2 minUV = float2(param_characterUVCorners.x, param_characterUVCorners.z); \
	float2 maxUV = float2(param_characterUVCorners.y, param_characterUVCorners.w); \
	float2 unclampedUV = lerp(minUV, maxUV, param_outlineData.uvNormalized); \
	float2 mainUV; \
	float d; \
	float dBlurred; \
	SampleAtlasSDF(unclampedUV, param_uvCornersInAtlas, param_texelSize, CalculateOutsideStackReach(param_mainCharacterData.faceDilate), mainUV, d, dBlurred); \
	param_outlineData.unclampedUV = unclampedUV; \
	param_outlineData.mainUV = mainUV; \
	param_outlineData.sdf = d; \
	param_outlineData.sdfBlurred = dBlurred;
	
#define CONFIGURE_OUTLINE_INSIDE(param_prefix, param_outlineData) \
	param_outlineData.bias = CalculateOutlineBiasInside(mainCharacterData.faceDilate, _Outline##param_prefix##Width); \
	param_outlineData.characterMask = CalculateOutlineMaskInside(outlineData.bias, _Outline##param_prefix##Width, _Outline##param_prefix##Smoothness, mainCharacterData.characterMask, sdfValueReference, 0, mainCharacterData.pixelSDF);
	
#define CONFIGURE_OUTLINE_OUTSIDE(param_prefix, param_outlineData) \
	param_outlineData.bias = CalculateOutlineBiasOutside(mainCharacterData.faceDilate, _Outline##param_prefix##Width); \
	param_outlineData.characterMask = CalculateOutlineMaskOutside(outlineData.bias, _Outline##param_prefix##Width, _Outline##param_prefix##Smoothness, mainCharacterData.characterMask, sdfValueReference, 0, mainCharacterData.pixelSDF);

#if defined(OUTLINE_01_ENABLED)
void ConfigureOutline01(float4 characterUVCorners, float4 uvCornersInAtlas, float2 texelSize, CharacterData mainCharacterData, inout CharacterData outlineData)
{
	INIT_SDF_AND_UV_DATA_FROM_MAIN_CHARACTER(mainCharacterData, outlineData)
	
	float sdfValueReference = outlineData.sdf;
	#if defined(_OUTLINE01SMOOTHNESSMODE_SMOOTH)
		sdfValueReference = outlineData.sdfBlurred;
	#endif
	
	#if defined(_OUTLINE01POSITION_INSIDE)
		outlineData.bias = CalculateOutlineBiasInside(mainCharacterData.faceDilate, _Outline01Width);
		outlineData.characterMask = CalculateOutlineMaskInside(outlineData.bias, _Outline01Width, _Outline01Smoothness, mainCharacterData.characterMask, sdfValueReference, 0, mainCharacterData.pixelSDF);
	#else
		outlineData.bias = CalculateOutlineBiasOutside(mainCharacterData.faceDilate, _Outline01Width);
		outlineData.characterMask = CalculateOutlineMaskOutside(outlineData.bias, _Outline01Width, _Outline01Smoothness, mainCharacterData.characterMask, sdfValueReference, 0, mainCharacterData.pixelSDF);
	#endif

	outlineData.characterMask = saturate(outlineData.characterMask);
}
#endif

#if defined(OUTLINE_02_ENABLED)
void ConfigureOutline02(float4 characterUVCorners, float4 uvCornersInAtlas, float2 texelSize, CharacterData mainCharacterData, inout CharacterData outlineData)
{
	INIT_SDF_AND_UV_DATA_FROM_MAIN_CHARACTER(mainCharacterData, outlineData)
	
	float sdfValueReference = outlineData.sdf;
	#if defined(_OUTLINE02SMOOTHNESSMODE_SMOOTH)
		sdfValueReference = outlineData.sdfBlurred;
	#endif
	
	#if defined(_OUTLINE02POSITION_INSIDE)
		CONFIGURE_OUTLINE_INSIDE(02, outlineData)
	#else
		CONFIGURE_OUTLINE_OUTSIDE(02, outlineData)
	#endif

	outlineData.characterMask = saturate(outlineData.characterMask);
}
#endif

#if defined(OUTLINE_03_ENABLED)
void ConfigureOutline03(float4 characterUVCorners, float4 uvCornersInAtlas, float2 texelSize, CharacterData mainCharacterData, inout CharacterData outlineData)
{
	INIT_SDF_AND_UV_DATA_FROM_MAIN_CHARACTER(mainCharacterData, outlineData)
	
	float sdfValueReference = outlineData.sdf;
	#if defined(_OUTLINE03SMOOTHNESSMODE_SMOOTH)
		sdfValueReference = outlineData.sdfBlurred;
	#endif
	
	#if defined(_OUTLINE03POSITION_INSIDE)
		CONFIGURE_OUTLINE_INSIDE(03, outlineData)
	#else
		CONFIGURE_OUTLINE_OUTSIDE(03, outlineData)
	#endif

	outlineData.characterMask = saturate(outlineData.characterMask);
}
#endif

//Atlas texels past the glyph rect the SDF-driven effects can reach, per axis. One SDF unit is one atlas spread of texels,
//the same scale the bias thresholds use, so this is exact up to the guard texel
//Quad kinds above 3 are Extrude's back copies of kind - 3 (4 glyph, 5 line, 6 mark)
float GetBaseQuadKind(float quadKind)
{
	return quadKind > 3.5 ? quadKind - 3.0 : quadKind;
}

//With Extrude a quad draws only its face and its back copy only the shadows and the extrusion, under every face
void GetQuadLayers(float quadKind, out bool drawsFace, out bool drawsBehind)
{
#if defined(_EXTRUDE_ON)
	drawsFace = quadKind < 3.5;
	drawsBehind = quadKind > 3.5;
#else
	drawsFace = true;
	drawsBehind = true;
#endif
}

float2 CalculateMeshMarginTexels(float faceDilate, bool drawsFace, bool drawsBehind)
{
	float spread = GetAtlasSpreadTexels();

	float2 res = drawsFace ? CalculateOutsideStackReach(faceDilate) * spread : float2(0.0, 0.0);

	if (drawsBehind)
	{
		//Shadow offsets are already atlas texels
#ifdef DROP_SHADOW_01_ENABLED
		float shadow01Extent = CalculateDropShadowReach(_DropShadowsSize, faceDilate) * spread;
		res = max(res, shadow01Extent + abs(float2(_DropShadowsOffsetX, _DropShadowsOffsetY)));
#endif

#ifdef DROP_SHADOW_02_ENABLED
		float shadow02Extent = CalculateDropShadowReach(_DropShadow02_Size, faceDilate) * spread;
		res = max(res, shadow02Extent + abs(float2(_DropShadow02_OffsetX, _DropShadow02_OffsetY)));
#endif

		//The depth itself is added one sided in the vertex stage (CalculateExtrudeMargin); this covers the outline around both ends
#ifdef _EXTRUDE_ON
		res = max(res, CalculateExtrudeOutlineReach(faceDilate) * spread);
#endif
	}

	return res + MESH_MARGIN_TEXEL_GUARD;
}

#ifdef _EXTRUDE_ON
//Signed rect units along the glyph's axes. Only the corners the extrusion points to grow by it
float2 CalculateExtrudeMargin(float2 glyphRight, float2 quadSizeRS, float2 quadSizeTexels)
{
	float2 rsPerTexel = quadSizeRS / max(quadSizeTexels, 1.0);
	return GetExtrudeDirectionGlyph(glyphRight) * max(ACCESS_PROP_FLOAT(_ExtrudeDepth), 0.0) * rsPerTexel;
}
#endif

//Fraction of the quad, per axis, the UV effects can pull a sample from. Ink can show that far past the glyph rect.
//The effects chain, so their bounds add. Capped: a whole quad per side is already a 9x fill cost
float2 CalculateMeshMarginUVBudget(float characterAspectRatio)
{
	float2 res = float2(0.0, 0.0);

#ifdef _HAND_DRAWN_ON
	res += 0.1 * ACCESS_PROP_FLOAT(_HandDrawnAmount);
#endif

#ifdef _GLITCH_ON
	res.x += 0.02 * ACCESS_PROP_FLOAT(_GlitchAmount);
#endif

#ifdef _WAVE_ON
	//Radial from the origin, so the farthest quad corner bounds it. 1.5 covers the margin band itself extending that reach
	float2 waveOrigin = float2(ACCESS_PROP_FLOAT(_WaveX), ACCESS_PROP_FLOAT(_WaveY));
	res += max(waveOrigin, 1.0 - waveOrigin) * (ACCESS_PROP_FLOAT(_WaveStrength) / 1000.0) * 1.5;
#endif

#ifdef _ROUND_WAVE_ON
	res += ACCESS_PROP_FLOAT(_RoundWaveStrength) / 10.0;
#endif

#ifdef _TWIST_ON
	//Rect-space swirl: a point inside the radius can end up anywhere on the far side of it
	float4 rect = ACCESS_PROP_FLOAT4(rectSize);
	float rectAspectRatio = rect.y / max(rect.x, 0.00001);
	res += 2.0 * ACCESS_PROP_FLOAT(_TwistUvRadius) * abs(float2(_TwistStretchX, _TwistStretchY / max(rectAspectRatio, 0.00001)));
#endif

#ifdef _WARP_ON
	res += ACCESS_PROP_FLOAT(_WarpStrength);
#endif

#ifdef _DISTORTION_ON
	//DISTORTION_IMPL remaps the noise to [-0.04, 0.16] before scaling by the amount
	res += 0.16 * ACCESS_PROP_FLOAT(_DistortAmount);
#endif

	return min(res, MESH_MARGIN_UV_BUDGET_CAP);
}

#ifdef _PIXELATE_ON
//Square blocks in rect units, _PixelateSize per em, on one grid for the whole text
float GetPixelateCellSize()
{
	return max(ACCESS_PROP_FLOAT(u_emSize), 0.0001) / max(ACCESS_PROP_FLOAT(_PixelateSize), 1.0);
}
#endif

#ifdef _LEDS_ON
//A lit dot is centered inside the glyph but its disc reaches past the edge. The extra half cell covers the edge dimming band
float CalculateLedMeshMargin()
{
	float cellRS = ACCESS_PROP_FLOAT4(rectSize).x / max(ACCESS_PROP_FLOAT(_NumLeds), 1.0);
	float dotSize = ACCESS_PROP_FLOAT(_LedSize);

#if defined(_LEDSGRID_PANEL)
	float4 panelGrid = ACCESS_PROP_FLOAT4(_LedPanelGrid);
	cellRS = panelGrid.w > 0.5 ? panelGrid.z : cellRS;
	dotSize = panelGrid.w > 0.5 ? ACCESS_PROP_FLOAT4(_LedPanelDot).x : dotSize;
#endif

	return cellRS * (0.5 * dotSize + 0.5);
}
#endif

//Rect-space units to push each quad corner outward, per axis. The quad's atlas size converts texels, its rect size converts UV fractions
float2 CalculateMeshMargin(float2 quadSizeRS, float2 quadSizeTexels, float characterAspectRatio, float faceDilate, bool drawsFace, bool drawsBehind)
{
	float2 rsPerTexel = quadSizeRS / max(quadSizeTexels, 1.0);

	float2 res = CalculateMeshMarginTexels(faceDilate, drawsFace, drawsBehind) * rsPerTexel;
	res += CalculateMeshMarginUVBudget(characterAspectRatio) * quadSizeRS;
#ifdef _LEDS_ON
	res += CalculateLedMeshMargin();
#endif
#ifdef _PIXELATE_ON
	//A block whose center is inside overhangs the edge by up to half its diagonal, whatever the glyph's rotation
	res += GetPixelateCellSize();
#endif
	res += _CharacterExtraMargin;

	//A negative extra margin may shrink the quad but never collapse it
	res = max(res, -0.45 * quadSizeRS);

	return res;
}

#endif /*ALLIN1TEXTSHADER_COMMON_FUNCTIONS*/