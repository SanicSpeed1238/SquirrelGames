using AllIn1Assets.SharedExtensions;
using UnityEngine;

namespace AllIn1Assets.AllIn13DShader
{
	public class ConverterStandard : ConverterGeneral
	{
		public ConverterStandard(Plugin3DConfig pluginConfig) : base(pluginConfig)
		{}

		public override void ApplyConversion(Material from, Material target)
		{
			base.ApplyConversion(from, target);

			bool cutout = from.IsKeywordEnabled("_ALPHATEST_ON");
			SetEnableEffect("ALPHA_CUTOFF", cutout);
			if (cutout)
			{
				target.SetFloat("_AlphaCutoffValue", from.GetFloat("_Cutoff"));
			}

			bool emissionEffectEnabled = from.IsKeywordEnabled("_EMISSION");
			SetEnableEffect("EMISSION", emissionEffectEnabled);

			bool normalMapEffectEnabled = from.IsKeywordEnabled("_NORMALMAP");
			SetEnableEffect("NORMAL_MAP", normalMapEffectEnabled);

			EnableLightModelClassic();

			EnablePBR();
			
			target.SetFloat("_ReflectionsAtten", 1.0f);

			bool specularEnabled = !from.IsKeywordEnabled("_SPECULARHIGHLIGHTS_OFF");
			if (specularEnabled)
			{
				EnableSpecularClassic();
				target.SetFloat("_SpecularAtten", 1.0f);
			}
			else
			{
				DisableEffect("SPECULARMODEL");
			}

			DisableEffect("RIM_LIGHTING");

			//Blending Mode
			ConvertBlending();
		}

		protected virtual void ConvertBlending()
		{
		
		}
	}
}