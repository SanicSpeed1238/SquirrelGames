#ifndef ALLIN1TEXTSHADER_GLOBALPROPERTIESANDTEXTURESDECLARATION
#define ALLIN1TEXTSHADER_GLOBALPROPERTIESANDTEXTURESDECLARATION

DECLARE_TEX2D(_MainTex)
DECLARE_TEX2D(_BlurredAtlas)

#if defined(_COLOR_RAMP_ON)
	DECLARE_TEX2D(_ColorRampTex)
#endif


float4 allIn13DShader_globalTime;

//Set per draw by uGUI for a RectMask2D, so outside the material buffer as in uGUI's and TMP's own shaders
float4 _ClipRect;
float _UIMaskSoftnessX;
float _UIMaskSoftnessY;

//The controller's light direction is shared by Bevel and Extrude
#if (defined(_BEVEL_ON) && defined(_BEVELLIGHTSOURCE_GLOBAL)) || (defined(_EXTRUDE_ON) && defined(_EXTRUDELIGHTSOURCE_GLOBAL))
	float4 global_BevelLight;		//x angle and y elevation in radians, z is 1 while a BevelLightController is active
#endif

#if defined(_EXTRUDE_ON) && defined(_EXTRUDELIGHTSOURCE_GLOBAL)
	float4 global_ExtrudeLightColor;
	float4 global_ExtrudeShadowColor;
	float4 global_ExtrudeLightRamp;	//x light smoothness
#endif

#if defined(_BEVEL_ON) && defined(_BEVELLIGHTSOURCE_GLOBAL)
	float4 global_BevelLightColor;	//rgb already multiplied by the intensity
	float4 global_BevelLightRamps;	//light cutoff, light smoothness, shadow cutoff, shadow smoothness
	float4 global_BevelShadowColor;
	float4 global_BevelLightBlend;	//x light blending, y shadow blending: 0 multiply, 1 additive, 2 replace
#endif

//------------------------

#if defined(_FADE_ON)
	DECLARE_TEX2D(_FadeTex)
#endif

#if defined(_UV_DISTORTION_ON)
	DECLARE_TEX2D(_DistortTex)
#endif

#if defined(_SHINE_ON)
	DECLARE_TEX2D(_ShineMask);
#endif

#if defined(_OVERLAY_TEXTURE_ON)
	DECLARE_TEX2D(_OverlayTex);
#endif

#if defined(_DISTORTION_ON)
	DECLARE_TEX2D(_DistortTex);
#endif

#if defined(_DISTORTION_02_ON)
	DECLARE_TEX2D(_Distort02Tex);
#endif

#if defined(_DISTORTION_03_ON)
	DECLARE_TEX2D(_Distort03Tex);
#endif

#if defined(_COLOR_GRADIENT_ON)
	DECLARE_TEX2D(_ColorGradientTex);
	#if defined(_COLOR_GRADIENT_NOISE_ON)
		DECLARE_TEX2D(_ColorGradientNoiseTex)
	#endif
#endif


#if defined(_NORMAL_MAP_ON)
	DECLARE_TEX2D(_NormalMap);
#endif

#if defined(_REFLECTIONS_ON)
	DECLARE_TEXCUBE(_ReflectionsMap);
#endif

#if defined(_ALPHA_DISTORTION_ON)
	DECLARE_TEX2D(_AlphaDistortionTex);
#endif

#endif //ALLIN1TEXTSHADER_GLOBALPROPERTIESANDTEXTURESDECLARATION