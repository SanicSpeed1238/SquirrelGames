using UnityEngine;


namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public abstract class TextLayer
	{
		protected const float Z_OFFSET_PER_INDEX = 5f;

		public enum BlendMode
		{
			NoBlend = 0,
			Additive = 1,
			Multiply = 2,
		}

		public string layerName;
		public Vector2 offset;
		public TextMaterialConfigurator materialConfigurator;
		public BlendMode blendMode;
		public bool customTimingSeed;
		public int index;
		public bool isVisible;

#if UNITY_EDITOR
		public bool unfolded;
		public bool shaderPropertiesUnfolded;
#endif

		public TextLayer(string layerName, Vector2 offset, int index, TextMaterialConfigurator materialConfigurator)
		{
			this.layerName = layerName;
			this.offset = offset;
			this.index = index;
			this.materialConfigurator = materialConfigurator;

#if UNITY_EDITOR
			isVisible = true;
			unfolded = false;
			shaderPropertiesUnfolded = false;
			customTimingSeed = false;
#endif
		}

		public abstract void UpdateBuffers();

		public abstract void Apply();

		public abstract void RefreshBuffersData(bool updateTimingSeeds);

		public void ApplyTimingSeedsBuffer()
		{
			materialConfigurator.ApplyTimingSeedsBuffer();
		}
	}
}
