using TMPro;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(TextMaterialConfiguratorMaster))]
	public class TextMaterialConfiguratorCustomEditor : Editor
	{
		private const string TAG = "TextMaterialConfiguratorCustomEditor";

		private TextMaterialConfiguratorMaster textMaterialConfigurator;

		private string lastText;
		private Vector2 lastRectSize;

		private void OnEnable()
		{
			EditorApplication.update += EditorUpdate;

			RefreshReferences();
		}

		private void OnDisable()
		{
			EditorApplication.update -= EditorUpdate;
		}

		private void RefreshReferences()
		{
			textMaterialConfigurator = (TextMaterialConfiguratorMaster)target;

			if (!EditorTypewritersManager.IsSceneMaterialConfiguratorAlreadyRegistered(textMaterialConfigurator))
			{
				EditorTypewritersManager.RegisterMaterialConfiguratorInScene(textMaterialConfigurator);
			}
		}

		// Nothing to show: the component is added and driven by AllIn1TextShaderComponent
		public override void OnInspectorGUI()
		{
			RefreshReferences();
		}

		private void EditorUpdate()
		{
			if(textMaterialConfigurator == null) { return; }
			if(!textMaterialConfigurator.gameObject.activeInHierarchy) { return; }
			if(textMaterialConfigurator.textMeshHandler == null) { return; }
			if(textMaterialConfigurator.textComponent == null) { return; }

			string newText = textMaterialConfigurator.textMeshHandler.GetText();
			Vector2 newRectSize = textMaterialConfigurator.textComponent.rectTransform.rect.size;

			if (newText != lastText || newRectSize != lastRectSize)
			{
				textMaterialConfigurator.UpdateConfiguration(true);

				lastText = newText;
				lastRectSize = newRectSize;
			}
		}
	}
}