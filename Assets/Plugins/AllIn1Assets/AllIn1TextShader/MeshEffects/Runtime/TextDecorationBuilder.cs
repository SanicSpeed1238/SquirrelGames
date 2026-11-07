using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

namespace AllIn1Assets.AllIn1TextShader
{
	public enum TextDecorationKind
	{
		NONE = 0,
		UNDERLINE = 1,
		STRIKETHROUGH = 2,
		HIGHLIGHT = 3
	}

	public struct TextDecorationSlice
	{
		public int firstVertex;
		public TextDecorationKind kind;
		public int runIndex;
		// The glyph whose cell the slice covers, -1 when the run has no visible glyph
		public int ownerCharacter;
		// First vertex of the owner's quad in submesh 0, -1 when the owner is not a main material glyph (fallback font, sprite)
		public int ownerVertex;
	}

	public struct TextDecorationRun
	{
		public TextDecorationKind kind;
		public int firstSlice;
		public int sliceCount;
	}

	// Replaces TMP's underline, strikethrough and mark geometry with one quad per decorated character, written after the glyph quads of submesh 0.
	// Runs inside TMP's OnPreRenderText, before the mesh cache snapshots the mesh, so what it writes is what TMP uploads
	public class TextDecorationBuilder
	{
		private const uint UNDERSCORE = 0x5F;
		private const int MAX_QUADS = 16383;
		private const float LARGE_POSITIVE = 32767f;

		public TextDecorationSlice[] slices = new TextDecorationSlice[0];
		public TextDecorationRun[] runs = new TextDecorationRun[0];
		public int SliceCount { get; private set; }
		public int RunCount { get; private set; }

		// Slices are contiguous: [FirstVertex, FirstVertex + VertexCount)
		public int FirstVertex { get; private set; }
		public int VertexCount { get { return SliceCount * 4; } }

		public bool HasUnderscore { get; private set; }
		public GlyphRect UnderscoreRect { get; private set; }

		private TextDecorationKind[] pendingKinds = new TextDecorationKind[0];
		private int[] pendingFirstCharacters = new int[0];
		private int[] pendingLastCharacters = new int[0];
		private int pendingCount;

		private int[] cellOwners = new int[0];
		private float[] cellEdges = new float[0];

		private float atlasWidth;
		private float atlasHeight;
		// Texels of SDF around the '_' ink the line quad samples, the padding TMP gives this material's quads
		private float atlasSamplePadding;

		public void Build(TMP_Text text, TMP_TextInfo info)
		{
			SliceCount = 0;
			RunCount = 0;
			pendingCount = 0;
			FirstVertex = 0;
			HasUnderscore = false;

			if (text == null || info == null || info.meshInfo == null || info.meshInfo.Length == 0 || info.meshInfo[0].vertices == null) { return; }

			FirstVertex = GetGlyphVertexEnd(info);

			// TMP's own decoration quads, the gap left by truncated characters and spare capacity
			Vector3[] vertices = info.meshInfo[0].vertices;
			for (int i = FirstVertex; i < vertices.Length; i++)
			{
				vertices[i] = Vector3.zero;
			}

			TMP_FontAsset font = text.font;
			if (font == null || !font.characterLookupTable.TryGetValue(UNDERSCORE, out TMP_Character underscore) || underscore.glyph == null) { return; }
			// The shader samples the first atlas page only
			if (underscore.glyph.atlasIndex != 0) { return; }

			HasUnderscore = true;
			UnderscoreRect = underscore.glyph.glyphRect;
			atlasWidth = font.atlasWidth;
			atlasHeight = font.atlasHeight;
			atlasSamplePadding = ShaderUtilities.GetPadding(text.fontSharedMaterial, text.extraPadding, false);

			// Marks first so the lines draw over them
			CollectRuns(text, info, TextDecorationKind.HIGHLIGHT, FontStyles.Highlight);
			CollectRuns(text, info, TextDecorationKind.UNDERLINE, FontStyles.Underline);
			CollectRuns(text, info, TextDecorationKind.STRIKETHROUGH, FontStyles.Strikethrough);

			EmitSlices(text, info, font);
		}

		// Glyph quads fill submesh 0 from index 0, one per non whitespace character, hidden ones included
		private static int GetGlyphVertexEnd(TMP_TextInfo info)
		{
#if UNITY_6000_0_OR_NEWER
			// vertexCount includes TMP's decoration tail on Unity 6
			int res = 0;
			int numCharacters = Mathf.Min(info.characterCount, info.characterInfo.Length);
			for (int i = 0; i < numCharacters; i++)
			{
				ref TMP_CharacterInfo characterInfo = ref info.characterInfo[i];
				if (characterInfo.materialReferenceIndex != 0 || !OwnsQuad(characterInfo.character)) { continue; }

				res = Mathf.Max(res, characterInfo.vertexIndex + 4);
			}
			return Mathf.Min(res, info.meshInfo[0].vertices.Length);
#else
			return Mathf.Min(info.meshInfo[0].vertexCount, info.meshInfo[0].vertices.Length);
#endif
		}

