using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class RainbowEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Rainbow;

		public PropertyFloatInfo speedProp;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				speedProp.CreatePropertyInstance(),
			};
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			RainbowStateValues values = vertexGroupLayout.rainbowStateValues;

			values.speed = MeshEffectUtils.FindFloatValue(propertyInstances, speedProp.propertyName);

			vertexGroupLayout.rainbowStateValues = values;
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new RainbowEffectState(EFFECT_FLAG);
			return res;
		}

		public override EffectFlag GetEffectFlag()
		{
			return EFFECT_FLAG;
		}

		public override void Invert(MeshEffectConfig target)
		{

		}

		protected override void SetupParsedParameter(ref TextParsedGroup parsedGroup, ParsedParameter parsedParameter, EasingFunctionsSO easeFunctionCollection)
		{
			RainbowStateValues values = parsedGroup.rainbowStateValues;

			if (speedProp.tagBinding == parsedParameter.paramName)
			{
				values.speed = parsedParameter.floatValue;
			}

			parsedGroup.rainbowStateValues = values;
		}
	}
}