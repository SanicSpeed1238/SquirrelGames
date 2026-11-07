using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(TextLayeredComponent))]
	public class TextLayeredComponentInspector : Editor
	{
		private const string STR_LAYERS_NEED_UI_TEXT = "Text layers draw through the UI canvas, so they need a TextMeshPro - Text (UI) text. This text is a 3D TextMeshPro, so its layers would never draw.";

		private TextLayeredComponent textLayeredComponent;
		private SerializedProperty spNumLayers;
		private SerializedProperty spLayers;
		private TextShaderMaterialEditor mainMaterialEditor;
		private List<TextShaderMaterialEditor> materialEditorsSlaves;

		private TextLayerDrawer textLayerDrawer;

		private void OnEnable()
		{
			RefreshReferences();
			textLayeredComponent.CheckReferences();

			textLayeredComponent.UpdateLayers();

			EditorApplication.delayCall += ApplyLayersDelayed;
		}

		private void ApplyLayersDelayed()
		{
			if (textLayeredComponent != null)
			{
				textLayeredComponent.ApplyAllLayers();
			}
		}

		private void OnDestroy()
		{
			if(target == null && textLayeredComponent.materialConfigurator != null)
			{
				ComponentRemoved();
			}
		}

		private void ComponentRemoved()
		{
			textLayeredComponent.materialConfigurator.TextMaterial.name = "MAT_" + textLayeredComponent.materialConfigurator.gameObject.name;
		}

		public void RefreshReferences()
		{
			textLayeredComponent = (TextLayeredComponent)target;

			if (spNumLayers == null)
			{
				spNumLayers = serializedObject.FindProperty("numLayers");
			}

			if (spLayers == null)
			{
				spLayers = serializedObject.FindProperty("layers");
			}

			if(textLayerDrawer == null)
			{
				textLayerDrawer = new TextLayerDrawer();
			}

			RefreshMaterialEditors();
		}

		private void RefreshMaterialEditors()
		{
			if (materialEditorsSlaves == null)
			{
				materialEditorsSlaves = new List<TextShaderMaterialEditor>();
			}

			if(mainMaterialEditor == null)
			{
				mainMaterialEditor = (TextShaderMaterialEditor)TextShaderMaterialEditor.CreateEditor(textLayeredComponent.materialConfigurator.TextMaterial, typeof(TextShaderMaterialEditor));
			}
			if (materialEditorsSlaves.Count != textLayeredComponent.slaveLayers.Count)
			{
				materialEditorsSlaves.Clear();
				for (int i = 0; i < textLayeredComponent.slaveLayers.Count; i++)
				{
					TextLayer textLayer = textLayeredComponent.slaveLayers[i];
					Material layerMaterial = textLayer.materialConfigurator != null ? textLayer.materialConfigurator.TextMaterial : null;
					TextShaderMaterialEditor materialEditor = layerMaterial != null ? (TextShaderMaterialEditor)TextShaderMaterialEditor.CreateEditor(layerMaterial, typeof(TextShaderMaterialEditor)) : null;

					materialEditorsSlaves.Add(materialEditor);
				}
			}
		}

		public override void OnInspectorGUI()
		{
			DrawersSharedConfig.Setup();

			RefreshReferences();

			serializedObject.Update();

			// The layer settings are edited on the component directly, not through the serialized object
			if (DrawLayers())
			{
				EditorUtility.SetDirty(textLayeredComponent);
			}

#if ALLIN1_DEVELOP
			EditorUIUtils.AddSpaces(4);
			DrawDevelopmentUI();
#endif

			serializedObject.ApplyModifiedProperties();
		}


#if ALLIN1_DEVELOP
		private void DrawDevelopmentUI()
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);
			EditorGUILayout.LabelField("Development Zone (Only visible in development mode)", DrawersSharedConfig.s_commonStyles.bigLabel);
			EditorGUI.BeginChangeCheck();
			textLayeredComponent.showLayersHierarchy = EditorGUILayout.Toggle("Show layers hierarchy", textLayeredComponent.showLayersHierarchy);
			if (EditorGUI.EndChangeCheck())
			{
				ShowLayersHierarchyChanged();
			}
			EditorGUILayout.EndVertical();
		}

		private void ShowLayersHierarchyChanged()
		{
			if (textLayeredComponent.showLayersHierarchy)
			{
				textLayeredComponent.ShowLayersHierarchy();
			}
			else
			{
				textLayeredComponent.HideLayersHierarchy();
			}
		}
