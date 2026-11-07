#ifndef ALLIN1TEXTSHADER_UVEFFECTS
#define ALLIN1TEXTSHADER_UVEFFECTS

#ifdef _SCROLL_TEXTURE_ON
float2 ScrollTexture(float2 inputUV, float3 shaderTime)
{
	float2 res = inputUV;

	res.x += (shaderTime.x * ACCESS_PROP_FLOAT(_ScrollTextureX)) % 1;
	res.y += (shaderTime.x * ACCESS_PROP_FLOAT(_ScrollTextureY)) % 1;

	return res;
}
#endif

#ifdef _HAND_DRAWN_ON
CharacterData HandDrawn(CharacterData characterData)
{
	CharacterData res = characterData;

	float stepTime = floor((characterData.shaderTime + characterData.timingSeed) * _HandDrawnNumSteps) / _HandDrawnNumSteps;
	
	float2 centeredUV = characterData.uvNormalized - 0.5;
	
	//fmod by 2*PI keeps the trig input bounded: unbounded time values eventually lose precision or overflow on some platforms
	float handDrawnTime = fmod(stepTime * _HandDrawnSpeed, 6.28318530718);
	
	float displacementX = sin(handDrawnTime * centeredUV.x + stepTime) * 0.1 * _HandDrawnAmount;
	float displacementY = cos(handDrawnTime * centeredUV.y + stepTime) * 0.1 * _HandDrawnAmount;
	float2 displacement = float2(displacementX, displacementY);
	
	centeredUV += 0.5;

	centeredUV += displacement;
	
	res.uvNormalized.xy = centeredUV;
	
	return res;
}
#endif

#ifdef _PIXELATE_ON
//One grid of square blocks for the whole text; each pixel reads its block's center, carried into the letter's coordinates through the screen derivatives
CharacterData Pixelate(CharacterData characterData)
{
	CharacterData res = characterData;

	float2 gridPos = characterData.rectUV * ACCESS_PROP_FLOAT4(rectSize).xy / GetPixelateCellSize();
	float2 toCenter = (floor(gridPos) + 0.5) - gridPos;

	float2 gridDx = ddx(gridPos);
	float2 gridDy = ddy(gridPos);
	float2 uvDx = ddx(characterData.uvNormalized);
	float2 uvDy = ddy(characterData.uvNormalized);
	float det = gridDx.x * gridDy.y - gridDy.x * gridDx.y;
	det = abs(det) < 1e-10 ? 1e-10 : det;

	float2 screenOffset = float2(toCenter.x * gridDy.y - gridDy.x * toCenter.y, gridDx.x * toCenter.y - toCenter.x * gridDx.y) / det;
	res.uvNormalized = characterData.uvNormalized + uvDx * screenOffset.x + uvDy * screenOffset.y;

	return res;
}
#endif

#ifdef _GLITCH_ON
CharacterData GlitchUV(CharacterData characterData)
{
	CharacterData res = characterData;

	float2 uvGlitch = characterData.uvNormalized;
	uvGlitch.y -= 0.5;
	float randomSeed = characterData.timingSeed;
	float lineNoise = pow(
		rand2(floor(uvGlitch * float2(24., 19.) * ACCESS_PROP_FLOAT(_GlitchSize)), 
		randomSeed, 
		ACCESS_PROP_FLOAT(_GlitchSpeed)), 3.0) * ACCESS_PROP_FLOAT(_GlitchAmount)
	* pow(rand2(floor(uvGlitch * float2(38., 14.) * ACCESS_PROP_FLOAT(_GlitchSize)), randomSeed, ACCESS_PROP_FLOAT(_GlitchSpeed)), 3.0);

	res.uvNormalized = characterData.uvNormalized + float2(lineNoise * 0.02 * rand2(float2(2.0, 1), randomSeed, ACCESS_PROP_FLOAT(_GlitchSpeed)), 0);

	return res;
}
#endif

#ifdef _WAVE_ON
CharacterData Wave(CharacterData characterData)
{
	CharacterData res = characterData;
	
	float2 uvWave = float2(ACCESS_PROP_FLOAT(_WaveX), ACCESS_PROP_FLOAT(_WaveY)) - characterData.uvNormalized;
	//Ripple rings measured in quad widths so they are circular on screen whatever the glyph's shape
	float2 uvWaveCircular = uvWave * float2(1.0, characterData.characterAspectRatio);
    //fmod by 2*PI keeps the trig input bounded: unbounded time values eventually lose precision or overflow on some platforms
    float waveTime = fmod((_Time.y + characterData.timingSeed) * ACCESS_PROP_FLOAT(_WaveSpeed), 6.28318530718);
	
	float angWave = (length(uvWaveCircular) * ACCESS_PROP_FLOAT(_WaveAmount)) - waveTime;
	res.uvNormalized = res.uvNormalized + uvWave * sin(angWave) * (ACCESS_PROP_FLOAT(_WaveStrength) / 1000.0);

	return res;
}
#endif

