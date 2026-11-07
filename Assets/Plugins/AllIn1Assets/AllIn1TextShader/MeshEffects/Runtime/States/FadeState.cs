using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class FadeState : MeshEffectState
	{
		private const string TAG = "FadeState";

		public FadeState(EffectFlag effectFlag) : base(effectFlag)
		{
		
		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup)
		{
			FadeStateValues values = vertexGroup.alphaStateValues;

			values.current = values.from;
			
			vertexGroup.alphaStateValues = values;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			FadeStateValues values = vertexGroup.alphaStateValues;

			float easeT = values.easeFunction.Evaluate(values.normalizedProgress);
			values.current = Mathf.LerpUnclamped(values.from, values.to, easeT);
			
			if(vertexGroup.playbackMode == PlaybackMode.LOOPED && values.normalizedProgress >= 1.0f)
			{
				values.normalizedProgress = 0f;
			}
			
			vertexGroup.alphaStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{
			FadeStateValues values = vertexGroup.alphaStateValues;
			
			values.current = values.to;
			
			vertexGroup.alphaStateValues = values;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout vertexGroup, Vector3 inputVertex)
		{
			Vector3 res = inputVertex;

			return res;
		}

		public override Color32 TransformColor(ref VertexGroupLayout vertexGroup, Color32 inputColor)
		{
			Color32 res = inputColor;

			// Multiplies so fades from different systems (reveal, loop, spline) combine instead of the last one winning
			FadeStateValues values = vertexGroup.alphaStateValues;
			res.a = (byte)(inputColor.a * Mathf.Clamp01(values.current));

			return res;
		}
	}
}