using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// Fades a showcase's non text parts. The demo calls FadeIn and FadeOut from a typewriter's On Reveal Started and On Hide Started events
	[RequireComponent(typeof(CanvasGroup))]
	public class DemoFrameFader : MonoBehaviour
	{
		public enum FadeEase
		{
			Smooth,
			// The scale and rotation overshoot on the way in and wind up before leaving
			Overshoot,
			// The alpha blinks on and off like a tube powering up, and the same backwards when fading out
			Flicker,
		}

		private const float BACK = 1.70158f;
		private static readonly float[] FLICKER_TIMES = { 0f, 0.1f, 0.16f, 0.34f, 0.4f, 0.46f, 0.62f, 0.68f, 0.74f };
		private static readonly float[] FLICKER_VALUES = { 0f, 0.9f, 0f, 1f, 0.1f, 0f, 1f, 0.35f, 1f };

		public float fadeInDuration = 0.5f;
		public float fadeOutDuration = 0.3f;
		public FadeEase ease = FadeEase.Smooth;
		// Scale while hidden, grown to one with the fade; one leaves the scale alone
		public Vector3 hiddenScale = Vector3.one;
		// Z rotation in degrees while hidden, turned to zero with the fade; zero leaves the rotation alone
		public float hiddenRotation;

		private CanvasGroup canvasGroup;
		private float level;
		private float target;
		private float speed;

		private void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}

		private void OnEnable()
		{
			level = 0f;
			target = 0f;
			canvasGroup.alpha = 0f;
			ApplyTransform(0f);
		}

		public void FadeIn()
		{
			StartFade(1f, fadeInDuration);
		}

		public void FadeOut()
		{
			StartFade(0f, fadeOutDuration);
		}

		private void StartFade(float to, float duration)
		{
			target = to;
			speed = duration > 0f ? 1f / duration : float.PositiveInfinity;
		}

		private void Update()
		{
			level = Mathf.MoveTowards(level, target, speed * Time.deltaTime);
			float eased = Mathf.SmoothStep(0f, 1f, level);
			canvasGroup.alpha = ease == FadeEase.Flicker ? Flicker(level) : eased;
			ApplyTransform(ease == FadeEase.Overshoot ? BackOut(level) : eased);
		}

		private void ApplyTransform(float eased)
		{
			if (hiddenScale != Vector3.one)
			{
				transform.localScale = Vector3.LerpUnclamped(hiddenScale, Vector3.one, eased);
			}

			if (hiddenRotation != 0f)
			{
				transform.localRotation = Quaternion.Euler(0f, 0f, hiddenRotation * (1f - eased));
			}
		}

		private static float BackOut(float t)
		{
			float u = t - 1f;
			return 1f + (BACK + 1f) * u * u * u + BACK * u * u;
		}

		private static float Flicker(float t)
		{
			float res = 0f;
			for (int i = 0; i < FLICKER_TIMES.Length && t >= FLICKER_TIMES[i]; i++)
			{
				res = FLICKER_VALUES[i];
			}

			return t >= 1f ? 1f : res;
		}
	}
}
