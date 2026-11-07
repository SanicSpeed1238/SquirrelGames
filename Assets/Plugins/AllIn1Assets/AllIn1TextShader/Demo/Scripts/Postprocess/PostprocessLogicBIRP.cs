using AllIn1Assets.AllIn1TextShader;
using UnityEngine;

#if ALLIN1_BIRP && ALLIN1_PPV2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace AllIn1Assets
{
	public class PostprocessLogicBIRP : PostprocessLogicAbstract
	{
#if ALLIN1_BIRP && ALLIN1_PPV2
		public PostProcessVolume postprocessVolume;
		public Bloom bloom;
#endif

		public PostprocessLogicBIRP(PostprocessController postprocessController) : base(postprocessController)
		{}

		public override void ApplyConfig(PostprocessConfig config)
		{
#if ALLIN1_BIRP && ALLIN1_PPV2
			bloom.enabled.value = config.bloomEnabled;
			bloom.intensity.value = config.bloomIntensity;
			bloom.color.value = config.bloomColor;
			bloom.threshold.value = config.bloomThreshold;
#endif
		}

		public override void Init()
		{
			base.Init();

#if ALLIN1_BIRP && ALLIN1_PPV2
			this.postprocessVolume = goVolumeInstance.AddComponent<PostProcessVolume>();

			this.postprocessVolume.isGlobal = true;
			this.bloom = this.postprocessVolume.profile.AddSettings<Bloom>();
			
			this.bloom.enabled.value = true;
			this.bloom.intensity.overrideState = true;
			this.bloom.color.overrideState = true;
			this.bloom.threshold.overrideState = true;
#endif
		}
	}
}