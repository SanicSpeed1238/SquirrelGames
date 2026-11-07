using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	// A text layer draws its main text's finished mesh with its own material and Extrude order, so it has no layout of its own to drift from the main text
	[ExecuteAlways]
	[RequireComponent(typeof(CanvasRenderer))]
	[AddComponentMenu("")]
	public class TextLayerGraphic : MaskableGraphic
	{
		private static readonly int PROP_ID_MAIN_TEX = Shader.PropertyToID("_MainTex");

		// 0 for the layer itself; its children draw the main text's other sub meshes (fallback fonts, bold typefaces, sprites)
		[SerializeField] private int subMeshIndex;

		private TextMeshHandler source;
		private Mesh mesh;
		private int mirroredLayoutVersion = -1;
		private int mirroredOutputVersion = -1;
		private readonly TextQuadDrawOrder drawOrder = new TextQuadDrawOrder();
		private readonly List<TextLayerGraphic> subMeshGraphics = new List<TextLayerGraphic>();

		// What a sub mesh child's material was made from
		private Material subMeshSourceMaterial;
		private Material subMeshLayerMaterial;
		private bool isFallbackMaterial;

		public Material LayerMaterial
		{
			get { return m_Material; }
			set { material = value; }
		}

		public override Texture mainTexture
		{
			get
			{
				Texture res = m_Material != null && m_Material.HasProperty(PROP_ID_MAIN_TEX) ? m_Material.GetTexture(PROP_ID_MAIN_TEX) : null;
				return res != null ? res : s_WhiteTexture;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if (subMeshIndex == 0)
			{
				CollectSubMeshGraphics();
				TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
			}
		}

		protected override void OnDisable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

			base.OnDisable();
		}

		protected override void OnDestroy()
		{
			ReleaseSubMeshMaterial();
			DestroyMesh();

			base.OnDestroy();
		}

		// The geometry is the main text's, so this rect never changes it
		protected override void OnRectTransformDimensionsChange()
		{
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
		}

		protected override void UpdateGeometry()
		{
			if (subMeshIndex == 0)
			{
				Mirror(true);
			}
			else if (mesh != null)
			{
				canvasRenderer.SetMesh(mesh);
			}
		}

		// The text can overflow its rect, so a RectMask2D culls by the mesh, as TMP does
		public override void Cull(Rect clipRect, bool validRect)
		{
			Rect rect = GetCanvasSpaceMeshRect();
			if (rect.width == 0f || rect.height == 0f) { return; }

			bool cull = !validRect || !clipRect.Overlaps(rect, true);
			if (canvasRenderer.cull != cull)
			{
				canvasRenderer.cull = cull;
				onCullStateChanged.Invoke(cull);
				OnCullingChanged();
			}
		}

		public void SetSource(TextMeshHandler source)
		{
			this.source = source;
			mirroredLayoutVersion = -1;

			Mirror(true);
		}

		// Stretched over its parent with the parent's pivot, so its origin is the parent's and the copied vertices land where the main text's do
		public void FitToParent()
		{
			RectTransform parent = rectTransform.parent as RectTransform;
			if (parent == null) { return; }

			RectTransform rt = rectTransform;
			if (rt.anchorMin != Vector2.zero) { rt.anchorMin = Vector2.zero; }
			if (rt.anchorMax != Vector2.one) { rt.anchorMax = Vector2.one; }
			if (rt.sizeDelta != Vector2.zero) { rt.sizeDelta = Vector2.zero; }
			if (rt.pivot != parent.pivot) { rt.pivot = parent.pivot; }
		}

		public void Place(Vector2 offset, float depth)
		{
			FitToParent();

			RectTransform rt = rectTransform;
			rt.anchoredPosition3D = new Vector3(offset.x, offset.y, depth);
			rt.localRotation = Quaternion.identity;
			rt.localScale = Vector3.one;
		}

		// Copies the main text's output. Safe inside the canvas rebuild: it only hands meshes to the canvas renderers
		public void Mirror(bool force)
		{
			if (subMeshIndex != 0 || source == null || !isActiveAndEnabled) { return; }

			TMP_TextInfo info = source.tmpText != null ? source.tmpText.textInfo : null;
			MeshCache cache = source.GetMeshCache();

			// TMP clears an empty text's mesh without the relayout callback, so the layer clears with it
			if (info == null || info.meshInfo == null || info.meshInfo.Length == 0 || info.characterCount == 0 || cache.subMeshes.Length == 0 || cache.subMeshes[0].outputVerts == null)
			{
				ClearAllMeshes();
				return;
			}

			EnsureMesh();

			bool full = force || mirroredLayoutVersion != source.LayoutVersion || mesh.vertexCount != cache.subMeshes[0].outputVerts.Length;
			if (!full && mirroredOutputVersion == source.OutputVersion) { return; }

			int[] layoutTriangles = info.meshInfo[0].triangles;
			CopyBuffers(ref cache.subMeshes[0], layoutTriangles, full);
			if (full)
			{
				drawOrder.Invalidate();
			}
			drawOrder.Refresh(info, mesh, layoutTriangles, m_Material, cache, full);
			canvasRenderer.SetMesh(mesh);

			MirrorSubMeshes(cache, info, full);

			mirroredLayoutVersion = source.LayoutVersion;
			mirroredOutputVersion = source.OutputVersion;
		}

		// Outside the canvas rebuild, where graphics can't be added or change material: placement, the sub mesh children and Extrude toggled on the layer's material
		public void RefreshLayer()
		{
			if (subMeshIndex != 0) { return; }

			FitToParent();

			TMP_TextInfo info = source != null && source.tmpText != null ? source.tmpText.textInfo : null;
			if (info == null || info.meshInfo == null || info.meshInfo.Length == 0) { return; }

			if (SyncSubMeshGraphics(info))
			{
				Mirror(true);
			}

			bool mirrored = mesh != null && mirroredLayoutVersion == source.LayoutVersion;
			if (mirrored && drawOrder.Refresh(info, mesh, info.meshInfo[0].triangles, m_Material, source.GetMeshCache(), false))
			{
				canvasRenderer.SetMesh(mesh);
			}
		}

		private void OnTextChanged(Object obj)
		{
			if (subMeshIndex != 0 || source == null || obj != source.tmpText) { return; }

			Mirror(false);
		}

		private void CopyBuffers(ref MeshCache.SubMeshBuffers buffers, int[] layoutTriangles, bool full)
		{
			if (full)
			{
				mesh.Clear();
				mesh.SetVertices(buffers.outputVerts);
				mesh.SetUVs(0, buffers.uvs0);
				mesh.SetUVs(1, buffers.uvs2);
				mesh.SetNormals(buffers.normals);
				mesh.SetTangents(buffers.tangents);
				mesh.SetColors(buffers.outputColors);

				// TMP grows the triangles with the vertex arrays, so they never index past them; a stale pair waits for the next relayout
				if (layoutTriangles != null && layoutTriangles.Length / 6 * 4 <= buffers.outputVerts.Length)
				{
					mesh.SetTriangles(layoutTriangles, 0, false);
				}
			}
			else
			{
				mesh.SetVertices(buffers.outputVerts);
				mesh.SetColors(buffers.outputColors);
			}

			mesh.RecalculateBounds();
		}

		private void MirrorSubMeshes(MeshCache cache, TMP_TextInfo info, bool full)
		{
			int count = Mathf.Min(info.materialCount, Mathf.Min(cache.subMeshes.Length, info.meshInfo.Length));

			for (int i = 0; i < subMeshGraphics.Count; i++)
			{
				TextLayerGraphic child = subMeshGraphics[i];
				if (child == null) { continue; }

				int m = child.subMeshIndex;
				if (m < count && cache.subMeshes[m].outputVerts != null)
				{
					child.MirrorSubMesh(ref cache.subMeshes[m], info.meshInfo[m].triangles, full);
				}
				else
				{
					child.ClearMesh();
				}
			}
		}

		private void MirrorSubMesh(ref MeshCache.SubMeshBuffers buffers, int[] layoutTriangles, bool full)
		{
			if (!isActiveAndEnabled) { return; }

			EnsureMesh();
			full = full || mesh.vertexCount != buffers.outputVerts.Length;

			CopyBuffers(ref buffers, layoutTriangles, full);
			canvasRenderer.SetMesh(mesh);
		}

		// One child per sub mesh the main text uses; true when one was added or changed material
		private bool SyncSubMeshGraphics(TMP_TextInfo info)
		{
			bool res = false;

			int count = Mathf.Min(info.materialCount, info.meshInfo.Length);
			for (int m = 1; m < count; m++)
			{
				TextLayerGraphic child = FindSubMeshGraphic(m);
				if (child == null)
				{
					child = CreateSubMeshGraphic(m);
					res = true;
				}

				res = child.SetSubMeshMaterial(info.meshInfo[m].material, m_Material) || res;
				child.FitToParent();
			}

			return res;
		}

		// Font glyphs get the material TMP itself makes for a fallback font from this layer's material; sprites keep their own
		private bool SetSubMeshMaterial(Material sourceMaterial, Material layerMaterial)
		{
			if (sourceMaterial == subMeshSourceMaterial && layerMaterial == subMeshLayerMaterial) { return false; }

			ReleaseSubMeshMaterial();
			subMeshSourceMaterial = sourceMaterial;
			subMeshLayerMaterial = layerMaterial;

			Material res = sourceMaterial;
			bool isFontMaterial = sourceMaterial != null && sourceMaterial.HasProperty(ShaderUtilities.ID_GradientScale) && sourceMaterial.GetTexture(ShaderUtilities.ID_MainTex) != null;
			if (isFontMaterial && layerMaterial != null)
			{
				res = TMP_MaterialManager.GetFallbackMaterial(layerMaterial, sourceMaterial);
				TMP_MaterialManager.AddFallbackMaterialReference(res);
				isFallbackMaterial = true;
			}

			material = res;
			return true;
		}

		private void ReleaseSubMeshMaterial()
		{
			if (isFallbackMaterial && m_Material != null)
			{
				TMP_MaterialManager.ReleaseFallbackMaterial(m_Material);
			}

			isFallbackMaterial = false;
		}

		private TextLayerGraphic FindSubMeshGraphic(int index)
		{
			TextLayerGraphic res = null;

			for (int i = 0; res == null && i < subMeshGraphics.Count; i++)
			{
				if (subMeshGraphics[i] != null && subMeshGraphics[i].subMeshIndex == index)
				{
					res = subMeshGraphics[i];
				}
			}

			return res;
		}

		// Not saved with the scene, like TMP's own sub mesh objects
		private TextLayerGraphic CreateSubMeshGraphic(int index)
		{
			GameObject go = new GameObject($"Layer SubMesh [{index}]", typeof(RectTransform));
			go.hideFlags = HideFlags.DontSave;
			go.layer = gameObject.layer;
			go.SetActive(false);
			go.transform.SetParent(transform, false);

			TextLayerGraphic res = go.AddComponent<TextLayerGraphic>();
			res.subMeshIndex = index;
			res.raycastTarget = false;
			res.Place(Vector2.zero, 0f);

			go.SetActive(true);
			subMeshGraphics.Add(res);

			return res;
		}

		private void CollectSubMeshGraphics()
		{
			subMeshGraphics.Clear();

			for (int i = 0; i < transform.childCount; i++)
			{
				TextLayerGraphic child = transform.GetChild(i).GetComponent<TextLayerGraphic>();
				if (child != null && child.subMeshIndex > 0)
				{
					subMeshGraphics.Add(child);
				}
			}
		}

		private void ClearAllMeshes()
		{
			ClearMesh();

			for (int i = 0; i < subMeshGraphics.Count; i++)
			{
				if (subMeshGraphics[i] != null)
				{
					subMeshGraphics[i].ClearMesh();
				}
			}

			mirroredLayoutVersion = -1;
		}

		private void ClearMesh()
		{
			if (mesh == null || mesh.vertexCount == 0) { return; }

			mesh.Clear();
			if (isActiveAndEnabled)
			{
				canvasRenderer.SetMesh(mesh);
			}
		}

		private void EnsureMesh()
		{
			if (mesh != null) { return; }

			mesh = new Mesh();
			mesh.hideFlags = HideFlags.HideAndDontSave;
			mesh.MarkDynamic();
		}

		private void DestroyMesh()
		{
			if (mesh == null) { return; }

			if (Application.isPlaying)
			{
				Destroy(mesh);
			}
			else
			{
				DestroyImmediate(mesh);
			}
			mesh = null;
		}

		private Rect GetCanvasSpaceMeshRect()
		{
			Canvas parentCanvas = canvas;
			if (parentCanvas == null || parentCanvas.rootCanvas == null || mesh == null) { return Rect.zero; }

			Transform rootCanvasTransform = parentCanvas.rootCanvas.transform;
			Bounds bounds = mesh.bounds;
			Vector2 position = rootCanvasTransform.InverseTransformPoint(rectTransform.position);
			Vector2 lossyScale = (Vector2)rectTransform.lossyScale / (Vector2)rootCanvasTransform.lossyScale;

			Rect res = new Rect(position + (Vector2)bounds.min * lossyScale, (Vector2)bounds.size * lossyScale);
			return res;
		}
	}
}
