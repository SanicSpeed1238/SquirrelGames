using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class TextLayerMaster : TextLayer
	{
		public TextMaterialConfiguratorMaster materialConfiguratorMaster;

		public TextLayerMaster(string layerName, Vector2 offset, int index, TextMaterialConfiguratorMaster materialConfigurator) :
			base(layerName, offset, index, materialConfigurator)
		{
			this.materialConfiguratorMaster = materialConfigurator;
		}

		public void SetText(string text)
		{
			materialConfiguratorMaster.textComponent.text = text;

			materialConfiguratorMaster.UpdateConfiguration(true);
		}

		public override void UpdateBuffers()
		{
			materialConfiguratorMaster.UpdateConfiguration(true);
		}

		public override void Apply()
		{

		}

		public override void RefreshBuffersData(bool updateTimingSeeds)
		{
			materialConfiguratorMaster.RefreshBuffersData(updateTimingSeeds);
		}
	}
}
