using AllIn1Assets.SharedExtensions;
using UnityEngine;

namespace AllIn1Assets.AllIn13DShader
{
	public class ConverterURPLit : ConverterStandard
	{
		public ConverterURPLit(Plugin3DConfig pluginConfig) : base(pluginConfig)
		{}

		public override void ApplyConversion(Material from, Material target)
		{
			base.ApplyConversion(from, target);

			target.SetTexture("_MainTex", from.GetTexture("_BaseMap"));
			target.SetTextureOffset("_MainTex", from.GetTextureOffset("_BaseMap"));
			target.SetTextureScale("_MainTex", from.GetTextureScale("_BaseMap"));
			target.SetColor("_Color", from.GetColor("_BaseColor"));
		}

		protected override void ConvertBlending()
		{
			if (from.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"))
			{
				//Lit's premultiply keyword means its shader scales the color by alpha itself, which this shader doesn't
				bool scaledInShader = from.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
				SetBlendSrc(scaledInShader ? UnityEngine.Rendering.BlendMode.SrcAlpha : (UnityEngine.Rendering.BlendMode)from.GetFloat("_SrcBlend"));
				SetBlendDst((UnityEngine.Rendering.BlendMode)from.GetFloat("_DstBlend"));

				if (from.GetFloat("_DstBlend") == (float)UnityEngine.Rendering.BlendMode.One)
				{
					SetAdditivePreset();
				}
				else
				{
					SetAlphaPreset();
				}
			}
			else
			{
				SetBlendSrc(UnityEngine.Rendering.BlendMode.One);
				SetBlendDst(UnityEngine.Rendering.BlendMode.Zero);
				SetOpaquePreset();
			}
		}
	}
}