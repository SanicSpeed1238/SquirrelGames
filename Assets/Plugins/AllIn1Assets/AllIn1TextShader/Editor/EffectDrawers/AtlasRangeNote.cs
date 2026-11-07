using System.Globalization;
using AllIn1Assets.SharedExtensions;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Mirrors CalculateOutsideStackReach, CalculateDropShadowReach, CalculateExtrudeOutlineReach and NeedsSDFRebuild in AllIn1TextShader_CommonFunctions.hlsl
	public static class AtlasRangeNote
	{
		public const string KW_OUTER_GLOW_ON = "_OUTER_GLOW_ON";

		private const float ATLAS_RANGE = 1f;

		private const string STR_COST = "so the shader does extra texture reads around each letter. Usually minor, mostly worth checking on low-end mobile.";
		private const string STR_PADDING = "or use a font asset with more Padding, where lower values give the same look.";

		private static readonly string[] OUTLINE_ENABLED_KEYWORDS = { "_OUTLINE_ON", "_OUTLINE_02_ON", "_OUTLINE_03_ON" };
		private static readonly string[] OUTLINE_INSIDE_KEYWORDS = { "_OUTLINE01POSITION_INSIDE", "_OUTLINE02POSITION_INSIDE", "_OUTLINE03POSITION_INSIDE" };
		private static readonly string[] OUTLINE_WIDTH_PROPERTIES = { "_Outline01Width", "_Outline02Width", "_Outline03Width" };

		//Face Dilate, the widest outside outline and the outer glow stack along one distance
		public static void DrawOutsideStackNote(Plugin3DMaterialInspectorReferences references)
		{
			float glowSize = 0f;
			bool isGlowOn = references.IsKeywordEnabled(KW_OUTER_GLOW_ON);

			if (!TryGetFloat(references, "_CustomFaceDilate", out float faceDilate) ||
				!TryGetMaxOutsideOutlineWidth(references, out float outlineWidth, out int numOutlines) ||
				(isGlowOn && !TryGetFloat(references, "_OuterGlowSize", out glowSize)))
			{
				return;
			}

			float reach = Mathf.Max(0f, faceDilate) + outlineWidth + glowSize;
			if (reach <= ATLAS_RANGE)
			{
				return;
			}

			string terms = AppendTerm(string.Empty, "Face Dilate", faceDilate);
			terms = AppendTerm(terms, numOutlines > 1 ? "widest Outline" : "Outline", outlineWidth);
			terms = AppendTerm(terms, "Outer Glow", glowSize);

			EditorGUILayout.HelpBox($"Total reach {Format(reach)} ({terms}) is over 1, {STR_COST}\nTo avoid it, lower the total to 1, {STR_PADDING}", MessageType.Info);
		}

		//The size slider maps to a dilate of lerp(-1, 1, size) on top of Face Dilate
		public static void DrawDropShadowNote(Plugin3DMaterialInspectorReferences references, string sizePropertyName)
		{
			if (!TryGetFloat(references, "_CustomFaceDilate", out float faceDilate) ||
				!TryGetFloat(references, sizePropertyName, out float size))
			{
				return;
			}

			float reach = Mathf.Max(0f, Mathf.LerpUnclamped(-1f, 1f, size) + faceDilate);
			if (reach <= ATLAS_RANGE)
			{
				return;
			}

			float maxSize = 1f - 0.5f * faceDilate;
			EditorGUILayout.HelpBox($"This shadow reaches {Format(reach)}, over 1, {STR_COST}\nTo avoid it, set Shadows Size to {Format(maxSize)} or less, {STR_PADDING}", MessageType.Info);
		}

		//The extruded shape is the face with its outside outlines, and the extrusion outline sits on top of that
		public static void DrawExtrudeNote(Plugin3DMaterialInspectorReferences references)
		{
			if (!TryGetFloat(references, "_CustomFaceDilate", out float faceDilate) ||
				!TryGetMaxOutsideOutlineWidth(references, out float outlineWidth, out int numOutlines) ||
				!TryGetFloat(references, "_ExtrudeOutlineWidth", out float extrudeOutlineWidth))
			{
				return;
			}

			float reach = Mathf.Max(0f, faceDilate) + outlineWidth + extrudeOutlineWidth;
			if (reach <= ATLAS_RANGE)
			{
				return;
			}

			string terms = AppendTerm(string.Empty, "Face Dilate", faceDilate);
			terms = AppendTerm(terms, numOutlines > 1 ? "widest Outline" : "Outline", outlineWidth);
			terms = AppendTerm(terms, "Extrude Outline", extrudeOutlineWidth);

			EditorGUILayout.HelpBox($"Extrusion reach {Format(reach)} ({terms}) is over 1, {STR_COST}\nTo avoid it, lower the total to 1, {STR_PADDING}", MessageType.Info);
		}

		private static bool TryGetMaxOutsideOutlineWidth(Plugin3DMaterialInspectorReferences references, out float width, out int numOutlines)
		{
			width = 0f;
			numOutlines = 0;

			for (int i = 0; i < OUTLINE_ENABLED_KEYWORDS.Length; i++)
			{
				//Each outline needs the ones before it, as in AllIn1TextShader_ShaderFeatures.hlsl
				if (!references.IsKeywordEnabled(OUTLINE_ENABLED_KEYWORDS[i]))
				{
					break;
				}

				if (references.IsKeywordEnabled(OUTLINE_INSIDE_KEYWORDS[i]))
				{
					continue;
				}

				if (!TryGetFloat(references, OUTLINE_WIDTH_PROPERTIES[i], out float outlineWidth))
				{
					return false;
				}

				width = Mathf.Max(width, outlineWidth);
				numOutlines++;
			}

			return true;
		}

		//False when the property is missing or differs across the selected materials
		private static bool TryGetFloat(Plugin3DMaterialInspectorReferences references, string propertyName, out float value)
		{
			value = 0f;

			MaterialProperty[] matProperties = references.matProperties;
			for (int i = 0; i < matProperties.Length; i++)
			{
				if (matProperties[i].name == propertyName)
				{
					value = matProperties[i].floatValue;
					return !matProperties[i].hasMixedValue;
				}
			}

			return false;
		}

		private static string AppendTerm(string terms, string label, float value)
		{
			if (value <= 0f)
			{
				return terms;
			}

			string term = $"{label} {Format(value)}";
			return string.IsNullOrEmpty(terms) ? term : $"{terms} + {term}";
		}

		private static string Format(float value)
		{
			return value.ToString("0.00", CultureInfo.InvariantCulture);
		}
	}
}
