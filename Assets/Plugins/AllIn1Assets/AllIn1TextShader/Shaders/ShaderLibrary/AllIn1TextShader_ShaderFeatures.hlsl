#ifndef ALLIN1TEXTSHADER_SHADERFEATURES
#define ALLIN1TEXTSHADER_SHADERFEATURES

#pragma shader_feature_local _BLENDINGMODE_MULTIPLY
#pragma shader_feature_local _BLENDINGMODE_ADDITIVE

#pragma shader_feature_local _COLOR_RAMP_ON
#pragma shader_feature_local _COLORRAMPSTAGE_BEFOREEFFECTS
#pragma shader_feature_local _COLORRAMPSTAGE_AFTEREFFECTS
#pragma shader_feature_local _POSTERIZE_ON
#pragma shader_feature_local _HUE_SHIFT_ON
#pragma shader_feature_local _CONTRAST_BRIGHTNESS_ON
#pragma shader_feature_local _GREYSCALE_ON
#pragma shader_feature_local _PIXELATE_ON
#pragma shader_feature_local _HAND_DRAWN_ON
#pragma shader_feature_local _GLITCH_ON
#pragma shader_feature_local _FLICKER_ON
#pragma shader_feature_local _SHINE_ON
#pragma shader_feature_local _WIND_ON

#pragma shader_feature_local _FADE_ON
#pragma shader_feature_local _FADE_BURN_ON

#pragma shader_feature_local _ALPHA_CUTOFF_ON

#pragma shader_feature_local _HOLOGRAM_ON
#pragma shader_feature_local _OVERLAY_TEXTURE_ON
#pragma shader_feature_local _WAVE_ON
#pragma shader_feature_local _ROUND_WAVE_ON
#pragma shader_feature_local _TWIST_ON
#pragma shader_feature_local _WARP_ON

#pragma shader_feature_local _DISTORTION_ON
#pragma shader_feature_local _DISTORTIONMOVEMENTTYPE_STEPPED

#pragma shader_feature_local _DISTORTION_02_ON
#pragma shader_feature_local _DISTORTION02MOVEMENTTYPE_STEPPED

#pragma shader_feature_local _DISTORTION_03_ON
#pragma shader_feature_local _DISTORTION03MOVEMENTTYPE_STEPPED

#pragma shader_feature_local _VERTEX_SHAKE_ON 

/* OUTLINES */
#pragma shader_feature_local _OUTLINE_ON
#pragma shader_feature_local _OUTLINE01POSITION_INSIDE
#pragma shader_feature_local _OUTLINE01POSITION_OUTSIDE
#pragma shader_feature_local _OUTLINE01SMOOTHNESSMODE_CLASSIC
#pragma shader_feature_local _OUTLINE01SMOOTHNESSMODE_SMOOTH


#pragma shader_feature_local _OUTLINE_02_ON
#pragma shader_feature_local _OUTLINE02POSITION_INSIDE 
#pragma shader_feature_local _OUTLINE02POSITION_OUTSIDE
#pragma shader_feature_local _OUTLINE02SMOOTHNESSMODE_CLASSIC
#pragma shader_feature_local _OUTLINE02SMOOTHNESSMODE_SMOOTH

#pragma shader_feature_local _OUTLINE_03_ON
#pragma shader_feature_local _OUTLINE03POSITION_INSIDE
#pragma shader_feature_local _OUTLINE03POSITION_OUTSIDE
#pragma shader_feature_local _OUTLINE03SMOOTHNESSMODE_CLASSIC
#pragma shader_feature_local _OUTLINE03SMOOTHNESSMODE_SMOOTH
/**/

#pragma shader_feature_local _INNER_GLOW_ON
#pragma shader_feature_local _INNERGLOWBLENDING_ADDITIVE
#pragma shader_feature_local _INNERGLOWBLENDING_MULTIPLY

