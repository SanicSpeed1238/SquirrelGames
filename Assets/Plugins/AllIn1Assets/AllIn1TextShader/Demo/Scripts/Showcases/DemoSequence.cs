using System;
using AllIn1Assets;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//A showcase that plays Show and Hide calls on a looping timeline: a step's spline travels in and out, or without a spline its typewriter reveals and hides,
	//or with neither its frame fades in and out.
	//Showing it fades in fadeInOnShow; leaving it stops the timeline, sends every text out and fades fadeOnHide
	//Late in the frame order, so the hold on waiting texts lands after the toolkit's own updates
	[DefaultExecutionOrder(1000)]
	public class DemoSequence : TextShaderDemoElement
	{
		[Serializable]
		public struct Step
		{
			public AllIn1TextSpline spline;
			public AllIn1TextTypewriter typewriter;
			public DemoFrameFader fader;
			public float showAt;
			public float hideAt;
		}

		public Step[] steps = new Step[0];
		public float loopLength = 4f;
		public DemoFrameFader[] fadeInOnShow = new DemoFrameFader[0];
		public DemoFrameFader[] fadeOnHide = new DemoFrameFader[0];

		private float time;
		private bool playing;
		//A typewriter step not yet revealed this visit stays out of Hide: IsHidden reads the last hide, which on a first visit never ran
		private bool[] started = new bool[0];

		public override void Show(DemoMovementDir dir)
		{
			if (!gameObject.activeSelf) { gameObject.SetActive(true); }
			for (int i = 0; i < materialConfigurators.Length; i++) { materialConfigurators[i].UpdateConfiguration(); }
			started = new bool[steps.Length];
			foreach (Step step in steps)
			{
				if (step.spline != null) { step.spline.HideImmediate(); }
				else if (step.typewriter != null) { step.typewriter.HideImmediate(); }
			}
			foreach (DemoFrameFader fader in fadeInOnShow)
			{
				if (fader != null) { fader.FadeIn(); }
			}
			time = 0f;
			playing = true;
			SetState(State.SHOWING);
		}

		public override void Hide(DemoMovementDir dir)
		{
			playing = false;
			for (int i = 0; i < steps.Length; i++)
			{
				Step step = steps[i];
				if (step.spline != null) { step.spline.Hide(); }
				else if (step.typewriter != null && Started(i) && !step.typewriter.IsHidden()) { step.typewriter.Hide(); }
				else if (step.fader != null) { step.fader.FadeOut(); }
			}
			foreach (DemoFrameFader fader in fadeOnHide)
			{
				if (fader != null) { fader.FadeOut(); }
			}
			SetState(State.HIDING);
		}

		public override float GetShowingTransitionProgress()
		{
			return 1f;
		}

		//Hidden once every text has finished its exit
		public override float GetHidingTransitionProgress()
		{
			for (int i = 0; i < steps.Length; i++)
			{
				Step step = steps[i];
				if (step.spline != null && (step.spline.IsShown || step.spline.IsTraveling)) { return 0f; }
				if (step.spline == null && step.typewriter != null && Started(i) && !step.typewriter.IsHidden()) { return 0f; }
			}
			return 1f;
		}

		private bool Started(int i)
		{
			return i >= started.Length || started[i];
		}

		protected override void Update()
		{
			base.Update();
			if (!playing) { return; }

			float from = time;
			time += Time.deltaTime;
			Fire(from, time);
			KeepWaitingTextsHidden();
			while (time >= loopLength)
			{
				time -= loopLength;
				Fire(-1f, time);
			}
		}

		//Unity calls only the most derived LateUpdate, and the base class's private one is what deactivates a hidden showcase, so this one does that too
		private void LateUpdate()
		{
			if (playing) { KeepWaitingTextsHidden(); }
			if (IsHidden() && gameObject.activeSelf) { DemoHidden(); }
		}

		//Texts waiting for their step stay in their reveal's start state. On the carousel's first showcase after Play, a mesh cache rebuild at the end of the
		//first frame restored them whole (a stopped typewriter never reapplies its hide), so they showed until their step; this puts the start state back silently
		private void KeepWaitingTextsHidden()
		{
			for (int i = 0; i < steps.Length && i < started.Length; i++)
			{
				AllIn1TextTypewriter tw = steps[i].typewriter;
				if (started[i] || steps[i].spline != null || tw == null || tw.meshEffectDispatcher == null) { continue; }
				tw.meshEffectDispatcher.Reveal();
				tw.meshEffectDispatcher.Update(0f);
				tw.WriteChangesToMesh();
			}
		}

		private void Fire(float from, float to)
		{
			for (int i = 0; i < steps.Length; i++)
			{
				Step step = steps[i];
				bool show = from < step.showAt && step.showAt <= to;
				bool hide = from < step.hideAt && step.hideAt <= to;
				if (step.spline != null)
				{
					if (show) { step.spline.Show(); }
					if (hide) { step.spline.Hide(); }
				}
				else if (step.typewriter != null)
				{
					if (show) { step.typewriter.Reveal(); if (i < started.Length) { started[i] = true; } }
					if (hide) { step.typewriter.Hide(); }
				}
				else if (step.fader != null)
				{
					if (show) { step.fader.FadeIn(); }
					if (hide) { step.fader.FadeOut(); }
				}
			}
		}
	}
}
