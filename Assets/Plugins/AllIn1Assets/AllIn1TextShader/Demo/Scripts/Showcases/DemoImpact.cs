using System;
using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//A landing hit: squashes a target onto its pivot and springs it back, shakes a group and plays one shot sprites (shockwave, dust). Call Hit from an event
	public class DemoImpact : MonoBehaviour
	{
		[Serializable]
		public struct Puff
		{
			public Graphic graphic;
			public Vector2 fromScale;
			public Vector2 toScale;
			public Vector2 move;
			public float duration;
			public float alpha;
		}

		public Transform squashTarget;
		//Scale change at the moment of impact, x and y; springs back to 1
		public Vector2 squash = new Vector2(0.3f, -0.3f);
		public float squashDamping = 7f;
		public float squashFrequency = 4f;
		public RectTransform shakeTarget;
		public float shakeAmount = 14f;
		public float shakeDuration = 0.35f;
		public Puff[] puffs = new Puff[0];

		private float time = -1f;
		private bool shaking;
		private Vector2 shakeBase;
		private Vector2[] puffBase = new Vector2[0];
		private Color[] puffColor = new Color[0];

		private void Awake()
		{
			puffBase = new Vector2[puffs.Length];
			puffColor = new Color[puffs.Length];
			for (int i = 0; i < puffs.Length; i++)
			{
				puffBase[i] = puffs[i].graphic.rectTransform.anchoredPosition;
				puffColor[i] = puffs[i].graphic.color;
			}
			ResetAll();
		}

		public void Hit()
		{
			if (!shaking && shakeTarget != null) { shakeBase = shakeTarget.anchoredPosition; }
			shaking = shakeTarget != null;
			time = 0f;
		}

		private void Update()
		{
			if (time < 0f) { return; }
			time += Time.deltaTime;

			float spring = Mathf.Exp(-squashDamping * time) * Mathf.Cos(2f * Mathf.PI * squashFrequency * time);
			if (squashTarget != null) { squashTarget.localScale = new Vector3(1f + squash.x * spring, 1f + squash.y * spring, 1f); }

			if (shaking)
			{
				float k = Mathf.Clamp01(1f - time / shakeDuration);
				float n = time * 40f;
				Vector2 offset = new Vector2(Mathf.PerlinNoise(n, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, n) - 0.5f) * (2f * shakeAmount * k * k);
				shakeTarget.anchoredPosition = shakeBase + offset;
			}

			bool busy = time < shakeDuration || Mathf.Exp(-squashDamping * time) > 0.004f;
			for (int i = 0; i < puffs.Length; i++)
			{
				Puff p = puffs[i];
				float t = Mathf.Clamp01(time / p.duration);
				float e = 1f - (1f - t) * (1f - t) * (1f - t);
				p.graphic.rectTransform.localScale = Vector2.LerpUnclamped(p.fromScale, p.toScale, e);
				p.graphic.rectTransform.anchoredPosition = puffBase[i] + p.move * e;
				Color c = puffColor[i];
				c.a = p.alpha * (1f - t) * (1f - t);
				p.graphic.color = c;
				busy |= t < 1f;
			}
			if (!busy) { ResetAll(); }
		}

		private void OnDisable()
		{
			ResetAll();
		}

		private void ResetAll()
		{
			time = -1f;
			if (squashTarget != null) { squashTarget.localScale = Vector3.one; }
			if (shaking) { shakeTarget.anchoredPosition = shakeBase; }
			shaking = false;
			for (int i = 0; i < puffs.Length && i < puffColor.Length; i++)
			{
				Color c = puffColor[i];
				c.a = 0f;
				puffs[i].graphic.color = c;
				puffs[i].graphic.rectTransform.anchoredPosition = puffBase[i];
			}
		}
	}
}
