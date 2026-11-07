using AllIn1Assets.SharedExtensions;
using System.Collections.Generic;

namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class AllIn1TextPropertiesConfig : PropertiesConfig
	{
		public List<int> fontProperties;

		public int fontAtlasIdx;
		public int blurredAtlasIdx;


		public AllIn1TextPropertiesConfig() : base()
		{
			fontProperties = new List<int>();
		}

		protected override void ProcessCustomProperty(string attribute, string propName, int propIndex)
		{
			base.ProcessCustomProperty(attribute, propName, propIndex);

			if (attribute == AllIn1TextConstants.SHADER_ATTRIBUTE_FONT_PROPERTY)
			{
				if(propName == AllIn1TextConstants.MAT_PROP_FONT_ATLAS)
				{
					fontAtlasIdx = propIndex;
				}
				else if(propName == AllIn1TextConstants.MAT_PROP_BLURRED_ATLAS)
				{
					blurredAtlasIdx = propIndex;
				}
				else
				{
					fontProperties.Add(propIndex);
				}
			}
		}
	}
}