using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class OffsetEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Offset;

		public PropertyVector2Info srcOffset;
		public PropertyVector2Info dstOffset;
		public PropertyFloatInfo speed;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				srcOffset.CreatePropertyInstance(),
				dstOffset.CreatePropertyInstance(),
				speed.CreatePropertyInstance()
			};
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			OffsetStateValues values = vertexGroupLayout.offsetStateValues;

			Vector2 from = MeshEffectUtils.FindVector2Value(propertyInstances, srcOffset.propertyName);
			Vector2 to = MeshEffectUtils.FindVector2Value(propertyInstances, dstOffset.propertyName);

			values.from = new Vector3(from.x, from.y, 0f);
			values.to = new Vector3(to.x, to.y, 0f);
			values.current = values.from;
			values.speed = MeshEffectUtils.FindFloatValue(propertyInstances, speed.propertyName);
			values.easeFunction = easeFunction;

			vertexGroupLayout.offsetStateValues = values;
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new OffsetState(EFFECT_FLAG);
			return res;
		}

		public override EffectFlag GetEffectFlag()
		{
			return EFFECT_FLAG;
		}

		public override void Invert(MeshEffectConfig target)
		{
			ref PropertyInstance srcOffsetPropInstance = ref target.FindPropertyRef(srcOffset.propertyName);
			ref PropertyInstance dstOffsetPropInstance = ref target.FindPropertyRef(dstOffset.propertyName);

			Vector2 aux = srcOffsetPropInstance.vec2Value;
			srcOffsetPropInstance.vec2Value = dstOffsetPropInstance.vec2Value;
			dstOffsetPropInstance.vec2Value = aux;
		}

		protected override void SetupParsedParameter(ref TextParsedGroup parsedGroup, ParsedParameter parsedParameter, EasingFunctionsSO easeFunctionCollection)
		{
			OffsetStateValues values = parsedGroup.offsetStateValues;

			if (srcOffset.tagBinding == parsedParameter.paramName)
			{
				values.from = GetParsedOffset(parsedParameter);
			}
			else if (dstOffset.tagBinding == parsedParameter.paramName)
			{
				values.to = GetParsedOffset(parsedParameter);
			}
			else if(speed.tagBinding == parsedParameter.paramName)
			{
				values.speed = parsedParameter.floatValue;
			}

			parsedGroup.offsetStateValues = values;
		}

		// One number is a vertical offset (to:5), the direction of the default slide; two are x and y
		private static Vector2 GetParsedOffset(ParsedParameter parsedParameter)
		{
			return parsedParameter.isVector2 ? parsedParameter.vec2Value : new Vector2(0f, parsedParameter.floatValue);
		}
	}
}