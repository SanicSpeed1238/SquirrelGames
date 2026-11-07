//Background of an AllIn1TextLedPanel: the unlit dots and the panel face between them.
//The grid and the dot shape come in the vertex data, so every text lit on the panel matches it exactly
Shader "AllIn1Text/LedPanel"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_PanelColor("Panel Color", Color) = (0.02, 0.02, 0.02, 1.0)
		_OffColor("Off Dot Color", Color) = (0.1, 0.1, 0.1, 1.0)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
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
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
			"RenderPipeline" = "UniversalPipeline"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Default"

			//No LightMode tag: SRPDefaultUnlit is the only pass both the Universal and the 2D Renderer draw

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "../ShaderLibrary/AllIn1TextShader_LedCommon.hlsl"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float4 vertex		: POSITION;
				float4 color		: COLOR;
				float4 texcoord		: TEXCOORD0;	//xy grid position in cells, zw columns and rows
				float4 texcoord1	: TEXCOORD1;	//dot size, roundness, softness, variation
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex			: SV_POSITION;
				fixed4 color			: COLOR;
				float4 grid				: TEXCOORD0;
				float4 dotParams		: TEXCOORD1;
				float4 worldPosition	: TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			fixed4 _PanelColor;
			fixed4 _OffColor;
			float4 _ClipRect;

			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.grid = v.texcoord;
				OUT.dotParams = v.texcoord1;
				OUT.color = v.color;

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 gridPos = IN.grid.xy;
				float2 cellId = floor(gridPos);

				float dotDistance = LedDotDistance(gridPos, IN.dotParams.y);
				float dotMask = LedDotMask(dotDistance, IN.dotParams.x, IN.dotParams.z) * LedInsideCells(cellId, IN.grid.zw);

				float4 offColor = _OffColor;
				offColor.rgb *= 1.0 - IN.dotParams.w * LedCellHash(cellId);
				float offAlpha = offColor.a * dotMask;

				//Unlit dot over the panel face, straight alpha
				float4 color;
				color.a = offAlpha + _PanelColor.a * (1.0 - offAlpha);
				color.rgb = (offColor.rgb * offAlpha + _PanelColor.rgb * _PanelColor.a * (1.0 - offAlpha)) / max(color.a, 0.0001);
				color *= IN.color;

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip(color.a - 0.001);
				#endif

				return color;
			}
			ENDCG
		}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Default"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "../ShaderLibrary/AllIn1TextShader_LedCommon.hlsl"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float4 vertex		: POSITION;
				float4 color		: COLOR;
				float4 texcoord		: TEXCOORD0;	//xy grid position in cells, zw columns and rows
				float4 texcoord1	: TEXCOORD1;	//dot size, roundness, softness, variation
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex			: SV_POSITION;
				fixed4 color			: COLOR;
				float4 grid				: TEXCOORD0;
				float4 dotParams		: TEXCOORD1;
				float4 worldPosition	: TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			fixed4 _PanelColor;
			fixed4 _OffColor;
			float4 _ClipRect;

			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.grid = v.texcoord;
				OUT.dotParams = v.texcoord1;
				OUT.color = v.color;

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 gridPos = IN.grid.xy;
				float2 cellId = floor(gridPos);

				float dotDistance = LedDotDistance(gridPos, IN.dotParams.y);
				float dotMask = LedDotMask(dotDistance, IN.dotParams.x, IN.dotParams.z) * LedInsideCells(cellId, IN.grid.zw);

				float4 offColor = _OffColor;
				offColor.rgb *= 1.0 - IN.dotParams.w * LedCellHash(cellId);
				float offAlpha = offColor.a * dotMask;

				//Unlit dot over the panel face, straight alpha
				float4 color;
				color.a = offAlpha + _PanelColor.a * (1.0 - offAlpha);
				color.rgb = (offColor.rgb * offAlpha + _PanelColor.rgb * _PanelColor.a * (1.0 - offAlpha)) / max(color.a, 0.0001);
				color *= IN.color;

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip(color.a - 0.001);
				#endif

				return color;
			}
			ENDCG
		}
	}
}