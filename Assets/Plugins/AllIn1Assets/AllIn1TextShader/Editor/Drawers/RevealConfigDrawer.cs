using UnityEditor;
using UnityEngine;
namespace AllIn1Assets.AllIn1TextShader
{
	public class RevealConfigDrawer
	{
		private MeshEffectConfigCollection s_configCollection;
		private MeshEffectCollection s_revealEffectDatabase;

		private int configSelectedIndex;
		private string[] configNames;

		private CommonStyles s_commonStyles;

		private RevealConfigEffectsDrawer revealConfigEffectsDrawer;

		public RevealConfigDrawer()
		{
			revealConfigEffectsDrawer = new RevealConfigEffectsDrawer();
		}

		public MeshEffectGroupConfig Draw(AllIn1TextTypewriter typewriter,
			MeshEffectConfigCollection configCollection, MeshEffectCollection meshEffectCollection, MeshEffectGroupConfig config,
			string configsFolderPath, string assetPrefix, string[] easeFunctionNames, EasingFunctionsSO easeFunctionCollection,
			CommonStyles commonStyles, ref bool changes)
		{
			s_configCollection = configCollection;
			s_revealEffectDatabase = meshEffectCollection;
			s_commonStyles = commonStyles;

			MeshEffectGroupConfig res = config;

			changes = false;

			EditorGUI.BeginChangeCheck();

			configCollection.RemoveMissingConfigs();

			if(config == null)
			{
				res = configCollection.configs[0];
			}
			else if (configCollection.FindIndex(config) < 0)
			{
				//Configs shipped outside the collection folder (demo scenes) are unknown to a freshly created collection
				configCollection.RegisterConfig(config);
			}

			configCollection.SetConfigNames(ref configNames);


			EditorGUILayout.BeginHorizontal();

			if (configNames.Length > 0)
			{
				configSelectedIndex = configCollection.FindIndex(res);
				configSelectedIndex = EditorGUILayout.Popup(configSelectedIndex, configNames, GUILayout.Width(250f));

				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.ObjectField(configCollection.configs[configSelectedIndex], typeof(ScriptableObject), false, GUILayout.Width(150f));
				EditorGUI.EndDisabledGroup();
			}

			if (GUILayout.Button("+"))
			{
				MeshEffectGroupConfig revealConfigSelected = configCollection.configs[configSelectedIndex];
				res = BtnCreateRevealOrHideConfig(revealConfigSelected, configsFolderPath, assetPrefix);
			}

			EditorGUI.BeginDisabledGroup(IsBuiltInConfigSelected(configCollection));
			if (GUILayout.Button("-"))
			{
				RemoveCurrentConfig();
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();

			res = configCollection.GetConfig(configSelectedIndex);

			if (res != null)
			{
				EditorUIUtils.AddSpaces(1);
				// Read after the buttons, which change the selection
				EditorGUI.BeginDisabledGroup(IsBuiltInConfigSelected(configCollection));
				revealConfigEffectsDrawer.Draw(res, meshEffectCollection, easeFunctionNames, easeFunctionCollection, s_commonStyles, ref changes);
				EditorGUI.EndDisabledGroup();
			}

			changes = changes || EditorGUI.EndChangeCheck();

			return res;
		}

		// Presets shipped in DefaultData (Default, None) are shared by every typewriter, so they stay read-only
		private bool IsBuiltInConfig(MeshEffectGroupConfig config)
		{
			if (config == null) { return false; }

			AllIn1TextShaderConfig pluginConfig = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);
			string assetPath = AssetDatabase.GetAssetPath(config);

			bool res = pluginConfig != null && assetPath.StartsWith(pluginConfig.DefaultTypewriterConfigsFolderPath + "/");
			return res;
		}

		private bool IsBuiltInConfigSelected(MeshEffectConfigCollection configCollection)
		{
			bool res = configSelectedIndex == 0 || IsBuiltInConfig(configCollection.GetConfig(configSelectedIndex));
			return res;
		}

		// The object being inspected switches to the first preset now; other objects that used it switch when their inspector next opens
		private void RemoveCurrentConfig()
		{
			MeshEffectGroupConfig config = s_configCollection.GetConfig(configSelectedIndex);
			if (config == null) { return; }

			string fallbackName = s_configCollection.configs[0].displayName;
			string message = $"Delete the preset \"{config.displayName}\"?\n\n" +
				$"Other typewriters and splines that use it switch to \"{fallbackName}\" the next time their inspector is opened, and play no effects until then.\n\n" +
				"This can't be undone.";
			if (!EditorUtility.DisplayDialog("Delete preset", message, "Delete", "Cancel")) { return; }

			s_configCollection.DeleteConfig(config);
			configSelectedIndex = 0;

			// Reported as a change, so the caller stores the fallback and rebuilds its preview
			GUI.changed = true;
		}

		private MeshEffectGroupConfig BtnCreateRevealOrHideConfig(MeshEffectGroupConfig revealConfigSelected, string configsFolderPath, string assetPrefix)
		{
			MeshEffectGroupConfig res = s_configCollection.CreateMeshEffectGroupConfig(configsFolderPath, assetPrefix);

			res.CopyValuesFrom(revealConfigSelected);

			EditorUtility.SetDirty(res);

			configSelectedIndex = s_configCollection.configs.Length - 1;

			return res;
		}
	}
}