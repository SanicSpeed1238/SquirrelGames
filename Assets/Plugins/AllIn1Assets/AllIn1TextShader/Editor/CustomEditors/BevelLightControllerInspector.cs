using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(BevelLightController))]
	public class BevelLightControllerInspector : Editor
	{
		private static readonly GUIContent RESET_BUTTON_LABEL = new GUIContent("R", "Resets to default value");

		private int controllersInScene;

		private void OnEnable()
		{
			controllersInScene = FindObjectsByType<BevelLightController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
		}

		public override void OnInspectorGUI()
		{
			if (controllersInScene > 1)
			{
				EditorGUILayout.HelpBox(controllersInScene + " Bevel Light Controllers are enabled in the scene. Only one drives the shader at a time, the last one edited or enabled.", MessageType.Warning);
			}

			serializedObject.Update();

			EditorGUILayout.LabelField("Light Direction (Bevel and Extrude)", EditorStyles.boldLabel);
			DrawFloat("lightAngle", BevelLightController.DEFAULT_LIGHT_ANGLE);
			DrawFloat("lightElevation", BevelLightController.DEFAULT_LIGHT_ELEVATION);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Light Color", EditorStyles.boldLabel);
			DrawEnum("lightBlending", (int)BevelLightController.DEFAULT_LIGHT_BLENDING);
			DrawColor("lightColor", BevelLightController.DEFAULT_LIGHT_COLOR);
			DrawFloat("lightCutoff", BevelLightController.DEFAULT_LIGHT_CUTOFF);
			DrawFloat("lightSmoothness", BevelLightController.DEFAULT_LIGHT_SMOOTHNESS);
			DrawFloat("lightIntensity", BevelLightController.DEFAULT_LIGHT_INTENSITY);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Shadow Color", EditorStyles.boldLabel);
			DrawEnum("shadowBlending", (int)BevelLightController.DEFAULT_SHADOW_BLENDING);
			DrawColor("shadowColor", BevelLightController.DEFAULT_SHADOW_COLOR);
			DrawFloat("shadowCutoff", BevelLightController.DEFAULT_SHADOW_CUTOFF);
			DrawFloat("shadowSmoothness", BevelLightController.DEFAULT_SHADOW_SMOOTHNESS);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Extrude", EditorStyles.boldLabel);
			DrawColor("extrudeLightColor", BevelLightController.DEFAULT_EXTRUDE_LIGHT_COLOR);
			DrawColor("extrudeShadowColor", BevelLightController.DEFAULT_EXTRUDE_SHADOW_COLOR);
			DrawFloat("extrudeLightSmoothness", BevelLightController.DEFAULT_EXTRUDE_LIGHT_SMOOTHNESS);

			//Applying runs OnValidate, which pushes the globals. Globals mark nothing dirty, so the views are repainted by hand
			if (serializedObject.ApplyModifiedProperties())
			{
				InternalEditorUtility.RepaintAllViews();
			}
		}

		private void DrawFloat(string propertyName, float defaultValue)
		{
			SerializedProperty property = BeginRow(propertyName);
			if (ResetButton())
			{
				property.floatValue = defaultValue;
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawColor(string propertyName, Color defaultValue)
		{
			SerializedProperty property = BeginRow(propertyName);
			if (ResetButton())
			{
				property.colorValue = defaultValue;
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawEnum(string propertyName, int defaultValue)
		{
			SerializedProperty property = BeginRow(propertyName);
			if (ResetButton())
			{
				property.enumValueIndex = defaultValue;
			}
			EditorGUILayout.EndHorizontal();
		}

		private SerializedProperty BeginRow(string propertyName)
		{
			SerializedProperty property = serializedObject.FindProperty(propertyName);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(property);
			return property;
		}

		private bool ResetButton()
		{
			return GUILayout.Button(RESET_BUTTON_LABEL, GUILayout.Width(20));
		}
	}
}
