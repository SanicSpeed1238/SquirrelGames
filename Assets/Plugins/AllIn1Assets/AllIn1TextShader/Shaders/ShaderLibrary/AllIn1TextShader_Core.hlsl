#ifndef ALLIN1TEXTSHADER_CORE
#define ALLIN1TEXTSHADER_CORE

EffectsData CalculateEffectsData(FragmentData i)
{
	EffectsData res;

	res.pixelSizeMask = GET_PIXEL_SIZE_MASK(i);
	
	res.vertexOS = GET_POSITION_OS(i);
	res.vertexVS = GET_VERTEX_VS(i);
	res.viewDirWS = normalize(GET_VIEWDIR_WS(i));
	res.texelSize = GET_TEXEL_SIZE(i);
	
	uint vid = (uint)GET_VERTEX_ID(i);
	
	res.characterUVCorners = extraInfoBuffer[vid].uvCorners;
	res.uvCornersInAtlas = extraInfoBuffer[vid].uvCornersInAtlas;
	res.faceColor = GET_FACE_COLOR(i);

	float2 glyphRight = extraInfoBuffer[vid].diagonalVector.zw;
	res.glyphRight = dot(glyphRight, glyphRight) > 0.0001 ? glyphRight : float2(1.0, 0.0);
	res.quadKind = extraInfoBuffer[vid].characterCenter.w;

	float4 uvCorners = res.characterUVCorners;
	float2 quadTexels = GET_CHAR_UV_NORMALIZED(i) * float2(uvCorners.y - uvCorners.x, uvCorners.w - uvCorners.z) / res.texelSize;
	res.pixelTexels = max(length(ddx(quadTexels)), length(ddy(quadTexels)));

	return res;
}

float4 ApplyExtrude(CharacterData characterData, EffectsData effectsData, float4 inputColor)
{
	float4 res = inputColor;

#ifdef _EXTRUDE_ON
	//Composited like ApplyShadows, the finished text over the extrusion. Marks are translucent boxes, so they are not extruded
	float textCoverage = Pow_4(saturate(res.a));

	[branch]
	if (textCoverage < 0.999 && abs(GetBaseQuadKind(effectsData.quadKind) - 3.0) > 0.5)
	{
		float4 extrude = Extrude(characterData, effectsData, effectsData.pixelTexels);
		float coverage = textCoverage + extrude.a * (1.0 - textCoverage);
		float3 premultiplied = res.rgb * textCoverage + extrude.rgb * extrude.a * (1.0 - textCoverage);
		res.rgb = premultiplied / max(coverage, 0.0001);
		res.a = pow(coverage, 0.25);
	}
#endif

	return res;
}

float4 ApplyShadows(CharacterData characterData, EffectsData effectsData, float4 inputColor)
{
	float4 res = inputColor;

	float4 shadow01 = 0;
	float4 shadow02 = 0;

#ifdef DROP_SHADOW_01_ENABLED
	shadow01 = DropShadow(characterData, effectsData);
#endif

#ifdef DROP_SHADOW_02_ENABLED
	shadow02 = DropShadow02(characterData, effectsData);
#endif

	//Layer 01 over layer 02, straight alpha
	float shadowAlpha = shadow01.a + shadow02.a * (1.0 - shadow01.a);
	float3 shadowRGB = (shadow01.rgb * shadow01.a + shadow02.rgb * shadow02.a * (1.0 - shadow01.a)) / max(shadowAlpha, 0.0001);

	//Runs after every colour and lighting stage, so the finished text (face, outlines, bevel, glows) is composited
	//over the shadow and no effect ever touches the shadow colour. The pass ends with Pow_4 on the alpha, so the
	//text is composited at the coverage it will really get, and the result is stored as its fourth root so the
	//shadow opacity survives that last step unchanged
	float textCoverage = Pow_4(saturate(res.a));
	float coverage = textCoverage + shadowAlpha * (1.0 - textCoverage);
	float3 premultiplied = res.rgb * textCoverage + shadowRGB * shadowAlpha * (1.0 - textCoverage);
	res.rgb = premultiplied / max(coverage, 0.0001);
	res.a = pow(coverage, 0.25);

	return res;
}

