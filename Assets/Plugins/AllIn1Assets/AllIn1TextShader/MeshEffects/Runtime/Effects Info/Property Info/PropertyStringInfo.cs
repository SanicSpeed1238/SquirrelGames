namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class PropertyStringInfo : PropertyInfo
	{
		public string defaultValue;

		public override void InitByDefault(ref PropertyInstance propertyInstance)
		{
			base.InitByDefault(ref propertyInstance);

			propertyInstance.stringValue = defaultValue;
			propertyInstance.stringValueDefault = defaultValue;

			propertyInstance.propertyType = PropertyDataType.STRING;
		}

		public override void SetToDefault(ref PropertyInstance propertyInstance)
		{
			propertyInstance.stringValue = defaultValue;
		}
	}
}