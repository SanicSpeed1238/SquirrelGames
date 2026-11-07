using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public abstract class MeshEffectState
	{
		public EffectFlag effectMask;

		public MeshEffectState(EffectFlag effectMask)
		{
			this.effectMask = effectMask;
		}

		// A tagged group only runs the effects its tag named; every other group enables them all
		public bool IsEnabledIn(ref VertexGroupLayout vertexGroup)
		{
			return (effectMask & vertexGroup.enabledEffectsMask) == effectMask;
		}

		public virtual void Update(float deltaTime, ref VertexGroupLayout vertexGroup)
		{
			if (!IsEnabledIn(ref vertexGroup)) { return; }

			switch (vertexGroup.displayState)
			{
				case VertexGroupDisplayState.Hidden:
					UpdateCharacterStateHidden(ref vertexGroup);
					break;
				case VertexGroupDisplayState.Revealing:
					UpdateCharacterStateRevealing(ref vertexGroup);
					break;
				case VertexGroupDisplayState.Revealed:
					UpdateCharacterStateRevealed(ref vertexGroup);
					break;
			}
		}

#region PREVIEW
		public void UpdatePreview(ref VertexGroupLayout vertexGroup)
		{
			if (!IsEnabledIn(ref vertexGroup)) { return; }

			switch (vertexGroup.displayState)
			{
				case VertexGroupDisplayState.Hidden:
					UpdateCharacterStateHidden(ref vertexGroup);
					break;
				case VertexGroupDisplayState.Revealing:
					UpdateCharacterStateRevealingNormalizedProgress(ref vertexGroup, vertexGroup.timerNormalized);
					break;
				case VertexGroupDisplayState.Revealed:
					UpdateCharacterStateRevealed(ref vertexGroup);
					break;
			}
		}
#endregion

		// Position-driven callers (the spline) set the state and progress directly instead of ticking time
		public void UpdateAtProgress(ref VertexGroupLayout vertexGroup, VertexGroupDisplayState displayState, float normalizedProgress)
		{
			vertexGroup.displayState = displayState;

			if (!IsEnabledIn(ref vertexGroup)) { return; }

			switch (displayState)
			{
				case VertexGroupDisplayState.Hidden:
					UpdateCharacterStateHidden(ref vertexGroup);
					break;
				case VertexGroupDisplayState.Revealing:
					UpdateCharacterStateRevealingNormalizedProgress(ref vertexGroup, normalizedProgress);
					break;
				case VertexGroupDisplayState.Revealed:
					UpdateCharacterStateRevealed(ref vertexGroup);
					break;
			}
		}

		protected abstract void UpdateCharacterStateHidden(ref VertexGroupLayout vertexGroup);

		protected void UpdateCharacterStateRevealingNormalizedProgress(ref VertexGroupLayout vertexGroup, float normalizedProgress)
		{
			vertexGroup.SetNormalizedProgress(normalizedProgress);
			UpdateCharacterStateValues(ref vertexGroup);
		}

		protected void UpdateCharacterStateRevealing(ref VertexGroupLayout vertexGroup)
		{
			UpdateCharacterStateValues(ref vertexGroup);
		}

		protected abstract void UpdateCharacterStateValues(ref VertexGroupLayout vertexGroup);

		protected abstract void UpdateCharacterStateRevealed(ref VertexGroupLayout vertexGroup);

		public abstract Vector3 TransformVertex(ref VertexGroupLayout vertexGroup, Vector3 inputVertex);

		public abstract Color32 TransformColor(ref VertexGroupLayout vertexGroup, Color32 inputColor);

		public virtual void Reset(ref VertexGroupLayout vertexGroup)
		{
			
		}
	}
}