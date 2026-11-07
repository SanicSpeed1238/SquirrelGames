using UnityEditor;

namespace AllIn1Assets.AllIn1TextShader
{
	public static class RegistryInserter
	{
		//Declaring the plugin costs nothing: no asset is read, found or created here. The registry
		//resolves the config asset the first time something actually asks for it
		[InitializeOnLoadMethod]
		private static void InitializeOnLoad()
		{
			PluginRegistry.Register(PluginID.ALLIN1_TEXT_SHADER, typeof(AllIn1TextShaderConfig));
		}
	}
}
