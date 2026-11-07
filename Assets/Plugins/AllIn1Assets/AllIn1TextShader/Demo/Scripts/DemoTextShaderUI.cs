using TMPro;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class DemoTextShaderUI : MonoBehaviour
	{
		public TMP_Text txtDemoName;
		public AllIn1DemoScaleTween demoNameTween;
		// The Effect Info popup's body, shown with the I key; filled from the current showcase's description
		public TMP_Text txtDemoInfo;
		public TextShaderDemoElementsController demoElementsController;

		private bool pendingRefresh;

		private void Start()
		{
			Init();
		}

		public void Init()
		{
			demoElementsController.Init(this);
		}

		public void RefreshUI()
		{
			TextShaderDemoElement demoElement = demoElementsController.GetCurrentDemoElement();
			txtDemoName.text = demoElement.demoName;
			if (demoNameTween != null)
			{
				demoNameTween.ScaleDownTween();
			}
			if (txtDemoInfo != null)
			{
				txtDemoInfo.text = demoElement.demoDescription;
			}
		}

		private void _RefreshUI()
		{
			pendingRefresh = true;
		}

		private void LateUpdate()
		{
			if (pendingRefresh)
			{
				RefreshUI();
				pendingRefresh = false;
			}
		}
	}
}