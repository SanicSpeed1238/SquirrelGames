#ifndef ALLIN1TEXTSHADER_COLOREFFECTS
#define ALLIN1TEXTSHADER_COLOREFFECTS

#include "../ShaderLibrary/AllIn1TextShader_LedCommon.hlsl"

//<EffectsCode>
float3 HueShift(float3 inputColor, float hueShift, float hueSaturation, float hueBrightness)
{
	float3 res = inputColor;

	float cosHsv = hueBrightness * hueSaturation * cos(hueShift * 3.14159265 / 180);
	float sinHsv = hueBrightness * hueSaturation * sin(hueShift * 3.14159265 / 180);
	res.r = (.299 * hueBrightness + .701 * cosHsv + .168 * sinHsv) * inputColor.x
		+ (.587 * hueBrightness - .587 * cosHsv + .330 * sinHsv) * inputColor.y
		+ (.114 * hueBrightness - .114 * cosHsv - .497 * sinHsv) * inputColor.z;
	res.g = (.299 * hueBrightness - .299 * cosHsv - .328 * sinHsv) *inputColor.x
		+ (.587 * hueBrightness + .413 * cosHsv + .035 * sinHsv) * inputColor.y
		+ (.114 * hueBrightness - .114 * cosHsv + .292 * sinHsv) * inputColor.z;
	res.b = (.299 * hueBrightness - .3 * cosHsv + 1.25 * sinHsv) * inputColor.x
		+ (.587 * hueBrightness - .588 * cosHsv - 1.05 * sinHsv) * inputColor.y
		+ (.114 * hueBrightness + .886 * cosHsv - .203 * sinHsv) * inputColor.z;

	return res;
}

float3 ContrastBrightness(float3 inputColor, float contrast, float brightness)
{
	float3 res = max(0, (inputColor - float3(0.5, 0.5, 0.5)) * contrast + float3(0.5, 0.5, 0.5) + brightness);
	return res;
}

#ifdef _HUE_SHIFT_ON
float3 HueShift(float3 inputColor)
{
	float3 res = HueShift(inputColor, ACCESS_PROP_FLOAT(_HueShift), ACCESS_PROP_FLOAT(_HueSaturation), ACCESS_PROP_FLOAT(_HueBrightness));
	return res;
}
#endif

#ifdef _HOLOGRAM_ON
float4 Hologram(float4 inputColor, CharacterData characterData)
{
	float4 res = inputColor;
	float4 hologramColor = ACCESS_PROP_FLOAT4(_HologramColor);
	float time = characterData.shaderTime.y;

	//Rect space measured in rect heights, so lines run continuously across every glyph and stay evenly spaced when rotated
	float2 rectPos = characterData.rectUV - 0.5;
	rectPos.x /= max(characterData.rectAspectRatio, 0.0001);

	float sinAngle, cosAngle;
	sincos(ACCESS_PROP_FLOAT(_HologramRotation) * ALLIN1_DEG_TO_RAD, sinAngle, cosAngle);
	float scanCoord = sinAngle * rectPos.x + cosAngle * rectPos.y;

	float lineCoord = scanCoord * ACCESS_PROP_FLOAT(_HologramFrequency) + time * ACCESS_PROP_FLOAT(_HologramScrollSpeed);
	float lineDistance = abs(frac(lineCoord) - 0.5);
	float lineHalfWidth = ACCESS_PROP_FLOAT(_HologramLineWidth) * 0.5;
	//fwidth keeps dense lines from shimmering on small text
	float lineEdge = max(lineHalfWidth * ACCESS_PROP_FLOAT(_HologramLineSmoothness), fwidth(lineCoord));
	float lineMask = 1.0 - SafeSmoothstep(lineHalfWidth - lineEdge, lineHalfWidth + lineEdge, lineDistance);

	float accentCoord = scanCoord * ACCESS_PROP_FLOAT(_HologramAccentFrequency) + time * ACCESS_PROP_FLOAT(_HologramAccentSpeed);
	float accentDistance = abs(frac(accentCoord) - 0.5);
	float accentMask = 1.0 - SafeSmoothstep(0.0, ACCESS_PROP_FLOAT(_HologramAccentWidth) * 0.5, accentDistance);
	accentMask *= accentMask;

	float4 hologram = res;
	hologram.rgb = lerp(res.rgb, res.rgb * hologramColor.rgb, ACCESS_PROP_FLOAT(_HologramTint));
	float glow = lineMask * ACCESS_PROP_FLOAT(_HologramLineGlow) + accentMask * ACCESS_PROP_FLOAT(_HologramAccentGlow);
	hologram.rgb += hologramColor.rgb * glow * res.a;

	//The pass ends with Pow_4 on the alpha, so the fourth root makes Base Alpha the opacity actually seen between lines
	float opacity = lerp(ACCESS_PROP_FLOAT(_HologramBaseAlpha), 1.0, saturate(lineMask + accentMask));
	hologram.a *= sqrt(sqrt(opacity));

	//hologram is built from the premultiplied face, so weighting it by the mask again would leave the plain face on antialiased edges
	res = lerp(res, hologram, hologramColor.a);

	return res;
}
#endif


