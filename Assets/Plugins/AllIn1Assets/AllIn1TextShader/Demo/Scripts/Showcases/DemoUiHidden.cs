using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Hide UI is a screenshot and beauty mode: nothing tells how to bring the UI back, and the hidden buttons take no clicks
	public class DemoUiHidden : MonoBehaviour
	{
		public CanvasGroup hiddenGroup;

		//0 with the UI shown, 1 with it hidden, eased along its fade: DemoFit and the backdrop glide the stage to the window's centre with it
		public static float Focus { get; private set; }

		private void OnEnable()
		{
			UpdateFocus();
		}

		//Statics outlive Play in the editor
		private void OnDisable()
		{
			Focus = 0f;
		}

		private void Update()
		{
			UpdateFocus();
		}

		private void LateUpdate()
		{
			hiddenGroup.blocksRaycasts = hiddenGroup.alpha > 0.5f;
		}

		private void UpdateFocus()
		{
			Focus = Mathf.SmoothStep(0f, 1f, 1f - hiddenGroup.alpha);
		}
	}
}
