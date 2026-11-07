using AllIn1Assets.SharedExtensions;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class AllIn1TextPropertiesConfigSO : PropertiesConfigCollection
	{
		public AllIn1TextPropertiesConfig propertiesConfig;

		public override void AddConfig(PropertiesConfig config)
		{
			this.propertiesConfig = (AllIn1TextPropertiesConfig)config;
		}

		public override PropertiesConfig FindPropertiesConfigByShader(Shader shader)
		{
			return propertiesConfig;
		}

		public override PropertiesConfig GetPropertiesConfig()
		{
			return propertiesConfig;
		}

		public override bool IsAllIn3DShaderMaterial()
		{
			return false;
		}
	}
}