using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// Draws a serialized property with the material inspectors' "R" button next to it
	public static class ResettablePropertyDrawer
	{
		private const float RESET_BUTTON_WIDTH = 20f;
		private static readonly GUIContent RESET_LABEL = new GUIContent("R", "Resets to default value");

		public static bool Draw(SerializedProperty property, float defaultValue)
		{
			return Draw(property, () => property.floatValue = defaultValue);
		}

		public static bool Draw(SerializedProperty property, int defaultValue)
		{
			return Draw(property, () => property.intValue = defaultValue);
		}

		public static bool Draw(SerializedProperty property, bool defaultValue)
		{
			return Draw(property, () => property.boolValue = defaultValue);
		}

		public static bool DrawEnum(SerializedProperty property, int defaultIndex)
		{
			return Draw(property, () => property.enumValueIndex = defaultIndex);
		}

		// Returns true when the field was edited or reset. A reset also raises GUI.changed so enclosing change checks see it
		public static bool Draw(SerializedProperty property, System.Action resetToDefault)
		{
			bool res = false;

			EditorGUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(property);
			res = EditorGUI.EndChangeCheck();

			if (GUILayout.Button(RESET_LABEL, GUILayout.Width(RESET_BUTTON_WIDTH)))
			{
				resetToDefault();
				GUI.changed = true;
				res = true;
			}

			EditorGUILayout.EndHorizontal();

			return res;
		}
	}
}
