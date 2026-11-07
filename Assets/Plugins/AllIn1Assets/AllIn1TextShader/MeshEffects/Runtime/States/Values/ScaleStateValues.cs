using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public struct ScaleStateValues : IEffectStateValues
	{
		public Vector2 from;
		public Vector2 to;
		public float speed;
		public EaseFunctionSO easeFunction;

		public Vector2 current;
		public float normalizedProgress;

		public void InitAtRandomPoint()
		{
			normalizedProgress = Random.Range(0f, 1f);
		}

		public void Reset()
		{
			current = Vector2.zero;
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