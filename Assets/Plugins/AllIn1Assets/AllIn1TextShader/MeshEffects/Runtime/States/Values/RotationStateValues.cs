using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public struct RotationStateValues : IEffectStateValues
	{
		private const float ROTATION_FACTOR = 100f;

		public float rotationSpeed;
		public float currentRotation;

		public void InitAtRandomPoint()
		{
			currentRotation = Random.Range(0f, 360f);
		}

		public void Reset()
		{
			currentRotation = 0f;
		}

		public void SetNormalizedProgress(float normalizedProgress)
		{
			currentRotation = normalizedProgress * 360f;
		}

		public float GetNormalizedProgress()
		{
			return 0f;
		}

		public void Update(float deltaTime)
		{
			currentRotation -= rotationSpeed * ROTATION_FACTOR * deltaTime;
		}
	}
}