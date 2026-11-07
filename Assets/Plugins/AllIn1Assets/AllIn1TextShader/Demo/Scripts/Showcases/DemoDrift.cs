using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Drifts its child images down an area with a sway, a spin and a flutter, wrapping back to the top, like falling petals
	public class DemoDrift : MonoBehaviour
	{
		public Vector2 area = new Vector2(900f, 480f);
		public Vector2 fallSpeed = new Vector2(25f, 55f);
		public float sway = 35f;
		public Vector2 swayPeriod = new Vector2(2.5f, 4.5f);
		public float spinSpeed = 70f;
		public int seed = 7;

		public float edgeFade = 50f;
		public float opacity = 0.9f;

		private RectTransform[] items;
		private UnityEngine.UI.Graphic[] graphics;
		private float[] speed;
		private float[] period;
		private float[] phase;
		private float[] spin;
		private float[] baseX;
		private float time;

		private void OnEnable()
		{
			int n = transform.childCount;
			items = new RectTransform[n];
			graphics = new UnityEngine.UI.Graphic[n];
			speed = new float[n];
			period = new float[n];
			phase = new float[n];
			spin = new float[n];
			baseX = new float[n];
			System.Random rng = new System.Random(seed);
			System.Func<float, float, float> range = (a, b) => a + (float)rng.NextDouble() * (b - a);
			for (int i = 0; i < n; i++)
			{
				items[i] = (RectTransform)transform.GetChild(i);
				graphics[i] = items[i].GetComponent<UnityEngine.UI.Graphic>();
				speed[i] = range(fallSpeed.x, fallSpeed.y);
				period[i] = range(swayPeriod.x, swayPeriod.y);
				phase[i] = range(0f, 6.283f);
				spin[i] = range(-spinSpeed, spinSpeed);
				baseX[i] = range(-area.x * 0.5f, area.x * 0.5f);
				items[i].anchoredPosition = new Vector2(baseX[i], range(-area.y * 0.5f, area.y * 0.5f));
			}
			time = 0f;
		}

		private void Update()
		{
			time += Time.deltaTime;
			for (int i = 0; i < items.Length; i++)
			{
				Vector2 p = items[i].anchoredPosition;
				p.y -= speed[i] * Time.deltaTime;
				if (p.y < -area.y * 0.5f) { p.y += area.y; }
				float wave = Mathf.Sin(time / period[i] * 6.283f + phase[i]);
				p.x = baseX[i] + sway * wave;
				items[i].anchoredPosition = p;
				items[i].localRotation = Quaternion.Euler(0f, 0f, spin[i] * time + 25f * wave);
				items[i].localScale = new Vector3(0.35f + 0.65f * Mathf.Abs(Mathf.Cos(time * 1.3f + phase[i])), 1f, 1f);
				if (graphics[i] != null)
				{
					Color c = graphics[i].color;
					c.a = opacity * Mathf.Clamp01((area.y * 0.5f - Mathf.Abs(p.y)) / edgeFade);
					graphics[i].color = c;
				}
			}
		}
	}
}
