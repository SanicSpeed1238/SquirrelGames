using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn13DShader
{
	//Behind a loading cover, shows every demo whole for a few frames with its environment, light and sky, so its shaders exist before the carousel starts
	public class AllIn13DShaderDemoPrewarmer
	{
		private const int FRAMES_PER_DEMO = 2;
		private const int MAX_EDITOR_SWEEPS = 3;
		private const float MAX_COMPILE_SECONDS = 600f;
		private const float COMPILER_IDLE_SECONDS = 0.5f;
		private const float FADE_SECONDS = 0.35f;
		private const int COVER_SORTING_ORDER = 32000;
		private const float BAR_WIDTH = 640f;
		private const float BAR_HEIGHT = 6f;

		private static readonly Color COVER_COLOR = new Color(0.137f, 0.122f, 0.125f, 1f);
		private static readonly Color LABEL_COLOR = new Color(1f, 1f, 1f, 0.7f);
		private static readonly Color BAR_BACK_COLOR = new Color(1f, 1f, 1f, 0.12f);

		private readonly AllIn13dShaderDemoController controller;
		private readonly GameObject cover;
		private readonly CanvasGroup coverGroup;
		private readonly TMP_Text status;
		private readonly RectTransform barFill;

		public AllIn13DShaderDemoPrewarmer(AllIn13dShaderDemoController controller, TMP_FontAsset font)
		{
			this.controller = controller;

			cover = new GameObject("Demo Prewarm Cover");
			Canvas canvas = cover.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = COVER_SORTING_ORDER;
			CanvasScaler scaler = cover.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920f, 1080f);
			scaler.matchWidthOrHeight = 1f;
			cover.AddComponent<GraphicRaycaster>();
			coverGroup = cover.AddComponent<CanvasGroup>();

			Image background = CreateImage("Background", cover.transform, COVER_COLOR);
			background.rectTransform.anchorMin = Vector2.zero;
			background.rectTransform.anchorMax = Vector2.one;
			background.rectTransform.sizeDelta = Vector2.zero;

			status = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer)).AddComponent<TextMeshProUGUI>();
			status.transform.SetParent(cover.transform, false);
			status.rectTransform.sizeDelta = new Vector2(1600f, 80f);
			status.rectTransform.anchoredPosition = new Vector2(0f, 30f);
			if (font != null) { status.font = font; }
			status.fontSize = 30f;
			status.color = LABEL_COLOR;
			status.alignment = TextAlignmentOptions.Center;
			status.raycastTarget = false;
			status.text = "Loading demos";

			Image barBack = CreateImage("Loading Bar", cover.transform, BAR_BACK_COLOR);
			barBack.rectTransform.sizeDelta = new Vector2(BAR_WIDTH, BAR_HEIGHT);
			barBack.rectTransform.anchoredPosition = new Vector2(0f, -20f);

			barFill = CreateImage("Fill", barBack.transform, LABEL_COLOR).rectTransform;
			barFill.anchorMin = Vector2.zero;
			barFill.pivot = new Vector2(0f, 0.5f);
			barFill.sizeDelta = Vector2.zero;
			SetProgress(0f);

#if UNITY_EDITOR
			//The sweep floods the background compiler, and a cover queued behind it would show as a cyan placeholder
			CompileNow(background.materialForRendering);
			CompileNow(status.fontSharedMaterial);
#endif
		}

		public IEnumerator Sweep()
		{
			//The cover reaches the screen before anything stalls
			yield return null;
			List<DemoElement> elements = controller.GetAllDemoElements();
			int sweeps = Application.isEditor ? MAX_EDITOR_SWEEPS : 1;
			float progressFrom = 0f;
			controller.BeginPrewarm();
			for (int sweep = 0; sweep < sweeps; sweep++)
			{
				//In the editor a sweep mostly queues compiles, so each fills half of what is left of the bar
				float progressTo = sweep < sweeps - 1 ? Mathf.Lerp(progressFrom, 1f, 0.5f) : 1f;
				bool compiling = false;
				for (int i = 0; i < elements.Count; i++)
				{
					status.text = $"Preparing demos {i + 1} / {elements.Count}";
					SetProgress(Mathf.Lerp(progressFrom, progressTo, (i + 1f) / elements.Count));
					controller.ShowForPrewarm(elements[i]);
					for (int f = 0; f < FRAMES_PER_DEMO; f++)
					{
						yield return null;
						compiling |= IsEditorCompiling();
					}
				}

				List<DemoEnvironment> environments = controller.GetPrewarmEnvironments();
				for (int i = 0; i < environments.Count; i++)
				{
					controller.ShowEnvironmentFadeForPrewarm(environments[i]);
					for (int f = 0; f < FRAMES_PER_DEMO; f++)
					{
						yield return null;
						compiling |= IsEditorCompiling();
					}
				}
				progressFrom = progressTo;

				//Variants still compiling in the background drew as placeholders: wait, then draw them for real
				if (!compiling) { break; }
#if UNITY_EDITOR
				yield return WaitForCompiler();
#endif
			}
			SetProgress(1f);
			controller.EndPrewarm();
		}

		public IEnumerator FadeOutCover()
		{
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

		private void SetProgress(float progress)
		{
			barFill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
		}

		private static Image CreateImage(string name, Transform parent, Color color)
		{
			Image res = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer)).AddComponent<Image>();
			res.transform.SetParent(parent, false);
			res.color = color;
			res.raycastTarget = parent.GetComponent<Canvas>() != null;
			return res;
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
		private static void CompileNow(Material material)
		{
			if (material == null || material.shader == null) { return; }
			for (int pass = 0; pass < material.passCount; pass++)
			{
				UnityEditor.ShaderUtil.CompilePass(material, pass, true);
			}
		}

		//Draws queue their compiles in the background, in parallel; ShaderUtil.CompilePass compiles one pass at a time in place, many times slower here
		private IEnumerator WaitForCompiler()
		{
			float start = Time.realtimeSinceStartup;
			float idle = 0f;
			while (Time.realtimeSinceStartup - start < MAX_COMPILE_SECONDS)
			{
				idle = UnityEditor.ShaderUtil.anythingCompiling ? 0f : idle + Time.unscaledDeltaTime;
				if (idle >= COMPILER_IDLE_SECONDS) { yield break; }
				status.text = $"Compiling shaders, {Time.realtimeSinceStartup - start:0} s";
				yield return null;
			}
		}
#endif
	}
}