		private static bool OwnsQuad(char character)
		{
			return !char.IsWhiteSpace(character) && character != 0x200B && character != 0xAD && character != 0x03;
		}

		private static bool IsDisplayed(TMP_Text text, ref TMP_CharacterInfo characterInfo, int index)
		{
			bool res = index >= text.firstVisibleCharacter && index < text.maxVisibleCharacters && characterInfo.lineNumber < text.maxVisibleLines;
			if (text.overflowMode == TextOverflowModes.Page)
			{
				res &= characterInfo.pageNumber + 1 == text.pageToDisplay;
			}
			return res;
		}

		// TMP's run rules, except that colour never splits a run: every slice carries its own glyph's colour
		private void CollectRuns(TMP_Text text, TMP_TextInfo info, TextDecorationKind kind, FontStyles flag)
		{
			int numCharacters = Mathf.Min(info.characterCount, info.characterInfo.Length);
			int runStart = -1;

			for (int i = 0; i <= numCharacters; i++)
			{
				bool member = false;
				if (i < numCharacters)
				{
					ref TMP_CharacterInfo characterInfo = ref info.characterInfo[i];
					char character = characterInfo.character;
					int lastVisible = info.lineInfo[characterInfo.lineNumber].lastVisibleCharacterIndex;

					member = (characterInfo.style & flag) == flag &&
						IsDisplayed(text, ref characterInfo, i) &&
						i <= lastVisible &&
						character != 10 && character != 11 && character != 13 &&
						(!OwnsQuad(character) || characterInfo.isVisible);

					if (member && runStart >= 0 && BreaksRun(info, kind, runStart, i))
					{
						AddRun(kind, runStart, i - 1);
						runStart = -1;
					}
				}

				if (member)
				{
					if (runStart < 0)
					{
						runStart = i;
					}
				}
				else if (runStart >= 0)
				{
					AddRun(kind, runStart, i - 1);
					runStart = -1;
				}
			}
		}

		private static bool BreaksRun(TMP_TextInfo info, TextDecorationKind kind, int runStart, int index)
		{
			ref TMP_CharacterInfo first = ref info.characterInfo[runStart];
			ref TMP_CharacterInfo current = ref info.characterInfo[index];

			bool res = current.lineNumber != first.lineNumber;
			if (kind == TextDecorationKind.STRIKETHROUGH)
			{
				// The strike sits at a height taken from the run's first character
				res |= current.pointSize != first.pointSize || !Mathf.Approximately(current.baseLine, first.baseLine) || current.fontAsset != first.fontAsset;
			}
			return res;
		}

		// Character ranges are collected for every kind first, slices are emitted once all of them are known
		private void AddRun(TextDecorationKind kind, int firstCharacter, int lastCharacter)
		{
			if (pendingCount == pendingKinds.Length)
			{
				int size = Mathf.Max(8, pendingKinds.Length * 2);
				System.Array.Resize(ref pendingKinds, size);
				System.Array.Resize(ref pendingFirstCharacters, size);
				System.Array.Resize(ref pendingLastCharacters, size);
			}

			pendingKinds[pendingCount] = kind;
			pendingFirstCharacters[pendingCount] = firstCharacter;
			pendingLastCharacters[pendingCount] = lastCharacter;
			pendingCount++;
		}

		private void EmitSlices(TMP_Text text, TMP_TextInfo info, TMP_FontAsset font)
		{
			float padding = atlasSamplePadding;
			byte fontAlpha = ((Color32)text.color).a;
			int maxSlices = Mathf.Max(0, MAX_QUADS - FirstVertex / 4);

			for (int p = 0; p < pendingCount; p++)
			{
				TextDecorationKind kind = pendingKinds[p];
				int firstCharacter = pendingFirstCharacters[p];
				int lastCharacter = pendingLastCharacters[p];

				if (!GetRunBand(info, font, kind, firstCharacter, lastCharacter, padding, out float bottom, out float top, out float left, out float right)) { continue; }

				int numCells = BuildCells(info, firstCharacter, lastCharacter, left, right);
				if (SliceCount + numCells > maxSlices) { break; }

				if (RunCount == runs.Length)
				{
					System.Array.Resize(ref runs, Mathf.Max(8, runs.Length * 2));
				}
				int runIndex = RunCount;
				runs[runIndex] = new TextDecorationRun { kind = kind, firstSlice = SliceCount, sliceCount = numCells };
				RunCount++;

				for (int c = 0; c < numCells; c++)
				{
					int owner = cellOwners[c] >= 0 ? cellOwners[c] : firstCharacter;
					Color32 color = GetSliceColor(info, kind, owner, fontAlpha);
					WriteSlice(info, kind, runIndex, cellOwners[c], cellEdges[c], cellEdges[c + 1], bottom, top, color);
				}
			}

#if UNITY_6000_0_OR_NEWER
			info.meshInfo[0].vertexCount = FirstVertex + VertexCount;
#endif
		}

