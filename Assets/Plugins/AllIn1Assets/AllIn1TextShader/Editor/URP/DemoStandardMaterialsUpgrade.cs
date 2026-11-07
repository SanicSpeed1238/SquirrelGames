#if ALLIN1_URP
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AllIn1Assets.AllIn1TextShader
{
	//The Text Toolkit's only URP fix, kept out of its config so no config is resolved at load. It keeps nothing,
	//so a load that runs before the materials import just finds them on the next one
	public static class DemoStandardMaterialsUpgrade
	{
		//The Retro TV cabinet materials (Built-in Standard, magenta under URP), by GUID so finding them needs no config asset
		private static readonly string[] DEMO_STANDARD_MATERIAL_GUIDS =
		{
			"f57d6397cdfced244bfb244d3bf9ae01",
			"6dfd74c9e6482214ea2dabd64ce9e8bd",
			"c19115222f16d2f45b084c31ead89044",
		};

		[InitializeOnLoadMethod]
		private static void ScheduleUpgrade()
		{
			if (UnityEditor.MPE.ProcessService.level == UnityEditor.MPE.ProcessLevel.Secondary) { return; }

			EditorApplication.delayCall += UpgradeDemoStandardMaterials;
		}

		private static void UpgradeDemoStandardMaterials()
		{
			//A package import reloads the domain before its materials arrive
			if (EditorApplication.isCompiling || EditorApplication.isUpdating)
			{
				EditorApplication.delayCall += UpgradeDemoStandardMaterials;
				return;
			}

			if (BuildPipeline.isBuildingPlayer) { return; }
			if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)) { return; }

			StandardUpgrader standardUpgrader = null;
			for (int i = 0; i < DEMO_STANDARD_MATERIAL_GUIDS.Length; i++)
			{
				string materialPath = AssetDatabase.GUIDToAssetPath(DEMO_STANDARD_MATERIAL_GUIDS[i]);
				if (string.IsNullOrEmpty(materialPath)) { continue; }

				Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
				if (mat == null || mat.shader == null || mat.shader.name != "Standard") { continue; }

				if (standardUpgrader == null) { standardUpgrader = new StandardUpgrader("Standard"); }

				standardUpgrader.Upgrade(mat, MaterialUpgrader.UpgradeFlags.None);
				EditorUtility.SetDirty(mat);
				AssetDatabase.SaveAssetIfDirty(mat);
			}
		}
	}
}
#endif