#ifdef _COLOR_RAMP_ON
float4 ColorRamp(float4 inputColor, CharacterData characterData)
{
	float luminance = GetLuminance(inputColor);
	luminance = saturate(luminance + ACCESS_PROP_FLOAT(_ColorRampLuminosity));

	float2 rampUV = float2(luminance, 0);
	rampUV.x *= ACCESS_PROP_FLOAT(_ColorRampTiling);
	rampUV.x += frac(characterData.shaderTime.x * ACCESS_PROP_FLOAT(_ColorRampScrollSpeed));

	float3 colorRamp = SAMPLE_TEX2D(_ColorRampTex, rampUV).rgb;
	float3 resRGB = lerp(inputColor.rgb, colorRamp, ACCESS_PROP_FLOAT(_ColorRampBlend));

	float4 res = float4(resRGB, inputColor.a);

	return res;	
}
#endif


#ifdef _GREYSCALE_ON
float3 Greyscale(float3 inputColorRGB)
{
	float3 res = inputColorRGB;
	float luminance = GetLuminance(res);
	luminance = saturate(luminance + ACCESS_PROP_FLOAT(_GreyscaleLuminosity));
	res = lerp(res, luminance * ACCESS_PROP_FLOAT4(_GreyscaleTintColor).rgb, ACCESS_PROP_FLOAT(_GreyscaleBlend));
	
	return res;
}
#endif

#ifdef _POSTERIZE_ON
float3 Posterize(float3 inputColorRGB)
{
	float3 res		= inputColorRGB;
	float gamma		= ACCESS_PROP_FLOAT(_PosterizeGamma);
	float numColors = ACCESS_PROP_FLOAT(_PosterizeNumColors);
	res.rgb = pow(res.rgb, gamma) * numColors;
	res.rgb = floor(res.rgb) / SafeDivisor(numColors);
	res.rgb = pow(res.rgb, 1.0 / gamma);
	
	return res;
}
#endif

#ifdef _CONTRAST_BRIGHTNESS_ON
float3 ContrastBrightness(float3 inputColorRGB)
{
	float3 res = ContrastBrightness(inputColorRGB, ACCESS_PROP_FLOAT(_Contrast), ACCESS_PROP_FLOAT(_Brightness));
	return res;
}
#endif

