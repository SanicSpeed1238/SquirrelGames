using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//The demo background per showcase. Two DemoBackdropLayers sets: the shown look sits on one, and when the carousel
	//moves the next look builds on the other, which fades in on top. Showcases without a look get the fallback
	public class DemoBackdrop : MonoBehaviour
	{
		[System.Serializable]
		public class Glow
		{
			public Texture texture;
			public Color color = new Color(1f, 1f, 1f, 0f);
			public Vector2 size = new Vector2(1.6f, 1f);
			public Vector2 offset;
			//Pinned to the screen's bottom edge at full width, size.y screen heights tall, at any aspect (the Asset Title's floor); size.x and offset are unused
			public bool pinToBottom;
		}

		[System.Serializable]
		public class Look
		{
			public string name = "Current";
			public Color top = new Color(0.275f, 0.275f, 0.275f, 1f);
			public Color bottom = new Color(0.061f, 0.061f, 0.061f, 1f);
			public float exponent = 1.2f;
			public Texture pattern;
			public Color patternColor = new Color(1f, 1f, 1f, 0.0252f);
			public float patternTiles = 4f;
			public Vector2 patternScroll = new Vector2(-2f, 3f);
			//Degrees the pattern is turned by (the Asset Title's names are tilted like the trailer's opening wall)
			public float patternAngle;
			public Glow lightA = new Glow();
			public Glow lightB = new Glow();
			public float vignette;
			public string words;
			public TMP_FontAsset wordsFont;
			public Color wordsColor = new Color(1f, 1f, 1f, 0.06f);
			public bool wordsOutline = true;
			public float wordsSize = 0.13f;
			public float wordsAngle = -8f;
			public float wordsSpeed = 0.025f;
			public Color dustColor = new Color(1f, 1f, 1f, 0f);
			public int dustCount = 40;
			//Non zero: motes show only inside this light (radii in screen heights around the stage centre), like dust catching a spotlight
			public Vector2 dustLight;
		}

		[System.Serializable]
		public class Entry
		{
			public string showcase;
			public Look look;
		}

		public DemoBackdropLayers[] sets = new DemoBackdropLayers[2];
		public List<Entry> showcases = new List<Entry>();
		public Look fallback = new Look();
		public float fadeSeconds = 0.7f;
		//Lights down, lights up: the old look fades to black over the first half, the new one comes up over the second, instead of a dissolve
		public bool dipToBlack;
		public bool followCarousel = true;

		private int front;
		private string shownFor;
		private float fadeT = 1f;
		private TextShaderDemoElementsController ctrl;

		private void Start()
		{
			ctrl = FindAnyObjectByType<TextShaderDemoElementsController>(FindObjectsInactive.Include);
			shownFor = Current();
			Apply(LookFor(shownFor));
		}

		private string Current()
		{
			TextShaderDemoElement e = ctrl != null ? ctrl.GetCurrentDemoElement() : null;
			return e != null ? e.name : null;
		}

		public Look LookFor(string showcase)
		{
			foreach (Entry e in showcases)
			{
				if (e.showcase == showcase) { return e.look; }
			}
			return fallback;
		}

		//At once, on the front set
		public void Apply(Look l)
		{
			sets[front].gameObject.SetActive(true);
			sets[front].group.alpha = 1f;
			sets[1 - front].gameObject.SetActive(false);
			fadeT = 1f;
			sets[front].Apply(l);
		}

		public void FadeTo(Look l)
		{
			int back = 1 - front;
			DemoBackdropLayers next = sets[back];
			next.gameObject.SetActive(true);
			next.transform.SetAsLastSibling();
			next.group.alpha = 0f;
			next.Apply(l);
			front = back;
			fadeT = 0f;
		}

		private void Update()
		{
			if (followCarousel && ctrl != null)
			{
				string name = Current();
				if (name != shownFor)
				{
					bool first = shownFor == null;
					shownFor = name;
					if (first) { Apply(LookFor(name)); }
					else { FadeTo(LookFor(name)); }
				}
			}
			if (fadeT < 1f)
			{
				fadeT = Mathf.Min(1f, fadeT + Time.deltaTime / Mathf.Max(0.01f, fadeSeconds));
				if (dipToBlack)
				{
					sets[1 - front].group.alpha = Smooth(1f - fadeT * 2f);
					sets[front].group.alpha = Smooth(fadeT * 2f - 1f);
				}
				else { sets[front].group.alpha = Smooth(fadeT); }
				if (fadeT >= 1f) { sets[1 - front].gameObject.SetActive(false); }
			}
		}

		private static float Smooth(float t)
		{
			t = Mathf.Clamp01(t);
			return t * t * (3f - 2f * t);
		}
	}
}
