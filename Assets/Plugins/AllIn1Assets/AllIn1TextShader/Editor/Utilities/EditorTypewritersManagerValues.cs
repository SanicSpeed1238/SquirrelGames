using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class EditorTypewritersManagerValues
	{
		public float lastTime;
		public float editorDeltaTime;
		public AllIn1TextTypewriter[] typewritersInScene;
		public AllIn1TextTypewriter[] typewritersInPrefabStage;
		public TextMaterialConfiguratorMaster[] materialConfiguratorsInScene;
		public TextMaterialConfiguratorMaster[] materialConfiguratorsInPrefabStage;
		public GameObject goPrefabRoot;
		public bool firstUpdateAfterProjectReload = false;

		public bool IsInPrefabStage()
		{
			bool res = goPrefabRoot != null;
			return res;
		}

		public void UpdateTypewritersInScene()
		{
			UpdateTypewriters(typewritersInScene);
		}

		public void UpdateTypewritersInPrefabStage()
		{
			UpdateTypewriters(typewritersInPrefabStage);
		}

		public void UpdateMaterialConfiguratorsInScene()
		{
			bool forceRefresh = firstUpdateAfterProjectReload;
			UpdateMaterialConfigurators(ref materialConfiguratorsInScene, forceRefresh);
		}

		public void UpdateMaterialConfiguratorsInPrefabStage()
		{
			bool forceRefresh = firstUpdateAfterProjectReload;
			UpdateMaterialConfigurators(ref materialConfiguratorsInPrefabStage, forceRefresh);
		}

		private void UpdateTypewriters(AllIn1TextTypewriter[] typewriters)
		{
			if (typewriters == null) { return; }

			for (int i = 0; i < typewriters.Length; i++)
			{
				if (typewriters[i] != null && typewriters[i].isActiveAndEnabled)
				{
					typewriters[i].UpdateEditor(editorDeltaTime);
				}
			}
		}

		private void UpdateMaterialConfigurators(ref TextMaterialConfiguratorMaster[] materialConfigurators, bool forceRefreshBuffer)
		{
			if (materialConfigurators == null) { return; }

			for (int i = materialConfigurators.Length - 1; i >= 0; i--)
			{
				if(materialConfigurators[i] == null)
				{
					ArrayUtility.RemoveAt(ref materialConfigurators, i);
					continue;
				}
				else if(materialConfigurators[i].isActiveAndEnabled)
				{
					materialConfigurators[i].UpdateConfiguration(forceRefreshBuffer);

					//If needsRefreshFontReferences is true, is very likely that font asset has been changed
					if (materialConfigurators[i].needsRefreshFontReferences)
					{
						RefreshFontValues(materialConfigurators[i]);
					}
				}
			}
		}

		private void RefreshFontValues(TextMaterialConfiguratorMaster materialConfiguratorMaster)
		{
			AllIn1TextShaderConfig allIn1TextShaderConfig = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);
			TMP_FontAsset fontAsset = materialConfiguratorMaster.GetFontAsset();
			Texture blurredAtlasTex = allIn1TextShaderConfig.FindBlurredTextureByFilename(fontAsset.atlasTexture.name);

			if (blurredAtlasTex == null)
			{
				string blurredAtlasName = allIn1TextShaderConfig.GetBlurredAtlasName(fontAsset.atlasTexture.name);
				blurredAtlasTex = AllIn1TextUtils.CreateBlurredAtlas(allIn1TextShaderConfig.FontTexturesFolderPath, fontAsset.atlasTexture.name, blurredAtlasName, fontAsset.atlasTexture);
			}

			materialConfiguratorMaster.SetBlurredAtlas(blurredAtlasTex);

			materialConfiguratorMaster.needsRefreshFontReferences = false;
		}

		public void AddTypewriterInScene(AllIn1TextTypewriter typewriter)
		{
			if (typewritersInScene == null) { return; }

			RemoveNullRegistersFromTypewriterCollection(ref typewritersInScene);
			ArrayUtility.Add(ref typewritersInScene, typewriter);
		}

		public void AddMaterialConfiguratorInScene(TextMaterialConfiguratorMaster materialConfigurator)
		{
			if (materialConfiguratorsInScene == null) { return; }


			RemoveNullRegistersFromMaterialConfiguratorCollection(ref materialConfiguratorsInScene);
			ArrayUtility.Add(ref materialConfiguratorsInScene, materialConfigurator);
		}

		private void RemoveNullRegistersFromTypewriterCollection(ref AllIn1TextTypewriter[] typewriters)
		{
			for (int i = typewriters.Length - 1; i >= 0; i--)
			{
				if (typewriters[i] == null)
				{
					ArrayUtility.RemoveAt(ref typewritersInScene, i);
				}
			}
		}

		private static void RemoveNullRegistersFromMaterialConfiguratorCollection(ref TextMaterialConfiguratorMaster[] materialConfigurators)
		{
			for (int i = materialConfigurators.Length - 1; i >= 0; i--)
			{
				if (materialConfigurators[i] == null)
				{
					ArrayUtility.RemoveAt(ref materialConfigurators, i);
				}
			}
		}

		public void RefreshCoordinatorsReferences()
		{
			typewritersInScene = GameObject.FindObjectsByType<AllIn1TextTypewriter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			materialConfiguratorsInScene = GameObject.FindObjectsByType<TextMaterialConfiguratorMaster>(FindObjectsInactive.Include, FindObjectsSortMode.None);

			if (IsInPrefabStage())
			{
				typewritersInPrefabStage = goPrefabRoot.GetComponentsInChildren<AllIn1TextTypewriter>();
				materialConfiguratorsInPrefabStage = goPrefabRoot.GetComponentsInChildren<TextMaterialConfiguratorMaster>();
			}
		}

		public void ReleaseBuffers()
		{
			if (materialConfiguratorsInScene == null) { return; }

			for (int i = 0; i < materialConfiguratorsInScene.Length; i++)
			{
				if (materialConfiguratorsInScene[i] != null)
				{
					materialConfiguratorsInScene[i].ReleaseBuffers();
				}
			}
		}

		public void InitTypwriters()
		{
			for (int i = 0; i < typewritersInScene.Length; i++)
			{
				if (typewritersInScene[i] != null)
				{
					typewritersInScene[i].Init();
				}
			}

			for(int i = 0; i < typewritersInPrefabStage.Length; i++)
			{
				if (typewritersInPrefabStage[i] != null)
				{
					typewritersInPrefabStage[i].Init();
				}
			}
		}

		public void InitMaterialConfigurators()
		{
			for (int i = 0; i < materialConfiguratorsInScene.Length; i++)
			{
				if (materialConfiguratorsInScene[i] != null)
				{
					materialConfiguratorsInScene[i].Init();
				}
			}

			for(int i = 0; i < materialConfiguratorsInPrefabStage.Length; i++)
			{
				if (materialConfiguratorsInPrefabStage[i] != null)
				{
					materialConfiguratorsInPrefabStage[i].Init();
				}
			}
		}

		public void RefreshAndInit()
		{
			RefreshCoordinatorsReferences();
			InitMaterialConfigurators();
			InitTypwriters();
		}

		public void OnPrefabStageOpened(PrefabStage stage)
		{
			goPrefabRoot = stage.prefabContentsRoot;

			RefreshAndInit();
		}

		public void OnPrefabStageClosing(PrefabStage stage)
		{
			goPrefabRoot = null;

			RefreshAndInit();
		}

		public void Init()
		{
			typewritersInScene = new AllIn1TextTypewriter[0];
			typewritersInPrefabStage = new AllIn1TextTypewriter[0];

			materialConfiguratorsInPrefabStage = new TextMaterialConfiguratorMaster[0];
			materialConfiguratorsInScene = new TextMaterialConfiguratorMaster[0];
		}
	}
}