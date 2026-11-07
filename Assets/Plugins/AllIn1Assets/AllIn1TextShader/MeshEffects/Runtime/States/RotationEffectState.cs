using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class RotationEffectState : MeshEffectState
	{
		public RotationEffectState(EffectFlag effectFlag) : base(effectFlag)
		{
		}

		public override Color32 TransformColor(ref VertexGroupLayout groupLayout, Color32 inputColor)
		{
			return inputColor;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout groupLayout, Vector3 inputVertex)
		{
			RotationStateValues values = groupLayout.rotationStateValues;

			Vector3 res = inputVertex;

			Vector3 centerToVertex = res - groupLayout.center;

			Quaternion quat = Quaternion.AngleAxis(values.currentRotation, Vector3.forward);

			centerToVertex = quat * centerToVertex;

			res = (groupLayout.center + centerToVertex);

			return res;
		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup)
		{

		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			RotationStateValues values = vertexGroup.rotationStateValues;

			if(Mathf.Abs(values.currentRotation) >= 360f)
			{
				if(values.currentRotation > 0)
				{
					values.currentRotation -= 360f;
				}
				else
				{
					values.currentRotation += 360f;
				}
			}

			vertexGroup.rotationStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{
			
		}
	}
}