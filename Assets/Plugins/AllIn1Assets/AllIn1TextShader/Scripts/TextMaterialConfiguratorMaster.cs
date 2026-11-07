using UnityEngine;
using TMPro;
using UnityEngine.TextCore;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	[ExecuteInEditMode]
	public class TextMaterialConfiguratorMaster : TextMaterialConfigurator
	{
		private const string TAG = "TextMaterialConfigurator";

		public struct ExtraInfo
		{
			public Vector4 uvCorners; 
			public Vector4 uvCornersInAtlas;
			public Vector4 characterCenter;
			public float aspectRatio;
			public Vector4 diagonalVector;
			public Vector2 uvNormalized;
			
			public static int GetSize()
			{
				int res = 0;

				res += sizeof(float) * 4;   //uvCorners
				res += sizeof(float) * 4;   //uvCornersInAtlas
				res += sizeof(float) * 4;	//characterCenter
				res += sizeof(float);		//aspectRatio
				res += sizeof(float) * 4;	//diagonalVector
				res += sizeof(float) * 2;   //uvNormalized

				return res; 
			}

			public override string ToString()
			{
				string res = string.Empty;

				res += uvCorners.ToString();
				res += "\n";
				res += uvCornersInAtlas.ToString();
				res += "\n";
				res += characterCenter.ToString();
				res += "\n";
				res += aspectRatio.ToString();
				res += "\n";
				res += diagonalVector.ToString();
				res += "\n";
				res += uvNormalized.ToString();

				return res;
			}
		}

		
		private static readonly int PROP_ID_MATRIX_CANVAS_TO_RECT	= Shader.PropertyToID("matrix_canvasToRect");
		private static readonly int PROP_ID_MATRIX_RECT_TO_CANVAS	= Shader.PropertyToID("matrix_rectToCanvas");
		private static readonly int PROP_ID_RECT_SIZE				= Shader.PropertyToID("rectSize");
		private static readonly int PROP_ID_BOTTOM_LEFT_RECT		= Shader.PropertyToID("bottomLeftRect");
		private static readonly int PROP_ID_EXTRA_INFO_BUFFER		= Shader.PropertyToID("extraInfoBuffer");
		private static readonly int PROP_ID_EM_SIZE					= Shader.PropertyToID("u_emSize");
		private static readonly int PROP_ID_SCALE_X					= Shader.PropertyToID("_ScaleX");
		private static readonly int PROP_ID_SCALE_Y					= Shader.PropertyToID("_ScaleY");
		private static readonly int PROP_ID_TEXTURE_WIDTH			= Shader.PropertyToID("_TextureWidth");
		private static readonly int PROP_ID_TEXTURE_HEIGHT			= Shader.PropertyToID("_TextureHeight");
		private static readonly int PROP_ID_ATLAS_PADDING			= Shader.PropertyToID("_AtlasPadding");

		private const string DEFAULT_TEXT = "ALLIN1 TEXT";

		// ExtraInfo.characterCenter.w, read by the vertex shader: glyphs grow on every side, underline and strikethrough slices only across the line, marks not at all
		private const float EXTRA_INFO_KIND_GLYPH = 1f;
		private const float EXTRA_INFO_KIND_LINE = 2f;
		private const float EXTRA_INFO_KIND_MARK = 3f;
		// Extrude's back copy of a quad is its kind plus this (4 glyph, 5 line, 6 mark)
		private const float EXTRA_INFO_KIND_BACK_OFFSET = 3f;

		// TMP's quad order: bottom left, top left, top right, bottom right
		private static readonly Vector2[] QUAD_CORNERS_UV = { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };

		private ExtraInfo[] extraInfoList;
		private float[] timingSeeds;
		// Seeds carried over from the previous allocation, so growing the text does not restart time based effects on the letters already there
		private int preservedSeedCount;
		private bool[] ownedVertices = new bool[0];

		private Vector4 rectSizeVec4;
		private Matrix4x4 canvasToRect;
		private Matrix4x4 rectToCanvas;
		private Vector4 bottomLeftRectVec4;

		private ComputeBuffer bufferChacterData;
		private ComputeBuffer timingSeedsBuffer;

		private bool pendingBufferRefreshing;
		private int refreshedOutputVersion = -1;
		private readonly TextQuadDrawOrder drawOrder = new TextQuadDrawOrder();
		
		public bool debugEnabled;


		private void OnEnable()
		{
			SubscribeToTMPRegeneration();

#if UNITY_EDITOR
			OnEnableEditor();
#endif
		}

		private void SubscribeToTMPRegeneration()
		{
			if (textComponent == null) { return; }

			textComponent.OnPreRenderText -= OnTMPPreRenderText;
			textComponent.OnPreRenderText += OnTMPPreRenderText;
		}

		private void OnDisable()
		{
			if (textComponent != null)
			{
				textComponent.OnPreRenderText -= OnTMPPreRenderText;
			}

			ReleaseBuffers();
			ReleaseMaterialClaim();
		}

		private void OnDestroy()
		{
			ReleaseBuffers();
			ReleaseMaterialClaim();
			DestroyOwnedMaterialInstance();

			// The text may go back to a TMP material, whose face texture mapping TMP writes into uv2.x again on its next layout
			if (textMeshHandler != null)
			{
				textMeshHandler.stampVertexIndices = false;
			}
		}

		// TMP relaid the text out (spacing, size, alignment...): re-snapshot the rest pose and push fresh per-character data
		private void OnTMPPreRenderText(TMP_TextInfo textInfo)
		{
			if (textMeshHandler == null) { return; }

			textMeshHandler.OnTMPLayoutRegenerated(textInfo);

			if (!AreBuffersSupported()) { return; }

			// Same vertex count: only the data changed, so refresh in place. Reallocating would also re-roll the timing seeds
			bool sameSize = bufferChacterData != null && bufferChacterData.IsValid() &&
				extraInfoList != null && extraInfoList.Length == GetNumBufferElements();
			if (!sameSize)
			{
				// Reallocate now, so the mesh TMP is about to upload never renders against the previous layout's buffer
				RefreshBuffers();
				if (TextMaterial != null)
				{
					CalculateData();
					UpdateMaterial();
				}
			}

			RefreshBuffersData(!sameSize);
			RefreshBuffersDataInLayers(!sameSize);
		}

#if UNITY_EDITOR
		private void OnEnableEditor()
		{
			SetPendingBufferRefreshing(true);
		}
#endif

		private void Awake()
		{
			if (Application.isPlaying)
			{
				Init();
			}
		}

		private void LateUpdate()
		{
			if (Application.isPlaying)
			{
				UpdateConfiguration();
			}
		}

#if UNITY_EDITOR
		private void Reset()
		{
			this.textComponent = GetComponent<TMP_Text>();
			this.textLayeredComponent = GetComponent<TextLayeredComponent>();

			Init();

			// The object's own scene, not the active one: preview scene objects (the material inspector preview) must not dirty the user's scene
			if (!EditorSceneManager.IsPreviewScene(gameObject.scene))
			{
				EditorSceneManager.MarkSceneDirty(gameObject.scene);
			}
		}
#endif

		public void SetPendingBufferRefreshing(bool value)
		{
			this.pendingBufferRefreshing = value;
		}

		public override void InitMeshHandler()
		{
			int instanceID = gameObject.GetInstanceID();
			textMeshHandler = TextMeshHandlerRegistry.GetOrCreateTextMeshHandler(instanceID, textComponent);
			textMeshHandler.stampVertexIndices = true;

			// Layers register first so the write below reaches them too
			if(textLayeredComponent != null)
			{
				textLayeredComponent.InitMeshHandlers();
			}

			// Attaches the spline and warp if they enabled before this handler existed; anything not live stays detached and attaches itself on enable
			AllIn1TextSpline spline = GetComponent<AllIn1TextSpline>();
			bool splineLive = spline != null && spline.enabled && spline.gameObject.activeInHierarchy;
			textMeshHandler.layoutStage = splineLive ? spline : null;

			AllIn1TextWarp warp = GetComponent<AllIn1TextWarp>();
			bool warpLive = warp != null && warp.enabled && warp.gameObject.activeInHierarchy;
			textMeshHandler.outputModifier = warpLive ? warp : null;

			if (splineLive || warpLive)
			{
				textMeshHandler.WriteChanges();
			}
		}

		public void Init()
		{
			// Awake can run inside AddComponent in play mode, before whoever added the component assigns the text
			if (textComponent == null)
			{
				textComponent = GetComponent<TMP_Text>();
			}
			if (textComponent == null) { return; }

			InitMeshHandler();
			SubscribeToTMPRegeneration();

			if (!AreBuffersSupported())
			{
				UsePlainSubShader();
			}

			RefreshBuffers();

			if(textMeshHandler.GetNumCharacters() == 0)
			{
				textMeshHandler.SetText(DEFAULT_TEXT);
			}

			this.pendingBufferRefreshing = true; 
		}

		// Indexed by vertex ID in the shader, so it spans TMP's whole vertex array, unused capacity included
		private int GetNumBufferElements()
		{
			TMP_TextInfo textInfo = textMeshHandler.tmpText.textInfo;
			if (textInfo == null || textInfo.meshInfo == null || textInfo.meshInfo.Length == 0) { return 0; }

			Vector3[] vertices = textInfo.meshInfo[0].vertices;
			int res = vertices != null ? vertices.Length : 0;
			return res;
		}

		public void RefreshBuffers()
		{
			AllIn1Logger.Log(TAG, "RefreshBuffer()"); 

			ReleaseBuffers();

			int numBufferElements = GetNumBufferElements();
			if (IsValidText() && numBufferElements > 0 && AreBuffersSupported())
			{
				float[] previousSeeds = this.timingSeeds;

				this.extraInfoList = new ExtraInfo[numBufferElements];
				this.timingSeeds = new float[numBufferElements];

				preservedSeedCount = 0;
				if (previousSeeds != null)
				{
					preservedSeedCount = Mathf.Min(previousSeeds.Length, numBufferElements);
					System.Array.Copy(previousSeeds, this.timingSeeds, preservedSeedCount);
				}

				bufferChacterData = new ComputeBuffer(numBufferElements, ExtraInfo.GetSize());
				timingSeedsBuffer = new ComputeBuffer(numBufferElements, sizeof(float));

				RefreshLayersBuffers(numBufferElements);
			}
		}

		private void RefreshLayersBuffers(int numBufferElements)
		{
			if(textLayeredComponent != null)
			{
				textLayeredComponent.RefreshBuffers(numBufferElements);
			}
		}

		private void ReleaseLayersBuffers()
		{
			if (textLayeredComponent != null)
			{
				textLayeredComponent.ReleaseBuffers();
			}
		}

		public void ReleaseBuffers()
		{
			if(bufferChacterData != null)
			{
				bufferChacterData.Release();
				bufferChacterData = null;
			}

			if(timingSeedsBuffer != null)
			{
				timingSeedsBuffer.Release();
				timingSeedsBuffer = null;
			}

			ReleaseLayersBuffers();
		}

		public void UpdateConfiguration(bool forceRefreshBuffer = false)
		{
			if (textMeshHandler == null)			{ return; }
			if (textMeshHandler.tmpText == null)	{ return; }
			if (!IsValidText())						{ return; }
			if (TextMaterial == null)				{ return; }

			// Before this frame writes any per text data, so it lands on this text's own material
			ClaimMaterial();
			if (textLayeredComponent != null)
			{
				textLayeredComponent.ClaimMaterials();
			}

			/*This code handles the font change*/
			if (lastMaterial == null)
			{
				lastMaterial = TextMaterial;
			}

			//TODO: Fix the string comparison
			if (lastMaterial != TextMaterial && TextMaterial.shader.name == "AllIn1Text/AllIn1Text")
			{
				CopyMatchingPropertiesKeepingLightAngle(TextMaterial, lastMaterial);
				lastMaterial = TextMaterial;
				CheckAtlasFontReference();
				RefreshFontAtlas();
				forceRefreshBuffer = true;
				needsRefreshFontReferences = true;
			}
			/**/

			bool needsRefreshBuffer = forceRefreshBuffer || NeedsRefreshBuffers();
			if (NeedsRebuildMeshCache() || pendingBufferRefreshing || textMeshHandler.BackQuadsOutOfDate())
			{
				textMeshHandler.RebuildCache();
			}

			if (textLayeredComponent != null)
			{
				textLayeredComponent.RefreshLayerGraphics();
			}

			// The plain SubShader reads no per character data, so there is nothing to build or upload
			if (!AreBuffersSupported())
			{
				pendingBufferRefreshing = false;
				return;
			}

			CalculateData();
			bool updateTimingSeed = false;
			if (needsRefreshBuffer || pendingBufferRefreshing)
			{
				RefreshBuffers();
				updateTimingSeed = true;
			}

			// The per-character data only changes with the mesh, so a still text skips the rebuild and the upload
			bool outputChanged = textMeshHandler.OutputVersion != refreshedOutputVersion;
			if (updateTimingSeed || pendingBufferRefreshing || outputChanged)
			{
				RefreshBuffersData(updateTimingSeed);
				RefreshBuffersDataInLayers(updateTimingSeed);
			}

			UpdateMaterial();

			// Catches Extrude being toggled or turned on a still text
			drawOrder.Refresh(textMeshHandler.tmpText, TextMaterial, textMeshHandler.GetMeshCache(), false);

			pendingBufferRefreshing = false;
		}

		private Vector2 GetRectSize()
		{
			//rect.size, not sizeDelta: with stretched anchors sizeDelta is only the offset from the parent
			Vector2 res = textMeshHandler.tmpText.rectTransform.rect.size;
			if (res.x <= 0f || res.y <= 0f)
			{
				RectTransform parentRectTransform = textMeshHandler.tmpText.rectTransform.parent as RectTransform;
				if (parentRectTransform != null)
				{
					res = parentRectTransform.rect.size;
				}
			}

			return res;
		}

		private void CalculateData()
		{
			Vector2 rectSize = GetRectSize();
			this.rectSizeVec4 = new Vector4(rectSize.x, rectSize.y, 0f, 0f);

			// UGUI batches every canvas, nested ones too, in its root canvas's space. Without one (3D TMP, the material preview) the mesh is already in rect space
			Canvas canvas = textMeshHandler.tmpText.canvas;
			if (canvas != null) { canvas = canvas.rootCanvas; }
			if (canvas != null)
			{
				this.canvasToRect = textMeshHandler.tmpText.rectTransform.worldToLocalMatrix * canvas.transform.localToWorldMatrix;
				this.rectToCanvas = canvas.transform.worldToLocalMatrix * textMeshHandler.tmpText.rectTransform.localToWorldMatrix;
			}
			else
			{
				this.canvasToRect = Matrix4x4.identity;
				this.rectToCanvas = Matrix4x4.identity;
			}

			//The pivot sets where the rect sits in its own local space, so the corner is only -size/2 for a centered pivot
			Vector2 pivot = textMeshHandler.tmpText.rectTransform.pivot;
			this.bottomLeftRectVec4 = new Vector4(-rectSize.x * pivot.x, -rectSize.y * pivot.y, 0f, 1f);
		}

		private bool NeedsRebuildMeshCache()
		{
			bool res = extraInfoList == null || extraInfoList.Length != GetNumBufferElements();

			return res;
		}

		private bool NeedsRefreshBuffers()
		{
			bool res = NeedsRefreshBuffer(bufferChacterData) || NeedsRefreshBuffer(timingSeedsBuffer);
			return res;
		}

		private bool NeedsRefreshBuffer(ComputeBuffer computeBufferToCheck)
		{
			bool res = false;

			if (computeBufferToCheck == null ||
				!computeBufferToCheck.IsValid())
			{
				res = true;
			}
			else
			{
				bool bufferSizeMissmatch = false;
				if (computeBufferToCheck != null)
				{
					bufferSizeMissmatch = computeBufferToCheck.count != GetNumBufferElements();
				}

				res = res || bufferSizeMissmatch;
				res = res || extraInfoList == null;
			}

			return res;
		} 

		private bool IsValidText()
		{
			bool res = textMeshHandler.GetNumCharacters() > 0;
			return res;
		}

		private Vector4 CalculateUVCornersInAtlas(float margin, float atlasWidth, float atlasHeight, GlyphRect glyphRect)
		{
			Vector4 res = Vector4.zero;

			float minUVx = (float)(glyphRect.x - margin) / atlasWidth;
			float maxUVx = (float)(glyphRect.x + glyphRect.width + margin) / atlasWidth;
			float minUVy = (float)(glyphRect.y - margin) / atlasHeight;
			float maxUVy = (float)(glyphRect.y + glyphRect.height + margin) / atlasHeight;

			res = new Vector4(minUVx, maxUVx, minUVy, maxUVy);

			return res;
		}

		private void RefreshBuffersDataInLayers(bool updateTimingSeed)
		{
			if(textLayeredComponent != null)
			{
				textLayeredComponent.RefreshBuffersDataInLayers(updateTimingSeed);
			}
		}

		public float GetTimingSeedByVertexIndex(int vertexIndex)
		{
			return timingSeeds[vertexIndex];
		}

		public void RefreshBuffersData(bool updateTimingSeed)
		{
			if (bufferChacterData == null || !bufferChacterData.IsValid() || extraInfoList == null) { return; }

			MeshCache meshCache = textMeshHandler.GetMeshCache();

			if(meshCache.subMeshes == null) { return; }
			if(meshCache.subMeshes.Length == 0) { return; }
			if(meshCache.subMeshes[0].outputVerts.Length == 0) { return; }

			TMP_TextInfo textInfo = textMeshHandler.tmpText.textInfo;
			if (textInfo == null || textInfo.characterInfo == null) { return; }

			// Placed positions, so the shader's per-character centers match what TMP renders
			Vector3[] vertices = meshCache.subMeshes[0].outputVerts;

			TMP_FontAsset font = textMeshHandler.GetFontAsset();
			bool hasExtraPadding = textMeshHandler.HasExtraPadding();

			int atlasHeight = font.atlasHeight;
			int atlasWidth = font.atlasWidth;

			// Must match the padding TMP used to expand the mesh geometry, not font.atlasPadding, or glyphs render thinner than TMP.
			float securityMargin = ShaderUtilities.GetPadding(TextMaterial, hasExtraPadding, false);
			// Clamp rect must stay inside the glyph's own SDF block (glyphRect + atlasPadding); texels past it belong to other glyphs. 1 texel guard for bilinear.
			float atlasPadding = font.atlasPadding - 1.0f;

			// Entries no glyph writes this pass (decoration quads, spare capacity, a longer earlier text) must not keep stale data
			System.Array.Clear(extraInfoList, 0, extraInfoList.Length);
			if (ownedVertices.Length < timingSeeds.Length)
			{
				ownedVertices = new bool[timingSeeds.Length];
			}
			System.Array.Clear(ownedVertices, 0, ownedVertices.Length);

			// TMP's own character list: rich text tags never reach it, and vertexIndex says where each visible glyph's quad sits
			int numCharacters = textInfo.characterCount;
			for (int i = 0; i < numCharacters; i++)
			{
				TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
				if (!characterInfo.isVisible) { continue; }
				// Sprites and glyphs from other font assets (fallbacks, bold typefaces) render through TMP sub meshes with their own material
				if (characterInfo.materialReferenceIndex != 0) { continue; }
				if (characterInfo.textElement == null) { continue; }

				int bottomLeftIndex		= characterInfo.vertexIndex;
				int bottomRightIndex	= bottomLeftIndex + 3;
				if (bottomRightIndex >= vertices.Length || bottomRightIndex >= extraInfoList.Length) { continue; }

				GlyphRect glyphRect = characterInfo.textElement.glyph.glyphRect;

				float minUVx = (float)(glyphRect.x - securityMargin) / atlasWidth;
				float maxUVx = (float)(glyphRect.x + glyphRect.width + securityMargin) / atlasWidth;
				float minUVy = (float)(glyphRect.y - securityMargin) / atlasHeight;
				float maxUVy = (float)(glyphRect.y + glyphRect.height + securityMargin) / atlasHeight;

				Vector4 uvCorners = new Vector4(minUVx, maxUVx, minUVy, maxUVy);

				Vector4 uvCornersInAtlas = CalculateUVCornersInAtlas(atlasPadding, atlasWidth, atlasHeight, glyphRect);

				// A seed of 0 marks a slot no glyph has used yet
				bool rollSeed = (updateTimingSeed && bottomRightIndex >= preservedSeedCount) || timingSeeds[bottomLeftIndex] == 0f;
				float timingSeed = rollSeed ? Random.Range(0f, 100f) : timingSeeds[bottomLeftIndex];

				float aspectRatio = (float)glyphRect.height / glyphRect.width;
				WriteQuadExtraInfo(vertices, bottomLeftIndex, uvCorners, uvCornersInAtlas, aspectRatio, EXTRA_INFO_KIND_GLYPH, timingSeed);

				WriteBackQuadExtraInfo(meshCache, vertices, bottomLeftIndex, uvCorners, uvCornersInAtlas, aspectRatio, EXTRA_INFO_KIND_GLYPH, timingSeed);
			}

			RefreshDecorationBuffersData(vertices, atlasWidth, atlasHeight, securityMargin, atlasPadding);

			for (int i = 0; i < timingSeeds.Length; i++)
			{
				if (!ownedVertices[i])
				{
					timingSeeds[i] = 0f;
				}
			}

			bufferChacterData.SetData(extraInfoList);
			timingSeedsBuffer.SetData(timingSeeds);

			refreshedOutputVersion = textMeshHandler.OutputVersion;

			drawOrder.Refresh(textMeshHandler.tmpText, TextMaterial, meshCache, true);
		}

		// Slices sample the '_' glyph: lines its middle column along the whole cell, marks one point inside it, so neighbouring slices meet without a seam
		private void RefreshDecorationBuffersData(Vector3[] vertices, int atlasWidth, int atlasHeight, float securityMargin, float atlasPadding)
		{
			TextDecorationBuilder decorations = textMeshHandler.decorations;
			if (!decorations.HasUnderscore || decorations.SliceCount == 0) { return; }

			GlyphRect rect = decorations.UnderscoreRect;
			float centerU = (rect.x + rect.width * 0.5f) / atlasWidth;
			float centerV = (rect.y + rect.height * 0.5f) / atlasHeight;
			Vector4 lineUVCorners = new Vector4(centerU, centerU, (rect.y - securityMargin) / atlasHeight, (rect.y + rect.height + securityMargin) / atlasHeight);
			Vector4 markUVCorners = new Vector4(centerU, centerU, centerV, centerV);
			Vector4 uvCornersInAtlas = CalculateUVCornersInAtlas(atlasPadding, atlasWidth, atlasHeight, rect);
			float aspectRatio = (float)rect.height / rect.width;

			for (int r = 0; r < decorations.RunCount; r++)
			{
				TextDecorationRun run = decorations.runs[r];

				// One seed for the whole run, kept while its first slice stays where it is, so time based effects move the line as one piece
				int runFirstVertex = decorations.slices[run.firstSlice].firstVertex;
				bool hasSeed = runFirstVertex < timingSeeds.Length && timingSeeds[runFirstVertex] != 0f;
				float runSeed = hasSeed ? timingSeeds[runFirstVertex] : Random.Range(0f, 100f);

				for (int s = run.firstSlice; s < run.firstSlice + run.sliceCount; s++)
				{
					TextDecorationSlice slice = decorations.slices[s];
					if (slice.firstVertex + 3 >= vertices.Length || slice.firstVertex + 3 >= extraInfoList.Length) { continue; }

					bool isMark = slice.kind == TextDecorationKind.HIGHLIGHT;
					Vector4 uvCorners = isMark ? markUVCorners : lineUVCorners;
					float kind = isMark ? EXTRA_INFO_KIND_MARK : EXTRA_INFO_KIND_LINE;

					WriteQuadExtraInfo(vertices, slice.firstVertex, uvCorners, uvCornersInAtlas, aspectRatio, kind, runSeed);
					WriteBackQuadExtraInfo(textMeshHandler.GetMeshCache(), vertices, slice.firstVertex, uvCorners, uvCornersInAtlas, aspectRatio, kind, runSeed);
				}
			}
		}

		// The back copy reads the same glyph with the same seed, so time based effects move both as one
		private void WriteBackQuadExtraInfo(MeshCache meshCache, Vector3[] vertices, int sourceIndex, Vector4 uvCorners, Vector4 uvCornersInAtlas, float aspectRatio, float kind, float timingSeed)
		{
			if (meshCache.backVertexCount == 0 || sourceIndex >= meshCache.backVertexCount) { return; }

			int backIndex = meshCache.backFirstVertex + sourceIndex;
			if (backIndex + 3 >= vertices.Length || backIndex + 3 >= extraInfoList.Length) { return; }

			WriteQuadExtraInfo(vertices, backIndex, uvCorners, uvCornersInAtlas, aspectRatio, kind + EXTRA_INFO_KIND_BACK_OFFSET, timingSeed);
		}

		private void WriteQuadExtraInfo(Vector3[] vertices, int bottomLeftIndex, Vector4 uvCorners, Vector4 uvCornersInAtlas, float aspectRatio, float kind, float timingSeed)
		{
			Vector3 bottomLeftRS = vertices[bottomLeftIndex];
			Vector3 topLeftRS = vertices[bottomLeftIndex + 1];
			Vector3 topRightRS = vertices[bottomLeftIndex + 2];
			Vector3 bottomRightRS = vertices[bottomLeftIndex + 3];

			// xy: edge lengths so the size survives rotation, zw: the glyph's right axis so the shader grows the quad along its own axes
			Vector3 rightEdge = bottomRightRS - bottomLeftRS;
			float quadWidth = rightEdge.magnitude;
			float quadHeight = (topLeftRS - bottomLeftRS).magnitude;
			Vector2 glyphRight = quadWidth > 0.00001f ? new Vector2(rightEdge.x / quadWidth, rightEdge.y / quadWidth) : Vector2.right;
			Vector4 diagonalVector = new Vector4(quadWidth, quadHeight, glyphRight.x, glyphRight.y);

			// w tells the shader what the quad is, see EXTRA_INFO_KIND_*
			Vector3 characterCenterXYZ = (bottomLeftRS + topRightRS) * 0.5f;
			Vector4 characterCenter = new Vector4(characterCenterXYZ.x, characterCenterXYZ.y, characterCenterXYZ.z, kind);

			for (int vertexIndex = 0; vertexIndex < 4; vertexIndex++)
			{
				int extraInfoIndex = bottomLeftIndex + vertexIndex;

				ExtraInfo extraInfo = extraInfoList[extraInfoIndex];
				extraInfo.uvCorners = uvCorners;
				extraInfo.uvCornersInAtlas = uvCornersInAtlas;
				extraInfo.characterCenter = characterCenter;
				extraInfo.aspectRatio = aspectRatio;
				extraInfo.diagonalVector = diagonalVector;
				extraInfo.uvNormalized = QUAD_CORNERS_UV[vertexIndex];

				timingSeeds[extraInfoIndex] = timingSeed;
				ownedVertices[extraInfoIndex] = true;

				extraInfoList[extraInfoIndex] = extraInfo;
			}
		}

		public ExtraInfo GetExtraInfoByCharacterIndex(int characterIndex)
		{
			return extraInfoList[characterIndex];
		}

		public void UpdateMaterial()
		{
			ApplyChangesToMaterial(TextMaterial);

			Material maskCopy = GetMaskCopyMaterial();
			if (maskCopy != null)
			{
				ApplyChangesToMaterial(maskCopy);
			}

			ApplyTimingSeedsBuffer();

			if(textLayeredComponent != null)
			{
				textLayeredComponent.UpdateMaterials();
				textLayeredComponent.ApplyTimingSeedsBuffers();
			}
		} 

		public void ApplyChangesToMaterial(Material mat)
		{
			TMP_FontAsset font = textMeshHandler.GetFontAsset();
			// TMP's own glyph scale: a 3D text that is not orthographic is laid out at a tenth of its font size
			TMP_Text tmpText = textMeshHandler.tmpText;
			float emSize = tmpText.fontSize * font.faceInfo.scale * (tmpText.isOrthographic ? 1f : 0.1f);

			mat.SetFloat(PROP_ID_TEXTURE_WIDTH, font.atlasWidth);
			mat.SetFloat(PROP_ID_TEXTURE_HEIGHT, font.atlasHeight);
			mat.SetFloat(PROP_ID_ATLAS_PADDING, font.atlasPadding);

			mat.SetMatrix(PROP_ID_MATRIX_CANVAS_TO_RECT, canvasToRect);
			mat.SetMatrix(PROP_ID_MATRIX_RECT_TO_CANVAS, rectToCanvas);
			mat.SetVector(PROP_ID_RECT_SIZE, rectSizeVec4);
			mat.SetVector(PROP_ID_BOTTOM_LEFT_RECT, bottomLeftRectVec4);
			mat.SetFloat(PROP_ID_EM_SIZE, emSize);

			mat.SetFloat(PROP_ID_SCALE_X, 1.0f);
			mat.SetFloat(PROP_ID_SCALE_Y, 1.0f);

			mat.SetBuffer(PROP_ID_EXTRA_INFO_BUFFER, bufferChacterData);
		}

		public override void ApplyTimingSeedsBuffer()
		{
			SetTimingSeedsBuffer(timingSeedsBuffer);
		}

		public bool IsFontAtlasSet()
		{
			Texture fontAtlasTex = TextMaterial.GetTexture(PROP_ID_FONT_ATLAS);
			bool res = fontAtlasTex != null;
			return res;
		}

		public void RefreshFontAtlas()
		{
			TMP_FontAsset font = textMeshHandler.GetFontAsset();
			SetFontAtlas(font.atlasTexture);
		}

		public TMP_FontAsset GetFontAsset()
		{
			return textMeshHandler.GetFontAsset();
		}

		public override void SetFontAtlas(Texture mainAtlasTex)
		{
			base.SetFontAtlas(mainAtlasTex);

			if(textLayeredComponent != null)
			{
				textLayeredComponent.SetFontAtlas(mainAtlasTex);
			}
		}

		public override void SetBlurredAtlas(Texture blurredAtlasTex)
		{
			base.SetBlurredAtlas(blurredAtlasTex);

			if (textLayeredComponent != null)
			{
				textLayeredComponent.SetBlurredAtlas(blurredAtlasTex);
			}
		}

		public void Dispose()
		{
			ReleaseBuffers();
		}

		public void CheckAtlasFontReference()
		{
			bool fontAtlasSet = IsFontAtlasSet();

			if (!fontAtlasSet)
			{
				TMP_FontAsset font = textMeshHandler.GetFontAsset();
				SetFontAtlas(font.atlasTexture);
			}
		}

		public ComputeBuffer GetTimingSeedsBuffer()
		{
			return timingSeedsBuffer;
		}

		public void InitMaterial(Material mat)
		{
			mat.SetFloat("_BlendSrc", 1);
			mat.SetFloat("_BlendDst", 10);
			mat.SetFloat("_ZTestMode", 4);
			mat.SetFloat("_CullingMode", 0);
			mat.SetFloat("_ZWrite", 0);
			mat.SetFloat("_ColorMask", 15);

			CheckAtlasFontReference();
		}

		private static readonly int PROP_ID_LIGHT_ANGLE = Shader.PropertyToID("_LightAngle");

		// TMP's SDF shaders name their light angle the same but in radians (ours is in degrees), so a value coming from a TMP material is not taken
		public static bool IsTMPLightAngle(Material source, Material target)
		{
			return source != null && target != null && source.shader != null && source.shader.name.StartsWith("TextMeshPro/")
				&& source.HasProperty(PROP_ID_LIGHT_ANGLE) && target.HasProperty(PROP_ID_LIGHT_ANGLE);
		}

		public static void CopyMatchingPropertiesKeepingLightAngle(Material target, Material source)
		{
			bool keepLightAngle = IsTMPLightAngle(source, target);
			float lightAngle = keepLightAngle ? target.GetFloat(PROP_ID_LIGHT_ANGLE) : 0f;

			target.CopyMatchingPropertiesFromMaterial(source);

			if (keepLightAngle) { target.SetFloat(PROP_ID_LIGHT_ANGLE, lightAngle); }
		}

		public string GetBlurredAtlasName(string originalAtlasName)
		{
			string res = $"{originalAtlasName}_Blurred";
			return res;
		}
	}
}