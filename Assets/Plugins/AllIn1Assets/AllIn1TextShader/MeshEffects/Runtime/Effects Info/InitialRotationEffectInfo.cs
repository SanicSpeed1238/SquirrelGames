
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class InitialRotationEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.InitialRotation;

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

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new InitialRotationEffectState(EFFECT_FLAG);
			return res;
		}

		public override EffectFlag GetEffectFlag()
		{
			return EFFECT_FLAG;
		}

		public override void Invert(MeshEffectConfig target)
		{
			ref PropertyInstance srcRotationPropInstance = ref target.FindPropertyRef(srcRotation.propertyName);
			ref PropertyInstance dstRotationPropInstance = ref target.FindPropertyRef(dstRotation.propertyName);
			
			float aux = srcRotationPropInstance.floatValue;
			srcRotationPropInstance.floatValue = dstRotationPropInstance.floatValue;
			dstRotationPropInstance.floatValue = aux;
		}

		protected override void SetupParsedParameter(ref TextParsedGroup parsedGroup, ParsedParameter parsedParameter, EasingFunctionsSO easeFunctionCollection)
		{
			InitialRotationStateValues values = parsedGroup.initialRotationStateValues;

			if (srcRotation.tagBinding == parsedParameter.paramName)
			{
				values.from = parsedParameter.floatValue;
			}
			else if(dstRotation.tagBinding == parsedParameter.paramName)
			{
				values.to = parsedParameter.floatValue;
			}
			else if(speed.tagBinding == parsedParameter.paramName)
			{
				values.speed = parsedParameter.floatValue;
			}

			values.current = 0f;
			values.normalizedProgress = 0f;

			parsedGroup.initialRotationStateValues = values;
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			InitialRotationStateValues values = vertexGroupLayout.initialRotationStateValues;

			values.from = MeshEffectUtils.FindFloatValue(propertyInstances, srcRotation.propertyName);
			values.to = MeshEffectUtils.FindFloatValue(propertyInstances, dstRotation.propertyName);

			values.speed = MeshEffectUtils.FindFloatValue(propertyInstances, speed.propertyName);
			values.current = 0f;
			values.easeFunction = easeFunction;
			values.current = 0f;

			vertexGroupLayout.initialRotationStateValues = values;
		}
	}
}