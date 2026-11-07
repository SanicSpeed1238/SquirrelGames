using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// Material inspector preview that renders real text: a hidden 3D TMP object driven by the plugin's own configurator, so the shader gets the per-character buffers it needs
	public class TextMaterialPreview
	{
		private const string PREVIEW_TEXT = "AllIn1\nText";
		private const float FONT_SIZE = 100f;
		// Fractions of the text size, since 3D TMP glyph units depend on the font scale
		private const float RECT_MARGIN = 0.2f;
		private const float FRAME_MARGIN = 0.25f;
		private const float CAMERA_DISTANCE = 100f;
		private const double REPAINT_INTERVAL = 1.0 / 15.0;
		private const double REPAINT_TIMEOUT = 0.5;

		// Windows that embed a material editor. Repaints only happen while the mouse is over one, so an idle editor stays quiet
		private static readonly string[] PREVIEW_HOST_WINDOWS = { "InspectorWindow", "PropertyEditor", "ProjectBrowser" };

		private static readonly int PROP_ID_FONT_ATLAS = Shader.PropertyToID("_MainTex");
		private static readonly Color BACKGROUND_COLOR = new Color(0.2f, 0.2f, 0.2f, 1f);

		private PreviewRenderUtility previewUtility;
		private GameObject rootGO;
		private TextMeshPro tmpText;
		private MeshRenderer meshRenderer;
		private TextMaterialConfiguratorMaster configurator;
		private bool rectFitPending;

		// The preview never drives the inspected material: a scene text may share it and would fight over the buffers
		private Material previewMaterial;
		private Material sourceMaterial;
		private bool materialChangePending;

		private readonly Dictionary<Texture, TMP_FontAsset> fontsByAtlas = new Dictionary<Texture, TMP_FontAsset>();
		private readonly Dictionary<MaterialEditor, double> lastDrawTimes = new Dictionary<MaterialEditor, double>();
		private readonly List<MaterialEditor> staleEditors = new List<MaterialEditor>();
		private double lastRepaintTime;
		private double mouseOverHostSince = -1.0;
		private bool repaintHooked;
		private bool lifetimeHooked;

		// Hooked on first use, not at load: the class reads the asset database and must stay out of import time
		private void EnsureLifetimeHooks()
		{
			if (lifetimeHooked) { return; }

			AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			EditorApplication.projectChanged += fontsByAtlas.Clear;
			lifetimeHooked = true;
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			Cleanup();
		}

		public bool CanPreview(MaterialEditor materialEditor)
		{
			bool res = materialEditor.targets.Length == 1 && ResolveFont(materialEditor.target as Material) != null;
			return res;
		}

		// Called by the inspector after an edit, so TMP recomputes its mesh padding and the buffers follow the new properties
		public void NotifyMaterialChanged()
		{
			materialChangePending = true;
		}

		public bool TryDraw(MaterialEditor materialEditor, Rect r, GUIStyle background)
		{
			if (materialEditor.targets.Length != 1) { return false; }

			Material material = materialEditor.target as Material;
			TMP_FontAsset font = ResolveFont(material);
			if (font == null) { return false; }

			RegisterRepaint(materialEditor);

			if (Event.current.type != EventType.Repaint) { return true; }
			if (r.width <= 1f || r.height <= 1f) { return true; }

			EnsureSceneObjects(material, font);
			AssignMaterial(material, font);
			SyncPreviewMaterial(material);

			if (materialChangePending)
			{
				tmpText.UpdateMeshPadding();
				configurator.SetPendingBufferRefreshing(true);
				materialChangePending = false;
			}

			if (rectFitPending)
			{
				FitRectToText();
			}

			configurator.UpdateConfiguration();
			FrameCamera(r);

			previewUtility.BeginPreview(r, background);
			previewUtility.Render(true);
			previewUtility.EndAndDrawPreview(r);

			return true;
		}

		private TMP_FontAsset ResolveFont(Material material)
		{
			EnsureLifetimeHooks();

			if (material == null) { return null; }
			if (!material.HasProperty(PROP_ID_FONT_ATLAS)) { return null; }

			Texture atlas = material.GetTexture(PROP_ID_FONT_ATLAS);
			if (atlas == null) { return null; }

			TMP_FontAsset res;
			if (!fontsByAtlas.TryGetValue(atlas, out res))
			{
				res = FindFontByAtlas(atlas);
				fontsByAtlas[atlas] = res;
			}

			return res;
		}

		private TMP_FontAsset FindFontByAtlas(Texture atlas)
		{
			TMP_FontAsset byName = null;
			string atlasNameByConvention = atlas.name;

			string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
				if (font == null) { continue; }

				if (font.atlasTexture == atlas)
				{
					return font;
				}

				if (byName == null && atlasNameByConvention == font.name + " Atlas")
				{
					byName = font;
				}
			}

			return byName;
		}

		private void EnsureSceneObjects(Material material, TMP_FontAsset font)
		{
			if (previewUtility != null && rootGO != null && tmpText != null && configurator != null) { return; }

			Cleanup();

			previewUtility = new PreviewRenderUtility();

			Camera camera = previewUtility.camera;
			camera.orthographic = true;
			camera.nearClipPlane = 1f;
			camera.farClipPlane = CAMERA_DISTANCE * 2f;
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.backgroundColor = BACKGROUND_COLOR;
			camera.transform.rotation = Quaternion.identity;

			previewUtility.lights[0].intensity = 1f;
			previewUtility.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
			previewUtility.lights[1].intensity = 0.5f;

			// A MeshRenderer text, not a canvas one: the preview camera renders regular renderers, canvases in preview scenes stay blank
			rootGO = new GameObject("AllIn1TextMaterialPreview", typeof(RectTransform));
			rootGO.hideFlags = HideFlags.HideAndDontSave;
			previewUtility.AddSingleGO(rootGO);

			tmpText = rootGO.AddComponent<TextMeshPro>();
			meshRenderer = rootGO.GetComponent<MeshRenderer>();
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.receiveShadows = false;

			tmpText.text = PREVIEW_TEXT;
			tmpText.fontSize = FONT_SIZE;
			tmpText.alignment = TextAlignmentOptions.Center;
#if UNITY_6000_0_OR_NEWER
			tmpText.textWrappingMode = TextWrappingModes.NoWrap;
#else
			tmpText.enableWordWrapping = false;
#endif
			tmpText.overflowMode = TextOverflowModes.Overflow;

			AssignMaterial(material, font);

			configurator = rootGO.AddComponent<TextMaterialConfiguratorMaster>();
			configurator.textComponent = tmpText;
			configurator.Init();

			rectFitPending = true;
		}

		// Rect based effects (gradients, LED cells) span the rect, so it hugs the text instead of TMP's default size
		private void FitRectToText()
		{
			tmpText.ForceMeshUpdate(true, true);

			Bounds textBounds = tmpText.textBounds;
			float margin = Mathf.Max(textBounds.size.x, textBounds.size.y) * RECT_MARGIN;
			tmpText.rectTransform.sizeDelta = new Vector2(textBounds.size.x + margin, textBounds.size.y + margin);

			configurator.SetPendingBufferRefreshing(true);
			rectFitPending = false;
		}

		private void AssignMaterial(Material material, TMP_FontAsset font)
		{
			if (tmpText.font != font)
			{
				tmpText.font = font;
				rectFitPending = true;
			}

			if (previewMaterial == null || sourceMaterial != material)
			{
				if (previewMaterial != null)
				{
					Object.DestroyImmediate(previewMaterial);
				}

				previewMaterial = new Material(material);
				previewMaterial.name = material.name + " (Preview)";
				previewMaterial.hideFlags = HideFlags.HideAndDontSave;
				sourceMaterial = material;

				if (configurator != null)
				{
					configurator.SetPendingBufferRefreshing(true);
				}
			}

			// Assigning the font resets TMP's material to the font default, so the preview material always goes last
			if (tmpText.fontSharedMaterial != previewMaterial)
			{
				tmpText.fontSharedMaterial = previewMaterial;
			}

			// TMP only pushes the material to the renderer on its own rebuild pass, which the preview does not wait for
			if (meshRenderer != null && meshRenderer.sharedMaterial != previewMaterial)
			{
				meshRenderer.sharedMaterial = previewMaterial;
			}
		}

		private void SyncPreviewMaterial(Material material)
		{
			if (previewMaterial.shader != material.shader)
			{
				previewMaterial.shader = material.shader;
			}

			previewMaterial.CopyPropertiesFromMaterial(material);
			previewMaterial.shaderKeywords = material.shaderKeywords;
			previewMaterial.renderQueue = material.renderQueue;
		}

		private void FrameCamera(Rect r)
		{
			Bounds textBounds = tmpText.textBounds;
			Vector3 center = tmpText.rectTransform.TransformPoint(textBounds.center);

			float padding = Mathf.Max(textBounds.size.x, textBounds.size.y) * FRAME_MARGIN;
			float halfWidth = textBounds.extents.x + padding;
			float halfHeight = textBounds.extents.y + padding;
			float aspect = r.width / r.height;

			Camera camera = previewUtility.camera;
			camera.orthographicSize = Mathf.Max(halfHeight, halfWidth / aspect, 0.01f);
			camera.transform.position = new Vector3(center.x, center.y, center.z - CAMERA_DISTANCE);
		}

		// Effects animate with shader time, so the inspector keeps repainting while its preview is visible
		private void RegisterRepaint(MaterialEditor materialEditor)
		{
			lastDrawTimes[materialEditor] = EditorApplication.timeSinceStartup;

			if (!repaintHooked)
			{
				EditorApplication.update += OnEditorUpdate;
				repaintHooked = true;
			}
		}

		private void OnEditorUpdate()
		{
			double now = EditorApplication.timeSinceStartup;
			if (now - lastRepaintTime < REPAINT_INTERVAL) { return; }
			lastRepaintTime = now;

			// Entries are kept while the mouse is away: nothing would redraw them, so they must survive until it comes back
			if (!IsMouseOverPreviewHost())
			{
				mouseOverHostSince = -1.0;
				return;
			}

			if (mouseOverHostSince < 0.0)
			{
				mouseOverHostSince = now;
			}

			// An entry is only stale once the mouse has been over a host long enough for a visible preview to have redrawn
			bool canExpire = now - mouseOverHostSince > REPAINT_TIMEOUT;

			staleEditors.Clear();
			foreach (KeyValuePair<MaterialEditor, double> entry in lastDrawTimes)
			{
				if (entry.Key == null || (canExpire && now - entry.Value > REPAINT_TIMEOUT))
				{
					staleEditors.Add(entry.Key);
				}
				else
				{
					entry.Key.Repaint();
				}
			}

			for (int i = 0; i < staleEditors.Count; i++)
			{
				lastDrawTimes.Remove(staleEditors[i]);
			}

			if (lastDrawTimes.Count == 0)
			{
				EditorApplication.update -= OnEditorUpdate;
				repaintHooked = false;
			}
		}

		private bool IsMouseOverPreviewHost()
		{
			EditorWindow window = EditorWindow.mouseOverWindow;
			if (window == null) { return false; }

			string windowType = window.GetType().Name;
			for (int i = 0; i < PREVIEW_HOST_WINDOWS.Length; i++)
			{
				if (windowType == PREVIEW_HOST_WINDOWS[i]) { return true; }
			}

			return false;
		}

		public void Cleanup()
		{
			if (rootGO != null)
			{
				Object.DestroyImmediate(rootGO);
			}

			if (previewMaterial != null)
			{
				Object.DestroyImmediate(previewMaterial);
			}

			if (previewUtility != null)
			{
				previewUtility.Cleanup();
			}

			previewUtility = null;
			rootGO = null;
			tmpText = null;
			meshRenderer = null;
			configurator = null;
			previewMaterial = null;
			sourceMaterial = null;
		}
	}
}