		// Vertical band and horizontal extent of a run in the text's local space, quad padding included
		private static bool GetRunBand(TMP_TextInfo info, TMP_FontAsset font, TextDecorationKind kind, int firstCharacter, int lastCharacter, float padding,
			out float bottom, out float top, out float left, out float right)
		{
			ref TMP_CharacterInfo first = ref info.characterInfo[firstCharacter];
			ref TMP_CharacterInfo last = ref info.characterInfo[lastCharacter];

			left = Mathf.Min(first.bottomLeft.x, last.bottomLeft.x);
			right = Mathf.Max(first.topRight.x, last.topRight.x);
			bottom = 0f;
			top = 0f;

			float thickness = font.faceInfo.underlineThickness;
			bool res = true;

			switch (kind)
			{
				case TextDecorationKind.UNDERLINE:
				{
					// Lowest underline position and largest scale over the run's glyphs, as TMP does
					float maxScale = 0f;
					float lineY = LARGE_POSITIVE;
					for (int i = firstCharacter; i <= lastCharacter; i++)
					{
						ref TMP_CharacterInfo characterInfo = ref info.characterInfo[i];
						if (!OwnsQuad(characterInfo.character)) { continue; }

						maxScale = Mathf.Max(maxScale, characterInfo.scale);
						lineY = Mathf.Min(lineY, characterInfo.baseLine + font.faceInfo.underlineOffset * maxScale);
					}

					// A run of spaces only: TMP leaves it off screen
					res = maxScale > 0f;
					top = lineY + padding * maxScale;
					bottom = lineY - (thickness + padding) * maxScale;
					break;
				}
				case TextDecorationKind.STRIKETHROUGH:
				{
					float scale = first.scale;
					TMP_FontAsset strikeFont = first.fontAsset != null ? first.fontAsset : font;
					float lineY = first.baseLine + strikeFont.faceInfo.strikethroughOffset * scale;

					res = scale > 0f;
					top = lineY + padding * scale;
					bottom = lineY - (thickness + padding) * scale;
					break;
				}
				case TextDecorationKind.HIGHLIGHT:
				{
					bottom = LARGE_POSITIVE;
					top = -LARGE_POSITIVE;
					for (int i = firstCharacter; i <= lastCharacter; i++)
					{
						ref TMP_CharacterInfo characterInfo = ref info.characterInfo[i];
						TMP_Offset markPadding = characterInfo.highlightState.padding;

						bottom = Mathf.Min(bottom, characterInfo.descender - markPadding.bottom);
						top = Mathf.Max(top, characterInfo.ascender + markPadding.top);
					}

					left -= first.highlightState.padding.left;
					right += last.highlightState.padding.right;
					break;
				}
			}

			return res && right > left;
		}

		// Splits [left, right] into one cell per visible glyph of the run, meeting halfway across the gap between neighbours, so spaces are shared by the letters around them
		private int BuildCells(TMP_TextInfo info, int firstCharacter, int lastCharacter, float left, float right)
		{
			int capacity = lastCharacter - firstCharacter + 2;
			if (cellOwners.Length < capacity)
			{
				cellOwners = new int[capacity];
				cellEdges = new float[capacity + 1];
			}

			int numCells = 0;
			for (int i = firstCharacter; i <= lastCharacter; i++)
			{
				if (info.characterInfo[i].isVisible)
				{
					cellOwners[numCells] = i;
					numCells++;
				}
			}

			if (numCells == 0)
			{
				cellOwners[0] = -1;
				cellEdges[0] = left;
				cellEdges[1] = right;
				return 1;
			}

			// Right to left text lays characters out in decreasing x
			SortOwnersByX(info, numCells);

			cellEdges[0] = left;
			for (int c = 1; c < numCells; c++)
			{
				float gapMiddle = (info.characterInfo[cellOwners[c - 1]].topRight.x + info.characterInfo[cellOwners[c]].bottomLeft.x) * 0.5f;
				cellEdges[c] = Mathf.Clamp(gapMiddle, cellEdges[c - 1], right);
			}
			cellEdges[numCells] = right;

			return numCells;
		}

