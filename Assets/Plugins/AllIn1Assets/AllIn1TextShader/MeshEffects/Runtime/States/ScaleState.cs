using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class ScaleState : MeshEffectState
	{
		public ScaleState(EffectFlag effectFlag) : base(effectFlag)
		{
			
		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup)
		{
			ScaleStateValues values = vertexGroup.scaleStateValues;

			values.current = values.from;
			
			vertexGroup.scaleStateValues = values;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			ScaleStateValues values = vertexGroup.scaleStateValues;

			float easeT = values.easeFunction.Evaluate(values.normalizedProgress);
			values.current = Vector2.LerpUnclamped(values.from, values.to, easeT);

			if(values.normalizedProgress >= 1f && vertexGroup.playbackMode == PlaybackMode.LOOPED)
			{
				values.normalizedProgress = 0f;
			}

			vertexGroup.scaleStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{
			ScaleStateValues values = vertexGroup.scaleStateValues;
			values.current = values.to;
			vertexGroup.scaleStateValues = values;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout vertexGroup, Vector3 inputVertex)
		{
			ScaleStateValues values = vertexGroup.scaleStateValues;

			Vector3 scaleDir = (inputVertex - vertexGroup.center);

			//Along the letter's own axes, so a squash after a rotation effect does not shear it
			Vector2 right = vertexGroup.right;
			Vector2 up = new Vector2(-right.y, right.x);
			float alongRight = (scaleDir.x * right.x + scaleDir.y * right.y) * values.current.x;
			float alongUp = (scaleDir.x * up.x + scaleDir.y * up.y) * values.current.y;

			Vector3 vectorScaled = new Vector3
			(
				right.x * alongRight + up.x * alongUp,
				right.y * alongRight + up.y * alongUp,
				scaleDir.z
			);

			Vector3 res = vertexGroup.center + vectorScaled;

			return res;
		}

		public override Color32 TransformColor(ref VertexGroupLayout groupLayout, Color32 inputColor)
		{
			Color32 res = inputColor;

			return res;
		}
	}
}