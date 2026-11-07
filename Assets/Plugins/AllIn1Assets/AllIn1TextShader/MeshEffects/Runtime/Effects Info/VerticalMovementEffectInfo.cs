namespace AllIn1Assets.AllIn1TextShader
{
	public class VerticalMovementEffectInfo : MeshEffectInfo
	{
		private const EffectFlag EFFECT_FLAG = EffectFlag.VerticalMovement;

		public PropertyFloatInfo propHeight;
		public PropertyFloatInfo propSpeed;

		public override void ConfigureOrderedProperties(MeshEffectConfig target)
		{
			target.orderedProperties = new PropertyInstance[]
			{
				propHeight.CreatePropertyInstance(),
				propSpeed.CreatePropertyInstance()
			};
		}

		public override void ConfigureVertexGroupLayout(ref VertexGroupLayout vertexGroupLayout, PropertyInstance[] propertyInstances, EaseFunctionSO easeFunction)
		{
			VerticalMovementStateValues values = vertexGroupLayout.verticalMovementStateValues;

			values.height = MeshEffectUtils.FindFloatValue(propertyInstances, propHeight.propertyName);
			values.speed = MeshEffectUtils.FindFloatValue(propertyInstances, propSpeed.propertyName);
			values.easeFunction = easeFunction;

			vertexGroupLayout.verticalMovementStateValues = values;
		}

		public override MeshEffectState CreateState()
		{
			MeshEffectState res = new VerticalMovementEffectState(EFFECT_FLAG);
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
			VerticalMovementStateValues values = parsedGroup.verticalMovementValues;

			if (parsedParameter.paramName == propHeight.tagBinding)
			{
				values.height = parsedParameter.floatValue;
			}
			else if (parsedParameter.paramName == propSpeed.tagBinding)
			{
				values.speed = parsedParameter.floatValue;
			}

			parsedGroup.verticalMovementValues = values;
		}
	}
}