using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public struct SwingStateValues : IEffectStateValues
	{
		public float swingAmount;
		public float speed;

		public float currentRotation;
		public float normalizedProgress;

		public EaseFunctionSO easeFunction;

		public void InitAtRandomPoint()
		{
			normalizedProgress = Random.Range(0f, 1f);
		}

		public void Reset()
		{
			currentRotation = 0f;
			normalizedProgress = 0f;
		}

		public void SetNormalizedProgress(float normalizedProgress)
		{
			this.normalizedProgress = normalizedProgress;
		}

		public float GetNormalizedProgress()
		{
			return normalizedProgress;
		}

		public void Update(float deltaTime)
		{
			this.normalizedProgress += speed * deltaTime;
		}
	}
}