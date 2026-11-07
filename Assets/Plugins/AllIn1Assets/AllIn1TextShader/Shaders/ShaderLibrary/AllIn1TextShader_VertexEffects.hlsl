#ifndef ALLIN1TEXTSHADER_VERTEXEFFECTS
#define ALLIN1TEXTSHADER_VERTEXEFFECTS

#ifdef _VERTEX_SHAKE_ON
float3 VertexShake(float3 vertexPos, float3 shaderTime)
{
	float3 res = vertexPos;
    
	float3 speedOffset = float3(1.0f, 1.13f, 1.07f) * ACCESS_PROP_FLOAT(_ShakeSpeedMult);
	//fmod by 2*PI keeps the trig input bounded: unbounded time values eventually lose precision or overflow on some platforms
	float3 shakeTime = fmod(shaderTime.y * ACCESS_PROP_FLOAT4(_ShakeSpeed).xyz * speedOffset, 6.28318530718);
	float3 displacement = sin(shakeTime) * ACCESS_PROP_FLOAT4(_ShakeMaxDisplacement).xyz;
	displacement *= ACCESS_PROP_FLOAT(_ShakeBlend);
    
	res += displacement;
	
	return res;
}
#endif

#if defined(_WIND_ON)
float3 Wind(float3 vertexPos, float3 shaderTime, float timingSeed, float3 characterCenter, float2 characterUVNormalized, float2 glyphRight, float2 quadSizeRS)
{
	float glyphHeight = quadSizeRS.y;
	float frequency = ACCESS_PROP_FLOAT(_WindWaveFrequency);
	float speed = ACCESS_PROP_FLOAT(_WindSpeed);
	float windTime = shaderTime.x - timingSeed;

	//Position measured in glyph heights so the wave looks the same at any font size
	float characterX = characterCenter.x / glyphHeight;
	float jitter = timingSeed * 0.01 * ALLIN13DSHADER_TWO_PI * ACCESS_PROP_FLOAT(_WindRandomness);

	//fmod by 2*PI keeps the trig input bounded: unbounded time values eventually lose precision or overflow on some platforms
	float phase = windTime * speed - characterX * frequency + jitter;
	float sway = (sin(fmod(phase, ALLIN13DSHADER_TWO_PI)) + 0.35 * sin(fmod(phase * 2.3 + 1.7, ALLIN13DSHADER_TWO_PI))) / 1.35;

	float gustPhase = windTime * speed * 0.21 - characterX * frequency * 0.37;
	float gust = lerp(1.0, 0.5 + 0.5 * sin(fmod(gustPhase, ALLIN13DSHADER_TWO_PI)), ACCESS_PROP_FLOAT(_WindGustStrength));

	float angle = (ACCESS_PROP_FLOAT(_WindLean) + sway * gust * ACCESS_PROP_FLOAT(_WindBendAmount)) * ALLIN1_DEG_TO_RAD;
	float s, c;
	sincos(angle, s, c);

	//Rotating the height around the anchor keeps the glyph's length, so bent tips drop instead of stretching
	float heightRS = (characterUVNormalized.y - ACCESS_PROP_FLOAT(_WindAnchor) * 0.5) * glyphHeight;
	float2 glyphUp = float2(-glyphRight.y, glyphRight.x);
	float2 offsetRS = glyphRight * (s * heightRS) + glyphUp * ((c - 1.0) * heightRS);

	return vertexPos + mul(matrix_rectToCanvas, float4(offsetRS, 0.0, 0.0)).xyz;
}
#endif

#endif //ALLIN1TEXTSHADER_VERTEXEFFECTS