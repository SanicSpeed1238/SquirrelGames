using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public static class MeshEffectsParser
	{
		private const string REGEX_MESH_EFFECT_TAG = @"(?<eraseGroup01><(?<effectContent>[\w;:\]\[,.-]+)>)([^<>]+)(?<eraseGroup02><\/>)";
		private const string REGEX_VECTOR2_TAG = @"(\w+):\[([0-9.-]+),([0-9.-]+)\]";

		public static bool HasSpecialTags(string strTarget)
		{
			MatchCollection matchCollection = Regex.Matches(strTarget, REGEX_MESH_EFFECT_TAG);

			bool res = matchCollection.Count > 0;
			return res;
		}

		public static string Parse(string textToParse, MeshEffectCollection meshEffectDatabase, List<TextParsedGroup> outParsedGroups, EasingFunctionsSO easeFunctionCollection)
		{
			MatchCollection matchCollection = Regex.Matches(textToParse, REGEX_MESH_EFFECT_TAG);

			string cleanText = textToParse;
			

			int numPreviousCharactersRemoved = 0;
			for (int i = 0; i < matchCollection.Count; i++)
			{
				Match match = matchCollection[i];

				Group groupToRemove01		= matchCollection[i].Groups["eraseGroup02"];
				Group groupToRemove02		= matchCollection[i].Groups["eraseGroup01"];
				Group groupEffectContent	= matchCollection[i].Groups["effectContent"];

				TextParsedGroup parsedGroup = new TextParsedGroup();

				int matchIndexSrc = match.Index - numPreviousCharactersRemoved;

				int initIndex = matchIndexSrc;
				int endIndex = initIndex + match.Length - groupToRemove01.Length - groupToRemove02.Length;
				parsedGroup.initIndex = initIndex;
				parsedGroup.length = endIndex - initIndex;


				if (ProcessParsedGroup(ref parsedGroup, meshEffectDatabase, groupEffectContent.ToString(), easeFunctionCollection))
				{
					outParsedGroups.Add(parsedGroup);
				}

				string firstSubstring = cleanText.Substring(0, matchIndexSrc + match.Length);
				int lengthBeforeReplace = firstSubstring.Length;
				int srcIndexSecondSubstring = matchIndexSrc + match.Length;


				firstSubstring = firstSubstring.Replace(groupToRemove01.ToString(), string.Empty);
				firstSubstring = firstSubstring.Replace(groupToRemove02.ToString(), string.Empty);

				cleanText = firstSubstring + cleanText.Substring(matchIndexSrc + match.Length, cleanText.Length - lengthBeforeReplace);

				numPreviousCharactersRemoved += groupToRemove01.Length + groupToRemove02.Length;
			}

			return cleanText;
		}

		// False for an unknown effect: the tag is still stripped, and its text keeps no effect
		public static bool ProcessParsedGroup(ref TextParsedGroup parsedGroup, MeshEffectCollection meshEffectDatabase, string parsedEffectContent, EasingFunctionsSO easeFunctionCollection)
		{
			string[] contentSplit = parsedEffectContent.Split(";");

			string effectID = contentSplit[0];
			MeshEffectInfo effectInfo = meshEffectDatabase.FindInfoByID(effectID);
			if (effectInfo == null)
			{
				Debug.LogWarning($"All In 1 Text: unknown effect tag <{effectID}>, its text is shown without an effect");
				return false;
			}

			List<ParsedParameter> parsedParameters = new List<ParsedParameter>();

			//Parameters
			for(int i = 1; i < contentSplit.Length; i++)
			{
				if (TryParseParameter(contentSplit[i], effectInfo.easeFunctionID.tagBinding, out ParsedParameter parsedParameter))
				{
					parsedParameters.Add(parsedParameter);
				}
				else if (contentSplit[i].Length > 0)
				{
					Debug.LogWarning($"All In 1 Text: ignored parameter '{contentSplit[i]}' in <{parsedEffectContent}>");
				}
			}

			effectInfo.ConfigureParsedGroup(ref parsedGroup, parsedParameters, easeFunctionCollection);
			parsedGroup.effectMask = effectInfo.GetEffectFlag();

			return true;
		}

		// name:[x,y], name:number, or the ease binding followed by an ease ID
		private static bool TryParseParameter(string parameterStr, string easeBinding, out ParsedParameter res)
		{
			res = new ParsedParameter();

			Match vec2Match = Regex.Match(parameterStr, REGEX_VECTOR2_TAG);
			if (vec2Match.Success)
			{
				bool parsed = TryParseFloat(vec2Match.Groups[2].Value, out float x) & TryParseFloat(vec2Match.Groups[3].Value, out float y);

				res.paramName = vec2Match.Groups[1].Value;
				res.vec2Value = new Vector2(x, y);
				res.isVector2 = true;

				return parsed;
			}

			int separatorIndex = parameterStr.IndexOf(':');
			if (separatorIndex <= 0)
			{
				return false;
			}

			res.paramName = parameterStr.Substring(0, separatorIndex);
			string valueStr = parameterStr.Substring(separatorIndex + 1);

			if (TryParseFloat(valueStr, out res.floatValue))
			{
				return true;
			}

			res.stringValue = valueStr;
			return res.paramName == easeBinding && valueStr.Length > 0;
		}

		// No thousands separators, so "1,5" is rejected instead of read as 15
		private static bool TryParseFloat(string floatStr, out float value)
		{
			return float.TryParse(floatStr, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
		}

		public static float ParseFloatStr(string floatStr)
		{
			float res = float.Parse(floatStr, System.Globalization.CultureInfo.InvariantCulture);

			return res;
		}

		public static Vector2 GetVector2FromMatch(Match match)
		{
			string xStr = match.Groups[2].Value;
			string yStr = match.Groups[3].Value;

			Vector2 res = new Vector2(ParseFloatStr(xStr), ParseFloatStr(yStr));

			return res;
		}
	}
}