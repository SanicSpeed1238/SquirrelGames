using AllIn1Assets.SharedExtensions;

namespace AllIn1Assets.AllIn13DShader
{
	public class ConverterStandardBIRP : ConverterStandard
	{
		public ConverterStandardBIRP(Plugin3DConfig pluginConfig) : base(pluginConfig)
		{}

		protected override void ConvertBlending()
		{
			//The shader doesn't premultiply, so Transparent blends like Fade
			if (from.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON") || from.IsKeywordEnabled("_ALPHABLEND_ON"))
			{
				SetBlendSrc(UnityEngine.Rendering.BlendMode.SrcAlpha);
				SetBlendDst(UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				SetAlphaPreset();
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