using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class TextShaderDemoElementsController : MonoBehaviour
	{
		[Header("Demo Scene Configurator")]
		public DemoSceneConfigurator demoSceneConfigurator;


		[Header("Demo Elements")]
		public TextShaderDemoElement[] demoElements;

		[Header("Config")]
		public float lerpSpeed;
		private DemoTextShaderUI ui;
		private int currentDemoIndex;
		// The showcase the carousel is on or heading to; shownDemoElement is the one Show was called on, null while it waits for the others to leave
		private TextShaderDemoElement currentDemoElement;
		private TextShaderDemoElement shownDemoElement;
		private readonly List<TextShaderDemoElement> hidingElements = new List<TextShaderDemoElement>();
		private bool isTransitioning;
		private DemoMovementDir transitionDir;

		[Header("Postprocess controller")]
		public PostprocessController postprocessController;

		[Header("Prewarm")]
		// Draws every showcase once behind a cover before the first one shows, so browsing never stalls on a shader; in the editor it first waits for the compiler
		public bool prewarmOnStart = true;
		private TextShaderDemoPrewarmer prewarmer;
		private Coroutine prewarmRoutine;

		public bool IsPrewarming => prewarmer != null;

		public void Init(DemoTextShaderUI ui)
		{
			this.ui = ui;

			for (int i = 0; i < demoElements.Length; i++)
			{
				demoElements[i].gameObject.SetActive(true);
				demoElements[i].Init(this, lerpSpeed);

				if (i > 0)
				{
					demoElements[i].gameObject.SetActive(false);
				}
			}

			currentDemoIndex = 0;
			currentDemoElement = demoElements[0];
			shownDemoElement = currentDemoElement;
			hidingElements.Clear();

			TMPro.TMP_FontAsset font = ui.txtDemoName != null ? ui.txtDemoName.font : null;
			if (!TextMaterialConfigurator.AreBuffersSupported())
			{
				TextShaderDemoPrewarmer.ShowPlainTextNotice(font);
			}

			if (prewarmOnStart && Application.isPlaying)
			{
				prewarmer = new TextShaderDemoPrewarmer(this, font);
				prewarmRoutine = StartCoroutine(PrewarmThenStart());
			}
			else
			{
				StartCarousel();
			}
		}

		private IEnumerator PrewarmThenStart()
		{
			yield return prewarmer.Sweep();

			TextShaderDemoPrewarmer finished = prewarmer;
			prewarmer = null;
			prewarmRoutine = null;
			StartCarousel();

			yield return finished.FadeOutCover();
		}

		// Starts the carousel at once, for tools that jump to a showcase on the first frames
		public void SkipPrewarm()
		{
			if (prewarmer == null) return;

			StopCoroutine(prewarmRoutine);
			prewarmer.Abort();
			prewarmer = null;
			prewarmRoutine = null;
			StartCarousel();
		}

		private void StartCarousel()
		{
			postprocessController.ApplyConfig(currentDemoElement.GetPostprocessConfig());
			currentDemoElement.Show(DemoMovementDir.LEFT);

			demoSceneConfigurator.ApplyCameraConfiguration(currentDemoElement.camerasConfiguration);

			isTransitioning = true;

			ui.RefreshUI();
		}

		private void ClampDemoIndex()
		{
			if (demoElements == null) return;

			if (currentDemoIndex >= demoElements.Length)
			{
				currentDemoIndex = 0;
			}
			else if (currentDemoIndex < 0)
			{
				currentDemoIndex = demoElements.Length - 1;
			}
		}

		public void NextDemo()
		{
			Navigate(1, DemoMovementDir.RIGHT);
		}

		public void PreviousDemo()
		{
			Navigate(-1, DemoMovementDir.LEFT);
		}

		// Works at any moment, even mid transition: the showcase on screen starts leaving and the new target shows once every leaving one is hidden enough.
		// Targets passed over by quick presses never show
		private void Navigate(int step, DemoMovementDir dir)
		{
			if (!CanNavigate()) return;

			currentDemoIndex += step;
			ClampDemoIndex();
			currentDemoElement = demoElements[currentDemoIndex];
			transitionDir = dir;

			if (shownDemoElement != null && shownDemoElement != currentDemoElement)
			{
				shownDemoElement.Hide(dir);
				if (!hidingElements.Contains(shownDemoElement))
				{
					hidingElements.Add(shownDemoElement);
				}
				shownDemoElement = null;
			}

			postprocessController.GoTo(currentDemoElement.GetPostprocessConfig());

			isTransitioning = true;

			ui.RefreshUI();
		}

		private void Update()
		{
			if (isTransitioning)
			{
				UpdateTransitioning();
			}
		}

		private void UpdateTransitioning()
		{
			hidingElements.RemoveAll(e => e == null || e.IsHidden());

			if (shownDemoElement == null)
			{
				if (AreLeavingElementsHiddenEnough())
				{
					// A showcase still on its way out is shown again from where it is
					hidingElements.Remove(currentDemoElement);
					currentDemoElement.Show(transitionDir);
					shownDemoElement = currentDemoElement;

					demoSceneConfigurator.ApplyCameraConfiguration(currentDemoElement.camerasConfiguration);
				}
			}
			else if (shownDemoElement.IsShown() && hidingElements.Count == 0)
			{
				isTransitioning = false;
			}
		}

		private bool AreLeavingElementsHiddenEnough()
		{
			for (int i = 0; i < hidingElements.Count; i++)
			{
				TextShaderDemoElement element = hidingElements[i];
				if (element != currentDemoElement && !element.IsHiddenEnough(element.GetProgressValueToShowThisDemo()))
				{
					return false;
				}
			}

			return true;
		}

		private bool CanNavigate()
		{
			return demoElements != null && demoElements.Length > 1 && currentDemoElement != null && prewarmer == null;
		}

		public TextShaderDemoElement GetCurrentDemoElement()
		{
			return currentDemoElement;
		}

		public Vector3 GetLeftOutPosition()
		{
			Vector3 res = new Vector3(-900f, 0f, 0f);
			return res;
		}

		public Vector3 GetRightOutPosition()
		{
			Vector3 res = new Vector3(900f, 0f, 0f);
			return res;
		}
	}
}
