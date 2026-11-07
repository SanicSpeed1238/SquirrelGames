using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//Behind a cover, shows every showcase whole for a few frames with its post process and cameras, so its shaders, render states and buffers exist before the carousel starts
	public class TextShaderDemoPrewarmer
	{
		private const int FRAMES_PER_SHOWCASE = 2;
		private const int MAX_EDITOR_SWEEPS = 3;
		private const float MAX_COMPILE_SECONDS = 600f;
		private const float COMPILER_IDLE_SECONDS = 0.5f;
		private const float FADE_SECONDS = 0.35f;
		private const int COVER_SORTING_ORDER = 32000;

		private static readonly Color COVER_COLOR = new Color(0.035f, 0.035f, 0.04f, 1f);
		private static readonly Color LABEL_COLOR = new Color(1f, 1f, 1f, 0.7f);

		private readonly TextShaderDemoElementsController controller;
		private readonly GameObject cover;
		private readonly CanvasGroup coverGroup;
		private readonly TMP_Text status;
		private TextShaderDemoElement warming;
		private bool aborted;

		public TextShaderDemoPrewarmer(TextShaderDemoElementsController controller, TMP_FontAsset font)
		{
			this.controller = controller;

			cover = CreateOverlayCanvas("Demo Prewarm Cover", COVER_SORTING_ORDER);
			cover.AddComponent<GraphicRaycaster>();
			coverGroup = cover.AddComponent<CanvasGroup>();

			Image background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer)).AddComponent<Image>();
			background.transform.SetParent(cover.transform, false);
			background.rectTransform.anchorMin = Vector2.zero;
			background.rectTransform.anchorMax = Vector2.one;
			background.rectTransform.sizeDelta = Vector2.zero;
			background.color = COVER_COLOR;

			status = CreateLabel(cover.transform, font, 30f);
			status.text = "Loading effects";
		}

		public IEnumerator Sweep()
		{
			//The cover reaches the screen before anything stalls
			yield return null;
#if UNITY_EDITOR
			yield return CompileUiVariants();
#endif
			if (aborted) { yield break; }
			TextShaderDemoElement[] elements = controller.demoElements;
			int sweeps = Application.isEditor ? MAX_EDITOR_SWEEPS : 1;
			for (int sweep = 0; sweep < sweeps; sweep++)
			{
				bool compiling = false;
				for (int i = 0; i < elements.Length; i++)
				{
					status.text = $"Preparing showcases {i + 1} / {elements.Length}";
					warming = elements[i];
					warming.BeginPrewarm();
					controller.postprocessController.ApplyConfig(warming.GetPostprocessConfig());
					controller.demoSceneConfigurator.ApplyCameraConfiguration(warming.camerasConfiguration);
					for (int f = 0; f < FRAMES_PER_SHOWCASE; f++)
					{
						yield return null;
						if (aborted) { yield break; }
						compiling |= IsEditorCompiling();
					}
					warming.EndPrewarm();
					warming = null;
				}

				//The neutral config the carousel fades through between showcases
				controller.postprocessController.ApplyConfig(PostprocessConfig.DEFAULT);
				yield return null;
				if (aborted) { yield break; }
				compiling |= IsEditorCompiling();

				//Variants only a draw asks for (post processing, masks, 3D sets) compiled in the background and drew as placeholders: wait, then draw them for real
				if (!compiling) { break; }
#if UNITY_EDITOR
				yield return WaitForCompiler(null);
#endif
				if (aborted) { yield break; }
			}
		}

		public IEnumerator FadeOutCover()
		{
			//Capture tools may remove the cover before it has faded
			if (coverGroup == null) { yield break; }
			coverGroup.blocksRaycasts = false;
			for (float t = 0f; t < FADE_SECONDS; t += Time.unscaledDeltaTime)
			{
				if (coverGroup == null) { yield break; }
				coverGroup.alpha = 1f - t / FADE_SECONDS;
				yield return null;
			}
			Object.Destroy(cover);
		}

		//Unity may keep a nested coroutine running after its parent stops, so every loop here also checks this
		public void Abort()
		{
			aborted = true;
			if (warming != null)
			{
				warming.EndPrewarm();
				warming = null;
			}
			Object.Destroy(cover);
		}

		//Where the GPU has no buffers in vertex shaders the texts use the plain SubShader, so the demo says why its effects are missing
		public static void ShowPlainTextNotice(TMP_FontAsset font)
		{
			GameObject canvas = CreateOverlayCanvas("Demo Plain Text Notice", COVER_SORTING_ORDER - 1);
			TMP_Text label = CreateLabel(canvas.transform, font, 26f);
			RectTransform rect = label.rectTransform;
			rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
			rect.anchoredPosition = new Vector2(0f, -24f);
			//A web build on OpenGLES3 is WebGL 2: the browser had no WebGPU
			bool webGL = Application.platform == RuntimePlatform.WebGLPlayer && SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3;
			label.text = webGL
				? "This browser has no WebGPU, so the text effects are off. Open the demo in a browser with WebGPU to see them"
				: "This GPU can't run the text effects (they need compute buffers in vertex shaders), so the texts show plain";
		}

		private static GameObject CreateOverlayCanvas(string name, int sortingOrder)
		{
			GameObject go = new GameObject(name);
			Canvas canvas = go.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = sortingOrder;
			CanvasScaler scaler = go.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920f, 1080f);
			scaler.matchWidthOrHeight = 1f;
			return go;
		}

		private static TMP_Text CreateLabel(Transform parent, TMP_FontAsset font, float size)
		{
			TextMeshProUGUI label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer)).AddComponent<TextMeshProUGUI>();
			label.transform.SetParent(parent, false);
			label.rectTransform.sizeDelta = new Vector2(1600f, 120f);
			if (font != null) { label.font = font; }
			label.fontSize = size;
			label.color = LABEL_COLOR;
			label.alignment = TextAlignmentOptions.Center;
			label.raycastTarget = false;
			return label;
		}

		private static bool IsEditorCompiling()
		{
#if UNITY_EDITOR
			return UnityEditor.ShaderUtil.anythingCompiling;
#else
			return false;
#endif
		}

