using System;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(TextShaderDemoElementsController))]
	public class TextShaderDemoElementsControllerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			TextShaderDemoElementsController controller = (TextShaderDemoElementsController)target;
			TextShaderDemoElement[] demoElements = controller.demoElements;
			if (demoElements == null || demoElements.Length == 0)
			{
				return;
			}

			int currentIndex = GetCurrentIndex(controller);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Demo Navigation", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(GetCurrentLabel(demoElements, currentIndex));

			using (new EditorGUI.DisabledScope(Application.isPlaying && currentIndex < 0))
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("< Previous"))
				{
					Navigate(controller, currentIndex, -1);
				}
				if (GUILayout.Button("Next >"))
				{
					Navigate(controller, currentIndex, 1);
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		private int GetCurrentIndex(TextShaderDemoElementsController controller)
		{
			if (Application.isPlaying)
			{
				return Array.IndexOf(controller.demoElements, controller.GetCurrentDemoElement());
			}

			for (int i = 0; i < controller.demoElements.Length; i++)
			{
				if (controller.demoElements[i] != null && controller.demoElements[i].gameObject.activeSelf)
				{
					return i;
				}
			}

			return -1;
		}

		private string GetCurrentLabel(TextShaderDemoElement[] demoElements, int currentIndex)
		{
			if (currentIndex < 0 || demoElements[currentIndex] == null)
			{
				return "No demo element shown";
			}

			TextShaderDemoElement element = demoElements[currentIndex];
			string elementName = string.IsNullOrEmpty(element.demoName) ? element.gameObject.name : element.demoName;
			return (currentIndex + 1) + " / " + demoElements.Length + "   " + elementName;
		}

		private void Navigate(TextShaderDemoElementsController controller, int currentIndex, int step)
		{
			if (Application.isPlaying)
			{
				if (step > 0)
				{
					controller.NextDemo();
				}
				else
				{
					controller.PreviousDemo();
				}
				return;
			}

			TextShaderDemoElement[] demoElements = controller.demoElements;
			int newIndex = currentIndex < 0 ? 0 : (currentIndex + step + demoElements.Length) % demoElements.Length;

			for (int i = 0; i < demoElements.Length; i++)
			{
				if (demoElements[i] == null)
				{
					continue;
				}

				GameObject elementGameObject = demoElements[i].gameObject;
				bool shouldBeActive = i == newIndex;
				if (elementGameObject.activeSelf != shouldBeActive)
				{
					Undo.RecordObject(elementGameObject, "Show Demo Element");
					elementGameObject.SetActive(shouldBeActive);
				}
			}

			TextShaderDemoElement newElement = demoElements[newIndex];
			if (newElement != null)
			{
				controller.demoSceneConfigurator.ApplyCameraConfiguration(newElement.camerasConfiguration);
				controller.postprocessController.ApplyConfig(newElement.GetPostprocessConfig());
			}
		}
	}
}
