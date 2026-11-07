using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LegacyText = UnityEngine.UI.Text;

namespace AllIn1Assets.AllIn1TextShader
{
	// Gives an object the TMP_Text AllIn1TextShaderComponent needs as it is added: a legacy UI Text is converted, an object with no text gets a new one
	public static class TextComponentSetup
	{
		private const int MAX_LISTED_LOSSES = 10;
		private const string STR_NO_TEXT = "it has no TextMeshPro text.";

		// Read once by the inspector's removal dialog; null means the user cancelled a conversion and was already told
		private static readonly Dictionary<int, string> failures = new Dictionary<int, string>();

		private struct TextReference
		{
			public MonoBehaviour owner;
			public string propertyPath;
			public bool canHoldTMP;
		}

		[InitializeOnLoadMethod]
		private static void RegisterHook()
		{
			AllIn1TextShaderComponent.textSetupHook = EnsureText;
		}

		public static bool EnsureText(GameObject go)
		{
			bool res = go.TryGetComponent(out TMP_Text _);
			if (!res)
			{
				res = go.TryGetComponent(out LegacyText legacyText) ? ConvertLegacyText(legacyText) : AddNewText(go);
			}
			return res;
		}

		public static string GetMissingTextReason(GameObject go)
		{
			int id = go.GetInstanceID();
			if (!failures.TryGetValue(id, out string res))
			{
				res = GetBlocker(go) ?? STR_NO_TEXT;
			}
			failures.Remove(id);
			return res;
		}

		private static bool IsUIObject(GameObject go)
		{
			bool res = go.transform is RectTransform || go.transform.GetComponentInParent<Canvas>(true) != null;
			return res;
		}

		// An object holds one Graphic, and a 3D text would take over the object's own mesh and renderer
		private static string GetBlocker(GameObject go)
		{
			string res = null;
			if (go.TryGetComponent(out Graphic graphic))
			{
				res = $"it already has a Graphic ({graphic.GetType().Name}), and an object can hold only one. {GetPlacementHint(go)}";
			}
			else if (!IsUIObject(go))
			{
				Component meshComponent = go.GetComponent<Renderer>();
				if (meshComponent == null) { meshComponent = go.GetComponent<MeshFilter>(); }
				if (meshComponent != null)
				{
					res = $"a 3D TextMeshPro text would take over its {meshComponent.GetType().Name}. {GetPlacementHint(go)}";
				}
			}
			return res;
		}

		// A text further down (a button's label, say) is likely where the component was meant to go
		private static string GetPlacementHint(GameObject go)
		{
			string res = "Add the text on a child object.";
			Graphic[] graphics = go.GetComponentsInChildren<Graphic>(true);
			for (int i = 0; i < graphics.Length; i++)
			{
				if (graphics[i].gameObject != go && (graphics[i] is TMP_Text || graphics[i] is LegacyText))
				{
					res = $"Add the component to {graphics[i].name} instead, which has a text.";
					break;
				}
			}
			return res;
		}

		// Under a canvas or on a RectTransform it is a UI text, anywhere else a 3D one. Left empty, the configurator writes its default text
		private static bool AddNewText(GameObject go)
		{
			bool res = GetBlocker(go) == null;
			if (res)
			{
				TMP_Text text = IsUIObject(go) ? (TMP_Text)Undo.AddComponent<TextMeshProUGUI>(go) : Undo.AddComponent<TextMeshPro>(go);
				res = text != null;
				if (!res)
				{
					failures[go.GetInstanceID()] = "a TextMeshPro text could not be added to it (the Console says why).";
				}
			}
			return res;
		}

