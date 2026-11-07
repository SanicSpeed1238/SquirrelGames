using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public static class MeshEffectUtils
	{
		public static readonly Color32[] RAINBOW_COLORS =
		{
			new Color32(255, 0, 0, 1),
			new Color32(255, 127, 0, 1),
			new Color32(255, 255, 0, 1),
			new Color32(127, 255, 0, 1),
			new Color32(0, 255, 0, 1),
			new Color32(0, 255, 127, 1),
			new Color32(0, 255, 255, 1),
			new Color32(0, 127, 255, 1),
			new Color32(0, 0, 255, 1),
			new Color32(127, 0, 255, 1),
			new Color32(255, 0, 255, 1),
			new Color32(255, 0, 127, 1)
		};


		private static PropertyInstance FindPropertyInstanceByName(PropertyInstance[] properties, string propertyName)
		{
			PropertyInstance res = PropertyInstance.DEFAULT;
			for (int i = 0; i < properties.Length; i++)
			{
				if (properties[i].propertyName == propertyName)
				{
					res = properties[i];
					break;
				}
			}

			return res;
		}

		private static PropertyInstance FindPropertyInstanceByTagBinding(PropertyInstance[] properties, string tagBinding)
		{
			PropertyInstance res = PropertyInstance.DEFAULT;
			for (int i = 0; i < properties.Length; i++)
			{
				if (properties[i].tagBinding == tagBinding)
				{
					res = properties[i];
					break;
				}
			}

			return res;
		}

		public static float FindFloatValue(PropertyInstance[] properties, string propertyName)
		{
			PropertyInstance propertyInstance = FindPropertyInstanceByName(properties, propertyName);
			float res = propertyInstance.floatValue;

			return res;
		}

		public static Vector2 FindVector2Value(PropertyInstance[] properties, string propertyName)
		{
			PropertyInstance propertyInstance = FindPropertyInstanceByName(properties, propertyName);
			Vector2 res = propertyInstance.vec2Value;

			return res;
		}

		public static void SetFloatByTag(ref float target, float value, string tagBinding, PropertyInstance[] properties)
		{
			PropertyInstance propertyInstance = FindPropertyInstanceByTagBinding(properties, tagBinding);

			target = propertyInstance.floatValue;
		}
	}
}