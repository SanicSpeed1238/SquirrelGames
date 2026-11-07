using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(AllIn1TextTypewriter))]
	public class AllIn1TextTypewriterInspector : Editor
	{
		private const string TAG = "AllIn1TextTypewriterInspector";

		private const string STR_REVEAL_EFFECTS = "Reveal Effects";
		private const string STR_REVEAL_CONFIG_PREFIX = "RevealConfig";

		private const string STR_LOOPED_EFFECTS = "Looped Effects";
		private const string STR_LOOPED_CONFIG_PREFIX = "LoopedConfig";

		private const string STR_HIDE_EFFECTS = "Hide Effects";
		private const string STR_HIDE_CONFIG = "Hide Config";

		private const string STR_EVENTS = "Events";

		private static readonly GUIContent LABEL_REVEAL_STARTED = new GUIContent("On Reveal Started", "Reveal(), Show Immediately, Play() on a text that has not revealed yet, or a reveal timed by the spline's Show().");
		private static readonly GUIContent LABEL_REVEAL_COMPLETED = new GUIContent("On Reveal Completed", "Every letter finished its reveal effects. Not fired when Hide() interrupts the reveal.");
		private static readonly GUIContent LABEL_CHARACTER_REVEAL_STARTED = new GUIContent("On Character Reveal Started", "Each letter the moment its reveal starts, the right time for a typing sound. Passes the character and its index in the TMP text info (Per Word: once per word, with its first letter). Spaces have no letter, so they never fire it, and neither do Show Immediately or a reveal timed by the spline.");
		private static readonly GUIContent LABEL_HIDE_STARTED = new GUIContent("On Hide Started", "Hide(), HideImmediate(), or a hide timed by the spline's Hide().");
		private static readonly GUIContent LABEL_HIDE_COMPLETED = new GUIContent("On Hide Completed", "Every letter finished its hide effects. Not fired when a reveal interrupts the hide.");

		private AllIn1TextShaderConfig config;
		
		private EasingFunctionsSO easeFunctionLoopedCollection;
		private EasingFunctionsSO easeFunctionOnceCollection;

		private SerializedProperty spUserRevealAsHideConfig;
		private SerializedProperty spOnStart;
		private SerializedProperty spOnRevealStarted;
		private SerializedProperty spOnRevealCompleted;
		private SerializedProperty spOnCharacterRevealStarted;
		private SerializedProperty spOnHideStarted;
		private SerializedProperty spOnHideCompleted;

		private AllIn1TextTypewriter typewriter;

		private string[] easeFunctionLoopedNames;
		private string[] easeFunctionOnceNames;

		private RevealConfigDrawer revealConfigDrawer;

		private void OnEnable()
		{
			this.typewriter = (AllIn1TextTypewriter)target;
			this.config = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);

			CheckReferences();

			this.spUserRevealAsHideConfig = serializedObject.FindProperty("useRevealAsHideConfig");
			this.spOnStart = serializedObject.FindProperty("onStart");
			this.spOnRevealStarted = serializedObject.FindProperty("onRevealStartedEvent");
			this.spOnRevealCompleted = serializedObject.FindProperty("onRevealCompletedEvent");
			this.spOnCharacterRevealStarted = serializedObject.FindProperty("onCharacterRevealStartedEvent");
			this.spOnHideStarted = serializedObject.FindProperty("onHideStartedEvent");
			this.spOnHideCompleted = serializedObject.FindProperty("onHideCompletedEvent");

			easeFunctionLoopedCollection = AssetDatabase.LoadAssetAtPath<EasingFunctionsSO>(config.EaseFunctionCollectionLoopedPath);
			easeFunctionLoopedNames = easeFunctionLoopedCollection.GetEaseFunctionNamesArray();

			easeFunctionOnceCollection = AssetDatabase.LoadAssetAtPath<EasingFunctionsSO>(config.EaseFunctionCollectionOncePath);
			easeFunctionOnceNames = easeFunctionOnceCollection.GetEaseFunctionNamesArray();
		}

		private void CheckReferences()
		{
			AllIn1TextShaderConfig pluginConfig = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);


			bool referencesChanged = typewriter.RefreshReferencesIfNeeded
			(
				configCollectionReveal: pluginConfig.revealConfigCollection,
				configCollectionLoop: pluginConfig.loopedConfigCollection,
				configCollectionHide: pluginConfig.hideConfigCollection,
				effectCollectionDataPath: pluginConfig.LoopedEffectDatabasePath,
				easeFunctionCollectionLoopedPath: pluginConfig.EaseFunctionCollectionLoopedPath
			);

			if (!EditorTypewritersManager.IsSceneTypewriterAlreadyRegistered(typewriter))
			{
				EditorTypewritersManager.RegisterTypewriterInScene(typewriter);
			}

			if(revealConfigDrawer == null)
			{
				revealConfigDrawer = new RevealConfigDrawer();
			}

			if (referencesChanged)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawersSharedConfig.Setup();

			EditorGUILayout.PropertyField(spOnStart, new GUIContent("On Start", "What the typewriter does when the scene starts. Looped effects only run while it plays"));
			EditorUIUtils.AddSpaces(2);

			bool isInPreviewMode = typewriter.IsInPreviewMode();
			bool changesInRevealConfig = false;
			typewriter.revealConfigUnfolded = GUILayout.Toggle(typewriter.revealConfigUnfolded, STR_REVEAL_EFFECTS, GUI.skin.button);
			if (typewriter.revealConfigUnfolded)
			{
				typewriter.revealConfig = DrawMeshEffectGroupConfig
				(
					changes:				ref changesInRevealConfig,
					groupName:				STR_REVEAL_EFFECTS,
					folderPath:				config.RevealConfigsFolderPath,
					assetPrefix:			STR_REVEAL_CONFIG_PREFIX,
					configCollection:		config.revealConfigCollection,
					effectCollection:		config.revealEffectDatabase,
					meshEffectGroupConfig:	typewriter.revealConfig,
					easeFunctionNames:		easeFunctionOnceNames,
					easeFunctionCollection: easeFunctionOnceCollection
				);
			}

			bool changesInLoopedConfig = false;
			typewriter.loopedConfigUnfolded = GUILayout.Toggle(typewriter.loopedConfigUnfolded, STR_LOOPED_EFFECTS, GUI.skin.button);
			if (typewriter.loopedConfigUnfolded)
			{
				typewriter.loopedConfig = DrawMeshEffectGroupConfig
				(
					changes:				ref changesInLoopedConfig,
					groupName:				STR_LOOPED_EFFECTS,
					folderPath:				config.LoopedConfigsFolderPath,
					assetPrefix:			STR_LOOPED_CONFIG_PREFIX,
					configCollection:		config.loopedConfigCollection,
					effectCollection:		config.loopedEffectDatabase,
					meshEffectGroupConfig:	typewriter.loopedConfig,
					easeFunctionNames:		easeFunctionLoopedNames,
					easeFunctionCollection: easeFunctionLoopedCollection
				);
			}

			bool changesInHideConfig = false;
			typewriter.hideConfigUnfolded = GUILayout.Toggle(typewriter.hideConfigUnfolded, STR_HIDE_EFFECTS, GUI.skin.button);
			if (typewriter.hideConfigUnfolded)
			{
				EditorGUILayout.PropertyField(spUserRevealAsHideConfig);

				if (!spUserRevealAsHideConfig.boolValue)
				{
					DrawHideConfig(ref changesInHideConfig);
				}
			}

			typewriter.eventsUnfolded = GUILayout.Toggle(typewriter.eventsUnfolded, STR_EVENTS, GUI.skin.button);
			if (typewriter.eventsUnfolded)
			{
				DrawEvents();
			}

			bool thereAreChanges = changesInRevealConfig || changesInLoopedConfig || changesInHideConfig;
			DrawPreviewSection(thereAreChanges);

			ProcessChanges(changesInRevealConfig, changesInLoopedConfig, changesInHideConfig);

			serializedObject.ApplyModifiedProperties();
		}

		private void ProcessChanges(bool changesInRevealConfig, bool changesInLoopedConfig, bool changesInHideConfig)
		{
			if (changesInRevealConfig || changesInHideConfig || changesInLoopedConfig)
			{
				MeshEffectGroupConfig hideConfigSelected = typewriter.GetHideConfigSelectedFomEditor();

				if (changesInRevealConfig)
				{
					EditorUtility.SetDirty(typewriter.revealConfig);

					if (hideConfigSelected != null)
					{
						EditorUtility.SetDirty(hideConfigSelected);
					}
				}

				if (changesInLoopedConfig)
				{
					if(typewriter.loopedConfig != null)
					{
						EditorUtility.SetDirty(typewriter.loopedConfig);
					}
				}

				if (changesInHideConfig)
				{
					EditorUtility.SetDirty(hideConfigSelected);
				}

				EditorUtils.SetDirtyCurrentScene();
			}
		}

		private MeshEffectGroupConfig DrawMeshEffectGroupConfig(ref bool changes, 
			string groupName, string folderPath, string assetPrefix,
			MeshEffectConfigCollection configCollection, MeshEffectCollection effectCollection, MeshEffectGroupConfig meshEffectGroupConfig,
			string[] easeFunctionNames, EasingFunctionsSO easeFunctionCollection)
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);


			MeshEffectGroupConfig res = revealConfigDrawer.Draw
			(
				typewriter:				this.typewriter,
				configCollection:		configCollection,
				meshEffectCollection:	effectCollection,
				config:					meshEffectGroupConfig,
				configsFolderPath:		folderPath,
				assetPrefix:			assetPrefix,
				easeFunctionNames:		easeFunctionNames,
				easeFunctionCollection: easeFunctionCollection,
				commonStyles:			DrawersSharedConfig.s_commonStyles,
				changes:				ref changes
			);

			EditorGUILayout.EndVertical();

			return res;
		}

		private void DrawHideConfig(ref bool changes)
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			MeshEffectGroupConfig hideConfigSelected = typewriter.GetHideConfigSelectedFomEditor();

			MeshEffectGroupConfig meshEffectGroupConfig = hideConfigSelected;
			meshEffectGroupConfig = revealConfigDrawer.Draw
			(
				typewriter:				typewriter, 
				configCollection:		config.hideConfigCollection,
				meshEffectCollection:	config.revealEffectDatabase, 
				config:					meshEffectGroupConfig,
				configsFolderPath:		config.HideConfigsFolderPath, 
				assetPrefix:			STR_HIDE_CONFIG,
				easeFunctionNames:		easeFunctionOnceNames, 
				easeFunctionCollection:	easeFunctionOnceCollection,
				commonStyles:			DrawersSharedConfig.s_commonStyles, 
				changes:				ref changes
			);

			typewriter.SetHideConfigSelectedFromEditor(meshEffectGroupConfig);

			EditorGUILayout.EndVertical();
		}

		private void DrawEvents()
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			EditorGUILayout.PropertyField(spOnRevealStarted, LABEL_REVEAL_STARTED);
			EditorGUILayout.PropertyField(spOnCharacterRevealStarted, LABEL_CHARACTER_REVEAL_STARTED);
			EditorGUILayout.PropertyField(spOnRevealCompleted, LABEL_REVEAL_COMPLETED);
			EditorGUILayout.PropertyField(spOnHideStarted, LABEL_HIDE_STARTED);
			EditorGUILayout.PropertyField(spOnHideCompleted, LABEL_HIDE_COMPLETED);

			EditorGUILayout.EndVertical();
		}

		private void DrawPreviewSection(bool changes)
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			EditorGUILayout.LabelField("Preview", DrawersSharedConfig.s_commonStyles.bigLabel);

			EditorGUI.BeginChangeCheck();
			typewriter.previewMode = (PreviewMode)EditorGUILayout.EnumPopup("Preview mode", typewriter.previewMode);
			bool previewModeChanged = EditorGUI.EndChangeCheck();
			if (previewModeChanged || changes)
			{
				typewriter.PreviewModeChanged();
				EditorUtility.SetDirty(target);
			}

			bool isInPreviewMode = typewriter.IsInPreviewMode();
			bool initCoordinatorNeeded = previewModeChanged || changes;
			if (isInPreviewMode)
			{
				bool setDirtyNeeded = changes;

				if (typewriter.previewMode != PreviewMode.PREVIEW_LOOPED)
				{
					EditorGUI.BeginChangeCheck();
					typewriter.previewSliderValue = EditorGUILayout.Slider("Preview", typewriter.previewSliderValue, 0f, 1f);
					setDirtyNeeded = EditorGUI.EndChangeCheck() || changes;
				}

				if (setDirtyNeeded)
				{
					EditorUtility.SetDirty(target);
				}
			}

			if (initCoordinatorNeeded)
			{
				typewriter.Init();
				EditorUtils.ExecuteActionAfterInspectorUpdate(typewriter.Update);
			}

			EditorGUILayout.EndVertical();
		}
	}
}