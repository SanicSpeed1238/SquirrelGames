#ifndef ALLIN1TEXTSHADER_LIGHTINGEFFECTS
#define ALLIN1TEXTSHADER_LIGHTINGEFFECTS

#if defined(_SPECULAR_ON)
float3 SpecularTerm(float3 lightDir, float3 normalDir)
{
	//+Z comes out of the text toward the camera
	float3 viewDir = float3(0, 0, 1);
	//_Shininess (0 to 2) remapped to a Blinn-Phong exponent (2 to 128)
	float glossiness = exp2(lerp(1.0, 7.0, saturate(_Shininess * 0.5)));
	float3 halfVector = normalize(viewDir + lightDir);
#if defined(_BEVELSTYLE_SYMMETRIC)
	//The half vector's in plane part points along the light, so abs mirrors the far edge onto the lit one like nDotL
	float NdotH = saturate(abs(dot(normalDir.xy, halfVector.xy)) + normalDir.z * halfVector.z);
#else
	float NdotH = saturate(dot(normalDir, halfVector));
#endif
	float specularIntensity = pow(NdotH, glossiness);

	float rampEnd = max(_SpecularCutoff + 0.001, min(_SpecularCutoff + _SpecularSmoothness, 1.0));
	specularIntensity = smoothstep(_SpecularCutoff, rampEnd, specularIntensity);

	float3 res = specularIntensity * _SpecularIntensity * _SpecularColor.rgb;
	return res;
}
#endif

#ifdef _BEVEL_ON
float3 CalculateNormal(CharacterData characterData, EffectsData commonData, float outlineMask)
{
	//Same UV as the face mask, so the relief follows the margin scaled quad and the UV effects
	float2 uv = characterData.unclampedUV;

	float2 diffUV = _BevelSize * commonData.texelSize;


	float2 uvLeft = uv;
	uvLeft.x -= diffUV.x;

	float2 uvRight = uv;
	uvRight.x += diffUV.x;

	float2 uvBottom = uv;
	uvBottom.y -= diffUV.y;

	float2 uvTop = uv;
	uvTop.y += diffUV.y;

	float4 h =
	{
		SampleBlurredAtlasSDF(uvLeft, commonData.uvCornersInAtlas, commonData.texelSize),
		SampleBlurredAtlasSDF(uvRight, commonData.uvCornersInAtlas, commonData.texelSize),
		SampleBlurredAtlasSDF(uvBottom, commonData.uvCornersInAtlas, commonData.texelSize),
		SampleBlurredAtlasSDF(uvTop, commonData.uvCornersInAtlas, commonData.texelSize)
	};

	//Height rescaled by padding / _BevelSize so a straight stroke edge slopes at 45 degrees for any font padding
	float2 slope = float2(h.y - h.x, h.w - h.z) * (max(_AtlasPadding, 1.0) / _BevelSize);
	float3 normal = normalize(float3(-slope, 1.0));

#ifdef _NORMAL_MAP_ON
	float2 uvNormals = characterData.mainUV;
	uvNormals = SIMPLE_CUSTOM_TRANSFORM_TEX(uvNormals, _NormalMap);
	float4 normalMapSample = SAMPLE_GLYPH_TEX(_NormalMap, uvNormals);
	float3 unpackedNormal = UnpackNormal(normalMapSample);
	
	float faceNormalAtten = (1.0 - outlineMask) * _NormalFaceAtten;
	float outlineNormalAtten = outlineMask * _NormalOutlineAtten;
	float normalAtten = faceNormalAtten + outlineNormalAtten; 
	unpackedNormal *= normalAtten;

	normal = normalize(normal - unpackedNormal);
#endif

	return normal;
}

#if defined(_BEVELLIGHTSOURCE_GLOBAL)
//Global materials share one variant, so the blending mode is a runtime value instead of a keyword
float3 GlobalBevelBlend(float3 baseColorRGB, float3 blendColor, float blendMode)
{
	if (blendMode < 0.5) return baseColorRGB * blendColor;
	if (blendMode < 1.5) return baseColorRGB + blendColor;
	return blendColor;
}
#endif