#ifdef _SHINE_ON
float4 Shine(float4 inputColor, CharacterData characterData, EffectsData effectsData)
{
	float4 res = inputColor;

	float2 uvShine = characterData.rectUV;
	 
	float cosAngle = cos(ACCESS_PROP_FLOAT(_ShineRotate));
	float sinAngle = sin(ACCESS_PROP_FLOAT(_ShineRotate));
	half2x2 rot = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
	uvShine -= float2(0.5, 0.5);
	uvShine = mul(rot, uvShine);
	uvShine += float2(0.5, 0.5);

	float shineMask = SAMPLE_TEX2D(_ShineMask, characterData.rectUV).a;
	float currentDistanceProjection = (uvShine.x + uvShine.y) / 2;

	float minValue = -0.1;
	float maxValue = ACCESS_PROP_FLOAT(_ShinePathLength);
	float pathRange = maxValue - minValue;
	float shineLocation = ACCESS_PROP_FLOAT(_ShineLocation) - characterData.shaderTime.y * ACCESS_PROP_FLOAT(_ShineSpeed);
	//frac based wrap so negative offsets (negative speeds) stay inside the path
	shineLocation = pathRange * frac((shineLocation - minValue) / pathRange);
	shineLocation += minValue;

	float whitePower = 1 - (abs(currentDistanceProjection - shineLocation) / ACCESS_PROP_FLOAT(_ShineWidth));

	float3 shineColorRGB = ACCESS_PROP_FLOAT4(_ShineColor);
	float shineAlpha = ACCESS_PROP_FLOAT4(_ShineColor).a;


	float3 shineColor = res.a * 
		whitePower * 
		ACCESS_PROP_FLOAT4(_ShineGlow) * 
		max(sign(currentDistanceProjection - (shineLocation - ACCESS_PROP_FLOAT(_ShineWidth))), 0.0) * 
		max(sign((shineLocation + ACCESS_PROP_FLOAT(_ShineWidth)) - currentDistanceProjection), 0.0) * 
		shineColorRGB * 
		shineAlpha *
		shineMask;

	res.rgb = lerp(res.rgb, res.rgb + shineColor, characterData.characterMask);

	return res;
}
#endif

#ifdef _OVERLAY_TEXTURE_ON
float4 OverlayTexture(float4 inputColor, CharacterData characterData, EffectsData effectsData)
{
	float4 res = inputColor;

	float2 overlayUvs = characterData.rectUV;
	overlayUvs.y *= characterData.rectAspectRatio;
	overlayUvs = SIMPLE_CUSTOM_TRANSFORM_TEX(overlayUvs, _OverlayTex);

	//Wrapped in whole tiles after the tiling, so a wrap never moves the pattern
	float speedReductionFactor = 0.05;
	float2 scrollSpeed = float2(ACCESS_PROP_FLOAT(_OverlayTextureScrollXSpeed), ACCESS_PROP_FLOAT(_OverlayTextureScrollYSpeed));
	overlayUvs += frac((_Time.y + characterData.timingSeed) * -scrollSpeed * speedReductionFactor * ACCESS_PROP_FLOAT4(_OverlayTex_ST).xy);

	float4 overlayCol = SAMPLE_TEX2D(_OverlayTex, overlayUvs);
	overlayCol.rgb *= _OverlayColor.rgb * _OverlayGlow;

	overlayCol.a *= _OverlayColor.a;
	//A multiply on the premultiplied face, so not weighted by the mask again: that would leave the plain face on antialiased edges
	res.rgb = lerp(res.rgb, res.rgb * overlayCol.rgb, _OverlayBlend);

	return res;
}
#endif


#if defined(_OUTLINE_ON) || defined(_OUTLINE_02_ON) || defined(_OUTLINE_03_ON) || defined(_OUTLINE_SECOND_LAYER_ON)
	float4 OutlineImpl(float4 inputColor, float4 outlineColor, float outlineMask)
	{
		float4 res = inputColor;
		
		res.rgb = lerp(res.rgb, outlineColor, outlineMask);
		res.a += outlineMask;

		return res;
	}

#ifdef _OUTLINE_ON
	float4 Outline(float4 inputColor, float outlineMask)
	{
		return OutlineImpl(inputColor, _Outline01Color, outlineMask);
	}
#endif

	#ifdef _OUTLINE_02_ON
	float4 Outline02(float4 inputColor, float outlineMask)
	{
		return OutlineImpl(inputColor, _Outline02Color, outlineMask);
	}