#if UNITY_EDITOR
		//Asks for every UI variant in the scene at once, so the compiler builds them in parallel instead of one per first draw
		private IEnumerator CompileUiVariants()
		{
			List<KeyValuePair<Material, int>> passes = CollectUncompiledUiPasses(controller.gameObject.scene);
			if (passes.Count == 0) { yield break; }

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			int requested = 0;
			while (requested < passes.Count)
			{
				if (aborted) { yield break; }
				//Requests return at once where the editor compiles in the background; where it compiles in place they spread over frames, so the count moves
				watch.Restart();
				while (requested < passes.Count && watch.ElapsedMilliseconds < 30)
				{
					UnityEditor.ShaderUtil.CompilePass(passes[requested].Key, passes[requested].Value);
					requested++;
				}
				status.text = $"Compiling shaders {CountCompiled(passes)} / {passes.Count}";
				yield return null;
			}

			yield return WaitForCompiler(passes);
		}

		private IEnumerator WaitForCompiler(List<KeyValuePair<Material, int>> passes)
		{
			float start = Time.realtimeSinceStartup;
			float idle = 0f;
			while (Time.realtimeSinceStartup - start < MAX_COMPILE_SECONDS)
			{
				if (aborted) { yield break; }
				int compiled = passes != null ? CountCompiled(passes) : 0;
				if (passes != null && compiled >= passes.Count) { yield break; }
				//A variant that fails to compile never reports done, so a compiler with nothing left to do ends the wait too
				idle = UnityEditor.ShaderUtil.anythingCompiling ? 0f : idle + Time.unscaledDeltaTime;
				if (idle >= COMPILER_IDLE_SECONDS) { yield break; }
				status.text = passes != null ? $"Compiling shaders {compiled} / {passes.Count}" : "Compiling shaders";
				yield return null;
			}
		}

		private static List<KeyValuePair<Material, int>> CollectUncompiledUiPasses(UnityEngine.SceneManagement.Scene scene)
		{
			List<KeyValuePair<Material, int>> res = new List<KeyValuePair<Material, int>>();
			HashSet<string> variants = new HashSet<string>();
			List<Graphic> graphics = new List<Graphic>();
			foreach (GameObject root in scene.GetRootGameObjects())
			{
				root.GetComponentsInChildren(true, graphics);
				foreach (Graphic graphic in graphics)
				{
					AddUncompiledPasses(UiMaterialOf(graphic), res, variants);
				}
			}
			return res;
		}

		//TMP's material getters create an instance, so its shared materials are read instead
		private static Material UiMaterialOf(Graphic graphic)
		{
			if (graphic is TMP_Text text) { return text.fontSharedMaterial; }
			if (graphic is TMP_SubMeshUI subMesh) { return subMesh.sharedMaterial; }
			return graphic.material;
		}

		//One request per variant: materials that differ only in property values share it
		private static void AddUncompiledPasses(Material material, List<KeyValuePair<Material, int>> res, HashSet<string> variants)
		{
			if (material == null || material.shader == null || !material.shader.isSupported) { return; }
			string[] keywords = material.shaderKeywords;
			System.Array.Sort(keywords, System.StringComparer.Ordinal);
			if (!variants.Add(material.shader.GetInstanceID() + "|" + string.Join(" ", keywords))) { return; }
			for (int pass = 0; pass < material.passCount; pass++)
			{
				if (!UnityEditor.ShaderUtil.IsPassCompiled(material, pass))
				{
					res.Add(new KeyValuePair<Material, int>(material, pass));
				}
			}
		}

		private static int CountCompiled(List<KeyValuePair<Material, int>> passes)
		{
			int res = 0;
			for (int i = 0; i < passes.Count; i++)
			{
				if (UnityEditor.ShaderUtil.IsPassCompiled(passes[i].Key, passes[i].Value)) { res++; }
			}
			return res;
		}
#endif
	}
}
