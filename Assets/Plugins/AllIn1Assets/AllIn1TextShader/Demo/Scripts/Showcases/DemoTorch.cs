using AllIn1Assets;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	//The cursor is a torch: each text's Bevel and Extrude light comes from the torch's side, higher and brighter the closer it is, a Shine band sits
	//under it and a halo follows it. A click casts: a quick springy swirl (Twist) around the click, a violet flare of the light and a flash. Without recent
	//mouse movement over the showcase the torch circles on its own. Sweep (on reveal started) carries the torch in across the text so it lights the
	//letters as they appear; Snuff (on hide started) puts it out. Writes the configurators' materials, as the demo's tweens do. The flame flickers every
	//light, and at Play it builds a stone wall that only shows where the torch lights it and dust catching its light. The embers are a Text Particles emitter
	public class DemoTorch : MonoBehaviour
	{
		private class Mote
		{
			public RawImage image;
			public Vector2 pos;
			public Vector2 vel;
			public float alpha;
		}

		public TextMaterialConfiguratorMaster[] texts = new TextMaterialConfiguratorMaster[0];
		public RectTransform halo;
		public CanvasGroup flash;
		public Vector2 orbit = new Vector2(300f, 110f);
		public float orbitSpeed = 0.7f;
		public float idleDelay = 2.5f;
		public float follow = 16f;
		public float reach = 420f;
		//Peaks reached with the torch right on a letter
		public float lightMax = 1f;
		public float specularMax = 2f;
		public float shineMax = 1.6f;
		//The cast: a damped spring on the Twist amount (peak, decay per second, angular frequency), small enough to keep the word readable
		public float twistPeak = 0.7f;
		public float twistDecay = 7f;
		public float twistFrequency = 15f;
		public Color castColor = new Color(0.7f, 0.45f, 1f, 1f);
		public float castFlare = 1.2f;
		//The entrance sweep, in the showcase's own units
		public float sweepFrom = -460f;
		public float sweepTo = 300f;
		public float sweepDuration = 0.8f;
		//The flame: every light flickers by up to this fraction on two noise octaves, the halo and the lit wall jitter by a few units
		public float flicker = 0.25f;
		public float flickerSpeed = 6f;
		public float jitter = 3f;
		public Color haloHot = new Color(1f, 0.74f, 0.3f, 1f);
		//A hot core inside the halo, in halo sizes
		public Color coreColor = new Color(1f, 0.85f, 0.55f, 0.5f);
		public float coreSize = 0.32f;
		//The wall, built under the frame: a stone texture (wallTile units per tile) under a darkness whose clamped mask leaves a lit hole at the torch,
		//lightRadius being the mask's half width in the showcase's units
		public Texture wallTexture;
		public Color wallColor = new Color(0.42f, 0.37f, 0.32f, 1f);
		public Vector2 wallSize = new Vector2(2800f, 1800f);
		public float wallTile = 420f;
		public Texture darkMask;
		public Color darkColor = new Color(0.02f, 0.015f, 0.012f, 0.94f);
		public float lightRadius = 760f;
		//Embers rising off the torch: a Text Particles emitter on the halo whose rate follows the light, plus a burst on a cast
		public AllIn1TextParticles embers;
		public int castEmbers = 12;
		//Dust catching the torch's light, drawn with the halo's texture and material
		public int dustCount = 36;
		public Vector2 dustArea = new Vector2(1100f, 640f);
		public float dustLight = 280f;
		public Color dustColor = new Color(1f, 0.85f, 0.65f, 0.55f);

		private RectTransform rectTransform;
		private Camera canvasCamera;
		private Vector2 torch;
		private Vector2 lastMouse;
		private float idle;
		private float orbitAngle;
		private float castTime = 99f;
		private Vector2 twistAt;
		private float sweepTime = 99f;
		private float lit = 1f;
		private float litTarget = 1f;
		private Color[] baseLight = new Color[0];
		private bool built;
		private RawImage haloImage;
		private Color haloColor;
		private RawImage core;
		private RawImage dark;
		private Mote[] motes = new Mote[0];
		private float noiseSeed;

		private void OnEnable()
		{
			rectTransform = (RectTransform)transform;
			Canvas canvas = GetComponentInParent<Canvas>();
			canvasCamera = canvas != null ? canvas.rootCanvas.worldCamera : null;
			idle = idleDelay;
			orbitAngle = 0f;
			torch = new Vector2(sweepFrom, 0f);
			lastMouse = MousePosition();
			castTime = 99f;
			lit = 1f;
			litTarget = 1f;
			if (flash != null) { flash.alpha = 0f; }
			baseLight = new Color[texts.Length];
			for (int i = 0; i < texts.Length; i++)
			{
				Material m = texts[i] != null ? texts[i].TextMaterial : null;
				baseLight[i] = m != null ? m.GetColor("_LightColor") : Color.white;
			}
			if (!built) { Build(); }
		}

		//Wall, darkness and dust go under the frame ahead of the halo; built once at Play, so edit mode keeps the scene as saved
		private void Build()
		{
			built = true;
			noiseSeed = Random.Range(0f, 100f);
			haloImage = halo != null ? halo.GetComponent<RawImage>() : null;
			if (haloImage != null) { haloColor = haloImage.color; }
			Texture dot = haloImage != null ? haloImage.texture : null;
			Material additive = haloImage != null ? haloImage.material : null;
			RectTransform frame = halo != null ? (RectTransform)halo.parent : rectTransform;

			int index = 0;
			if (wallTexture != null)
			{
				RawImage wall = CreateImage("Wall", frame, wallTexture, null, wallColor, wallSize);
				wall.uvRect = new Rect(0f, 0f, wallSize.x / wallTile, wallSize.y / wallTile);
				wall.transform.SetSiblingIndex(index++);
			}
			if (darkMask != null)
			{
				//Wide enough to cover the wall from wherever the torch goes inside the showcase
				dark = CreateImage("Darkness", frame, darkMask, null, darkColor, wallSize + rectTransform.rect.size);
				dark.transform.SetSiblingIndex(index++);
			}
			if (dot == null) { return; }

			motes = new Mote[Mathf.Max(0, dustCount)];
			if (motes.Length > 0)
			{
				RectTransform dust = CreateHolder("Dust", frame);
				dust.SetSiblingIndex(index++);
				for (int i = 0; i < motes.Length; i++)
				{
					float s = Random.Range(2f, 5f);
					motes[i] = new Mote
					{
						image = CreateImage("Mote" + i, dust, dot, additive, Color.clear, new Vector2(s, s)),
						pos = new Vector2(Random.Range(-0.5f, 0.5f) * dustArea.x, Random.Range(-0.5f, 0.5f) * dustArea.y),
						vel = Random.insideUnitCircle * 8f,
						alpha = Random.Range(0.4f, 1f),
					};
				}
			}
			if (halo != null)
			{
				core = CreateImage("Core", halo, dot, additive, coreColor, halo.rect.size * coreSize);
			}
		}

		private RectTransform CreateHolder(string name, Transform parent)
		{
			GameObject go = new GameObject(name, typeof(RectTransform));
			go.layer = gameObject.layer;
			go.transform.SetParent(parent, false);
			return (RectTransform)go.transform;
		}

		private RawImage CreateImage(string name, Transform parent, Texture texture, Material material, Color color, Vector2 size)
		{
			GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
			go.layer = gameObject.layer;
			go.transform.SetParent(parent, false);
			RawImage image = go.AddComponent<RawImage>();
			image.texture = texture;
			image.material = material;
			image.color = color;
			image.raycastTarget = false;
			image.rectTransform.sizeDelta = size;
			return image;
		}

		//Carries the torch in across the text from the left, so it lights the letters as they appear
		public void Sweep()
		{
			sweepTime = 0f;
			lit = 1f;
			litTarget = 1f;
			torch = new Vector2(sweepFrom, 0f);
		}

		//Puts the torch out: the light dies away and the letters fall back into the dark
		public void Snuff()
		{
			litTarget = 0f;
		}

		//Casts at a point in the showcase's own units, as a click there does
		public void Cast(Vector2 local)
		{
			castTime = 0f;
			twistAt = local;
			if (embers != null) { embers.Emit(castEmbers); }
		}

		private void Update()
		{
			float dt = Time.deltaTime;
			Vector2 mouse = MousePosition();
			bool moved = (mouse - lastMouse).sqrMagnitude > 1f;
			lastMouse = mouse;
			Vector2 local;
			bool inside = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mouse, canvasCamera, out local)
				&& Mathf.Abs(local.x) < rectTransform.rect.width * 0.5f && Mathf.Abs(local.y) < rectTransform.rect.height * 0.5f;
			idle = moved && inside ? 0f : idle + dt;

			sweepTime += dt;
			if (sweepTime < sweepDuration)
			{
				float k = sweepTime / sweepDuration;
				torch = new Vector2(Mathf.Lerp(sweepFrom, sweepTo, k), 40f * Mathf.Sin(k * Mathf.PI));
				//The orbit picks up where the sweep ends
				orbitAngle = 0f;
			}
			else
			{
				orbitAngle += orbitSpeed * dt;
				Vector2 target = idle < idleDelay ? local : new Vector2(Mathf.Cos(orbitAngle) * orbit.x, Mathf.Sin(orbitAngle) * orbit.y);
				torch = Vector2.Lerp(torch, target, 1f - Mathf.Exp(-follow * dt));
			}
			lit = Mathf.MoveTowards(lit, litTarget, dt / 0.35f);

			if (inside && Pressed()) { Cast(local); }
			castTime += dt;
			float envelope = Mathf.Exp(-twistDecay * castTime);
			float twist = twistPeak * envelope * Mathf.Cos(twistFrequency * castTime);
			float flare = Mathf.Exp(-5f * castTime);

			float t = Time.time;
			float f = Flicker(t);
			Vector2 flame = torch + new Vector2(Mathf.PerlinNoise(t * 9f, noiseSeed + 3f) - 0.5f, Mathf.PerlinNoise(noiseSeed + 5f, t * 9f) - 0.5f) * (2f * jitter);
			UpdateFlame(flame, f, flare);
			if (embers != null) { embers.rateMultiplier = lit; }
			UpdateDust(dt, lit * f);

			if (flash != null)
			{
				flash.alpha = Mathf.Clamp01(flare * 1.2f);
				((RectTransform)flash.transform).anchoredPosition = twistAt;
				float s = 0.35f + 1.15f * (1f - Mathf.Exp(-14f * castTime));
				flash.transform.localScale = new Vector3(s, s, 1f);
			}

			for (int i = 0; i < texts.Length; i++)
			{
				TextMaterialConfiguratorMaster text = texts[i];
				Material m = text != null ? text.TextMaterial : null;
				if (m == null) { continue; }
				RectTransform rt = (RectTransform)text.transform;
				Vector2 d = torch - rt.anchoredPosition;
				float near = (1f - Mathf.Clamp01(d.magnitude / reach)) * lit;
				float angle = Mathf.Repeat(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, 360f);
				float elevation = Mathf.Lerp(12f, 55f, near * near);
				m.SetFloat("_LightAngle", angle);
				m.SetFloat("_LightElevation", elevation);
				m.SetFloat("_LightIntensity", Mathf.Lerp(0.1f * lit, lightMax, near) * f + castFlare * flare);
				m.SetColor("_LightColor", Color.Lerp(i < baseLight.Length ? baseLight[i] : Color.white, castColor, flare));
				m.SetFloat("_ExtrudeLightAngle", angle);
				m.SetFloat("_ExtrudeLightElevation", elevation * 0.5f);
				m.SetFloat("_SpecularIntensity", Mathf.Lerp(0f, specularMax, near));
				//The shine band and the swirl sit at the torch and the click, in the text's rect space (0 to 1 across)
				Vector2 size = rt.rect.size;
				m.SetFloat("_ShineLocation", (torch.x - rt.anchoredPosition.x) / Mathf.Max(1f, size.x) + 0.5f);
				m.SetFloat("_ShineGlow", Mathf.Lerp(0f, shineMax, near));
				m.SetFloat("_TwistUvAmount", twist);
				m.SetFloat("_TwistUvPosX", (twistAt.x - rt.anchoredPosition.x) / Mathf.Max(1f, size.x) + 0.5f);
				m.SetFloat("_TwistUvPosY", (twistAt.y - rt.anchoredPosition.y) / Mathf.Max(1f, size.y) + 0.5f);
			}
		}

		//1 on average, wandering by up to flicker on a slow and a fast noise
		private float Flicker(float t)
		{
			float slow = Mathf.PerlinNoise(t * flickerSpeed, noiseSeed) * 2f - 1f;
			float fast = Mathf.PerlinNoise(t * flickerSpeed * 2.9f, noiseSeed + 7.3f) * 2f - 1f;
			return 1f + flicker * (0.65f * slow + 0.35f * fast);
		}

		private void UpdateFlame(Vector2 flame, float f, float flare)
		{
			if (halo != null)
			{
				halo.anchoredPosition = flame;
				float hs = Mathf.Lerp(0.3f, 1f, lit) * (1f + 0.5f * (f - 1f));
				halo.localScale = new Vector3(hs, hs, 1f);
			}
			if (haloImage != null)
			{
				Color c = Color.Lerp(haloColor, new Color(haloHot.r, haloHot.g, haloHot.b, haloColor.a), Mathf.PerlinNoise(Time.time * 1.7f, noiseSeed + 11f) * 0.6f);
				c.a = haloColor.a * f;
				haloImage.color = c;
			}
			if (core != null)
			{
				Color c = coreColor;
				c.a *= lit * f * f;
				core.color = c;
			}
			if (dark != null)
			{
				//The lit hole shrinks shut as the torch dies, breathes with the flicker and opens up on a cast
				float radius = Mathf.Max(1f, lightRadius * Mathf.Lerp(0.04f, 1f, lit) * (1f + 0.6f * (f - 1f)) * (1f + 0.3f * flare));
				Vector2 size = dark.rectTransform.sizeDelta;
				Vector2 span = size / (2f * radius);
				dark.rectTransform.anchoredPosition = flame;
				dark.uvRect = new Rect(0.5f - span.x * 0.5f, 0.5f - span.y * 0.5f, span.x, span.y);
			}
		}

		private void UpdateDust(float dt, float glow)
		{
			float radius = Mathf.Max(1f, dustLight * glow);
			foreach (Mote m in motes)
			{
				m.vel = Vector2.ClampMagnitude(m.vel + Random.insideUnitCircle * (30f * dt), 12f);
				m.pos += (m.vel + new Vector2(0f, 3f)) * dt;
				//Wrapped inside the dust area so the motes drift forever
				m.pos.x = Mathf.Repeat(m.pos.x + dustArea.x * 0.5f, dustArea.x) - dustArea.x * 0.5f;
				m.pos.y = Mathf.Repeat(m.pos.y + dustArea.y * 0.5f, dustArea.y) - dustArea.y * 0.5f;
				Vector2 d = (m.pos - torch) / radius;
				Color c = dustColor;
				c.a *= m.alpha * Mathf.Exp(-d.sqrMagnitude);
				m.image.color = c;
				m.image.rectTransform.anchoredPosition = m.pos;
			}
		}

		private static Vector2 MousePosition()
		{
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
			return Input.mousePosition;
#endif
		}

		private static bool Pressed()
		{
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
			return Input.GetMouseButtonDown(0);
#endif
		}
	}
}
