using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// Extrude's draw order: every back copy (TextBackQuads) before every face, each set sorted along the extrusion by the layout so animated letters never swap places
	public class TextQuadDrawOrder
	{
		private const string KW_EXTRUDE_ON = "_EXTRUDE_ON";
		private static readonly int PROP_ID_EXTRUDE_ANGLE = Shader.PropertyToID("_ExtrudeAngle");
		private const float DIRECTION_EPSILON = 1e-4f;

		private sealed class QuadKeyComparer : IComparer<int>
		{
			public float[] keys = new float[0];

			public int Compare(int a, int b)
			{
				int res = keys[a].CompareTo(keys[b]);
				if (res == 0)
				{
					res = a.CompareTo(b);
				}

				return res;
			}
		}

		private readonly QuadKeyComparer comparer = new QuadKeyComparer();
		private int[] appliedOrder = new int[0];
		private int[] candidateOrder = new int[0];
		private int[] sortedGlyphs = new int[0];
		private int[] triangles = new int[0];

		private bool isSorted;
		private float sortedAngle;
		private Mesh sortedMesh;
		// TMP replaces this array whenever it reallocates the mesh, which also puts the mesh back in layout order
		private int[] sortedTMPTriangles;

		public void Refresh(TMP_Text text, Material material, MeshCache meshCache, bool positionsChanged)
		{
			if (text == null) { return; }

			TMP_TextInfo textInfo = text.textInfo;
			if (textInfo == null || textInfo.meshInfo == null || textInfo.meshInfo.Length == 0) { return; }

			Mesh mesh = text.mesh;
			if (Refresh(textInfo, mesh, textInfo.meshInfo[0].triangles, material, meshCache, positionsChanged))
			{
				// Between reallocations TMP re-uploads only vertex data, so a canvas text needs its mesh pushed again
				text.UpdateGeometry(mesh, 0);
			}
		}

		// The layout and TMP's triangles come from the text that laid the quads out, which for a text layer is its main text. True when the mesh's triangles changed and it needs uploading again
		public bool Refresh(TMP_TextInfo textInfo, Mesh mesh, int[] tmpTriangles, Material material, MeshCache meshCache, bool positionsChanged)
		{
			if (textInfo == null || mesh == null || tmpTriangles == null) { return false; }
			if (meshCache == null || meshCache.subMeshes == null || meshCache.subMeshes.Length == 0) { return false; }

			bool extrude = material != null && material.IsKeywordEnabled(KW_EXTRUDE_ON);
			if (!extrude)
			{
				bool restored = isSorted && Restore(mesh, tmpTriangles);
				return restored;
			}

			float angle = material.GetFloat(PROP_ID_EXTRUDE_ANGLE);
			bool meshReset = !isSorted || sortedMesh != mesh || sortedTMPTriangles != tmpTriangles;
			if (!positionsChanged && !meshReset && angle == sortedAngle) { return false; }

			int quadCapacity = tmpTriangles.Length / 6;
			if (mesh.vertexCount < quadCapacity * 4) { return false; }

			// TMP's layout rather than the output, which mesh effects, the spline and the warp move every frame
			Vector3[] vertices = meshCache.subMeshes[0].srcVerts;
			if (vertices == null) { return false; }
			int glyphQuads = Mathf.Min(meshCache.decorationFirstVertex / 4, quadCapacity, vertices.Length / 4);
			int backFirstQuad = meshCache.backFirstVertex / 4;
			int backQuadCount = Mathf.Max(0, Mathf.Min(meshCache.backVertexCount / 4, quadCapacity - backFirstQuad));

			BuildOrder(textInfo, vertices, glyphQuads, backFirstQuad, backQuadCount, quadCapacity, angle);

			if (!meshReset && HasSameOrder(quadCapacity))
			{
				sortedAngle = angle;
				return false;
			}

			int[] swap = appliedOrder;
			appliedOrder = candidateOrder;
			candidateOrder = swap;

			Upload(mesh, quadCapacity);

			isSorted = true;
			sortedAngle = angle;
			sortedMesh = mesh;
			sortedTMPTriangles = tmpTriangles;

			return true;
		}

		// The mesh's triangles were put back in TMP's order by someone else
		public void Invalidate()
		{
			isSorted = false;
			sortedMesh = null;
			sortedTMPTriangles = null;
			appliedOrder = new int[0];
		}

		// Back copies first, then the faces, each ascending along the direction; decoration slices and spare capacity keep TMP's order after them
		private void BuildOrder(TMP_TextInfo textInfo, Vector3[] vertices, int glyphQuads, int backFirstQuad, int backQuadCount, int quadCapacity, float angle)
		{
			float radians = angle * Mathf.Deg2Rad;
			Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
			// cos(270) comes out near 1e-8, not 0, which would let the letters' x break ties a straight down extrusion should leave alone
			if (Mathf.Abs(direction.x) < DIRECTION_EPSILON) { direction.x = 0f; }
			if (Mathf.Abs(direction.y) < DIRECTION_EPSILON) { direction.y = 0f; }

			if (comparer.keys.Length < glyphQuads)
			{
				comparer.keys = new float[glyphQuads];
			}

			if (sortedGlyphs.Length < glyphQuads)
			{
				sortedGlyphs = new int[glyphQuads];
			}

			if (candidateOrder.Length != quadCapacity)
			{
				candidateOrder = new int[quadCapacity];
			}

			for (int quad = 0; quad < glyphQuads; quad++)
			{
				Vector3 center = (vertices[quad * 4] + vertices[quad * 4 + 2]) * 0.5f;
				comparer.keys[quad] = center.x * direction.x + center.y * direction.y;
				sortedGlyphs[quad] = quad;
			}

			// A letter stands on its baseline rather than its ink center, so a line's letters tie across it and keep TMP's order instead of sorting by glyph height
			int numCharacters = textInfo.characterInfo != null ? Mathf.Min(textInfo.characterCount, textInfo.characterInfo.Length) : 0;
			for (int c = 0; c < numCharacters; c++)
			{
				ref TMP_CharacterInfo characterInfo = ref textInfo.characterInfo[c];
				if (!characterInfo.isVisible || characterInfo.materialReferenceIndex != 0) { continue; }

				int quad = characterInfo.vertexIndex / 4;
				if (quad >= glyphQuads) { continue; }

				float centerX = (vertices[quad * 4].x + vertices[quad * 4 + 2].x) * 0.5f;
				comparer.keys[quad] = centerX * direction.x + characterInfo.baseLine * direction.y;
			}

			System.Array.Sort(sortedGlyphs, 0, glyphQuads, comparer);

			int count = 0;
			for (int i = 0; i < glyphQuads; i++)
			{
				if (sortedGlyphs[i] < backQuadCount)
				{
					candidateOrder[count++] = backFirstQuad + sortedGlyphs[i];
				}
			}

			// The slices' back copies, in TMP's order like the slices themselves
			for (int quad = glyphQuads; quad < backQuadCount; quad++)
			{
				candidateOrder[count++] = backFirstQuad + quad;
			}

			for (int i = 0; i < glyphQuads; i++)
			{
				candidateOrder[count++] = sortedGlyphs[i];
			}

			for (int quad = glyphQuads; quad < quadCapacity; quad++)
			{
				if (quad >= backFirstQuad && quad < backFirstQuad + backQuadCount) { continue; }

				candidateOrder[count++] = quad;
			}
		}

		private bool HasSameOrder(int quadCapacity)
		{
			bool res = appliedOrder.Length == quadCapacity;

			for (int i = 0; res && i < quadCapacity; i++)
			{
				res = appliedOrder[i] == candidateOrder[i];
			}

			return res;
		}

		private void Upload(Mesh mesh, int quadCapacity)
		{
			if (triangles.Length != quadCapacity * 6)
			{
				triangles = new int[quadCapacity * 6];
			}

			for (int i = 0; i < quadCapacity; i++)
			{
				int firstVertex = appliedOrder[i] * 4;
				int firstIndex = i * 6;

				triangles[firstIndex + 0] = firstVertex + 0;
				triangles[firstIndex + 1] = firstVertex + 1;
				triangles[firstIndex + 2] = firstVertex + 2;
				triangles[firstIndex + 3] = firstVertex + 2;
				triangles[firstIndex + 4] = firstVertex + 3;
				triangles[firstIndex + 5] = firstVertex + 0;
			}

			mesh.SetTriangles(triangles, 0, false);
		}

		private bool Restore(Mesh mesh, int[] tmpTriangles)
		{
			bool res = mesh.vertexCount >= tmpTriangles.Length / 6 * 4;
			if (res)
			{
				mesh.SetTriangles(tmpTriangles, 0, false);
			}

			Invalidate();

			return res;
		}
	}
}
