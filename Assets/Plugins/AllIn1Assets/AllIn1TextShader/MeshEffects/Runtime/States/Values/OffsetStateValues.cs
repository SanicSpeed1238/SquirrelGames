using UnityEngine;
namespace AllIn1Assets.AllIn1TextShader
{
	public struct OffsetStateValues : IEffectStateValues
	{
		public Vector3 from;
		public Vector3 to;
		public float speed;
		public EaseFunctionSO easeFunction;

		public float normalizedProgress;
		public Vector3 current;

		public void InitAtRandomPoint()
		{
			normalizedProgress = Random.Range(0f, 1f);
		}

		public void Reset()
		{
			normalizedProgress = 0f;
			current = from;
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