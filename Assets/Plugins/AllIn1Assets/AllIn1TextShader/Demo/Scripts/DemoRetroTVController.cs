using System;
using System.Collections;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class DemoRetroTVController : TextShaderDemoElement
	{
		public enum TVState
		{
			DISPLAYING_TITLE,
			HIDING_TITLE,
			DISPLAYING_TEXT,
			WAITING_TO_HIDE,
			HIDING_TEXT,
			WAITING_TO_SHOW,
		}

		public Camera cam;
		public Camera camEnvironment;
		public RenderTexture rt;
		public GameObject environment3D;

		public Vector3 camEnvironmentCenterPos;
		public float waitingTimeToHideText = 1f;
		public float waitingTimeToRevealText = 1f;
		public float waitingTimeToHideTitle = 4f;

		private Vector3 camPosFrom;
		private Vector3 camPosTo;

		private float timer;
		private float screenTimer;

		private TVState tvState;

		public AllIn1TextTypewriter typewriterTitle;
		public AllIn1TextTypewriter typewriterText;

		private RenderTexture prewarmTarget;
		private Vector3 prewarmEnvironmentPos;
		private bool prewarmTitleActive;
		private bool prewarmTextActive;

		public override void Show(DemoMovementDir dir)
		{
			base.Show(dir);

			PrepareToShow(dir);

			timer = 0f;
			cam.targetTexture = rt;
			cam.clearFlags = CameraClearFlags.SolidColor;
			cam.backgroundColor = Color.black;
			StartCoroutine(ExecuteAfterAFrame(() =>
			{
				gameObject.SetActive(true);
				environment3D.SetActive(true);
			}));

			screenTimer = 0f;
			tvState = TVState.DISPLAYING_TITLE;

			typewriterText.gameObject.SetActive(false);
			typewriterTitle.gameObject.SetActive(true);
		}

		public override float GetProgressValueToShowThisDemo()
		{
			return 1.0f;
		}

		public override void Hide(DemoMovementDir dir)
		{
			base.Hide(dir);
			PrepareToHide(dir);
		}

		// The TV's texts draw into its render texture, seen on the screen of the 3D set, so the prewarm turns all of it on
		public override void BeginPrewarm()
		{
			prewarmTarget = cam.targetTexture;
			prewarmEnvironmentPos = camEnvironment.transform.localPosition;
			prewarmTitleActive = typewriterTitle.gameObject.activeSelf;
			prewarmTextActive = typewriterText.gameObject.activeSelf;

			cam.targetTexture = rt;
			camEnvironment.transform.localPosition = camEnvironmentCenterPos;
			environment3D.SetActive(true);
			typewriterTitle.gameObject.SetActive(true);
			typewriterText.gameObject.SetActive(true);

			base.BeginPrewarm();

			typewriterTitle.RevealImmediate();
			typewriterText.RevealImmediate();
		}

		public override void EndPrewarm()
		{
			cam.targetTexture = prewarmTarget;
			camEnvironment.transform.localPosition = prewarmEnvironmentPos;
			typewriterTitle.gameObject.SetActive(prewarmTitleActive);
			typewriterText.gameObject.SetActive(prewarmTextActive);

			base.EndPrewarm();
		}

		protected override void Update()
		{
			base.Update();

			UpdateScreenLoop();
		}

		private void UpdateScreenLoop()
		{
			AllIn1TextTypewriter typewriter = typewriters[0];

			if (tvState == TVState.DISPLAYING_TITLE)
			{
				screenTimer += Time.deltaTime;
				if (screenTimer >= waitingTimeToHideTitle)
				{
					typewriterTitle.Hide();
					tvState = TVState.HIDING_TITLE;
				}
			}
			else if (tvState == TVState.HIDING_TITLE)
			{
				if (typewriterTitle.IsHidden())
				{
					typewriterTitle.gameObject.SetActive(false);
					typewriterText.gameObject.SetActive(true);
					typewriterText.HideImmediate();
					typewriterText.Reveal();

					tvState = TVState.DISPLAYING_TEXT;
				}
			}
			else if (tvState == TVState.DISPLAYING_TEXT)
			{
				if (typewriter.IsRevealed())
				{
					tvState = TVState.WAITING_TO_HIDE;
					screenTimer = 0f;
				}
			}
			else if (tvState == TVState.WAITING_TO_HIDE)
			{
				screenTimer += Time.deltaTime;
				if (screenTimer >= waitingTimeToHideText)
				{
					screenTimer = 0f;
					tvState = TVState.HIDING_TEXT;
					typewriter.Hide();
				}
			}
			else if (tvState == TVState.HIDING_TEXT)
			{
				if (typewriter.IsHidden())
				{
					screenTimer = 0f;
					tvState = TVState.WAITING_TO_SHOW;
				}
			}
			else if (tvState == TVState.WAITING_TO_SHOW)
			{
				screenTimer += Time.deltaTime;
				if (screenTimer >= waitingTimeToRevealText)
				{
					screenTimer = 0f;
					tvState = TVState.DISPLAYING_TEXT;
					typewriter.Reveal();
				}
			}
		}

		protected override void UpdateShowing(float deltaTime)
		{
			MoveEnvironmentCamera();

			float transitionProgress = GetTransitionProgressNormalized();
			if (transitionProgress >= 1f)
			{
				SetState(State.SHOWN);
			}
		}

		protected override void UpdateHiding(float deltaTime)
		{
			MoveEnvironmentCamera();

			float transitionProgress = GetTransitionProgressNormalized();
			if (transitionProgress >= 1f)
			{
				timer = 0f;
				state = State.HIDDEN;

				cam.targetTexture = null;
				cam.clearFlags = CameraClearFlags.Depth;
			}
		}

		protected override void SetState(State newState)
		{
			base.SetState(newState);

			switch (newState)
			{
				case State.SHOWN:
					timer = 0f;
					break;
			}
		}

		private void PrepareToShow(DemoMovementDir dir)
		{
			switch (dir)
			{
				case DemoMovementDir.LEFT:
					camPosFrom = camEnvironmentCenterPos + Vector3.right * 15f;
					break;
				case DemoMovementDir.RIGHT:
					camPosFrom = camEnvironmentCenterPos - Vector3.right * 15f;
					break;
			}

			state = State.SHOWING;
			camPosTo = camEnvironmentCenterPos;
			timer = 0f;
		}

		private void PrepareToHide(DemoMovementDir dir)
		{
			switch (dir)
			{
				case DemoMovementDir.LEFT:
					camPosTo = camEnvironmentCenterPos - Vector3.right * 15f;
					break;
				case DemoMovementDir.RIGHT:
					camPosTo = camEnvironmentCenterPos + Vector3.right * 15f;
					break;
			}

			state = State.HIDING;
			camPosFrom = camEnvironmentCenterPos;
			timer = 0f;
		}

		private float MoveEnvironmentCamera()
		{
			float t = GetShowingTransitionProgress();
			timer += Time.deltaTime;

			Vector3 newLocalPos = Vector3.Lerp(camPosFrom, camPosTo, t);
			camEnvironment.transform.localPosition = newLocalPos;

			return t;
		}

		public override float GetShowingTransitionProgress()
		{
			float res = timer / 0.25f;
			return res;
		}

		public override float GetHidingTransitionProgress()
		{
			float res = timer / 0.25f;
			return res;
		}

		private IEnumerator ExecuteAfterAFrame(Action action)
		{
			yield return null;

			if (action != null)
			{
				action();
			}
		}

		protected override void DemoHidden()
		{
			base.DemoHidden();

			environment3D.SetActive(false);
		}
	}
}