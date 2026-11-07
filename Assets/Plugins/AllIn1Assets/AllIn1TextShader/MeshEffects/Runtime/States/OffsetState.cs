using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class OffsetState : MeshEffectState
	{
		public int indexThresholdToInvert;

		public OffsetState(EffectFlag effectFlag) : base(effectFlag)
		{

		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup)
		{
			OffsetStateValues values = vertexGroup.offsetStateValues;

			values.current = values.from;

			vertexGroup.offsetStateValues = values;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			OffsetStateValues values = vertexGroup.offsetStateValues;

			float easeT = values.easeFunction.Evaluate(values.normalizedProgress);
			values.current = Vector3.LerpUnclamped(values.from, values.to, easeT);

			if(values.normalizedProgress >= 1f && vertexGroup.playbackMode == PlaybackMode.LOOPED)
			{
				values.normalizedProgress = 0f;
			}

			vertexGroup.offsetStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{
			OffsetStateValues values = vertexGroup.offsetStateValues;

			values.current = values.to;

			vertexGroup.offsetStateValues = values; 
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout vertexGroup, Vector3 inputVertex)
		{
			OffsetStateValues values = vertexGroup.offsetStateValues;

			Vector3 res = inputVertex;

			res += values.current;

			return res;
		}

		public override Color32 TransformColor(ref VertexGroupLayout vertexGroup, Color32 inputColor)
		{
			Color32 res = inputColor;

			return res;
		}
	}
}