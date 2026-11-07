Shader "AllIn1Text/AllIn1Text"
{
    Properties
    {
		_RenderPreset("Render Preset", Float) = 1
		[AdvancedProperty]_AdvancedConfigurationEnabled("Show Advanced Configuration", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendSrc ("Blend mode Source", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendDst ("Blend mode Destination", Float) = 10
		[AdvancedProperty][Enum(UnityEngine.Rendering.CullMode)]_CullingMode("Culling Mode", Float) = 0
		[AdvancedProperty][Enum(Off, 0, On, 1)]_ZWrite("Depth Write", float) = 1.0
        [AdvancedProperty][Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("Z Test Mode", float) = 4
		[AdvancedProperty][Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask("Color Write Mask", float) = 15
		
		[AdvancedProperty]_StencilComp		("Stencil Comparison", Float) = 8 
		[AdvancedProperty]_Stencil			("Stencil ID", Float) = 0
		[AdvancedProperty]_StencilOp		("Stencil Operation", Float) = 0
		[AdvancedProperty]_StencilWriteMask	("Stencil Write Mask", Float) = 255
		[AdvancedProperty]_StencilReadMask	("Stencil Read Mask", Float) = 255
		[AdvancedProperty][KeywordEnum(None, Multiply, Additive)]_BlendingMode("Blending Mode", Float) = 0

		[FontProperty]_FontPropertiesEnabled("Show Font Properties", Float) = 0
		[NoScaleOffset][FontProperty]_MainTex ("Font Atlas", 2D) = "white" {}
		[NoScaleOffset][FontProperty]_BlurredAtlas ("Blurred Atlas", 2D) = "white" {}

		[SingleProperty][HDR]_FaceColor			("Face Color", Color) = (1,1,1,1)
		[SingleProperty]_CustomFaceDilate				("Face Dilate", Range(-1,1)) = 0
		[SingleProperty]_CustomOutlineSoftness		("Softness", Range(0,1)) = 0

		[SingleProperty]_CharacterExtraMargin("Character extra margin", Range(-100.0, 200.0)) = 0.0
		[SingleProperty]_InnerAlpha("Inner Alpha", Range(0, 1)) = 1.0
		[SingleProperty]_ColorIntensity("Color Intensity", Range(0, 5)) = 1.0

		//Vertex Shake
		[Effect(EffectID# VERTEX_SHAKE, GroupID# MeshEffects)][Toggle(_VERTEX_SHAKE_ON)] _VertexShakeOn("Vertex Shake", Float) = 0
		[EffectProperty(VERTEX_SHAKE)][Vector3] _ShakeSpeed("Speed", Vector) = (41, 49, 45, 0)
    	[EffectProperty(VERTEX_SHAKE)] _ShakeSpeedMult("Shake Mult", Float) = 1.0
		[EffectProperty(VERTEX_SHAKE)][Vector3] _ShakeMaxDisplacement("Shake Max Displacement", Vector) = (0.1, 0.1, 0.1, 0)
		[EffectProperty(VERTEX_SHAKE)] _ShakeBlend("Shake Blend", Range(0, 1)) = 1.0

		//Wind
		[Effect(EffectID# WIND, GroupID# MeshEffects)][Toggle(_WIND_ON)]_Wind("Wind", Float) = 0
		[EffectProperty(WIND)]_WindBendAmount("Bend Amount", Range(0, 60)) = 15.0
		[EffectProperty(WIND)]_WindLean("Lean", Range(-45, 45)) = 0.0
		[EffectProperty(WIND)]_WindSpeed("Speed", Range(0, 10)) = 2.0
		[EffectProperty(WIND)]_WindWaveFrequency("Wave Frequency", Range(0, 2)) = 0.4
		[EffectProperty(WIND)]_WindRandomness("Randomness", Range(0, 1)) = 0.15
		[EffectProperty(WIND)]_WindGustStrength("Gust Strength", Range(0, 1)) = 0.5
		[EffectProperty(WIND)][Enum(Bottom, 0, Center, 1, Top, 2)]_WindAnchor("Anchor", Float) = 0

		//Bevel
		[Effect(EffectID# BEVEL, GroupID# Lighting, CustomDrawer# BEVEL_EFFECT_DRAWER)][Toggle(_BEVEL_ON)]_BevelEnabled("Bevel", Float) = 0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_BEVEL_ON), AllowReset# False)][KeywordEnum(Classic, Symmetric)]_BevelStyle("Bevel Style", Float) = 0.0
		[EffectProperty(BEVEL)]_BevelSize("Bevel Size", Range(1.0, 32.0)) = 4.0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_BEVEL_ON), AllowReset# False)][KeywordEnum(Material, Global)]_BevelLightSource("Light Source", Float) = 0.0
		[EffectProperty(BEVEL)]_LightAngle("Light Angle", Range(0.0, 360.0)) = 300.0
		[EffectProperty(BEVEL)]_LightElevation("Light Elevation", Range(0, 90)) = 0.0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_BEVEL_ON), AllowReset# False)][KeywordEnum(Multiply, Additive, Relplace)]_LightBlending("Light Blending", Float) = 1.0
		[EffectProperty(BEVEL)]_LightColor("Light Color", Color) = (0.576, 0.576, 0.576, 1.0)
		[EffectProperty(BEVEL)]_LightCutoff("Light Cutoff", Range(0, 1)) = 0.5
		[EffectProperty(BEVEL)]_LightSmoothness("Light Smoothness", Range(0, 1)) = 0.75
		[EffectProperty(BEVEL)]_LightIntensity("Light Intensity", Range(0.0, 4.0)) = 1.5
		[EffectProperty(ParentEffect# BEVEL, Keywords(_BEVEL_ON), AllowReset# False)][KeywordEnum(Multiply, Additive, Relplace)]_ShadowBlending("Shadow Blending", Float) = 0.0
		[EffectProperty(BEVEL)]_ShadowColor("Shadow Color", Color) = (0.25, 0.25, 0.25, 1.0)
		[EffectProperty(BEVEL)]_ShadowCutoff("Shadow Cutoff", Range(0, 1)) = 0.0
		[EffectProperty(BEVEL)]_ShadowSmoothness("Shadow Smoothness", Range(0, 1)) = 0.6
		//Bevel Normal Map
		[EffectProperty(BEVEL)][Toggle(_NORMAL_MAP_ON)]_NormalMapOn("Normal Map", Float) = 0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_NORMAL_MAP_ON), AllowReset# True)]_NormalMap("Normal Map", 2D) = "bump" {}
		[EffectProperty(ParentEffect# BEVEL, Keywords(_NORMAL_MAP_ON), AllowReset# True)]_NormalStrength("Normal Strength", Range(0.0, 10.0)) = 1.0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_NORMAL_MAP_ON), AllowReset# True)]_NormalFaceAtten("Face Atten", Range(0.0, 1.0)) = 1.0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_NORMAL_MAP_ON), AllowReset# True)]_NormalOutlineAtten("Outline Atten", Range(0.0, 1.0)) = 1.0
		//Bevel Specular
		[EffectProperty(BEVEL)][Toggle(_SPECULAR_ON)]_SpecularOn("Specular", Float) = 0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_SPECULAR_ON), AllowReset# True)]_SpecularColor("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
		[EffectProperty(ParentEffect# BEVEL, Keywords(_SPECULAR_ON), AllowReset# True)]_Shininess("Shininess", Range(0.01, 2.0)) = 1.0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_SPECULAR_ON), AllowReset# True)]_SpecularCutoff("Specular Cutoff", Range(0.0, 1.0)) = 0.0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_SPECULAR_ON), AllowReset# True)]_SpecularSmoothness("Specular Smoothness", Range(0.0, 1.0)) = 1.0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_SPECULAR_ON), AllowReset# True)]_SpecularIntensity("Intensity", Range(0.0, 4.0)) = 1.0
		//Bevel Reflections
		[EffectProperty(BEVEL)][Toggle(_REFLECTIONS_ON)]_ReflectionsOn("Reflections", Float) = 0.0
		[EffectProperty(ParentEffect# BEVEL, Keywords(_REFLECTIONS_ON), AllowReset# True)]_ReflectionsMap("Reflections Map", Cube) = "black" {}
		[EffectProperty(ParentEffect# BEVEL, Keywords(_REFLECTIONS_ON), AllowReset# True)]_ReflectionsAtten("Attenuation", Range(0, 1)) = 0.5
		//Bevel Outlines
		[EffectProperty(BEVEL)][Toggle(_BEVEL_AFFECT_OUTLINES_ON)]_BevelAffectOutlines("Affect Outlines", Float) = 0

		//******** Drop Shadows ********
		[Effect(EffectID# DROP_SHADOW, GroupID# Lighting, CustomDrawer# DROP_SHADOW_EFFECT_DRAWER)][Toggle(_DROP_SHADOW_ON)]_DropShadowOn("Drop Shadow", Float) = 0
		[EffectProperty(DROP_SHADOW)]_DropShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
		[EffectProperty(DROP_SHADOW)]_DropShadowsSize("Shadows Size", Range(0, 1)) = 0.5
		[EffectProperty(DROP_SHADOW)]_DropShadowsOffsetX("Offset X", Range(-40.0, 40.0)) = 1.5
		[EffectProperty(DROP_SHADOW)]_DropShadowsOffsetY("Offset Y", Range(-40.0, 40.0)) = 0
		[EffectProperty(DROP_SHADOW)]_DropShadowsSmoothness("Smoothness", Range(0, 1)) = 0
		//Drop Shadows 02
		[EffectProperty(DROP_SHADOW)][Toggle(_DROP_SHADOW_02_ON)]_DropShadow02On("Drop Shadow (02)", Float) = 0
		[EffectProperty(ParentEffect# DROP_SHADOW, Keywords(_DROP_SHADOW_02_ON), AllowReset# True)]_DropShadow02_Color("Shadow Color", Color) = (0, 0, 0, 1)
		[EffectProperty(ParentEffect# DROP_SHADOW, Keywords(_DROP_SHADOW_02_ON), AllowReset# True)]_DropShadow02_Size("Shadows Size", Range(0, 1)) = 0.5
		[EffectProperty(ParentEffect# DROP_SHADOW, Keywords(_DROP_SHADOW_02_ON), AllowReset# True)]_DropShadow02_OffsetX("Offset X", Range(-40.0, 40.0)) = 3.0
		[EffectProperty(ParentEffect# DROP_SHADOW, Keywords(_DROP_SHADOW_02_ON), AllowReset# True)]_DropShadow02_OffsetY("Offset Y", Range(-40.0, 40.0)) = 0
		[EffectProperty(ParentEffect# DROP_SHADOW, Keywords(_DROP_SHADOW_02_ON), AllowReset# True)]_DropShadow02_Smoothness("Smoothness", Range(0, 1)) = 0
		//*************************

		//Extrude
		[Effect(EffectID# EXTRUDE, GroupID# Lighting, CustomDrawer# EXTRUDE_EFFECT_DRAWER)][Toggle(_EXTRUDE_ON)]_ExtrudeOn("Extrude", Float) = 0
		[EffectProperty(EXTRUDE)]_ExtrudeDepth("Depth", Range(0.0, 64.0)) = 10.0
		[EffectProperty(EXTRUDE)]_ExtrudeAngle("Angle", Range(0.0, 360.0)) = 270.0
		[EffectProperty(EXTRUDE)]_ExtrudeColor("Side Color", Color) = (0.6, 0.6, 0.65, 1.0)
		[EffectProperty(EXTRUDE)]_ExtrudeLetterTint("Letter Color Tint", Range(0, 1)) = 1.0
		[EffectProperty(EXTRUDE)]_ExtrudeDepthShade("Depth Shade", Range(0, 1)) = 0.3
		[EffectProperty(ParentEffect# EXTRUDE, Keywords(_EXTRUDE_ON), AllowReset# False)][KeywordEnum(Material, Global)]_ExtrudeLightSource("Light Source", Float) = 0.0
		[EffectProperty(EXTRUDE)]_ExtrudeLightAngle("Light Angle", Range(0.0, 360.0)) = 300.0
		[EffectProperty(EXTRUDE)]_ExtrudeLightElevation("Light Elevation", Range(0, 90)) = 0.0
		[EffectProperty(EXTRUDE)]_ExtrudeLightColor("Light Color", Color) = (1.0, 1.0, 1.0, 1.0)
		[EffectProperty(EXTRUDE)]_ExtrudeShadowColor("Shadow Color", Color) = (0.4, 0.4, 0.5, 1.0)
		[EffectProperty(EXTRUDE)]_ExtrudeLightSmoothness("Light Smoothness", Range(0, 1)) = 1.0
		[EffectProperty(EXTRUDE)]_ExtrudeOutlineWidth("Outline Thickness", Range(0, 1)) = 0.0
		[EffectProperty(EXTRUDE)]_ExtrudeOutlineColor("Outline Color", Color) = (0, 0, 0, 1)

		//Color Ramp
		[Effect(EffectID# COLOR_RAMP, GroupID# ColorEffects, CustomDrawer# COLOR_RAMP_EFFECT_DRAWER)][Toggle(_COLOR_RAMP_ON)]_ColorRampOn("Color Ramp", Float) = 0
		[EffectProperty(ParentEffect# COLOR_RAMP, Keywords(_COLOR_RAMP_ON), AllowReset# False)][KeywordEnum(BeforeEffects, AfterEffects)]_ColorRampStage("Stage", Float) = 0.0
		[EffectProperty(COLOR_RAMP)][AllIn1TextShaderGradientDrawer]_ColorRampTex("Color Ramp Tex", 2D) = "white" {}
		[EffectProperty(COLOR_RAMP)]_ColorRampLuminosity("Color Ramp Luminosity", Range(0, 1)) = 0
		[EffectProperty(COLOR_RAMP)]_ColorRampBlend("Color Ramp Blend", Range(0, 1)) = 1
		[EffectProperty(COLOR_RAMP)]_ColorRampTiling("Tiling", Range(0.01, 10)) = 1.0 
		[EffectProperty(COLOR_RAMP)]_ColorRampScrollSpeed("Scroll Speed", Float) = 0.0

		//Hue Shift
		[Effect(EffectID# HUE_SHIFT, GroupID# ColorEffects)][Toggle(_HUE_SHIFT_ON)] _HueShiftEnabled("Hue Shift", Float) = 0
		[EffectProperty(HUE_SHIFT)]_HueShift("Hue Shift", Range(0, 360)) = 0
		[EffectProperty(HUE_SHIFT)]_HueSaturation("Saturation", Range(0, 4)) = 1
		[EffectProperty(HUE_SHIFT)]_HueBrightness("Brightness", Range(0, 2)) = 1

		//Greyscale
		[Effect(EffectID# GREYSCALE, GroupID# ColorEffects)][Toggle(_GREYSCALE_ON)]_Greyscale("Greyscale", Float) = 0
		[EffectProperty(GREYSCALE)] _GreyscaleLuminosity("Luminosity", Range(-1, 1)) = 0 
		[EffectProperty(GREYSCALE)] _GreyscaleTintColor("Greyscale Tint", Color) = (1.0, 1.0, 1.0, 1.0)
		[EffectProperty(GREYSCALE)] _GreyscaleBlend("Blend", Range(0, 1)) = 1

		//Posterize
    	[Effect(EffectID# POSTERIZE, GroupID# ColorEffects)][Toggle(_POSTERIZE_ON)]_Posterize("Posterize", Float) = 0
    	[EffectProperty(POSTERIZE)] _PosterizeNumColors("Number of Colors",  Range(0,200)) = 8
		[EffectProperty(POSTERIZE)] _PosterizeGamma("Posterize Gamma",  Range(0.1,10)) = 0.75

		//Contrast and Brightness 
		[Effect(EffectID# CONTRAST_BRIGHTNESS, GroupID# ColorEffects)][Toggle(_CONTRAST_BRIGHTNESS_ON)]_ContrastBrightnessOn("Contrast and Brightness", Float) = 0
		[EffectProperty(CONTRAST_BRIGHTNESS)]_Contrast("Contrast", Range(0, 10)) = 1.0
		[EffectProperty(CONTRAST_BRIGHTNESS)]_Brightness("Brightness", Range(-1, 1)) = 0.0

		//Shine
		[Effect(EffectID# SHINE, GroupID# ColorEffects)][Toggle(_SHINE_ON)]_Shine("Shine", Float) = 0
		[EffectProperty(SHINE)]_ShineColor("Shine Color", Color) = (1,1,1,1)
		[EffectProperty(SHINE)]_ShineSpeed("Shine Speed", Float) = 0.5
		[EffectProperty(SHINE)]_ShinePathLength("Path Length", Range(1.0, 10.0)) = 1.0
        [EffectProperty(SHINE)]_ShineLocation("Shine Location", Range(0,1)) = 0.5
        [EffectProperty(SHINE)]_ShineRotate("Rotate Angle(radians)", Range(0, 6.2831)) = 3.75
        [EffectProperty(SHINE)]_ShineWidth("Shine Width", Range(0.01, 1.0)) = 0.075
        [EffectProperty(SHINE)]_ShineGlow("Shine Glow", Range(0,100)) = 1
		[EffectProperty(SHINE)][NoScaleOffset] _ShineMask("Shine Mask", 2D) = "white" {}

		//Hologram
		[Effect(EffectID# HOLOGRAM, GroupID# ColorEffects)][Toggle(_HOLOGRAM_ON)] _Hologram("Hologram", Float) = 0
        [EffectProperty(HOLOGRAM)][HDR]_HologramColor("Hologram Color", Color) = (0.35,0.85,1.5,1)
        [EffectProperty(HOLOGRAM)]_HologramTint("Hologram Tint", Range(0, 1)) = 0.75
		[EffectProperty(HOLOGRAM)]_HologramRotation("Hologram Rotation", Range(0, 360)) = 0
        [EffectProperty(HOLOGRAM)]_HologramBaseAlpha("Hologram Base Alpha", Range(0, 1)) = 0.75

        [EffectProperty(HOLOGRAM)]_HologramFrequency("Hologram Lines Per Rect Height", Float) = 30
        [EffectProperty(HOLOGRAM)]_HologramScrollSpeed("Hologram Scroll Speed (lines/s)", Float) = 1.5
        [EffectProperty(HOLOGRAM)]_HologramLineWidth("Hologram Line Width", Range(0.01, 1)) = 0.45
        [EffectProperty(HOLOGRAM)]_HologramLineSmoothness("Hologram Line Smoothness", Range(0.01, 1)) = 1
        [EffectProperty(HOLOGRAM)]_HologramLineGlow("Hologram Line Glow", Range(0, 4)) = 0.35

        [EffectProperty(HOLOGRAM)]_HologramAccentFrequency("Hologram Accent Frequency", Float) = 2
        [EffectProperty(HOLOGRAM)]_HologramAccentSpeed("Hologram Accent Speed", Float) = 1
        [EffectProperty(HOLOGRAM)]_HologramAccentWidth("Hologram Accent Width", Range(0.01, 1)) = 0.3
        [EffectProperty(HOLOGRAM)]_HologramAccentGlow("Hologram Accent Glow", Range(0, 4)) = 0.6

		//Overlay Texture
		[Effect(EffectID# OVERLAY_TEXTURE, GroupID# ColorEffects)][Toggle(_OVERLAY_TEXTURE_ON)]_OverlayTextureOn("Overlay Texture", Float) = 0
		[EffectProperty(OVERLAY_TEXTURE)]_OverlayTex("Overlay Texture", 2D) = "white" {}
		[EffectProperty(OVERLAY_TEXTURE)]_OverlayTextureScrollXSpeed("Speed X Axis", Range(-5, 5)) = 0.25
		[EffectProperty(OVERLAY_TEXTURE)]_OverlayTextureScrollYSpeed("Speed Y Axis", Range(-5, 5)) = 0.25
		[EffectProperty(OVERLAY_TEXTURE)]_OverlayColor("Overlay Color", Color) = (1, 1, 1, 1)
		[EffectProperty(OVERLAY_TEXTURE)]_OverlayGlow("Overlay Glow", Range(0,25)) = 1
		[EffectProperty(OVERLAY_TEXTURE)]_OverlayBlend("Overlay Blend", Range(0, 1)) = 1

		//Inner Glow
		[Effect(EffectID# INNER_GLOW, GroupID# ColorEffects)][Toggle(_INNER_GLOW_ON)]_InnerGlowOn("Inner Glow", Float) = 0
		[EffectProperty(ParentEffect# INNER_GLOW, Keywords(_INNER_GLOW_ON), AllowReset# False)][KeywordEnum(Additive, Multiply, Replace)]_InnerGlowBlending("Blending", Float) = 1.0
		[EffectProperty(INNER_GLOW)][HDR]_InnerGlowColor("Glow Color", Color) = (0, 1, 0, 0.5)
		[EffectProperty(INNER_GLOW)]_InnerGlowSize("Glow Size", Range(0, 1)) = 0.2
		[EffectProperty(INNER_GLOW)]_InnerGlowSmoothness("Smoothness", Range(0, 1.0)) = 0.8 
		[EffectProperty(INNER_GLOW)]_InnerGlowAtten("Attenuation", Range(0, 1)) = 1.0
		
		//Outer Glow
		[Effect(EffectID# OUTER_GLOW, GroupID# ColorEffects, CustomDrawer# OUTER_GLOW_EFFECT_DRAWER)][Toggle(_OUTER_GLOW_ON)]_OuterGlowOn("Outer Glow", Float) = 0
		[EffectProperty(OUTER_GLOW)][HDR]_OuterGlowColor("Glow Color", Color) = (0, 1, 0, 0.5)
		[EffectProperty(OUTER_GLOW)]_OuterGlowSize("Glow Size", Range(0, 1)) = 0.7
		[EffectProperty(OUTER_GLOW)]_OuterGlowSmoothness("Smoothness", Range(0, 1)) = 0.5
		[EffectProperty(OUTER_GLOW)]_OuterGlowAttenuation("Attenuation", Range(0, 1)) = 1.0

		//******** Outlines ********
		[Effect(EffectID# OUTLINE, GroupID# ColorEffects, CustomDrawer# OUTLINE_EFFECT_DRAWER)][Toggle(_OUTLINE_ON)] _OutlineOn("Outline", Float) = 0
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_ON), AllowReset# False)][KeywordEnum(Inside, Outside)]_Outline01Position("Outline Position", Float) = 1.0
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_ON), AllowReset# False)][KeywordEnum(Classic, Smooth)]_Outline01SmoothnessMode("Smoothness Mode", Float) = 0.0
		[EffectProperty(OUTLINE)][HDR]_Outline01Color	("Outline Color", Color) = (0,0,0,1)
		[EffectProperty(OUTLINE)]_Outline01Width ("Outline Thickness", Range(0,1)) = 0.38
		[EffectProperty(OUTLINE)]_Outline01Smoothness ("Outline Smoothness", Range(0,1)) = 0.0

		//Outline 02
		[EffectProperty(OUTLINE)][Toggle(_OUTLINE_02_ON)] _Outline02On("Outline 02", Float) = 0
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_02_ON), AllowReset# False)][KeywordEnum(Inside, Outside)]_Outline02Position("Outline Position", Float) = 1.0
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_02_ON), AllowReset# False)][KeywordEnum(Classic, Smooth)]_Outline02SmoothnessMode("Smoothness Mode", Float) = 0.0
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_02_ON), AllowReset# True)][HDR]_Outline02Color ("Outline Color", Color) = (1,0,0,1)
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_02_ON), AllowReset# True)]_Outline02Width ("Outline Thickness", Range(0,1)) = 0.178
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_02_ON), AllowReset# True)]_Outline02Smoothness ("Outline 02 Smoothness", Range(0,1)) = 0.0

		//Outline 03
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_02_ON), AllowReset# False)][Toggle(_OUTLINE_03_ON)] _Outline03On("Outline 03", Float) = 0
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_03_ON), AllowReset# False)][KeywordEnum(Inside, Outside)]_Outline03Position("Outline Position", Float) = 0.0
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_03_ON), AllowReset# False)][KeywordEnum(Classic, Smooth)]_Outline03SmoothnessMode("Smoothness Mode", Float) = 0.0
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_03_ON), AllowReset# True))][HDR]_Outline03Color ("Outline Color", Color) = (0,0,0,1)
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_03_ON), AllowReset# True)]_Outline03Width ("Outline Thickness", Range(0,1)) = 0.09
		[EffectProperty(ParentEffect# OUTLINE, Keywords(_OUTLINE_03_ON), AllowReset# True))]_Outline03Smoothness ("Outline 03 Smoothness", Range(0,1)) = 0.0

		//*************************

		//Leds
		[Effect(EffectID# LEDS, GroupID# ColorEffects)][Toggle(_LEDS_ON)]_LedOn("Led", Float) = 0
		[EffectProperty(ParentEffect# LEDS, Keywords(_LEDS_ON), AllowReset# False)][KeywordEnum(Text, Panel)]_LedsGrid("Grid", Float) = 0.0
		[EffectProperty(LEDS)]_LedsColor01("Core Color", Color) = (1.0, 1.0, 1.0, 1.0)
		[EffectProperty(LEDS)]_LedsColor02("Rim Color", Color) = (0.0, 1.0, 0.0, 1.0)
		[EffectProperty(LEDS)]_LedsColorTransition("Core Size", Range(0, 1)) = 0.3
		[EffectProperty(LEDS)]_LedsIntensity("Leds Intensity", Range(1.0, 10.0)) = 2.0
		[EffectProperty(LEDS)]_LedsEdgeDimming("Edge Dimming", Range(0.0, 1.0)) = 0.25
		[EffectProperty(ParentEffect# LEDS, IncompatibleWithKws(_LEDSGRID_PANEL), Keywords(_LEDS_ON), AllowReset# True)]_NumLeds("Num Leds", Range(5.0, 200.0)) = 64.0
		[EffectProperty(ParentEffect# LEDS, IncompatibleWithKws(_LEDSGRID_PANEL), Keywords(_LEDS_ON), AllowReset# True)]_LedSize("Led Size", Range(0.1, 1.5)) = 0.8
		[EffectProperty(ParentEffect# LEDS, IncompatibleWithKws(_LEDSGRID_PANEL), Keywords(_LEDS_ON), AllowReset# True)]_LedsRoundness("Roundness", Range(0.0, 1.0)) = 1.0
		[EffectProperty(ParentEffect# LEDS, IncompatibleWithKws(_LEDSGRID_PANEL), Keywords(_LEDS_ON), AllowReset# True)]_LedsSoftness("Softness", Range(0.0, 1.0)) = 0.1
		[EffectProperty(ParentEffect# LEDS, IncompatibleWithKws(_LEDSGRID_PANEL), Keywords(_LEDS_ON), AllowReset# True)]_LedsVariation("Variation", Range(0.0, 1.0)) = 0.15

		//Color Gradient
		[Effect(EffectID# COLOR_GRADIENT, GroupID# ColorEffects)][Toggle(_COLOR_GRADIENT_ON)]_ColorGradientOn("Color Gradient", Float) = 0
		[EffectProperty(ParentEffect# COLOR_GRADIENT, Keywords(_COLOR_GRADIENT_ON), AllowReset# False)][KeywordEnum(Rect, Character)]_GradientScope("Scope", Float) = 0.0
		[EffectProperty(ParentEffect# COLOR_GRADIENT, Keywords(_COLOR_GRADIENT_ON), AllowReset# False)][KeywordEnum(Additive, Multiply, Replace)]_GradientBlendingMode("Blending Mode", Float) = 1.0
		[EffectProperty(COLOR_GRADIENT)]_GradientBlend("Blend", Range(0, 1)) = 1.0
		[EffectProperty(COLOR_GRADIENT)][AllIn1TextShaderGradientDrawer]_ColorGradientTex("Gradient", 2D) = "white" {}
		[EffectProperty(COLOR_GRADIENT)]_GradientOrigin("Gradient Origin", Range(-1, 1)) = 0.0
		[EffectProperty(COLOR_GRADIENT)]_GradientSize("Gradient Size", Range(0, 1)) = 1.0
		[EffectProperty(COLOR_GRADIENT)]_GradientAngle("Angle", Range(0, 360)) = 0.0
		[EffectProperty(COLOR_GRADIENT)]_GradientSpeed("Gradient Speed", Range(0, 2)) = 0.0
		[EffectProperty(COLOR_GRADIENT)][Toggle(_COLOR_GRADIENT_NOISE_ON)]_ColorGradientNoiseOn("Use Noise?", Float) = 0.0
		[EffectProperty(ParentEffect# COLOR_GRADIENT, Keywords(_COLOR_GRADIENT_NOISE_ON), AllowReset# True)]_ColorGradientNoiseTex("Noise Texture", 2D) = "white" {}
		[EffectProperty(ParentEffect# COLOR_GRADIENT, Keywords(_COLOR_GRADIENT_NOISE_ON), AllowReset# True)]_ColorGradientNoiseXSpeed("X Speed", Float) = 1.0
		[EffectProperty(ParentEffect# COLOR_GRADIENT, Keywords(_COLOR_GRADIENT_NOISE_ON), AllowReset# True)]_ColorGradientNoiseYSpeed("Y Speed", Float) = 1.0
		[EffectProperty(ParentEffect# COLOR_GRADIENT, Keywords(_COLOR_GRADIENT_NOISE_ON), AllowReset# True)]_ColorGradientNoiseCutoff("Cutoff", Range(0, 1)) = 0.0
		[EffectProperty(ParentEffect# COLOR_GRADIENT, Keywords(_COLOR_GRADIENT_NOISE_ON), AllowReset# True)]_ColorGradientNoiseSmoothness("Smoothness", Range(0, 1)) = 1.0

		//Color Grading
		[Effect(EffectID# COLOR_GRADING, GroupID# ColorEffects)][Toggle(_COLOR_GRADING_ON)]_ColorGradingOn("Color Grading", Float) = 0
		[EffectProperty(COLOR_GRADING)][HDR]_ColorGradingLight("Light Color Tint", Color) = (1,0,0,1)
    	[EffectProperty(COLOR_GRADING)][HDR]_ColorGradingMiddle("Mid Tone Color Tint", Color) = (0,1,0,1)
    	[EffectProperty(COLOR_GRADING)][HDR]_ColorGradingDark("Dark/Shadow Color Tint", Color) = (0,0,1,1)
    	[EffectProperty(COLOR_GRADING)]_ColorGradingMidPoint("Mid Point", Range(0.01, 0.99)) = 0.5

		//Glitch
		[Effect(EffectID# GLITCH, GroupID# UVEffects)][Toggle(_GLITCH_ON)]_Glitch("Glitch", Float) = 0
		[EffectProperty(GLITCH)]_GlitchSize ("Glitch Size", Range(0.1, 0.5)) = 0.5
		[EffectProperty(GLITCH)]_GlitchAmount ("Glitch Amount", Range(0, 20.0)) = 10.0
        [EffectProperty(GLITCH)]_GlitchSpeed ("Glitch Speed", Float) = 10.0

		//Pixelate
		[Effect(EffectID# PIXELATE, GroupID# UVEffects)][Toggle(_PIXELATE_ON)]_Pixelate("Pixelate", Float) = 0
		[EffectProperty(PIXELATE)]_PixelateSize("Pixels Per Em", Range(4, 512)) = 32

		//Hand Drawn 
		[Effect(EffectID# HAND_DRAWN, GroupID# UVEffects)][Toggle(_HAND_DRAWN_ON)]_HandDrawn("Hand Drawn", Float) = 0
		[EffectProperty(HAND_DRAWN)]_HandDrawnNumSteps("Num Steps", Range(1.0, 10.0)) = 5.0
		[EffectProperty(HAND_DRAWN)]_HandDrawnAmount("Hand Drawn Amount", Range(0.0, 1.0)) = 0.2
		[EffectProperty(HAND_DRAWN)]_HandDrawnBlend("Blend", Range(0, 1.0)) = 1.0
		[EffectProperty(HAND_DRAWN)]_HandDrawnSpeed("Hand Drawn Speed", Float) = 5

		//Wave
		[Effect(EffectID# WAVE, GroupID# UVEffects)][Toggle(_WAVE_ON)]_Wave("Wave", Float) = 0
		[EffectProperty(WAVE)]_WaveAmount("Wave Amount", Range(0, 25)) = 7
		[EffectProperty(WAVE)]_WaveSpeed("Wave Speed", Range(0, 25)) = 10
		[EffectProperty(WAVE)]_WaveStrength("Wave Strength", Range(0, 25)) = 7.5
		[EffectProperty(WAVE)]_WaveX("Wave X Axis", Range(0, 1)) = 0
		[EffectProperty(WAVE)]_WaveY("Wave Y Axis", Range(0, 1)) = 0.5

		//Round Wave
		[Effect(EffectID# ROUND_WAVE, GroupID# UVEffects)][Toggle(_ROUND_WAVE_ON)]_RoundWave("Round Wave", Float) = 0
		[EffectProperty(ROUND_WAVE)]_RoundWaveStrength("Wave Strength", Range(0, 1)) = 0.7
		[EffectProperty(ROUND_WAVE)]_RoundWaveSpeed("Wave Speed", Range(0, 5)) = 2

		//Twist
		[Effect(EffectID# TWIST, GroupID# UVEffects)][Toggle(_TWIST_ON)]_Twist("Twist", Float) = 0
		[EffectProperty(TWIST)]_TwistUvAmount("Twist Amount", Range(0, 3.1416)) = 1
		[EffectProperty(TWIST)]_TwistUvPosX("Twist Pos X Axis", Range(0, 1)) = 0.5
		[EffectProperty(TWIST)]_TwistUvPosY("Twist Pos Y Axis", Range(0, 1)) = 0.5
		[EffectProperty(TWIST)]_TwistUvRadius("Twist Radius", Range(0, 3)) = 0.75
		[EffectProperty(TWIST)]_TwistStretchX("Stretch X", Float) = 1.0
		[EffectProperty(TWIST)]_TwistStretchY("Stretch Y", Float) = 1.0

		//Warp
		[Effect(EffectID# WARP, GroupID# UVEffects)][Toggle(_WARP_ON)]_Warp("Warp", Float) = 0
		[EffectProperty(WARP)]_WarpStrength("Warp Strength", Range(0, 0.1)) = 0.025
		[EffectProperty(WARP)]_WarpSpeed("Warp Speed", Range(0, 25)) = 8
		[EffectProperty(WARP)]_WarpScale("Warp Scale", Range(0.05, 3)) = 0.5

		//Distortion
		[Effect(EffectID# DISTORTION, GroupID# UVEffects)][Toggle(_DISTORTION_ON)]_Distortion("Distortion", Float) = 0
		[EffectProperty(DISTORTION)]_DistortTex("Distortion Texture", 2D) = "black" {}
        [EffectProperty(DISTORTION)]_DistortAmount("Distortion Amount", Range(0, 10)) = 0.5
        [EffectProperty(DISTORTION)]_DistortXSpeed("Scroll speed X", Float) = 0.1
        [EffectProperty(DISTORTION)]_DistortYSpeed("Scroll speed Y", Float) = 0.1
		[EffectProperty(DISTORTION)][KeywordEnum(Continuous, Stepped)]_DistortionMovementType("Movement Type", Float) = 0
		[EffectProperty(ParentEffect# DISTORTION, Keywords(_DISTORTIONMOVEMENTTYPE_STEPPED), AllowReset# False)]_DistortionFrequency("Frequency", Range(0, 60)) = 4

		//Alpha Cutoff
		[Effect(EffectID# ALPHA_CUTOFF, GroupID# AlphaEffects)][Toggle(_ALPHA_CUTOFF_ON)]_AlphaCutoffOn("Alpha Cutoff", Float) = 0
		[EffectProperty(ALPHA_CUTOFF)]_AlphaCutoffValue("Cutoff Value", Range(0, 1)) = 0.25

		//Flicker
		[Effect(EffectID# FLICKER, GroupID# AlphaEffects)][Toggle(_FLICKER_ON)]_Flicker("Flicker", Float) = 0
		[EffectProperty(FLICKER)]_FlickerPercent("Flicker Percent", Range(0, 1)) = 0.05
		[EffectProperty(FLICKER)]_FlickerFreq("Flicker Frequency", Range(0, 5)) = 0.2
		[EffectProperty(FLICKER)]_FlickerAlpha("Flicker Alpha", Range(0, 1)) = 0

		//Fade
		[Effect(EffectID# FADE, GroupID# AlphaEffects)][Toggle(_FADE_ON)]_FadeOn("Fade", Float) = 0
		[EffectProperty(FADE)]_FadeTex("Fade Tex", 2D) = "white" {}
		[EffectProperty(FADE)]_FadeAmount("Fade Amount", Range(0, 1)) = 0.0
		[EffectProperty(FADE)]_FadePower("Fade Power", Range(0.25, 4.0)) = 1.0
		[EffectProperty(FADE)]_FadeTransition("Fade Transition", Range(0, 0.4)) = 0.2
		[EffectProperty(FADE)][Toggle(_FADE_BURN_ON)]_FadeBurnOn("Use Fade Burn Color?", Float) = 0.0
		[EffectProperty(ParentEffect# FADE, Keywords(_FADE_BURN_ON), AllowReset# True)][HDR]_FadeBurnColor("Fade Burn Color", Color) = (1,1,0,1) 
		[EffectProperty(ParentEffect# FADE, Keywords(_FADE_BURN_ON), AllowReset# True)]_FadeBurnWidth("Fade Burn Width", Range(0, 0.2)) = 0.01
		//		

		[FontProperty] _WeightNormal	("Weight Normal", Float) = 0
		[FontProperty] _WeightBold		("Weight Bold", Float) = .5

		[FontProperty]_TextureWidth			("Texture Width", Float) = 512
		[FontProperty]_TextureHeight		("Texture Height", Float) = 512
		[FontProperty]_AtlasPadding			("Atlas Padding", Float) = 9

		//To avoid warnings related with srp compatiblity
		rectSize("rectSize", Vector) = (0.0, 0.0, 0.0, 0.0)
		bottomLeftRect("bottomLeftRect", Vector) = (0.0, 0.0, 0.0, 0.0)
		//

		//Useless properties to avoid errors with TMP
		_CullMode("_CullingMode", Float) = 0
		//
	}
    SubShader
    {
		PackageRequirements
		{
			"com.unity.render-pipelines.universal" : "12.0"
		}

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
		}

		//Low on purpose, so a project that lowers Shader.globalMaximumLOD keeps the effects; the plain SubShader at the end sits below it for maximumLOD to pick
		LOD 100

		Pass
        {
			Name "AllIn1TextShader_MainPass_URP"

			//No LightMode tag: SRPDefaultUnlit is the only pass both the Universal and the 2D Renderer draw
			Cull [_CullingMode]
			Blend [_BlendSrc] [_BlendDst]
			ZWrite [_ZWrite]
			ZTest [_ZTestMode]
			ColorMask [_ColorMask]
			Lighting Off
			Fog { Mode Off }

			Stencil
			{
				Ref [_Stencil]
				Comp [_StencilComp]
				Pass [_StencilOp]
				ReadMask [_StencilReadMask]
				WriteMask [_StencilWriteMask]
			}

            HLSLPROGRAM

			//The per character data comes from StructuredBuffers, which need 4.5 (compute); where it is missing the plain SubShader at the end draws the text
			#pragma target 4.5

			//#pragma shader_feature __ OUTLINE_ON

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			#define URP_PASS
			#define ALLIN1_FORWARD_PASS
			 
			#pragma vertex AllIn1TextShaderVertex
			#pragma fragment AllIn1TextShaderFragment
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

			#include_with_pragmas "../ShaderLibrary/AllIn1TextShader_Features.hlsl" 
			#include "../ShaderLibrary/AllIn1TextShader_InterpolatorMacros.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_CommonStructs.hlsl"

			#include "../ShaderLibrary/AllIn1TextShader_CommonFunctions.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_URPHelper.hlsl"

			#include "../ShaderLibrary/AllIn1TextShader_LightingEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_UVEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_VertexEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_ColorEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_AlphaEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_Core.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_BasePass.hlsl"

            ENDHLSL 
        }
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}

		LOD 100

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull [_CullingMode]
		Blend [_BlendSrc] [_BlendDst]
		ZWrite [_ZWrite]
		ZTest [_ZTestMode]
		ColorMask [_ColorMask]
		Fog { Mode Off }

		Pass
		{
			Name "AllIn1TextShader_Forward_BIRP"

			Tags
			{
				"LightMode" = "ForwardBase"
			}

			HLSLPROGRAM
			#pragma target 4.5

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			#define BIRP_PASS
			#define BUILTIN_MAIN_PASS

			#pragma vertex AllIn1TextShaderVertex
			#pragma fragment AllIn1TextShaderFragment

			#include "UnityCG.cginc" 
			#include "AutoLight.cginc"
			#include "UnityLightingCommon.cginc"


			#include_with_pragmas "../ShaderLibrary/AllIn1TextShader_Features.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_InterpolatorMacros.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_CommonStructs.hlsl"

			#include "../ShaderLibrary/AllIn1TextShader_CommonFunctions.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_BIRPHelper.hlsl"

			#include "../ShaderLibrary/AllIn1TextShader_LightingEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_UVEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_VertexEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_ColorEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_AlphaEffects.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_Core.hlsl"
			#include "../ShaderLibrary/AllIn1TextShader_BasePass.hlsl"

			ENDHLSL
		}
	}

	//Plain face, no effects, for GPUs without StructuredBuffers in the vertex stage (WebGL, GLES 3.0, some GLES 3.1 devices).
	//No LightMode tag, so every pipeline draws it. The face math matches CalculateCharacterMask and the end of AllIn1TextShaderFragment
	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}

		LOD 50

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull [_CullingMode]
		Blend [_BlendSrc] [_BlendDst]
		ZWrite [_ZWrite]
		ZTest [_ZTestMode]
		ColorMask [_ColorMask]
		Fog { Mode Off }

		Pass
		{
			Name "AllIn1TextShader_Plain"

			HLSLPROGRAM
			#pragma target 3.0

			#pragma shader_feature_local _BLENDINGMODE_MULTIPLY
			#pragma shader_feature_local _BLENDINGMODE_ADDITIVE
			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			#pragma vertex PlainTextVertex
			#pragma fragment PlainTextFragment

			#include "UnityCG.cginc"

			struct PlainTextAttributes
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 texcoord0 : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct PlainTextVaryings
			{
				float4 pos : SV_POSITION;
				float4 faceColor : COLOR;
				float2 uv : TEXCOORD0;
				float bias : TEXCOORD1;
				float2 positionOS : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _FaceColor;
			float _CustomFaceDilate;
			float _CustomOutlineSoftness;
			float _InnerAlpha;
			float _ColorIntensity;
			float4 _ClipRect;
			float _UIMaskSoftnessX;
			float _UIMaskSoftnessY;

			PlainTextVaryings PlainTextVertex(PlainTextAttributes v)
			{
				PlainTextVaryings o;

				//TMP flags fake bold by negating the scale it packs in uv2.y, in uv0.w since uGUI 2.0
				#if UNITY_VERSION >= 202320
				float faceDilate = _CustomFaceDilate + step(v.texcoord0.w, 0) * 0.2;
				#else
				float faceDilate = _CustomFaceDilate + step(v.texcoord1.y, 0) * 0.2;
				#endif

				o.pos = UnityObjectToClipPos(v.vertex);
				o.faceColor = v.color * _FaceColor;
				o.faceColor.rgb *= o.faceColor.a;
				o.uv = v.texcoord0.xy;
				o.bias = clamp(-faceDilate, -2.0, 0.5);
				o.positionOS = v.vertex.xy;

				return o;
			}

			float4 PlainTextFragment(PlainTextVaryings i) : SV_Target
			{
				float sdfRemapped = (tex2D(_MainTex, i.uv).a - 0.5) * 2.0;
				//Never zero: smoothstep with equal edges divides by zero on GLSL and SPIR-V, and fwidth is 0 inside a glyph
				float edgeWidth = max(max(_CustomOutlineSoftness, 2.0 * fwidth(sdfRemapped)), 0.0001);
				//Never below the atlas floor, where the field is a flat -1 over the glyph's block, unless the threshold already is
				float edgeStart = i.bias - 0.75 * max(edgeWidth - _CustomOutlineSoftness, 0.0);
				edgeStart = max(edgeStart, min(i.bias, 3.0 / 255.0 - 1.0));
				float characterBodyAlpha = smoothstep(edgeStart, edgeStart + edgeWidth, sdfRemapped) * _InnerAlpha;

				float3 finalRGB = i.faceColor.rgb * characterBodyAlpha * _ColorIntensity;
				float finalAlpha = saturate(characterBodyAlpha * i.faceColor.a);
				finalAlpha *= finalAlpha;
				finalAlpha *= finalAlpha;

			#if defined(UNITY_UI_CLIP_RECT)
				//Same as UIRectClip in the library
				float4 clipRect = clamp(_ClipRect, -2e10, 2e10);
				float2 inside = clipRect.zw - clipRect.xy - abs(i.positionOS * 2.0 - clipRect.xy - clipRect.zw);
				float2 clipMask = saturate(inside * (0.25 / (0.25 * float2(_UIMaskSoftnessX, _UIMaskSoftnessY) + max(fwidth(i.positionOS), 0.00001))));
				finalAlpha *= clipMask.x * clipMask.y;
			#endif

			#if defined(UNITY_UI_ALPHACLIP)
				clip(finalAlpha - 0.001);
			#endif

			#if defined(_BLENDINGMODE_MULTIPLY)
				float3 rgb = lerp(1.0, finalRGB, finalAlpha);
			#else
				float3 rgb = finalRGB * finalAlpha;
			#endif

				return float4(rgb, finalAlpha);
			}
			ENDHLSL
		}
	}

	CustomEditor "AllIn1Assets.AllIn1TextShader.AllIn1TextMaterialInspector"
}