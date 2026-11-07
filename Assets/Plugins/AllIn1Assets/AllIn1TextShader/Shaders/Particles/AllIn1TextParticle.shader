//UI particles for AllIn1TextParticles: premultiplied output so one shader does Additive (One One) and Alpha Blend (One OneMinusSrcAlpha).
//uv1.x carries each particle's HDR intensity; plain UI graphics leave it 0, which reads as 1
Shader "AllIn1Text/UI Particle"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Intensity ("Intensity", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

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
		Blend [_SrcBlend] [_DstBlend]
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				half4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 local : TEXCOORD1;
				half intensity : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Intensity;
			float4 _ClipRect;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.local = v.vertex;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				o.intensity = (v.uv1.x > 0 ? v.uv1.x : 1) * _Intensity;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 t = tex2D(_MainTex, i.uv);
				half a = t.a * i.color.a;
				#ifdef UNITY_UI_CLIP_RECT
				a *= UnityGet2DClipping(i.local.xy, _ClipRect);
				#endif
				return half4(t.rgb * i.color.rgb * a * i.intensity, a);
			}
			ENDCG
		}
	}
}
