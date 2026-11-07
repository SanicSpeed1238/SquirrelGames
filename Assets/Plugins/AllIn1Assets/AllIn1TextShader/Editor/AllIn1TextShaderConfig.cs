using AllIn1Assets.SharedExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

#if ALLIN1_URP
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.Rendering.Universal;

#if !UNITY_6000_0_OR_NEWER
using UnityEngine.Experimental.Rendering.Universal;
#endif

#endif

namespace AllIn1Assets.AllIn1TextShader
{
	public class AllIn1TextShaderConfig : Plugin3DConfig
	{
		private const string PLUGIN_NAME = "AllIn1TextShader";
		private const string VERSION = "1.0";
		private const PluginID PLUGIN_ID = PluginID.ALLIN1_TEXT_SHADER;


		public const string FOLDER_NAME_USER_TMP_FONT_ASSETS = "Font Assets TMP";
		public const string FOLDER_NAME_USER_FONT_TEXTURES = "Font Textures";
		public const string FOLDER_NAME_TEXT_MESH_PRO = "TextMesh Pro";



		private const string REVEAL_CONFIGS_FOLDER_NAME = "Reveal Configurations";
		private const string LOOPED_CONFIGS_FOLDER_NAME = "Looped Configurations";
		private const string HIDE_CONFIGS_FOLDER_NAME = "Hide Configurations";


		private static readonly string[] SHADER_NAMES = { "AllIn1Text" };

		private static readonly string[] SHADER_TEMPLATES = { };

