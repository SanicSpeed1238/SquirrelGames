using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class ShakeEffectState : MeshEffectState
	{
		public ShakeEffectState(EffectFlag effectFlag) : base(effectFlag)
		{
		}

		public override Color32 TransformColor(ref VertexGroupLayout groupLayout, Color32 inputColor)
		{
			return inputColor;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout groupLayout, Vector3 inputVertex)
		{
			ShakeStateValues values = groupLayout.shakeStateValues;

			Vector3 offset = Vector3.Lerp(values.fromOffset, values.toOffset, values.normalizedDisplacement);
			Vector3 res = inputVertex + offset;

			return res;
		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup)
		{
			ShakeStateValues values = vertexGroup.shakeStateValues;

			Vector3 randomDir = Random.onUnitSphere;
			randomDir.z = 0f;

			Vector3 newOffset = randomDir * values.shakeLength;

			values.fromOffset = Vector3.zero;
			values.toOffset = newOffset;
			values.normalizedDisplacement = 0f;

			vertexGroup.shakeStateValues = values;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			ShakeStateValues values = vertexGroup.shakeStateValues;

			if(values.normalizedDisplacement >= 1f)
			{
				Vector3 randomDir = Random.onUnitSphere;
				randomDir.z = 0f;

				Vector3 newOffset = randomDir * values.shakeLength;

				values.fromOffset = values.toOffset;
				values.toOffset = newOffset;

				values.normalizedDisplacement = 0f;
			}


			vertexGroup.shakeStateValues = values;
			
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{

		}
	}
}