using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace AllIn1Assets.AllIn1TextShader
{
	public abstract class TextMaterialConfigurator : MonoBehaviour
	{


		private static readonly int PROP_ID_BLENDING_MODE = Shader.PropertyToID("_BlendingMode");

		protected static readonly int PROP_ID_TIMING_SEEDS_BUFFER = Shader.PropertyToID("timingSeedsBuffer");
		protected static readonly int PROP_ID_BLURRED_ATLAS = Shader.PropertyToID("_BlurredAtlas");
		protected static readonly int PROP_ID_FONT_ATLAS = Shader.PropertyToID("_MainTex");


		private const string STR_BLENDING_MODE_MULTIPLY = "_BLENDINGMODE_MULTIPLY";
		private const string STR_BLENDING_MODE_ADDITIVE = "_BLENDINGMODE_ADDITIVE";

		// The LOD of AllIn1Text.shader's plain SubShader, below the effect SubShaders' 100
		private const int PLAIN_SUBSHADER_LOD = 50;

		public Material lastMaterial;
		public bool needsRefreshFontReferences; //When true, it's necessary to refresh the blurred atlas reference (and maybe other fields)
		public virtual Material TextMaterial => textComponent != null ? textComponent.fontSharedMaterial : null;
		public TMP_Text textComponent;

		public TextMeshHandler textMeshHandler;

		public TextLayeredComponent textLayeredComponent;

		// Which live text owns each material at runtime: the per text data lives on the material, and UGUI also batches texts that share one into a single draw
		private static readonly Dictionary<Material, TextMaterialConfigurator> materialOwners = new Dictionary<Material, TextMaterialConfigurator>();

		private Material claimedMaterial;
		private Material ownedMaterialInstance;
		private Material materialBeforeInstance;

		public abstract void InitMeshHandler();

		protected virtual void SetTextMaterial(Material material)
		{
			textComponent.fontSharedMaterial = material;
		}

		protected virtual CanvasRenderer GetCanvasRenderer()
		{
			CanvasRenderer res = textComponent is TextMeshProUGUI uiText ? uiText.canvasRenderer : null;
			return res;
		}

		// Play mode only, and never from inside TMP's rebuild: switching the material there would re register the text mid rebuild loop
		public void ClaimMaterial()
		{
			if (!Application.isPlaying) { return; }

			Material mat = TextMaterial;
			if (mat == null || mat == claimedMaterial) { return; }

			ReleaseMaterialClaim();

			if (materialOwners.TryGetValue(mat, out TextMaterialConfigurator owner) && owner != null && owner != this)
			{
				// Another text was given this very instance, so it is no longer ours to destroy
				if (ownedMaterialInstance == mat)
				{
					ownedMaterialInstance = null;
				}

				DestroyOwnedMaterialInstance();

				materialBeforeInstance = mat;
				ownedMaterialInstance = new Material(mat);
				ownedMaterialInstance.name = mat.name + " (Instance)";
				SetTextMaterial(ownedMaterialInstance);

				// A copy of the same material is no font change, while a pending one still reaches the master's font change handling
				if (lastMaterial == mat)
				{
					lastMaterial = ownedMaterialInstance;
				}

				mat = ownedMaterialInstance;
			}

			materialOwners[mat] = this;
			claimedMaterial = mat;
		}

		protected void ReleaseMaterialClaim()
		{
			if (claimedMaterial != null && materialOwners.TryGetValue(claimedMaterial, out TextMaterialConfigurator owner) && owner == this)
			{
				materialOwners.Remove(claimedMaterial);
			}

			claimedMaterial = null;
		}

		protected void DestroyOwnedMaterialInstance()
		{
			if (ownedMaterialInstance == null) { return; }

			// Only this component is going away: the text goes back to the material it had
			if (TextMaterial == ownedMaterialInstance)
			{
				SetTextMaterial(materialBeforeInstance);
			}

			Destroy(ownedMaterialInstance);
			ownedMaterialInstance = null;
			materialBeforeInstance = null;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetMaterialOwners()
		{
			materialOwners.Clear();
		}

		// The effects read StructuredBuffers in the vertex and fragment stages, which WebGL, GLES 3.0 and some GLES 3.1 GPUs can't
		public static bool AreBuffersSupported()
		{
			return SystemInfo.supportsComputeShaders && SystemInfo.maxComputeBufferInputsVertex >= 2 && SystemInfo.maxComputeBufferInputsFragment >= 2;
		}

		// #pragma target 4.5 already skips the effect SubShaders where compute is missing; this covers GPUs that have compute but no vertex stage buffers
		protected void UsePlainSubShader()
		{
			if (TextMaterial != null)
			{
				TextMaterial.shader.maximumLOD = PLAIN_SUBSHADER_LOD;
			}
		}

		public abstract void ApplyTimingSeedsBuffer();


		public void WriteMeshChanges()
		{
			textMeshHandler.WriteChanges(); 
		}

		private void SetBlendSrc(float value)
		{
			TextMaterial.SetFloat("_BlendSrc", value);
		}

		private void SetBlendDst(float value)
		{
			TextMaterial.SetFloat("_BlendDst", value);
		}

		public void SetBlendingAdditive()
		{
			SetBlendSrc((float)BlendMode.One);
			SetBlendDst((float)BlendMode.One);

			TextMaterial.SetFloat(PROP_ID_BLENDING_MODE, 2f);
			TextMaterial.DisableKeyword(STR_BLENDING_MODE_MULTIPLY);
			TextMaterial.EnableKeyword(STR_BLENDING_MODE_ADDITIVE);
		}

		public void SetBlendingMultiply()
		{
			SetBlendSrc((float)BlendMode.DstColor);
			SetBlendDst((float)BlendMode.Zero);

			TextMaterial.SetFloat(PROP_ID_BLENDING_MODE, 1f);
			TextMaterial.DisableKeyword(STR_BLENDING_MODE_ADDITIVE);
			TextMaterial.EnableKeyword(STR_BLENDING_MODE_MULTIPLY);
		}

		// The shader writes premultiplied colour, so plain blending is a new material's One, OneMinusSrcAlpha, with no keyword left from Additive or Multiply
		public void SetBlendingNoBlend()
		{
			SetBlendSrc((float)BlendMode.One);
			SetBlendDst((float)BlendMode.OneMinusSrcAlpha);

			TextMaterial.SetFloat(PROP_ID_BLENDING_MODE, 0f);
			TextMaterial.DisableKeyword(STR_BLENDING_MODE_MULTIPLY);
			TextMaterial.DisableKeyword(STR_BLENDING_MODE_ADDITIVE);
		}

		public virtual void SetFontAtlas(Texture mainAtlasTex)
		{
			TextMaterial.SetTexture(PROP_ID_FONT_ATLAS, mainAtlasTex);
		}

		public virtual void SetBlurredAtlas(Texture blurredAtlasTex)
		{
			TextMaterial.SetTexture(PROP_ID_BLURRED_ATLAS, blurredAtlasTex);
		}

		// Under a stencil Mask, UGUI renders a copy it made once from the material (StencilMaterial), which the per text data has to reach too
		protected Material GetMaskCopyMaterial()
		{
			Material res = null;

			CanvasRenderer canvasRenderer = GetCanvasRenderer();
			if (canvasRenderer != null && canvasRenderer.materialCount > 0)
			{
				Material renderMaterial = canvasRenderer.GetMaterial(0);
				if (renderMaterial != null && renderMaterial != TextMaterial)
				{
					res = renderMaterial;
				}
			}

			return res;
		}

		protected void SetTimingSeedsBuffer(ComputeBuffer timingSeedsBuffer)
		{
			TextMaterial.SetBuffer(PROP_ID_TIMING_SEEDS_BUFFER, timingSeedsBuffer);

			Material maskCopy = GetMaskCopyMaterial();
			if (maskCopy != null)
			{
				maskCopy.SetBuffer(PROP_ID_TIMING_SEEDS_BUFFER, timingSeedsBuffer);
			}
		}
	}
}