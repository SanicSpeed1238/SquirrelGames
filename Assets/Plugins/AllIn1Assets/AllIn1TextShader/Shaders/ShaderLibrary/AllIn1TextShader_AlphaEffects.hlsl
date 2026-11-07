#ifndef ALLIN1TEXTSHADER_ALPHAEFFECTS
#define ALLIN1TEXTSHADER_ALPHAEFFECTS

#ifdef _FLICKER_ON
float4 Flicker(float4 inputColor, CharacterData characterData)
{
	float4 res = inputColor;

	res.a *= saturate(res.a * step(frac(0.05 + (_Time.w + characterData.timingSeed) * ACCESS_PROP_FLOAT(_FlickerFreq)), 1 - ACCESS_PROP_FLOAT(_FlickerPercent)) + ACCESS_PROP_FLOAT(_FlickerAlpha));

	return res;
}
#endif

#if defined(_FADE_ON)
float4 Fade(float4 inputColor, float2 uv, CharacterData characterData)
{
	float4 res = inputColor;

	float2 fadeUV = SIMPLE_CUSTOM_TRANSFORM_TEX(uv, _FadeTex);
	fadeUV.y *= characterData.rectAspectRatio;
	float fadeSample = SAMPLE_TEX2D(_FadeTex, fadeUV).r;
	
	fadeSample = pow(saturate(fadeSample), ACCESS_PROP_FLOAT(_FadePower));


	#ifdef _FADE_BURN_ON
		float fadeAmount = lerp(ACCESS_PROP_FLOAT(_FadeAmount) - ACCESS_PROP_FLOAT(_FadeTransition) - ACCESS_PROP_FLOAT(_FadeBurnWidth), 1.0, ACCESS_PROP_FLOAT(_FadeAmount));
		float fade = SafeSmoothstep(fadeAmount, fadeAmount + ACCESS_PROP_FLOAT(_FadeTransition), fadeSample);

		float fadePlusBurn = SafeSmoothstep(fadeAmount + ACCESS_PROP_FLOAT(_FadeBurnWidth), fadeAmount + ACCESS_PROP_FLOAT(_FadeBurnWidth) + ACCESS_PROP_FLOAT(_FadeTransition), fadeSample);
	
		res.rgb = lerp(ACCESS_PROP_FLOAT4(_FadeBurnColor).rgb, res.rgb,fadePlusBurn);
	
	#else
		float fadeAmount = lerp(ACCESS_PROP_FLOAT(_FadeAmount) - ACCESS_PROP_FLOAT(_FadeTransition), 1.0, ACCESS_PROP_FLOAT(_FadeAmount));
		float fade = SafeSmoothstep(fadeAmount, fadeAmount + ACCESS_PROP_FLOAT(_FadeTransition), fadeSample);
	#endif
	
	res.a *= fade;

	return res;
}
#endif

#endif //ALLIN1TEXTSHADER_ALPHAEFFECTS