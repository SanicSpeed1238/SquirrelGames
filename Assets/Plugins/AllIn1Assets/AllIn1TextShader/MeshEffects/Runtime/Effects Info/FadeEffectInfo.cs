using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class FadeEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.Fade;

		public PropertyFloatInfo srcAlpha;
		public PropertyFloatInfo dstAlpha;
		public PropertyFloatInfo speed;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				srcAlpha.CreatePropertyInstance(),
				dstAlpha.CreatePropertyInstance(),
				speed.CreatePropertyInstance()
			};
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new FadeState(EFFECT_FLAG);

			return res;
		}

		public override EffectFlag GetEffectFlag()
		{
			return EFFECT_FLAG;
		}

		public override void Invert(MeshEffectConfig target)
		{
			ref PropertyInstance srcAlphaPropInstance = ref target.FindPropertyRef(srcAlpha.propertyName);
			ref PropertyInstance dstAlphaPropInstance = ref target.FindPropertyRef(dstAlpha.propertyName);
			
			float aux = srcAlphaPropInstance.floatValue;
			srcAlphaPropInstance.floatValue = dstAlphaPropInstance.floatValue;
			dstAlphaPropInstance.floatValue = aux;
		}

		protected override void SetupParsedParameter(ref TextParsedGroup parsedGroup, ParsedParameter parsedParameter, EasingFunctionsSO easeFunctionCollection)
		{
			FadeStateValues values = parsedGroup.alphaStateValues;

			if (srcAlpha.tagBinding == parsedParameter.paramName)
			{
				values.from = parsedParameter.floatValue;
			}
			else if (dstAlpha.tagBinding == parsedParameter.paramName)
			{
				values.to = parsedParameter.floatValue;
			}
			else if(speed.tagBinding == parsedParameter.paramName)
			{
				values.speed = parsedParameter.floatValue;
			}

			parsedGroup.alphaStateValues = values;
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			FadeStateValues values = vertexGroupLayout.alphaStateValues;

			values.from = MeshEffectUtils.FindFloatValue(propertyInstances, srcAlpha.propertyName);
			values.to = MeshEffectUtils.FindFloatValue(propertyInstances, dstAlpha.propertyName);
			values.speed = MeshEffectUtils.FindFloatValue(propertyInstances, speed.propertyName);

			values.easeFunction = easeFunction;
			values.current = values.from;

			vertexGroupLayout.alphaStateValues = values;
		}
	}
}