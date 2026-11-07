namespace AllIn1Assets.AllIn1TextShader
{
	public interface IEffectStateValues
	{
		void InitAtRandomPoint();

		void Reset();

		void Update(float deltaTime);

		void SetNormalizedProgress(float normalizedProgress);

		float GetNormalizedProgress();
	}
}