#ifdef _ROUND_WAVE_ON
CharacterData RoundWave(CharacterData characterData)
{
	CharacterData res = characterData;

	float2 rectUV = res.rectUV;

	half xWave = (0.5 - rectUV.x);
	half yWave = (0.5 - rectUV.y) * res.rectAspectRatio;
	half ripple = -sqrt(xWave * xWave + yWave * yWave);
	//fmod by 2*PI keeps the trig input bounded: unbounded time values eventually lose precision or overflow on some platforms
	float roundWaveTime = fmod((_Time.y + characterData.timingSeed) * (ACCESS_PROP_FLOAT(_RoundWaveSpeed) / 10.0) / 0.015, 6.28318530718);
	rectUV += (sin((ripple / 0.015) + roundWaveTime) * (ACCESS_PROP_FLOAT(_RoundWaveStrength) / 10.0)) % 1;

	res.rectUV = rectUV;

	return res;
}
#endif

#ifdef _TWIST_ON
CharacterData Twist(CharacterData characterData)
{
	CharacterData res = characterData;
	
	float stretchAspect = _TwistStretchY / _TwistStretchX;
	float2 rectUV = res.rectUV;
	float aspectRatio = characterData.rectAspectRatio;
	
	half2 tempUv = rectUV - half2(ACCESS_PROP_FLOAT(_TwistUvPosX), ACCESS_PROP_FLOAT(_TwistUvPosY));	
	tempUv.y *= aspectRatio;
	
	tempUv.x /= SafeDivisor(_TwistStretchX);
	tempUv.y /= SafeDivisor(_TwistStretchY);

	half percent = saturate((ACCESS_PROP_FLOAT(_TwistUvRadius) - length(tempUv)) / SafeDivisor(ACCESS_PROP_FLOAT(_TwistUvRadius)));
	half theta = percent * percent * (2.0 * sin(ACCESS_PROP_FLOAT(_TwistUvAmount))) * 8.0;
	half s = sin(theta);
	half c = cos(theta);
	half beta = max(sign(ACCESS_PROP_FLOAT(_TwistUvRadius) - length(tempUv)), 0.0);

	tempUv = half2(dot(tempUv, half2(c, -s)), dot(tempUv, half2(s, c))) * beta + tempUv * (1 - beta);

	tempUv.y /= aspectRatio;
	tempUv.x *= _TwistStretchX;
	tempUv.y *= _TwistStretchY;


	tempUv += half2(ACCESS_PROP_FLOAT(_TwistUvPosX), ACCESS_PROP_FLOAT(_TwistUvPosY));

	res.rectUV = tempUv;

	return res;
}
#endif

#ifdef _WARP_ON
CharacterData Warp(CharacterData characterData)
{
	CharacterData res = characterData;
	
	half2 warpUv = characterData.uvNormalized;
    //fmod by tau keeps the trig input bounded: unbounded time values eventually lose precision or overflow on some platforms
    float warpTime = fmod((_Time.y + characterData.timingSeed) * ACCESS_PROP_FLOAT(_WarpSpeed), ALLIN1_TAU);
    float xWarp = warpTime + warpUv.x * ALLIN1_TAU / ACCESS_PROP_FLOAT(_WarpScale);
    float yWarp = warpTime + warpUv.y * ALLIN1_TAU / ACCESS_PROP_FLOAT(_WarpScale);
    float2 warp = float2(sin(xWarp), sin(yWarp)) * ACCESS_PROP_FLOAT(_WarpStrength);
    res.uvNormalized += warp;

	return res;
}
#endif

#if defined(_DISTORTION_ON) || defined(_DISTORTION_02_ON) || defined(OUTLINES_DISTORTION_ENABLED)

	#define DECLARE_DISTORTION_TIME_CONTINUOUS(variable_name, param_timingSeed) \
		float variable_name = _Time.y + param_timingSeed;

	#define DECLARE_DISTORTION_TIME_STEPPED(variable_name, param_timingSeed, param_frequency) \
		DECLARE_DISTORTION_TIME_CONTINUOUS(variable_name, param_timingSeed) \
		variable_name = floor(variable_name * param_frequency) / SafeDivisor(param_frequency);
		
		
	#define DISTORTION_IMPL(res, uvDist, tex, xSpeed, ySpeed, distortAmount, time) \
		uvDist = (uvDist * tex##_ST.xy) + tex##_ST.zw; \
		uvDist.x += (time * ACCESS_PROP_FLOAT(xSpeed)) % 1; \
		uvDist.y += (time * ACCESS_PROP_FLOAT(ySpeed)) % 1; \
		float4 __noiseSample = SAMPLE_TEX2D(tex, uvDist); \
		float __correctedNoise = (__noiseSample.r - 0.2) * 0.2; \
		float __correctedDistortAmount = __correctedNoise * ACCESS_PROP_FLOAT(distortAmount);  \
		res.x += __correctedDistortAmount; \
		res.y += __correctedDistortAmount;
		
#endif

#if defined(_DISTORTION_ON)
float2 Distortion(float2 uvDist, float timingSeed)
{
	float2 res = uvDist;
	
	#if defined(_DISTORTIONMOVEMENTTYPE_STEPPED)
		DECLARE_DISTORTION_TIME_STEPPED(distortionTime, timingSeed, _DistortionFrequency)
	#else
		DECLARE_DISTORTION_TIME_CONTINUOUS(distortionTime, timingSeed)
	#endif
	
	DISTORTION_IMPL(res, uvDist, _DistortTex, _DistortXSpeed, _DistortYSpeed, _DistortAmount, distortionTime)
	
	return res;
}
#endif

#endif //ALLIN1TEXTSHADER_UVEFFECTS