float3 Bevel02(CharacterData characterData, EffectsData commonData, float bevelMask, float outlinesMask, float3 baseColorRGB)
{
	float3 normal = CalculateNormal(characterData, commonData, outlinesMask);
	
	float phi   = _LightElevation   * ALLIN1_DEG_TO_RAD; // above the text plane, 90 = frontal light
	float theta = _LightAngle * ALLIN1_DEG_TO_RAD; // around Z
	float3 lightColor = _LightColor.rgb * _LightIntensity;
	float lightAlpha = _LightColor.a;
	float lightCutoff = _LightCutoff;
	float lightSmoothness = _LightSmoothness;
	float4 shadowColor = _ShadowColor;
	float shadowCutoff = _ShadowCutoff;
	float shadowSmoothness = _ShadowSmoothness;

#if defined(_BEVELLIGHTSOURCE_GLOBAL)
	//Without an active BevelLightController the material values stay in use
	if (global_BevelLight.z > 0.5)
	{
		theta = global_BevelLight.x;
		phi = global_BevelLight.y;
		lightColor = global_BevelLightColor.rgb;
		lightAlpha = global_BevelLightColor.a;
		lightCutoff = global_BevelLightRamps.x;
		lightSmoothness = global_BevelLightRamps.y;
		shadowCutoff = global_BevelLightRamps.z;
		shadowSmoothness = global_BevelLightRamps.w;
		shadowColor = global_BevelShadowColor;
	}
#endif

	//The angle is in rect space like Extrude's but the normal is in the glyph's own axes, so the light is turned into them: a rotated letter keeps the text's light
	float2 glyphUp = float2(-commonData.glyphRight.y, commonData.glyphRight.x);
	float2 lightDirRS = float2(cos(theta), sin(theta));
	float2 lightDir2D = float2(dot(lightDirRS, commonData.glyphRight), dot(lightDirRS, glyphUp));
	float3 light = normalize(float3(cos(phi) * lightDir2D, sin(phi)));

	//Which way the bevel faces in the text plane, and how steep it is (mask saturates at a 30 degree slope)
	float edgeLength = length(normal.xy);
	float2 edgeDir = normal.xy / max(edgeLength, 0.0001);
	float reliefMask = saturate(edgeLength * 2.0);

#if defined(_BEVELSTYLE_SYMMETRIC)
	//Both sides of each stroke receive the same lighting
	float facing = abs(dot(edgeDir, lightDir2D));
	float nDotL = saturate(abs(dot(normal.xy, light.xy)) + normal.z * light.z);
#else
	float facing = dot(edgeDir, lightDir2D);
	float nDotL = saturate(dot(normal, light));
#endif

	//1 on the edge facing the light, -1 on the edge facing away. Elevation pulls every edge toward lit, so at 90 all
	//edges are fully lit with no shadow and at 0 the away edges are fully dark
	float lightAmount = lerp(facing, 1.0, light.z);

	float3 specularLight = 0;
#if defined(_SPECULAR_ON)
	specularLight = SpecularTerm(light, normal);
#endif

	float lightRampEnd = max(lightCutoff + 0.001, min(lightCutoff + lightSmoothness, 1.0));
	float colorT = smoothstep(lightCutoff, lightRampEnd, saturate(lightAmount)) * lightAlpha * reliefMask;
#ifdef _LIGHTBLENDING_MULTIPLY
	float3 lightTarget = baseColorRGB * lightColor;
#elif _LIGHTBLENDING_ADDITIVE
	float3 lightTarget = baseColorRGB + lightColor;
#else
	float3 lightTarget = lightColor;
#endif

	float shadowRampEnd = max(shadowCutoff + 0.001, min(shadowCutoff + shadowSmoothness, 1.0));
	float colorTShadow = smoothstep(shadowCutoff, shadowRampEnd, saturate(-lightAmount)) * shadowColor.a * reliefMask;
#ifdef _SHADOWBLENDING_MULTIPLY
	float3 shadowTarget = baseColorRGB * shadowColor.rgb;
#elif _SHADOWBLENDING_ADDITIVE
	float3 shadowTarget = baseColorRGB + shadowColor.rgb;
#else
	float3 shadowTarget = shadowColor.rgb;
#endif

#if defined(_BEVELLIGHTSOURCE_GLOBAL)
	if (global_BevelLight.z > 0.5)
	{
		lightTarget = GlobalBevelBlend(baseColorRGB, lightColor, global_BevelLightBlend.x);
		shadowTarget = GlobalBevelBlend(baseColorRGB, shadowColor.rgb, global_BevelLightBlend.y);
	}
#endif

	float3 res = baseColorRGB;
	res = lerp(res, shadowTarget, colorTShadow);
	res = lerp(res, lightTarget, colorT);

	//Relief masked like light and shadow, since the flat face would take one uniform highlight value
	res += specularLight * nDotL * reliefMask;

#ifdef _REFLECTIONS_ON
	float3 fakeViewDir = normalize(-float3(commonData.vertexVS.x, commonData.vertexVS.y, 0));

	float3 reflectedDirWS = normalize(reflect(fakeViewDir, -normal) + 1.5);
	float3 environmentSample = ALLIN1_SAMPLE_TEXCUBE (_ReflectionsMap, reflectedDirWS) * _ReflectionsAtten;

	res += environmentSample * nDotL;
#endif


	res = lerp(baseColorRGB, res, bevelMask);

	return res;
}