#endif

		private bool DrawLayers()
		{
			bool changes = false;

			EditorGUILayout.BeginVertical();

			bool isUIText = textLayeredComponent.GetComponent<TMP_Text>() is TextMeshProUGUI;
			if (!isUIText)
			{
				EditorGUILayout.HelpBox(STR_LAYERS_NEED_UI_TEXT, MessageType.Warning);
			}

			DrawMainLayer();
			for (int i = 0; i < textLayeredComponent.slaveLayers.Count; i++)
			{
				bool changesInLayer = DrawSlaveLayer(i);
				changes = changes || changesInLayer;
			}

			using (new EditorGUI.DisabledScope(!isUIText))
			{
				if (GUILayout.Button("New Layer"))
				{
					CreateLayer();
				}
			}

			EditorGUILayout.EndVertical();

			return changes;
		}

		private bool DrawLayer(TextLayer textLayer, TextShaderMaterialEditor materialEditor, TextLayerDrawer.DrawLayerHandler drawHandler, bool removeButton = false, int layerIndex = -1)
		{
			bool changesInLayer = false;

			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal();
			if (removeButton)
			{
				if (GUILayout.Button("X", GUILayout.Width(20f)))
				{
					textLayeredComponent.RemoveLayer(layerIndex);
					EditorUtility.SetDirty(textLayeredComponent);
					GUIUtility.ExitGUI();
				}
			}

			textLayer.unfolded = EditorGUILayout.Foldout(textLayer.unfolded, textLayer.layerName, true);
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;

			EditorUIUtils.AddSpaces(1);


			if (textLayer.unfolded && materialEditor != null)
			{
				changesInLayer = drawHandler(textLayer, materialEditor);
			}
			EditorGUILayout.EndVertical();

			return changesInLayer;
		}

		private bool DrawMainLayer()
		{
			bool res = DrawLayer(textLayeredComponent.masterLayer, mainMaterialEditor, textLayerDrawer.DrawMainLayer, false, -1);
			return res;
		}

		private bool DrawSlaveLayer(int index)
		{
			TextLayer textLayer = textLayeredComponent.slaveLayers[index];
			bool res = DrawLayer(textLayer, materialEditorsSlaves[index], textLayerDrawer.DrawSlaveLayer, true, index);
			return res;
		}

		private void CreateLayer()
		{
			TextMaterialConfiguratorSlave res = CreateLayerObject();

			TextLayer newLayer = textLayeredComponent.AddSlaveLayer(res, Vector2.zero);

			res.gameObject.name = newLayer.layerName;
			res.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;

			newLayer.Apply();
			EditorUtility.SetDirty(textLayeredComponent);
		}

		// The layer copies the main text's material and draws its mesh, so it needs nothing of the text but its parent
		private TextMaterialConfiguratorSlave CreateLayerObject()
		{
			Transform trParent = textLayeredComponent.transform;

			GameObject go = new GameObject("Layer", typeof(RectTransform));
			go.layer = trParent.gameObject.layer;
			go.transform.SetParent(trParent, false);

			TMP_Text textSrc = trParent.GetComponent<TMP_Text>();

			TextLayerGraphic layerGraphic = go.AddComponent<TextLayerGraphic>();
			layerGraphic.raycastTarget = false;
			layerGraphic.LayerMaterial = new Material(textSrc.fontSharedMaterial);

			TextMaterialConfiguratorSlave res = go.AddComponent<TextMaterialConfiguratorSlave>();
			res.layerGraphic = layerGraphic;

			return res;
		}
	}
}
