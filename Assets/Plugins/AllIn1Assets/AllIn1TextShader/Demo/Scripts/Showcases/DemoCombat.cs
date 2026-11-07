using System;
using System.Collections.Generic;
using AllIn1Assets;
using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//Floating combat text: pooled texts spawn on a looping beat over a target, reveal, rise and hide; the target squashes, flashes or dodges and its HP bar follows
	public class DemoCombat : TextShaderDemoElement
	{
		public enum Reaction { NONE, HIT, CRIT, HEAL, DODGE }

		[Serializable]
		public struct Kind
		{
			public string[] contents;
			public AllIn1TextTypewriter[] pool;
			public Vector2 offset;
			public float spread;
			public float rise;
			public float drift;
			public float life;
			public float punch;
			public float hp;
			public Reaction reaction;
		}

		[Serializable]
		public struct Beat
		{
			public int kind;
			public float at;
		}

		private class Popup
		{
			public RectTransform rect;
			public AllIn1TextTypewriter typewriter;
			public Vector2 from;
			public float drift;
			public float rise;
			public float life;
			public float punch;
			public float time;
			public bool hiding;
		}

		public Kind[] kinds = new Kind[0];
		public Beat[] beats = new Beat[0];
		public float loopLength = 6f;
		public Vector2 spawnOrigin;

		public RectTransform target;
		public Graphic[] flashGraphics = new Graphic[0];
		//Silhouettes over a textured target, shown in the flash colour since tinting the texture can't brighten it
		public Graphic[] flashOverlays = new Graphic[0];
		public float flashOverlayAlpha = 0.85f;
		//Texts that stay up for the whole showcase, revealed on Show and hidden on Hide
		public AllIn1TextTypewriter[] labels = new AllIn1TextTypewriter[0];
		public RectTransform hpFill;
		//Behind the fill: a hit drops the fill at once and this drains after it, a heal moves this ahead and the fill grows into it
		public RectTransform hpTrail;
		public float trailHold = 0.35f;
		public float catchUpRate = 6f;
		public float hpFullWidth = 230f;
		public Graphic hpFillGraphic;
		public Color hpHigh = new Color(0.36f, 0.86f, 0.36f);
		public Color hpLow = new Color(0.95f, 0.25f, 0.2f);

		private const float HIDE_TIME = 0.3f;

		private readonly List<Popup> active = new List<Popup>();
		private int[] nextInPool = new int[0];
		private Color[] baseColors = new Color[0];
		private Vector2 targetBase;
		private float time;
		private bool playing;
		private float hp = 1f;
		private float shownHp = 1f;
		private float trailHp = 1f;
		private float holdTime;
		private float reactTime = 99f;
		private Reaction reaction;
		private Color flashColor;

		//Read before any Update moves the target: the carousel's prewarm runs Update before the first Show
		private void Awake()
		{
			baseColors = new Color[flashGraphics.Length];
			for (int i = 0; i < flashGraphics.Length; i++) { baseColors[i] = flashGraphics[i].color; }
			if (target != null) { targetBase = target.anchoredPosition; }
		}

		public override void Show(DemoMovementDir dir)
		{
			if (!gameObject.activeSelf) { gameObject.SetActive(true); }
			for (int i = 0; i < materialConfigurators.Length; i++) { materialConfigurators[i].UpdateConfiguration(); }
			for (int i = 0; i < typewriters.Length; i++) { typewriters[i].HideImmediate(); }

			nextInPool = new int[kinds.Length];
			active.Clear();
			hp = shownHp = trailHp = 1f;
			holdTime = 0f;
			reactTime = 99f;
			time = 0f;
			playing = true;
			foreach (AllIn1TextTypewriter label in labels) { label.Reveal(); }
			SetState(State.SHOWING);
		}

		public override void Hide(DemoMovementDir dir)
		{
			playing = false;
			foreach (Popup popup in active)
			{
				if (!popup.hiding) { popup.typewriter.Hide(); popup.hiding = true; }
				popup.life = Mathf.Min(popup.life, popup.time + HIDE_TIME);
			}
			foreach (AllIn1TextTypewriter label in labels) { label.Hide(); }
			SetState(State.HIDING);
		}

		public override float GetShowingTransitionProgress()
		{
			return 1f;
		}

		public override float GetHidingTransitionProgress()
		{
			if (active.Count > 0) { return 0f; }
			foreach (AllIn1TextTypewriter label in labels)
			{
				if (!label.IsHidden()) { return 0f; }
			}
			return 1f;
		}

		protected override void Update()
		{
			base.Update();
			float dt = Time.deltaTime;

			if (playing)
			{
				float from = time;
				time += dt;
				Fire(from, time);
				while (time >= loopLength)
				{
					time -= loopLength;
					Fire(-1f, time);
				}
			}

			UpdatePopups(dt);
			UpdateTarget(dt);
		}

		private void Fire(float from, float to)
		{
			foreach (Beat beat in beats)
			{
				if (from < beat.at && beat.at <= to) { Spawn(beat.kind); }
			}
		}

		private void Spawn(int kindIndex)
		{
			Kind kind = kinds[kindIndex];
			if (kind.pool == null || kind.pool.Length == 0) { return; }

			AllIn1TextTypewriter typewriter = kind.pool[nextInPool[kindIndex]];
			nextInPool[kindIndex] = (nextInPool[kindIndex] + 1) % kind.pool.Length;
			active.RemoveAll(p => p.typewriter == typewriter);

			typewriter.SetText(kind.contents[UnityEngine.Random.Range(0, kind.contents.Length)]);
			//A pooled text was stopped by HideImmediate, and SetText only arms the reveal of a stopped typewriter
			typewriter.Reveal();
			TextMaterialConfiguratorMaster configurator = typewriter.GetComponent<TextMaterialConfiguratorMaster>();
			if (configurator != null) { configurator.UpdateConfiguration(true); }

			RectTransform rect = (RectTransform)typewriter.transform;
			Vector2 jitter = new Vector2(UnityEngine.Random.Range(-kind.spread, kind.spread), UnityEngine.Random.Range(0f, kind.spread * 0.25f));
			Popup popup = new Popup
			{
				rect = rect,
				typewriter = typewriter,
				from = spawnOrigin + kind.offset + jitter,
				drift = kind.drift * (jitter.x >= 0f ? 1f : -1f),
				rise = kind.rise,
				life = kind.life,
				punch = kind.punch,
			};
			rect.anchoredPosition = popup.from;
			rect.localScale = Vector3.one * kind.punch;
			active.Add(popup);

			if (kind.hp != 0f)
			{
				hp = Mathf.Clamp(hp + kind.hp, 0.05f, 1f);
				holdTime = trailHold;
			}
			if (kind.reaction != Reaction.NONE)
			{
				reaction = kind.reaction;
				reactTime = 0f;
				flashColor = kind.reaction == Reaction.HEAL ? new Color(0.6f, 1f, 0.6f, 1f) : Color.white;
			}
		}

		private void UpdatePopups(float dt)
		{
			for (int i = active.Count - 1; i >= 0; i--)
			{
				Popup popup = active[i];
				popup.time += dt;
				float p = Mathf.Clamp01(popup.time / popup.life);
				float rise = 1f - (1f - p) * (1f - p) * (1f - p);
				popup.rect.anchoredPosition = popup.from + new Vector2(popup.drift * p, popup.rise * rise);
				float scale = 1f + (popup.punch - 1f) * Mathf.Exp(-popup.time * 14f);
				popup.rect.localScale = new Vector3(scale, scale, 1f);

				if (!popup.hiding && popup.time >= popup.life - HIDE_TIME)
				{
					popup.typewriter.Hide();
					popup.hiding = true;
				}
				if (popup.hiding && popup.time >= popup.life + 0.15f && popup.typewriter.IsHidden())
				{
					active.RemoveAt(i);
				}
			}
		}

		private void UpdateTarget(float dt)
		{
			if (hp < shownHp) { shownHp = hp; }
			if (hp > trailHp) { trailHp = hp; }
			holdTime -= dt;
			if (holdTime <= 0f)
			{
				float catchUp = 1f - Mathf.Exp(-dt * catchUpRate);
				shownHp = Mathf.Lerp(shownHp, hp, catchUp);
				trailHp = Mathf.Lerp(trailHp, hp, catchUp);
			}
			if (hpFill != null) { hpFill.sizeDelta = new Vector2(hpFullWidth * shownHp, hpFill.sizeDelta.y); }
			if (hpTrail != null) { hpTrail.sizeDelta = new Vector2(hpFullWidth * trailHp, hpTrail.sizeDelta.y); }
			if (hpFillGraphic != null) { hpFillGraphic.color = Color.Lerp(hpLow, hpHigh, Mathf.InverseLerp(0.2f, 0.7f, shownHp)); }
			if (target == null) { return; }

			reactTime += dt;
			float k = Mathf.Exp(-reactTime * 9f);
			Vector3 scale = Vector3.one;
			Vector2 offset = Vector2.zero;
			switch (reaction)
			{
				case Reaction.HIT:
					scale = new Vector3(1f + 0.18f * k, 1f - 0.18f * k, 1f);
					break;
				case Reaction.CRIT:
					scale = new Vector3(1f + 0.3f * k, 1f - 0.3f * k, 1f);
					offset = new Vector2(Mathf.Sin(reactTime * 70f) * 14f * k, 0f);
					break;
				case Reaction.HEAL:
					scale = new Vector3(1f - 0.08f * k, 1f + 0.12f * k, 1f);
					break;
				case Reaction.DODGE:
					offset = new Vector2(Mathf.Sin(Mathf.Clamp01(reactTime / 0.4f) * Mathf.PI) * 70f, 0f);
					break;
			}
			target.localScale = scale;
			target.anchoredPosition = targetBase + offset;

			float flash = reaction == Reaction.DODGE ? 0f : Mathf.Clamp01(1f - reactTime / 0.18f);
			for (int i = 0; i < flashGraphics.Length; i++)
			{
				flashGraphics[i].color = Color.Lerp(baseColors[i], new Color(flashColor.r, flashColor.g, flashColor.b, baseColors[i].a), flash);
			}
			for (int i = 0; i < flashOverlays.Length; i++)
			{
				flashOverlays[i].color = new Color(flashColor.r, flashColor.g, flashColor.b, flash * flashOverlayAlpha);
			}
		}
	}
}
