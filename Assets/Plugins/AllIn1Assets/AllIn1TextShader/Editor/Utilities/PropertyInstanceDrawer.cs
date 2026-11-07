using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public static class PropertyInstanceDrawer
	{
		public static void Draw(ref PropertyInstance propertyInstance, PropertyInstance[] effectDefaults = null)
		{
			EditorGUILayout.BeginHorizontal();

			switch (propertyInstance.propertyType)
			{
				case PropertyDataType.FLOAT:
					DrawPropertyFloat(ref propertyInstance);
					break;
				case PropertyDataType.VECTOR2:
					DrawPropertyVector2(ref propertyInstance);
					break;
				case PropertyDataType.VECTOR3:
					DrawPropertyVector3(ref propertyInstance);
					break;
				case PropertyDataType.COLOR:
					break;
			}

			if (GUILayout.Button("R", GUILayout.Width(20)) && !ResetFromEffect(ref propertyInstance, effectDefaults))
			{
				propertyInstance.SetToDefault();
			}

			EditorGUILayout.EndHorizontal();
		}

		// A preset's stored reset values are copies that can be stale (the shipped presets kept zeros); this also repairs the copy
		private static bool ResetFromEffect(ref PropertyInstance propertyInstance, PropertyInstance[] effectDefaults)
		{
			if (effectDefaults == null) { return false; }

			for (int i = 0; i < effectDefaults.Length; i++)
			{
				if (effectDefaults[i].propertyName != propertyInstance.propertyName) { continue; }

				propertyInstance.CopyValuesFrom(effectDefaults[i]);
				return true;
			}

			return false;
		}

		public static void DrawPropertyFloat(ref PropertyInstance propertyData)
		{
			propertyData.floatValue = EditorGUILayout.FloatField(propertyData.displayName, propertyData.floatValue);
		}

		public static void DrawPropertyVector2(ref PropertyInstance propertyData)
		{
			propertyData.vec2Value = EditorGUILayout.Vector2Field(propertyData.displayName, propertyData.vec2Value);
		}

		public static void DrawPropertyVector3(ref PropertyInstance propertyData)
		{
			propertyData.vec3Value = EditorGUILayout.Vector3Field(propertyData.displayName, propertyData.vec3Value);
		}
	}
}