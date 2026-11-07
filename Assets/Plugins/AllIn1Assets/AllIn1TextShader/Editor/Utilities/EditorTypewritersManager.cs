using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AllIn1Assets.AllIn1TextShader
{
	public static class EditorTypewritersManager
	{
		private const string TAG = "EditorTypewritersManager";

		private const float MAX_EDITOR_DELTA_TIME = 0.0666f; //Prevents artificially large delta time when assembly is reloaded and other long-running operations

		private static readonly EditorTypewritersManagerValues values = new EditorTypewritersManagerValues();

		public static void ResetDeltaTime()
		{
			values.editorDeltaTime = 0f;
			values.lastTime = Time.realtimeSinceStartup;
		}

		public static void EditorUpdate()
		{
			if (Application.isPlaying) { return; }

			values.editorDeltaTime = Time.realtimeSinceStartup - values.lastTime;
			values.editorDeltaTime = Mathf.Clamp(values.editorDeltaTime, 0f, MAX_EDITOR_DELTA_TIME);

			values.UpdateTypewritersInScene();

			values.UpdateMaterialConfiguratorsInScene();
			
			if (values.IsInPrefabStage())
			{
				values.UpdateTypewritersInPrefabStage();
				values.UpdateMaterialConfiguratorsInPrefabStage();
			}

			values.lastTime = Time.realtimeSinceStartup;
			values.firstUpdateAfterProjectReload = false;
		}

		public static bool IsSceneTypewriterAlreadyRegistered(AllIn1TextTypewriter typewriter)
		{
			bool res = false;
			if (typewriter != null && values.typewritersInScene != null)
			{
				res = ArrayUtility.Contains(values.typewritersInScene, typewriter);
			}
			
			return res;
		}

		public static bool IsSceneMaterialConfiguratorAlreadyRegistered(TextMaterialConfiguratorMaster materialConfigurator)
		{
			bool res = false;

			if(materialConfigurator != null && values.materialConfiguratorsInScene != null)
			{
				res = ArrayUtility.Contains(values.materialConfiguratorsInScene, materialConfigurator);
			}

			return res;
		}

		public static void RegisterTypewriterInScene(AllIn1TextTypewriter typewriter)
		{
			values.AddTypewriterInScene(typewriter);
		}

		public static void RegisterMaterialConfiguratorInScene(TextMaterialConfiguratorMaster materialConfigurator)
		{
			values.AddMaterialConfiguratorInScene(materialConfigurator);
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
		{
			if (playModeStateChange != PlayModeStateChange.ExitingEditMode && playModeStateChange != PlayModeStateChange.EnteredPlayMode)
			{
				RefreshAndInit(); 
			}
		}

		private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
		{
			RefreshAndInit();
		}

		private static void OnBeforeAssemblyReload()
		{
			ResetDeltaTime();
			ReleaseBuffers();
		}

		private static void OnAfterAssemblyReload()
		{
			RefreshAndInit();
			ResetDeltaTime(); 
		}

		private static void OnSceneClosing(Scene scene, bool removingScene)
		{
			values.ReleaseBuffers();
		}

		private static void ReleaseBuffers()
		{
			values.ReleaseBuffers();
		}

		public static void RefreshAndInit()
		{
			values.RefreshAndInit();
		}

		private static void OnPrefabStageOpened(PrefabStage stage)
		{
			values.OnPrefabStageOpened(stage);
		}

		private static void OnPrefabStageClosing(PrefabStage stage)
		{
			values.OnPrefabStageClosing(stage);
		}


		[DidReloadScripts()]
		private static void Init()
		{
			AllIn1Logger.Log(TAG, "Init()");

			values.Init();
			
			EditorApplication.update		-= EditorUpdate;
			PrefabStage.prefabStageOpened	-= OnPrefabStageOpened;
			PrefabStage.prefabStageClosing	-= OnPrefabStageClosing;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; 
			EditorSceneManager.sceneClosing -= OnSceneClosing;
			EditorSceneManager.sceneOpened -= OnSceneOpened;
			AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

			EditorApplication.update		+= EditorUpdate;
			PrefabStage.prefabStageOpened	+= OnPrefabStageOpened;
			PrefabStage.prefabStageClosing	+= OnPrefabStageClosing;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			EditorSceneManager.sceneClosing += OnSceneClosing;
			EditorSceneManager.sceneOpened += OnSceneOpened;
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

			RefreshAndInit();

			EditorApplication.delayCall += RefreshAndInit;

			values.firstUpdateAfterProjectReload = true;
			values.lastTime = 0f;
		}
	}
}