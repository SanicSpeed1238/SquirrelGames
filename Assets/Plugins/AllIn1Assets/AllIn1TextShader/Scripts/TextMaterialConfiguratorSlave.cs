using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[ExecuteInEditMode]
	public class TextMaterialConfiguratorSlave : TextMaterialConfigurator
	{
		public TextMaterialConfiguratorMaster master;
		public TextLayerGraphic layerGraphic;

		public int index;

		private float[] timingSeeds;
		public ComputeBuffer timingSeedsBuffer;

		public override Material TextMaterial => layerGraphic != null ? layerGraphic.LayerMaterial : null;

		protected override void SetTextMaterial(Material material)
		{
			if (layerGraphic != null)
			{
				layerGraphic.LayerMaterial = material;
			}
		}

		protected override CanvasRenderer GetCanvasRenderer()
		{
			CanvasRenderer res = layerGraphic != null ? layerGraphic.canvasRenderer : null;
			return res;
		}

		public override void InitMeshHandler()
		{
			if (master == null || layerGraphic == null) { return; }

			int instanceID = master.gameObject.GetInstanceID();
			textMeshHandler = TextMeshHandlerRegistry.GetTextMeshHandler(instanceID);
			if (textMeshHandler != null)
			{
				textMeshHandler.SetLayer(index, layerGraphic);
			}
		}

		public void Init(TextLayeredComponent textLayeredComponent, TextMaterialConfiguratorMaster master, int index)
		{
			if (layerGraphic == null)
			{
				layerGraphic = GetComponent<TextLayerGraphic>();
			}

			this.textLayeredComponent = textLayeredComponent;
			this.master = master;
			this.index = index;

			InitMeshHandler();
		}

		public void UpdateMaterial()
		{
			Material mat = TextMaterial;
			if (mat == null || master == null) { return; }

			master.ApplyChangesToMaterial(mat);

			Material maskCopy = GetMaskCopyMaterial();
			if (maskCopy != null)
			{
				master.ApplyChangesToMaterial(maskCopy);
			}
		}

		public void RefreshLayerGraphic()
		{
			if (layerGraphic != null)
			{
				layerGraphic.RefreshLayer();
			}
		}

		public override void SetFontAtlas(Texture mainAtlasTex)
		{
			if (TextMaterial == null) { return; }

			base.SetFontAtlas(mainAtlasTex);

			// The canvas takes the atlas from the graphic, which reads it off the material only when the material is dirty
			layerGraphic.SetMaterialDirty();
		}

		public override void SetBlurredAtlas(Texture blurredAtlasTex)
		{
			if (TextMaterial == null) { return; }

			base.SetBlurredAtlas(blurredAtlasTex);
		}

		public override void ApplyTimingSeedsBuffer()
		{
			if (TextMaterial == null) { return; }

			SetTimingSeedsBuffer(timingSeedsBuffer);
		}

		private void OnDestroy()
		{
			ReleaseBuffers();
			ReleaseMaterialClaim();
			DestroyOwnedMaterialInstance();
		}

		public void RefreshBuffers(int numBufferElements)
		{
			ReleaseBuffers();

			timingSeeds = new float[numBufferElements];
			timingSeedsBuffer = new ComputeBuffer(numBufferElements, sizeof(float));
		}

		public void ReleaseBuffers()
		{
			if(timingSeedsBuffer != null)
			{
				timingSeedsBuffer.Release();
				timingSeedsBuffer = null;
			}
		}

		public void RefreshBuffersData(bool updateTimingSeed)
		{
			if (timingSeedsBuffer == null || !timingSeedsBuffer.IsValid() || master == null) { return; }

			bool isCustomTimingSeed = IsCustomTimingSeed();
			if (isCustomTimingSeed)
			{
				if (!updateTimingSeed) { return; }

				for (int i = 0; i < timingSeeds.Length; i += 4)
				{
					float timingSeed = UnityEngine.Random.Range(0f, 100f);
					timingSeeds[i] = timingSeed;
					timingSeeds[i + 1] = timingSeed;
					timingSeeds[i + 2] = timingSeed;
					timingSeeds[i + 3] = timingSeed;
				}
			}
			else
			{
				// The master rolls seeds for new slices and reused slots on any refresh, not only when it reallocates
				for (int i = 0; i < timingSeeds.Length; i++)
				{
					timingSeeds[i] = master.GetTimingSeedByVertexIndex(i);
				}
			}

			timingSeedsBuffer.SetData(timingSeeds);
		}

		private bool IsCustomTimingSeed()
		{
			bool res = textLayeredComponent != null && index >= 0 && index < textLayeredComponent.slaveLayers.Count && textLayeredComponent.slaveLayers[index].customTimingSeed;
			return res;
		}
	}
}
