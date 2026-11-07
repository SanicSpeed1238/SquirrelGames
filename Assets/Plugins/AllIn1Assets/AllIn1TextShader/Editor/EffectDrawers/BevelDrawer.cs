using AllIn1Assets.SharedExtensions;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class BevelDrawer : AbstractEffectDrawer
	{
		private EffectProperty effectPropBevelStyle;
		private EffectProperty effectPropBevelSize;

		private EffectProperty effectPropBevelLightSource;

		//Light Direction
		private EffectProperty effectPropLightAngle;
		private EffectProperty effectPropLightElevation;

		//Light Color
		private EffectProperty effectPropLightBlending;
		private EffectProperty effectPropLightColor;
		private EffectProperty effectPropLightCutoff;
		private EffectProperty effectPropLightSmoothness;
		private EffectProperty effectPropLightIntensity;

		//Shadow Color
		private EffectProperty effectPropShadowBlending;
		private EffectProperty effectPropShadowColor;
		private EffectProperty effectPropShadowCutoff;
		private EffectProperty effectPropShadowSmoothness;

		//Normal Map
		private EffectProperty effectPropNormalMapOn;
		private EffectProperty effectPropNormalMap;
		private EffectProperty effectPropNormalStrength;
		private EffectProperty effectPropNormalFaceAtten;
		private EffectProperty effectPropNormalOutlineAtten;

		//Specular
		private EffectProperty effectPropSpecularOn;
		private EffectProperty effectPropSpecularColor;
		private EffectProperty effectPropShininess;
		private EffectProperty effectPropSpecularCutoff;
		private EffectProperty effectPropSpecularSmoothness;
		private EffectProperty effectPropSpecularIntensity;

		//Reflections
		private EffectProperty effectPropReflectionsOn;
		private EffectProperty effectPropReflectionsMap;
		private EffectProperty effectPropReflectionsAtten;

		private EffectProperty effectPropBevelAffectOutlines;

		private BevelLightController bevelLightController;

		private const string KW_NORMAL_MAP_ON = "_NORMAL_MAP_ON";
		private const string KW_SPECULAR_ON = "_SPECULAR_ON";
		private const string KW_REFLECTIONS_ON = "_REFLECTIONS_ON";
		private const string KW_BEVEL_AFFECT_OUTLINES_ON = "_BEVEL_AFFECT_OUTLINES_ON";

		public BevelDrawer(Plugin3DMaterialInspectorReferences references, PropertiesConfig propertiesConfig, Plugin3DConfig pluginConfig) : base(references, propertiesConfig, pluginConfig)
		{
			this.drawerID = AllIn1TextConstants.BEVEL_EFFECT_DRAWER_ID;

			effectConfig = propertiesConfig.FindEffectConfigByID("BEVEL");

			this.effectPropBevelStyle = effectConfig.FindEffectPropertyByName("_BevelStyle");
			this.effectPropBevelSize = effectConfig.FindEffectPropertyByName("_BevelSize");
			this.effectPropBevelLightSource = effectConfig.FindEffectPropertyByName("_BevelLightSource");

			this.effectPropLightAngle = effectConfig.FindEffectPropertyByName("_LightAngle");
			this.effectPropLightElevation = effectConfig.FindEffectPropertyByName("_LightElevation");

			this.effectPropLightBlending = effectConfig.FindEffectPropertyByName("_LightBlending");
			this.effectPropLightColor = effectConfig.FindEffectPropertyByName("_LightColor");
			this.effectPropLightCutoff = effectConfig.FindEffectPropertyByName("_LightCutoff");
			this.effectPropLightSmoothness = effectConfig.FindEffectPropertyByName("_LightSmoothness");
			this.effectPropLightIntensity = effectConfig.FindEffectPropertyByName("_LightIntensity");

			this.effectPropShadowBlending = effectConfig.FindEffectPropertyByName("_ShadowBlending");
			this.effectPropShadowColor = effectConfig.FindEffectPropertyByName("_ShadowColor");
			this.effectPropShadowCutoff = effectConfig.FindEffectPropertyByName("_ShadowCutoff");
			this.effectPropShadowSmoothness = effectConfig.FindEffectPropertyByName("_ShadowSmoothness");

			this.effectPropNormalMapOn = effectConfig.FindEffectPropertyByName("_NormalMapOn");
			this.effectPropNormalMap = effectConfig.FindEffectPropertyByName("_NormalMap");
			this.effectPropNormalStrength = effectConfig.FindEffectPropertyByName("_NormalStrength");
			this.effectPropNormalFaceAtten = effectConfig.FindEffectPropertyByName("_NormalFaceAtten");
			this.effectPropNormalOutlineAtten = effectConfig.FindEffectPropertyByName("_NormalOutlineAtten");

			this.effectPropSpecularOn = effectConfig.FindEffectPropertyByName("_SpecularOn");
			this.effectPropSpecularColor = effectConfig.FindEffectPropertyByName("_SpecularColor");
			this.effectPropShininess = effectConfig.FindEffectPropertyByName("_Shininess");
			this.effectPropSpecularCutoff = effectConfig.FindEffectPropertyByName("_SpecularCutoff");
			this.effectPropSpecularSmoothness = effectConfig.FindEffectPropertyByName("_SpecularSmoothness");
			this.effectPropSpecularIntensity = effectConfig.FindEffectPropertyByName("_SpecularIntensity");

			this.effectPropReflectionsOn = effectConfig.FindEffectPropertyByName("_ReflectionsOn");
			this.effectPropReflectionsMap = effectConfig.FindEffectPropertyByName("_ReflectionsMap");
			this.effectPropReflectionsAtten = effectConfig.FindEffectPropertyByName("_ReflectionsAtten");

			this.effectPropBevelAffectOutlines = effectConfig.FindEffectPropertyByName("_BevelAffectOutlines");
		}

		private void RefreshReferences()
		{
			bevelLightController = BevelLightController.Active;
		}

		protected override void DrawProperties()
		{
			RefreshReferences();

			DrawProperty(effectPropBevelStyle);
			DrawProperty(effectPropBevelSize);
			DrawProperty(effectPropBevelLightSource);

			if (references.matProperties[effectPropBevelLightSource.propertyIndex].floatValue > 0.5f)
			{
				if (bevelLightController != null)
				{
					EditorGUILayout.HelpBox("Lighting is driven by the Bevel Light Controller on " + bevelLightController.name + ".", MessageType.Info);
					if (GUILayout.Button("Select Bevel Light Controller"))
					{
						Selection.activeGameObject = bevelLightController.gameObject;
					}
					GUILayout.Space(10f);
					DrawSurfaceProperties();
					return;
				}

				EditorGUILayout.HelpBox("No active Bevel Light Controller in the scene, so the material values below are used.", MessageType.Warning);
				if (GUILayout.Button("Create Bevel Light Controller"))
				{
					GameObject controllerObject = new GameObject("Bevel Light Controller", typeof(BevelLightController));
					Undo.RegisterCreatedObjectUndo(controllerObject, "Create Bevel Light Controller");
					Selection.activeGameObject = controllerObject;
				}
			}

			GUILayout.Space(10f);
			EditorGUILayout.LabelField("Light Direction", style: references.mediumLabelStyle);
			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropLightAngle);
			DrawProperty(effectPropLightElevation);
			EditorGUILayout.EndVertical();

			GUILayout.Space(10f);
			EditorGUILayout.LabelField("Light Color", style: references.mediumLabelStyle);
			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropLightBlending);
			DrawProperty(effectPropLightColor);
			DrawProperty(effectPropLightCutoff);
			DrawProperty(effectPropLightSmoothness);
			DrawProperty(effectPropLightIntensity);
			EditorGUILayout.EndVertical();

			GUILayout.Space(10f);
			EditorGUILayout.LabelField("Shadow Color", style: references.mediumLabelStyle);
			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropShadowBlending);
			DrawProperty(effectPropShadowColor);
			DrawProperty(effectPropShadowCutoff);
			DrawProperty(effectPropShadowSmoothness);
			EditorGUILayout.EndVertical();

			GUILayout.Space(10f);
			DrawSurfaceProperties();
		}

		//Normal map, specular, reflections and Affect Outlines stay on the material in both light sources
		private void DrawSurfaceProperties()
		{
			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropNormalMapOn);
			if (references.IsKeywordEnabled(KW_NORMAL_MAP_ON))
			{
				DrawProperty(effectPropNormalMap);
				DrawProperty(effectPropNormalStrength);
				DrawProperty(effectPropNormalFaceAtten);
				//Outline pixels only get the bevel with Affect Outlines on
				if (references.IsKeywordEnabled(KW_BEVEL_AFFECT_OUTLINES_ON))
				{
					DrawProperty(effectPropNormalOutlineAtten);
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropSpecularOn);
			if (references.IsKeywordEnabled(KW_SPECULAR_ON))
			{
				DrawProperty(effectPropSpecularColor);
				DrawProperty(effectPropShininess);
				DrawProperty(effectPropSpecularCutoff);
				DrawProperty(effectPropSpecularSmoothness);
				DrawProperty(effectPropSpecularIntensity);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropReflectionsOn);
			if (references.IsKeywordEnabled(KW_REFLECTIONS_ON))
			{
				DrawProperty(effectPropReflectionsMap);
				DrawProperty(effectPropReflectionsAtten);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropBevelAffectOutlines);
			EditorGUILayout.EndVertical();

			GUILayout.Space(10f);
		}
	}
}