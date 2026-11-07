using TMPro;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//The carousel title from the handoff: copies the name into its drop shadow (a second text, like the CSS text-shadow) and writes "Demo N of M" under it
	public class DemoUiLabel : MonoBehaviour
	{
		public TextShaderDemoElementsController controller;
		public TMP_Text demoName;
		public TMP_Text nameShadow;
		public TMP_Text count;
		public string countFormat = "Demo {0} of {1}";

		private TextShaderDemoElement countedElement;

		private void LateUpdate()
		{
			if (nameShadow.text != demoName.text) { nameShadow.text = demoName.text; }

			TextShaderDemoElement current = controller.GetCurrentDemoElement();
			if (current == null || current == countedElement) { return; }
			countedElement = current;
			int index = System.Array.IndexOf(controller.demoElements, current);
			count.text = string.Format(countFormat, index + 1, controller.demoElements.Length);
		}
	}
}
