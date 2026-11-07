using System;
using System.Collections.Generic;

namespace AllIn1Assets.AllIn1TextShader
{
	public class MeshEffectsDispatcher
	{
		private const string STR_REVEAL_STATE = "RevealState";
		private const string STR_LOOP_STATE = "LoopState";
		private const string STR_HIDE_STATE = "HideState";

		public event Action<MeshCache> OnEffectsApplied;
		private TextMeshHandler meshHandler;

		private MeshEffectGroupConfig revealConfig;
		private MeshEffectGroupConfig loopedConfig;
		private MeshEffectGroupConfig hideConfig;

		private MeshEffectGroupState revealState;
		private MeshEffectGroupState loopedState;
		private HideState hideState;

		private MeshEffectGroupState applyingState;

		private float timeScale;

		// While set, every group of the one-shot state sits at this progress instead of ticking; loops keep ticking
		private bool externalProgressActive;
		private float externalProgress;
		// When set, each group reads its progress from the quad it starts on instead of the shared value
		private float[] externalQuadProgress;

		private List<int> revealStartedGroups;

		public MeshEffectsDispatcher(TextMeshHandler meshHandler, 
			MeshEffectGroupConfig revealConfig, MeshEffectGroupConfig loopedConfig, MeshEffectGroupConfig hideConfig)
		{
			this.meshHandler = meshHandler;

			SetEffectGroupConfigurations(revealConfig, loopedConfig, hideConfig);
		}

		public void SetEffectGroupConfigurations(MeshEffectGroupConfig revealConfig, MeshEffectGroupConfig loopedConfig, MeshEffectGroupConfig hideConfig)
		{
			this.revealConfig = revealConfig;
			this.loopedConfig = loopedConfig;
			this.hideConfig = hideConfig;
		}

		public void Build(List<TextParsedGroup> parsedGroups)
		{
			GroupType revealGroupType = MeshEffectGroupState.GetGroupType(revealConfig);
			GroupType loopedGroupType = MeshEffectGroupState.GetGroupType(loopedConfig);

			EffectFlag effectsMaskReveal = meshHandler.BuildVertexGroupsReveal(revealGroupType, parsedGroups);
			EffectFlag effectsMaskLooped = meshHandler.BuildVertexGroupsLooped(loopedGroupType, parsedGroups);
			EffectFlag effectsMaskHide = meshHandler.BuildVertexGroupsHide(MeshEffectGroupState.GetGroupType(hideConfig), parsedGroups);

			MeshCache meshCache = meshHandler.GetMeshCache();
			this.revealState = new MeshEffectGroupState(revealConfig, ref meshCache.vertexGroupLayoutsReveal, PlaybackMode.ONE_SHOT, revealGroupType, effectsMaskReveal, STR_REVEAL_STATE);
			this.revealState.startedGroups = revealStartedGroups;
			this.loopedState = new MeshEffectGroupState(loopedConfig, ref meshCache.vertexGroupLayoutsLooped, PlaybackMode.LOOPED, loopedGroupType, effectsMaskLooped, STR_LOOP_STATE);
			this.hideState = new HideState(hideConfig, ref meshCache.vertexGroupLayoutsHide, STR_HIDE_STATE);

			this.applyingState = revealState;

			this.timeScale = 1f;
		}

		public void Update(float deltaTime)
		{
			MeshCache meshCache = meshHandler.GetMeshCache();
			float correctedDeltaTime = deltaTime * timeScale;

			if (externalProgressActive)
			{
				if (externalQuadProgress != null)
				{
					applyingState.SetProgressPerQuad(externalQuadProgress);
				}
				else
				{
					applyingState.SetUniformProgress(externalProgress);
				}
			}
			else
			{
				applyingState.Update(correctedDeltaTime, meshCache);
			}
			
			if(loopedState != null)
			{
				loopedState.Update(correctedDeltaTime, meshCache);
			}
			
			meshCache.Apply(applyingState, loopedState);

			if (OnEffectsApplied != null)
			{
				OnEffectsApplied(meshCache);
			}
		}