#endif

#if defined(_DROP_SHADOW_ON) || defined(_DROP_SHADOW_02_ON)
//Straight colour plus coverage. The colour alpha is the shadow opacity
float4 DropShadow(CharacterData characterData, EffectsData effectsData,
	float offsetX, float offsetY, float dropShadowsSize, float dropShadowsSmoothness, float4 dropShadowsColor)
{
	float offsetXScaled = -offsetX / _TextureWidth;
	float offsetYScaled = -offsetY / _TextureHeight;
	float2 offsetAmount = float2(offsetXScaled, offsetYScaled);

	float2 displacedUnclampedUV = characterData.unclampedUV + offsetAmount;

	float dropShadowSizeRemapped = lerp(-1.0, 1.0, dropShadowsSize);

	float2 displacedUV;
	float displacedOriginalSDF;
	float displacedSDF;
	float shadowReach = CalculateDropShadowReach(dropShadowsSize, characterData.faceDilate);
	SampleAtlasSDF(displacedUnclampedUV, effectsData.uvCornersInAtlas, effectsData.texelSize, shadowReach, displacedUV, displacedOriginalSDF, displacedSDF);

	float sdf = lerp(displacedSDF, displacedOriginalSDF, effectsData.pixelSizeMask);

	float shadowDilate = dropShadowSizeRemapped + characterData.faceDilate;
	shadowDilate = clamp(shadowDilate, -2.0, 2.0);

	float shadowBias = CalculateMainBias(shadowDilate);

	float shadowMask = CalculateCoverageMask(sdf, shadowBias, max(dropShadowsSmoothness, 0.001), characterData.pixelSDF);
	shadowMask = saturate(shadowMask) * dropShadowsColor.a;

	return float4(dropShadowsColor.rgb, shadowMask);
}
#endif

#ifdef _DROP_SHADOW_ON
float4 DropShadow(CharacterData characterData, EffectsData effectsData)
{
	return DropShadow(characterData, effectsData,
		_DropShadowsOffsetX, _DropShadowsOffsetY, _DropShadowsSize, _DropShadowsSmoothness, _DropShadowColor);
}
#endif

#ifdef _DROP_SHADOW_02_ON
float4 DropShadow02(CharacterData characterData, EffectsData effectsData)
{
	return DropShadow(characterData, effectsData,
		_DropShadow02_OffsetX, _DropShadow02_OffsetY, _DropShadow02_Size, _DropShadow02_Smoothness, _DropShadow02_Color);
}
#endif

#ifdef _EXTRUDE_ON
#define EXTRUDE_MAX_STEPS 32

