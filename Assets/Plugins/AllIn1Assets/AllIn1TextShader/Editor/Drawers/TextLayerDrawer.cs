using UnityEditor;

namespace AllIn1Assets.AllIn1TextShader
{
	public class TextLayerDrawer
	{
		public delegate bool DrawLayerHandler(TextLayer textLayer, TextShaderMaterialEditor materialEditor);

		private TextLayer textLayer;
		private TextShaderMaterialEditor materialEditor;
		private float lastLabelWidth;

		public void Setup(TextLayer textLayer, TextShaderMaterialEditor materialEditor)
		{
			this.textLayer = textLayer;
			this.materialEditor = materialEditor;
		}

		public bool DrawMainLayer(TextLayer textLayer, TextShaderMaterialEditor materialEditor)
		{
			bool changes = false;

			EditorGUI.indentLevel++;
			textLayer.shaderPropertiesUnfolded = EditorGUILayout.Foldout(textLayer.shaderPropertiesUnfolded, "Material Properties");
			EditorGUI.indentLevel--;

			if (textLayer.shaderPropertiesUnfolded)
			{
				materialEditor.OnInspectorGUI();
			}

			return changes;
		}

		public bool DrawSlaveLayer(TextLayer textLayer, TextShaderMaterialEditor materialEditor)
		{
			bool changes = false;

			EditorGUILayout.BeginVertical();
			EditorGUI.BeginChangeCheck();

			ChangeLabelWidth(200f);
			textLayer.isVisible = EditorGUILayout.Toggle("Visible", textLayer.isVisible);
			
			textLayer.offset = EditorGUILayout.Vector2Field("Offset", textLayer.offset);
			textLayer.blendMode = (TextLayer.BlendMode)EditorGUILayout.EnumPopup("Blending mode", textLayer.blendMode);

			if (EditorGUI.EndChangeCheck())
			{
				textLayer.Apply();
				changes = true;
			}

			EditorGUI.BeginChangeCheck();
			textLayer.customTimingSeed = EditorGUILayout.Toggle("Custom timing seed", textLayer.customTimingSeed);
			if (EditorGUI.EndChangeCheck())
			{
				textLayer.RefreshBuffersData(true);
				textLayer.ApplyTimingSeedsBuffer();

				EditorUtility.SetDirty(textLayer.materialConfigurator.gameObject);
			}

			RestoreLabelWidth();
			EditorGUILayout.EndVertical();

			EditorGUI.indentLevel++;
			textLayer.shaderPropertiesUnfolded = EditorGUILayout.Foldout(textLayer.shaderPropertiesUnfolded, "Material Properties");
			EditorGUI.indentLevel--;

			if (textLayer.shaderPropertiesUnfolded)
			{
				materialEditor.OnInspectorGUI();
			}

			return changes;
		}

		private void ChangeLabelWidth(float width)
		{
			lastLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = width;
		}

		private void RestoreLabelWidth()
		{
			EditorGUIUtility.labelWidth = lastLabelWidth;
		}
	}
}