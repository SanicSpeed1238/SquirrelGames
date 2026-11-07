using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class RotationEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Rotation;

		public PropertyFloatInfo propRotationSpeed;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				propRotationSpeed.CreatePropertyInstance()
			};
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			RotationStateValues values = vertexGroupLayout.rotationStateValues;

			values.rotationSpeed = MeshEffectUtils.FindFloatValue(propertyInstances, propRotationSpeed.propertyName);

			vertexGroupLayout.rotationStateValues = values;
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new RotationEffectState(EFFECT_FLAG);
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
			RotationStateValues values = parsedGroup.rotationStateValues;

			if(propRotationSpeed.tagBinding == parsedParameter.paramName)
			{
				values.rotationSpeed = parsedParameter.floatValue;
			}

			parsedGroup.rotationStateValues = values;
		}
	}
}