		#region FOLDER PATHS
		public string FontTexturesFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(PluginConfigFolderPath, FOLDER_NAME_USER_FONT_TEXTURES);
				return res;
			}
		}

		public string RevealConfigsFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(PluginConfigFolderPath, REVEAL_CONFIGS_FOLDER_NAME);
				return res;
			}
		}

		public string LoopedConfigsFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(PluginConfigFolderPath, LOOPED_CONFIGS_FOLDER_NAME);
				return res;
			}
		}

		public string HideConfigsFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(PluginConfigFolderPath, HIDE_CONFIGS_FOLDER_NAME);
				return res;
			}
		}

		public string DataFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(EditorFolderPath, GlobalConstants.FOLDER_NAME_DATA);
				return res;
			}
		}

		public string MeshEffectsFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(RootPluginPath, AllIn1TextConstants.MESH_EFFECTS_FOLDER_NAME);
				return res;
			}
		}

		public string MeshEffectsRuntimeFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(MeshEffectsFolderPath, GlobalConstants.FOLDER_NAME_RUNTIME);
				return res;
			}
		}

		public string DefaultTypewriterConfigsFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(MeshEffectsRuntimeFolderPath, GlobalConstants.FOLDER_NAME_DEFAULT_DATA);
				return res;
			}
		}
		
		public string UserTMPFontAssetsFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(PluginConfigFolderPath, FOLDER_NAME_USER_TMP_FONT_ASSETS);
				return res;
			}
		}

		public string TextMeshProFolderPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(GlobalConstants.FOLDER_NAME_ASSET, FOLDER_NAME_TEXT_MESH_PRO);
				return res;
			}
		}

		public string DemoMaterialsFolderPath => EditorUtils.CombineAssetPath(DemoFolderPath, "Materials");

		#endregion

		#region FILE PATHS
		//Effect Database
		public string RevealEffectDatabasePath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(MeshEffectsRuntimeFolderPath, GlobalConstants.FOLDER_NAME_DATA, AllIn1TextConstants.REVEAL_EFFECT_DATABASE_FILE_NAME);
				return res;
			}
		}

		public string LoopedEffectDatabasePath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(MeshEffectsRuntimeFolderPath, GlobalConstants.FOLDER_NAME_DATA, AllIn1TextConstants.LOOPED_EFFECT_DATABASE_FILE_NAME);
				return res;
			}
		}

		//Config Collection
		public string HideConfigCollectionPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(PluginConfigFolderPath, AllIn1TextConstants.HIDE_CONFIG_COLLECTION_FILE_NAME);
				return res;
			}
		}

		public string LoopedConfigCollectionPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(PluginConfigFolderPath, AllIn1TextConstants.LOOPED_CONFIG_COLLECTION_FILE_NAME);
				return res;
			}
		}

		public string RevealConfigCollectionPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(PluginConfigFolderPath, AllIn1TextConstants.REVEAL_CONFIG_COLLECTION_FILE_NAME);
				return res;
			}
		}

		public string EaseFunctionCollectionLoopedPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(MeshEffectsRuntimeFolderPath, "Easing Functions/Data", AllIn1TextConstants.EASE_FUNCTION_COLLECTION_LOOPED_FILE_NAME);
				return res;
			}
		}

		public string EaseFunctionCollectionOncePath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(MeshEffectsRuntimeFolderPath, "Easing Functions/Data", AllIn1TextConstants.EASE_FUNCTION_COLLECTION_ONCE_FILE_NAME);
				return res;
			}
		}

		//Default Configs
		public string DefaultRevealConfigPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(DefaultTypewriterConfigsFolderPath, AllIn1TextConstants.DEFAULT_REVEAL_CONFIG_FILE_NAME);
				return res;
			}
		}

		public string DefaultLoopedConfigPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(DefaultTypewriterConfigsFolderPath, AllIn1TextConstants.DEFAULT_LOOPED_CONFIG_FILE_NAME);
				return res;
			}
		}

		public string DefaultHideConfigPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(DefaultTypewriterConfigsFolderPath, AllIn1TextConstants.DEFAULT_HIDE_CONFIG_FILE_NAME);
				return res;
			}
		}

		public string NoneRevealConfigPath
		{
			get
			{
				string res = EditorUtils.CombineAssetPath(DefaultTypewriterConfigsFolderPath, AllIn1TextConstants.NONE_REVEAL_CONFIG_FILE_NAME);
				return res;
			}
		}
		#endregion

		
		public MeshEffectCollection revealEffectDatabase;
		public MeshEffectCollection loopedEffectDatabase;

		public MeshEffectConfigCollection revealConfigCollection;
		public MeshEffectConfigCollection loopedConfigCollection;
		public MeshEffectConfigCollection hideConfigCollection;

		public override void Init()
		{
			base.Init();

			shaderResolver = new ShaderResolver();

			if(revealEffectDatabase == null) 
			{ 
				revealEffectDatabase = AssetDatabase.LoadAssetAtPath<MeshEffectCollection>(RevealEffectDatabasePath);
			}

			if(loopedEffectDatabase == null)
			{
				loopedEffectDatabase = AssetDatabase.LoadAssetAtPath<MeshEffectCollection>(LoopedEffectDatabasePath);
			}

			CheckVisibilityConfigCollection(ref hideConfigCollection, DefaultHideConfigPath, HideConfigCollectionPath, false);
			CheckVisibilityConfigCollection(ref loopedConfigCollection, DefaultLoopedConfigPath, LoopedConfigCollectionPath, true);
			CheckVisibilityConfigCollection(ref revealConfigCollection, DefaultRevealConfigPath, RevealConfigCollectionPath, false);
			CheckNoneRevealConfig();
		}

		// Right after DEFAULT, so the null fallback to index 0 keeps picking DEFAULT
		private void CheckNoneRevealConfig()
		{
			if (revealConfigCollection == null || revealConfigCollection.configs == null || revealConfigCollection.configs.Length == 0) { return; }

			MeshEffectGroupConfig noneConfig = AssetDatabase.LoadAssetAtPath<MeshEffectGroupConfig>(NoneRevealConfigPath);
			if (noneConfig == null)
			{
				// Present but not imported yet (a reload runs Init before new assets import): OnAssetsProcessed retries once it lands
				if (File.Exists(NoneRevealConfigPath) || revealEffectDatabase == null) { return; }

				noneConfig = CreateNoneRevealConfig();
			}

			if (revealConfigCollection.FindIndex(noneConfig) >= 0) { return; }

			ArrayUtility.Insert(ref revealConfigCollection.configs, 1, noneConfig);
			EditorUtility.SetDirty(revealConfigCollection);
		}

		public override void OnAssetsProcessed(string[] importedAssets, string[] deletedAssets)
		{
			base.OnAssetsProcessed(importedAssets, deletedAssets);

			if (initialized && Array.IndexOf(importedAssets, NoneRevealConfigPath) >= 0)
			{
				CheckNoneRevealConfig();
			}

			RefreshBlurredAtlasesOfFonts(importedAssets);
		}

		//A font asset regenerated or grown since its blurred atlas was made would leave Bevel, Outline and Drop Shadow reading old glyphs
		private void RefreshBlurredAtlasesOfFonts(string[] importedAssets)
		{
			for (int i = 0; i < importedAssets.Length; i++)
			{
				if (!importedAssets[i].EndsWith(".asset") || AssetDatabase.GetMainAssetTypeAtPath(importedAssets[i]) != typeof(TMP_FontAsset)) { continue; }

				TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(importedAssets[i]);
				if (fontAsset != null)
				{
					AllIn1TextUtils.RefreshBlurredAtlasesOf(this, fontAsset);
				}
			}
		}

		private MeshEffectGroupConfig CreateNoneRevealConfig()
		{
			MeshEffectGroupConfig res = CreateInstance<MeshEffectGroupConfig>();

			res.displayName = "None";
			res.traversalOrder = TraversalOrder.LEFT_TO_RIGHT;
			res.effectConfigs = revealEffectDatabase.CreateMeshEffectConfigs();
			for (int i = 0; i < res.effectConfigs.Length; i++)
			{
				res.effectConfigs[i].enabled = false;
			}

			AssetDatabase.CreateAsset(res, NoneRevealConfigPath);

			return res;
		}

		private void CheckVisibilityConfigCollection(ref MeshEffectConfigCollection configCollection, string defaultConfigPath, string path, bool isLooped)
		{
			if (!File.Exists(path))
			{
				configCollection = CreateInstance<MeshEffectConfigCollection>();

				MeshEffectGroupConfig defaultRevealConfig = AssetDatabase.LoadAssetAtPath<MeshEffectGroupConfig>(defaultConfigPath);
				if(defaultRevealConfig == null)
				{
					defaultRevealConfig = CreateDefaultRevealConfig(isLooped ? loopedEffectDatabase : revealEffectDatabase, defaultConfigPath, isLooped);
				}

				configCollection.configs = new MeshEffectGroupConfig[1];
				configCollection.configs[0] = defaultRevealConfig;

				AssetDatabase.CreateAsset(configCollection, path);
			}
			else
			{
				if (configCollection == null)
				{
					configCollection = AssetDatabase.LoadAssetAtPath<MeshEffectConfigCollection>(path);
				}
			}
		}

		private MeshEffectGroupConfig CreateDefaultRevealConfig(MeshEffectCollection meshEffectCollection, string defaultConfigPath, bool isLooped)
		{
			MeshEffectGroupConfig res = CreateInstance<MeshEffectGroupConfig>();

			res.displayName = "Default";
			res.traversalOrder = TraversalOrder.LEFT_TO_RIGHT;
			res.effectConfigs = meshEffectCollection.CreateMeshEffectConfigs();
			res.isLooped = isLooped;

			AssetDatabase.CreateAsset(res, defaultConfigPath);

			return res;
		}

		protected override void RefreshShadersReferences()
		{
			shStandard = Shader.Find("AllIn1Text/" + SHADER_NAMES[0]);
		}

		public override string[] GetShaderNames()
		{
			return SHADER_NAMES;
		}

		public override string[] GetShaderTemplates()
		{
			return SHADER_TEMPLATES;
		}

		public override string GetVersion()
		{
			return VERSION;
		}

		public override string GetPluginName() 
		{
			return PLUGIN_NAME;
		}

		public override PluginID GetPluginID()
		{
			return PLUGIN_ID;
		}

		public override string GetRenderPipelineDocURL()
		{
			return $"{GlobalConstants.URL_DOCS_SITE}/text-toolkit/setup-and-render-pipelines";
		}

		//Text needs no renderer setting, only the demo's Standard materials upgraded, which happens quietly on load, so it never raises the URP prompt
		public override bool NeedsURPConfiguration()
		{
			return false;
		}

		public override Shader GetShaderAfterMaterialRemoval()
		{
			Shader res = Shader.Find("TextMeshPro/Mobile/Distance Field");
			return res;
		}

		protected override void CreateFolders()
		{
			base.CreateFolders();

			EditorUtils.CreateFolderIfNotExist(FontTexturesFolderPath);
			EditorUtils.CreateFolderIfNotExist(RevealConfigsFolderPath);
			EditorUtils.CreateFolderIfNotExist(LoopedConfigsFolderPath);
			EditorUtils.CreateFolderIfNotExist(HideConfigsFolderPath);
			EditorUtils.CreateFolderIfNotExist(UserTMPFontAssetsFolderPath);
		}

		public override PropertiesConfig CreatePropertiesConfig()
		{
			PropertiesConfig res = new AllIn1TextPropertiesConfig();
			return res;
		}

		public override PropertiesConfigCollection CreatePropertiesConfigSO()
		{
			PropertiesConfigCollection res = ScriptableObject.CreateInstance<AllIn1TextPropertiesConfigSO>();
			return res;
		}

		public override bool IsWritableFolder(string folderPath)
		{
			string normalizedFolderPath = EditorUtils.NormalizeAssetPath(folderPath);

			bool isInPluginFolder = normalizedFolderPath.StartsWith(PluginConfigFolderPath);
			bool isInTMPFolder = normalizedFolderPath.StartsWith(TextMeshProFolderPath);

			bool isRestrictedFolder = !isInPluginFolder || isInTMPFolder;

			bool res = !isRestrictedFolder;
			return res;
		}

		public string GetBlurredAtlasName(string originalAtlasName)
		{
			string res = $"{originalAtlasName}_Blurred";
			return res;
		}

		public string GetBlurredAtlasNameWithExtension(string originalAtlasName)
		{
			string res = $"{GetBlurredAtlasName(originalAtlasName)}.png";
			return res;
		}

		public Texture FindBlurredTextureByFilename(string atlasName)
		{
			Texture res = null;

			string blurredAtlasFilenameWithExtension = $"{GetBlurredAtlasName(atlasName)}.png";
			string blurredAtlasPath = EditorUtils.CombineAssetPath(FontTexturesFolderPath, blurredAtlasFilenameWithExtension);

			if (!File.Exists(blurredAtlasPath))
			{
				blurredAtlasPath = EditorUtils.CombineAssetPath(DemoFolderPath, "Font Atlases", blurredAtlasFilenameWithExtension);
			}

			res = AssetDatabase.LoadAssetAtPath<Texture>(blurredAtlasPath);

			return res;
		}

		public void RefreshMeshEffectConfigCollections()
		{
			RefreshMeshEffectConfigCollection(revealConfigCollection, revealEffectDatabase);
			RefreshMeshEffectConfigCollection(loopedConfigCollection, loopedEffectDatabase);
			RefreshMeshEffectConfigCollection(hideConfigCollection, revealEffectDatabase);
		}

		private void RefreshMeshEffectConfigCollection(MeshEffectConfigCollection configCollection, MeshEffectCollection meshEffectCollection)
		{
			for (int i = 0; i < configCollection.configs.Length; i++)
			{
				MeshEffectGroupConfig meshEffectGroupConfig = configCollection.configs[i];
				if (meshEffectGroupConfig == null) { continue; }

				Undo.RecordObject(meshEffectGroupConfig, "Refresh Mesh Effects configuration");

				MeshEffectConfig[] oldMeshEffectConfigsCopy = new MeshEffectConfig[meshEffectGroupConfig.effectConfigs.Length];
				for(int j = 0; j < oldMeshEffectConfigsCopy.Length; j++)
				{
					oldMeshEffectConfigsCopy[j] = meshEffectGroupConfig.effectConfigs[j].DeepCopy();
				}

				meshEffectGroupConfig.effectConfigs = meshEffectCollection.CreateMeshEffectConfigs();

				meshEffectGroupConfig.RefreshEffectProperties(meshEffectCollection, oldMeshEffectConfigsCopy);

				EditorUtility.SetDirty(meshEffectGroupConfig);
			}

			EditorUtility.SetDirty(configCollection);
		}

		public override bool IsDerivedFromAllIn13DShader()
		{
			return true;
		}

