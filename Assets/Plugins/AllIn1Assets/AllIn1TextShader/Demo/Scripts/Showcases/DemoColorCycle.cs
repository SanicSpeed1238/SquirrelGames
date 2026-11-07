using System;
using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//Walks a text's material through a loop of looks: each stage eases its colour filters (Hue Shift, Greyscale, Posterize, Contrast and Brightness,
	//Color Grading) and its light (Bevel and Extrude angle, elevation, colour) in, then holds. Ambient graphics take the stage's colour, and while the
	//showcase is shown a caption typewriter retypes the stage's name. Writes the configurator's material, as the demo's tweens do.
	//An optional Custom warp takes each stage's own shape, a 4x4 cage (baked from a preset style when the showcase is built), and morphs into the next
	//stage's as the colours ease: the cage is linear in its points, so blending them blends the warped text. Runs before the typewriter so each frame
	//writes the mesh once
	[DefaultExecutionOrder(-20)]
	public class DemoColorCycle : MonoBehaviour
	{
		[Serializable]
		public struct Stage
		{
			public string name;
			public float blend;
			public float hold;
			public float hue;
			public float greyscale;
			public Color greyTint;
			public float posterize;
			public float contrast;
			public float brightness;
			public Color gradeLight;
			public Color gradeMid;
			public Color gradeDark;
			public float lightAngle;
			public float lightElevation;
			public Color lightColor;
			public float lightIntensity;
			public Color ambient;
			public Vector2[] warpCage;
		}

		public Stage[] stages = new Stage[0];
		public TextMaterialConfiguratorMaster target;
		public TextShaderDemoElement showcase;
		public AllIn1TextTypewriter caption;
		public Graphic[] ambientGraphics = new Graphic[0];
		public AllIn1TextWarp warp;

		private float time;
		private int current = -1;
		private bool firstPass;

		private void OnEnable()
		{
			time = 0f;
			current = -1;
			firstPass = true;
			if (stages.Length > 0) { Apply(stages[0], stages[0], 1f); }
		}

		private void Update()
		{
			if (stages.Length == 0) { return; }
			float loop = 0f;
			foreach (Stage s in stages) { loop += s.blend + s.hold; }
			time = Mathf.Repeat(time + Time.deltaTime, Mathf.Max(0.01f, loop));

			float t = time;
			int i = 0;
			while (i < stages.Length - 1 && t >= stages[i].blend + stages[i].hold)
			{
				t -= stages[i].blend + stages[i].hold;
				i++;
			}
			Stage to = stages[i];
			//The first stage of a visit starts on its own look, not eased in from the last one
			if (i != 0) { firstPass = false; }
			Stage from = firstPass ? to : stages[(i + stages.Length - 1) % stages.Length];
			float k = to.blend > 0f ? Mathf.Clamp01(t / to.blend) : 1f;
			k = k * k * (3f - 2f * k);
			Apply(from, to, k);

			if (i != current)
			{
				current = i;
				if (caption != null && showcase != null && showcase.IsShown())
				{
					caption.SetText(to.name);
					caption.Reveal();
				}
			}
		}

		private void Apply(Stage a, Stage b, float k)
		{
			Material m = target != null ? target.TextMaterial : null;
			if (m != null)
			{
				m.SetFloat("_HueShift", Mathf.Repeat(a.hue + Mathf.DeltaAngle(a.hue, b.hue) * k, 360f));
				m.SetFloat("_GreyscaleBlend", Mathf.Lerp(a.greyscale, b.greyscale, k));
				m.SetColor("_GreyscaleTintColor", Color.Lerp(a.greyTint, b.greyTint, k));
				//Colour counts read better eased in log space: 200 to 5 spends its time where the bands show
				m.SetFloat("_PosterizeNumColors", Mathf.Exp(Mathf.Lerp(Mathf.Log(a.posterize), Mathf.Log(b.posterize), k)));
				m.SetFloat("_Contrast", Mathf.Lerp(a.contrast, b.contrast, k));
				m.SetFloat("_Brightness", Mathf.Lerp(a.brightness, b.brightness, k));
				m.SetColor("_ColorGradingLight", Color.Lerp(a.gradeLight, b.gradeLight, k));
				m.SetColor("_ColorGradingMiddle", Color.Lerp(a.gradeMid, b.gradeMid, k));
				m.SetColor("_ColorGradingDark", Color.Lerp(a.gradeDark, b.gradeDark, k));
				float angle = a.lightAngle + Mathf.DeltaAngle(a.lightAngle, b.lightAngle) * k;
				float elevation = Mathf.Lerp(a.lightElevation, b.lightElevation, k);
				m.SetFloat("_LightAngle", Mathf.Repeat(angle, 360f));
				m.SetFloat("_LightElevation", elevation);
				m.SetColor("_LightColor", Color.Lerp(a.lightColor, b.lightColor, k));
				m.SetFloat("_LightIntensity", Mathf.Lerp(a.lightIntensity, b.lightIntensity, k));
				m.SetFloat("_ExtrudeLightAngle", Mathf.Repeat(angle, 360f));
				m.SetFloat("_ExtrudeLightElevation", elevation * 0.5f);
			}
			if (warp != null && a.warpCage != null && b.warpCage != null && a.warpCage.Length == 16 && b.warpCage.Length == 16)
			{
				for (int c = 0; c < 16; c++) { warp.SetCagePoint(c, Vector2.Lerp(a.warpCage[c], b.warpCage[c], k)); }
			}
			Color ambient = Color.Lerp(a.ambient, b.ambient, k);
			foreach (Graphic g in ambientGraphics)
			{
				if (g != null) { g.color = ambient; }
			}
			//The caption's face colour through its material: a vertex colour change would relayout the text every frame of a blend
			if (caption != null)
			{
				TMPro.TMP_Text text = caption.GetComponent<TMPro.TMP_Text>();
				Material cm = text != null ? text.fontSharedMaterial : null;
				if (cm != null) { cm.SetColor("_FaceColor", Color.Lerp(Color.white, new Color(ambient.r, ambient.g, ambient.b, 1f), 0.45f)); }
			}
		}
	}
}