		// Re-dresses a fresh rest pose with the current state without advancing time
		public void Reapply()
		{
			if (applyingState == null) { return; }

			MeshCache meshCache = meshHandler.GetMeshCache();
			if (!LayoutsFit(meshCache, applyingState) || !LayoutsFit(meshCache, loopedState)) { return; }

			meshCache.Apply(applyingState, loopedState);
		}

		// Groups built for a previous text must not index a cache that was just re-snapshotted for a different one
		private static bool LayoutsFit(MeshCache meshCache, MeshEffectGroupState state)
		{
			if (state == null) { return true; }
			if (meshCache.subMeshes == null || meshCache.subMeshes.Length == 0) { return false; }

			int numVertices = meshCache.subMeshes[0].currentVerts != null ? meshCache.subMeshes[0].currentVerts.Length : 0;
			VertexGroupLayout[] layouts = state.vertexGroupLayouts;
			for (int i = 0; i < layouts.Length; i++)
			{
				if (layouts[i].endVertexIndex >= numVertices) { return false; }
			}

			return true;
		}

		public void Stop(MeshCache meshCache)
		{

		}

		public void Pause()
		{
			timeScale = 0f;
		}

		public void Resume()
		{
			timeScale = 1f;
		}

		public void Reveal()
		{
			timeScale = 1f;
			externalProgressActive = false;
			applyingState = revealState;
			applyingState.Reset();
		}

		public void RevealImmediate()
		{
			timeScale = 1f;
			externalProgressActive = false;
			applyingState = revealState;
			applyingState.FinishImmediate();
		}

		public void Hide()
		{
			timeScale = 1f;
			applyingState = hideState;
			applyingState.Reset();
		}

		public void HideImmediate()
		{
			timeScale = 1f;
			externalProgressActive = false;
			applyingState = hideState;
			applyingState.FinishImmediate();
		}

		// Reveal or hide driven from outside (the spline's travel): progress 0 is the start, 1 the end
		public void RevealAtProgress(float normalizedProgress)
		{
			SetExternalProgress(revealState, normalizedProgress);
		}

		public void HideAtProgress(float normalizedProgress)
		{
			SetExternalProgress(hideState, normalizedProgress);
		}

		// Per quad progress (index = vertex index / 4); groups take the progress of their first quad
		public void RevealAtProgress(float[] quadProgress)
		{
			SetExternalProgress(revealState, 0f);
			externalQuadProgress = quadProgress;
		}

		public void HideAtProgress(float[] quadProgress)
		{
			SetExternalProgress(hideState, 0f);
			externalQuadProgress = quadProgress;
		}

		private void SetExternalProgress(MeshEffectGroupState state, float normalizedProgress)
		{
			timeScale = 1f;
			if (applyingState != state || !externalProgressActive)
			{
				applyingState = state;
				applyingState.Reset();
			}

			externalProgressActive = true;
			externalProgress = UnityEngine.Mathf.Clamp01(normalizedProgress);
			externalQuadProgress = null;
		}

		// Lands the scrubbed state on its end values and hands control back to time
		public void FinishExternalProgress()
		{
			if (!externalProgressActive) { return; }

			externalProgressActive = false;
			externalQuadProgress = null;
			applyingState.FinishImmediate();
		}

		public bool IsExternalProgressActive()
		{
			return externalProgressActive;
		}

		// True while that one-shot is being scrubbed, so the next RevealAtProgress or HideAtProgress continues it instead of restarting
		public bool IsExternalProgressActive(VisibilityEffectType type)
		{
			MeshEffectGroupState state = type == VisibilityEffectType.REVEAL ? revealState : hideState;
			bool res = externalProgressActive && applyingState == state;
			return res;
		}

