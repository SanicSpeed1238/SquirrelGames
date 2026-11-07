using AllIn1Assets.SharedExtensions;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class DropShadowDrawer : AbstractEffectDrawer
	{
		//Drop Shadow 01
		private EffectProperty effectPropDropShadowColor;
		private EffectProperty effectPropDropShadowSize;
		private EffectProperty effectPropDropShadowOffsetX;
		private EffectProperty effectPropDropShadowOffsetY;
		private EffectProperty effectPropDropShadowSmoothness;

		//Drop Shadow 02
		private EffectProperty effectPropDropShadow02On;
		private EffectProperty effectPropDropShadow02Color;
		private EffectProperty effectPropDropShadow02Size;
		private EffectProperty effectPropDropShadow02OffsetX;
		private EffectProperty effectPropDropShadow02OffsetY;
		private EffectProperty effectPropDropShadow02Smoothness;


		private const string KW_DROP_SHADOW_02_ON = "_DROP_SHADOW_02_ON";

		public DropShadowDrawer(Plugin3DMaterialInspectorReferences references, PropertiesConfig propertiesConfig, Plugin3DConfig pluginConfig) : base(references, propertiesConfig, pluginConfig)
		{
			this.drawerID = AllIn1TextConstants.DROP_SHADOW_EFFECT_DRAWER_ID;

			effectConfig = propertiesConfig.FindEffectConfigByID("DROP_SHADOW");

			this.effectPropDropShadowColor			= effectConfig.FindEffectPropertyByName("_DropShadowColor");
			this.effectPropDropShadowSize			= effectConfig.FindEffectPropertyByName("_DropShadowsSize");
			this.effectPropDropShadowOffsetX		= effectConfig.FindEffectPropertyByName("_DropShadowsOffsetX");
			this.effectPropDropShadowOffsetY		= effectConfig.FindEffectPropertyByName("_DropShadowsOffsetY");
			this.effectPropDropShadowSmoothness		= effectConfig.FindEffectPropertyByName("_DropShadowsSmoothness");

			this.effectPropDropShadow02On			= effectConfig.FindEffectPropertyByName("_DropShadow02On");
			this.effectPropDropShadow02Color		= effectConfig.FindEffectPropertyByName("_DropShadow02_Color");
			this.effectPropDropShadow02Size			= effectConfig.FindEffectPropertyByName("_DropShadow02_Size");
			this.effectPropDropShadow02OffsetX		= effectConfig.FindEffectPropertyByName("_DropShadow02_OffsetX");
			this.effectPropDropShadow02OffsetY		= effectConfig.FindEffectPropertyByName("_DropShadow02_OffsetY");
			this.effectPropDropShadow02Smoothness	= effectConfig.FindEffectPropertyByName("_DropShadow02_Smoothness");
		}

		protected override void DrawProperties()
		{
			bool isDropShadow02On = references.IsKeywordEnabled(KW_DROP_SHADOW_02_ON);

			EditorGUILayout.BeginVertical(style: references.propertiesStyle); 
			DrawProperty(effectPropDropShadowColor);
			DrawProperty(effectPropDropShadowSize);
			DrawProperty(effectPropDropShadowOffsetX);
			DrawProperty(effectPropDropShadowOffsetY);
			DrawProperty(effectPropDropShadowSmoothness);
			AtlasRangeNote.DrawDropShadowNote(references, "_DropShadowsSize");

			GUILayout.Space(10f);
			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropDropShadow02On);
			if (isDropShadow02On)
			{
				DrawProperty(effectPropDropShadow02Color);
				DrawProperty(effectPropDropShadow02Size);
				DrawProperty(effectPropDropShadow02OffsetX);
				DrawProperty(effectPropDropShadow02OffsetY);
				DrawProperty(effectPropDropShadow02Smoothness);
				AtlasRangeNote.DrawDropShadowNote(references, "_DropShadow02_Size");
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndVertical();
		}
	}
}