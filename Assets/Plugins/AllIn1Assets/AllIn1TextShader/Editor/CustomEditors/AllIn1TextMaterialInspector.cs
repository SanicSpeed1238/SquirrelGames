using AllIn1Assets.SharedExtensions;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CanEditMultipleObjects]
	public class AllIn1TextMaterialInspector : ShaderGUI
	{
		private AllIn1TextPropertiesConfigSO propertiesConfigCollection;

		private AllIn1TextPropertiesConfig currentPropertiesConfig;

		private Plugin3DMaterialInspectorReferences inspectorReferences;
		private AbstractEffectDrawer[] drawers;
		private GlobalPropertiesDrawer globalPropertiesDrawer;
		private FontPropertiesDrawer fontPropertiesDrawer;
		private AdvancedPropertiesDrawer advancedPropertiesDrawer;
		private MaterialProperty matPropertyRenderPreset;
		private MaterialProperty matPropertyBlendSrc;
		private MaterialProperty matPropertyBlendDst;
		private MaterialProperty matPropertyZWrite;
		private MaterialProperty matPropertyFontAtlas;
		private MaterialProperty matPropertyBlurredAtlas;

		private TextMaterialPreview textMaterialPreview;

		private int lastRenderQueue;
		private float lasTimeRebuilt;


		private bool isTextLayer;
		private Plugin3DConfig pluginConfig;
		
		private static readonly CommonStyles commonStyles = new CommonStyles();

		private void RefreshReferences(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			this.pluginConfig = PluginRegistry.GetPluginByID<Plugin3DConfig>(PluginID.ALLIN1_TEXT_SHADER);

			isTextLayer = materialEditor.targets.Length == 1 && materialEditor.target.name.StartsWith(AllIn1TextConstants.PREFIX_MAT_TEXT_LAYER);

			if (inspectorReferences == null)
			{
				inspectorReferences = new Plugin3DMaterialInspectorReferences(pluginConfig);
				inspectorReferences.Setup(materialEditor, properties);

				if (lastRenderQueue > 0)
				{
					for (int i = 0; i < inspectorReferences.targetMatInfos.Length; i++)
					{
						inspectorReferences.targetMatInfos[i].mat.renderQueue = lastRenderQueue;
					}
				}
			}

			if (propertiesConfigCollection == null || currentPropertiesConfig == null)
			{
				propertiesConfigCollection = (AllIn1TextPropertiesConfigSO)pluginConfig.propertiesConfigCollection;

				if (propertiesConfigCollection != null)
				{
					RefreshPropertiesConfig();
				}

				lasTimeRebuilt = (float)EditorApplication.timeSinceStartup;
			}

			//Nothing below can be built without a config. Everything is retried on the next repaint
			if (currentPropertiesConfig == null)
			{
				ResetReferences();
				return;
			}

			CreateDrawers();

			matPropertyRenderPreset = inspectorReferences.matProperties[currentPropertiesConfig.renderPreset];
			matPropertyBlendSrc		= inspectorReferences.matProperties[currentPropertiesConfig.blendSrcIdx];
			matPropertyBlendDst		= inspectorReferences.matProperties[currentPropertiesConfig.blendDstIdx];
			matPropertyZWrite		= inspectorReferences.matProperties[currentPropertiesConfig.zWriteIndex];
			matPropertyFontAtlas	= inspectorReferences.matProperties[currentPropertiesConfig.fontAtlasIdx]; 
			matPropertyBlurredAtlas	= inspectorReferences.matProperties[currentPropertiesConfig.blurredAtlasIdx];

			//We ensure that data is refreshed. Sometimes objects are not null but we need to refresh the references
			inspectorReferences.Setup(materialEditor, properties);

			RefreshMaterialPreviewInstance();

			RefreshDrawers();
		}

		private void ResetReferences()
		{
			this.propertiesConfigCollection = null;
			this.currentPropertiesConfig = null;

			inspectorReferences = null;
			drawers = null;

			globalPropertiesDrawer = null;
			advancedPropertiesDrawer = null;
		}

		private void CreateDrawers()
		{
			if (propertiesConfigCollection == null ||
				currentPropertiesConfig == null ||
				inspectorReferences == null ||
				drawers == null ||
				globalPropertiesDrawer == null ||
				advancedPropertiesDrawer == null)
			{
				drawers = new AbstractEffectDrawer[0];

				GeneralEffectDrawer generalEffectDrawer = new GeneralEffectDrawer(inspectorReferences, currentPropertiesConfig, pluginConfig);


				ColorRampEffectDrawer colorRampEffectDrawer = new ColorRampEffectDrawer(inspectorReferences, currentPropertiesConfig, pluginConfig);
				BevelDrawer bevelDrawer = new BevelDrawer(inspectorReferences, currentPropertiesConfig, pluginConfig);
				OutlineDrawer outlineDrawer = new OutlineDrawer(inspectorReferences, currentPropertiesConfig, pluginConfig);
				DropShadowDrawer dropShadowDrawer = new DropShadowDrawer(inspectorReferences, currentPropertiesConfig, pluginConfig);
				OuterGlowDrawer outerGlowDrawer = new OuterGlowDrawer(inspectorReferences, currentPropertiesConfig, pluginConfig);
				ExtrudeDrawer extrudeDrawer = new ExtrudeDrawer(inspectorReferences, currentPropertiesConfig, pluginConfig);

				drawers = new AbstractEffectDrawer[]
				{
					generalEffectDrawer,
					colorRampEffectDrawer,
					bevelDrawer,
					outlineDrawer,
					dropShadowDrawer,
					outerGlowDrawer,
					extrudeDrawer
				};

				advancedPropertiesDrawer = new AdvancedPropertiesDrawer(currentPropertiesConfig.advancedProperties, currentPropertiesConfig.blendSrcIdx, currentPropertiesConfig.blendDstIdx, inspectorReferences);
				fontPropertiesDrawer = new FontPropertiesDrawer(fontProperties: currentPropertiesConfig.fontProperties, 
					fontAtlasPropIdx: currentPropertiesConfig.fontAtlasIdx,  blurredAtlasPropIdx: currentPropertiesConfig.blurredAtlasIdx,
					references: inspectorReferences);
			}

			if (globalPropertiesDrawer == null)
			{
				globalPropertiesDrawer = new GlobalPropertiesDrawer();
			}
		}

		private void RefreshDrawers()
		{
			for (int i = 0; i < drawers.Length; i++)
			{
				drawers[i].Refresh(inspectorReferences);
			}
		}

		private AbstractEffectDrawer FindEffectDrawerByID(string drawerID)
		{
			AbstractEffectDrawer res = null;

			for (int i = 0; i < drawers.Length; i++)
			{
				if (drawers[i].ID == drawerID)
				{
					res = drawers[i];
					break;
				}
			}

			return res;
		}

		public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
		{
			base.AssignNewShaderToMaterial(material, oldShader, newShader);
		}

		private void RefreshPropertiesConfig()
		{
			Shader shader = inspectorReferences.GetShader();
			currentPropertiesConfig = (AllIn1TextPropertiesConfig)propertiesConfigCollection.FindPropertiesConfigByShader(shader);

			inspectorReferences.SetOutlineEffect(currentPropertiesConfig);
			inspectorReferences.SetCastShadowsEffect(currentPropertiesConfig);
		}

		private void RefreshMaterialPreviewInstance()
		{
			if (textMaterialPreview == null)
			{
				textMaterialPreview = new TextMaterialPreview();
			}
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			if (pluginConfig == null)
			{
				ResetReferences();
				lasTimeRebuilt = (float)EditorApplication.timeSinceStartup;
			}
			else if (lasTimeRebuilt <= EditorPrefs.GetFloat(pluginConfig.lastTimeShaderPropertiesRebuiltKey, float.MaxValue))
			{
				ResetReferences();
				lasTimeRebuilt = (float)EditorApplication.timeSinceStartup;
			}

			if (inspectorReferences != null && drawers != null)
			{
				inspectorReferences.Setup(materialEditor, properties);
			}

			RefreshReferences(materialEditor, properties);

			if (currentPropertiesConfig == null)
			{
				EditorGUILayout.HelpBox("The shader configuration is being rebuilt. Reselect the material if this message stays.", MessageType.Info);
				return;
			}

			commonStyles.InitStylesIfNeeded();

#if ALLIN1_URP
			bool urpCorrectlyConfigured = URPConfigurator.IsURPCorrectlyConfigured();
			if (!urpCorrectlyConfigured)
			{
				EditorGUILayout.LabelField(CommonMessages.URP_PIPELINE_NOT_ASSIGNED, commonStyles.warningLabel);
				EditorUtils.DrawButtonLink(pluginConfig.GetRenderPipelineDocURL());

				GUILayout.Space(50f);
			}

			EditorGUI.BeginDisabledGroup(!urpCorrectlyConfigured);
#endif

			
			EditorGUI.BeginChangeCheck();

			if (!isTextLayer)
			{
				DrawFontProperties();
				DrawAdvancedProperties();
			}

			DrawGlobalProperties();
			DrawEffects();

			EditorUIUtils.AddSpaces(1);

			if (EditorGUI.EndChangeCheck())
			{
				CheckBlurredAtlasReference();
				textMaterialPreview.NotifyMaterialChanged();
			}

#if ALLIN1_URP
			EditorGUI.EndDisabledGroup(); 
#endif
		}

		// Real text instead of Unity's sphere: the shader reads per-character buffers, so the default preview is meaningless
		public override void OnMaterialPreviewGUI(MaterialEditor materialEditor, Rect r, GUIStyle background)
		{
			RefreshMaterialPreviewInstance();

			if (!textMaterialPreview.TryDraw(materialEditor, r, background))
			{
				base.OnMaterialPreviewGUI(materialEditor, r, background);
			}
		}

		public override void OnMaterialInteractivePreviewGUI(MaterialEditor materialEditor, Rect r, GUIStyle background)
		{
			RefreshMaterialPreviewInstance();

			if (!textMaterialPreview.TryDraw(materialEditor, r, background))
			{
				base.OnMaterialInteractivePreviewGUI(materialEditor, r, background);
			}
		}

		public override void OnMaterialPreviewSettingsGUI(MaterialEditor materialEditor)
		{
			RefreshMaterialPreviewInstance();

			if (!textMaterialPreview.CanPreview(materialEditor))
			{
				base.OnMaterialPreviewSettingsGUI(materialEditor);
			}
		}

		private void DrawFontProperties()
		{
			if(fontPropertiesDrawer != null)
			{
				fontPropertiesDrawer.Draw();
			}
		}

		private void DrawAdvancedProperties()
		{
			//Security check
			if (advancedPropertiesDrawer != null)
			{
				advancedPropertiesDrawer.Draw();
			}
		}

		private void DrawGlobalProperties()
		{
			globalPropertiesDrawer.Draw(currentPropertiesConfig.singleProperties, inspectorReferences);
		}

		private void DrawEffects()
		{
			int globalEffectIndex = 0;
			for (int groupIdx = 0; groupIdx < currentPropertiesConfig.effectsGroups.Length; groupIdx++)
			{
				EffectGroup effectGroup = currentPropertiesConfig.effectsGroups[groupIdx];
				if (effectGroup.effects.Length <= 0) { continue; }

				EditorGUILayout.Separator();
				EditorUtils.DrawLine(Color.grey, 1, 3);
				GUILayout.Label(effectGroup.DisplayName, inspectorReferences.bigLabelStyle);

				for (int effectIdx = 0; effectIdx < effectGroup.effects.Length; effectIdx++)
				{
					AllIn13DEffectConfig effectConfig = effectGroup.effects[effectIdx];

					globalEffectIndex++;

					AbstractEffectDrawer drawer = FindEffectDrawerByID(effectConfig.effectDrawerID);
					drawer.Draw(currentPropertiesConfig, effectConfig, globalEffectIndex);
				}		
			}
		}

		private void CheckBlurredAtlasReference()
		{
			bool anyBlurredAtlasEffectEnabled = false;
			for(int i = 0; i < AllIn1TextConstants.EFFECTS_BLURRED_ATLAS.Length; i++)
			{
				AllIn13DEffectConfig effectConfig = currentPropertiesConfig.FindEffectConfigByID(AllIn1TextConstants.EFFECTS_BLURRED_ATLAS[i]);
				anyBlurredAtlasEffectEnabled = anyBlurredAtlasEffectEnabled || AllIn13DEffectConfig.IsEffectEnabled(effectConfig, inspectorReferences);
			}

			Texture2D fontAtlas = matPropertyFontAtlas.textureValue as Texture2D;
			if (anyBlurredAtlasEffectEnabled && fontAtlas != null)
			{
				AllIn1TextShaderConfig config = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);

				Texture blurredAtlasTex = AllIn1TextUtils.GetCurrentBlurredAtlas(config, fontAtlas, matPropertyBlurredAtlas.textureValue);
				if (matPropertyBlurredAtlas.textureValue != blurredAtlasTex)
				{
					matPropertyBlurredAtlas.textureValue = blurredAtlasTex;
				}
			}
		}

		private void ApplyMaterialPreset(Material targetMat, BlendingMode previousPresset, BlendingMode newPreset)
		{
			matPropertyBlendSrc.floatValue = (float)newPreset.blendSrc;
			matPropertyBlendDst.floatValue = (float)newPreset.blendDst;
			matPropertyZWrite.floatValue = newPreset.depthWrite ? 1.0f : 0.0f;


			lastRenderQueue = (int)newPreset.renderQueue;
			targetMat.renderQueue = lastRenderQueue;

			if (previousPresset != newPreset && previousPresset.defaultEnabledEffects != null)
			{
				for (int i = 0; i < previousPresset.defaultEnabledEffects.Length; i++)
				{
					string effectID = previousPresset.defaultEnabledEffects[i];
					AllIn13DEffectConfig effectConfig = currentPropertiesConfig.FindEffectConfigByID(effectID);

					for (int matIdx = 0; matIdx < inspectorReferences.targetMatInfos.Length; matIdx++)
					{
						AbstractMaterialInfo matInfo = inspectorReferences.targetMatInfos[matIdx];
						AllIn13DEffectConfig.DisableEffectToggle(effectConfig, inspectorReferences, matInfo);
					}
				}
			}

			if (newPreset.defaultEnabledEffects != null)
			{
				for (int matIdx = 0; matIdx < inspectorReferences.targetMatInfos.Length; matIdx++)
				{
					AbstractMaterialInfo matInfo = inspectorReferences.targetMatInfos[matIdx];

					if (newPreset.isTransparent)
					{
						matInfo.EnableKeyword(Constants.KEYWORD_ALLIN13D_SURFACE_TRANSPARENT);
					}
					else
					{
						matInfo.DisableKeyword(Constants.KEYWORD_ALLIN13D_SURFACE_TRANSPARENT);
					}

					for (int i = 0; i < newPreset.defaultEnabledEffects.Length; i++)
					{
						string effectID = newPreset.defaultEnabledEffects[i];
						AllIn13DEffectConfig effectConfig = currentPropertiesConfig.FindEffectConfigByID(effectID);
						AllIn13DEffectConfig.EnableEffectToggle(effectConfig, inspectorReferences, matInfo);
					}
				}
			}
		}
	}
}