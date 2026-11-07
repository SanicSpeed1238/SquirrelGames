using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class TextLayerSlave : TextLayer
	{
		public TextMaterialConfiguratorSlave materialConfiguratorSlave;

		public TextLayerSlave(string layerName, Vector2 offset, int index, TextMaterialConfiguratorSlave materialConfiguratorSlave) : base(layerName, offset, index, materialConfiguratorSlave)
		{
			this.materialConfiguratorSlave = materialConfiguratorSlave;
		}

		// A layer whose object was deleted by hand stays in the list until it is removed from the inspector
		public bool IsValid => materialConfiguratorSlave != null;

		public void SetIndex(int index)
		{
			this.index = index;

			if (materialConfiguratorSlave != null)
			{
				materialConfiguratorSlave.index = index;
			}
		}

		public void UpdateMaterial()
		{
			materialConfiguratorSlave.UpdateMaterial();
		}

		public void InitMeshHandler()
		{
			materialConfiguratorSlave.InitMeshHandler();
		}

		public void RefreshLayerGraphic()
		{
			materialConfiguratorSlave.RefreshLayerGraphic();
		}

		public override void Apply()
		{
			if (!IsValid || materialConfiguratorSlave.TextMaterial == null) { return; }

			TextLayerGraphic layerGraphic = materialConfiguratorSlave.layerGraphic;
			layerGraphic.Place(offset, -(index + 1) * Z_OFFSET_PER_INDEX);

			switch (blendMode)
			{
				case BlendMode.NoBlend:
					materialConfigurator.SetBlendingNoBlend();
					break;
				case BlendMode.Additive:
					materialConfigurator.SetBlendingAdditive();
					break;
				case BlendMode.Multiply:
					materialConfigurator.SetBlendingMultiply();
					break;
			}

			materialConfigurator.gameObject.SetActive(isVisible);

			UpdateMaterial();
		}

		public override void UpdateBuffers()
		{

		}

		public void RefreshBuffers(int numBufferElements)
		{
			materialConfiguratorSlave.RefreshBuffers(numBufferElements);
		}

		public void ReleaseBuffers()
		{
			materialConfiguratorSlave.ReleaseBuffers();
		}

		public override void RefreshBuffersData(bool updateTimingSeed)
		{
			materialConfiguratorSlave.RefreshBuffersData(updateTimingSeed);
		}

		public void SetBlurredAtlas(Texture blurredAtlasTex)
		{
			materialConfiguratorSlave.SetBlurredAtlas(blurredAtlasTex);
		}

		public void SetFontAtlas(Texture mainAtlasTex)
		{
			materialConfiguratorSlave.SetFontAtlas(mainAtlasTex);
		}
	}
}
