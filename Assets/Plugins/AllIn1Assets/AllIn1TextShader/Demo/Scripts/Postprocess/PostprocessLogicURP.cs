using UnityEngine;
#if ALLIN1_URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	public class PostprocessLogicURP : PostprocessLogicAbstract
	{
#if ALLIN1_URP
		private Bloom bloom;
		public Volume volume;
#endif

		public PostprocessLogicURP(PostprocessController postprocessController) : base(postprocessController)
		{
		}

		public override void ApplyConfig(PostprocessConfig config)
		{
#if ALLIN1_URP
			bloom.active = config.bloomEnabled;
			bloom.threshold.value = config.bloomThreshold;
			bloom.intensity.value = config.bloomIntensity;
			bloom.tint.value = config.bloomColor;
#endif
		}

		public override void Init()
		{
			base.Init();

#if ALLIN1_URP
			volume = goVolumeInstance.AddComponent<Volume>();
			volume.isGlobal = true;

			VolumeProfile volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
			bloom = volumeProfile.Add<Bloom>();
			bloom.active = true;
			bloom.threshold.overrideState = true;
			bloom.intensity.overrideState = true;
			bloom.tint.overrideState = true;

			volume.profile = volumeProfile;
#endif
		}
	}
}