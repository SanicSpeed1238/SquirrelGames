using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class InitialRotationEffectState : MeshEffectState
	{
		public InitialRotationEffectState(EffectFlag effectFlag) : base(effectFlag)
		{

		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup)
		{
			InitialRotationStateValues values = vertexGroup.initialRotationStateValues;
			values.current = values.from;
			vertexGroup.initialRotationStateValues = values;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			InitialRotationStateValues values = vertexGroup.initialRotationStateValues;
			
			float easeT = values.easeFunction != null ? values.easeFunction.Evaluate(values.normalizedProgress) : Mathf.Clamp01(values.normalizedProgress);
			values.current = Mathf.LerpUnclamped(values.from, values.to, easeT);
			vertexGroup.initialRotationStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{
			InitialRotationStateValues values = vertexGroup.initialRotationStateValues;

			values.current = values.to;
			
			vertexGroup.initialRotationStateValues = values;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout groupLayout, Vector3 inputVertex)
		{
			InitialRotationStateValues values = groupLayout.initialRotationStateValues;

			Vector3 res = inputVertex;

			Vector3 centerToVertex = res - groupLayout.center;

			Quaternion quat = Quaternion.AngleAxis(values.current, Vector3.forward);

			centerToVertex = quat * centerToVertex;

			res = (groupLayout.center + centerToVertex);

			return res;
		}

		public override Color32 TransformColor(ref VertexGroupLayout groupLayout, Color32 inputColor)
		{
			Color32 res = inputColor;

			return res;
		}
	}
}