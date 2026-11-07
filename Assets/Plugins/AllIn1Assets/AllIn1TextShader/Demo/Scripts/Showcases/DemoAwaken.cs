using AllIn1Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//Text Particles: AWAKEN types in out of sparks (Per Letter emission), floats up as its glow heats and streaks converge, implodes into an orb and slams
	//back in molten gold (its burst and shockwave fire on Reveal Completed), holds, grows a little and shrinks away (the last burst fires on Hide Completed)
	//while the backdrop dips to black, then loops. The emitters' own triggers do what they can; this timeline runs the rest
	public class DemoAwaken : TextShaderDemoElement
	{
		public AllIn1TextTypewriter cold;
		public AllIn1TextTypewriter gold;
		//Shaken on the slam; the texts and their emitters live under it, the flash doesn't
		public RectTransform stage;
		public AllIn1TextParticles charge;
		//The charge's Rate Multiplier over the loop's time
		public AnimationCurve chargeRate = AnimationCurve.Linear(0f, 1f, 1f, 1f);
		public AllIn1TextParticles[] orb = new AllIn1TextParticles[0];
		//Played on the slam, stopped as the gold word leaves
		public AllIn1TextParticles[] afterglow = new AllIn1TextParticles[0];
		//The gold word's Hide Completed bursts; HideImmediate fires that event too, so a restart silences them
		public AllIn1TextParticles[] exitBurst = new AllIn1TextParticles[0];
		public Graphic flash;
		public DemoBackdrop.Look slamLook = new DemoBackdrop.Look();
		public float loopLength = 8f;

		[Header("Timeline (seconds into the loop)")]
		public float revealAt = 0.15f;
		public float chargeAt = 0.85f;
		public float orbAt = 3.25f;
		public float chargeStopAt = 3.4f;
		public float implodeAt = 3.5f;
		public float slamAt = 3.86f;
		public float impactAt = 4f;
		public float afterglowAt = 4.05f;
		public float exitAt = 6.5f;
		public float dipAt = 6.8f;

		[Header("Gathering energy")]
		public float gatherFrom = 0.8f;
		public float gatherTo = 3.5f;
		public float rise = 26f;
		public float tremble = 3.5f;
		[ColorUsage(true, true)] public Color glowTo = new Color(4.5f, 3.98f, 2.83f, 1f);
		public float glowSizeTo = 0.95f;

		[Header("Impact")]
		public float flashPeak = 0.85f;
		public float flashTime = 0.45f;
		public float shakeAmount = 18f;
		public float shakeTime = 0.5f;

		private static readonly int GLOW_COLOR = Shader.PropertyToID("_OuterGlowColor");
		private static readonly int GLOW_SIZE = Shader.PropertyToID("_OuterGlowSize");

		private float time;
		private bool playing;
		private Material coldMaterial;
		private Color glowFrom;
		private float glowSizeFrom;
		private Vector2 coldBase;
		private Vector2 stageBase;
		private DemoBackdrop backdrop;

		//Read before any Update moves them: the carousel's prewarm runs Update before the first Show
		private void Awake()
		{
			if (cold != null) { coldBase = ((RectTransform)cold.transform).anchoredPosition; }
			if (stage != null) { stageBase = stage.anchoredPosition; }
		}

		public override void Show(DemoMovementDir dir)
		{
			if (!gameObject.activeSelf) { gameObject.SetActive(true); }
			for (int i = 0; i < materialConfigurators.Length; i++) { materialConfigurators[i].UpdateConfiguration(); }
			backdrop = FindAnyObjectByType<DemoBackdrop>();
			//The runtime copy at its saved values on the first visit; later visits put those back
			if (coldMaterial == null)
			{
				coldMaterial = cold.GetComponent<TMP_Text>().fontSharedMaterial;
				glowFrom = coldMaterial.GetColor(GLOW_COLOR);
				glowSizeFrom = coldMaterial.GetFloat(GLOW_SIZE);
			}
			Restart();
			time = 0f;
			playing = true;
			SetState(State.SHOWING);
		}

		public override void Hide(DemoMovementDir dir)
		{
			playing = false;
			if (!cold.IsHidden()) { cold.Hide(); }
			if (!gold.IsHidden()) { gold.Hide(); }
			if (charge != null) { charge.Stop(); }
			foreach (AllIn1TextParticles p in afterglow) { if (p != null) { p.Stop(); } }
			SetFlash(0f);
			if (stage != null) { stage.anchoredPosition = stageBase; }
			SetState(State.HIDING);
		}

		public override float GetShowingTransitionProgress()
		{
			return 1f;
		}

		public override float GetHidingTransitionProgress()
		{
			return cold.IsHidden() && gold.IsHidden() ? 1f : 0f;
		}

		protected override void Update()
		{
			base.Update();
			if (!playing) { return; }
			float from = time;
			time += Time.deltaTime;
			if (time >= loopLength)
			{
				time -= loopLength;
				from = -1f;
				Restart();
			}
			Fire(from, time);
			Animate();
		}

		private void Restart()
		{
			cold.HideImmediate();
			gold.HideImmediate();
			foreach (AllIn1TextParticles p in exitBurst) { if (p != null) { p.Stop(true, true); } }
			if (charge != null)
			{
				charge.Stop(true, true);
				charge.rateMultiplier = 1f;
			}
			foreach (AllIn1TextParticles p in orb) { if (p != null) { p.Stop(true, true); } }
			foreach (AllIn1TextParticles p in afterglow) { if (p != null) { p.Stop(true, true); } }
			if (coldMaterial != null)
			{
				coldMaterial.SetColor(GLOW_COLOR, glowFrom);
				coldMaterial.SetFloat(GLOW_SIZE, glowSizeFrom);
			}
			((RectTransform)cold.transform).anchoredPosition = coldBase;
			if (stage != null) { stage.anchoredPosition = stageBase; }
			SetFlash(0f);
		}

		private void Fire(float from, float to)
		{
			if (Crossed(from, to, revealAt)) { cold.Reveal(); }
			if (Crossed(from, to, chargeAt) && charge != null) { charge.Play(); }
			if (Crossed(from, to, orbAt)) { foreach (AllIn1TextParticles p in orb) { if (p != null) { p.Play(); } } }
			if (Crossed(from, to, chargeStopAt) && charge != null) { charge.Stop(); }
			if (Crossed(from, to, implodeAt)) { cold.Hide(); }
			if (Crossed(from, to, slamAt)) { gold.Reveal(); }
			if (Crossed(from, to, impactAt) && backdrop != null) { backdrop.Apply(slamLook); }
			if (Crossed(from, to, afterglowAt)) { foreach (AllIn1TextParticles p in afterglow) { if (p != null) { p.Play(); } } }
			if (Crossed(from, to, exitAt))
			{
				foreach (AllIn1TextParticles p in afterglow) { if (p != null) { p.Stop(); } }
				gold.Hide();
			}
			if (Crossed(from, to, dipAt) && backdrop != null) { backdrop.FadeTo(backdrop.LookFor(name)); }
		}

		private static bool Crossed(float from, float to, float at)
		{
			return from < at && at <= to;
		}

		private void Animate()
		{
			if (charge != null) { charge.rateMultiplier = chargeRate.Evaluate(time); }

			//Gathering energy: the cold word drifts up and trembles harder while its glow heats
			if (time >= gatherFrom && time < implodeAt + 0.5f)
			{
				float k = Mathf.Clamp01((time - gatherFrom) / Mathf.Max(0.01f, gatherTo - gatherFrom));
				float shiver = tremble * k * k;
				Vector2 drift = new Vector2((Mathf.PerlinNoise(time * 30f, 0.13f) - 0.5f) * 2f * shiver, rise * Mathf.SmoothStep(0f, 1f, k) + (Mathf.PerlinNoise(0.57f, time * 30f) - 0.5f) * 2f * shiver);
				((RectTransform)cold.transform).anchoredPosition = coldBase + drift;
				float g = Mathf.SmoothStep(0f, 1f, k);
				if (coldMaterial != null)
				{
					coldMaterial.SetColor(GLOW_COLOR, Color.Lerp(glowFrom, glowTo, g));
					coldMaterial.SetFloat(GLOW_SIZE, Mathf.Lerp(glowSizeFrom, glowSizeTo, g));
				}
			}

			float since = time - impactAt;
			SetFlash(since >= 0f ? flashPeak * Mathf.Pow(1f - Mathf.Clamp01(since / Mathf.Max(0.01f, flashTime)), 2f) : 0f);
			if (stage != null)
			{
				float s = since >= 0f && since < shakeTime ? 1f - since / shakeTime : 0f;
				Vector2 jolt = new Vector2(Mathf.PerlinNoise(time * 38f, 0.37f) - 0.5f, Mathf.PerlinNoise(0.71f, time * 38f) - 0.5f) * 2f * shakeAmount * s * s;
				stage.anchoredPosition = stageBase + jolt;
			}
		}

		private void SetFlash(float alpha)
		{
			if (flash == null) { return; }
			Color c = flash.color;
			c.a = alpha;
			flash.color = c;
		}
	}
}
