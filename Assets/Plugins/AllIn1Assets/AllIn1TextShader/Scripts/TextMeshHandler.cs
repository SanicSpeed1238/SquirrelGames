using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class TextMeshHandler
	{
		private const string TAG = "TextMeshHandler";

		private MeshCache meshCache;

		public TMP_Text tmpText;

		// The text layers, which draw this text's finished mesh with their own materials
		private readonly List<TextLayerGraphic> layers = new List<TextLayerGraphic>();

		public TextMeshHandler parentMeshHandler;

		// Set by the typewriter so a fresh snapshot can be re-dressed with the live effect state
		public System.Action reapplyEffects;

		// Optional final placement (the spline); runs after the effects on every write-back
		public ITextLayoutStage layoutStage;

		// Optional deformation of the finished layout (the warp); runs after the layout stage
		public ITextOutputModifier outputModifier;

		private bool snapshotTakenByTMP;

		// Underline, strikethrough and mark slices of the main text, rebuilt on every TMP regeneration
		public readonly TextDecorationBuilder decorations = new TextDecorationBuilder();
		// Extrude's back copies of the glyph and slice quads, rebuilt with the slices
		public readonly TextBackQuads backQuads = new TextBackQuads();
		// The slices' UVs reach the mesh with TMP's own upload after the hook; a snapshot taken without the hook has to send them itself
		private bool uvUploadPending;

		// Set by the material configurator: only the text shader reads the stamped uv2.x, while TMP's own shaders map face textures with it
		public bool stampVertexIndices;

		// Bumped every time the output buffers change, so consumers can skip work while the mesh is still
		public int OutputVersion { get; private set; }

		// Bumped on every relayout snapshot, when the vertex count, the UVs and the triangles may have changed too
		public int LayoutVersion { get; private set; }

		public TextMeshHandler(TMP_Text tmpText)
		{
			this.tmpText = tmpText;

			Init();
		}

		public void SetLayer(int index, TextLayerGraphic layer)
		{
			while (layers.Count <= index)
			{
				layers.Add(null);
			}

			layers[index] = layer;
			layer.SetSource(this);
		}

		public void ClearLayers()
		{
			layers.Clear();
		}

		public void Init()
		{
			meshCache = new MeshCache();
			RebuildCache();

			WriteChanges();
		}

		public void UpdateTMPMesh()
		{
			tmpText.ForceMeshUpdate(true, true);
		}

		// Finishes the frame's mesh and uploads it. The other way a mesh reaches TMP is the relayout callback below, which finishes it the same way and lets TMP upload
		public void WriteChanges()
		{
			// TMP clears the mesh of an empty text without the relayout callback, so the cache still holds the previous text and must not bring it back
			if (tmpText == null || tmpText.textInfo == null || tmpText.textInfo.characterCount == 0) { return; }

			FinishOutput();

			meshCache.WriteBackTo(tmpText, uvUploadPending);
			uvUploadPending = false;

			MirrorToLayers();
		}

		private void MirrorToLayers()
		{
			for (int i = 0; i < layers.Count; i++)
			{
				if (layers[i] != null)
				{
					layers[i].Mirror(false);
				}
			}
		}

		// The decoration slices go into TMP's arrays first, so the rest pose the cache takes already holds them
		private void SnapshotLayout(TMP_TextInfo textInfo)
		{
			decorations.Build(tmpText, textInfo);
			meshCache.decorationFirstVertex = decorations.FirstVertex;
			meshCache.decorationVertexCount = decorations.VertexCount;

			if (NeedsBackQuads())
			{
				backQuads.Build(textInfo, decorations.FirstVertex + decorations.VertexCount);
			}
			else
			{
				backQuads.Clear();
			}
			meshCache.backFirstVertex = backQuads.FirstVertex;
			meshCache.backVertexCount = backQuads.VertexCount;

			// After the slices and back copies are written, so TMP's upload and every copy of the arrays carry it
			if (stampVertexIndices)
			{
				StampVertexIndices(textInfo);
			}

			meshCache.Rebuild(tmpText);

			LayoutVersion++;
		}

		// SV_VertexID includes the draw's base vertex on Vulkan, OpenGL and WebGPU, so each vertex carries its own index in TMP's uv2.x, which the text shader reads instead
		private static void StampVertexIndices(TMP_TextInfo textInfo)
		{
			if (textInfo == null || textInfo.meshInfo == null || textInfo.meshInfo.Length == 0) { return; }

			Vector2[] uvs2 = textInfo.meshInfo[0].uvs2;
			if (uvs2 == null) { return; }

			for (int i = 0; i < uvs2.Length; i++)
			{
				uvs2[i].x = -(i + 1);
			}
		}

		// Any layer drawing Extrude needs the copies, and every layer draws the main text's mesh
		public bool NeedsBackQuads()
		{
			bool res = TextBackQuads.IsNeeded(tmpText.fontSharedMaterial);

			for (int i = 0; !res && i < layers.Count; i++)
			{
				res = layers[i] != null && TextBackQuads.IsNeeded(layers[i].LayerMaterial);
			}

			return res;
		}

		// Extrude was turned on or off since the last layout, so the copies have to be added or dropped
		public bool BackQuadsOutOfDate()
		{
			return NeedsBackQuads() != backQuads.Requested;
		}

		// From the bottom and left edges, so rotation and shear keep it and a letter squashed flat on either axis has none
		private static float GetQuadArea(Vector3[] verts, int bottomLeft)
		{
			Vector3 right = verts[bottomLeft + 3] - verts[bottomLeft];
			Vector3 up = verts[bottomLeft + 1] - verts[bottomLeft];
			return Mathf.Abs(right.x * up.y - right.y * up.x);
		}

		// Slices belong to no effect group, so each one takes its owner letter's visibility (alpha ratio times size ratio against the rest pose,
		// size as the square root of the area ratio): the line appears with its letters on reveal and leaves with them on hide, without moving
		private void ApplyDecorationVisibility()
		{
			if (decorations.SliceCount == 0 || meshCache.subMeshes.Length == 0) { return; }

			ref MeshCache.SubMeshBuffers subMesh = ref meshCache.subMeshes[0];
			if (subMesh.currentVerts == null) { return; }
			int numVertices = subMesh.currentVerts.Length;

			for (int s = 0; s < decorations.SliceCount; s++)
			{
				TextDecorationSlice slice = decorations.slices[s];
				if (slice.firstVertex + 3 >= numVertices) { continue; }

				float visibility = 1f;
				int owner = slice.ownerVertex;
				if (owner >= 0 && owner + 3 < numVertices)
				{
					byte srcAlpha = subMesh.srcColors[owner].a;
					float alphaRatio = srcAlpha > 0 ? Mathf.Clamp01((float)subMesh.currentColors[owner].a / srcAlpha) : 1f;

					float srcArea = GetQuadArea(subMesh.srcVerts, owner);
					float sizeRatio = srcArea > 0f ? Mathf.Clamp01(Mathf.Sqrt(GetQuadArea(subMesh.currentVerts, owner) / srcArea)) : 1f;

					visibility = alphaRatio * sizeRatio;
				}

				// A mark's alpha is stored as its fourth root (see TextDecorationBuilder), so its linear opacity scales by the fourth root too
				if (slice.kind == TextDecorationKind.HIGHLIGHT)
				{
					visibility = Mathf.Sqrt(Mathf.Sqrt(visibility));
				}

				for (int k = 0; k < 4; k++)
				{
					Color32 color = subMesh.srcColors[slice.firstVertex + k];
					color.a = (byte)Mathf.RoundToInt(color.a * visibility);
					subMesh.currentColors[slice.firstVertex + k] = color;
				}
			}
		}

		// The layout stage and the output modifier on top of the effects. Every mesh handed to TMP passes through here, so the version counts every change
		private void FinishOutput()
		{
			// A component destroyed while inactive never detached itself
			if (layoutStage is Object destroyedStage && destroyedStage == null) { layoutStage = null; }
			if (outputModifier is Object destroyedModifier && destroyedModifier == null) { outputModifier = null; }

			ApplyDecorationVisibility();
			meshCache.ApplyLayoutStage(layoutStage);
			if (outputModifier != null)
			{
				outputModifier.ModifyOutput(meshCache);
			}

			// Last, so the copies take the letters exactly as every stage above left them
			if (meshCache.subMeshes.Length > 0)
			{
				backQuads.Mirror(meshCache.subMeshes[0].outputVerts, meshCache.subMeshes[0].outputColors);
			}

			OutputVersion++;
		}

		public void RebuildCache()
		{
			snapshotTakenByTMP = false;
			UpdateTMPMesh();

			// When the TMP callback ran during the forced update it already snapshotted and finished the mesh, and TMP's arrays hold it
			if (!snapshotTakenByTMP)
			{
				SnapshotLayout(tmpText.textInfo);
				FinishOutput();
				uvUploadPending = true;
				MirrorToLayers();
			}
		}

		// Runs inside TMP's generation (OnPreRenderText): layout is final, mesh not uploaded yet
		public void OnTMPLayoutRegenerated(TMP_TextInfo textInfo)
		{
			if (tmpText == null || textInfo != tmpText.textInfo) { return; }

			SnapshotLayout(textInfo);
			snapshotTakenByTMP = true;

			if (reapplyEffects != null)
			{
				reapplyEffects();
			}

			FinishOutput();
			meshCache.CopyOutputTo(textInfo);
			MirrorToLayers();
		}

		// The groups are read off TMP's character list, so a text assigned this frame is laid out first
		private TMP_TextInfo GetCurrentTextInfo()
		{
			if (tmpText.havePropertiesChanged)
			{
				tmpText.ForceMeshUpdate(true, true);
			}
			return tmpText.textInfo;
		}

		public EffectFlag BuildVertexGroupsReveal(GroupType groupType, List<TextParsedGroup> parsedGroups)
		{
			EffectFlag effectsEnabled = meshCache.BuildVertexReveal(GetCurrentTextInfo(), groupType, parsedGroups);
			return effectsEnabled;
		}

		public EffectFlag BuildVertexGroupsLooped(GroupType groupType, List<TextParsedGroup> parsedGroups)
		{
			EffectFlag effectsEnabled = meshCache.BuildVertexGroupsLooped(GetCurrentTextInfo(), groupType, parsedGroups);
			return effectsEnabled;
		}

		public EffectFlag BuildVertexGroupsHide(GroupType groupType, List<TextParsedGroup> parsedGroups)
		{
			EffectFlag effectsEnabled = meshCache.BuildVertexHide(GetCurrentTextInfo(), groupType, parsedGroups);
			return effectsEnabled;
		}

		public MeshCache GetMeshCache()
		{
			return meshCache;
		}

		public void SetText(string text)
		{
			this.tmpText.text = text;
		}

		public string GetText()
		{
			return tmpText.text;
		}

		public TMP_FontAsset GetFontAsset()
		{
			return tmpText.font;
		}
		 
		public float GetFontSize()
		{
			return tmpText.fontSize;
		}

		public bool HasExtraPadding()
		{
			return tmpText.extraPadding;
		}

		public int GetNumCharacters()
		{
			return tmpText.text.Length;
		}

		public Bounds GetTextBounds()
		{
			return tmpText.textBounds;
		}
	}
}