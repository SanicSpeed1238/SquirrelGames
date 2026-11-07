using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class DangleEffectState : MeshEffectState
	{
		public DangleEffectState(EffectFlag effectFlag) : base(effectFlag)
		{
		}

		public override Color32 TransformColor(ref VertexGroupLayout groupLayout, Color32 inputColor)
		{
			return inputColor;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout groupLayout, Vector3 inputVertex)
		{
			DangleStateValues values = groupLayout.dangleStateValues;

			Vector3 res = inputVertex;
			if(inputVertex.y < groupLayout.center.y)
			{
				res += Vector3.right * values.current;
			}

			return res;
		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout groupLayout)
		{
			DangleStateValues values = groupLayout.dangleStateValues;

			values.normalizedProgress = 0.5f;
			values.current = 0f;

			groupLayout.dangleStateValues = values;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			DangleStateValues values = vertexGroup.dangleStateValues;

			
			if(values.normalizedProgress >= 1f)
			{
				values.normalizedProgress = 0f;
			}

			float easeT = values.easeFunction.Evaluate(values.normalizedProgress);

			values.current = Mathf.Lerp(-values.dangleLength, values.dangleLength, easeT);
			
			vertexGroup.dangleStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout groupLayout)
		{

		}
	}
}