namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class TextShaderTweenController
	{
		public TextMaterialConfiguratorMaster materialConfigurator;
		public PropertyTweenFader[] faderTweens;
		public PropertyTweenSinWave[] sinWaveTweens;

		public void Init()
		{
			if (materialConfigurator.TextMaterial == null) return;

			for (int i = 0; i < faderTweens.Length; i++)
			{
				faderTweens[i].Init(materialConfigurator.TextMaterial);
			}

			for (int i = 0; i < sinWaveTweens.Length; i++)
			{
				sinWaveTweens[i].Init(materialConfigurator.TextMaterial);
			}
		}

		public void Update(float deltaTime)
		{
			if (materialConfigurator == null) return;

			for (int i = 0; i < faderTweens.Length; i++)
			{
				faderTweens[i].Update(deltaTime);
			}

			for (int i = 0; i < sinWaveTweens.Length; i++)
			{
				sinWaveTweens[i].Update(deltaTime);
			}
		}
	}
}