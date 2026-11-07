using System.Collections.Generic;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class TextLayeredComponent : MonoBehaviour
	{
		public TextLayerMaster masterLayer;
		public List<TextLayerSlave> slaveLayers;

		public TextMaterialConfiguratorMaster materialConfigurator;

#if UNITY_EDITOR
		public bool showLayersHierarchy;
#endif

		public void SetText(string text)
		{
			masterLayer.SetText(text);
		}

		public void RemoveLayer(int index)
		{
			if (slaveLayers[index].materialConfigurator != null)
			{
				DestroyImmediate(slaveLayers[index].materialConfigurator.gameObject);
			}

			slaveLayers.RemoveAt(index);

			InitMeshHandlers();
			ApplyAllLayers();
		}

		public void ApplyAllLayers()
		{
			masterLayer.Apply();
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				slaveLayers[i].Apply();
			}

#if UNITY_EDITOR
			if (showLayersHierarchy)
			{
				ShowLayersHierarchy();
			}
			else
			{
				HideLayersHierarchy();
			}
#endif
		}

		/*Only called from TextConfigurationMaster*/
		public void UpdateMaterials()
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].UpdateMaterial();
				}
			}
		}

		/*Only called from TextConfigurationMaster, outside the canvas rebuild*/
		public void RefreshLayerGraphics()
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].RefreshLayerGraphic();
				}
			}
		}

		public void InitMeshHandlers()
		{
			RenumberLayers();

			TextMeshHandler textMeshHandler = TextMeshHandlerRegistry.GetTextMeshHandler(gameObject.GetInstanceID());
			if (textMeshHandler != null)
			{
				textMeshHandler.ClearLayers();
			}

			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].InitMeshHandler();
				}
			}
		}

		// A layer's index is its place in the list: its handler slot, its depth and where its custom timing seed setting is read
		private void RenumberLayers()
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				TextLayerSlave slaveLayer = slaveLayers[i];
				if (slaveLayer.index != i || (slaveLayer.IsValid && slaveLayer.materialConfiguratorSlave.index != i))
				{
					slaveLayer.SetIndex(i);
				}
			}
		}

		public void ApplyTimingSeedsBuffers()
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].ApplyTimingSeedsBuffer();
				}
			}
		}

		/*Only called from TextConfigurationMaster*/
		public void ClaimMaterials()
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].materialConfigurator.ClaimMaterial();
				}
			}
		}

		public void RefreshBuffers(int numBufferElements)
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].RefreshBuffers(numBufferElements);
				}
			}
		}

		public void ReleaseBuffers()
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].ReleaseBuffers();
				}
			}
		}

		public void RefreshBuffersDataInLayers(bool updateTimingSeed)
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].RefreshBuffersData(updateTimingSeed);
				}
			}
		}

		public void SetBlurredAtlas(Texture blurredAtlasTex)
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].SetBlurredAtlas(blurredAtlasTex);
				}
			}
		}

		public void SetFontAtlas(Texture mainAtlasTex)
		{
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].SetFontAtlas(mainAtlasTex);
				}
			}
		}

#if UNITY_EDITOR
		public TextLayerSlave AddSlaveLayer(TextMaterialConfiguratorSlave materialConfiguratorSlave, Vector2 offset)
		{
			int layerIndex = slaveLayers.Count;
			string layerName = $"Layer_{layerIndex + 1}";
			TextLayerSlave slaveLayer = new TextLayerSlave(layerName, offset, layerIndex, materialConfiguratorSlave);

			slaveLayers.Add(slaveLayer);

			materialConfiguratorSlave.Init(this, materialConfigurator, layerIndex);

			return slaveLayer;
		}

		public void ShowLayersHierarchy()
		{
			SetLayersHideFlags(HideFlags.None);
		}

		public void HideLayersHierarchy()
		{
			SetLayersHideFlags(HideFlags.NotEditable | HideFlags.HideInHierarchy);
		}

		private void SetLayersHideFlags(HideFlags hideFlags)
		{
			gameObject.hideFlags = HideFlags.None;
			for (int i = 0; i < slaveLayers.Count; i++)
			{
				if (slaveLayers[i].IsValid)
				{
					slaveLayers[i].materialConfigurator.gameObject.hideFlags = hideFlags;
				}
			}
		}

		private void Reset()
		{
			slaveLayers = new List<TextLayerSlave>();

			CheckReferences();

			masterLayer = new TextLayerMaster("MainLayer", Vector2.zero, 0, materialConfigurator);
			masterLayer.materialConfigurator.TextMaterial.name = AllIn1TextConstants.PREFIX_MAT_TEXT_LAYER + masterLayer.materialConfigurator.TextMaterial.name;

			materialConfigurator.textLayeredComponent = this;

			UpdateLayers();
		}

		public void CheckReferences()
		{
			this.materialConfigurator = GetComponent<TextMaterialConfiguratorMaster>();
		}

		public void UpdateLayers()
		{
			masterLayer.materialConfiguratorMaster.UpdateConfiguration(true);
		}
#endif
	}
}
