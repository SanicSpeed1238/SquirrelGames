using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class RainbowEffectState : MeshEffectState
	{
		public RainbowEffectState(EffectFlag effectFlag) : base(effectFlag)
		{
		}

		public override Color32 TransformColor(ref VertexGroupLayout groupLayout, Color32 inputColor)
		{
			RainbowStateValues values = groupLayout.rainbowStateValues;

			Color32 dstColor = MeshEffectUtils.RAINBOW_COLORS[values.rainbowIndex];
			Color32 res = Color32.Lerp(values.from, dstColor, values.normalizedProgress);
			res.a = inputColor.a;

			return res;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout groupLayout, Vector3 inputVertex)
		{
			return inputVertex;
		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup)
		{
			RainbowStateValues values = vertexGroup.rainbowStateValues;

			values.from = Color.white;
			values.rainbowIndex = 0;
			values.normalizedProgress = 0f;


			vertexGroup.rainbowStateValues = values;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup)
		{
			RainbowStateValues values = vertexGroup.rainbowStateValues;

			if(values.normalizedProgress >= 1f)
			{
				values.from = MeshEffectUtils.RAINBOW_COLORS[values.rainbowIndex];
				values.normalizedProgress = 0f;

				values.rainbowIndex++;
				if(values.rainbowIndex >= MeshEffectUtils.RAINBOW_COLORS.Length)
				{
					values.rainbowIndex = 0;
				}
			}
			
			vertexGroup.rainbowStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{

		}
	}
}