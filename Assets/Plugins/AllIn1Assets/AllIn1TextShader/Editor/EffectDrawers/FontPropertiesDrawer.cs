using AllIn1Assets.SharedExtensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class FontPropertiesDrawer
	{
		private Plugin3DMaterialInspectorReferences references;

		private List<int> fontProperties;
		private int fontAtlasPropIdx;
		private int blurredAtlasPropIdx;

		private MaterialProperty fontPropertiesEnabledMatProperty;


		private bool FontPropertiesEnabled
		{
			get
			{
				bool res = fontPropertiesEnabledMatProperty.floatValue > 0;
				return res;
			}
			set
			{
				float floatValue = value ? 1.0f : 0f;
				fontPropertiesEnabledMatProperty.floatValue = floatValue;
			}
		}

		public FontPropertiesDrawer(List<int> fontProperties, int fontAtlasPropIdx, int blurredAtlasPropIdx, Plugin3DMaterialInspectorReferences references)
		{
			this.fontProperties = fontProperties;
			this.fontAtlasPropIdx = fontAtlasPropIdx;
			this.blurredAtlasPropIdx = blurredAtlasPropIdx;

			this.references = references;

			fontPropertiesEnabledMatProperty = references.matProperties[fontProperties[0]];
		}

		public void Draw()
		{
			FontPropertiesEnabled = GUILayout.Toggle(FontPropertiesEnabled, new GUIContent("Show Font Properties"), references.toggleButtonStyle);
			if (FontPropertiesEnabled)
			{
				EditorGUILayout.BeginVertical(references.propertiesStyle);

				MaterialProperty matPropertyFontAtlas = references.matProperties[fontAtlasPropIdx];
				MaterialProperty matProeprtyBlurredAtlas = references.matProperties[blurredAtlasPropIdx];


				EffectPropertyDrawer.DrawProperty(
					materialProperty: matPropertyFontAtlas,
					labelPrefix: string.Empty,
					displayName: matPropertyFontAtlas.displayName,
					customValue: string.Empty,
					allowReset: false,
					isKeywordProperty: false,
					propertyType: EffectProperty.PropertyType.BASIC,
					references);

				EffectPropertyDrawer.DrawProperty(
					materialProperty: matProeprtyBlurredAtlas,
					labelPrefix: string.Empty,
					displayName: matProeprtyBlurredAtlas.displayName,
					customValue: string.Empty,
					allowReset: false,
					isKeywordProperty: false,
					propertyType: EffectProperty.PropertyType.BASIC,
					references);

				EditorUtils.DrawThinLine();


				for (int i = 1; i < fontProperties.Count; i++)
				{
					MaterialProperty matProperty = references.matProperties[fontProperties[i]];
					EffectPropertyDrawer.DrawProperty(
						materialProperty: matProperty,
						labelPrefix: string.Empty,
						displayName: matProperty.displayName,
						customValue: string.Empty,
						allowReset: false,
						isKeywordProperty: false,
						propertyType: EffectProperty.PropertyType.BASIC,
						references: references);

					EditorUtils.DrawThinLine();
				}

				EditorGUILayout.EndVertical();
			}
		}
	}
}