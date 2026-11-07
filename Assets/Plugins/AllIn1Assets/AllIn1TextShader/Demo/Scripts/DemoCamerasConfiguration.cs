using System.Collections.Generic;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CreateAssetMenu(menuName = "AllIn1Text/Demo/Cameras Configuration")]
	public class DemoCamerasConfiguration : ScriptableObject
	{
		public CameraConfiguration backgroundCameraConfig;
		public CameraConfiguration uiCameraConfig;
		public CameraConfiguration demoElementsCameraConfig;
		public CameraConfiguration environment3DCameraConfig;


		public void GetCameraBases(List<CameraConfiguration> outCameraBases)
		{
			if(backgroundCameraConfig.renderType == CameraConfiguration.RenderType.BASE)
			{
				outCameraBases.Add(backgroundCameraConfig);
			}

			if (uiCameraConfig.renderType == CameraConfiguration.RenderType.BASE)
			{
				outCameraBases.Add(uiCameraConfig);
			}

			if (demoElementsCameraConfig.renderType == CameraConfiguration.RenderType.BASE)
			{
				outCameraBases.Add(demoElementsCameraConfig);
			}

			if (environment3DCameraConfig.renderType == CameraConfiguration.RenderType.BASE)
			{
				outCameraBases.Add(environment3DCameraConfig);
			}
		}
	}
}