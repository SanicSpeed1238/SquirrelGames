using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//One full set of backdrop layers (gradient, two lights, pattern, word rows, dust, vignette) under a CanvasGroup; DemoBackdrop keeps two and dissolves
	//between them. Sizes are in screen heights, light positions are offsets from the stage centre (the middle of the space DemoFit leaves above the UI row)
	public class DemoBackdropLayers : MonoBehaviour
	{
		public Image gradient;
		public Image pattern;
		public RawImage lightA;
		public RawImage lightB;
		public RectTransform words;
		public RectTransform dust;
		public RawImage vignette;
		public Texture dustTexture;
		public CanvasGroup group;
		//The shipped checker, for looks without a pattern of their own (the pattern material holds the last applied look's texture)
		public Texture defaultPattern;

		private const int MAX_DUST = 120;

		private Material gradientMat;
		private Material patternMat;
		private DemoBackdrop.Look current;
		private readonly List<TextMeshProUGUI> rows = new List<TextMeshProUGUI>();
		private readonly List<float> rowShift = new List<float>();
		private float unitWidth;
		private readonly List<RawImage> motes = new List<RawImage>();
		private Vector2[] motePos;
		private Vector3[] moteData;
		private float[] moteAlpha;
		private float stageY;
		private float stageWithUi;
		private readonly Dictionary<TMP_FontAsset, Material> outlineMats = new Dictionary<TMP_FontAsset, Material>();

		private RectTransform Rect => (RectTransform)transform;

		//In edit mode a look writes into the shared materials, so the scene shows it before Play; in play mode each set gets copies
		private void Awake()
		{
			if (!Application.isPlaying)
			{
				gradientMat = gradient.material;
				patternMat = pattern.material;
				return;
			}
			gradientMat = new Material(gradient.material);
			gradient.material = gradientMat;
			patternMat = new Material(pattern.material);
			pattern.material = patternMat;
		}

		//The stage centre in this canvas with the UI shown: DemoFit centres showcases between a 19.2 unit top margin and a 96 unit bottom row of a 960 wide
		//canvas. Hide UI (DemoUiHidden.Focus) brings it down to the centre
		private float StageY()
		{
			TextShaderDemoElementsController ctrl = FindAnyObjectByType<TextShaderDemoElementsController>(FindObjectsInactive.Include);
			float h = ctrl != null ? ((RectTransform)ctrl.GetComponentInParent<Canvas>(true).rootCanvas.transform).rect.height : 540f;
			return (96f - 19.2f) * 0.5f / h * Rect.rect.height;
		}

		public void Apply(DemoBackdrop.Look l)
		{
			if (gradientMat == null) { Awake(); }
			current = l;
			float h = Rect.rect.height;
			float w = Rect.rect.width;
			stageWithUi = StageY();

			gradientMat.SetColor("_TopColor", l.top);
			gradientMat.SetColor("_BottomColor", l.bottom);
			gradientMat.SetFloat("_Exponent", l.exponent);

			//A turned pattern grows until it covers the screen's corners again and tiles that much more, so its texels keep their size
			float cover = PatternCover(l.patternAngle, w, h);
			pattern.rectTransform.localEulerAngles = new Vector3(0f, 0f, l.patternAngle);
			pattern.rectTransform.localScale = new Vector3(cover, cover, 1f);
			patternMat.SetTexture("_MainTex", l.pattern != null ? l.pattern : defaultPattern);
			patternMat.SetTextureScale("_MainTex", Vector2.one * l.patternTiles * cover);
			patternMat.SetColor("_Color", l.patternColor);
			patternMat.SetFloat("_ScrollXSpeed", l.patternScroll.x);
			patternMat.SetFloat("_ScrollYSpeed", l.patternScroll.y);
			pattern.color = Color.white;
			pattern.enabled = l.patternColor.a > 0f;

			PlaceAroundStage(stageWithUi * (1f - DemoUiHidden.Focus), w, h);

			BuildWords(l, w, h);
			BuildDust(l, w, h);
			gradient.SetMaterialDirty();
			pattern.SetMaterialDirty();
		}

		private static float PatternCover(float degrees, float w, float h)
		{
			if (Mathf.Approximately(degrees, 0f) || w <= 0f || h <= 0f) { return 1f; }
			float c = Mathf.Abs(Mathf.Cos(degrees * Mathf.Deg2Rad)), s = Mathf.Abs(Mathf.Sin(degrees * Mathf.Deg2Rad));
			return Mathf.Max((w * c + h * s) / w, (w * s + h * c) / h);
		}

		private void PlaceAroundStage(float stage, float w, float h)
		{
			stageY = stage;
			Place(lightA, current.lightA, w, h, stage);
			Place(lightB, current.lightB, w, h, stage);

			//Anchored to the screen (1.5 widths by 1.9 heights around the stage), so its dark rim fits any resolution without a reapply
			vignette.enabled = current.vignette > 0f;
			vignette.color = new Color(1f, 1f, 1f, current.vignette);
			float stageFrac = 0.5f + stage / h;
			vignette.rectTransform.anchorMin = new Vector2(-0.25f, stageFrac - 0.95f);
			vignette.rectTransform.anchorMax = new Vector2(1.25f, stageFrac + 0.95f);
			vignette.rectTransform.sizeDelta = Vector2.zero;
			vignette.rectTransform.anchoredPosition = Vector2.zero;
		}

		private static void Place(RawImage img, DemoBackdrop.Glow g, float w, float h, float stage)
		{
			img.enabled = g.texture != null && g.color.a > 0f;
			img.texture = g.texture;
			img.color = g.color;
			RectTransform rt = img.rectTransform;
			if (g.pinToBottom)
			{
				//Anchored to the screen, a few pixels past its sides and bottom edge, so no seam shows at any resolution or aspect
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = new Vector2(1f, g.size.y);
				rt.pivot = new Vector2(0.5f, 0f);
				rt.sizeDelta = new Vector2(8f, 2f);
				rt.anchoredPosition = new Vector2(0f, -1f);
				return;
			}
			rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
			rt.sizeDelta = g.size * h;
			rt.anchoredPosition = new Vector2(0f, stage) + g.offset * h;
		}

		private void BuildWords(DemoBackdrop.Look l, float w, float h)
		{
			//Rows are built at runtime only: built in edit mode they would be saved into the scene and doubled at Play
			bool on = Application.isPlaying && !string.IsNullOrEmpty(l.words) && l.wordsFont != null && l.wordsColor.a > 0f;
			words.gameObject.SetActive(on);
			if (!on) { return; }
			words.localRotation = Quaternion.Euler(0f, 0f, l.wordsAngle);
			float size = l.wordsSize * h;
			float pitch = size * 1.12f;
			float span = Mathf.Sqrt(w * w + h * h);
			int count = Mathf.CeilToInt(span / pitch) + 2;
			Material mat = WordsMaterial(l.wordsFont, l.wordsOutline, l.wordsColor);
			string unit = l.words + "   ";
			while (rows.Count < count)
			{
				GameObject go = new GameObject("Row" + rows.Count, typeof(RectTransform));
				go.layer = gameObject.layer;
				go.transform.SetParent(words, false);
				TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
#if UNITY_6000_0_OR_NEWER
				t.textWrappingMode = TextWrappingModes.NoWrap;
#else
				t.enableWordWrapping = false;
#endif
				t.overflowMode = TextOverflowModes.Overflow;
				t.alignment = TextAlignmentOptions.Left;
				t.raycastTarget = false;
				rows.Add(t);
				rowShift.Add(0f);
			}
			TextMeshProUGUI probe = rows[0];
			probe.font = l.wordsFont;
			probe.fontSize = size;
			unitWidth = probe.GetPreferredValues(unit + unit + l.words).x - probe.GetPreferredValues(unit + l.words).x;
			int repeats = Mathf.CeilToInt(span * 1.2f / Mathf.Max(1f, unitWidth)) + 2;
			string line = string.Concat(System.Linq.Enumerable.Repeat(unit, repeats));
			for (int i = 0; i < rows.Count; i++)
			{
				TextMeshProUGUI t = rows[i];
				bool used = i < count;
				t.gameObject.SetActive(used);
				if (!used) { continue; }
				t.font = l.wordsFont;
				t.fontSharedMaterial = mat;
				t.fontSize = size;
				t.color = l.wordsOutline ? new Color(1f, 1f, 1f, l.wordsColor.a) : l.wordsColor;
				t.text = line;
				t.rectTransform.pivot = new Vector2(0f, 0.5f);
				t.rectTransform.sizeDelta = new Vector2(unitWidth * repeats, size * 1.3f);
				rowShift[i] = -Mathf.Repeat(i * 0.37f, 1f) * unitWidth;
				t.rectTransform.anchoredPosition = new Vector2(-span * 0.6f + rowShift[i], (i - (count - 1) * 0.5f) * pitch);
				t.ForceMeshUpdate();
			}
		}

		private Material WordsMaterial(TMP_FontAsset font, bool outline, Color color)
		{
			if (!outline) { return font.material; }
			Material m;
			if (!outlineMats.TryGetValue(font, out m))
			{
				m = new Material(font.material);
				if (!m.shader.name.StartsWith("TextMeshPro")) { m.shader = Shader.Find("TextMeshPro/Distance Field"); }
				m.name = font.name + " Backdrop Outline";
				outlineMats[font] = m;
			}
			m.SetColor(ShaderUtilities.ID_FaceColor, new Color(1f, 1f, 1f, 0f));
			m.SetColor(ShaderUtilities.ID_OutlineColor, new Color(color.r, color.g, color.b, 1f));
			m.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.14f);
			m.EnableKeyword("OUTLINE_ON");
			ShaderUtilities.UpdateShaderRatios(m);
			return m;
		}

		private void BuildDust(DemoBackdrop.Look l, float w, float h)
		{
			int count = Application.isPlaying && l.dustColor.a > 0f ? Mathf.Min(l.dustCount, MAX_DUST) : 0;
			dust.gameObject.SetActive(count > 0);
			if (count == 0) { return; }
			while (motes.Count < count)
			{
				GameObject go = new GameObject("Mote" + motes.Count, typeof(RectTransform));
				go.layer = gameObject.layer;
				go.transform.SetParent(dust, false);
				RawImage img = go.AddComponent<RawImage>();
				img.raycastTarget = false;
				motes.Add(img);
			}
			Random.State keep = Random.state;
			Random.InitState(1234);
			motePos = new Vector2[count];
			moteData = new Vector3[count];
			moteAlpha = new float[count];
			bool lit = l.dustLight != Vector2.zero;
			float spreadX = lit ? Mathf.Min(0.5f, 1.3f * l.dustLight.x * h / w) : 0.5f;
			for (int i = 0; i < motes.Count; i++)
			{
				RawImage img = motes[i];
				img.gameObject.SetActive(i < count);
				if (i >= count) { continue; }
				float s = Random.Range(0.004f, 0.012f) * (lit ? 1.4f : 1f);
				img.texture = dustTexture;
				img.rectTransform.sizeDelta = Vector2.one * s * h;
				Color c = l.dustColor;
				c.a *= Random.Range(0.5f, 1f);
				img.color = c;
				moteAlpha[i] = c.a;
				motePos[i] = new Vector2(Random.Range(-spreadX, spreadX), Random.Range(-0.5f, 0.5f));
				//Rise speed in screen heights per second, sway phase, sway amount
				moteData[i] = new Vector3(Random.Range(0.008f, 0.03f), Random.Range(0f, 6.28f), Random.Range(0.005f, 0.02f));
				PlaceMote(i, w, h, 0f);
			}
			Random.state = keep;
		}

		private void PlaceMote(int i, float w, float h, float t)
		{
			Vector2 p = motePos[i];
			Vector3 d = moteData[i];
			Vector2 pos = new Vector2((p.x + Mathf.Sin(t * 0.6f + d.y) * d.z) * w, p.y * h);
			motes[i].rectTransform.anchoredPosition = pos;
			Vector2 light = current != null ? current.dustLight : Vector2.zero;
			if (light != Vector2.zero)
			{
				float dx = pos.x / (light.x * h);
				float dy = (pos.y - stageY) / (light.y * h);
				Color c = motes[i].color;
				c.a = moteAlpha[i] * Mathf.Exp(-(dx * dx + dy * dy));
				motes[i].color = c;
			}
		}

		private void Update()
		{
			if (current == null) { return; }
			float h = Rect.rect.height;
			float w = Rect.rect.width;
			float stage = stageWithUi * (1f - DemoUiHidden.Focus);
			if (stage != stageY) { PlaceAroundStage(stage, w, h); }
			if (words.gameObject.activeSelf && unitWidth > 0f)
			{
				float span = Mathf.Sqrt(w * w + h * h);
				for (int i = 0; i < rows.Count; i++)
				{
					if (!rows[i].gameObject.activeSelf) { continue; }
					float dir = i % 2 == 0 ? 1f : -1f;
					rowShift[i] = Mathf.Repeat(rowShift[i] + dir * current.wordsSpeed * h * Time.deltaTime, unitWidth) - unitWidth;
					Vector2 pos = rows[i].rectTransform.anchoredPosition;
					rows[i].rectTransform.anchoredPosition = new Vector2(-span * 0.6f + rowShift[i], pos.y);
				}
			}
			if (dust.gameObject.activeSelf && motePos != null)
			{
				for (int i = 0; i < motePos.Length; i++)
				{
					motePos[i].y += moteData[i].x * Time.deltaTime;
					if (motePos[i].y > 0.55f) { motePos[i].y -= 1.1f; }
					PlaceMote(i, w, h, Time.time);
				}
			}
		}
	}
}
