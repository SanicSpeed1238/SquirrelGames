using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	//Draws the unlit dots of an LED panel and lays the same grid over its texts, so their Led effect (Grid: Panel) lights the panel's own dots
	[ExecuteAlways]
	[AddComponentMenu("AllIn1TextShader/LED Panel")]
	public class AllIn1TextLedPanel : MaskableGraphic
	{
		private const string DEFAULT_MATERIAL_GUID = "ae13b20b5cc24163853cd3387e4d6c70";

		private static readonly int PROP_ID_LED_PANEL_MATRIX	= Shader.PropertyToID("_LedPanelMatrix");
		private static readonly int PROP_ID_LED_PANEL_GRID		= Shader.PropertyToID("_LedPanelGrid");
		private static readonly int PROP_ID_LED_PANEL_DOT		= Shader.PropertyToID("_LedPanelDot");

		[Range(4, 512)] public int columns = 96;
		[Range(0.1f, 1.5f)] public float dotSize = 0.8f;
		[Range(0f, 1f)] public float dotRoundness = 1f;
		[Range(0f, 1f)] public float dotSoftness = 0.1f;
		[Range(0f, 1f)] public float dotVariation = 0.15f;

		[Tooltip("Texts lit on this panel. Their material needs the Led effect with Grid set to Panel, and they must draw after the panel")]
		public List<TMP_Text> texts = new List<TMP_Text>();

		private readonly HashSet<Material> drivenMaterials = new HashSet<Material>();
		private readonly HashSet<Material> currentMaterials = new HashSet<Material>();

		protected override void OnEnable()
		{
			base.OnEnable();
			EnableDotChannel();
			PushToTexts();
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			foreach (Material mat in drivenMaterials)
			{
				ReleaseMaterial(mat);
			}
			drivenMaterials.Clear();
		}

		protected override void OnCanvasHierarchyChanged()
		{
			base.OnCanvasHierarchyChanged();
			EnableDotChannel();
		}

		private void LateUpdate()
		{
			PushToTexts();
		}

#if UNITY_EDITOR
		protected override void Reset()
		{
			base.Reset();

			string defaultMaterialPath = AssetDatabase.GUIDToAssetPath(DEFAULT_MATERIAL_GUID);
			if (!string.IsNullOrEmpty(defaultMaterialPath))
			{
				material = AssetDatabase.LoadAssetAtPath<Material>(defaultMaterialPath);
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			PushToTexts();
		}
#endif

		//The dot shape travels in UV1, which a canvas drops unless asked for
		private void EnableDotChannel()
		{
			if (canvas == null) { return; }

			canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
			canvas.rootCanvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
		}

		//Square cells, as many whole rows as fit, centered vertically so the leftover is an even band above and below
		private bool GetGrid(out float cellSize, out int rows, out Vector2 gridOrigin)
		{
			Rect rect = rectTransform.rect;
			cellSize = rect.width / Mathf.Max(columns, 1);
			rows = 0;
			gridOrigin = rect.min;

			if (cellSize <= 0f || rect.height <= 0f) { return false; }

			rows = Mathf.Max(1, Mathf.FloorToInt(rect.height / cellSize + 0.0001f));
			gridOrigin = new Vector2(rect.xMin, rect.yMin + (rect.height - rows * cellSize) * 0.5f);
			return true;
		}

		private Vector4 GetDotParams()
		{
			return new Vector4(dotSize, dotRoundness, dotSoftness, dotVariation);
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			if (!GetGrid(out float cellSize, out int rows, out Vector2 gridOrigin)) { return; }

			Rect rect = rectTransform.rect;
			Vector4 dotParams = GetDotParams();
			Color32 vertexColor = color;

			AddVertex(vh, new Vector2(rect.xMin, rect.yMin), gridOrigin, cellSize, rows, dotParams, vertexColor);
			AddVertex(vh, new Vector2(rect.xMin, rect.yMax), gridOrigin, cellSize, rows, dotParams, vertexColor);
			AddVertex(vh, new Vector2(rect.xMax, rect.yMax), gridOrigin, cellSize, rows, dotParams, vertexColor);
			AddVertex(vh, new Vector2(rect.xMax, rect.yMin), gridOrigin, cellSize, rows, dotParams, vertexColor);

			vh.AddTriangle(0, 1, 2);
			vh.AddTriangle(2, 3, 0);
		}

		private void AddVertex(VertexHelper vh, Vector2 position, Vector2 gridOrigin, float cellSize, int rows, Vector4 dotParams, Color32 vertexColor)
		{
			Vector2 gridPos = (position - gridOrigin) / cellSize;

			UIVertex vertex = UIVertex.simpleVert;
			vertex.position = position;
			vertex.color = vertexColor;
			vertex.uv0 = new Vector4(gridPos.x, gridPos.y, columns, rows);
			vertex.uv1 = dotParams;

			vh.AddVert(vertex);
		}

		private void PushToTexts()
		{
			if (!isActiveAndEnabled) { return; }
			if (!GetGrid(out float cellSize, out int rows, out Vector2 gridOrigin)) { return; }

			//Panel local space to grid space, one unit per cell with integers on the cell corners
			Matrix4x4 localToGrid = Matrix4x4.Scale(new Vector3(1f / cellSize, 1f / cellSize, 1f)) * Matrix4x4.Translate(new Vector3(-gridOrigin.x, -gridOrigin.y, 0f));
			Matrix4x4 worldToGrid = localToGrid * transform.worldToLocalMatrix;
			Vector3 cellWorld = transform.localToWorldMatrix.MultiplyVector(new Vector3(cellSize, 0f, 0f));
			Vector4 dotParams = GetDotParams();

			currentMaterials.Clear();
			for (int i = 0; i < texts.Count; i++)
			{
				TMP_Text text = texts[i];
				if (text == null || text.canvas == null) { continue; }

				Material mat = text.fontSharedMaterial;
				if (mat == null) { continue; }

				//Text vertices arrive in their root canvas's space (nested canvases included), the same space matrix_canvasToRect starts from
				Matrix4x4 canvasToGrid = worldToGrid * text.canvas.rootCanvas.transform.localToWorldMatrix;
				float cellSizeInTextRect = text.rectTransform.worldToLocalMatrix.MultiplyVector(cellWorld).magnitude;

				mat.SetMatrix(PROP_ID_LED_PANEL_MATRIX, canvasToGrid);
				mat.SetVector(PROP_ID_LED_PANEL_GRID, new Vector4(columns, rows, cellSizeInTextRect, 1f));
				mat.SetVector(PROP_ID_LED_PANEL_DOT, dotParams);

				currentMaterials.Add(mat);
			}

			//Texts taken off the list fall back to their own grid
			foreach (Material mat in drivenMaterials)
			{
				if (!currentMaterials.Contains(mat))
				{
					ReleaseMaterial(mat);
				}
			}

			drivenMaterials.Clear();
			drivenMaterials.UnionWith(currentMaterials);
		}

		private void ReleaseMaterial(Material mat)
		{
			if (mat == null) { return; }

			mat.SetVector(PROP_ID_LED_PANEL_GRID, Vector4.zero);
		}
	}
}