#endif

#ifdef _OUTLINE_03_ON
	float4 Outline03(float4 inputColor, float outlineMask)
	{
		return OutlineImpl(inputColor, _Outline03Color, outlineMask);
	}
#endif

#endif


#ifdef _INNER_GLOW_ON
//innerFreeMask is the face with the inside outlines removed, so the glow starts at their inner edge
float4 InnerGlow(float4 inputColor, CharacterData characterData, float innerFreeMask)
{
	float4 res = inputColor;

	//Size is the reach inward from the innermost outline edge, Smoothness the fraction of it that fades
	float innerEdgeBias = min(characterData.bias + CalculateMaxInsideOutlineWidth(), 0.5);
	float innerGlowBias = innerEdgeBias + _InnerGlowSize;
	float innerGlowRamp = max(_InnerGlowSize * _InnerGlowSmoothness, 0.001);

	float innerGlowMask = innerFreeMask - CalculateCharacterMask(characterData.sdf, innerGlowBias - innerGlowRamp, innerGlowRamp, characterData.pixelSDF);
	innerGlowMask = saturate(innerGlowMask);
	
	innerGlowMask *= _InnerGlowAtten;
	
	
	float innerGlowMaskPow4 = Pow_4(innerGlowMask);
	float3 innerGlowColorRGB = _InnerGlowColor.rgb * innerGlowMaskPow4;
	
	
#ifdef _INNERGLOWBLENDING_ADDITIVE
		res.rgb = lerp(res.rgb, res.rgb + innerGlowColorRGB, innerGlowMaskPow4);
#elif _INNERGLOWBLENDING_MULTIPLY
		res.rgb = lerp(res.rgb, res.rgb * innerGlowColorRGB, innerGlowMaskPow4);
#else
		res.rgb = lerp(res.rgb, innerGlowColorRGB, innerGlowMaskPow4);
#endif
	
	res.a += innerGlowMask;
	
	return res;
}
#endif

#ifdef _OUTER_GLOW_ON
//coveredMask is the face plus the outside outlines, so the glow starts at the outermost outline edge
float4 OuterGlow(float4 inputColor, CharacterData data, float coveredMask)
{
	float4 res = inputColor;

	//Size is the reach outward from the outermost outline edge, Smoothness the fraction of it that fades
	float outerEdgeBias = max(data.bias - CalculateMaxOutsideOutlineWidth(), -2.0);
	float outerGlowBias = max(outerEdgeBias - _OuterGlowSize, -2.0);
	float outerGlowRamp = max(_OuterGlowSize * _OuterGlowSmoothness, 0.001);

	float outerGlowRaw = CalculateCharacterMask(data.sdf, outerGlowBias, outerGlowRamp, data.pixelSDF);
	float outerGlowMask = saturate(outerGlowRaw - coveredMask) * _OuterGlowAttenuation;

	//The cover comes off once and linearly: through the 8th power it would starve the glow along an antialiased outline edge
	float outerGlowPow4 = Pow_4(saturate(outerGlowRaw) * _OuterGlowAttenuation);
	res.rgb += _OuterGlowColor.rgb * outerGlowPow4 * outerGlowPow4 * saturate(1.0 - coveredMask);
	res.a += outerGlowMask;
	
	
	return res;
}
#endif

