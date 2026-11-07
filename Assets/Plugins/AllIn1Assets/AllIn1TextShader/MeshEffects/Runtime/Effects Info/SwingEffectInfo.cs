using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class SwingEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Swing;

		public PropertyFloatInfo swingAmount;
		public PropertyFloatInfo swingSpeed;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				swingAmount.CreatePropertyInstance(),
				swingSpeed.CreatePropertyInstance()
			};
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			SwingStateValues values = vertexGroupLayout.swingStateValues;

			values.swingAmount = MeshEffectUtils.FindFloatValue(propertyInstances, swingAmount.propertyName);
			values.speed = MeshEffectUtils.FindFloatValue(propertyInstances, swingSpeed.propertyName);
			values.normalizedProgress = 0f;
			values.easeFunction = easeFunction;

			vertexGroupLayout.swingStateValues = values;
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new SwingEffectState(EFFECT_FLAG);
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
			SwingStateValues values = parsedGroup.swingStateValues;

			if (parsedParameter.paramName == swingAmount.tagBinding)
			{
				values.swingAmount = parsedParameter.floatValue;
			}
			else if (parsedParameter.paramName == swingSpeed.tagBinding)
			{
				values.speed = parsedParameter.floatValue;
			}

			values.normalizedProgress = 0f;
			values.currentRotation = 0f;

			parsedGroup.swingStateValues = values;
		}
	}
}