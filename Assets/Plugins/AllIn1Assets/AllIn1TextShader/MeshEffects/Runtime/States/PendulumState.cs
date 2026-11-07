using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class PendulumState : MeshEffectState
	{
		public PendulumState(EffectFlag effectFlag) : base(effectFlag)
		{
		}

		public override Color32 TransformColor(ref VertexGroupLayout vertexGroup, Color32 inputColor)
		{
			return inputColor;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout groupLayout, Vector3 inputVertex)
		{
			PendulumStateValues values = groupLayout.pendulumStateValues;

			Vector3 res = inputVertex;

			Vector3 centerToVertex = res - groupLayout.centerTop;

			Quaternion quat = Quaternion.AngleAxis(values.current, Vector3.forward);

			centerToVertex = quat * centerToVertex;

			res = (groupLayout.centerTop + centerToVertex);

			return res;
		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup)
		{
			PendulumStateValues values = vertexGroup.pendulumStateValues;
			values.current = values.from;
			vertexGroup.pendulumStateValues = values;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			PendulumStateValues values = vertexGroup.pendulumStateValues;

			float easeT = values.easeFunction.Evaluate(values.normalizedProgress);
			values.current = Mathf.Lerp(values.from, values.to, easeT);
			if(values.normalizedProgress >= 1f && vertexGroup.playbackMode == PlaybackMode.LOOPED)
			{
				values.normalizedProgress = 0f;
			}

			vertexGroup.pendulumStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{
			PendulumStateValues values = vertexGroup.pendulumStateValues;

			values.current = values.to;

			vertexGroup.pendulumStateValues = values;
		}
	}
}