#ifdef _LEDS_ON
float4 Led(float4 inputColor, CharacterData characterData, EffectsData effectsData)
{
	float4 res = inputColor;

	//Text grid: square cells laid over the text rect
	float2 gridPos = characterData.rectUV * ACCESS_PROP_FLOAT(_NumLeds) * float2(1.0, characterData.rectAspectRatio);
	float4 dotParams = float4(ACCESS_PROP_FLOAT(_LedSize), ACCESS_PROP_FLOAT(_LedsRoundness), ACCESS_PROP_FLOAT(_LedsSoftness), ACCESS_PROP_FLOAT(_LedsVariation));
	float insidePanel = 1.0;

#if defined(_LEDSGRID_PANEL)
	//Panel grid: the cells of an AllIn1TextLedPanel. Without one the text grid stays in use
	float4 panelGrid = ACCESS_PROP_FLOAT4(_LedPanelGrid);
	bool usePanel = panelGrid.w > 0.5;
	float2 panelGridPos = mul(_LedPanelMatrix, float4(effectsData.vertexOS, 1.0)).xy;
	gridPos = usePanel ? panelGridPos : gridPos;
	dotParams = usePanel ? ACCESS_PROP_FLOAT4(_LedPanelDot) : dotParams;
	insidePanel = usePanel ? LedInsideCells(floor(gridPos), panelGrid.xy) : 1.0;
#endif

	float2 cellId = floor(gridPos);

	//The glyph is read once at the cell center, so every dot is either whole or off instead of being cut by the glyph edge.
	//Grid offset to screen offset to atlas offset through the screen derivatives, which holds for any rotation or spline
	float2 gridDx = ddx(gridPos);
	float2 gridDy = ddy(gridPos);
	float2 atlasDx = ddx(characterData.unclampedUV);
	float2 atlasDy = ddy(characterData.unclampedUV);
	float det = gridDx.x * gridDy.y - gridDy.x * gridDx.y;
	det = abs(det) < 1e-10 ? 1e-10 : det;

	float2 toCenter = (cellId + 0.5) - gridPos;
	float2 screenOffset = float2(toCenter.x * gridDy.y - gridDy.x * toCenter.y, gridDx.x * toCenter.y - toCenter.x * gridDx.y) / det;
	float2 centerUV = characterData.unclampedUV + atlasDx * screenOffset.x + atlasDy * screenOffset.y;

	float sdfDrop;
	float sdfFarBound;
	float2 centerClampedUV = ClampUVToGlyphRect(centerUV, effectsData.uvCornersInAtlas, effectsData.texelSize, sdfDrop, sdfFarBound);
	//Explicit LOD: the implicit derivatives jump at every cell border and would pick a blurry mip there
	float centerRaw = SAMPLE_TEX2D_LOD(_MainTex, float4(centerClampedUV, 0.0, 0.0)).a;
	float centerSDF = min(centerRaw - sdfDrop, sdfFarBound);

	//Dots straddling the outline light partially, the way matrix displays antialias their fonts
	float edgeBand = max(ACCESS_PROP_FLOAT(_LedsEdgeDimming), 0.0001);

	[branch]
	if (NeedsSDFRebuild(centerRaw, sdfFarBound, 0.5 * edgeBand - characterData.bias))
	{
		centerSDF = RebuildSDFPastSpread(centerUV, effectsData.uvCornersInAtlas, effectsData.texelSize);
	}

	float lit = smoothstep(characterData.bias - 0.5 * edgeBand, characterData.bias + 0.5 * edgeBand, (centerSDF - 0.5) * 2.0);

	float dotDistance = LedDotDistance(gridPos, dotParams.y);
	float dotMask = LedDotMask(dotDistance, dotParams.x, dotParams.z) * insidePanel;
	float brightness = 1.0 - dotParams.w * LedCellHash(cellId);

	float colorT = SafeSmoothstep(ACCESS_PROP_FLOAT(_LedsColorTransition) * dotParams.x, dotParams.x, dotDistance);
	float4 ledsColor = lerp(ACCESS_PROP_FLOAT4(_LedsColor01), ACCESS_PROP_FLOAT4(_LedsColor02), colorT);
	//Tinted by the vertex and face color, so TMP color tags still color the dots
	float3 textTint = effectsData.faceColor.rgb / max(effectsData.faceColor.a, 0.0001);
	ledsColor.rgb *= textTint * ACCESS_PROP_FLOAT(_LedsIntensity) * brightness;

	//Only lit dots are drawn: unlit ones come from a panel, since the glyph quads alone would show them as boxes around each letter
	res.rgb = ledsColor.rgb;

	//The pass raises the final alpha to the 4th power, so the 4th root keeps the dot coverage and its antialiasing intact
	res.a = pow(saturate(dotMask * lit * ledsColor.a), 0.25);

	return res;
}
#endif

