using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class CameraConfiguration
	{
		public enum RenderType
		{
			BASE = 0,
			OVERLAY = 1,
		}

		public enum CameraID
		{
			BACKGROUND_CAMERA,
			UI_CAMERA,
			DEMO_ELEMENTS_CAMERA,
			ENVIRONMENT_3D_CAMERA,
		}

		public RenderType renderType;
		public CameraID cameraID;
		public CameraID[] overlayStackConfig;

		public bool postprocessingEnabled;

#if ALLIN1_URP

		public UnityEngine.Rendering.Universal.CameraRenderType GetCameraRenderType()
		{
			UnityEngine.Rendering.Universal.CameraRenderType res;
			switch (renderType)
			{
				case RenderType.BASE:
					res = UnityEngine.Rendering.Universal.CameraRenderType.Base;
					break;
				case RenderType.OVERLAY:
					res = UnityEngine.Rendering.Universal.CameraRenderType.Overlay;
					break;
				default:
					res = UnityEngine.Rendering.Universal.CameraRenderType.Base;
					break;
			}

			return res;
		}
#endif
	}
}