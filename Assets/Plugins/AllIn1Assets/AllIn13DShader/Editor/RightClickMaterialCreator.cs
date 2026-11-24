using AllIn1Assets.SharedExtensions;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn13DShader
{
	public static class RightClickMaterialCreator
	{
		private const string MENU_PATH = "Assets/Create/AllIn13DShader/Materials";

		public static void CreateMaterial(Material matSource, AllIn13DShaderConfig pluginConfig)
		{
			string saveFolderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

			if (!AssetDatabase.IsValidFolder(saveFolderPath))
			{
				saveFolderPath = pluginConfig.MaterialSavePath;
			}

			Material mat = new Material(matSource);

			string materialPath = EditorUtils.CombineAssetPath(saveFolderPath, $"MAT_{pluginConfig.GetPluginName()}.mat");
			materialPath = AssetDatabase.GenerateUniqueAssetPath(materialPath);
			AssetDatabase.CreateAsset(mat, materialPath);

			AssetDatabase.SaveAssets();
			/*AssetDatabase.Refresh();*/

			Selection.activeObject = mat;
		}

		private static Material FindPresetMaterial(AllIn13DShaderConfig pluginConfig, string materialName)
		{
			MaterialPreset[] presets = pluginConfig.materialPresetCollection.materialPresets;
			for (int i = 0; i < presets.Length; i++)
			{
				if (presets[i].material != null && presets[i].material.name == materialName)
				{
					return presets[i].material;
				}
			}

			Debug.LogWarning($"[AllIn1] No material preset uses {materialName}, creating the default material instead.");
			return pluginConfig.defaultPreset.material;
		}

		[MenuItem(MENU_PATH + "/Default Material", false, 1)]
		public static void CreateMaterialDefault()
		{
			AllIn13DShaderConfig pluginConfig = PluginRegistry.GetPluginByID<AllIn13DShaderConfig>(PluginID.ALL_IN_1_3D_SHADER);

			CreateMaterial(pluginConfig.defaultPreset.material, pluginConfig);
		}

		[MenuItem(MENU_PATH + "/Toon Material", false, 1)]
		public static void CreateMaterialToon()
		{
			AllIn13DShaderConfig pluginConfig = PluginRegistry.GetPluginByID<AllIn13DShaderConfig>(PluginID.ALL_IN_1_3D_SHADER);

			CreateMaterial(FindPresetMaterial(pluginConfig, "MAT_Preset_Toon"), pluginConfig);
		}

		[MenuItem(MENU_PATH + "/PBR Material", false, 1)]
		public static void CreateMaterialPBR()
		{
			AllIn13DShaderConfig pluginConfig = PluginRegistry.GetPluginByID<AllIn13DShaderConfig>(PluginID.ALL_IN_1_3D_SHADER);

			CreateMaterial(FindPresetMaterial(pluginConfig, "MAT_Preset_StandardPBR"), pluginConfig);
		}

		[MenuItem(MENU_PATH + "/Basic Lighting Material", false, 1)]
		public static void CreateMaterialBasic()
		{
			AllIn13DShaderConfig pluginConfig = PluginRegistry.GetPluginByID<AllIn13DShaderConfig>(PluginID.ALL_IN_1_3D_SHADER);

			CreateMaterial(FindPresetMaterial(pluginConfig, "MAT_Preset_Basic"), pluginConfig);
		}

		[MenuItem(MENU_PATH + "/AllIn13D Look", false, 1)]
		public static void CreateMaterialAllIn3DLook()
		{
			AllIn13DShaderConfig pluginConfig = PluginRegistry.GetPluginByID<AllIn13DShaderConfig>(PluginID.ALL_IN_1_3D_SHADER);

			CreateMaterial(FindPresetMaterial(pluginConfig, "MAT_Preset_AllIn13DShaderLook"), pluginConfig);
		}
	}
}