		private void SortOwnersByX(TMP_TextInfo info, int numCells)
		{
			for (int a = 1; a < numCells; a++)
			{
				int owner = cellOwners[a];
				float x = info.characterInfo[owner].bottomLeft.x;
				int b = a - 1;
				while (b >= 0 && info.characterInfo[cellOwners[b]].bottomLeft.x > x)
				{
					cellOwners[b + 1] = cellOwners[b];
					b--;
				}
				cellOwners[b + 1] = owner;
			}
		}

		private static Color32 GetSliceColor(TMP_TextInfo info, TextDecorationKind kind, int owner, byte fontAlpha)
		{
			ref TMP_CharacterInfo characterInfo = ref info.characterInfo[owner];

			Color32 res;
			switch (kind)
			{
				case TextDecorationKind.STRIKETHROUGH:
					res = characterInfo.strikethroughColor;
					break;
				case TextDecorationKind.HIGHLIGHT:
					res = characterInfo.highlightState.color;
					break;
				default:
					res = characterInfo.underlineColor;
					break;
			}

			res.a = res.a < fontAlpha ? res.a : fontAlpha;

			// The shader raises vertex alpha to the fourth power, while a mark's alpha is an opacity TMP renders linearly
			if (kind == TextDecorationKind.HIGHLIGHT)
			{
				res.a = (byte)Mathf.RoundToInt(Mathf.Pow(res.a / 255f, 0.25f) * 255f);
			}

			return res;
		}

		private void WriteSlice(TMP_TextInfo info, TextDecorationKind kind, int runIndex, int owner, float left, float right, float bottom, float top, Color32 color)
		{
			int first = FirstVertex + SliceCount * 4;
			EnsureCapacity(info, first + 4);

			if (SliceCount == slices.Length)
			{
				System.Array.Resize(ref slices, Mathf.Max(16, slices.Length * 2));
			}
			int ownerVertex = -1;
			if (owner >= 0 && info.characterInfo[owner].isVisible && info.characterInfo[owner].materialReferenceIndex == 0)
			{
				ownerVertex = info.characterInfo[owner].vertexIndex;
			}
			slices[SliceCount] = new TextDecorationSlice { firstVertex = first, kind = kind, runIndex = runIndex, ownerCharacter = owner, ownerVertex = ownerVertex };
			SliceCount++;

			ref TMP_MeshInfo meshInfo = ref info.meshInfo[0];

			meshInfo.vertices[first + 0] = new Vector3(left, bottom, 0f);
			meshInfo.vertices[first + 1] = new Vector3(left, top, 0f);
			meshInfo.vertices[first + 2] = new Vector3(right, top, 0f);
			meshInfo.vertices[first + 3] = new Vector3(right, bottom, 0f);

			// Lines sample the middle column of '_' stretched across the cell, marks a single point inside it, like TMP's own quads
			GlyphRect rect = UnderscoreRect;
			float centerU = (rect.x + rect.width * 0.5f) / atlasWidth;
			Vector2 bottomUV;
			Vector2 topUV;
			if (kind == TextDecorationKind.HIGHLIGHT)
			{
				bottomUV = new Vector2(centerU, (rect.y + rect.height * 0.5f) / atlasHeight);
				topUV = bottomUV;
			}
			else
			{
				bottomUV = new Vector2(centerU, (rect.y - atlasSamplePadding) / atlasHeight);
				topUV = new Vector2(centerU, (rect.y + rect.height + atlasSamplePadding) / atlasHeight);
			}

			for (int k = 0; k < 4; k++)
			{
				Vector2 uv = (k == 1 || k == 2) ? topUV : bottomUV;
#if UNITY_6000_0_OR_NEWER
				meshInfo.uvs0[first + k] = new Vector4(uv.x, uv.y, 0f, 1f);
#else
				meshInfo.uvs0[first + k] = uv;
#endif
				meshInfo.uvs2[first + k] = new Vector2(0f, 1f);
				meshInfo.colors32[first + k] = color;
			}
		}

		// Grows TMP's own arrays in place (TMP_MeshInfo is a struct) by powers of two, as TMP sizes them, so the buffers are not reallocated for every longer text
		private static void EnsureCapacity(TMP_TextInfo info, int numVertices)
		{
			if (numVertices <= info.meshInfo[0].vertices.Length) { return; }

			int numQuads = Mathf.Min(Mathf.NextPowerOfTwo((numVertices + 3) / 4), MAX_QUADS);
			info.meshInfo[0].ResizeMeshInfo(numQuads);
		}
	}
}
