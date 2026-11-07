using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public struct PropertyInstance
	{
		public static readonly PropertyInstance DEFAULT = new PropertyInstance
		{
			propertyName = string.Empty,
			displayName = string.Empty,
			propertyType = PropertyDataType.COLOR,
			floatValue = 0f,
			colValue = Color.black,
			vec2Value = Vector2.zero,
			vec3Value = Vector3.zero
		};

		public string propertyName;
		public string tagBinding;
		public string displayName;
		public PropertyDataType propertyType;

		public float floatValue;
		public Vector2 vec2Value;
		public Vector3 vec3Value;
		public Color colValue;
		public string stringValue;


		public float floatValueDefault;
		public Vector2 vec2ValueDefault;
		public Vector3 vec3ValueDefault;
		public Color colValueDefault;
		public string stringValueDefault;

		public void CopyFrom(PropertyInstance from)
		{
			this.propertyName	= from.propertyName;
			this.tagBinding		= from.tagBinding;
			this.displayName	= from.displayName;
			this.propertyType	= from.propertyType;

			CopyValuesFrom(from);
		}

		public void CopyValuesFrom(PropertyInstance from)
		{
			this.floatValue		= from.floatValue;
			this.vec2Value		= from.vec2Value;
			this.vec3Value		= from.vec3Value;
			this.colValue		= from.colValue;
			this.stringValue	= from.stringValue;

			this.floatValueDefault	= from.floatValueDefault;
			this.vec2ValueDefault	= from.vec2ValueDefault;
			this.vec3ValueDefault	= from.vec3ValueDefault;
			this.colValueDefault	= from.colValueDefault;
			this.stringValueDefault = from.stringValueDefault;
		}

		public void SetToDefault()
		{
			switch (propertyType)
			{
				case PropertyDataType.FLOAT:
					floatValue = floatValueDefault;
					break;
				case PropertyDataType.VECTOR2:
					vec2Value = vec2ValueDefault;
					break;
				case PropertyDataType.VECTOR3:
					vec3Value = vec3ValueDefault;
					break;
				case PropertyDataType.COLOR:
					colValue = colValueDefault;
					break;
				case PropertyDataType.STRING:
					stringValue = stringValueDefault;
					break;
			}
		}

		public string GetTagValue()
		{
			string res = string.Empty;

			System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.InvariantCulture;

			switch (propertyType)
			{
				case PropertyDataType.FLOAT:
					res = floatValue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
					break;
				case PropertyDataType.VECTOR2:
					res = $"[{vec2Value.x.ToString("F2", cultureInfo)},{vec2Value.y.ToString("F2", cultureInfo)}]";
					break;
				case PropertyDataType.VECTOR3:
					break;
				case PropertyDataType.COLOR:
					break;
				case PropertyDataType.STRING:
					break;
			}

			return res;
		}
	}
}