using AllIn1Assets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//One button of the demo UI from the DemoUIHandoff: its key, hover colour, tooltip and pressed look, the same for clicks and key presses
	public class DemoUiButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		public Button button;
		public InputHandler input;

		[Header("Hover")]
		public Graphic background;
		public Color normalColor = Color.white;
		public Color hoverColor = Color.white;
		public CanvasGroup tooltip;
		public float tooltipFade = 0.12f;

		[Header("Press")]
		//Moves down while pressed and the shadow under it hides, like a key going down
		public RectTransform face;
		public float pressDepth = 1.2f;
		public Graphic shadow;
		public float keyPressTime = 0.1f;

		private bool hovered;
		private bool pointerDown;
		private float keyPressTimer;
		private Vector2 faceRest;

		private void Awake()
		{
			if (face != null) { faceRest = face.anchoredPosition; }
		}

		private void OnDisable()
		{
			hovered = false;
			pointerDown = false;
			keyPressTimer = 0f;
			if (tooltip != null) { tooltip.alpha = 0f; }
			Apply();
		}

		private void Update()
		{
			if (input != null && input.IsKeyPressed())
			{
				button.onClick.Invoke();
				keyPressTimer = keyPressTime;
			}
			keyPressTimer -= Time.unscaledDeltaTime;
			if (tooltip != null) { tooltip.alpha = Mathf.MoveTowards(tooltip.alpha, hovered ? 1f : 0f, Time.unscaledDeltaTime / tooltipFade); }
			Apply();
		}

		private void Apply()
		{
			bool pressed = pointerDown || keyPressTimer > 0f;
			if (background != null) { background.color = hovered ? hoverColor : normalColor; }
			if (face != null) { face.anchoredPosition = faceRest + (pressed ? new Vector2(0f, -pressDepth) : Vector2.zero); }
			if (shadow != null) { shadow.enabled = !pressed; }
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			hovered = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			hovered = false;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			pointerDown = eventData.button == PointerEventData.InputButton.Left;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			pointerDown = false;
			//A click selects the button, and Space or Enter would then click it again
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject) { EventSystem.current.SetSelectedGameObject(null); }
		}
	}
}
