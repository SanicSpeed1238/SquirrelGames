using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public struct SlideStateValues : IEffectStateValues
	{
		public float length;
		public float speed;

		public float from;
		public float to;

		public float normalizedProgress;
		public float current;

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