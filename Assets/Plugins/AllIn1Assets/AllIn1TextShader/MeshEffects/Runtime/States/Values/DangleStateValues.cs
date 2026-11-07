using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public struct DangleStateValues : IEffectStateValues
	{
		public float dangleLength;
		
		public float speed;
		public float normalizedProgress;
		public float current;

		public EaseFunctionSO easeFunction;

		public void InitAtRandomPoint()
		{
			normalizedProgress = Random.Range(0f, 1f);
		}

		public void Reset()
		{
			current = 0f;
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