//The letter as a prism seen at an angle: a pixel shows the nearest depth whose copy of the letter covers it. Straight colour plus coverage
float4 Extrude(CharacterData characterData, EffectsData effectsData, float pixelTexels)
{
	float2 texelSize = effectsData.texelSize;
	float4 uvCornersInAtlas = effectsData.uvCornersInAtlas;
	float spread = GetAtlasSpreadTexels();

	float2 directionGlyph = GetExtrudeDirectionGlyph(effectsData.glyphRight);
	float2 stepUV = directionGlyph * texelSize;
	float depth = max(ACCESS_PROP_FLOAT(_ExtrudeDepth), 0.0);
	float outlineWidth = ACCESS_PROP_FLOAT(_ExtrudeOutlineWidth);

	float shapeDilate = characterData.faceDilate + CalculateMaxOutsideOutlineWidth();
	float shapeBias = CalculateMainBias(shapeDilate);
	float outlineBias = CalculateMainBias(shapeDilate + outlineWidth);
	float reach = CalculateExtrudeOutlineReach(characterData.faceDilate);

	//Edge ramp in the remapped units of CalculateCharacterMask, never under a pixel
	float edgeWidth = max(max(ACCESS_PROP_FLOAT(_CustomOutlineSoftness), pixelTexels / spread), 0.001);
	float hitSDF = 0.5 + 0.5 * (shapeBias + 0.5 * edgeWidth);

	//Starting past the edge ramp keeps side colour out of the face's antialiased edge where the ray leaves the letter
	float startT = min(edgeWidth * spread, depth);
	float minStep = max(depth / (EXTRUDE_MAX_STEPS - 8), 0.5);

	float t = startT;
	float maxSDF = SDF_FAR_ALPHA;
	float maxT = startT;
	float hitT = -1.0;
	float previousT = startT;
	float previousSDF = SDF_FAR_ALPHA;

	[loop]
	for (int i = 0; i < EXTRUDE_MAX_STEPS; i++)
	{
		float sdf = SampleAtlasSDFLod(characterData.unclampedUV - stepUV * t, uvCornersInAtlas, texelSize, reach);

		if (sdf > maxSDF)
		{
			maxSDF = sdf;
			maxT = t;
		}

		if (sdf >= hitSDF)
		{
			//Secant between the last sample outside and this one
			hitT = i == 0 ? t : lerp(previousT, t, saturate((hitSDF - previousSDF) / max(sdf - previousSDF, 0.0001)));
			break;
		}

		if (t >= depth)
		{
			break;
		}

		previousT = t;
		previousSDF = sdf;

		//Texels to the shape's edge, since the field falls 0.5 per spread. 0.8 absorbs the far field's overestimate
		float distanceTexels = (hitSDF - sdf) * 2.0 * spread;
		t = min(t + max(distanceTexels * 0.8, minStep), depth);
	}

	[branch]
	if (hitT >= 0.0)
	{
		//Past the entry the ray runs into the letter; one deeper sample, capped at the back, settles the back edge's coverage
		float deeperT = min(hitT + edgeWidth * spread * 2.0 + 1.0, depth);
		maxSDF = max(maxSDF, SampleAtlasSDFLod(characterData.unclampedUV - stepUV * deeperT, uvCornersInAtlas, texelSize, reach));
	}

	float sideMask = depth > startT ? smoothstep(shapeBias, shapeBias + edgeWidth, (maxSDF - 0.5) * 2.0) : 0.0;

	//The outline also wraps the face's own edge, which the march starts past. The face's field was sampled for the outside stack
	//only; where this outline reaches past it, a floored field would pass the threshold across the glyph's whole atlas block
	float faceSDF = characterData.sdf;
	[branch]
	if (reach > CalculateOutsideStackReach(characterData.faceDilate))
	{
		faceSDF = SampleAtlasSDFLod(characterData.unclampedUV, uvCornersInAtlas, texelSize, reach);
	}
	float outlineMask = smoothstep(outlineBias, outlineBias + edgeWidth, (max(maxSDF, faceSDF) - 0.5) * 2.0);
	float ringMask = outlineWidth > 0.0 ? saturate(outlineMask - sideMask) : 0.0;

	[branch]
	if (sideMask + ringMask <= 0.0)
	{
		return float4(0.0, 0.0, 0.0, 0.0);
	}

	//Wall normal from the field at the visible point, rotated from the glyph's axes into rect space
	float shadeT = hitT >= 0.0 ? hitT : maxT;
	float2 shadeUV = characterData.unclampedUV - stepUV * shadeT;
	float2 dx = float2(texelSize.x, 0.0);
	float2 dy = float2(0.0, texelSize.y);
	float2 gradient = float2(
		SampleAtlasSDFLod(shadeUV + dx, uvCornersInAtlas, texelSize, 0.0) - SampleAtlasSDFLod(shadeUV - dx, uvCornersInAtlas, texelSize, 0.0),
		SampleAtlasSDFLod(shadeUV + dy, uvCornersInAtlas, texelSize, 0.0) - SampleAtlasSDFLod(shadeUV - dy, uvCornersInAtlas, texelSize, 0.0));
	float gradientLength = length(gradient);
	float2 normalGlyph = gradientLength > 0.00001 ? -gradient / gradientLength : directionGlyph;
	float2 glyphUp = float2(-effectsData.glyphRight.y, effectsData.glyphRight.x);
	float2 normalRS = normalGlyph.x * effectsData.glyphRight + normalGlyph.y * glyphUp;

	float theta = ACCESS_PROP_FLOAT(_ExtrudeLightAngle) * ALLIN1_DEG_TO_RAD;
	float phi = ACCESS_PROP_FLOAT(_ExtrudeLightElevation) * ALLIN1_DEG_TO_RAD;
	float3 lightColor = ACCESS_PROP_FLOAT4(_ExtrudeLightColor).rgb;
	float3 shadowColor = ACCESS_PROP_FLOAT4(_ExtrudeShadowColor).rgb;
	float lightSmoothness = ACCESS_PROP_FLOAT(_ExtrudeLightSmoothness);

#if defined(_EXTRUDELIGHTSOURCE_GLOBAL)
	//Without an active BevelLightController the material values stay in use
	if (global_BevelLight.z > 0.5)
	{
		theta = global_BevelLight.x;
		phi = global_BevelLight.y;
		lightColor = global_ExtrudeLightColor.rgb;
		shadowColor = global_ExtrudeShadowColor.rgb;
		lightSmoothness = global_ExtrudeLightRamp.x;
	}
#endif

	//The walls stand at right angles to the face, so only the light's in plane part reaches them
	float nDotL = dot(normalRS, float2(cos(theta), sin(theta))) * cos(phi);
	lightSmoothness = max(lightSmoothness, 0.001);
	float lightAmount = smoothstep(-lightSmoothness, lightSmoothness, nDotL);
	float3 lighting = lerp(shadowColor, lightColor, lightAmount);

	//faceColor comes premultiplied by the vertex alpha
	float3 letterColor = effectsData.faceColor.rgb / max(effectsData.faceColor.a, 0.0001);
	float4 sideColor = ACCESS_PROP_FLOAT4(_ExtrudeColor);
	sideColor.rgb *= lerp(float3(1.0, 1.0, 1.0), letterColor, ACCESS_PROP_FLOAT(_ExtrudeLetterTint));
	sideColor.rgb *= 1.0 - ACCESS_PROP_FLOAT(_ExtrudeDepthShade) * saturate(shadeT / max(depth, 0.0001));
	sideColor.rgb *= lighting;

	float4 outlineColor = ACCESS_PROP_FLOAT4(_ExtrudeOutlineColor);
	float sideAlpha = sideMask * sideColor.a;
	float ringAlpha = ringMask * outlineColor.a;
	float alpha = sideAlpha + ringAlpha;
	float3 rgb = (sideColor.rgb * sideAlpha + outlineColor.rgb * ringAlpha) / max(alpha, 0.0001);

	return float4(rgb, alpha);
}
#endif




#endif //ALLIN1TEXTSHADER_LIGHTINGEFFECTS