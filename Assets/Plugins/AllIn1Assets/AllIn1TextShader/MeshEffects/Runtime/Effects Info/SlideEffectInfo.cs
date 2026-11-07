using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class SlideEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Slide;

		public PropertyFloatInfo propLength;
		public PropertyFloatInfo propSpeed;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				propLength.CreatePropertyInstance(),
				propSpeed.CreatePropertyInstance()
			};
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			SlideStateValues values = vertexGroupLayout.slideStateValues;

			values.length = MeshEffectUtils.FindFloatValue(propertyInstances, propLength.propertyName);
			values.speed = MeshEffectUtils.FindFloatValue(propertyInstances, propSpeed.propertyName);

			vertexGroupLayout.slideStateValues = values;
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new SlideEffectState(EFFECT_FLAG);
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
			SlideStateValues values = parsedGroup.slideStateValues;

			if (parsedParameter.paramName == propLength.tagBinding)
			{
				values.length = parsedParameter.floatValue;
			}
			else if (parsedParameter.paramName == propSpeed.tagBinding)
			{
				values.speed = parsedParameter.floatValue;
			}

			parsedGroup.slideStateValues = values;
		}
	}
}