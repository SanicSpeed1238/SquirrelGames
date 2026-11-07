using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class TextShaderDemoElement : MonoBehaviour
	{
		private const string TAG = "TextShaderDemoElement";

		public enum State
		{
			SHOWING,
			SHOWN,
			HIDING,
			HIDDEN,
		}

		private TextShaderDemoElementsController demoElementsController;
		private DemoElementMovementState movementState;
		private UIElementMovement uiElementMovement;
		private float lerpSpeed;

		protected State state;

		public TextShaderTweenController[] tweens;
		public RectTransform rectTransform;
		public TextMaterialConfiguratorMaster[] materialConfigurators;
		public AllIn1TextTypewriter[] typewriters;

		public bool moveAwayOnHide = true;

#if UNITY_EDITOR
		public bool drawGizmos;
#endif
		
		private bool initialized;

		[Header("Config")]
		public string demoName;
		[TextArea] public string demoDescription;

		public PostprocessConfigSO postprocessConfigSO;
		public DemoCamerasConfiguration camerasConfiguration;

		public void Init(TextShaderDemoElementsController demoElementsController, float lerpSpeed)
		{
			this.demoElementsController = demoElementsController;
			this.lerpSpeed = lerpSpeed;

			this.uiElementMovement = new UIElementMovement(rectTransform, lerpSpeed);

			UseRuntimeMaterialCopies();

			for(int i = 0; i < materialConfigurators.Length; i++)
			{
				materialConfigurators[i].Init();
			}

			for(int i = 0; i < typewriters.Length; i++)
			{
				typewriters[i].Init();
			}

			for(int i = 0; i < tweens.Length; i++)
			{
				tweens[i].Init();
			}

			initialized = true;
			state = State.HIDDEN;
		}

		//Tweens and the toolkit write to a text's material while playing, and a .mat asset keeps those values after Play: texts on an asset get their own copy
		//first (before the tweens cache their material). Scene embedded materials reset on their own, and builds can't change assets
		private void UseRuntimeMaterialCopies()
		{
#if UNITY_EDITOR
			foreach (TMPro.TMP_Text text in GetComponentsInChildren<TMPro.TMP_Text>(true))
			{
				Material material = text.fontSharedMaterial;
				if (material == null || !UnityEditor.AssetDatabase.Contains(material)) { continue; }
				text.fontSharedMaterial = new Material(material) { name = material.name + " (Play)" };
			}
#endif
		}

		public void Reveal()
		{
			for (int i = 0; i < typewriters.Length; i++)
			{
				typewriters[i].Reveal();
			}

			for(int i = 0; i < materialConfigurators.Length; i++)
			{
				materialConfigurators[i].UpdateConfiguration();
			}

			for (int i = 0; i < typewriters.Length; i++)
			{
				typewriters[i].Update();
			}
		}

		// For the carousel's prewarm: the showcase whole in its place, every text revealed at once so each material really draws
		public virtual void BeginPrewarm()
		{
			gameObject.SetActive(true);

			if (moveAwayOnHide)
			{
				rectTransform.anchoredPosition3D = Vector3.zero;
			}

			for (int i = 0; i < typewriters.Length; i++)
			{
				typewriters[i].RevealImmediate();
			}

			for (int i = 0; i < materialConfigurators.Length; i++)
			{
				materialConfigurators[i].UpdateConfiguration();
			}

			for (int i = 0; i < typewriters.Length; i++)
			{
				typewriters[i].Update();
			}

			// Not hidden, so neither LateUpdate (this one or a subclass's) deactivates it before it draws
			state = State.SHOWN;
		}

		public virtual void EndPrewarm()
		{
			state = State.HIDDEN;
			DemoHidden();
		}

		public virtual void Show(DemoMovementDir dir)
		{
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}

			Reveal();

			if (moveAwayOnHide)
			{
				PrepareBeforeShow(dir);
				StartTransitionIn();
			}

			state = State.SHOWING;
		}

		public virtual void Hide(DemoMovementDir dir)
		{
			for (int i = 0; i < typewriters.Length; i++)
			{
				typewriters[i].Hide();
			}

			if (moveAwayOnHide)
			{
				StartTransitionOut(dir);
			}

			state = State.HIDING;
		}

		public virtual float GetProgressValueToShowThisDemo()
		{
			return 0.5f;
		}

		public bool IsHiddenEnough(float valueReference)
		{
			bool res = GetTransitionProgressNormalized() >= valueReference;
			return res;
		}

		public bool IsHidden()
		{
			bool res = state == State.HIDDEN;
			return res;
		}

		public bool IsShown()
		{
			bool res = state == State.SHOWN;
			return res;
		}

		protected virtual void Update()
		{
			if (!initialized)
			{
				return;
			}

			float deltaTime = Time.deltaTime;

			switch (state)
			{
				case State.SHOWING:
					UpdateShowing(deltaTime);
					break;
				case State.HIDING:
					UpdateHiding(deltaTime);
					break;
			}

			for(int i = 0; i < tweens.Length; i++)
			{
				tweens[i].Update(deltaTime);
			}
		}

		private void LateUpdate()
		{
			if (IsHidden() && gameObject.activeSelf)
			{
				DemoHidden();
			}
		}

		protected virtual void DemoHidden()
		{
			gameObject.SetActive(false);
		}

		protected virtual void UpdateShowing(float deltaTime)
		{
			if (moveAwayOnHide)
			{
				uiElementMovement.Update(deltaTime);
			}

			float transitionProgress = GetTransitionProgressNormalized();
			if(transitionProgress >= 1f)
			{
				SetState(State.SHOWN);
			}
		}

		protected virtual void UpdateHiding(float deltaTime)
		{
			if (moveAwayOnHide)
			{
				uiElementMovement.Update(deltaTime);
			}

			float transitionProgress = GetTransitionProgressNormalized();
			if (transitionProgress >= 1f)
			{
				state = State.HIDDEN;
			}
		}

		protected virtual void SetState(State newState)
		{
			this.state = newState; 
		}

		protected virtual float GetTransitionProgressNormalized()
		{
			float res = 0f;
			if (moveAwayOnHide)
			{
				res = uiElementMovement.GetProgressNormalized();
			}
			else
			{
				switch (state)
				{
					case State.SHOWING:
						res = GetShowingTransitionProgress();
						break;
					case State.HIDING:
						res = GetHidingTransitionProgress();
						break;
					case State.HIDDEN:
						res = 1.0f;
						break;
				}
			}

			return res;
		}

		public virtual float GetShowingTransitionProgress()
		{
			float res = 0f;
			
			for (int i = 0; i < typewriters.Length; i++)
			{
				res += typewriters[i].GetRevealProgressNormalized();
			}

			res = res / typewriters.Length;

			return res;
		}

		public virtual float GetHidingTransitionProgress()
		{
			float res = 0f;

			for (int i = 0; i < typewriters.Length; i++)
			{
				res += typewriters[i].GetHideProgressNormalized();
			}

			res = res / typewriters.Length;

			return res;
		}


		public void StartTransitionIn()
		{
			MoveToCenter();
		}

		public void StartTransitionOut(DemoMovementDir dir)
		{
			switch (dir)
			{
				case DemoMovementDir.LEFT:
					MoveToLeft();
					break;
				case DemoMovementDir.RIGHT:
					MoveToRight();
					break;
			}
		}

		public void MoveToLeft()
		{
			this.uiElementMovement.StartMoving(rectTransform.anchoredPosition3D, demoElementsController.GetLeftOutPosition(), lerpSpeed, true);
		}

		public void MoveToRight()
		{
			this.uiElementMovement.StartMoving(rectTransform.anchoredPosition3D, demoElementsController.GetRightOutPosition(), lerpSpeed, true);
		}

		public void MoveToCenter()
		{
			this.uiElementMovement.StartMoving(rectTransform.anchoredPosition3D, Vector3.zero, lerpSpeed, false);
		}

		public void PrepareBeforeShow(DemoMovementDir dir)
		{
			if(dir == DemoMovementDir.LEFT)
			{
				PlaceOutRight();
			}
			else
			{
				PlaceOutLeft();
			}
		}

		public void PlaceOutLeft()
		{
			rectTransform.anchoredPosition3D = demoElementsController.GetLeftOutPosition();
		}

		public void PlaceOutRight()
		{
			rectTransform.anchoredPosition3D = demoElementsController.GetRightOutPosition();
		}

		public PostprocessConfig GetPostprocessConfig()
		{
#if ALLIN1_BIRP
			return postprocessConfigSO.postprocessConfig;
#else
			return postprocessConfigSO.postprocessConfigURP;
#endif
		}
	}
}