float4 ApplyLightingEffects(float4 inputColor, EffectsData data, CharacterData mainCharacterData, CharacterData outline01Data, CharacterData outline02Data, CharacterData outline03Data)
{
	float4 res = inputColor;

#ifdef _BEVEL_ON
	//Disabled outlines keep a zero mask
	float outlinesMask = saturate(outline01Data.characterMask + outline02Data.characterMask + outline03Data.characterMask);

#if defined(_LEDS_ON)
	//Dots overhang the glyph edge and their own alpha draws them whole, so the edge must not cut the bevel through a dot
	float faceMask = saturate(1.0 - outlinesMask);
#else
	//Outside rings never overlap the face, only inside ones cut into it
	float insideOutlinesMask = 0;
#if defined(_OUTLINE01POSITION_INSIDE)
	insideOutlinesMask += outline01Data.characterMask;
#endif
#if defined(_OUTLINE02POSITION_INSIDE)
	insideOutlinesMask += outline02Data.characterMask;
#endif
#if defined(_OUTLINE03POSITION_INSIDE)
	insideOutlinesMask += outline03Data.characterMask;
#endif
	float faceMask = saturate(mainCharacterData.characterMask - insideOutlinesMask);
#endif

	//Face only by default, face plus outlines with Affect Outlines. Drop shadows are composited after this stage
#if defined(_BEVEL_AFFECT_OUTLINES_ON)
	float bevelMask = saturate(faceMask + outlinesMask);
#else
	float bevelMask = faceMask;
#endif

	res.rgb = Bevel02(mainCharacterData, data, bevelMask, outlinesMask, res.rgb);
#endif

	return res;
}

CharacterData ApplyUVEffects_FragmentStage(CharacterData data)
{
	CharacterData res = data;

#ifdef _HAND_DRAWN_ON
	res = HandDrawn(res);
#endif

#ifdef _UV_DISTORTION_ON
	res = UVDistortion(res);
#endif

#ifdef _PIXELATE_ON
	res = Pixelate(res);
#endif

#ifdef _GLITCH_ON
	res = GlitchUV(res);
#endif

#ifdef _WAVE_ON
	res = Wave(res);
#endif

#ifdef _ROUND_WAVE_ON
	res = RoundWave(res);
#endif

#ifdef _TWIST_ON
	res = Twist(res);
#endif

#ifdef _WARP_ON
	res = Warp(res);
#endif

#ifdef _DISTORTION_ON
	res.uvNormalized = Distortion(res.uvNormalized, data.timingSeed);
#endif

	return res;
}

float4 ApplyVertexEffects(float4 vertexOS, float3 shaderTime, float timingSeed, float3 characterCenter, float2 characterUVNormalized, float2 glyphRight, float2 quadSizeRS)
{
	float4 res = vertexOS;

#ifdef _VERTEX_SHAKE_ON
	res.xyz = VertexShake(res.xyz, shaderTime);
#endif

#ifdef _WIND_ON
	res.xyz = Wind(res.xyz, shaderTime, timingSeed, characterCenter, characterUVNormalized, glyphRight, quadSizeRS);
#endif

	return res;
}

