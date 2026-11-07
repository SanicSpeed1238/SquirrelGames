using TMPro;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// With Extrude each glyph and decoration slice quad gets a copy, right after the slices, that draws what sits behind it; all copies draw before all faces
	public class TextBackQuads
	{
		private const string KW_EXTRUDE_ON = "_EXTRUDE_ON";
		private const int MAX_QUADS = 16383;

		// Copies are contiguous and in source order: back vertex = FirstVertex + source vertex, for source vertices below VertexCount
		public int FirstVertex { get; private set; }
		public int VertexCount { get; private set; }

		// Asked for on the last build, even when the copies did not fit in TMP's arrays, so a text that is too long is not rebuilt every frame
		public bool Requested { get; private set; }

		public static bool IsNeeded(Material material)
		{
			bool res = material != null && material.IsKeywordEnabled(KW_EXTRUDE_ON);
			return res;
		}

		public void Clear()
		{
			FirstVertex = 0;
			VertexCount = 0;
			Requested = false;
		}

		// sourceVertexEnd: the end of the decoration slices. The copies of [0, sourceVertexEnd) start right there
		public void Build(TMP_TextInfo info, int sourceVertexEnd)
		{
			Clear();
			Requested = true;

			if (info == null || info.meshInfo == null || info.meshInfo.Length == 0 || info.meshInfo[0].vertices == null) { return; }
			if (sourceVertexEnd <= 0) { return; }

			int end = sourceVertexEnd * 2;
			if ((end + 3) / 4 > MAX_QUADS) { return; }

			EnsureCapacity(info, end);

			ref TMP_MeshInfo meshInfo = ref info.meshInfo[0];
			System.Array.Copy(meshInfo.vertices, 0, meshInfo.vertices, sourceVertexEnd, sourceVertexEnd);
			System.Array.Copy(meshInfo.uvs0, 0, meshInfo.uvs0, sourceVertexEnd, sourceVertexEnd);
			System.Array.Copy(meshInfo.uvs2, 0, meshInfo.uvs2, sourceVertexEnd, sourceVertexEnd);
			System.Array.Copy(meshInfo.colors32, 0, meshInfo.colors32, sourceVertexEnd, sourceVertexEnd);

			FirstVertex = sourceVertexEnd;
			VertexCount = sourceVertexEnd;
		}

		public void Mirror(Vector3[] vertices, Color32[] colors)
		{
			if (VertexCount == 0) { return; }

			if (vertices != null && FirstVertex + VertexCount <= vertices.Length)
			{
				System.Array.Copy(vertices, 0, vertices, FirstVertex, VertexCount);
			}

			if (colors != null && FirstVertex + VertexCount <= colors.Length)
			{
				System.Array.Copy(colors, 0, colors, FirstVertex, VertexCount);
			}
		}

		// Grows TMP's own arrays in place by powers of two, as TextDecorationBuilder does
		private static void EnsureCapacity(TMP_TextInfo info, int numVertices)
		{
			if (numVertices <= info.meshInfo[0].vertices.Length) { return; }

			int numQuads = Mathf.Min(Mathf.NextPowerOfTwo((numVertices + 3) / 4), MAX_QUADS);
			info.meshInfo[0].ResizeMeshInfo(numQuads);
		}
	}
}
