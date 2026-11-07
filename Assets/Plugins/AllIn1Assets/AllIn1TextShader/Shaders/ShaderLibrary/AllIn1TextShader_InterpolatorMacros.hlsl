#ifndef ALLIN1TEXTSHADER_INTERPOLATORMACROS
#define ALLIN1TEXTSHADER_INTERPOLATORMACROS

#define GET_MAIN_UV(input)				input.texcoord0.xy

#define GET_FACE_UV(input) input.interpolator_00.xy
#define SET_FACE_UV(input, value) input.interpolator_00.xy = value;

#define GET_BIAS(input)					input.interpolator_00.w
#define SET_BIAS(input, value)			input.interpolator_00.w = value;

#define GET_POSITION_OS(input)			input.interpolator_01.xyz
#define SET_POSITION_OS(input, value)	input.interpolator_01.xyz = value;

#define GET_VERTEX_ID(input) input.interpolator_01.w
#define SET_VERTEX_ID(input, value) input.interpolator_01.w = value;

#define GET_VIEWDIR_WS(input) input.interpolator_02.xyz
#define SET_VIEWDIR_WS(input, value) input.interpolator_02.xyz = value;

#define GET_PIXEL_SIZE_MASK(input) input.interpolator_02.w
#define SET_PIXEL_SIZE_MASK(input, value) input.interpolator_02.w = value;

#define GET_RECT_UV(input) input.interpolator_03.xy
#define SET_RECT_UV(input, value) input.interpolator_03.xy = value;

#define GET_CHAR_UV_NORMALIZED(input) input.interpolator_03.zw
#define SET_CHAR_UV_NORMALIZED(input, value) input.interpolator_03.zw = value;

#define GET_FACE_COLOR(input) input.interpolator_04.xyzw
#define SET_FACE_COLOR(input, value) input.interpolator_04.xyzw = value;

#define GET_UV_NORMALIZED_RAW(input) input.interpolator_05.xy
#define SET_UV_NORMALIZED_RAW(input, value) input.interpolator_05.xy = value;

#define GET_FACE_DILATE(input) input.interpolator_05.z
#define SET_FACE_DILATE(input, value) input.interpolator_05.z = value;

#define GET_TEXEL_SIZE(input) float2(input.interpolator_05.w, input.interpolator_06.x)
#define SET_TEXEL_SIZE(input, value) \
	input.interpolator_05.w = value.x; \
	input.interpolator_06.x = value.y;
	
#define GET_VERTEX_VS(input) input.interpolator_06.yzw
#define SET_VERTEX_VS(input, value) input.interpolator_06.yzw = value;

#define GET_SHADER_TIME(input) _Time.y

#endif //ALLIN1TEXTSHADER_INTERPOLATORMACROS