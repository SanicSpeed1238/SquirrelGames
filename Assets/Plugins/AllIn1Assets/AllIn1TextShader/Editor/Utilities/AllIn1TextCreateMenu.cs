using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public static class AllIn1TextCreateMenu
	{
		private const string TAG = "AllIn1TextCreateMenu";
		private const string TMP_TEXT_MENU_PATH = "GameObject/UI/Text - TextMeshPro";
		private const string TMP_CREATE_MENU_TYPE = "TMPro.EditorUtilities.TMPro_CreateObjectMenu";
		private const string TMP_CREATE_TEXT_METHOD = "CreateTextMeshProGuiObjectPerform";
		private const string OBJECT_NAME = "All In 1 Text";

		[MenuItem("GameObject/UI/All In 1 Text", false, 2002)]
		private static void CreateAllIn1Text(MenuCommand menuCommand)
		{
			int undoGroup = Undo.GetCurrentGroup();

			if (!CreateTMPText(menuCommand)) return;

			GameObject textGO = Selection.activeGameObject;
			if (textGO == null || !textGO.TryGetComponent(out TMPro.TMP_Text _)) return;

			textGO.name = GameObjectUtility.GetUniqueNameForSibling(textGO.transform.parent, OBJECT_NAME);
			Undo.AddComponent<AllIn1TextShaderComponent>(textGO);
			Undo.SetCurrentGroupName("Create All In 1 Text");
			Undo.CollapseUndoOperations(undoGroup);
		}

		//TMP places the text from menuCommand.context, which ExecuteMenuItem drops, so its handler is called directly when it can be found
		private static bool CreateTMPText(MenuCommand menuCommand)
		{
			MethodInfo createMethod = typeof(TMPro.EditorUtilities.TMP_EditorPanelUI).Assembly
				.GetType(TMP_CREATE_MENU_TYPE)?
				.GetMethod(TMP_CREATE_TEXT_METHOD, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(MenuCommand) }, null);

			if (createMethod != null)
			{
				createMethod.Invoke(null, new object[] { menuCommand });
				return true;
			}

			if (menuCommand.context is GameObject parent)
			{
				Selection.activeGameObject = parent;
			}

			if (EditorApplication.ExecuteMenuItem(TMP_TEXT_MENU_PATH)) return true;

			AllIn1Logger.LogError(TAG, $"Could not run the '{TMP_TEXT_MENU_PATH}' menu item, is TextMeshPro installed?");
			return false;
		}
	}
}
