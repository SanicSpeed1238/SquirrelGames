using System.Collections.Generic;
using UnityEngine;

#if ALLIN1_BIRP && ALLIN1_PPV2
using UnityEngine.Rendering.PostProcessing;
#endif

#if ALLIN1_URP
using UnityEngine.Rendering.Universal;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	[ExecuteInEditMode]
	public class DemoSceneConfigurator : MonoBehaviour
	{
		public PostprocessController postprocessController;


		public Camera cameraUI;
		public Camera cameraDemoElements;
		public Camera cameraBackground;
		public Camera cameraRetroTV;

		//PPv2's PostProcessResources, typed loosely so this compiles without the package: a layer added at runtime gets no default references
		public ScriptableObject postprocessResources;

#if ALLIN1_URP
		private UniversalAdditionalCameraData cameraUIAdditionalData;
		private UniversalAdditionalCameraData cameraDemoElementsAdditionalCameraData;
		private UniversalAdditionalCameraData cameraBackgroundAdditionalData;
		private UniversalAdditionalCameraData cameraRetroUniversalData;
#endif

		private List<CameraConfiguration> cameraBasesList;
		public DemoCamerasConfiguration defaultCamerasConfiguration;


		private void OnEnable()
		{
			cameraBasesList = new List<CameraConfiguration>();
			ConfigureScene();
			postprocessController.Init();

			if (!Application.isPlaying)
			{
				ApplyCameraConfiguration(defaultCamerasConfiguration); 
			}
		}

#if ALLIN1_URP
		private void ConfigureScene()
		{
			this.cameraUIAdditionalData = cameraUI.GetUniversalAdditionalCameraData();
			this.cameraDemoElementsAdditionalCameraData = cameraDemoElements.GetUniversalAdditionalCameraData();
			this.cameraBackgroundAdditionalData = cameraBackground.GetUniversalAdditionalCameraData();
			this.cameraRetroUniversalData = cameraRetroTV.GetUniversalAdditionalCameraData();
		}

#elif ALLIN1_BIRP
		private void ConfigureScene()
		{
			if(!Application.isPlaying) { return; }

#if ALLIN1_PPV2
			PostProcessLayer postprocessLayer = cameraDemoElements.GetComponent<PostProcessLayer>();
			if(postprocessLayer == null)
			{
				postprocessLayer = cameraDemoElements.gameObject.AddComponent<PostProcessLayer>();
				postprocessLayer.Init(postprocessResources as PostProcessResources);
			}

			postprocessLayer.volumeLayer = 1 << LayerMask.NameToLayer("Default");
#elif UNITY_EDITOR
			Debug.Log("All In 1 Text Toolkit demo: install the Post Processing package from the Package Manager to see the showcases' bloom in the Built-in pipeline");
#endif
		}

		public void ApplyCameraConfiguration(DemoCamerasConfiguration demoCamerasConfiguration)
		{

		}
#else
		//No pipeline symbol (HDRP, or the first compile before RenderPipelineChecker sets one): nothing to configure
		private void ConfigureScene()
		{
		}

		public void ApplyCameraConfiguration(DemoCamerasConfiguration demoCamerasConfiguration)
		{
		}
#endif

#if ALLIN1_URP
		public void ApplyCameraConfiguration(DemoCamerasConfiguration demoCamerasConfiguration)
		{
			ClearCameraConfigurations();

			cameraUIAdditionalData.renderType = demoCamerasConfiguration.uiCameraConfig.GetCameraRenderType();
			cameraDemoElementsAdditionalCameraData.renderType = demoCamerasConfiguration.demoElementsCameraConfig.GetCameraRenderType();
			cameraBackgroundAdditionalData.renderType = demoCamerasConfiguration.backgroundCameraConfig.GetCameraRenderType();
			cameraRetroUniversalData.renderType = demoCamerasConfiguration.environment3DCameraConfig.GetCameraRenderType();

			cameraUIAdditionalData.renderPostProcessing = demoCamerasConfiguration.uiCameraConfig.postprocessingEnabled;
			cameraDemoElementsAdditionalCameraData.renderPostProcessing = demoCamerasConfiguration.demoElementsCameraConfig.postprocessingEnabled;
			cameraBackgroundAdditionalData.renderPostProcessing = demoCamerasConfiguration.backgroundCameraConfig.postprocessingEnabled;
			cameraRetroUniversalData.renderPostProcessing = demoCamerasConfiguration.environment3DCameraConfig.postprocessingEnabled;

			cameraBasesList.Clear();
			demoCamerasConfiguration.GetCameraBases(cameraBasesList);

			for(int i = 0; i < cameraBasesList.Count; i++)
			{
				CameraConfiguration cameraBaseConfig = cameraBasesList[i];
				UniversalAdditionalCameraData cameraAdditionalData = GetUniversalCameraDataByCameraID(cameraBaseConfig.cameraID);

				ConfiguraCameraStack(cameraAdditionalData, cameraBaseConfig);
			}
		}

		private UniversalAdditionalCameraData GetUniversalCameraDataByCameraID(CameraConfiguration.CameraID cameraID)
		{
			UniversalAdditionalCameraData res = null;

			switch (cameraID)
			{
				case CameraConfiguration.CameraID.BACKGROUND_CAMERA:
					res = cameraBackgroundAdditionalData;
					break;
				case CameraConfiguration.CameraID.DEMO_ELEMENTS_CAMERA:
					res = cameraDemoElementsAdditionalCameraData;
					break;
				case CameraConfiguration.CameraID.ENVIRONMENT_3D_CAMERA:
					res = cameraRetroUniversalData;
					break;
				case CameraConfiguration.CameraID.UI_CAMERA:
					res = cameraUIAdditionalData;
					break;
			}

			return res;
		}

		private Camera GetCameraDataByCameraID(CameraConfiguration.CameraID cameraID)
		{
			Camera res = null;

			switch (cameraID)
			{
				case CameraConfiguration.CameraID.BACKGROUND_CAMERA:
					res = cameraBackground;
					break;
				case CameraConfiguration.CameraID.DEMO_ELEMENTS_CAMERA:
					res = cameraDemoElements;
					break;
				case CameraConfiguration.CameraID.ENVIRONMENT_3D_CAMERA:
					res = cameraRetroTV;
					break;
				case CameraConfiguration.CameraID.UI_CAMERA:
					res = cameraUI;
					break;
			}

			return res;
		}

		private void ClearCameraConfigurations()
		{
			if (cameraUIAdditionalData.renderType == CameraRenderType.Base)
			{
				cameraUIAdditionalData.cameraStack.Clear();
			}

			if (cameraDemoElementsAdditionalCameraData.renderType == CameraRenderType.Base)
			{
				cameraDemoElementsAdditionalCameraData.cameraStack.Clear();
			}

			if (cameraBackgroundAdditionalData.renderType == CameraRenderType.Base)
			{
				cameraBackgroundAdditionalData.cameraStack.Clear();
			}

			if (cameraRetroUniversalData.renderType == CameraRenderType.Base)
			{
				cameraRetroUniversalData.cameraStack.Clear();
			}
		}

		private void ConfiguraCameraStack(UniversalAdditionalCameraData additionalCameraData, CameraConfiguration cameraConfiguration)
		{
			for(int i = 0; i < cameraConfiguration.overlayStackConfig.Length; i++)
			{
				Camera cam = GetCameraDataByCameraID(cameraConfiguration.overlayStackConfig[i]);
				additionalCameraData.cameraStack.Add(cam);
			}
		}
#endif
	}
}