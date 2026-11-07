using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class SwingEffectState : MeshEffectState
	{
		public SwingEffectState(EffectFlag effectFlag) : base(effectFlag)
		{
		}

		public override Color32 TransformColor(ref VertexGroupLayout groupLayout, Color32 inputColor)
		{
			return inputColor;
		}

		public override Vector3 TransformVertex(ref VertexGroupLayout groupLayout, Vector3 inputVertex)
		{
			SwingStateValues values = groupLayout.swingStateValues;

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
			SwingStateValues values = vertexGroup.swingStateValues;

			//float easeT = EasingFunctions.EaseInOutSine(values.normalizedProgress);


			/*
			if (values.normalizedProgress >= 1f)
			{
				if (values.fromRotation > 0)
				{
					values.fromRotation = -values.swingAmount;
					values.toRotation = values.swingAmount;
				}
				else
				{
					values.fromRotation = values.swingAmount;
					values.toRotation = -values.swingAmount;
				}

				values.normalizedProgress = 0f;
			}
			*/
			if (values.normalizedProgress >= 1f)
			{
				values.normalizedProgress = 0f;
			}

			float easeT = values.easeFunction.Evaluate(values.normalizedProgress);
			values.currentRotation = Mathf.Lerp(-values.swingAmount, values.swingAmount, easeT);
			
			vertexGroup.swingStateValues = values;
		}

		protected override void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup)
		{

		}
	}
}