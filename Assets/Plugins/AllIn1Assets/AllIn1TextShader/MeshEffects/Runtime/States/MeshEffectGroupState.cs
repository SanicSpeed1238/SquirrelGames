using System.Collections.Generic;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class MeshEffectGroupState
	{
		protected float revealTimer;

		protected int currentGroupIndex;
		protected int minCharIndex;
		protected int maxCharIndex;

		protected float characterDelay;

		protected PlaybackMode playbackMode;
		protected RevealAnimationState animationState;
		protected TraversalOrder traversalOrder;
		protected EffectFlag enabledEffectsMask;

		public VertexGroupLayout[] vertexGroupLayouts;
		public List<MeshEffectState> states;
		public GroupType groupType;

		// When set, each group the timed traversal starts is added here (the typewriter's per character event drains it)
		public List<int> startedGroups;

		public MeshEffectGroupState(MeshEffectGroupConfig meshEffectGroupConfig, ref VertexGroupLayout[] vertexGroupLayouts, 
			PlaybackMode playbackMode, GroupType groupType, EffectFlag enabledEffects, string groupStateName)
		{
			this.vertexGroupLayouts = vertexGroupLayouts;

			states = new List<MeshEffectState>();
			this.groupType = groupType;

			this.revealTimer = 0f;
			this.playbackMode = playbackMode;
			this.enabledEffectsMask = enabledEffects;

			// A missing preset (its asset was deleted) plays no effects instead of throwing
			if (meshEffectGroupConfig != null)
			{
				this.traversalOrder = meshEffectGroupConfig.traversalOrder;
				this.characterDelay = meshEffectGroupConfig.groupDelay;
			}

			SetInitialElementIndex(vertexGroupLayouts.Length);

			Reset();

			if (meshEffectGroupConfig != null)
			{
				meshEffectGroupConfig.PopulateStates(states, ref vertexGroupLayouts, playbackMode, groupType, enabledEffects);
			}
		}

		public static GroupType GetGroupType(MeshEffectGroupConfig meshEffectGroupConfig)
		{
			GroupType res = meshEffectGroupConfig != null ? meshEffectGroupConfig.groupType : GroupType.PER_CHARACTER;

			// Tags only drive loops: a hide can't reverse a tag's values, so a reveal or hide set to tags plays per character
			if (res == GroupType.PER_TAGS && !meshEffectGroupConfig.isLooped)
			{
				res = GroupType.PER_CHARACTER;
			}

			return res;
		}

		public void Update(float deltaTime, MeshCache meshCache)
		{
			UpdateRevealState(deltaTime);

			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				VertexGroupLayout groupLayout = vertexGroupLayouts[i];
				groupLayout.Update(deltaTime);

				UpdateMeshEffectStates(deltaTime, ref groupLayout);

				vertexGroupLayouts[i] = groupLayout;
			}

			if (currentGroupIndex >= vertexGroupLayouts.Length)
			{
				animationState = RevealAnimationState.FINISHED;
			}
		}

		private void UpdateMeshEffectStates(float deltaTime, ref VertexGroupLayout vertexGroup)
		{
			for (int i = 0; i < states.Count; i++)
			{
				states[i].Update(deltaTime, ref vertexGroup);
			}
		}

		public void Reset(RevealAnimationState revealAnimationState = RevealAnimationState.REVEALING)
		{
			int numGroups = vertexGroupLayouts.Length;
			SetInitialElementIndex(numGroups);

			animationState = revealAnimationState;
			revealTimer = 0f;

			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				VertexGroupLayout vertexGroup = vertexGroupLayouts[i];
				vertexGroup.Reset();

				for (int stateIdx = 0; stateIdx < states.Count; stateIdx++)
				{
					states[stateIdx].Reset(ref vertexGroupLayouts[i]);
				}

				vertexGroupLayouts[i] = vertexGroup;
			}
		}

		// Every group at the same progress, for a caller whose clock is not time (the spline's travel). No group delay, no traversal order
		public void SetUniformProgress(float normalizedProgress)
		{
			normalizedProgress = Mathf.Clamp01(normalizedProgress);

			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				for (int stateIdx = 0; stateIdx < states.Count; stateIdx++)
				{
					states[stateIdx].UpdateAtProgress(ref vertexGroupLayouts[i], VertexGroupDisplayState.Revealing, normalizedProgress);
				}
			}
		}

		// Each group at the progress of the quad it starts on. Quads past the array's end count as finished
		public void SetProgressPerQuad(float[] quadProgress)
		{
			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				int quad = vertexGroupLayouts[i].startVertexIndex / 4;
				float progress = quad < quadProgress.Length ? Mathf.Clamp01(quadProgress[quad]) : 1f;

				for (int stateIdx = 0; stateIdx < states.Count; stateIdx++)
				{
					states[stateIdx].UpdateAtProgress(ref vertexGroupLayouts[i], VertexGroupDisplayState.Revealing, progress);
				}
			}
		}

		public void FinishImmediate()
		{
			int numGroups = vertexGroupLayouts.Length;
			// Past the last group: back on the first one, the one shot timer walked every group back to Revealing and the unstarted letters typed again
			SetFinishedElementIndex(numGroups);

			animationState = RevealAnimationState.FINISHED;
			revealTimer = 0f;

			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				VertexGroupLayout vertexGroup = vertexGroupLayouts[i];
				vertexGroup.SetDisplayState(VertexGroupDisplayState.Revealed);
				// Every effect at its end, so the progress reads done (it stayed where each effect was)
				vertexGroup.SetNormalizedProgress(1f);

				vertexGroupLayouts[i] = vertexGroup;
			}
		}

		private void UpdateRevealState(float deltaTime)
		{
			if (playbackMode == PlaybackMode.LOOPED)
			{
				UpdateRevealTimerLooped();
			}
			else
			{
				UpdateRevealTimerOneShot(deltaTime);
			}
		}

		private void UpdateRevealTimerLooped()
		{
			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				SetVertexGroupDisplayState(VertexGroupDisplayState.Revealing, i);
			}
		}

		private void UpdateRevealTimerOneShot(float deltaTime)
		{
			if (currentGroupIndex >= vertexGroupLayouts.Length || currentGroupIndex < 0) { return; }

			revealTimer += deltaTime;
			// Every group due in this step starts and the rest carries over, so the pace doesn't follow the frame rate (bounded for a zero delay)
			for (int steps = 0; steps <= vertexGroupLayouts.Length && revealTimer >= characterDelay; steps++)
			{
				SetCurrentVertexGroupDisplayState(VertexGroupDisplayState.Revealing);
				EnableNextElement();

				revealTimer = characterDelay > 0f ? revealTimer - characterDelay : 0f;
				if (currentGroupIndex >= vertexGroupLayouts.Length || currentGroupIndex < 0) { break; }
			}
		}

		private void SetVertexGroupDisplayState(VertexGroupDisplayState displayState, int groupIndex)
		{
			if (groupIndex >= vertexGroupLayouts.Length || groupIndex < 0) { return; }

			VertexGroupLayout vertexGroup = vertexGroupLayouts[groupIndex];
			if (startedGroups != null && vertexGroup.displayState == VertexGroupDisplayState.Hidden && displayState == VertexGroupDisplayState.Revealing)
			{
				startedGroups.Add(groupIndex);
			}
			SetVertexGroupDisplayState(displayState, ref vertexGroup);
			vertexGroupLayouts[groupIndex] = vertexGroup;
		}

		private void SetVertexGroupDisplayState(VertexGroupDisplayState displayState, ref VertexGroupLayout vertexGroup)
		{
			vertexGroup.displayState = displayState;
		}

		private void SetCurrentVertexGroupDisplayState(VertexGroupDisplayState displayState)
		{
			switch (traversalOrder)
			{
				case TraversalOrder.LEFT_TO_RIGHT:
				case TraversalOrder.RIGHT_TO_LEFT:
					SetVertexGroupDisplayState(displayState, currentGroupIndex);
					break;
				case TraversalOrder.FROM_CENTER_TO_OUTWARD:
				case TraversalOrder.FROM_OUTWARD_TO_CENTER:
					SetVertexGroupDisplayState(displayState, minCharIndex);
					SetVertexGroupDisplayState(displayState, maxCharIndex);
					break;
			}
		}

		private void EnableNextElement()
		{
			switch (traversalOrder)
			{
				case TraversalOrder.LEFT_TO_RIGHT:
					currentGroupIndex++;
					break;
				case TraversalOrder.RIGHT_TO_LEFT:
					currentGroupIndex--;
					break;
				case TraversalOrder.FROM_CENTER_TO_OUTWARD:
					minCharIndex--;
					maxCharIndex++;
					break;
				case TraversalOrder.FROM_OUTWARD_TO_CENTER:
					minCharIndex++;
					maxCharIndex--;
					break;
			}
		}

		private void SetFinishedElementIndex(int numGroups)
		{
			switch (traversalOrder)
			{
				case TraversalOrder.RIGHT_TO_LEFT:
					currentGroupIndex = -1;
					break;
				case TraversalOrder.LEFT_TO_RIGHT:
					currentGroupIndex = numGroups;
					break;
				case TraversalOrder.FROM_CENTER_TO_OUTWARD:
					minCharIndex = -1;
					maxCharIndex = numGroups;
					break;
				case TraversalOrder.FROM_OUTWARD_TO_CENTER:
					minCharIndex = numGroups;
					maxCharIndex = -1;
					break;
			}
		}

		private void SetInitialElementIndex(int numGroups)
		{
			switch (traversalOrder)
			{
				case TraversalOrder.RIGHT_TO_LEFT:
					currentGroupIndex = numGroups - 1;
					break;
				case TraversalOrder.LEFT_TO_RIGHT:
					currentGroupIndex = 0;
					break;
				case TraversalOrder.FROM_CENTER_TO_OUTWARD:
					minCharIndex = Mathf.FloorToInt(numGroups * 0.5f);
					maxCharIndex = minCharIndex + 1;
					break;
				case TraversalOrder.FROM_OUTWARD_TO_CENTER:
					minCharIndex = 0;
					maxCharIndex = numGroups - 1;
					break;
			}
		}

		public virtual void Reset(ref VertexGroupLayout[] vertexGroupLayouts)
		{
			int numGroups = vertexGroupLayouts.Length;
			SetInitialElementIndex(numGroups);

			animationState = RevealAnimationState.REVEALING;
			revealTimer = 0f;

			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				VertexGroupLayout vertexGroup = vertexGroupLayouts[i];

				vertexGroup.Reset();

				vertexGroupLayouts[i] = vertexGroup;
			}
		}

		public EffectFlag GetEnabledEffectsMask()
		{
			return enabledEffectsMask;
		}

		public float GetProgressNormalized()
		{
			// No groups (empty or whitespace text): nothing to wait for
			if (vertexGroupLayouts == null || vertexGroupLayouts.Length == 0)
			{
				return 1f;
			}

			float res = 0f;
			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				res += vertexGroupLayouts[i].GetProgressNormalized(states.Count);
			}

			res = res / vertexGroupLayouts.Length;
			return res;
		}

		// Every group started and every effect of it done. Unlike GetProgressNormalized it holds for no groups, no effects (NaN there) and FinishImmediate
		public bool IsComplete()
		{
			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				VertexGroupDisplayState displayState = vertexGroupLayouts[i].displayState;
				if (displayState == VertexGroupDisplayState.Hidden) { return false; }
				if (displayState == VertexGroupDisplayState.Revealing && !AreEffectsDone(ref vertexGroupLayouts[i])) { return false; }
			}

			return true;
		}

		// Each effect on its own: an effect's progress keeps counting past 1, so in the group average a fast one covers for a slow one
		private bool AreEffectsDone(ref VertexGroupLayout vertexGroup)
		{
			for (int i = 0; i < states.Count; i++)
			{
				if (!states[i].IsEnabledIn(ref vertexGroup)) { continue; }
				if (vertexGroup.GetEffectProgress(states[i].effectMask) < 1f) { return false; }
			}

			return true;
		}

		#region PREVIEW
		public float GetTotalTimePreview(ref VertexGroupLayout[] vertexGroups)
		{
			float res = 0f;

			switch (traversalOrder)
			{
				case TraversalOrder.RIGHT_TO_LEFT:
				case TraversalOrder.LEFT_TO_RIGHT:
					res = Mathf.Max((vertexGroups.Length - 2), 1) * characterDelay;
					break;
				case TraversalOrder.FROM_OUTWARD_TO_CENTER:
				case TraversalOrder.FROM_CENTER_TO_OUTWARD:
					res = Mathf.Max((vertexGroups.Length - 1), 1) * characterDelay * 0.5f;
					break;
			}

			return res;
		}

		public void UpdatePreview(float normalizedTime, MeshCache meshCache)
		{
			float totalTime = GetTotalTimePreview(ref vertexGroupLayouts);
			float currentTime = normalizedTime * totalTime;

			UpdateDisplayStatePreview(totalTime, currentTime);

			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				UpdateMeshEffectStatesPreview(normalizedTime, ref vertexGroupLayouts[i]);
			}
		}

		private void UpdateMeshEffectStatesPreview(float normalizedTime, ref VertexGroupLayout vertexGroup)
		{
			for (int i = 0; i < states.Count; i++)
			{
				states[i].UpdatePreview(ref vertexGroup); 
			}
		}

		protected void UpdateDisplayStatePreview(float totalTime, float currentTime)
		{
			switch (traversalOrder)
			{
				case TraversalOrder.LEFT_TO_RIGHT:
					UpdateDisplayStatePreviewLeftToRight(totalTime, currentTime);
					break;
				case TraversalOrder.RIGHT_TO_LEFT:
					UpdateDisplayStatePreviewRightToLeft(totalTime, currentTime);
					break;
				case TraversalOrder.FROM_CENTER_TO_OUTWARD:
					UpdateDisplayStatePreviewFromCenterToOutward(totalTime, currentTime);
					break;
				case TraversalOrder.FROM_OUTWARD_TO_CENTER:
					UpdateDisplayStatePreviewFromOutwardToCenter(totalTime, currentTime);
					break;
			}
		}

		private void UpdateDisplayStatePreviewLeftToRight(float totalTime, float currentTime)
		{
			int localIndex = 0;
			for (int i = 0; i < vertexGroupLayouts.Length; i++)
			{
				float normalizedIndexFloat = (localIndex * 1.0f) / (vertexGroupLayouts.Length);

				float fromTime = normalizedIndexFloat * totalTime;
				float toTime = fromTime + characterDelay;
				toTime = Mathf.Clamp(toTime, 0f, totalTime);

				VertexGroupLayout vertexGroup = vertexGroupLayouts[i];

				UpdateVertexGroupPreview(currentTime, fromTime, toTime, ref vertexGroup);

				vertexGroupLayouts[i] = vertexGroup;

				localIndex++;
			}
		}

		private void UpdateDisplayStatePreviewRightToLeft(float totalTime, float currentTime)
		{
			int localIndex = 0;
			for (int i = vertexGroupLayouts.Length - 1; i >= 0; i--)
			{
				float normalizedIndexFloat = (localIndex * 1.0f) / (vertexGroupLayouts.Length);

				float fromTime = normalizedIndexFloat * totalTime;
				float toTime = fromTime + characterDelay;
				toTime = Mathf.Clamp(toTime, 0f, totalTime);

				VertexGroupLayout vertexGroup = vertexGroupLayouts[i];

				UpdateVertexGroupPreview(currentTime, fromTime, toTime, ref vertexGroup);

				vertexGroupLayouts[i] = vertexGroup;

				localIndex++;
			}
		}

		private void UpdateDisplayStatePreviewFromCenterToOutward(float totalTime, float currentTime)
		{
			int halfLength = (int)(vertexGroupLayouts.Length * 0.5f);

			bool isOdd = (vertexGroupLayouts.Length % 2) != 0;

			int leftLocalIndex = halfLength - 1;
			int rightLocalIndex = halfLength;
			int localIndex = 0;

			for (int i = halfLength; i >= 0; i--)
			{
				float referenceSize = isOdd ? halfLength : halfLength + 1;

				float normalizedIndexFloat = (localIndex * 1.0f) / (referenceSize + 2);
				float fromTime = normalizedIndexFloat * totalTime;
				float toTime = fromTime + characterDelay;

				if (leftLocalIndex >= 0)
				{
					VertexGroupLayout leftVertexGroup = vertexGroupLayouts[leftLocalIndex];
					UpdateVertexGroupPreview(currentTime, fromTime, toTime, ref leftVertexGroup);
					vertexGroupLayouts[leftLocalIndex] = leftVertexGroup;
				}

				if (rightLocalIndex <= vertexGroupLayouts.Length - 1)
				{
					VertexGroupLayout rightVertexGroup = vertexGroupLayouts[rightLocalIndex];
					UpdateVertexGroupPreview(currentTime, fromTime, toTime, ref rightVertexGroup);
					vertexGroupLayouts[rightLocalIndex] = rightVertexGroup;
				}

				leftLocalIndex--;
				rightLocalIndex++;
				localIndex++;
			}
		}

		private void UpdateDisplayStatePreviewFromOutwardToCenter(float totalTime, float currentTime)
		{
			int halfLength = (int)(vertexGroupLayouts.Length * 0.5f);
			bool isOdd = (vertexGroupLayouts.Length % 2) != 0;

			int leftLocalIndex = 0;
			int rightLocalIndex = vertexGroupLayouts.Length - 1;
			int localIndex = 0;

			for (int i = halfLength; i >= 0; i--)
			{
				float referenceSize = isOdd ? halfLength : halfLength + 1;

				float normalizedIndexFloat = (localIndex * 1.0f) / (referenceSize + 2);
				float fromTime = normalizedIndexFloat * totalTime;
				float toTime = fromTime + characterDelay;

				if (leftLocalIndex < vertexGroupLayouts.Length)
				{
					VertexGroupLayout leftVertexGroup = vertexGroupLayouts[leftLocalIndex];
					UpdateVertexGroupPreview(currentTime, fromTime, toTime, ref leftVertexGroup);
					vertexGroupLayouts[leftLocalIndex] = leftVertexGroup;
				}

				if (rightLocalIndex >= 0)
				{
					VertexGroupLayout rightVertexGroup = vertexGroupLayouts[rightLocalIndex];
					UpdateVertexGroupPreview(currentTime, fromTime, toTime, ref rightVertexGroup);
					vertexGroupLayouts[rightLocalIndex] = rightVertexGroup;
				}

				leftLocalIndex++;
				rightLocalIndex--;
				localIndex++;
			}
		}

		private void UpdateVertexGroupPreview(float currentTime, float fromTime, float toTime, ref VertexGroupLayout vertexGroup)
		{
			vertexGroup.timerNormalized = 0f;

			if (currentTime < fromTime)
			{
				SetVertexGroupDisplayState(VertexGroupDisplayState.Hidden, ref vertexGroup);
			}
			else if (currentTime >= fromTime && currentTime <= toTime)
			{
				float localTimerNormalized = (currentTime - fromTime) / (toTime - fromTime);
				localTimerNormalized = Mathf.Clamp01(localTimerNormalized);
				vertexGroup.timerNormalized = localTimerNormalized;

				SetVertexGroupDisplayState(VertexGroupDisplayState.Revealing, ref vertexGroup);
			}
			else
			{
				SetVertexGroupDisplayState(VertexGroupDisplayState.Revealed, ref vertexGroup);
			}
		}
		#endregion
	}
}