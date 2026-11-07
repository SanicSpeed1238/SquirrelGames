using AllIn1Assets.SharedExtensions;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(AllIn1TextShaderComponent))]
	[CanEditMultipleObjects]
	public class AllIn1TextShaderComponentCustomEditor : PluginComponentCustomEditor
	{
		private const string TAG = "AllIn1TextShaderComponentCustomEditor";

		private AllIn1TextShaderConfig config;
		private AllIn1TextShaderComponent textShaderComponent;
		private readonly List<string> missingTextReasons = new List<string>();

		protected override PluginID GetPluginID()
		{
			return PluginID.ALLIN1_TEXT_SHADER;
		}

		protected override void DisplayNotValidComponentsDialog()
		{
			// Empty when every one was a cancelled Text conversion, whose dialog already said the component goes
			if (missingTextReasons.Count == 0) { return; }

			EditorUtility.DisplayDialog("Missing TextMeshPro text", "AllIn1TextShaderComponent needs a TextMeshPro text (TextMeshPro or TextMeshPro - Text (UI)) and was removed from:\n\n" + string.Join("\n", missingTextReasons), "Ok");
		}

		// Read before the base removes the components, while their objects still say why they have no text
		private void CollectMissingTextReasons()
		{
			missingTextReasons.Clear();
			for (int i = 0; i < targets.Length; i++)
			{
				AllIn1TextShaderComponent component = (AllIn1TextShaderComponent)targets[i];
				if (component == null || component.TryGetComponent(out TMP_Text _)) { continue; }

				string reason = TextComponentSetup.GetMissingTextReason(component.gameObject);
				if (reason != null) { missingTextReasons.Add($"{component.name}: {reason}"); }
			}
		}

		private bool AllTargetsHaveText()
		{
			bool res = true;
			for (int i = 0; i < targets.Length && res; i++)
			{
				Component component = targets[i] as Component;
				res = component != null && component.TryGetComponent(out TMP_Text _);
			}
			return res;
		}

		// Only reached when the base could not remove a text-less component: inside a prefab, or before the configuration loaded
		public override void OnInspectorGUI()
		{
			if (!AllTargetsHaveText())
			{
				string reasons = missingTextReasons.Count > 0 ? "\n\n" + string.Join("\n", missingTextReasons) : string.Empty;
				EditorGUILayout.HelpBox(Messages.STR_WARNING_NO_TEXT_COMPONENT + reasons, MessageType.Warning);
				return;
			}

			base.OnInspectorGUI();
		}

		private void RefreshSerializedProperties()
		{
		
		}

		protected override void OnEnableImpl()
		{
			CollectMissingTextReasons();

			base.OnEnableImpl();

			// The base destroys a component left without a text, and this editor's target with it
			if (target == null) { return; }

			config = (AllIn1TextShaderConfig)pluginConfigGeneric;
			textShaderComponent = (AllIn1TextShaderComponent)target;

			if (!AllTargetsHaveText()) { return; }

			textShaderComponent.OnEnableImpl();

			Material currentMaterial = textShaderComponent.GetCurrentMaterial();
			bool isCurrentMaterialSubasset = AssetDatabase.IsSubAsset(currentMaterial);
			string materialPath = AssetDatabase.GetAssetPath(currentMaterial);
			AllIn1Logger.Log(TAG, $"Is current material subasset: {isCurrentMaterialSubasset} || Material Path: {materialPath}");
			
			RefreshSerializedProperties();
		}

		//Each effect's own toggle and keyword off; a ring or sub toggle stays as set, inert while its effect is off
		protected override void DeactivateAllEffects(Material mat)
		{
			AllIn1TextShaderConfig textConfig = (AllIn1TextShaderConfig)pluginConfigGeneric;
			if (!textConfig.IsMaterialFromThisPlugin(mat)) { return; }

			textConfig.EnsureCollections();
			if (textConfig.propertiesConfigCollection == null) { return; }

			//A baked variant has its keywords compiled in, there is nothing left to turn off
			AbstractMaterialInfo matInfo = AbstractMaterialInfo.CreateInstance(mat, textConfig);
			if (matInfo.IsShaderVariant()) { return; }

			PropertiesConfig propertiesConfig = textConfig.propertiesConfigCollection.FindPropertiesConfigByShader(mat.shader);
			if (propertiesConfig == null) { return; }

			List<AllIn13DEffectConfig> effects = propertiesConfig.GetAllEffects();
			for (int i = 0; i < effects.Count; i++)
			{
				AllIn13DEffectConfig effectConfig = effects[i];

				AllIn13DEffectConfig.DisableEffect(effectConfig, matInfo);

				if (effectConfig.keywords.Count > 1)
				{
					matInfo.EnableKeyword(effectConfig.keywords[0].keyword);
				}

				if (mat.HasProperty(effectConfig.keywordPropertyName))
				{
					mat.SetFloat(effectConfig.keywordPropertyName, 0f);
				}
			}
		}

		protected override void PlayModeStateChanged(PlayModeStateChange playModeStateChange)
		{
			base.PlayModeStateChanged(playModeStateChange);

			if(playModeStateChange == PlayModeStateChange.ExitingPlayMode)
			{

			}
		}

		protected override void DrawExtraButtons()
		{
			
		}

		protected override void DrawExtraSections()
		{
			DrawSectionSeparator();

			SyncAddOnReferences();

			// The add and remove buttons act on one text at a time
			EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
			DrawAddOnButtons();
			EditorGUI.EndDisabledGroup();

			DrawCheckAtlasSection();
		}

		// Picks up a spline, warp or text layered added from the Add Component menu or removed some other way, so the buttons below always match the object
		private void SyncAddOnReferences()
		{
			AllIn1TextSpline foundSpline = textShaderComponent.GetComponent<AllIn1TextSpline>();
			AllIn1TextWarp foundWarp = textShaderComponent.GetComponent<AllIn1TextWarp>();
			TextLayeredComponent foundLayered = textShaderComponent.GetComponent<TextLayeredComponent>();
			if (textShaderComponent.spline != foundSpline || textShaderComponent.warp != foundWarp || textShaderComponent.textLayered != foundLayered)
			{
				textShaderComponent.spline = foundSpline;
				textShaderComponent.warp = foundWarp;
				textShaderComponent.textLayered = foundLayered;
				EditorUtility.SetDirty(textShaderComponent);
			}
		}

		private void DrawAddOnButtons()
		{
			AllIn1TextTypewriter typewriter = textShaderComponent.GetTypwriter();
			TextLayeredComponent textLayered = textShaderComponent.GetTextLayered();
			AllIn1TextSpline spline = textShaderComponent.GetSpline();
			AllIn1TextWarp warp = textShaderComponent.GetWarp();

			if (typewriter == null)
			{
				if(GUILayout.Button("Add Typewriter"))
				{
					AddTypewriter();
				}
			}
			else
			{
				if (GUILayout.Button("Remove Typewriter"))
				{
					DestroyImmediate(typewriter);
					EditorUtility.SetDirty(target);

					textShaderComponent.materialConfigurator.SetPendingBufferRefreshing(true);
				}
			}


			if(textLayered == null)
			{
				// Layers draw through the UI canvas, which a 3D TextMeshPro text is not under
				bool isUIText = textShaderComponent.textComponent is TextMeshProUGUI;
				using (new EditorGUI.DisabledScope(!isUIText))
				{
					GUIContent addTextLayeredContent = new GUIContent("Add Text Layered", isUIText ? string.Empty : "Text layers need a TextMeshPro - Text (UI) text");
					if (GUILayout.Button(addTextLayeredContent))
					{
						AddTextLayered();
					}
				}
			}
			else
			{
				if (GUILayout.Button("Remove Text Layered"))
				{
					RemoveLayerObjects(textLayered);
					DestroyImmediate(textLayered);
					EditorUtility.SetDirty(target);

					textShaderComponent.materialConfigurator.SetPendingBufferRefreshing(true);
				}
			}

			if (spline == null)
			{
				if (GUILayout.Button("Add Spline"))
				{
					AddSpline();
				}
			}
			else
			{
				if (GUILayout.Button("Remove Spline"))
				{
					DestroyImmediate(spline);
					textShaderComponent.spline = null;
					EditorUtility.SetDirty(target);
				}
			}

			if (warp == null)
			{
				if (GUILayout.Button("Add Warp"))
				{
					AddWarp();
				}
			}
			else
			{
				if (GUILayout.Button("Remove Warp") && ConfirmRemoveWarp(warp))
				{
					DestroyImmediate(warp);
					textShaderComponent.warp = null;
					EditorUtility.SetDirty(target);
				}
			}
		}

		// Removing a component is not undoable here (undo of a component destroy proved unreliable on prefab instances), so a hand shaped cage asks first
		private static bool ConfirmRemoveWarp(AllIn1TextWarp warp)
		{
			bool res = true;
			if (warp.Style == TextWarpStyle.CUSTOM && !warp.IsCageFlat())
			{
				res = EditorUtility.DisplayDialog("Remove Warp", "This warp has a hand shaped Custom cage. Removing the component loses it, and this cannot be undone.", "Remove", "Cancel");
			}
			return res;
		}

		private void DrawCheckAtlasSection()
		{
			TMP_FontAsset currentFontAsset = textShaderComponent.GetFontAsset();

			DrawersSharedConfig.Setup();
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			bool isFontAssetOptimallyConfigured = AllIn1TextUtils.IsFontAssetOptimallyConfigured(currentFontAsset);
			string messageStr = isFontAssetOptimallyConfigured ? Messages.STR_FONT_ASSET_CORRECTLY_CONFIGURED : Messages.STR_WARNING_FONT_ASSET_NOT_CORRECTLY_CONFIGURED;
			MessageType messageType = isFontAssetOptimallyConfigured ? MessageType.Info : MessageType.Warning;
			
			EditorGUILayout.HelpBox(messageStr, messageType);
			if (currentFontAsset.atlasPopulationMode != AtlasPopulationMode.Static && UsesBlurredAtlas(textShaderComponent.GetCurrentMaterial()))
			{
				EditorGUILayout.HelpBox(Messages.STR_WARNING_DYNAMIC_FONT_BLURRED_ATLAS, MessageType.Warning);
			}
			int otherAssetCharacters = CountOtherAssetCharacters(textShaderComponent.textComponent);
			if (otherAssetCharacters > 0)
			{
				EditorGUILayout.HelpBox(string.Format(Messages.STR_WARNING_OTHER_FONT_CHARACTERS, otherAssetCharacters), MessageType.Warning);
			}

			if (!isFontAssetOptimallyConfigured)
			{
				if (GUILayout.Button("Create Font Atlas"))
				{
					CreateFontAtlas();
				}
			}

			EditorGUILayout.EndVertical();
		}

		private void CreateFontAtlas()
		{
			TMP_FontAsset newFontAsset = AllIn1TextUtils.CreateFontAsset(textShaderComponent.textComponent);
			if (newFontAsset == null) { return; }

			Material oldMaterial = textShaderComponent.GetCurrentMaterial();
			textShaderComponent.textComponent.font = newFontAsset;

			textShaderComponent.NewCleanMaterial(pluginConfigGeneric.GetDefaultMaterial());

			Material currMaterial = textShaderComponent.GetCurrentMaterial();
			TextMaterialConfiguratorMaster.CopyMatchingPropertiesKeepingLightAngle(currMaterial, oldMaterial);

			string blurredAtlasName = config.GetBlurredAtlasName(newFontAsset.atlasTexture.name);
			Texture blurredAtlasTex = AllIn1TextUtils.CreateBlurredAtlas(config.FontTexturesFolderPath, newFontAsset.atlasTexture.name, blurredAtlasName, newFontAsset.atlasTexture);

			textShaderComponent.SetCurrentMaterial(currMaterial);

			textShaderComponent.materialConfigurator.SetFontAtlas(newFontAsset.atlasTexture);
			textShaderComponent.materialConfigurator.SetBlurredAtlas(blurredAtlasTex);

			textShaderComponent.materialConfigurator.RefreshBuffers();
		}

		// TMP draws them in its sub meshes with their own material, which the configurator, the reveal groups and the loops all skip
		private static int CountOtherAssetCharacters(TMP_Text text)
		{
			int res = 0;
			TMP_TextInfo info = text != null ? text.textInfo : null;
			if (info == null || info.characterInfo == null) { return res; }

			int count = Mathf.Min(info.characterCount, info.characterInfo.Length);
			for (int i = 0; i < count; i++)
			{
				if (info.characterInfo[i].isVisible && info.characterInfo[i].materialReferenceIndex != 0) { res++; }
			}

			return res;
		}

		private static bool UsesBlurredAtlas(Material material)
		{
			bool res = material != null
				&& (material.IsKeywordEnabled("_BEVEL_ON") || material.IsKeywordEnabled("_OUTLINE_ON") || material.IsKeywordEnabled("_DROP_SHADOW_ON"));
			return res;
		}

		private void AddTypewriter()
		{
			AllIn1TextTypewriter newTypewriter = textShaderComponent.gameObject.AddComponent<AllIn1TextTypewriter>();

			newTypewriter.RefreshReferencesIfNeeded
			(
				configCollectionReveal: config.revealConfigCollection,
				configCollectionLoop: config.loopedConfigCollection, 
				configCollectionHide: config.hideConfigCollection, 
				effectCollectionDataPath: config.LoopedEffectDatabasePath,
				easeFunctionCollectionLoopedPath: config.EaseFunctionCollectionLoopedPath
			);

			textShaderComponent.typewriter = newTypewriter;
			
			EditorUtility.SetDirty(target);
		}

		private void AddTextLayered()
		{
			TextLayeredComponent textLayered = textShaderComponent.gameObject.AddComponent<TextLayeredComponent>();

			textShaderComponent.textLayered = textLayered;

		}

		// The layer objects are hidden, so they would stay behind and keep drawing
		private static void RemoveLayerObjects(TextLayeredComponent textLayered)
		{
			for (int i = textLayered.slaveLayers.Count - 1; i >= 0; i--)
			{
				textLayered.RemoveLayer(i);
			}
		}

		// Reuses a spline added from the Add Component menu: it is a DisallowMultipleComponent, so AddComponent would return null
		private void AddSpline()
		{
			AllIn1TextSpline newSpline = textShaderComponent.GetComponent<AllIn1TextSpline>();
			if (newSpline == null)
			{
				newSpline = textShaderComponent.gameObject.AddComponent<AllIn1TextSpline>();
				newSpline.SeedDefaultPoints();
			}
			newSpline.RefreshConfigReferencesIfNeeded(config.revealConfigCollection, config.hideConfigCollection);

			textShaderComponent.spline = newSpline;

			EditorUtility.SetDirty(newSpline);
			EditorUtility.SetDirty(target);
		}

		// Reuses a warp added from the Add Component menu, which the reference does not know about yet
		private void AddWarp()
		{
			AllIn1TextWarp newWarp = textShaderComponent.GetComponent<AllIn1TextWarp>();
			if (newWarp == null)
			{
				newWarp = textShaderComponent.gameObject.AddComponent<AllIn1TextWarp>();
			}

			textShaderComponent.warp = newWarp;

			EditorUtility.SetDirty(newWarp);
			EditorUtility.SetDirty(target);
		}
	}
}