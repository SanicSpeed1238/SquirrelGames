using System.Collections.Generic;
using TMPro;

namespace AllIn1Assets.AllIn1TextShader
{
	public static class TextMeshHandlerRegistry
	{
		private static readonly Dictionary<int, TextMeshHandler> registry;

		static TextMeshHandlerRegistry()
		{
			registry = new Dictionary<int, TextMeshHandler>();
		}

		public static void AddEntry(int instanceID, TextMeshHandler textMeshHandler)
		{
			if (!registry.ContainsKey(instanceID))
			{
				registry.Add(instanceID, textMeshHandler);
			}
		}

		public static TextMeshHandler GetTextMeshHandler(int instanceID)
		{
			TextMeshHandler res = null;

			if (registry.ContainsKey(instanceID))
			{
				res = registry[instanceID];
			}

			return res;
		}

		public static TextMeshHandler CreateTextMeshHandler(int instanceID, TMP_Text tmpText)
		{
			TextMeshHandler res = new TextMeshHandler(tmpText);

			AddEntry(instanceID, res);

			return res;
		}

		// With domain and scene reload disabled the registry outlives play mode while its TMP_Text references do not
		public static TextMeshHandler GetOrCreateTextMeshHandler(int instanceID, TMP_Text tmpText)
		{
			TextMeshHandler res = GetTextMeshHandler(instanceID);

			if (res == null)
			{
				res = CreateTextMeshHandler(instanceID, tmpText);
			}
			else if (res.tmpText == null)
			{
				res.tmpText = tmpText;
				res.ClearLayers();
				res.reapplyEffects = null;
				res.layoutStage = null;
				res.outputModifier = null;
			}

			return res;
		}

#if ALLIN1_DEVELOP && UNITY_EDITOR
		[UnityEditor.MenuItem("Tools/Debug/Num TextMeshHandlers")]
		private static void DebugNumTextMeshHandlers()
		{
			AllIn1Logger.LogWarning($"Num entries in dict: {registry.Count}"); 
		}
#endif
	}
}