		// The timed reveal adds each group it starts to this list; it survives rebuilds
		public void RecordRevealStartedGroups(List<int> startedGroups)
		{
			revealStartedGroups = startedGroups;
			if (revealState != null)
			{
				revealState.startedGroups = startedGroups;
			}
		}

		public int GetRevealGroupStartVertex(int groupIndex)
		{
			if (revealState == null || groupIndex < 0 || groupIndex >= revealState.vertexGroupLayouts.Length) { return -1; }

			int res = revealState.vertexGroupLayouts[groupIndex].startVertexIndex;
			return res;
		}

		public bool IsRevealComplete()
		{
			bool res = revealState != null && revealState.IsComplete();
			return res;
		}

		public bool IsHideComplete()
		{
			bool res = hideState != null && hideState.IsComplete();
			return res;
		}

		public float GetRevealProgressNormalized()
		{
			float res = revealState.GetProgressNormalized();
			return res;
		}

		public float GetHideProgressNormalized()
		{
			float res = hideState.GetProgressNormalized();
			return res;
		}

		public bool IsRevealed()
		{
			bool res = revealState.GetProgressNormalized() >= 1.0f;
			return res;
		}

		public bool IsHidden()
		{
			bool res = hideState.GetProgressNormalized() >= 1.0f;
			return res;
		}

#if UNITY_EDITOR
		public void UpdatePreview(float normalizedTime, float deltaTime, PreviewMode previewMode)
		{
			MeshCache meshCache = meshHandler.GetMeshCache();

			applyingState = GetPreviewRevealState(previewMode);

			ref VertexGroupLayout[] vertexGroupLayoutRef = ref meshCache.GetVertexGroupLayoutByPreviewMode(previewMode);

			if (previewMode == PreviewMode.PREVIEW_LOOPED)
			{
				if(applyingState.groupType == GroupType.PER_TAGS)
				{
					StartAllEffectsAtTheSameTime(ref vertexGroupLayoutRef);
				}

				applyingState.Update(deltaTime, meshCache);
			}
			else
			{
				applyingState.UpdatePreview(normalizedTime, meshCache);
			}

			meshCache.ApplyPreview(applyingState, previewMode);
		}

		public void ReapplyPreview(PreviewMode previewMode)
		{
			if (applyingState == null) { return; }

			MeshCache meshCache = meshHandler.GetMeshCache();
			if (!LayoutsFit(meshCache, applyingState)) { return; }

			meshCache.ApplyPreview(applyingState, previewMode);
		}

		private void StartAllEffectsAtTheSameTime(ref VertexGroupLayout[] vertexGroupLayoutRef)
		{
			for (int i = 0; i < vertexGroupLayoutRef.Length; i++)
			{
				VertexGroupLayout groupLayout = vertexGroupLayoutRef[i];

				groupLayout.displayState = VertexGroupDisplayState.Revealing;

				vertexGroupLayoutRef[i] = groupLayout;
			}
		}

		private MeshEffectGroupState GetPreviewRevealState(PreviewMode previewMode)
		{
			MeshEffectGroupState res = null;

			switch (previewMode)
			{
				case PreviewMode.PREVIEW_HIDE:
					res = hideState;
					break;
				case PreviewMode.PREVIEW_LOOPED:
					res = loopedState;
					break;
				case PreviewMode.PREVIEW_REVEAL:
					res = revealState;
					break;
			}

			return res;
		}

		public VertexGroupLayout[] GetVertexGroupLayoutRef(PreviewMode previewMode)
		{
			MeshCache meshCache = meshHandler.GetMeshCache();

			VertexGroupLayout[] res = null;
			switch (previewMode)
			{
				case PreviewMode.PREVIEW_HIDE:
					res = meshCache.vertexGroupLayoutsReveal;
					break;
				case PreviewMode.PREVIEW_LOOPED:
					res = meshCache.vertexGroupLayoutsLooped;
					break;
				case PreviewMode.PREVIEW_REVEAL:
					res = meshCache.vertexGroupLayoutsReveal;
					break;
			}
			return res;
		}
#endif
	}
}