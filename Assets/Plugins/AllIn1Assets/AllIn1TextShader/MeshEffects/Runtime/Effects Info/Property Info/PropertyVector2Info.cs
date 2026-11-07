using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class PropertyVector2Info : PropertyInfo
	{
		public Vector2 defaultValue;

		public override void InitByDefault(ref PropertyInstance propertyInstance)
		{
			base.InitByDefault(ref propertyInstance);

			propertyInstance.vec2Value = defaultValue;
			propertyInstance.vec2ValueDefault = defaultValue;

			propertyInstance.propertyType = PropertyDataType.VECTOR2;
		}

		public override void SetToDefault(ref PropertyInstance propertyInstance)
		{
			propertyInstance.vec2Value = defaultValue;
		}
	}
}