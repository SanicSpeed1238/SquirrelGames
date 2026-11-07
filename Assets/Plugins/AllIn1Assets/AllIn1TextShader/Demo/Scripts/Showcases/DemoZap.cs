using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//Fires short lived flashes on a set of images at random: bolts crackle through their frames while alive, flares pop and fade.
	//Each strike can also brighten a CanvasGroup (a haze behind the title)
	public class DemoZap : MonoBehaviour
	{
		public RawImage[] slots;
		public Texture[] frames;
		public Vector2 life = new Vector2(0.12f, 0.3f);
		public Vector2 wait = new Vector2(0.5f, 1.8f);
		public float frameRate = 20f;
		public Vector2 scale = new Vector2(0.85f, 1.15f);
		public bool mirror = true;
		public float popIn;
		[Range(0f, 1f)] public float fadeOut;
		public CanvasGroup flash;
		public float flashAmount = 0.4f;
		public float flashDecay = 6f;
		public int seed = 1;

		private float[] timers;
		private float[] lives;
		private float[] nextFrame;
		private Vector3[] baseScales;
		private Vector3[] strikeScales;
		private System.Random rng;

		private float Range(Vector2 r) => r.x + (float)rng.NextDouble() * (r.y - r.x);

		private void OnEnable()
		{
			rng = new System.Random(seed);
			int n = slots.Length;
			timers = new float[n];
			lives = new float[n];
			nextFrame = new float[n];
			strikeScales = new Vector3[n];
			if (baseScales == null || baseScales.Length != n)
			{
				baseScales = new Vector3[n];
				for (int i = 0; i < n; i++) { baseScales[i] = slots[i].rectTransform.localScale; }
			}
			for (int i = 0; i < n; i++)
			{
				Hide(i);
				timers[i] = -Range(wait) * (float)rng.NextDouble();
			}
			if (flash != null) { flash.alpha = 0f; }
		}

		private void Hide(int i)
		{
			Color c = slots[i].color;
			c.a = 0f;
			slots[i].color = c;
			lives[i] = 0f;
		}

		private void Strike(int i)
		{
			lives[i] = Range(life);
			timers[i] = 0f;
			nextFrame[i] = 0f;
			float s = Range(scale);
			float flip = mirror && rng.NextDouble() < 0.5 ? -1f : 1f;
			strikeScales[i] = new Vector3(baseScales[i].x * s, baseScales[i].y * s * flip, 1f);
			slots[i].rectTransform.localScale = strikeScales[i];
			if (flash != null) { flash.alpha = Mathf.Min(1f, flash.alpha + flashAmount); }
		}

		private void Update()
		{
			float dt = Time.deltaTime;
			for (int i = 0; i < slots.Length; i++)
			{
				timers[i] += dt;
				if (lives[i] <= 0f)
				{
					if (timers[i] >= 0f) { Strike(i); } else { continue; }
				}
				if (timers[i] >= lives[i])
				{
					Hide(i);
					timers[i] = -Range(wait);
					continue;
				}
				RawImage img = slots[i];
				if (frames.Length > 0 && timers[i] >= nextFrame[i])
				{
					img.texture = frames[rng.Next(frames.Length)];
					nextFrame[i] = frameRate > 0f ? timers[i] + 1f / frameRate : float.MaxValue;
				}
				float t = timers[i] / lives[i];
				float a = 1f;
				if (popIn > 0f) { a = Mathf.Min(a, timers[i] / popIn); }
				if (fadeOut > 0f && t > 1f - fadeOut) { a = Mathf.Min(a, (1f - t) / fadeOut); }
				if (frameRate > 0f) { a *= 0.65f + 0.35f * (float)rng.NextDouble(); }
				Color c = img.color;
				c.a = a;
				img.color = c;
				if (popIn > 0f) { img.rectTransform.localScale = strikeScales[i] * Mathf.Lerp(0.4f, 1f, Mathf.Clamp01(timers[i] / popIn)); }
			}
			if (flash != null) { flash.alpha = Mathf.MoveTowards(flash.alpha, 0f, flash.alpha * flashDecay * dt + 0.2f * dt); }
		}
	}
}