#if ALLIN1_DEVELOP
		//Shared demo files the Text demo never uses (by GUID, type use and name, 2026-10-18) stay out of its package, with the credits they need
		private static readonly string[] UNUSED_SHARED_DEMO_PATHS =
		{
			"Materials",
			"Meshes",
			"Scripts/UI",
			"Scripts/AllIn1DemoButtonPress.cs",
			"Scripts/Billboard.cs",
			"Scripts/DemoCameraController.cs",
			"Scripts/DemoElement.cs",
			"Scripts/DemoElementUI.cs",
			"Scripts/DemoExpositor.cs",
			"Scripts/DemoExpositorMovement.cs",
			"Scripts/DemoSceneController.cs",
			"Scripts/DemoUI.cs",
			"Scripts/MaterialPrewarmer.cs",
			"Scripts/ObjectSpawner.cs",
			"Scripts/ScatterAroundCircle.cs",
			"Scripts/Tweens/PropertyTweenCollection.cs",
			"Scripts/Tweens/SinVerticalPositionTween.cs",
			"Scripts/Tweens/TransformScaler.cs",
			"Shaders/SkyStylized.shader",
			"Textures/Ui",
			"Textures/AlienStatueAO.tif",
			"Textures/AlienStatueAlbedo.tif",
			"Textures/AlienStatueEmission.tif",
			"Textures/AlienStatueNormal.tif",
			"Textures/DaySkyRamp.png",
			"Textures/Galaxy.png",
			"Textures/MossNormal.tif",
			"Textures/NightSkyRamp.png",
			"Textures/RampTexture1.png",
			"Textures/Ramp_01.tif",
			"Textures/WaterMossAlbedo.tif",
		};

		public override string[] GetAssetPathsToExcludeFromExport()
		{
			string sharedDemoFolderPath = PluginRegistry.GetSharedConfig().AllIn1SharedDemoFolderPath;

			string[] res = new string[UNUSED_SHARED_DEMO_PATHS.Length];
			for (int i = 0; i < res.Length; i++)
			{
				res[i] = EditorUtils.CombineAssetPath(sharedDemoFolderPath, UNUSED_SHARED_DEMO_PATHS[i]);
			}

			return res;
		}
#endif

#if ALLIN1_URP
		public override void ConfigureRenderPipeline(UniversalRendererData universalRendererData, RenderPipelineAsset renderPipelineAsset)
		{
			ConvertMaterialsFolder();
		}

		public override void ConvertMaterialsFolder()
		{
			string dirPath = DemoMaterialsFolderPath;

			StandardUpgrader standardUpgrader = new StandardUpgrader("Standard");
			DirectoryInfo dir = new DirectoryInfo(dirPath);

			List<string> materialsPathsToConvert = new List<string>();
			FileInfo[] files = dir.GetFiles("*.mat");
			for (int i = 0; i < files.Length; i++)
			{
				if (!files[i].Name.StartsWith("MAT_Standard")) { continue; }

				string materialPath = EditorUtils.CombineAssetPath(dirPath, files[i].Name);
				materialsPathsToConvert.Add(materialPath);
			}

			for (int i = 0; i < materialsPathsToConvert.Count; i++)
			{
				Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialsPathsToConvert[i]);

				standardUpgrader.Upgrade(mat, MaterialUpgrader.UpgradeFlags.None);
			}
		}
#endif
	}
}