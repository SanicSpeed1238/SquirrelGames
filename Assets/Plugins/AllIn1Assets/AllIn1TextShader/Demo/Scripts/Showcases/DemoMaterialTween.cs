using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Eases one float of a text's material between a hidden and a shown value: the demo calls In from the typewriter's On Reveal Started and Out from
	//On Hide Started, like DemoFrameFader for a material property. Writes the configurator's material, as the demo's tweens do
	public class DemoMaterialTween : MonoBehaviour
	{
		public enum Ease { Linear, Out, In, InOut }

		public string property = "_TwistUvAmount";
		public float hiddenValue = 1.5708f;
		public float shownValue;
		public float inDuration = 1f;
		public Ease inEase = Ease.Out;
		public float outDuration = 0.6f;
		public Ease outEase = Ease.In;

		private TextMaterialConfiguratorMaster configurator;
		private int id;
		private float from;
		private float to;
		private float time;
		private float duration = 1f;
		private Ease ease;
		private bool running;

		private void OnEnable()
		{
			configurator = GetComponent<TextMaterialConfiguratorMaster>();
			id = Shader.PropertyToID(property);
			running = false;
			Set(hiddenValue);
		}

		public void In()
		{
			Run(shownValue, inDuration, inEase);
		}

		public void Out()
		{
			Run(hiddenValue, outDuration, outEase);
		}

		private void Run(float target, float seconds, Ease curve)
		{
			Material m = Mat();
			from = m != null ? m.GetFloat(id) : target;
			to = target;
			duration = Mathf.Max(0.0001f, seconds);
			ease = curve;
			time = 0f;
			running = true;
		}

		private void Update()
		{
			if (!running) { return; }
			time += Time.deltaTime;
			float t = Mathf.Clamp01(time / duration);
			Set(Mathf.Lerp(from, to, Apply(t)));
			if (t >= 1f) { running = false; }
		}

		private float Apply(float t)
		{
			switch (ease)
			{
				case Ease.Out: return 1f - (1f - t) * (1f - t) * (1f - t);
				case Ease.In: return t * t * t;
				case Ease.InOut: return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
				default: return t;
			}
		}

		private Material Mat()
		{
			return configurator != null ? configurator.TextMaterial : null;
		}

		private void Set(float value)
		{
			Material m = Mat();
			if (m != null) { m.SetFloat(id, value); }
		}
	}
}
