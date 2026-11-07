using System;
using AllIn1Assets;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	//A dialogue showcase: the typewriter types each line (effect tags included), holds it, then moves on by itself.
	//A click or Space finishes the line being typed, or skips to the next one
	public class DemoDialogue : TextShaderDemoElement
	{
		[Serializable]
		public struct Line
		{
			public string speaker;
			[TextArea] public string text;
			public int side;
			public int portrait;
			public float hold;
		}

		public AllIn1TextTypewriter body;
		public AllIn1TextTypewriter speaker;
		public RectTransform nameTag;
		public float[] nameTagX = new float[0];
		public Image[] sidePortraits = new Image[0];
		public Sprite[] portraits = new Sprite[0];
		public Color listenerColor = new Color(0.45f, 0.45f, 0.5f, 1f);
		//The speaker grows a little and the listener shrinks, from the portrait's bottom edge so the cut stays put
		public float speakerScale = 1.05f;
		public float listenerScale = 0.95f;
		public float portraitScaleTime = 0.15f;
		public CanvasGroup nextIndicator;
		//Above 0 the indicator blinks hard on and off with this period, like an 8 or 16 bit prompt
		public float indicatorBlink;
		public AllIn1DemoScaleTween popOnLine;
		//Plays the body's hide before the next line instead of swapping the text at once
		public bool hideBetweenLines;
		public float hideTimeout = 1.2f;
		//Full while a line types, dimmed once it is typed (a voice meter, a speaking portrait)
		public CanvasGroup talking;
		public float talkingIdleAlpha = 0.25f;
		public Line[] lines = new Line[0];
		//The whole showcase scales up and fades in when shown, scales down and fades out when left
		public float fadeInDuration = 0.3f;
		public float fadeOutDuration = 0.35f;
		public float hiddenScale = 0.85f;

		private int index;
		private float holdTimer;
		private bool playing;
		private bool clearing;
		private float clearTimer;
		private string currentSpeaker;
		private CanvasGroup group;
		private float level;
		private float levelTarget;
		private int speakingSide = -1;
		private Vector2[] portraitBase;
		private float[] portraitScale;
		private float[] portraitVelocity;

		public override void Show(DemoMovementDir dir)
		{
			base.Show(dir);
			if (group == null) { group = GetComponent<CanvasGroup>(); }
			if (group == null) { group = gameObject.AddComponent<CanvasGroup>(); }
			levelTarget = 1f;
			ApplyLevel();
			index = 0;
			currentSpeaker = null;
			clearing = false;
			playing = lines.Length > 0;
			if (playing) { StartLine(); }
			UpdatePortraits(true);
		}

		public override void Hide(DemoMovementDir dir)
		{
			playing = false;
			clearing = false;
			if (nextIndicator != null) { nextIndicator.alpha = 0f; }
			levelTarget = 0f;
			base.Hide(dir);
		}

		//The dialogue loops forever, so it counts as shown at once instead of waiting on a typing line
		public override float GetShowingTransitionProgress()
		{
			return 1f;
		}

		//Left once the fade out ends, so the carousel waits for it
		public override float GetHidingTransitionProgress()
		{
			return 1f - level;
		}

		protected override void Update()
		{
			float duration = levelTarget > level ? fadeInDuration : fadeOutDuration;
			level = Mathf.MoveTowards(level, levelTarget, duration > 0f ? Time.deltaTime / duration : 1f);
			ApplyLevel();

			base.Update();
			UpdatePortraits(false);
			if (!playing) { return; }

			if (clearing)
			{
				clearTimer += Time.deltaTime;
				if (body.IsHidden() || clearTimer >= hideTimeout || Pressed())
				{
					clearing = false;
					index = (index + 1) % lines.Length;
					StartLine();
				}
				return;
			}

			bool typed = body.IsRevealed();
			if (nextIndicator != null)
			{
				bool lit = indicatorBlink <= 0f || Mathf.Repeat(Time.time, indicatorBlink) < indicatorBlink * 0.6f;
				nextIndicator.alpha = typed && lit ? 1f : 0f;
			}
			if (talking != null) { talking.alpha = Mathf.MoveTowards(talking.alpha, typed ? talkingIdleAlpha : 1f, Time.deltaTime * 4f); }

			if (Pressed())
			{
				if (typed) { NextLine(); }
				else { body.RevealImmediate(); }
				return;
			}

			if (typed)
			{
				holdTimer += Time.deltaTime;
				if (holdTimer >= lines[index].hold) { NextLine(); }
			}
		}

		private void NextLine()
		{
			if (hideBetweenLines)
			{
				clearing = true;
				clearTimer = 0f;
				if (nextIndicator != null) { nextIndicator.alpha = 0f; }
				body.Hide();
				return;
			}
			index = (index + 1) % lines.Length;
			StartLine();
		}

		private void StartLine()
		{
			Line line = lines[index];
			holdTimer = 0f;

			//A new speaker's name plays its own reveal; the first one was already revealed by Show
			if (speaker != null && line.speaker != currentSpeaker)
			{
				if (speaker.GetComponent<TMPro.TMP_Text>().text != line.speaker) { speaker.SetText(line.speaker); }
				currentSpeaker = line.speaker;
			}
			if (nameTag != null && line.side >= 0 && line.side < nameTagX.Length)
			{
				nameTag.anchoredPosition = new Vector2(nameTagX[line.side], nameTag.anchoredPosition.y);
			}
			speakingSide = line.side;
			for (int i = 0; i < sidePortraits.Length; i++)
			{
				if (sidePortraits[i] == null) { continue; }
				bool talking = i == line.side;
				if (talking && line.portrait >= 0 && line.portrait < portraits.Length) { sidePortraits[i].sprite = portraits[line.portrait]; }
				sidePortraits[i].color = talking ? Color.white : listenerColor;
			}

			body.SetText(line.text);
			for (int i = 0; i < materialConfigurators.Length; i++) { materialConfigurators[i].UpdateConfiguration(true); }
			if (popOnLine != null) { popOnLine.ScaleUpTween(); }
		}

		private void UpdatePortraits(bool snap)
		{
			if (portraitBase == null || portraitBase.Length != sidePortraits.Length)
			{
				portraitBase = new Vector2[sidePortraits.Length];
				portraitScale = new float[sidePortraits.Length];
				portraitVelocity = new float[sidePortraits.Length];
				for (int i = 0; i < sidePortraits.Length; i++)
				{
					portraitScale[i] = 1f;
					if (sidePortraits[i] != null) { portraitBase[i] = sidePortraits[i].rectTransform.anchoredPosition; }
				}
			}
			for (int i = 0; i < sidePortraits.Length; i++)
			{
				if (sidePortraits[i] == null) { continue; }
				float target = speakingSide < 0 ? 1f : i == speakingSide ? speakerScale : listenerScale;
				if (snap)
				{
					portraitScale[i] = target;
					portraitVelocity[i] = 0f;
				}
				else { portraitScale[i] = Mathf.SmoothDamp(portraitScale[i], target, ref portraitVelocity[i], portraitScaleTime); }
				float s = portraitScale[i];
				RectTransform rt = sidePortraits[i].rectTransform;
				rt.localScale = new Vector3(s, s, 1f);
				rt.anchoredPosition = portraitBase[i] + new Vector2(0f, (s - 1f) * rt.rect.height * rt.pivot.y);
			}
		}

		private void ApplyLevel()
		{
			if (group == null) { return; }
			float eased = Mathf.SmoothStep(0f, 1f, level);
			group.alpha = eased;
			float scale = Mathf.LerpUnclamped(hiddenScale, 1f, eased);
			rectTransform.localScale = new Vector3(scale, scale, 1f);
		}

		private static bool Pressed()
		{
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
			bool click = Input.GetMouseButtonDown(0);
#endif
			return click || AllIn1InputSystem.GetKeyDown(KeyCode.Space);
		}
	}
}
