using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//Hidden, unsaved stand in that draws an AllIn1TextParticles' mesh right before or after its text, for when the emitter's own place in the hierarchy is on the other side
	[ExecuteAlways]
	[AddComponentMenu("")]
	[RequireComponent(typeof(CanvasRenderer))]
	public class TextParticlesDrawProxy : MaskableGraphic
	{
		[System.NonSerialized] public AllIn1TextParticles owner;

		public static TextParticlesDrawProxy Create(AllIn1TextParticles owner)
		{
			GameObject go = new GameObject(owner.name + " (draw)", typeof(RectTransform), typeof(CanvasRenderer));
			go.hideFlags = HideFlags.HideAndDontSave;
			go.layer = owner.gameObject.layer;
			go.SetActive(false);
			TextParticlesDrawProxy res = go.AddComponent<TextParticlesDrawProxy>();
			res.owner = owner;
			res.raycastTarget = false;
			go.AddComponent<LayoutElement>().ignoreLayout = true;
			return res;
		}

		//Sibling just before the text (behind) or just after it and its children (in front)
		public void Place(Transform text, bool behind)
		{
			Transform parent = text.parent;
			if (transform.parent != parent)
			{
				transform.SetParent(parent, false);
				RectTransform rt = rectTransform;
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = Vector2.one;
				rt.sizeDelta = Vector2.zero;
				rt.anchoredPosition = Vector2.zero;
			}

			int textIndex = text.GetSiblingIndex();
			int current = transform.GetSiblingIndex();
			if (behind && current != textIndex - 1) { transform.SetSiblingIndex(current < textIndex ? textIndex - 1 : textIndex); }
			else if (!behind && current != textIndex + 1) { transform.SetSiblingIndex(current < textIndex ? textIndex : textIndex + 1); }

			if (!gameObject.activeSelf) { gameObject.SetActive(true); }
		}

		public override Texture mainTexture => owner != null ? owner.mainTexture : s_WhiteTexture;

		public override Material defaultMaterial => owner != null ? owner.defaultMaterial : base.defaultMaterial;

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			if (owner != null) { owner.PopulateParticles(vh, transform.worldToLocalMatrix * owner.transform.localToWorldMatrix); }
		}

		//An owner lost to a domain reload or a delete leaves nothing behind
		private void Update()
		{
			if (owner != null) { return; }
			if (Application.isPlaying) { Destroy(gameObject); }
			else { DestroyImmediate(gameObject); }
		}
	}
}
