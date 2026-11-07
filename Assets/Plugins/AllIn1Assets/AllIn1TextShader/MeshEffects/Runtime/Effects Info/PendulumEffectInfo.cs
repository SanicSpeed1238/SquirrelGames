using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class PendulumEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Pendulum;

		public PropertyFloatInfo srcRotation;
		public PropertyFloatInfo dstRotation;
		public PropertyFloatInfo speed;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				srcRotation.CreatePropertyInstance(),
				dstRotation.CreatePropertyInstance(),
				speed.CreatePropertyInstance()
			};
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			PendulumStateValues values = vertexGroupLayout.pendulumStateValues;

			values.from = MeshEffectUtils.FindFloatValue(propertyInstances, srcRotation.propertyName);
			values.to = MeshEffectUtils.FindFloatValue(propertyInstances, dstRotation.propertyName);
			values.speed = MeshEffectUtils.FindFloatValue(propertyInstances, speed.propertyName);
			values.current = 0f;
			values.easeFunction = easeFunction;

			vertexGroupLayout.pendulumStateValues = values;
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new PendulumState(EFFECT_FLAG);
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
			PendulumStateValues values = parsedGroup.pendulumStateValues;

			if (srcRotation.tagBinding == parsedParameter.paramName)
			{
				values.from = parsedParameter.floatValue;
			}
			else if (dstRotation.tagBinding == parsedParameter.paramName)
			{
				values.to = parsedParameter.floatValue;
			}
			else if(speed.tagBinding == parsedParameter.paramName)
			{
				values.speed = parsedParameter.floatValue;
			}

			values.current = 0f;

			parsedGroup.pendulumStateValues = values;
		}
	}
}