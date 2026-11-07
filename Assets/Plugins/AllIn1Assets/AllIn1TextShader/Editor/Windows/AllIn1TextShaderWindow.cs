using AllIn1Assets.SharedExtensions;
using UnityEditor;

namespace AllIn1Assets.AllIn1TextShader
{
	public class AllIn1TextShaderWindow : AllIn1Asset3DWindow
	{
		private const string WINDOW_NAME = "Text Shader Window";

		[MenuItem("Tools/AllIn1/Text Shader Window", priority = 5)]
		public static void ShowAllIn13DShaderWindow()
		{
			GetWindow<AllIn1TextShaderWindow>(WINDOW_NAME);
		}

		protected override void CreateTabs()
		{
			savePathsTabDrawer = new SavePathsTabDrawer(commonStyles, this, plugin3DConfig, false);
			textureEditorTabDrawer = new TextureEditorTabDrawer(commonStyles, this, plugin3DConfig);
			textureCreatorTabDrawer = new TextureCreatorTabDrawer(commonStyles, this, plugin3DConfig);
			overrideMaterialsTabDrawer = new OverrideMaterialsTabDrawer(commonStyles, this, plugin3DConfig);
			otherTabDrawer = new OtherTextShaderTabDrawer(commonStyles, this, plugin3DConfig);

			effectsProfileTabDrawer = new EffectsProfileTabDrawer(commonStyles, this, plugin3DConfig);
		}

		protected override void ConfigureTabs()
		{
			tabDrawers = new AssetWindowTabDrawer[]
			{
				savePathsTabDrawer,
				textureEditorTabDrawer,
				textureCreatorTabDrawer,
				otherTabDrawer
			};
		}

		protected override AbstractPluginConfig FindPluginConfig()
		{
			return PluginRegistry.GetPluginByID<Plugin3DConfig>(PluginID.ALLIN1_TEXT_SHADER);
		}
	}
}