		private static bool ConvertLegacyText(LegacyText legacyText)
		{
			GameObject go = legacyText.gameObject;
			int id = go.GetInstanceID();

			// A legacy only project may never have imported them; asking for the settings opens TMP's importer
			if (TMP_Settings.instance == null)
			{
				failures[id] = "TextMeshPro's Essential Resources are not imported yet. Import them from the window that opened, then add the component again.";
				return false;
			}

			List<TextReference> references = FindReferences(legacyText);
			List<string> losses = new List<string>();
			for (int i = 0; i < references.Count; i++)
			{
				if (!references[i].canHoldTMP) { losses.Add(DescribeReference(references[i])); }
			}
			losses.AddRange(FindAnimationsOf(legacyText));

			if (losses.Count > 0 && !ConfirmLosses(go, losses))
			{
				failures[id] = null;
				return false;
			}

			TMP_FontAsset fontAsset = ResolveFontAsset(legacyText.font, legacyText.text, out string fontNote);
			LegacyTextSettings settings = new LegacyTextSettings(legacyText);
			RectTransform rectTransform = (RectTransform)go.transform;
			Vector2 sizeDelta = rectTransform.sizeDelta;

			Undo.DestroyObjectImmediate(legacyText);
			if (legacyText != null)
			{
				failures[id] = "its Text could not be removed (the Console says why).";
				return false;
			}

			TextMeshProUGUI tmpText = Undo.AddComponent<TextMeshProUGUI>(go);
			if (tmpText == null)
			{
				failures[id] = "its Text was removed but no TextMeshPro text could be added (the Console says why). Undo brings the Text back.";
				return false;
			}

			settings.ApplyTo(tmpText, fontAsset);
			// TextMeshPro gives a 100 x 100 rect its default text size as it is added
			rectTransform.sizeDelta = sizeDelta;
			bool untruncated = UntruncateIfHidden(tmpText);

			int retargeted = RetargetReferences(references, tmpText);
			ReportConversion(go, fontNote, retargeted, losses, untruncated);
			return true;
		}

		// Truncate drops a line taller than the rect, and the new font's lines can be taller than the Text's were
		private static bool UntruncateIfHidden(TextMeshProUGUI tmpText)
		{
			bool res = false;
			if (tmpText.overflowMode == TextOverflowModes.Truncate && CountVisibleCharacters(tmpText) == 0)
			{
				tmpText.overflowMode = TextOverflowModes.Overflow;
				res = CountVisibleCharacters(tmpText) > 0;
				if (!res) { tmpText.overflowMode = TextOverflowModes.Truncate; }
			}
			return res;
		}

		private static int CountVisibleCharacters(TMP_Text text)
		{
			text.ForceMeshUpdate(ignoreActiveState: true);

			int res = 0;
			TMP_TextInfo info = text.textInfo;
			if (info == null || info.characterInfo == null) { return res; }

			int count = Mathf.Min(info.characterCount, info.characterInfo.Length);
			for (int i = 0; i < count; i++)
			{
				if (info.characterInfo[i].isVisible) { res++; }
			}
			return res;
		}

		// An asset already made from the same font, else a new one like Create Font Atlas makes; Unity's built in fonts become TextMeshPro's default font asset
		private static TMP_FontAsset ResolveFontAsset(Font font, string text, out string note)
		{
			TMP_FontAsset res = null;
			note = null;

			string fontPath = font != null ? AssetDatabase.GetAssetPath(font) : string.Empty;
			bool isProjectFont = fontPath.StartsWith("Assets/") || fontPath.StartsWith("Packages/");
			if (isProjectFont)
			{
				res = FindFontAssetMadeFrom(fontPath);
				if (res != null)
				{
					note = $"{res.name}, the font asset already made from {font.name}";
				}
				else
				{
					res = CreateFontAssetFrom(font, text);
					if (res != null) { note = $"{res.name}, made from {font.name} at {AssetDatabase.GetAssetPath(res)}"; }
				}
			}

			if (res == null)
			{
				res = TMP_Settings.defaultFontAsset;
				if (isProjectFont) { note = $"TextMeshPro's default font asset, since no font asset could be made from {font.name}"; }
				else if (font != null) { note = $"TextMeshPro's default font asset in place of {font.name}, one of Unity's built in fonts"; }
				else { note = "TextMeshPro's default font asset (the Text had no font)"; }
			}

			return res;
		}

		// A font asset's Source Font File is one of its dependencies, so nothing is loaded until one matches
		private static TMP_FontAsset FindFontAssetMadeFrom(string fontPath)
		{
			TMP_FontAsset res = null;

			string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				if (System.Array.IndexOf(AssetDatabase.GetDependencies(path, false), fontPath) < 0) { continue; }

				TMP_FontAsset candidate = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
				bool isBetter = candidate != null && (res == null ||
					(!AllIn1TextUtils.IsFontAssetOptimallyConfigured(res) && AllIn1TextUtils.IsFontAssetOptimallyConfigured(candidate)));
				if (isBetter) { res = candidate; }
			}

			return res;
		}

