using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[InitializeOnLoad]
	public static class ObjectChangesManager
	{
		private const string TAG = "ObjectChangesManager";

		static ObjectChangesManager()
		{
			ObjectChangeEvents.changesPublished += ChangesPublished;
		}

		static void ChangesPublished(ref ObjectChangeEventStream stream)
		{
			for (int i = 0; i < stream.length; ++i)
			{
				ObjectChangeKind changeKind = stream.GetEventType(i);

				switch (changeKind)
				{
					case ObjectChangeKind.CreateGameObjectHierarchy:
						CreateGameObjectHierarchyEventArgs args;
						stream.GetCreateGameObjectHierarchyEvent(i, out args);
						ProcessCreateGameObjectHierarchy(args);
						break;
				}
			}
		}

		private static void ProcessCreateGameObjectHierarchy(CreateGameObjectHierarchyEventArgs args)
		{
#if UNITY_6000_0_OR_NEWER
			GameObject go = EditorUtility.EntityIdToObject(args.instanceId) as GameObject;
#else
			GameObject go = EditorUtility.InstanceIDToObject(args.instanceId) as GameObject;
#endif

			AllIn1TextShaderComponent textShaderComponent = go.GetComponentInChildren<AllIn1TextShaderComponent>(true);
			AllIn1TextTypewriter typewriter = go.GetComponentInChildren<AllIn1TextTypewriter>(true);

			bool isPartOfPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(go);
			bool isPartOfAnyPrefab = PrefabUtility.IsPartOfAnyPrefab(go);
			if (textShaderComponent)
			{
				AllIn1TextUtils.NewMaterialInAllHierarchy(go);
				EditorTypewritersManager.RefreshAndInit();

				if (PrefabUtility.IsPartOfAnyPrefab(go))
				{
					PrefabUtility.RevertObjectOverride(go, InteractionMode.AutomatedAction);
					PrefabUtility.RevertObjectOverride(textShaderComponent, InteractionMode.AutomatedAction);
					if (typewriter != null)
					{
						PrefabUtility.RevertObjectOverride(typewriter, InteractionMode.AutomatedAction);
					}
				}
			}
		}
	}
}