using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class SlideEffectState : MeshEffectState
	{
		public SlideEffectState(EffectFlag effectFlag) : base(effectFlag)
		{
		}

		public override Color32 TransformColor(ref VertexGroupLayout groupLayout, Color32 inputColor)
		{
			return inputColor;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout groupLayout, Vector3 inputVertex)
		{
			SlideStateValues values = groupLayout.slideStateValues;

			Vector3 res = inputVertex;
			if (inputVertex.y < groupLayout.center.y)
			{
				res += Vector3.right * values.current;
			}
			else
			{
				res -= Vector3.right * values.current;
			}

			return res;
		}

		protected override void UpdateCharacterStateHidden(ref VertexGroupLayout groupLayout)
		{
			SlideStateValues values = groupLayout.slideStateValues;

			values.from = -values.length;
			values.to = values.length;
			values.normalizedProgress = 0.5f;
			values.current = 0f;

			groupLayout.slideStateValues = values;
		}

		protected override void UpdateCharacterStateValues(ref VertexGroupLayout groupLayout)
		{
			SlideStateValues values = groupLayout.slideStateValues;

			if (values.normalizedProgress >= 1f)
			{
				if (values.current > 0)
				{
					values.from = values.length;
					values.to = -values.length;
				}
				else
				{
					values.from = -values.length;
					values.to = values.length;
				}

				values.normalizedProgress = 0f;
			}


			float easeT = EasingFunctions.EaseInOutSine(values.normalizedProgress);
			values.current = Mathf.Lerp(values.from, values.to, easeT);
			
			groupLayout.slideStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout groupLayout)
		{
			
		}
	}
}