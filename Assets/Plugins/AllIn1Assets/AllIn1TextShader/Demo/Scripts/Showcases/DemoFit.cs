using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Centres a showcase in the space the demo UI leaves above its bottom row and scales it to fit, for any window: the canvas is always 960 wide and its
	//height follows the aspect (540 at 16:9, 650 in the 960 x 650 WebGL player).
	//Hide UI (DemoUiHidden.Focus) glides the showcase to the window's centre and pushes in by focusZoom
	public class DemoFit : MonoBehaviour
	{
		//The showcase's footprint over its loop, in its own units at scale 1
		public Vector2 center;
		public Vector2 size = new Vector2(760f, 400f);
		//Canvas units the UI keeps: a margin at the top and sides, the bottom row with its margins at the bottom
		public float margin = 19.2f;
		public float bottom = 96f;
		[Range(0.5f, 1f)] public float fill = 0.9f;
		public float maxScale = 1.3f;
		public float focusZoom = 0.05f;

		private RectTransform rectTransform;
		private RectTransform canvasTransform;

		private void OnEnable()
		{
			rectTransform = (RectTransform)transform;
			canvasTransform = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
			Apply();
		}

		private void LateUpdate()
		{
			Apply();
		}

		private void Apply()
		{
			Rect canvas = canvasTransform.rect;
			float top = canvas.height * 0.5f - margin;
			float low = -canvas.height * 0.5f + bottom;
			float focus = DemoUiHidden.Focus;
			float s = Mathf.Min((canvas.width - 2f * margin) * fill / size.x, (top - low) * fill / size.y, maxScale) * (1f + focusZoom * focus);
			Vector3 scale = new Vector3(s, s, 1f);
			Vector2 pos = new Vector2(-center.x * s, Mathf.Lerp((top + low) * 0.5f, 0f, focus) - center.y * s);
			if (rectTransform.localScale != scale) { rectTransform.localScale = scale; }
			if (rectTransform.anchoredPosition != pos) { rectTransform.anchoredPosition = pos; }
		}
	}
}
