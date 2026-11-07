namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class PropertyFloatInfo : PropertyInfo
	{
		public float defaultValue;

		public override void InitByDefault(ref PropertyInstance propertyInstance)
		{
			base.InitByDefault(ref propertyInstance);

			propertyInstance.floatValue = defaultValue;
			propertyInstance.floatValueDefault = defaultValue;

			propertyInstance.propertyType = PropertyDataType.FLOAT;
		}

		public override void SetToDefault(ref PropertyInstance propertyInstance)
		{
			propertyInstance.floatValue = defaultValue;
		}
	}
}