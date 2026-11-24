#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AllIn1Assets.AllIn13DShader
{
    public class AllIn13dShaderDemoController : MonoBehaviour
    {
		public DemoExpositor CurrentExpositor
		{
			get
			{
				return demoExpositors[currentExpositorIndex];
			}
		}

		public DemoElement CurrentDemoElement
		{
			get
			{
				return CurrentExpositor.CurrentDemoElement;
			}
		}


		public DemoSceneConfiguration demoSceneConfig;
		public DemoUI demoUI;
		public Transform initPosition;

		[SerializeField] private SkyController skyController;
		[SerializeField] private AllIn1MainLightController mainLightController;

		[SerializeField] private DemoExpositor[] demoExpositors;
		[SerializeField] private GameObject expositorNameGo;

		[SerializeField] private DemoCameraController cameraController;

		[SerializeField] private GameObject environmentsParentGo;


		//private DemoElement currentDemoElement;
		private DemoElement previousDemoElement;

        [Space, Header("Start Index")]
        [SerializeField] private int startExpositorIndex;
        [SerializeField] private int startDemoIndex;

        [Space, Header("Event Systems")]
        [SerializeField] private EventSystem oldInputEventSystem;
        [SerializeField] private EventSystem newInputEventSystem;

        [Space, Header("Input Handlers")]
		[SerializeField] private InputHandler showDemoInfoHandler;
		
		[Space, Header("Scale Tweens")]
		[SerializeField] private AllIn1DemoScaleTween demoLabelScaleTween;
		[SerializeField] private AllIn1DemoScaleTween expositorLabelScaleTween;
		[SerializeField] private AllIn1DemoScaleTween infoPanelScaleTween;

		[Space, Header("Prewarm")]
		//Draws every demo once behind a loading bar before the first one shows, so browsing never stalls on a shader
		public bool prewarmOnStart = true;
		private AllIn13DShaderDemoPrewarmer prewarmer;
		private DemoElement prewarmedElement;
		private readonly Dictionary<DemoEnvironment, GameObject> prewarmEnvironments = new Dictionary<DemoEnvironment, GameObject>();
		private GameObject prewarmFadingEnvironment;
		private readonly List<Material> prewarmFadingMaterials = new List<Material>();
		private Vector3 cameraPositionBeforePrewarm;
		private Quaternion cameraRotationBeforePrewarm;

		public bool IsPrewarming => prewarmer != null;

		private int currentExpositorIndex;
        private Vector3 targetCameraPosition;

		private EnvironmentController currentEnvironmentController;
		private bool moveCurrentEnvironment;

        private void Start()
        {
			for(int i = 0; i < demoExpositors.Length; i++)
			{
				demoExpositors[i].Init(demoSceneConfig);
			}
			
			int childCount = environmentsParentGo.transform.childCount;
			for(int i = childCount - 1; i >= 0; i--) Destroy(environmentsParentGo.transform.GetChild(i).gameObject);

            EnableCorrectInputSystem();
            demoExpositors[startExpositorIndex].SetDemoExpositor(startDemoIndex);

			demoUI.Refresh(CurrentExpositor);

			previousDemoElement = null;

			skyController.Init();

			cameraController.Init(demoSceneConfig);

			expositorNameGo.SetActive(demoExpositors.Length > 1);

			if (prewarmOnStart)
			{
				prewarmer = new AllIn13DShaderDemoPrewarmer(this, demoUI.txtDemoName != null ? demoUI.txtDemoName.font : null);
				StartCoroutine(PrewarmThenStart());
			}
			else
			{
				StartCarousel();
			}
        }

		private IEnumerator PrewarmThenStart()
		{
			yield return prewarmer.Sweep();

			AllIn13DShaderDemoPrewarmer finished = prewarmer;
			prewarmer = null;
			StartCarousel();

			yield return finished.FadeOutCover();
		}

		private void StartCarousel()
		{
			DemoChanged();

			CurrentDemoElement.Show();
		}

		public List<DemoElement> GetAllDemoElements()
		{
			List<DemoElement> res = new List<DemoElement>();
			for (int i = 0; i < demoExpositors.Length; i++)
			{
				res.AddRange(demoExpositors[i].DemoElements);
			}
			return res;
		}

		public void BeginPrewarm()
		{
			cameraPositionBeforePrewarm = cameraController.target.position;
			cameraRotationBeforePrewarm = cameraController.target.rotation;
		}

		//The demo whole under the camera, with its environment, light, sky and post process, as DemoChanged shows it
		public void ShowForPrewarm(DemoElement demoElement)
		{
			ClearPrewarmFadingEnvironment();

			if (prewarmedElement != null)
			{
				prewarmedElement.EndPrewarm();
			}
			prewarmedElement = demoElement;

			DemoElementData data = demoElement.demoElementData;
			mainLightController.DemoChanged(data);
			skyController.DemoChanged(data);
			cameraController.SnapTo(demoElement.transform.position, Quaternion.identity);
			cameraController.ApplyPostProcess(data);

			foreach (GameObject environmentGo in prewarmEnvironments.Values)
			{
				environmentGo.SetActive(false);
			}

			if (data.environment != null)
			{
				if (!prewarmEnvironments.TryGetValue(data.environment, out GameObject environmentGo))
				{
					environmentGo = Instantiate(data.environment.prefab, environmentsParentGo.transform);
					prewarmEnvironments.Add(data.environment, environmentGo);
				}

				environmentGo.transform.SetPositionAndRotation(demoElement.transform.position, Quaternion.identity);
				environmentGo.transform.localScale = Vector3.one;
				environmentGo.SetActive(true);
			}

			demoElement.BeginPrewarm();
		}

		public List<DemoEnvironment> GetPrewarmEnvironments()
		{
			return new List<DemoEnvironment>(prewarmEnvironments.Keys);
		}

		//EnvironmentController.Hide fades an environment out with transparent materials, a variant of its own on Built-in, so a copy of each draws once that way
		public void ShowEnvironmentFadeForPrewarm(DemoEnvironment environment)
		{
			ClearPrewarmFadingEnvironment();

			foreach (GameObject environmentGo in prewarmEnvironments.Values)
			{
				environmentGo.SetActive(false);
			}

			prewarmFadingEnvironment = Instantiate(environment.prefab, environmentsParentGo.transform);
			prewarmFadingEnvironment.transform.SetPositionAndRotation(cameraController.target.position, Quaternion.identity);
			prewarmFadingEnvironment.transform.localScale = Vector3.one;
			//Some environment prefabs are saved inactive; Show activates them in the carousel
			prewarmFadingEnvironment.SetActive(true);

			EnvironmentController environmentController = prewarmFadingEnvironment.GetComponent<EnvironmentController>();
			if (environmentController == null || environmentController.renderers == null) return;

			for (int i = 0; i < environmentController.renderers.Length; i++)
			{
				if (environmentController.renderers[i] == null) continue;

				Material fadingMaterial = environmentController.renderers[i].material;
				DemoUtils.SetMaterialTransparent(fadingMaterial);
				prewarmFadingMaterials.Add(fadingMaterial);
			}
		}

		private void ClearPrewarmFadingEnvironment()
		{
			if (prewarmFadingEnvironment != null)
			{
				Destroy(prewarmFadingEnvironment);
				prewarmFadingEnvironment = null;
			}

			for (int i = 0; i < prewarmFadingMaterials.Count; i++)
			{
				Destroy(prewarmFadingMaterials[i]);
			}
			prewarmFadingMaterials.Clear();
		}

		public void EndPrewarm()
		{
			ClearPrewarmFadingEnvironment();

			if (prewarmedElement != null)
			{
				prewarmedElement.EndPrewarm();
				prewarmedElement = null;
			}

			foreach (GameObject environmentGo in prewarmEnvironments.Values)
			{
				Destroy(environmentGo);
			}
			prewarmEnvironments.Clear();

			cameraController.SnapTo(cameraPositionBeforePrewarm, cameraRotationBeforePrewarm);
		}

        public void NavigateToNextExpositor()
        {
	        if (IsPrewarming) return;

	        previousDemoElement = CurrentDemoElement;
	        Vector3 nextDemoPos = previousDemoElement.transform.position + cameraController.cam.transform.up * demoSceneConfig.distanceBetweenDemos;

	        CurrentDemoElement.Hide();
	        ChangeExpositor(1);

	        moveCurrentEnvironment = false;

	        CurrentDemoElement.transform.position = nextDemoPos;
	        CurrentDemoElement.Show();

	        expositorLabelScaleTween.ScaleDownTween();
	        infoPanelScaleTween.ScaleUpTween();
	        DemoChanged();
        }

        public void NavigateToPreviousExpositor()
        {
	        if (IsPrewarming) return;

	        previousDemoElement = CurrentDemoElement;
	        Vector3 nextDemoPos = previousDemoElement.transform.position - cameraController.cam.transform.up * demoSceneConfig.distanceBetweenDemos;

	        CurrentDemoElement.Hide();
	        ChangeExpositor(-1);

	        moveCurrentEnvironment = false;

	        CurrentDemoElement.transform.position = nextDemoPos;
	        CurrentDemoElement.Show();

	        expositorLabelScaleTween.ScaleDownTween();
	        infoPanelScaleTween.ScaleUpTween();
	        DemoChanged();
        }
        
        public void NavigateToPreviousDemo()
        {
	        if (IsPrewarming) return;

	        previousDemoElement = CurrentDemoElement;

	        DemoElement nextDemoElement = demoExpositors[currentExpositorIndex].GetPreviousDemoElement();
	        Vector3 nextDemoPos = GetNextDemoPos(previousDemoElement, nextDemoElement, -cameraController.cam.transform.right);

	        moveCurrentEnvironment = previousDemoElement.demoElementData.environment == nextDemoElement.demoElementData.environment;

	        demoExpositors[currentExpositorIndex].PreviousDemoElement(nextDemoPos);
	        demoLabelScaleTween.ScaleDownTween();
	        infoPanelScaleTween.ScaleUpTween();
	        DemoChanged();
        }

        public void NavigateToNextDemo()
        {
	        if (IsPrewarming) return;

	        previousDemoElement = CurrentDemoElement;

	        DemoElement nextDemoElement = demoExpositors[currentExpositorIndex].GetNextDemoElement();
	        Vector3 nextDemoPos = GetNextDemoPos(previousDemoElement, nextDemoElement, cameraController.cam.transform.right);

	        moveCurrentEnvironment = previousDemoElement.demoElementData.environment == nextDemoElement.demoElementData.environment;

	        demoExpositors[currentExpositorIndex].NextDemoElement(nextDemoPos);
	        demoLabelScaleTween.ScaleDownTween();
	        infoPanelScaleTween.ScaleUpTween();
	        DemoChanged();
        }
        

		private void LateUpdate()
		{
			if (moveCurrentEnvironment && currentEnvironmentController != null)
			{
				currentEnvironmentController.transform.position += cameraController.GetDeltaMovement();
			}
		}

		private void DemoChanged()
		{
			demoUI.Refresh(CurrentExpositor);

			mainLightController.DemoChanged(CurrentDemoElement.demoElementData);
			skyController.DemoChanged(CurrentDemoElement.demoElementData);

			cameraController.DemoChanged(CurrentDemoElement);

			CheckCurrentEnvironment();
		}

		private Vector3 GetNextDemoPos(DemoElement previousDemo, DemoElement nextDemo, Vector3 dir)
		{
			//Vector3 res = previousDemo.transform.position;

			//if(previousDemo.demoElementData.environment != nextDemo.demoElementData.environment)
			//{
			//	res += dir * demoSceneConfig.distanceBetweenDemos;
			//}

			Vector3 res = previousDemo.transform.position + dir * demoSceneConfig.distanceBetweenDemos;
			return res;
		}

		private void CheckCurrentEnvironment()
		{
			DemoEnvironment currentEnvironment = CurrentDemoElement.demoElementData.environment;

			DemoEnvironment previousEnvironment = previousDemoElement?.demoElementData.environment;

			if(currentEnvironment != previousEnvironment)
			{
				if (currentEnvironmentController != null)
				{
					currentEnvironmentController.Hide();
				}

				if(currentEnvironment != null)
				{
					ChangeEnvironment(currentEnvironment);
				}

				moveCurrentEnvironment = false;
			}
			else
			{
				moveCurrentEnvironment = true;
			}
		}

		private void ChangeEnvironment(DemoEnvironment environment)
		{
			GameObject newEnvironmentGo = Instantiate(environment.prefab);
			newEnvironmentGo.transform.parent = environmentsParentGo.transform;
			newEnvironmentGo.transform.position = CurrentDemoElement.transform.position;
			newEnvironmentGo.transform.rotation = Quaternion.identity;
			newEnvironmentGo.transform.localScale = Vector3.zero;


			currentEnvironmentController = newEnvironmentGo.GetComponent<EnvironmentController>();
			currentEnvironmentController.Init(demoSceneConfig);

			currentEnvironmentController.Show(CurrentDemoElement);
		}

#if UNITY_EDITOR
		[ContextMenu("Setup Demo Scene")]
		public void Setup()
		{
			RemoveOldElements();

			for(int i = 0; i < demoSceneConfig.expositors.Length; i++)
			{
				DemoExpositorData expositorData = demoSceneConfig.expositors[i];

				GameObject goExpositor = new GameObject($"Expositor {i + 1}");
				goExpositor.transform.parent = transform;
				goExpositor.transform.localPosition = new Vector3(0f, i * -25f, 0f);
				goExpositor.transform.localRotation = Quaternion.identity;

				DemoExpositor expositor = goExpositor.AddComponent<DemoExpositor>();
				expositor.Setup(expositorData, demoSceneConfig);

				ArrayUtility.Add(ref demoExpositors, expositor);
			}

			currentExpositorIndex = 0;

			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		private void RemoveOldElements()
		{
			DemoExpositor[] expositorsToDestroy = transform.GetComponentsInChildren<DemoExpositor>();
			for(int i = 0; i < expositorsToDestroy.Length; i++)
			{
				DestroyImmediate(expositorsToDestroy[i].gameObject);
			}

			ArrayUtility.Clear(ref demoExpositors);
		}
#endif

		private void ChangeExpositor(int changeAmount)
        {
			currentExpositorIndex += changeAmount;
            if(currentExpositorIndex >= demoExpositors.Length) currentExpositorIndex = 0;
            if(currentExpositorIndex < 0) currentExpositorIndex = demoExpositors.Length - 1;
		}
		
		public int GetCurrentExpositorIndex() => currentExpositorIndex;
        
        private void EnableCorrectInputSystem()
        {
            bool newInputSystemEnabled = false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            newInputSystemEnabled = true;
#endif
            oldInputEventSystem.gameObject.SetActive(!newInputSystemEnabled);
            newInputEventSystem.gameObject.SetActive(newInputSystemEnabled);
        }
    }
}