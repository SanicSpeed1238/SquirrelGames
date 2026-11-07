using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public struct ShakeStateValues : IEffectStateValues
	{
		public float shakeSpeed;
		public float shakeLength;

		public Vector3 fromOffset;
		public Vector3 toOffset;
		public float normalizedDisplacement;

		public void InitAtRandomPoint()
		{
			normalizedDisplacement = Random.Range(0f, 1f);
		}

		public void Reset()
		{
			normalizedDisplacement = 0f;
		}

		public void SetNormalizedProgress(float normalizedProgress)
		{
			normalizedDisplacement = normalizedProgress;
		}

		public float GetNormalizedProgress()
		{
			return 0f;
		}

		public void Update(float deltaTime)
		{
			normalizedDisplacement += deltaTime * shakeSpeed;
		}
	}
}