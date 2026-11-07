using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class MeshCache
	{
		public struct SubMeshBuffers
		{
			// srcVerts is the flat rest pose, currentVerts is what the effects write, outputVerts is what the layout stage writes and TMP receives
			public Vector3[] srcVerts;
			public Color32[] srcColors;
			public Vector3[] currentVerts;
			public Color32[] currentColors;
			public Vector3[] outputVerts;
			public Color32[] outputColors;
			public Vector3[] normals;
			public Vector4[] tangents;

#if UNITY_6000_0_OR_NEWER
			public Vector4[] uvs0;
#else
			public Vector2[] uvs0;
#endif
			public Vector2[] uvs2;
		}

		public GroupType groupType;

		public SubMeshBuffers[] subMeshes;

		public VertexGroupLayout[] vertexGroupLayoutsReveal;
		public VertexGroupLayout[] vertexGroupLayoutsLooped;
		public VertexGroupLayout[] vertexGroupLayoutsHide;

		private bool[] ownedQuads = new bool[0];

		// Underline, strikethrough and mark slices written by TextDecorationBuilder into submesh 0: [decorationFirstVertex, decorationFirstVertex + decorationVertexCount)
		public int decorationFirstVertex;
		public int decorationVertexCount;

		// Extrude's back copies of the glyph and slice quads written by TextBackQuads: [backFirstVertex, backFirstVertex + backVertexCount), vertex v copied at backFirstVertex + v
		public int backFirstVertex;
		public int backVertexCount;

		public MeshCache()
		{
			subMeshes = new SubMeshBuffers[0];
			vertexGroupLayoutsReveal	= new VertexGroupLayout[0];
			vertexGroupLayoutsLooped	= new VertexGroupLayout[0];
			vertexGroupLayoutsHide		= new VertexGroupLayout[0];
		}

		public void Rebuild(TMP_Text text)
		{
			TMP_TextInfo info = text.textInfo;

			int count = 0;
			if(info != null && info.meshInfo != null)
			{
				count = info.meshInfo.Length;
			}

			if (subMeshes.Length != count)
			{
				subMeshes = new SubMeshBuffers[count];
			}

			for (int m = 0; m < count; m++)
			{
				Vector3[] srcVerts = info.meshInfo[m].vertices;
				Color32[] srcColors = info.meshInfo[m].colors32;
				Vector3[] normals = info.meshInfo[m].normals;
				Vector4[] tangents = info.meshInfo[m].tangents;

#if UNITY_6000_0_OR_NEWER
				Vector4[] uvs0 = info.meshInfo[m].uvs0;
#else
				Vector2[] uvs0 = info.meshInfo[m].uvs0;
#endif

				Vector2[] uvs2 = info.meshInfo[m].uvs2;

				int vertCount = srcVerts != null ? srcVerts.Length : 0;

				if (subMeshes[m].srcVerts == null || subMeshes[m].srcVerts.Length != vertCount)
				{
					subMeshes[m].srcVerts		= new Vector3[vertCount];
					subMeshes[m].srcColors		= new Color32[vertCount];
					subMeshes[m].currentVerts	= new Vector3[vertCount];
					subMeshes[m].currentColors	= new Color32[vertCount];
					subMeshes[m].outputVerts	= new Vector3[vertCount];
					subMeshes[m].outputColors	= new Color32[vertCount];
					subMeshes[m].normals		= new Vector3[vertCount];
					subMeshes[m].tangents		= new Vector4[vertCount];

#if UNITY_6000_0_OR_NEWER
					subMeshes[m].uvs0			= new Vector4[vertCount];
#else
					subMeshes[m].uvs0			= new Vector2[vertCount];
#endif

					subMeshes[m].uvs2			= new Vector2[vertCount];
				}

				for (int i = 0; i < vertCount; i++)
				{
					subMeshes[m].srcVerts[i]		= srcVerts[i];
					subMeshes[m].srcColors[i]		= srcColors[i];
					subMeshes[m].currentVerts[i]	= srcVerts[i];
					subMeshes[m].currentColors[i]	= srcColors[i];
					subMeshes[m].outputVerts[i]		= srcVerts[i];
					subMeshes[m].outputColors[i]	= srcColors[i];
					subMeshes[m].normals[i]			= normals[i];
					subMeshes[m].tangents[i]		= tangents[i];
					subMeshes[m].uvs0[i]			= uvs0[i];
					subMeshes[m].uvs2[i]			= uvs2[i];
				}

				// TMP clears the other sub meshes' unused vertices only when it uploads, after this snapshot, so a write back would bring old glyphs back
				if (m > 0)
				{
					ClearUnusedVertices(ref subMeshes[m], Mathf.Clamp(info.meshInfo[m].vertexCount, 0, vertCount));
				}
			}

			CollapseQuadsWithoutGlyph(info);
		}

		private static void ClearUnusedVertices(ref SubMeshBuffers subMesh, int vertexCount)
		{
			int length = subMesh.srcVerts.Length - vertexCount;
			if (length <= 0) { return; }

			System.Array.Clear(subMesh.srcVerts, vertexCount, length);
			System.Array.Clear(subMesh.currentVerts, vertexCount, length);
			System.Array.Clear(subMesh.outputVerts, vertexCount, length);
		}

		// TMP's underline, strikethrough and mark quads follow the glyphs in submesh 0 with no per-character data behind them, so nothing that is not a glyph is drawn
		private void CollapseQuadsWithoutGlyph(TMP_TextInfo info)
		{
			if (subMeshes.Length == 0 || subMeshes[0].srcVerts == null) { return; }

			ref SubMeshBuffers subMesh = ref subMeshes[0];
			int numQuads = subMesh.srcVerts.Length / 4;
			MarkOwnedQuads(info, numQuads, decorationFirstVertex, decorationVertexCount, backFirstVertex, backVertexCount, ref ownedQuads);

			CollapseUnmarkedQuads(subMesh.srcVerts, numQuads, ownedQuads);
			CollapseUnmarkedQuads(subMesh.currentVerts, numQuads, ownedQuads);
			CollapseUnmarkedQuads(subMesh.outputVerts, numQuads, ownedQuads);
		}

		public bool IsDecorationVertex(int vertexIndex)
		{
			return vertexIndex >= decorationFirstVertex && vertexIndex < decorationFirstVertex + decorationVertexCount;
		}

		public bool IsBackVertex(int vertexIndex)
		{
			return vertexIndex >= backFirstVertex && vertexIndex < backFirstVertex + backVertexCount;
		}

		// Back copies are marked too: they mirror their glyph, collapsed or not, when the output is finished
		private static void MarkOwnedQuads(TMP_TextInfo info, int numQuads, int decorationFirstVertex, int decorationVertexCount, int backFirstVertex, int backVertexCount, ref bool[] ownedQuads)
		{
			if (ownedQuads.Length < numQuads)
			{
				ownedQuads = new bool[numQuads];
			}
			System.Array.Clear(ownedQuads, 0, numQuads);

			int numCharacters = GetCharacterCount(info);
			for (int c = 0; c < numCharacters; c++)
			{
				ref TMP_CharacterInfo characterInfo = ref info.characterInfo[c];
				if (!OwnsMainQuad(ref characterInfo)) { continue; }

				int quad = characterInfo.vertexIndex / 4;
				if (quad < numQuads)
				{
					ownedQuads[quad] = true;
				}
			}

			int lastDecorationQuad = Mathf.Min(numQuads, (decorationFirstVertex + decorationVertexCount) / 4);
			for (int quad = decorationFirstVertex / 4; quad < lastDecorationQuad; quad++)
			{
				ownedQuads[quad] = true;
			}

			int lastBackQuad = Mathf.Min(numQuads, (backFirstVertex + backVertexCount) / 4);
			for (int quad = backFirstVertex / 4; quad < lastBackQuad; quad++)
			{
				ownedQuads[quad] = true;
			}
		}

		private static void CollapseUnmarkedQuads(Vector3[] vertices, int numQuads, bool[] ownedQuads)
		{
			for (int quad = 0; quad < numQuads; quad++)
			{
				if (ownedQuads[quad]) { continue; }

				int first = quad * 4;
				Vector3 collapsed = vertices[first];
				vertices[first + 1] = collapsed;
				vertices[first + 2] = collapsed;
				vertices[first + 3] = collapsed;
			}
		}

		public EffectFlag BuildVertexReveal(TMP_TextInfo textInfo, GroupType groupType, List<TextParsedGroup> parsedGroups)
		{
			EffectFlag res = BuildVertexGroups(textInfo, groupType, parsedGroups, PlaybackMode.ONE_SHOT, ref vertexGroupLayoutsReveal);
			return res;
		}

		public EffectFlag BuildVertexGroupsLooped(TMP_TextInfo textInfo, GroupType groupType, List<TextParsedGroup> parsedGroups)
		{
			EffectFlag res = BuildVertexGroups(textInfo, groupType, parsedGroups, PlaybackMode.LOOPED, ref vertexGroupLayoutsLooped);

			for(int i = 0; i < vertexGroupLayoutsLooped.Length; i++)
			{
				VertexGroupLayout vertexGroupLayout = vertexGroupLayoutsLooped[i];

				vertexGroupLayout.displayState = VertexGroupDisplayState.Revealing;

				vertexGroupLayoutsLooped[i] = vertexGroupLayout;
			}

			return res;
		}

		public EffectFlag BuildVertexHide(TMP_TextInfo textInfo, GroupType groupType, List<TextParsedGroup> parsedGroups)
		{
			EffectFlag res = BuildVertexGroups(textInfo, groupType, parsedGroups, PlaybackMode.ONE_SHOT, ref vertexGroupLayoutsHide);
			return res;
		}

		// Groups come from TMP's own character list: rich text tags never reach it and vertexIndex says where each glyph's quad sits,
		// so the string is never counted. Only main material glyphs are grouped, fallback font and sprite glyphs live in other submeshes
		public EffectFlag BuildVertexGroups(TMP_TextInfo textInfo, GroupType groupType, List<TextParsedGroup> parsedGroups, PlaybackMode playbackMode, ref VertexGroupLayout[] vertexGroupLayouts)
		{
			EffectFlag res = EffectFlag.All;

			switch (groupType)
			{
				case GroupType.PER_CHARACTER:
					InitVertexLayoutsPerCharacter(textInfo, playbackMode, ref vertexGroupLayouts);
					break;
				case GroupType.PER_WORD:
					InitVertexLayoutsPerWord(textInfo, playbackMode, ref vertexGroupLayouts);
					break;
				case GroupType.PER_TAGS:
					res = InitVertexLayoutsPerTags(textInfo, parsedGroups, playbackMode, ref vertexGroupLayouts);
					break;
			}

			return res;
		}

		private static int GetCharacterCount(TMP_TextInfo textInfo)
		{
			int res = 0;
			if (textInfo != null && textInfo.characterInfo != null)
			{
				res = Mathf.Min(textInfo.characterCount, textInfo.characterInfo.Length);
			}
			return res;
		}

		private static bool OwnsMainQuad(ref TMP_CharacterInfo characterInfo)
		{
			return characterInfo.isVisible && characterInfo.materialReferenceIndex == 0;
		}

		private static VertexGroupLayout CreateGroupLayout(int groupIndex, int startVertexIndex, int endVertexIndex, PlaybackMode playbackMode)
		{
			VertexGroupLayout res = new VertexGroupLayout();
			res.groupIndex = groupIndex;
			res.startVertexIndex = startVertexIndex;
			res.endVertexIndex = endVertexIndex;
			res.enabledEffectsMask = EffectFlag.All;
			res.playbackMode = playbackMode;
			return res;
		}

		public void InitVertexLayoutsPerCharacter(TMP_TextInfo textInfo, PlaybackMode playbackMode, ref VertexGroupLayout[] vertexGroupLayouts)
		{
			int numCharacters = GetCharacterCount(textInfo);
			List<VertexGroupLayout> groups = new List<VertexGroupLayout>(numCharacters);

			for (int i = 0; i < numCharacters; i++)
			{
				ref TMP_CharacterInfo characterInfo = ref textInfo.characterInfo[i];
				if (!OwnsMainQuad(ref characterInfo)) { continue; }

				groups.Add(CreateGroupLayout(groups.Count, characterInfo.vertexIndex, characterInfo.vertexIndex + 3, playbackMode));
			}

			vertexGroupLayouts = groups.ToArray();
		}

		// A word is a run of glyphs between whitespace characters. Whitespace only runs own no vertices and make no group
		public void InitVertexLayoutsPerWord(TMP_TextInfo textInfo, PlaybackMode playbackMode, ref VertexGroupLayout[] vertexGroupLayouts)
		{
			int numCharacters = GetCharacterCount(textInfo);
			List<VertexGroupLayout> groups = new List<VertexGroupLayout>();

			int wordStart = -1;
			int wordEnd = -1;
			for (int i = 0; i <= numCharacters; i++)
			{
				bool separator = i == numCharacters || char.IsWhiteSpace(textInfo.characterInfo[i].character);
				if (separator)
				{
					if (wordStart >= 0)
					{
						groups.Add(CreateGroupLayout(groups.Count, wordStart, wordEnd, playbackMode));
					}
					wordStart = -1;
					continue;
				}

				ref TMP_CharacterInfo characterInfo = ref textInfo.characterInfo[i];
				if (!OwnsMainQuad(ref characterInfo)) { continue; }

				if (wordStart < 0) { wordStart = characterInfo.vertexIndex; }
				wordEnd = characterInfo.vertexIndex + 3;
			}

			vertexGroupLayouts = groups.ToArray();
		}

		// A parsed group spans a range of the clean string (custom tags removed, TMP tags kept); characterInfo.index is that same string's index
		public EffectFlag InitVertexLayoutsPerTags(TMP_TextInfo textInfo, List<TextParsedGroup> parsedGroups, PlaybackMode playbackMode, ref VertexGroupLayout[] vertexGroupLayouts)
		{
			EffectFlag res = EffectFlag.None;

			int numCharacters = GetCharacterCount(textInfo);
			List<VertexGroupLayout> groups = new List<VertexGroupLayout>(parsedGroups.Count);

			for (int i = 0; i < parsedGroups.Count; i++)
			{
				TextParsedGroup parsedGroup = parsedGroups[i];
				int rangeStart = parsedGroup.initIndex;
				int rangeEnd = parsedGroup.initIndex + parsedGroup.length;

				int groupStart = -1;
				int groupEnd = -1;
				for (int c = 0; c < numCharacters; c++)
				{
					ref TMP_CharacterInfo characterInfo = ref textInfo.characterInfo[c];
					if (characterInfo.index < rangeStart || characterInfo.index >= rangeEnd) { continue; }
					if (!OwnsMainQuad(ref characterInfo)) { continue; }

					if (groupStart < 0) { groupStart = characterInfo.vertexIndex; }
					groupEnd = characterInfo.vertexIndex + 3;
				}

				if (groupStart < 0) { continue; }

				VertexGroupLayout groupLayout = CreateGroupLayout(groups.Count, groupStart, groupEnd, playbackMode);
				groupLayout.enabledEffectsMask = parsedGroup.effectMask;

				//Effect States
				groupLayout.initialRotationStateValues = parsedGroup.initialRotationStateValues;
				groupLayout.scaleStateValues	= parsedGroup.scaleStateValues;
				groupLayout.alphaStateValues	= parsedGroup.alphaStateValues;
				groupLayout.offsetStateValues	= parsedGroup.offsetStateValues;
				groupLayout.pendulumStateValues = parsedGroup.pendulumStateValues;
				groupLayout.shakeStateValues	= parsedGroup.shakeStateValues;
				groupLayout.rainbowStateValues	= parsedGroup.rainbowStateValues;
				groupLayout.swingStateValues	= parsedGroup.swingStateValues;
				groupLayout.rotationStateValues = parsedGroup.rotationStateValues;
				groupLayout.dangleStateValues	= parsedGroup.dangleStateValues;
				groupLayout.slideStateValues	= parsedGroup.slideStateValues;
				groupLayout.verticalMovementStateValues	= parsedGroup.verticalMovementValues;
				//

				groups.Add(groupLayout);

				res = res | parsedGroup.effectMask;
			}

			vertexGroupLayouts = groups.ToArray();

			return res;
		}

		public bool NeedsToRebuildVertexGroups()
		{
			bool res = false;



			return res;
		}

		public void WriteBackTo(TMP_Text text, bool includeUVs = false)
		{
			TMP_TextInfo info = text.textInfo;

			if (info != null && text.mesh != null)
			{
				CopyOutputTo(info);

				TMP_VertexDataUpdateFlags flags = TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32;
				if (includeUVs)
				{
					flags |= TMP_VertexDataUpdateFlags.Uv0 | TMP_VertexDataUpdateFlags.Uv2;
				}
				text.UpdateVertexData(flags);
			}
		}

		// With no stage the output is the effects' result as is
		public void ApplyLayoutStage(ITextLayoutStage layoutStage)
		{
			if (layoutStage != null)
			{
				layoutStage.ApplyLayout(this);
			}
			else
			{
				CopyCurrentToOutput();
			}
		}

		public void CopyCurrentToOutput()
		{
			for (int m = 0; m < subMeshes.Length; m++)
			{
				Vector3[] current = subMeshes[m].currentVerts;
				Vector3[] output = subMeshes[m].outputVerts;
				Color32[] currentColors = subMeshes[m].currentColors;
				Color32[] outputColors = subMeshes[m].outputColors;
				if (current == null || output == null || current.Length != output.Length) { continue; }
				if (currentColors == null || outputColors == null || currentColors.Length != outputColors.Length) { continue; }

				System.Array.Copy(current, output, current.Length);
				System.Array.Copy(currentColors, outputColors, currentColors.Length);
			}
		}

		// Copies rather than aliases: TMP writes every regeneration into its own arrays, and those must never be the working buffer
		public void CopyOutputTo(TMP_TextInfo info)
		{
			if (info == null || info.meshInfo == null) { return; }

			int count = subMeshes.Length;
			for (int m = 0; m < count && m < info.meshInfo.Length; m++)
			{
				Vector3[] verts = subMeshes[m].outputVerts;
				Color32[] colors = subMeshes[m].outputColors;
				Vector3[] targetVerts = info.meshInfo[m].vertices;
				Color32[] targetColors = info.meshInfo[m].colors32;

				if (verts == null || targetVerts == null || verts.Length != targetVerts.Length) { continue; }
				if (colors == null || targetColors == null || colors.Length != targetColors.Length) { continue; }

				System.Array.Copy(verts, targetVerts, verts.Length);
				System.Array.Copy(colors, targetColors, colors.Length);
			}
		}

		public void Reset()
		{
			int count = subMeshes.Length;

			for (int subMesheshIndex = 0; subMesheshIndex < subMeshes.Length; subMesheshIndex++)
			{
				ref SubMeshBuffers submeshBuffer = ref subMeshes[subMesheshIndex];
				for(int vertIndex = 0; vertIndex < submeshBuffer.srcVerts.Length; vertIndex++)
				{
					submeshBuffer.currentVerts[vertIndex] = submeshBuffer.srcVerts[vertIndex];
					submeshBuffer.currentColors[vertIndex] = submeshBuffer.srcColors[vertIndex];
					submeshBuffer.outputVerts[vertIndex] = submeshBuffer.srcVerts[vertIndex];
					submeshBuffer.outputColors[vertIndex] = submeshBuffer.srcColors[vertIndex];
				}
			}
		}

		// Groups index main material glyphs, which all live in submesh 0; the other submeshes hold fallback font and sprite glyphs at unrelated indices
		private bool GroupFitsMainSubMesh(ref VertexGroupLayout groupLayout)
		{
			return subMeshes.Length > 0 && subMeshes[0].currentVerts != null &&
				groupLayout.startVertexIndex >= 0 && groupLayout.endVertexIndex < subMeshes[0].currentVerts.Length;
		}

		public void SetSubmeshToSrcValues(ref VertexGroupLayout[] vertexGroupLayouts)
		{
			for (int vertexGroupIdx = 0; vertexGroupIdx < vertexGroupLayouts.Length; vertexGroupIdx++)
			{
				ref VertexGroupLayout groupLayout = ref vertexGroupLayouts[vertexGroupIdx];
				if (!GroupFitsMainSubMesh(ref groupLayout)) { continue; }

				ref MeshCache.SubMeshBuffers subMesh = ref subMeshes[0];
				for (int vertIdx = groupLayout.startVertexIndex; vertIdx <= groupLayout.endVertexIndex; vertIdx++)
				{
					subMesh.currentVerts[vertIdx] = subMesh.srcVerts[vertIdx];
					subMesh.currentColors[vertIdx] = subMesh.srcColors[vertIdx];
				}
			}
		}

		public void ApplyStates(MeshEffectGroupState meshEffectGroupState, ref VertexGroupLayout groupLayout, ref SubMeshBuffers subMesh)
		{
			for (int stateIdx = 0; stateIdx < meshEffectGroupState.states.Count; stateIdx++)
			{
				EffectFlag effectMask = meshEffectGroupState.states[stateIdx].effectMask;
				bool stateEnabldInThisGroup = (effectMask & groupLayout.enabledEffectsMask) == effectMask;

				Vector3 center = (subMesh.currentVerts[groupLayout.startVertexIndex] + subMesh.currentVerts[groupLayout.endVertexIndex - 1]) * 0.5f;
				Vector3 centerTop = (subMesh.currentVerts[groupLayout.startVertexIndex + 1] + subMesh.currentVerts[groupLayout.endVertexIndex - 1]) * 0.5f;

				groupLayout.center = center;
				groupLayout.centerTop = centerTop;
				groupLayout.SetRight(subMesh.currentVerts[groupLayout.startVertexIndex], subMesh.currentVerts[groupLayout.startVertexIndex + 3]);

				for (int vertIdx = groupLayout.startVertexIndex; vertIdx <= groupLayout.endVertexIndex; vertIdx++)
				{
					Vector3 vertex = subMesh.currentVerts[vertIdx];
					Color32 color = subMesh.currentColors[vertIdx];

					if (stateEnabldInThisGroup)
					{
						vertex = meshEffectGroupState.states[stateIdx].TransformVertex(ref groupLayout, vertex);
						color = meshEffectGroupState.states[stateIdx].TransformColor(ref groupLayout, color);
					}

					subMesh.currentColors[vertIdx] = color;
					subMesh.currentVerts[vertIdx] = vertex;
				}
			}
		}

		public void Apply(MeshEffectGroupState effectGroupState, ref VertexGroupLayout[] vertexGroupLayouts)
		{
			for (int vertexGroupIdx = 0; vertexGroupIdx < vertexGroupLayouts.Length; vertexGroupIdx++)
			{
				ref VertexGroupLayout groupLayout = ref vertexGroupLayouts[vertexGroupIdx];
				if (!GroupFitsMainSubMesh(ref groupLayout)) { continue; }

				ApplyStates(effectGroupState, ref groupLayout, ref subMeshes[0]);
			}
		}

		public void Apply(MeshEffectGroupState revealOrHideState, MeshEffectGroupState loopedState)
		{
			SetSubmeshToSrcValues(ref revealOrHideState.vertexGroupLayouts);
			SetSubmeshToSrcValues(ref loopedState.vertexGroupLayouts);

			Apply(revealOrHideState, ref revealOrHideState.vertexGroupLayouts);
			Apply(loopedState, ref loopedState.vertexGroupLayouts);
		}

		/*
		private void CheckSize(int submeshesTargetSize, int verticesTargetSize)
		{
			if(subMeshes == null || subMeshes.Length != submeshesTargetSize)
			{
				subMeshes = new SubMeshBuffers[submeshesTargetSize];
			}

			for(int submeshIdx = 0; submeshIdx < subMeshes.Length; submeshIdx++)
			{
				SubMeshBuffers submeshBuffer = subMeshes[submeshIdx];

				if(submeshBuffer.)

				subMeshes[submeshIdx] = submeshBuffer;
			}
		}
		*/

#if UNITY_EDITOR
		public ref VertexGroupLayout[] GetVertexGroupLayoutByPreviewMode(PreviewMode previewMode)
		{
			ref VertexGroupLayout[] res = ref vertexGroupLayoutsReveal;

			switch (previewMode)
			{
				case PreviewMode.PREVIEW_HIDE:
					res = ref vertexGroupLayoutsHide;
					break;
				case PreviewMode.PREVIEW_LOOPED:
					res = ref vertexGroupLayoutsLooped;
					break;
				case PreviewMode.PREVIEW_REVEAL:
					res = ref vertexGroupLayoutsReveal;
					break;
			}

			return ref res;
		}

		public void ApplyPreview(MeshEffectGroupState effectGroupState, PreviewMode previewMode)
		{
			ref VertexGroupLayout[] vertexGroupLayout = ref GetVertexGroupLayoutByPreviewMode(previewMode);

			SetSubmeshToSrcValues(ref vertexGroupLayout);
			Apply(effectGroupState, ref vertexGroupLayout);
		}
#endif
	}
}
