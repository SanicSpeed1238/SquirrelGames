using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public struct InitialRotationStateValues : IEffectStateValues
	{
		public float from;
		public float to;

		public float speed;
		public EaseFunctionSO easeFunction;

		public float current;
		public float normalizedProgress;

		public void InitAtRandomPoint()
		{

		}

		public void Reset()
		{
			current = 0f;
			normalizedProgress = 0f;
		}

		public void SetNormalizedProgress(float normalizedProgress)
		{
			this.normalizedProgress = Mathf.Clamp01(normalizedProgress);
		}

		public float GetNormalizedProgress()
		{
			return normalizedProgress;
		}

		public void Update(float deltaTime)
		{
			this.normalizedProgress += speed * deltaTime;
			this.normalizedProgress = Mathf.Clamp01(this.normalizedProgress);
		}
	}
}