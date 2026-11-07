namespace AllIn1Assets.AllIn1TextShader
{
	public abstract class PropertyInfo
	{
		public string propertyName;
		public string displayName;
		public string tagBinding;

		public PropertyInstance CreatePropertyInstance()
		{
			PropertyInstance res = new PropertyInstance();
			InitByDefault(ref res);

			return res;
		}

		public virtual void InitByDefault(ref PropertyInstance propertyInstance)
		{
			propertyInstance.propertyName = propertyName;
			propertyInstance.displayName = displayName;
			propertyInstance.tagBinding = tagBinding;
		}

		public abstract void SetToDefault(ref PropertyInstance propertyInstance);

	}
}