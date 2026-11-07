using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class DangleEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Dangle;

		public PropertyFloatInfo propDangleLength;
		public PropertyFloatInfo propSpeed;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				propDangleLength.CreatePropertyInstance(),
				propSpeed.CreatePropertyInstance()
			};
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			DangleStateValues values = vertexGroupLayout.dangleStateValues;

			values.dangleLength = MeshEffectUtils.FindFloatValue(propertyInstances, propDangleLength.propertyName);
			values.speed = MeshEffectUtils.FindFloatValue(propertyInstances, propSpeed.propertyName);
			values.easeFunction = easeFunction;

			vertexGroupLayout.dangleStateValues = values;
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new DangleEffectState(EFFECT_FLAG);
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
			DangleStateValues values = parsedGroup.dangleStateValues;

			if (parsedParameter.paramName == propDangleLength.tagBinding)
			{
				values.dangleLength = parsedParameter.floatValue;
			}
			else if(parsedParameter.paramName == propSpeed.tagBinding)
			{
				values.speed = parsedParameter.floatValue;
			}

			parsedGroup.dangleStateValues = values;
		}
	}
}