#ifdef _COLOR_GRADIENT_ON
float4 ColorGradient(float4 inputColor, CharacterData characterData, EffectsData effectsData)
{
	float4 res = inputColor;
	
#if defined(_COLOR_GRADIENT_NOISE_ON)
	float2 gradientUV = float2(0, 0);
	
	float2 noiseDisplacement = (characterData.timingSeed + _Time.y) * float2(_ColorGradientNoiseXSpeed, _ColorGradientNoiseYSpeed); 
	float2 noiseUV = characterData.uvNormalized + noiseDisplacement;
	noiseUV = (noiseUV * _ColorGradientNoiseTex_ST.xy) + _ColorGradientNoiseTex_ST.zw;
	gradientUV.x = SAMPLE_TEX2D(_ColorGradientNoiseTex, noiseUV).r;
	gradientUV.x = SafeSmoothstep(_ColorGradientNoiseCutoff, _ColorGradientNoiseCutoff + _ColorGradientNoiseSmoothness, gradientUV.x);
#else
	#ifdef _GRADIENTSCOPE_RECT
		float2 gradientUV = characterData.rectUV;
#else
		float2 gradientUV = characterData.uvNormalized;
	#endif
	
	float angleInRadians = ACCESS_PROP_FLOAT(_GradientAngle) * ALLIN1_DEG_TO_RAD;
	float2x2 rotateMatrix = float2x2(
		cos(angleInRadians), -sin(angleInRadians),
		sin(angleInRadians), cos(angleInRadians)
	);
	
	gradientUV -= 0.5;
	gradientUV = mul(rotateMatrix, gradientUV);
	gradientUV += 0.5;
	
	gradientUV.x += _GradientSpeed * _Time.y;
	// Only a scrolling gradient loops; a still one keeps its end color where a warp or mesh effect pushes letters past the rect
	gradientUV.x = _GradientSpeed != 0 ? fmod(gradientUV.x, 1.0) : gradientUV.x;
	
	
#endif
	
	float gradient = gradientUV.x;
	gradient = SafeSmoothstep(_GradientOrigin, _GradientOrigin + _GradientSize, gradient);
	
	float4 gradientColor = SAMPLE_TEX2D(_ColorGradientTex, float2(gradient, 0));
	
	//res is premultiplied by the face mask, so the gradient's own colour and alpha are too and the blend is not weighted by the mask again:
	//that would leave part of the ungraded face showing on antialiased edges
	float faceMask = characterData.characterMask;
#if defined(_GRADIENTBLENDINGMODE_ADDITIVE)
		float3 gradientFinalColorRGB = res.rgb + gradientColor.rgb * faceMask;
#elif defined(_GRADIENTBLENDINGMODE_MULTIPLY)
		float3 gradientFinalColorRGB = res.rgb * gradientColor.rgb;
#else
		float3 gradientFinalColorRGB = gradientColor.rgb * faceMask;
#endif

	float4 gradientFinalColor = float4(gradientFinalColorRGB, gradientColor.a * faceMask);

	res = lerp(res, gradientFinalColor, ACCESS_PROP_FLOAT(_GradientBlend));
	return res; 
}
#endif

#ifdef _COLOR_GRADING_ON
float4 ColorGrading(float4 inputColor, EffectsData data)
{
	float4 res = inputColor;

	float luminance = GetLuminanceRaw(res);
	res.rgb *= lerp(lerp(_ColorGradingDark, _ColorGradingMiddle, luminance / _ColorGradingMidPoint),
	lerp(_ColorGradingMiddle, _ColorGradingLight, (luminance - _ColorGradingMidPoint)/(1.0 - _ColorGradingMidPoint)), step(_ColorGradingMidPoint, luminance));

	return res;
}
#endif

#endif