using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class ScaleEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Scale;

		public PropertyVector2Info srcScale;
		public PropertyVector2Info dstScale;
		public PropertyFloatInfo speed;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				srcScale.CreatePropertyInstance(),
				dstScale.CreatePropertyInstance(),
				speed.CreatePropertyInstance()
			};
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new ScaleState(EFFECT_FLAG);

			return res;
		}

		public override void Invert(MeshEffectConfig target)
		{
			ref PropertyInstance srcScalePropInstance = ref target.FindPropertyRef(srcScale.propertyName);
			ref PropertyInstance dstScalePropInstance = ref target.FindPropertyRef(dstScale.propertyName);

			Vector2 aux = srcScalePropInstance.vec2Value;
			srcScalePropInstance.vec2Value = dstScalePropInstance.vec2Value;
			dstScalePropInstance.vec2Value = aux;
		}

		public override EffectFlag GetEffectFlag()
		{
			return EFFECT_FLAG;
		}

		protected override void SetupParsedParameter(ref TextParsedGroup parsedGroup, ParsedParameter parsedParameter, EasingFunctionsSO easeFunctionCollection)
		{
			ScaleStateValues scaleStateValues = parsedGroup.scaleStateValues;

			if (srcScale.tagBinding == parsedParameter.paramName)
			{
				scaleStateValues.from = GetParsedScale(parsedParameter);
			}
			else if (dstScale.tagBinding == parsedParameter.paramName)
			{
				scaleStateValues.to = GetParsedScale(parsedParameter);
			}
			else if(speed.tagBinding == parsedParameter.paramName)
			{
				scaleStateValues.speed = parsedParameter.floatValue;
			}

			parsedGroup.scaleStateValues = scaleStateValues;
		}

		//to:1.5 scales both axes, to:[1.5,0.8] each one
		private static Vector2 GetParsedScale(ParsedParameter parsedParameter)
		{
			return parsedParameter.isVector2 ? parsedParameter.vec2Value : Vector2.one * parsedParameter.floatValue;
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			ScaleStateValues values = vertexGroupLayout.scaleStateValues;

			values.from		= MeshEffectUtils.FindVector2Value(propertyInstances, srcScale.propertyName);
			values.to		= MeshEffectUtils.FindVector2Value(propertyInstances, dstScale.propertyName);
			values.speed	= MeshEffectUtils.FindFloatValue(propertyInstances, speed.propertyName);
			values.easeFunction = easeFunction;
			values.current	= Vector2.zero;

			vertexGroupLayout.scaleStateValues = values;
		}
	}
}