using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(TextMeshProUGUI))]
	[CanEditMultipleObjects]
	public class AllIn1TMPExtension : TMPro.EditorUtilities.TMP_EditorPanelUI
	{
		private AllIn1TextTypewriter typewriter;

		protected override void OnEnable()
		{
			base.OnEnable();

			typewriter = ((TextMeshProUGUI)target).transform.GetComponent<AllIn1TextTypewriter>();
		}

		public override void OnInspectorGUI()
		{
			if (typewriter == null)
			{
				base.OnInspectorGUI();
				return;
			}

			// Make sure Multi selection only includes TMP Text objects.
			if (IsMixSelectionTypes()) return;

			serializedObject.Update();

			bool showTags = TypewriterTagsField.Draw(typewriter, (TMP_Text)target);

			EditorGUI.BeginDisabledGroup(!showTags);
			DrawTextInput();
			EditorGUI.EndDisabledGroup();

			DrawMainSettings();

			DrawExtraSettings();

			EditorGUILayout.Space();

			if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
			{
				m_TextComponent.havePropertiesChanged = true;
				m_HavePropertiesChanged = false;
				EditorUtility.SetDirty(target);
			}
		}
	}

	// The 3D TextMeshPro text gets the same Show Tags toggle: without it, text edited in the stock inspector never reached the typewriter's raw text
	[CustomEditor(typeof(TextMeshPro))]
	[CanEditMultipleObjects]
	public class AllIn1TMPExtension3D : TMPro.EditorUtilities.TMP_EditorPanel
	{
		private AllIn1TextTypewriter typewriter;

		protected override void OnEnable()
		{
			base.OnEnable();

			typewriter = ((TextMeshPro)target).transform.GetComponent<AllIn1TextTypewriter>();
		}

		public override void OnInspectorGUI()
		{
			if (typewriter == null)
			{
				base.OnInspectorGUI();
				return;
			}

			// Make sure Multi selection only includes TMP Text objects.
			if (IsMixSelectionTypes()) return;

			serializedObject.Update();

			bool showTags = TypewriterTagsField.Draw(typewriter, (TMP_Text)target);

			EditorGUI.BeginDisabledGroup(!showTags);
			DrawTextInput();
			EditorGUI.EndDisabledGroup();

			DrawMainSettings();

			DrawExtraSettings();

			EditorGUILayout.Space();

			if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
			{
				m_TextComponent.havePropertiesChanged = true;
				m_HavePropertiesChanged = false;
				EditorUtility.SetDirty(target);
			}
		}
	}

	// Show Tags swaps the text between the typewriter's tagged raw text and the clean text TextMeshPro draws
	internal static class TypewriterTagsField
	{
		public static bool Draw(AllIn1TextTypewriter typewriter, TMP_Text text)
		{
			bool isInPreviewMode = typewriter.previewMode != PreviewMode.NONE;
			EditorGUI.BeginChangeCheck();
			EditorGUI.BeginDisabledGroup(isInPreviewMode);
			typewriter.showTags = EditorGUILayout.Toggle("Show Tags", typewriter.showTags);
			EditorGUI.EndDisabledGroup();
			if (EditorGUI.EndChangeCheck())
			{
				if (typewriter.showTags)
				{
					RetrieveRawText(typewriter, text);
				}
				else
				{
					SaveRawTextAndClean(typewriter, text);
				}

				EditorUtility.SetDirty(typewriter);
			}

			return typewriter.showTags;
		}

		private static void RetrieveRawText(AllIn1TextTypewriter typewriter, TMP_Text text)
		{
			text.text = typewriter.rawText;

			GUI.FocusControl(null);
			EditorUtility.SetDirty(text);
		}

		private static void SaveRawTextAndClean(AllIn1TextTypewriter typewriter, TMP_Text text)
		{
			string rawText = text.text;

			List<TextParsedGroup> outParsedGroups = new List<TextParsedGroup>();
			AllIn1TextShaderConfig config = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);

			typewriter.rawText = rawText;
			string cleanText = MeshEffectsParser.Parse(rawText, config.loopedEffectDatabase, outParsedGroups, typewriter.easeFunctionCollectionLooped);

			text.text = cleanText;

			GUI.FocusControl(null);
			EditorUtility.SetDirty(text);
		}
	}
}
