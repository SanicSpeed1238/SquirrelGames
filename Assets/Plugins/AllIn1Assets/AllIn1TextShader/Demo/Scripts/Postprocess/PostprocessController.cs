using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class PostprocessController : MonoBehaviour
	{
		public enum LerpingState
		{
			NONE,
			FADING_OUT,
			FADING_IN
		}

		public float lerpingTime;

		private PostprocessLogicAbstract postprocessLogic;

		private float timer;
		private PostprocessConfig from;
		private PostprocessConfig to;
		private PostprocessConfig current;
		private LerpingState lerpingState;

		public void Init()
		{
#if ALLIN1_BIRP
			postprocessLogic = new PostprocessLogicBIRP(this);
#elif ALLIN1_URP
			postprocessLogic = new PostprocessLogicURP(this);
#endif

			//Null without a pipeline symbol (HDRP, or before RenderPipelineChecker ran): the demo runs without post processing
			postprocessLogic?.Init();

			lerpingState = LerpingState.NONE;

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				ApplyConfig(PostprocessConfig.DEFAULT);
			}
#endif
		}

		public void GoTo(PostprocessConfig from, PostprocessConfig to)
		{
			this.from = from;
			this.to = to;
			this.current = from;
			lerpingState = LerpingState.FADING_OUT;

			timer = 0f;
		}

		// From whatever is applied now, so a new target mid fade never jumps
		public void GoTo(PostprocessConfig to)
		{
			GoTo(current, to);
		}

		public void ApplyConfig(PostprocessConfig config)
		{
			current = config;
			postprocessLogic?.ApplyConfig(config);
		}

		private void Update()
		{
			switch (lerpingState)
			{
				case LerpingState.FADING_OUT:
					FadingOut();
					break;
				case LerpingState.FADING_IN:
					FadingIn();
					break;
			}
		}

		private void FadingOut()
		{
			float t = timer / lerpingTime;
			timer += Time.deltaTime;

			current = PostprocessConfig.Lerp(from, PostprocessConfig.DEFAULT, t);
			ApplyConfig(current);

			if (t >= 1f)
			{
				timer = 0f;
				lerpingState = LerpingState.FADING_IN;
			}
		}

		private void FadingIn()
		{
			float t = timer / lerpingTime;
			timer += Time.deltaTime;

			current = PostprocessConfig.Lerp(PostprocessConfig.DEFAULT, to, t);
			ApplyConfig(current);

			if (t >= 1f)
			{
				timer = 0f;
				lerpingState = LerpingState.NONE;
			}
		}
	}
}