float4 ApplyColorEffects(float4 inputColor, EffectsData data, CharacterData mainCharacterData, CharacterData outline01Data, CharacterData outline02Data, CharacterData outline03Data)
{
	float4 res = inputColor;

#if defined(_COLOR_RAMP_ON) && defined(_COLORRAMPSTAGE_BEFOREEFFECTS)
	res = ColorRamp(res, mainCharacterData);
#endif

#if defined(_OVERLAY_TEXTURE_ON)
	res = OverlayTexture(res, mainCharacterData, data);
#endif

#if defined(_COLOR_GRADIENT_ON)
	res = ColorGradient(res, mainCharacterData, data);
#endif

#if defined(_SHINE_ON)
	res = Shine(res, mainCharacterData, data);
#endif

#ifdef _HOLOGRAM_ON
	res = Hologram(res, mainCharacterData);
#endif

#if defined(_LEDS_ON)
	res = Led(res, mainCharacterData, data);
#endif

#if defined(OUTLINE_01_ENABLED) && !defined(_LEDS_ON) && !(defined(_COLOR_RAMP_ON) && defined(_COLORRAMPSTAGE_BEFOREEFFECTS))
	//The face colour is premultiplied by its mask, so a ring lerped over it would fade it twice and darken an antialiased edge.
	//Outside rings are stacked over each other in order, then the face goes over them; inside rings paint over the face
	float faceMask = mainCharacterData.characterMask;
	float3 ringsUnder = float3(0.0, 0.0, 0.0);

	#if !defined(_OUTLINE01POSITION_INSIDE)
	ringsUnder = lerp(ringsUnder, _Outline01Color.rgb, saturate(outline01Data.characterMask + faceMask));
	res.a += outline01Data.characterMask;
	#endif

	#if defined(OUTLINE_02_ENABLED) && !defined(_OUTLINE02POSITION_INSIDE)
	ringsUnder = lerp(ringsUnder, _Outline02Color.rgb, saturate(outline02Data.characterMask + faceMask));
	res.a += outline02Data.characterMask;
	#endif

	#if defined(OUTLINE_03_ENABLED) && !defined(_OUTLINE03POSITION_INSIDE)
	ringsUnder = lerp(ringsUnder, _Outline03Color.rgb, saturate(outline03Data.characterMask + faceMask));
	res.a += outline03Data.characterMask;
	#endif

	res.rgb += ringsUnder * (1.0 - faceMask);

	#if defined(_OUTLINE01POSITION_INSIDE)
	res = Outline(res, outline01Data.characterMask);
	#endif

	#if defined(OUTLINE_02_ENABLED) && defined(_OUTLINE02POSITION_INSIDE)
	res = Outline02(res, outline02Data.characterMask);
	#endif

	#if defined(OUTLINE_03_ENABLED) && defined(_OUTLINE03POSITION_INSIDE)
	res = Outline03(res, outline03Data.characterMask);
	#endif
#else
	//LEDs and an early Color Ramp leave a straight colour on the face, which a plain lerp blends correctly
	#if defined(OUTLINE_01_ENABLED)
	res = Outline(res, outline01Data.characterMask);
	#endif

	#if defined(OUTLINE_02_ENABLED)
	res = Outline02(res, outline02Data.characterMask);
	#endif

	#if defined(OUTLINE_03_ENABLED)
	res = Outline03(res, outline03Data.characterMask);
	#endif
#endif

	return res;
}

float4 ApplyColorEffectsAfterLighting(float4 inputColor, EffectsData effectsData, CharacterData data, CharacterData outline01Data, CharacterData outline02Data, CharacterData outline03Data)
{
	float4 res = inputColor;

#if defined(_INNER_GLOW_ON) || defined(_OUTER_GLOW_ON)
	//Disabled outlines keep a zero mask, inside ones add nothing outward and outside ones nothing inward
	float outlinesMask = outline01Data.characterMask + outline02Data.characterMask + outline03Data.characterMask;
#endif

#ifdef _INNER_GLOW_ON
	res = InnerGlow(res, data, saturate(data.characterMask - outlinesMask));
#endif

#ifdef _OUTER_GLOW_ON
	res = OuterGlow(res, data, saturate(data.characterMask + outlinesMask));
#endif

	//Colour adjustments filter everything drawn so far: lit face, outlines and glows. The drop shadow is
	//composited afterwards, so it keeps its colour
#if defined(_COLOR_GRADING_ON)
	res = ColorGrading(res, effectsData);
#endif

#ifdef _HUE_SHIFT_ON
	res.rgb = HueShift(res.rgb);
#endif

#ifdef _CONTRAST_BRIGHTNESS_ON
	res.rgb = ContrastBrightness(res.rgb);
#endif

#ifdef _POSTERIZE_ON
	res.rgb = Posterize(res.rgb);
#endif

#if defined(_GREYSCALE_ON)
	res.rgb = Greyscale(res.rgb);
#endif

#if defined(_COLOR_RAMP_ON) && !defined(_COLORRAMPSTAGE_BEFOREEFFECTS)
	res = ColorRamp(res, data);
#endif

	return res;
}

float4 ApplyAlphaEffects(float4 inputColor, float2 uv, CharacterData characterData)
{
	float4 res = inputColor;

#ifdef _FLICKER_ON
	res = Flicker(res, characterData);
#endif

#ifdef _FADE_ON
	res = Fade(res, uv, characterData);
#endif

	return res;
}

#endif