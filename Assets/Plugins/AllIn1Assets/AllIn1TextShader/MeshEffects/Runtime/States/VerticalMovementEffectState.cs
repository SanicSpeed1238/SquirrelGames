using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class VerticalMovementEffectState : MeshEffectState
	{
		public VerticalMovementEffectState(EffectFlag effectFlag) : base(effectFlag)
		{
		}

		public override Color32 TransformColor(ref VertexGroupLayout vertexGroup, Color32 inputColor)
		{
			return inputColor;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout vertexGroup, Vector3 inputVertex)
		{
			Vector3 res = inputVertex;

			VerticalMovementStateValues values = vertexGroup.verticalMovementStateValues;

			res += Vector3.up * values.current;

			return res;
		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup)
		{
			VerticalMovementStateValues values = vertexGroup.verticalMovementStateValues;

			values.current = 0f;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			VerticalMovementStateValues values = vertexGroup.verticalMovementStateValues;

			float easeT = values.easeFunction.Evaluate(values.normalizedProgress);
			values.current = Mathf.Lerp(0f, values.height, easeT);
			
			if(values.normalizedProgress >= 1f)
			{
				values.normalizedProgress = 0f;
			}

			vertexGroup.verticalMovementStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{
			
		}
	}
}