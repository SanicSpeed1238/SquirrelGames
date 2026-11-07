using AllIn1Assets.SharedExtensions;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class ExtrudeDrawer : AbstractEffectDrawer
	{
		//Shape
		private EffectProperty effectPropDepth;
		private EffectProperty effectPropAngle;
		private EffectProperty effectPropColor;
		private EffectProperty effectPropLetterTint;
		private EffectProperty effectPropDepthShade;

		private EffectProperty effectPropLightSource;

		//Light
		private EffectProperty effectPropLightAngle;
		private EffectProperty effectPropLightElevation;
		private EffectProperty effectPropLightColor;
		private EffectProperty effectPropShadowColor;
		private EffectProperty effectPropLightSmoothness;

		//Outline
		private EffectProperty effectPropOutlineWidth;
		private EffectProperty effectPropOutlineColor;

		private BevelLightController bevelLightController;

		public ExtrudeDrawer(Plugin3DMaterialInspectorReferences references, PropertiesConfig propertiesConfig, Plugin3DConfig pluginConfig) : base(references, propertiesConfig, pluginConfig)
		{
			this.drawerID = AllIn1TextConstants.EXTRUDE_EFFECT_DRAWER_ID;

			effectConfig = propertiesConfig.FindEffectConfigByID("EXTRUDE");

			this.effectPropDepth = effectConfig.FindEffectPropertyByName("_ExtrudeDepth");
			this.effectPropAngle = effectConfig.FindEffectPropertyByName("_ExtrudeAngle");
			this.effectPropColor = effectConfig.FindEffectPropertyByName("_ExtrudeColor");
			this.effectPropLetterTint = effectConfig.FindEffectPropertyByName("_ExtrudeLetterTint");
			this.effectPropDepthShade = effectConfig.FindEffectPropertyByName("_ExtrudeDepthShade");

			this.effectPropLightSource = effectConfig.FindEffectPropertyByName("_ExtrudeLightSource");

			this.effectPropLightAngle = effectConfig.FindEffectPropertyByName("_ExtrudeLightAngle");
			this.effectPropLightElevation = effectConfig.FindEffectPropertyByName("_ExtrudeLightElevation");
			this.effectPropLightColor = effectConfig.FindEffectPropertyByName("_ExtrudeLightColor");
			this.effectPropShadowColor = effectConfig.FindEffectPropertyByName("_ExtrudeShadowColor");
			this.effectPropLightSmoothness = effectConfig.FindEffectPropertyByName("_ExtrudeLightSmoothness");

			this.effectPropOutlineWidth = effectConfig.FindEffectPropertyByName("_ExtrudeOutlineWidth");
			this.effectPropOutlineColor = effectConfig.FindEffectPropertyByName("_ExtrudeOutlineColor");
		}

		private void RefreshReferences()
		{
			bevelLightController = BevelLightController.Active;
		}

		protected override void DrawProperties()
		{
			RefreshReferences();

			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropDepth);
			DrawProperty(effectPropAngle);
			DrawProperty(effectPropColor);
			DrawProperty(effectPropLetterTint);
			DrawProperty(effectPropDepthShade);
			EditorGUILayout.EndVertical();

			GUILayout.Space(10f);
			DrawProperty(effectPropLightSource);

			if (DrawGlobalLightNotice())
			{
				GUILayout.Space(10f);
				EditorGUILayout.LabelField("Light", style: references.mediumLabelStyle);
				EditorGUILayout.BeginVertical(style: references.propertiesStyle);
				DrawProperty(effectPropLightAngle);
				DrawProperty(effectPropLightElevation);
				DrawProperty(effectPropLightColor);
				DrawProperty(effectPropShadowColor);
				DrawProperty(effectPropLightSmoothness);
				EditorGUILayout.EndVertical();
			}

			GUILayout.Space(10f);
			EditorGUILayout.LabelField("Outline", style: references.mediumLabelStyle);
			EditorGUILayout.BeginVertical(style: references.propertiesStyle);
			DrawProperty(effectPropOutlineWidth);
			DrawProperty(effectPropOutlineColor);
			AtlasRangeNote.DrawExtrudeNote(references);
			EditorGUILayout.EndVertical();

			GUILayout.Space(10f);
		}

		

		//Returns whether the material's light values are in use, so they should be drawn
		private bool DrawGlobalLightNotice()
		{
			bool res = true;

			if (references.matProperties[effectPropLightSource.propertyIndex].floatValue > 0.5f)
			{
				if (bevelLightController != null)
				{
					EditorGUILayout.HelpBox("Lighting is driven by the Bevel Light Controller on " + bevelLightController.name + ", the same light as Bevel's Global mode.", MessageType.Info);
					if (GUILayout.Button("Select Bevel Light Controller"))
					{
						Selection.activeGameObject = bevelLightController.gameObject;
					}
					res = false;
				}
				else
				{
					EditorGUILayout.HelpBox("No active Bevel Light Controller in the scene, so the material values below are used.", MessageType.Warning);
					if (GUILayout.Button("Create Bevel Light Controller"))
					{
						GameObject controllerObject = new GameObject("Bevel Light Controller", typeof(BevelLightController));
						Undo.RegisterCreatedObjectUndo(controllerObject, "Create Bevel Light Controller");
						Selection.activeGameObject = controllerObject;
					}
				}
			}

			return res;
		}
	}
}
