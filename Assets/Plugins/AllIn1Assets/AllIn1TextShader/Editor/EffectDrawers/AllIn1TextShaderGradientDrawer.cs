using AllIn1Assets.SharedExtensions;

namespace AllIn1Assets.AllIn1TextShader
{
	public class AllIn1TextShaderGradientDrawer : AllIn1AbstractShaderGradientDrawer
	{
		public override Plugin3DConfig GetPluginConfig()
		{
			Plugin3DConfig res = PluginRegistry.GetPluginByID<Plugin3DConfig>(PluginID.ALLIN1_TEXT_SHADER);
			return res;
		}
	}
}