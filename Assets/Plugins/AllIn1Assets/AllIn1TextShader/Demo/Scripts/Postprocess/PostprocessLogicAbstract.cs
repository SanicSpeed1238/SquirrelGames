using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public abstract class PostprocessLogicAbstract
	{
		public PostprocessController postprocessController;
		public GameObject prefabVolume;

		protected GameObject goVolumeInstance;


		public PostprocessLogicAbstract(PostprocessController postprocessController)
		{
			this.postprocessController = postprocessController;
		}

		public virtual void Init()
		{
			for(int i = 0; i < postprocessController.transform.childCount; i++)
			{
#if UNITY_EDITOR
				GameObject.DestroyImmediate(postprocessController.transform.GetChild(i).gameObject);
#else
				GameObject.Destroy(postprocessController.transform.GetChild(i).gameObject);
#endif
			}

			goVolumeInstance = new GameObject("Volume");
			goVolumeInstance.name = "Volume";
			goVolumeInstance.transform.parent = postprocessController.transform;
			goVolumeInstance.transform.localPosition = Vector3.zero;
		}


		public abstract void ApplyConfig(PostprocessConfig config);
	}
}