		private static TMP_FontAsset CreateFontAssetFrom(Font font, string text)
		{
			TMP_FontAsset res = null;
			try
			{
				AllIn1TextShaderConfig config = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);
				if (config != null)
				{
					string assetPath = AssetDatabase.GenerateUniqueAssetPath(EditorUtils.CombineAssetPath(config.UserTMPFontAssetsFolderPath, $"{font.name}.asset"));
					res = AllIn1TextUtils.CreateStaticFontAsset(font, assetPath, GetCharactersBeyondASCII(text));
				}
			}
			catch (System.Exception e)
			{
				Debug.LogWarning($"[AllIn1] No font asset could be made from {font.name}: {e}");
			}
			return res;
		}

		// The new atlas gets the text's own letters on top of the set every font asset gets
		private static string GetCharactersBeyondASCII(string text)
		{
			StringBuilder res = new StringBuilder();
			HashSet<char> added = new HashSet<char>();
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (c >= 160 && !char.IsSurrogate(c) && added.Add(c)) { res.Append(c); }
			}
			return res.ToString();
		}

		// Fields in the Text's scene, or in its prefab when that is edited as an asset, that point at it
		private static List<TextReference> FindReferences(LegacyText legacyText)
		{
			List<TextReference> res = new List<TextReference>();

			UnityEngine.SceneManagement.Scene scene = legacyText.gameObject.scene;
			GameObject[] roots = scene.IsValid() ? scene.GetRootGameObjects() : new GameObject[] { legacyText.transform.root.gameObject };
			for (int r = 0; r < roots.Length; r++)
			{
				MonoBehaviour[] behaviours = roots[r].GetComponentsInChildren<MonoBehaviour>(true);
				for (int b = 0; b < behaviours.Length; b++)
				{
					MonoBehaviour behaviour = behaviours[b];
					if (behaviour == null || behaviour == legacyText) { continue; }

					SerializedObject serializedObject = new SerializedObject(behaviour);
					SerializedProperty property = serializedObject.GetIterator();
					bool enterChildren = true;
					while (property.Next(enterChildren))
					{
						enterChildren = property.propertyType == SerializedPropertyType.Generic;
						if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == legacyText)
						{
							res.Add(new TextReference
							{
								owner = behaviour,
								propertyPath = property.propertyPath,
								canHoldTMP = CanHoldTextMeshPro(behaviour, property.propertyPath)
							});
						}
					}
				}
			}

			return res;
		}

		// An event call names its target's type and method, which the new text would not match
		private static bool CanHoldTextMeshPro(MonoBehaviour owner, string propertyPath)
		{
			bool res = !IsEventCall(propertyPath);
			if (res)
			{
				System.Type fieldType = GetFieldType(owner.GetType(), propertyPath);
				res = fieldType != null && fieldType.IsAssignableFrom(typeof(TextMeshProUGUI));
			}
			return res;
		}

		private static bool IsEventCall(string propertyPath)
		{
			bool res = propertyPath.Contains("m_PersistentCalls");
			return res;
		}

		// Follows the serialized path through the declared field types; null where it can't (SerializeReference fields, for one)
		private static System.Type GetFieldType(System.Type ownerType, string propertyPath)
		{
			System.Type res = ownerType;
			string[] parts = propertyPath.Split('.');
			for (int i = 0; i < parts.Length && res != null; i++)
			{
				if (parts[i] == "Array" && i + 1 < parts.Length && parts[i + 1].StartsWith("data["))
				{
					res = res.IsArray ? res.GetElementType() : (res.IsGenericType ? res.GetGenericArguments()[0] : null);
					i++;
				}
				else
				{
					FieldInfo field = FindField(res, parts[i]);
					res = field != null ? field.FieldType : null;
				}
			}
			return res;
		}

		private static FieldInfo FindField(System.Type type, string name)
		{
			FieldInfo res = null;
			for (System.Type t = type; t != null && res == null; t = t.BaseType)
			{
				res = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			}
			return res;
		}

		// Clips bind their curves by component type, so curves on the Text stop driving anything once it is gone
		private static List<string> FindAnimationsOf(LegacyText legacyText)
		{
			List<string> res = new List<string>();
			HashSet<AnimationClip> checkedClips = new HashSet<AnimationClip>();

			Animator[] animators = legacyText.GetComponentsInParent<Animator>(true);
			for (int a = 0; a < animators.Length; a++)
			{
				RuntimeAnimatorController controller = animators[a].runtimeAnimatorController;
				if (controller == null) { continue; }

				string path = AnimationUtility.CalculateTransformPath(legacyText.transform, animators[a].transform);
				AnimationClip[] clips = controller.animationClips;
				for (int c = 0; c < clips.Length; c++)
				{
					if (clips[c] == null || !checkedClips.Add(clips[c])) { continue; }

					EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clips[c]);
					for (int i = 0; i < bindings.Length; i++)
					{
						if (bindings[i].type == typeof(LegacyText) && bindings[i].path == path)
						{
							res.Add($"the {clips[c].name} clip of the Animator on {animators[a].name} animates the Text");
							break;
						}
					}
				}
			}

			return res;
		}

		private static int RetargetReferences(List<TextReference> references, TMP_Text tmpText)
		{
			int res = 0;
			for (int i = 0; i < references.Count; i++)
			{
				TextReference reference = references[i];
				if (!reference.canHoldTMP || reference.owner == null) { continue; }

				SerializedObject serializedObject = new SerializedObject(reference.owner);
				SerializedProperty property = serializedObject.FindProperty(reference.propertyPath);
				if (property == null) { continue; }

				property.objectReferenceValue = tmpText;
				serializedObject.ApplyModifiedProperties();
				res++;
			}
			return res;
		}

		private static bool ConfirmLosses(GameObject go, List<string> losses)
		{
			string message = $"AllIn1TextShaderComponent replaces the legacy Text on \"{go.name}\" with a TextMeshPro text. " +
				$"These point at the Text and can't follow it (fields are left empty, animations stop driving it):\n\n{DescribeList(losses)}\n\n" +
				"Cancel keeps the Text and removes AllIn1TextShaderComponent.";
			bool res = EditorUtility.DisplayDialog("Convert Text to TextMeshPro", message, "Convert", "Cancel");
			return res;
		}

		private static void ReportConversion(GameObject go, string fontNote, int retargeted, List<string> losses, bool untruncated)
		{
			StringBuilder message = new StringBuilder($"[AllIn1] \"{go.name}\": its legacy Text was replaced by a TextMeshPro - Text (UI) with the same text and settings. Font: {fontNote}.");
			if (retargeted > 0)
			{
				message.Append($" {retargeted} field(s) that pointed at the Text now point at the new text.");
			}

			bool lostSomething = losses.Count > 0 || untruncated;
			if (untruncated)
			{
				message.Append("\nIts Vertical Overflow was Truncate, but its first line no longer fits the rect in the new font and nothing showed, so Overflow is set instead. Make the rect taller to go back to Truncate.");
			}
			if (lostSomething)
			{
				message.Append($"\nNot carried over:\n{DescribeList(losses)}");
			}

			// TextMeshPro builds its own mesh and never runs a Graphic's mesh modifiers
			BaseMeshEffect[] meshEffects = go.GetComponents<BaseMeshEffect>();
			if (meshEffects.Length > 0)
			{
				lostSomething = true;
				string names = string.Join(", ", System.Array.ConvertAll(meshEffects, e => e.GetType().Name));
				message.Append($"\nTextMeshPro ignores the {names} on this object; the text shader's Outline and Drop Shadow effects can stand in for them.");
			}

			if (lostSomething) { Debug.LogWarning(message.ToString(), go); }
			else { Debug.Log(message.ToString(), go); }
		}

		private static string DescribeReference(TextReference reference)
		{
			string field = ObjectNames.NicifyVariableName(reference.propertyPath.Split('.')[0]);
			string kind = IsEventCall(reference.propertyPath) ? " event call" : string.Empty;
			string res = $"{reference.owner.name} ({reference.owner.GetType().Name}): {field}{kind}";
			return res;
		}

		private static string DescribeList(List<string> items)
		{
			StringBuilder res = new StringBuilder();
			int count = Mathf.Min(items.Count, MAX_LISTED_LOSSES);
			for (int i = 0; i < count; i++)
			{
				if (i > 0) { res.Append('\n'); }
				res.Append("• ").Append(items[i]);
			}
			if (items.Count > count)
			{
				res.Append($"\n...and {items.Count - count} more");
			}
			return res.ToString();
		}

		private static TextAlignmentOptions ToAlignment(TextAnchor anchor)
		{
			TextAlignmentOptions res;
			switch (anchor)
			{
				case TextAnchor.UpperLeft:		res = TextAlignmentOptions.TopLeft; break;
				case TextAnchor.UpperCenter:	res = TextAlignmentOptions.Top; break;
				case TextAnchor.UpperRight:		res = TextAlignmentOptions.TopRight; break;
				case TextAnchor.MiddleLeft:		res = TextAlignmentOptions.Left; break;
				case TextAnchor.MiddleCenter:	res = TextAlignmentOptions.Center; break;
				case TextAnchor.MiddleRight:	res = TextAlignmentOptions.Right; break;
				case TextAnchor.LowerLeft:		res = TextAlignmentOptions.BottomLeft; break;
				case TextAnchor.LowerCenter:	res = TextAlignmentOptions.Bottom; break;
				default:						res = TextAlignmentOptions.BottomRight; break;
			}
			return res;
		}

		private static FontStyles ToFontStyles(FontStyle fontStyle)
		{
			FontStyles res = FontStyles.Normal;
			if (fontStyle == FontStyle.Bold || fontStyle == FontStyle.BoldAndItalic) { res |= FontStyles.Bold; }
			if (fontStyle == FontStyle.Italic || fontStyle == FontStyle.BoldAndItalic) { res |= FontStyles.Italic; }
			return res;
		}

		// Text scales the font's line height, TextMeshPro adds to it in hundredths of an em
		private static float ToLineSpacing(float multiplier, TMP_FontAsset fontAsset)
		{
			float lineHeightInEm = 1f;
			if (fontAsset != null && fontAsset.faceInfo.pointSize > 0)
			{
				lineHeightInEm = fontAsset.faceInfo.lineHeight / fontAsset.faceInfo.pointSize;
			}

			float res = (multiplier - 1f) * lineHeightInEm * 100f;
			return res;
		}

		// The Text's settings TextMeshPro has a match for, read before the Text goes
		private class LegacyTextSettings
		{
			private readonly string text;
			private readonly bool richText;
			private readonly int fontSize;
			private readonly FontStyle fontStyle;
			private readonly TextAnchor alignment;
			private readonly bool wrap;
			private readonly bool truncate;
			private readonly float lineSpacing;
			private readonly bool bestFit;
			private readonly int minSize;
			private readonly int maxSize;
			private readonly Color color;
			private readonly bool raycastTarget;
			private readonly Vector4 raycastPadding;
			private readonly bool maskable;
			private readonly bool enabled;

			public LegacyTextSettings(LegacyText legacyText)
			{
				text = legacyText.text;
				richText = legacyText.supportRichText;
				fontSize = legacyText.fontSize;
				fontStyle = legacyText.fontStyle;
				alignment = legacyText.alignment;
				wrap = legacyText.horizontalOverflow == HorizontalWrapMode.Wrap;
				truncate = legacyText.verticalOverflow == VerticalWrapMode.Truncate;
				lineSpacing = legacyText.lineSpacing;
				bestFit = legacyText.resizeTextForBestFit;
				minSize = legacyText.resizeTextMinSize;
				maxSize = legacyText.resizeTextMaxSize;
				color = legacyText.color;
				raycastTarget = legacyText.raycastTarget;
				raycastPadding = legacyText.raycastPadding;
				maskable = legacyText.maskable;
				enabled = legacyText.enabled;
			}

			public void ApplyTo(TextMeshProUGUI tmpText, TMP_FontAsset fontAsset)
			{
				if (fontAsset != null) { tmpText.font = fontAsset; }

				tmpText.text = text;
				tmpText.richText = richText;
				tmpText.fontSize = fontSize;
				tmpText.fontStyle = ToFontStyles(fontStyle);
				tmpText.alignment = ToAlignment(alignment);
#if UNITY_6000_0_OR_NEWER
				tmpText.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
#else
				tmpText.enableWordWrapping = wrap;
#endif
				tmpText.overflowMode = truncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
				tmpText.lineSpacing = ToLineSpacing(lineSpacing, tmpText.font);
				tmpText.enableAutoSizing = bestFit;
				tmpText.fontSizeMin = minSize;
				tmpText.fontSizeMax = maxSize;
				tmpText.color = color;
				tmpText.raycastTarget = raycastTarget;
				tmpText.raycastPadding = raycastPadding;
				tmpText.maskable = maskable;
				tmpText.autoSizeTextContainer = false;
				tmpText.enabled = enabled;
			}
		}
	}
}
