using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class ShakeEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Shake;

		public PropertyFloatInfo shakeSpeed;
		public PropertyFloatInfo shakeLength;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				shakeSpeed.CreatePropertyInstance(),
				shakeLength.CreatePropertyInstance()
			};
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			ShakeStateValues values = vertexGroupLayout.shakeStateValues;

			float shakeSpeedValue	= MeshEffectUtils.FindFloatValue(propertyInstances, shakeSpeed.propertyName);
			float shakeLengthValue	= MeshEffectUtils.FindFloatValue(propertyInstances, shakeLength.propertyName);

			values.shakeSpeed = shakeSpeedValue;
			values.shakeLength = shakeLengthValue;

			vertexGroupLayout.shakeStateValues = values;
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new ShakeEffectState(EFFECT_FLAG);
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
			ShakeStateValues values = parsedGroup.shakeStateValues;

			if (shakeSpeed.tagBinding == parsedParameter.paramName)
			{
				values.shakeSpeed = parsedParameter.floatValue;
			}
			else if (shakeLength.tagBinding == parsedParameter.paramName)
			{
				values.shakeLength = parsedParameter.floatValue;
			}

			parsedGroup.shakeStateValues = values;
		}
	}
}