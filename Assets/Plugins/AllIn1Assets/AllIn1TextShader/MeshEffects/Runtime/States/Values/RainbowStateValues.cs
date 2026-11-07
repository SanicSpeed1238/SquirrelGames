using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public struct RainbowStateValues : IEffectStateValues
	{
		public float speed;
		
		public Color32 from;

		public int rainbowIndex;
		public float normalizedProgress;

		public void InitAtRandomPoint()
		{
			rainbowIndex = Random.Range(0, MeshEffectUtils.RAINBOW_COLORS.Length);
			normalizedProgress = Random.Range(0f, 1f);
		}

		public void Reset()
		{
			rainbowIndex = 0;
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