#pragma shader_feature_local _OUTER_GLOW_ON
#pragma shader_feature_local _LEDS_ON
#pragma shader_feature_local _LEDSGRID_PANEL

#pragma shader_feature_local _COLOR_GRADIENT_ON
#pragma shader_feature_local _COLOR_GRADIENT_NOISE_ON

#pragma shader_feature_local _GRADIENTSCOPE_RECT

#pragma shader_feature_local _COLOR_GRADING_ON
#pragma shader_feature_local _GRADIENTBLENDINGMODE_ADDITIVE
#pragma shader_feature_local _GRADIENTBLENDINGMODE_MULTIPLY
#pragma shader_feature_local _GRADIENTBLENDINGMODE_REPLACE

#pragma shader_feature_local _BEVEL_ON
#pragma shader_feature_local _BEVELSTYLE_SYMMETRIC
#pragma shader_feature_local _BEVELLIGHTSOURCE_GLOBAL

#pragma shader_feature_local _LIGHTBLENDING_MULTIPLY
#pragma shader_feature_local _LIGHTBLENDING_ADDITIVE
#pragma shader_feature_local _LIGHTBLENDING_REPLACE

#pragma shader_feature_local _SHADOWBLENDING_MULTIPLY
#pragma shader_feature_local _SHADOWBLENDING_ADDITIVE
#pragma shader_feature_local _SHADOWBLENDING_REPLACE

#pragma shader_feature_local _OUTLINE_CREASE_ON

#pragma shader_feature_local _NORMAL_MAP_ON
#pragma shader_feature_local _SPECULAR_ON
#pragma shader_feature_local _REFLECTIONS_ON
#pragma shader_feature_local _BEVEL_AFFECT_OUTLINES_ON
#pragma shader_feature_local _DROP_SHADOW_ON
#pragma shader_feature_local _DROP_SHADOW_02_ON
#pragma shader_feature_local _EXTRUDE_ON
#pragma shader_feature_local _EXTRUDELIGHTSOURCE_GLOBAL




//---------- Additional Outline macros -----------------------
#if defined(_OUTLINE_ON)
	#define OUTLINE_01_ENABLED
#endif

#if defined(_OUTLINE_ON) && defined(_OUTLINE_02_ON)
	#define OUTLINE_02_ENABLED
#endif

#if defined(_OUTLINE_ON) && defined(_OUTLINE_02_ON) && defined(_OUTLINE_03_ON)
	#define OUTLINE_03_ENABLED
#endif

#if defined(OUTLINE_01_ENABLED) || defined(OUTLINE_02_ENABLED) || defined(OUTLINE_03_ENABLED) || defined(_OUTLINE_SECOND_LAYER_ON)
	#define OUTLINES_ENABLED
#endif

#if defined(OUTLINES_ENABLED) && (defined(_OUTLINE01_DISTORTION) || defined(_OUTLINE02_DISTORTION) || defined(_OUTLINE03_DISTORTION) || defined(_OUTLINE01_SECOND_LAYER_DISTORTION))
	#define OUTLINES_DISTORTION_ENABLED
#endif

#if defined(OUTLINES_DISTORTION_ENABLED) /*Here we should or-concatenate other possible outlines extra effects: || defined() || defined()*/
	#define OUTLINES_EXTRA_EFFECTS_ENABLED;
#endif

/*Here we should or-concatenate other possible outlines extra UV effects: || defined() || defined()*/
#if defined(OUTLINES_ENABLED) && defined(OUTLINES_DISTORTION_ENABLED)
	#define OUTLINES_EXTRA_UV_EFFECTS_ENABLED
#endif
//-------------------------------------------------------------

#if defined(_DROP_SHADOW_ON)
	#define DROP_SHADOW_01_ENABLED
#endif

#if defined(_DROP_SHADOW_ON) && defined(_DROP_SHADOW_02_ON) 
	#define DROP_SHADOW_02_ENABLED
#endif

#endif //ALLIN1TEXTSHADER_SHADERFEATURES