using AllIn1Assets.SharedExtensions;
using UnityEditor;

namespace AllIn1Assets.AllIn1TextShader
{
	public class OutlineDrawer : AbstractEffectDrawer
	{
		public class OutlinePropertyCollection
		{
			public EffectProperty effectPropPosition;
			public EffectProperty effectPropSmoothnessMode;
			public EffectProperty effectPropColor;
			public EffectProperty effectPropWidth;
			public EffectProperty effectPropSmoothness;
			public EffectProperty effectPropDistortionToggle;
			public EffectProperty effectPropDistortTex;
			public EffectProperty effectPropDistortAmount;
			public EffectProperty effectPropDistortXSpeed;
			public EffectProperty effectPropDistortYSpeed;
			public EffectProperty effectPropDistortionMovementType;
			public EffectProperty effectPropDistortionFrequency;

			public EffectProperty effectPropChildOutlineToggle;


			public OutlinePropertyCollection childOutline;

			public string enabledKeyword;
			public string distortionKeyword;
			public string distortionSteppedKeyword;
			public bool isLast;

			public OutlinePropertyCollection(string prefix, string enabledKeyword, string distortionKeyword, string distortionSteppedKeyword, bool isLast, int outlineindex, OutlinePropertyCollection childOutline, AllIn13DEffectConfig effectConfig)
			{
				this.effectPropPosition			= effectConfig.FindEffectPropertyByName($"_{prefix}Position");
				this.effectPropSmoothnessMode	= effectConfig.FindEffectPropertyByName($"_{prefix}SmoothnessMode");
				this.effectPropColor			= effectConfig.FindEffectPropertyByName($"_{prefix}Color");
				this.effectPropWidth			= effectConfig.FindEffectPropertyByName($"_{prefix}Width");
				this.effectPropSmoothness		= effectConfig.FindEffectPropertyByName($"_{prefix}Smoothness");
				this.effectPropDistortionToggle = effectConfig.FindEffectPropertyByName($"_{prefix}DistortionOn");
				this.effectPropDistortTex		= effectConfig.FindEffectPropertyByName($"_{prefix}DistortTex");
				this.effectPropDistortAmount	= effectConfig.FindEffectPropertyByName($"_{prefix}DistortAmount");
				this.effectPropDistortXSpeed	= effectConfig.FindEffectPropertyByName($"_{prefix}DistortXSpeed");
				this.effectPropDistortYSpeed	= effectConfig.FindEffectPropertyByName($"_{prefix}DistortYSpeed");
				this.effectPropDistortionMovementType = effectConfig.FindEffectPropertyByName($"_{prefix}DistortionMovementType");
				this.effectPropDistortionFrequency = effectConfig.FindEffectPropertyByName($"_{prefix}DistortionFrequency");


				this.enabledKeyword = enabledKeyword;
				this.distortionKeyword = distortionKeyword;
				this.distortionSteppedKeyword = distortionSteppedKeyword;
				this.isLast = isLast;
				this.childOutline = childOutline;

				if (!isLast)
				{
					string childPropName = $"_Outline{(outlineindex + 1):D2}On";
					effectPropChildOutlineToggle = effectConfig.FindEffectPropertyByName(childPropName);
				}
			}
		}

		private OutlinePropertyCollection outline01Properties; 
		private OutlinePropertyCollection outline02Properties;
		private OutlinePropertyCollection outline03Properties;

		public OutlineDrawer(Plugin3DMaterialInspectorReferences references, PropertiesConfig propertiesConfig, Plugin3DConfig pluginConfig) : base(references, propertiesConfig, pluginConfig)
		{
			this.drawerID = AllIn1TextConstants.OUTLINE_EFFECT_DRAWER_ID;

			effectConfig = propertiesConfig.FindEffectConfigByID("OUTLINE");

			outline03Properties = new OutlinePropertyCollection("Outline03", "_OUTLINE_03_ON", "_OUTLINE03_DISTORTION", "_OUTLINE03DISTORTIONMOVEMENTTYPE_STEPPED", true, 3, null, effectConfig);
			outline02Properties = new OutlinePropertyCollection("Outline02", "_OUTLINE_02_ON", "_OUTLINE02_DISTORTION", "_OUTLINE02DISTORTIONMOVEMENTTYPE_STEPPED", false, 2, outline03Properties, effectConfig);
			outline01Properties = new OutlinePropertyCollection("Outline01", "_OUTLINE_ON", "_OUTLINE01_DISTORTION", "_OUTLINE01DISTORTIONMOVEMENTTYPE_STEPPED", false, 1, outline02Properties, effectConfig);
		}

		//Draw recursively the outline hierarchy from root (OUTLINE_01)
		private void DrawOutlinePropertyCollection(OutlinePropertyCollection outlineRoot)
		{
			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(outlineRoot.effectPropPosition);
			DrawProperty(outlineRoot.effectPropSmoothnessMode);
			DrawProperty(outlineRoot.effectPropColor);
			DrawProperty(outlineRoot.effectPropWidth);
			DrawProperty(outlineRoot.effectPropSmoothness);

			if (outlineRoot.effectPropDistortionToggle != null)
			{
				DrawProperty(outlineRoot.effectPropDistortionToggle);
				if (references.IsKeywordEnabled(outlineRoot.distortionKeyword))
				{
					DrawProperty(outlineRoot.effectPropDistortTex);
					DrawProperty(outlineRoot.effectPropDistortAmount);
					DrawProperty(outlineRoot.effectPropDistortXSpeed);
					DrawProperty(outlineRoot.effectPropDistortYSpeed);
					DrawProperty(outlineRoot.effectPropDistortionMovementType);

					if (references.IsKeywordEnabled(outlineRoot.distortionSteppedKeyword))
					{
						DrawProperty(outlineRoot.effectPropDistortionFrequency);
					}
				}
			}
			EditorGUILayout.EndVertical();

			if (outlineRoot.childOutline != null)
			{
				bool isChildEnabled = references.IsKeywordEnabled(outlineRoot.childOutline.enabledKeyword);

				EditorGUILayout.BeginVertical();
				DrawProperty(outlineRoot.effectPropChildOutlineToggle);
				
				if (isChildEnabled)
				{
					DrawOutlinePropertyCollection(outlineRoot.childOutline);
				}
				EditorGUILayout.EndVertical();
			}
		}

		protected override void DrawProperties()
		{
			DrawOutlinePropertyCollection(outline01Properties);

			//With the glow on, the note sits on the glow instead, the outermost layer of the stack
			if (!references.IsKeywordEnabled(AtlasRangeNote.KW_OUTER_GLOW_ON))
			{
				AtlasRangeNote.DrawOutsideStackNote(references);
			}
		}
	}
}