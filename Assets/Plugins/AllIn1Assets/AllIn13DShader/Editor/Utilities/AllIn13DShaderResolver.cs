using AllIn1Assets.SharedExtensions;
using UnityEngine;

namespace AllIn1Assets.AllIn13DShader
{
	public class AllIn13DShaderResolver : ShaderResolver
	{
		public override bool ResolveShader(Material targetMat, Plugin3DConfig pluginConfig)
		{
			AllIn13DShaderConfig config = pluginConfig as AllIn13DShaderConfig;
			if (targetMat == null || targetMat.shader == null || config == null) { return false; }

			//The override preview also runs on other shaders' materials, and baked variants keep their shader, as in the material inspector
			Shader shader = targetMat.shader;
			bool isGenericShader = shader == config.shStandard || shader == config.shStandardNoShadowCaster || shader == config.shOutline || shader == config.shOutlineNoShadowCaster;
			if (!isGenericShader) { return false; }

			return MaterialUtils.CheckMaterialShader(targetMat, config);
		}
	}
}
