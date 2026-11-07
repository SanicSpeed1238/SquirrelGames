using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//Sprite particles for UI text: emits from a TextMeshPro text's letters, plays on its typewriter's events and draws every particle in one canvas mesh
	[ExecuteAlways]
	[RequireComponent(typeof(CanvasRenderer))]
	[AddComponentMenu("AllIn1TextShader/Particles")]
	public class AllIn1TextParticles : MaskableGraphic
	{
		public const string SHADER_NAME = "AllIn1Text/UI Particle";
		private const float FIXED_STEP = 1f / 60f;
		private const int MAX_PARTICLES = 10000;

		public TextParticlesMain main = new TextParticlesMain();
		public TextParticlesTextLink textLink = new TextParticlesTextLink();
		public TextParticlesEmission emission = new TextParticlesEmission();
		public TextParticlesShapeModule shape = new TextParticlesShapeModule();
		public TextParticlesForces forces = new TextParticlesForces();
		public TextParticlesSizeOverLifetime sizeOverLifetime = new TextParticlesSizeOverLifetime();
		public TextParticlesColorOverLifetime colorOverLifetime = new TextParticlesColorOverLifetime();
		public TextParticlesRotationOverLifetime rotationOverLifetime = new TextParticlesRotationOverLifetime();
		public TextParticlesFlicker flicker = new TextParticlesFlicker();
		public TextParticlesTextureSheet textureSheet = new TextParticlesTextureSheet();
		public TextParticlesLifePreset lifePreset = new TextParticlesLifePreset();
		public TextParticlesRenderer rendererModule = new TextParticlesRenderer();
		[SerializeField, HideInInspector] private Shader particleShader;

		//Scales the emission rate from code, e.g. a torch dying down
		[System.NonSerialized] public float rateMultiplier = 1f;
		//Freezes the simulation where it is (the overlay's pause and scrub)
		[System.NonSerialized] public bool paused;

		//Raised with how many particles each emission made; per parent child emitters listen to it
		public event System.Action<int> OnEmitted;

		private struct Particle
		{
			public Vector2 pos;
			public Vector2 vel;
			public float age;
			public float life;
			public float size;
			public float rotation;
			public float rotationSpeed;
			public float random;
			public float flipX;
			public float flipY;
			public int frame;
			public float nextFrameAt;
			public Color color;
		}

		private struct Quad
		{
			public Vector2 bl;
			public Vector2 tl;
			public Vector2 tr;
			public Vector2 br;

			public Vector2 Center => (bl + tr) * 0.5f;
		}

		private struct ScheduledLetter
		{
			public float at;
			public int charIndex;
		}

		private static readonly Dictionary<int, Material> sharedMaterials = new Dictionary<int, Material>();
		private static readonly List<int> pathA = new List<int>();
		private static readonly List<int> pathB = new List<int>();

		private TextParticlesDrawProxy proxy;

		private Particle[] particles = new Particle[0];
		private int count;
		private System.Random rng = new System.Random(1);
		private int activeSeed;
		private bool emitting;
		private float systemTime;
		private float emitDebt;
		private readonly List<ScheduledLetter> letterSchedule = new List<ScheduledLetter>();
		private AllIn1TextTypewriter subscribedTypewriter;
		private AllIn1TextParticles subscribedParent;
		private bool prewarming;

		private readonly List<Quad> letters = new List<Quad>();
		private Vector2 spawnCenter;
		private Vector2 spawnNormal;
		private Vector2 spawnOrigin;
		private bool hasTextBox;
		private Vector2 textBoxMin;
		private Vector2 textBoxMax;
		private Matrix4x4 localToSim = Matrix4x4.identity;
		private Matrix4x4 simToLocal = Matrix4x4.identity;
		private float simScale = 1f;

		private MeshEffectGroupState lifeState;
		private VertexGroupLayout[] lifeLayouts = new VertexGroupLayout[0];
		private MeshEffectGroupConfig builtLifePreset;
		private bool lifeStateDirty = true;
		private readonly Vector3[] quadVerts = new Vector3[4];
		private readonly Color32[] quadColors = new Color32[4];

		public int ParticleCount => count;
		public bool IsEmitting => emitting;
		public bool IsAlive => emitting || count > 0 || letterSchedule.Count > 0;
		public float PlaybackTime => systemTime;
		//The span the overlay's time slider covers: one cycle plus the longest life
		public float PreviewLength => Mathf.Max(0.1f, main.startDelay + main.duration + main.startLifetime.max);
		public TMP_Text LinkedText => textLink.text != null ? textLink.text : GetComponentInParent<TMP_Text>();
		public AllIn1TextTypewriter LinkedTypewriter
		{
			get
			{
				TMP_Text t = LinkedText;
				return t != null ? t.GetComponent<AllIn1TextTypewriter>() : null;
			}
		}

		#region PLAYBACK
		//Starts emitting this emitter and every child set to Parent or Code. Does nothing to one already emitting
		public void Play(bool withChildren = true)
		{
			PlaySelf();
			if (!withChildren) { return; }
			foreach (AllIn1TextParticles child in GetComponentsInChildren<AllIn1TextParticles>())
			{
				if (child != this && child.textLink.playOn == TextParticlesPlayOn.Manual) { child.PlaySelf(); }
			}
		}

		public void Stop(bool withChildren = true, bool clear = false)
		{
			foreach (AllIn1TextParticles e in Family(withChildren))
			{
				e.emitting = false;
				e.letterSchedule.Clear();
				if (clear) { e.count = 0; }
				e.SetVerticesDirty();
			}
		}

		public void Clear(bool withChildren = true)
		{
			foreach (AllIn1TextParticles e in Family(withChildren))
			{
				e.count = 0;
				e.emitting = false;
				e.letterSchedule.Clear();
				e.SetVerticesDirty();
			}
		}

		//Starts over as if the trigger had just fired: per letter emission replays along the letters at the typewriter's pace. Used by the overlay
		public void Restart(bool withChildren = true, bool newSeed = true)
		{
			foreach (AllIn1TextParticles e in Family(withChildren))
			{
				e.RestartSelf(newSeed);
				e.UpdateDrawRoute();
				e.SetVerticesDirty();
			}
		}

		//Restarts and runs t seconds in fixed steps, then holds there (paused). Same seed, same t, same picture
		public void Simulate(float t, bool withChildren = true)
		{
			SimulateTogether(new List<AllIn1TextParticles>(Family(withChildren)), t);
		}

		//Restarts every emitter, then steps them together, so a per parent child keeps what its parent emits along the way
		public static void SimulateTogether(List<AllIn1TextParticles> emitters, float t)
		{
			emitters.RemoveAll(e => e == null);
			foreach (AllIn1TextParticles e in emitters) { e.RestartSelf(false); }
			int steps = Mathf.FloorToInt(t / FIXED_STEP);
			for (int i = 0; i <= steps; i++)
			{
				float dt = i < steps ? FIXED_STEP : t - steps * FIXED_STEP;
				foreach (AllIn1TextParticles e in emitters) { e.AdvanceInternal(dt); }
			}
			foreach (AllIn1TextParticles e in emitters)
			{
				e.paused = true;
				e.UpdateDrawRoute();
				e.SetVerticesDirty();
			}
		}

		//One simulation step; the editor preview calls it, play mode runs it from LateUpdate
		public void Advance(float dt)
		{
			UpdateDrawRoute();
			AdvanceInternal(dt);
			SetVerticesDirty();
		}

		public void Emit(int n)
		{
			EmitBatch(n, -1);
		}

		//Emits at one letter, by its index in the text's characterInfo
		public void EmitAtLetter(int charIndex, int n)
		{
			EmitBatch(n, charIndex);
		}

		private IEnumerable<AllIn1TextParticles> Family(bool withChildren)
		{
			if (!withChildren) { return new[] { this }; }
			return GetComponentsInChildren<AllIn1TextParticles>();
		}

		private void PlaySelf()
		{
			paused = false;
			if (emitting) { return; }
			Reseed(true);
			emitting = true;
			systemTime = 0f;
			emitDebt = 0f;
			if (main.prewarm && main.looping) { Prewarm(); }
		}

		private void RestartSelf(bool newSeed)
		{
			count = 0;
			emitDebt = 0f;
			systemTime = 0f;
			paused = false;
			letterSchedule.Clear();
			Reseed(newSeed);
			emitting = emission.enabled && (emission.rateOverTime > 0f || emission.bursts.Count > 0);
			if (emission.enabled && emission.perLetter) { ScheduleLetters(); }
			if (main.prewarm && main.looping && emitting) { Prewarm(); }
		}

		private void Reseed(bool newSeed)
		{
			if (main.seed != 0) { activeSeed = main.seed; }
			else if (newSeed || activeSeed == 0) { activeSeed = System.Environment.TickCount | 1; }
			rng = new System.Random(activeSeed);
		}

		private void Prewarm()
		{
			float total = Mathf.Max(FIXED_STEP, main.duration);
			prewarming = true;
			for (float t = 0f; t < total; t += FIXED_STEP) { AdvanceInternal(FIXED_STEP); }
			prewarming = false;
		}

		//Preview stand in for the typewriter's per letter event: every visible letter in order, at the reveal preset's delay
		private void ScheduleLetters()
		{
			TMP_Text t = LinkedText;
			if (t == null || t.textInfo == null) { return; }
			AllIn1TextTypewriter tw = t.GetComponent<AllIn1TextTypewriter>();
			float delay = tw != null && tw.revealConfig != null && tw.revealConfig.groupDelay > 0f ? tw.revealConfig.groupDelay : 0.05f;
			TMP_TextInfo info = t.textInfo;
			float at = main.startDelay;
			for (int i = 0; i < info.characterCount; i++)
			{
				if (!info.characterInfo[i].isVisible) { continue; }
				letterSchedule.Add(new ScheduledLetter { at = at, charIndex = i });
				at += delay;
			}
		}
		#endregion

		#region SIMULATION
		private void LateUpdate()
		{
			if (!Application.isPlaying) { return; }
			if (paused || !IsAlive)
			{
				UpdateDrawRoute();
				return;
			}
			Advance(Time.deltaTime);
		}

		private void AdvanceInternal(float dt)
		{
			dt *= Mathf.Max(0f, main.simulationSpeed);
			if (dt <= 0f) { return; }

			if (emitting) { EmitOverTime(dt); }
			else { systemTime += dt; }

			while (letterSchedule.Count > 0 && letterSchedule[0].at <= systemTime)
			{
				int ci = letterSchedule[0].charIndex;
				letterSchedule.RemoveAt(0);
				EmitBatch(PerLetterCount(), ci);
			}

			UpdateParticles(dt);
		}

		private void EmitOverTime(float dt)
		{
			float duration = Mathf.Max(0.01f, main.duration);
			float from = systemTime - main.startDelay;
			systemTime += dt;
			float to = systemTime - main.startDelay;
			if (to <= 0f) { return; }
			from = Mathf.Max(0f, from);
			if (!main.looping)
			{
				from = Mathf.Min(from, duration);
				to = Mathf.Min(to, duration);
			}

			if (emission.enabled && to > from)
			{
				emitDebt += Mathf.Max(0f, emission.rateOverTime) * Mathf.Max(0f, rateMultiplier) * (to - from);
				int n = (int)emitDebt;
				emitDebt -= n;
				EmitBatch(n, -1);
				FireBursts(from, to, duration);
			}

			if (!main.looping && systemTime - main.startDelay >= duration) { emitting = false; }
		}

		private void FireBursts(float from, float to, float duration)
		{
			if (emission.bursts.Count == 0) { return; }
			if (!main.looping)
			{
				FireBurstsInCycle(from, to);
				return;
			}
			while (from < to)
			{
				float cycleStart = Mathf.Floor(from / duration) * duration;
				float segmentEnd = Mathf.Min(to, cycleStart + duration);
				if (segmentEnd <= from) { break; }
				FireBurstsInCycle(from - cycleStart, segmentEnd - cycleStart);
				from = segmentEnd;
			}
		}

		//Half open [a, b), so a burst at 0 fires on the first step
		private void FireBurstsInCycle(float a, float b)
		{
			foreach (TextParticlesBurst burst in emission.bursts)
			{
				int cycles = Mathf.Max(1, burst.cycles);
				float interval = Mathf.Max(0.01f, burst.interval);
				for (int c = 0; c < cycles; c++)
				{
					float t = burst.time + c * interval;
					if (t >= a && t < b) { EmitBatch(Mathf.RoundToInt(burst.count.Lerp(R())), -1); }
				}
			}
		}

		private int PerLetterCount()
		{
			return Mathf.RoundToInt(emission.perLetterCount.Lerp(R()));
		}

		//Order kept (no swap remove) so alpha blended particles never trade places
		private void UpdateParticles(float dt)
		{
			int frames = FrameCount();
			int write = 0;
			for (int i = 0; i < count; i++)
			{
				Particle p = particles[i];
				p.age += dt;
				if (p.age >= p.life) { continue; }

				if (forces.enabled)
				{
					p.vel += forces.gravity * dt;
					if (forces.turbulence != 0f)
					{
						float n = p.age * forces.turbulenceFrequency;
						Vector2 wander = new Vector2(Mathf.PerlinNoise(p.random * 97f, n) - 0.5f, Mathf.PerlinNoise(n + 31f, p.random * 61f) - 0.5f);
						p.vel += wander * (2f * forces.turbulence * dt);
					}
					if (forces.drag > 0f) { p.vel *= Mathf.Exp(-forces.drag * dt); }
				}
				p.pos += p.vel * dt;
				p.rotation += p.rotationSpeed * dt;

				if (textureSheet.enabled && textureSheet.mode == TextParticlesFrameMode.Crackle && textureSheet.fps > 0f)
				{
					while (p.age >= p.nextFrameAt)
					{
						p.frame = rng.Next(frames);
						p.nextFrameAt += 1f / textureSheet.fps;
					}
				}

				particles[write++] = p;
			}
			count = write;
		}
		#endregion

		#region EMISSION
		private float R() => (float)rng.NextDouble();

		private void EmitBatch(int n, int charIndex)
		{
			if (n <= 0) { return; }
			if (rng == null || activeSeed == 0) { Reseed(true); }
			EnsureCapacity();
			PrepareSpawn(charIndex);
			int before = count;
			for (int i = 0; i < n && count < particles.Length; i++) { SpawnOne(); }
			SetVerticesDirty();
			if (count > before && !prewarming && OnEmitted != null) { OnEmitted(count - before); }
		}

		private void EnsureCapacity()
		{
			int size = Mathf.Clamp(main.maxParticles, 1, MAX_PARTICLES);
			if (particles.Length == size) { return; }
			Particle[] resized = new Particle[size];
			count = Mathf.Min(count, size);
			System.Array.Copy(particles, resized, count);
			particles = resized;
		}

		private void RefreshSpace()
		{
			localToSim = Matrix4x4.identity;
			if (main.simulationSpace == TextParticlesSpace.Canvas)
			{
				Canvas c = canvas;
				if (c != null) { localToSim = c.rootCanvas.transform.worldToLocalMatrix * transform.localToWorldMatrix; }
			}
			simToLocal = localToSim.inverse;
			simScale = ((Vector2)localToSim.MultiplyVector(Vector3.up)).magnitude;
		}

		private void PrepareSpawn(int charIndex)
		{
			RefreshSpace();
			letters.Clear();
			hasTextBox = false;
			spawnOrigin = shape.offset;
			spawnCenter = shape.offset;

			TMP_Text t = LinkedText;
			if (t == null) { return; }
			Matrix4x4 textToLocal = transform.worldToLocalMatrix * t.transform.localToWorldMatrix;
			GetLetterSource(t, out TMP_TextInfo info, out Vector3[] verts, out Color32[] cols);

			//The ink box: every visible glyph's live quad, so it hugs the letters (TMP's textBounds can be the whole rect)
			if (info != null && verts != null)
			{
				Vector2 lo = new Vector2(float.MaxValue, float.MaxValue);
				Vector2 hi = new Vector2(float.MinValue, float.MinValue);
				for (int i = 0; i < info.characterCount; i++)
				{
					if (!TryGetLetter(info, verts, cols, i, false, textToLocal, out Quad q)) { continue; }
					lo = Vector2.Min(lo, Vector2.Min(Vector2.Min(q.bl, q.tl), Vector2.Min(q.tr, q.br)));
					hi = Vector2.Max(hi, Vector2.Max(Vector2.Max(q.bl, q.tl), Vector2.Max(q.tr, q.br)));
					hasTextBox = true;
				}
				textBoxMin = lo;
				textBoxMax = hi;
			}
			if (!hasTextBox)
			{
				//An empty or inactive text reports an inverted box of plus and minus two billion
				Bounds b = t.textBounds;
				if (b.size.x > 0.01f && b.size.y > 0.01f)
				{
					Vector2 a = textToLocal.MultiplyPoint3x4(b.min);
					Vector2 z = textToLocal.MultiplyPoint3x4(b.max);
					textBoxMin = Vector2.Min(a, z);
					textBoxMax = Vector2.Max(a, z);
					hasTextBox = true;
				}
			}
			if (hasTextBox) { spawnCenter = (textBoxMin + textBoxMax) * 0.5f + shape.offset; }

			bool letterShape = shape.shape == TextParticlesShape.Letters || shape.shape == TextParticlesShape.LetterTops || shape.shape == TextParticlesShape.LetterBottoms;
			if (info == null || verts == null || (charIndex < 0 && !letterShape)) { return; }

			if (charIndex >= 0)
			{
				if (TryGetLetter(info, verts, cols, charIndex, false, textToLocal, out Quad q))
				{
					letters.Add(q);
					spawnCenter = q.Center + shape.offset;
					spawnOrigin = spawnCenter;
				}
				return;
			}

			for (int i = 0; i < info.characterCount; i++)
			{
				if (TryGetLetter(info, verts, cols, i, shape.revealedLettersOnly, textToLocal, out Quad q)) { letters.Add(q); }
			}
		}

		//The cache's output vertices follow the effects, the spline and the warp; TMP's own mesh is the fallback before the toolkit has run
		private static void GetLetterSource(TMP_Text t, out TMP_TextInfo info, out Vector3[] verts, out Color32[] cols)
		{
			info = t.textInfo;
			verts = null;
			cols = null;
			TextMeshHandler handler = TextMeshHandlerRegistry.GetTextMeshHandler(t.gameObject.GetInstanceID());
			MeshCache cache = handler != null ? handler.GetMeshCache() : null;
			if (cache != null && cache.subMeshes != null && cache.subMeshes.Length > 0)
			{
				verts = cache.subMeshes[0].outputVerts;
				cols = cache.subMeshes[0].outputColors;
			}
			if ((verts == null || verts.Length == 0) && info != null && info.meshInfo != null && info.meshInfo.Length > 0)
			{
				verts = info.meshInfo[0].vertices;
				cols = info.meshInfo[0].colors32;
			}
		}

		private static bool TryGetLetter(TMP_TextInfo info, Vector3[] verts, Color32[] cols, int ci, bool revealedOnly, Matrix4x4 m, out Quad q)
		{
			q = default;
			if (ci < 0 || ci >= info.characterCount) { return false; }
			TMP_CharacterInfo c = info.characterInfo[ci];
			if (!c.isVisible || c.materialReferenceIndex != 0) { return false; }
			int v = c.vertexIndex;
			if (v + 3 >= verts.Length) { return false; }
			if (revealedOnly && cols != null && v + 3 < cols.Length && cols[v].a + cols[v + 1].a + cols[v + 2].a + cols[v + 3].a < 40) { return false; }

			q.bl = m.MultiplyPoint3x4(verts[v]);
			q.tl = m.MultiplyPoint3x4(verts[v + 1]);
			q.tr = m.MultiplyPoint3x4(verts[v + 2]);
			q.br = m.MultiplyPoint3x4(verts[v + 3]);

			if (revealedOnly)
			{
				Vector2 d1 = q.tr - q.bl;
				Vector2 d2 = q.tl - q.br;
				if (Mathf.Abs(d1.x * d2.y - d1.y * d2.x) < 1f) { return false; }
			}
			return true;
		}

		private void SpawnOne()
		{
			spawnNormal = Vector2.zero;
			Vector2 p = SpawnPoint();
			Vector2 dir = SpawnDirection(p);
			float speed = main.startSpeed.Lerp(R());

			Particle pt = new Particle();
			pt.pos = localToSim.MultiplyPoint3x4(p);
			pt.vel = localToSim.MultiplyVector(dir * speed);
			pt.age = 0f;
			float life = main.startLifetime.Lerp(R());
			if (main.travelDistance.max > 0f && speed > 0.01f) { life = main.travelDistance.Lerp(R()) / speed; }
			pt.life = Mathf.Max(0.01f, life);
			pt.size = main.startSize.Lerp(R()) * simScale;
			pt.rotation = main.startRotation.Lerp(R());
			pt.rotationSpeed = rotationOverLifetime.enabled ? rotationOverLifetime.speed.Lerp(R()) : 0f;
			pt.color = main.startColor != null ? main.startColor.Evaluate(R()) : Color.white;
			pt.random = R();
			pt.flipX = rendererModule.randomFlipX && R() < 0.5f ? -1f : 1f;
			pt.flipY = rendererModule.randomFlipY && R() < 0.5f ? -1f : 1f;
			int frames = FrameCount();
			pt.frame = textureSheet.enabled && textureSheet.mode != TextParticlesFrameMode.OverLifetime ? rng.Next(frames) : 0;
			pt.nextFrameAt = textureSheet.fps > 0f ? 1f / textureSheet.fps : float.MaxValue;
			particles[count++] = pt;
		}

		private Vector2 SpawnPoint()
		{
			switch (shape.shape)
			{
				case TextParticlesShape.Circle:
				{
					float a = R() * Mathf.PI * 2f;
					float r = shape.edgeOnly ? shape.radius : shape.radius * Mathf.Sqrt(R());
					spawnNormal = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
					return spawnOrigin + spawnNormal * r;
				}
				case TextParticlesShape.Box:
					return spawnOrigin + RectPoint(-shape.boxSize * 0.5f, shape.boxSize * 0.5f);
				case TextParticlesShape.TextBox:
					if (letters.Count > 0) { return LetterPoint(letters[0], false); }
					return hasTextBox ? RectPoint(textBoxMin, textBoxMax) + shape.offset : spawnOrigin;
				case TextParticlesShape.Letters:
				case TextParticlesShape.LetterTops:
				case TextParticlesShape.LetterBottoms:
					if (letters.Count == 0) { return spawnOrigin; }
					return LetterPoint(letters[rng.Next(letters.Count)], true) + shape.offset;
				default:
					return spawnOrigin;
			}
		}

		private Vector2 LetterPoint(Quad q, bool useShape)
		{
			TextParticlesShape s = useShape ? shape.shape : TextParticlesShape.Letters;
			if (s == TextParticlesShape.LetterTops)
			{
				spawnNormal = EdgeNormal(q.tl, q.tr);
				return Vector2.Lerp(q.tl, q.tr, R());
			}
			if (s == TextParticlesShape.LetterBottoms)
			{
				spawnNormal = EdgeNormal(q.br, q.bl);
				return Vector2.Lerp(q.bl, q.br, R());
			}
			if (shape.edgeOnly)
			{
				float e = R() * 4f;
				int side = Mathf.Min(3, (int)e);
				float f = e - side;
				//Clockwise from the bottom left, so each side's normal is its direction turned left
				Vector2 a = side == 0 ? q.bl : side == 1 ? q.tl : side == 2 ? q.tr : q.br;
				Vector2 b = side == 0 ? q.tl : side == 1 ? q.tr : side == 2 ? q.br : q.bl;
				spawnNormal = EdgeNormal(a, b);
				return Vector2.Lerp(a, b, f);
			}
			float u = R();
			float v = R();
			return Vector2.Lerp(Vector2.Lerp(q.bl, q.br, u), Vector2.Lerp(q.tl, q.tr, u), v);
		}

		private Vector2 RectPoint(Vector2 min, Vector2 max)
		{
			if (!shape.edgeOnly) { return new Vector2(Mathf.Lerp(min.x, max.x, R()), Mathf.Lerp(min.y, max.y, R())); }
			Vector2 size = max - min;
			float e = R() * 2f * (size.x + size.y);
			if (e < size.x)
			{
				spawnNormal = Vector2.down;
				return new Vector2(min.x + e, min.y);
			}
			e -= size.x;
			if (e < size.y)
			{
				spawnNormal = Vector2.right;
				return new Vector2(max.x, min.y + e);
			}
			e -= size.y;
			if (e < size.x)
			{
				spawnNormal = Vector2.up;
				return new Vector2(max.x - e, max.y);
			}
			e -= size.x;
			spawnNormal = Vector2.left;
			return new Vector2(min.x, max.y - e);
		}

		private static Vector2 EdgeNormal(Vector2 a, Vector2 b)
		{
			Vector2 d = b - a;
			Vector2 n = new Vector2(-d.y, d.x);
			return n.sqrMagnitude > 1e-6f ? n.normalized : Vector2.zero;
		}

		private Vector2 SpawnDirection(Vector2 p)
		{
			float baseAngle = shape.angle;
			if (shape.direction == TextParticlesDirection.Outward)
			{
				Vector2 d = p - spawnCenter;
				baseAngle = d.sqrMagnitude > 1e-4f ? Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg : R() * 360f;
			}
			else if (shape.direction == TextParticlesDirection.Sideways)
			{
				baseAngle = p.x >= spawnCenter.x ? shape.angle : 180f - shape.angle;
			}
			else if (shape.direction == TextParticlesDirection.ShapeNormal)
			{
				Vector2 d = spawnNormal.sqrMagnitude > 1e-6f ? spawnNormal : p - spawnCenter;
				baseAngle = d.sqrMagnitude > 1e-4f ? Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg : R() * 360f;
			}
			float a = (baseAngle + (R() - 0.5f) * shape.spread) * Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
		}
		#endregion

		#region TEXT EVENTS
		protected override void OnEnable()
		{
			base.OnEnable();
			EnsureShaderChannels();
			Subscribe();
			if (Application.isPlaying && textLink.playOn == TextParticlesPlayOn.OnEnable) { Play(true); }
		}

		protected override void OnDisable()
		{
			Unsubscribe();
			DestroyProxy();
			count = 0;
			emitting = false;
			letterSchedule.Clear();
			base.OnDisable();
		}

		private void Subscribe()
		{
			Unsubscribe();
			if (emission.perParentParticle && transform.parent != null)
			{
				AllIn1TextParticles parent = transform.parent.GetComponentInParent<AllIn1TextParticles>();
				if (parent != null)
				{
					parent.OnEmitted += HandleParentEmitted;
					subscribedParent = parent;
				}
			}
			AllIn1TextTypewriter tw = LinkedTypewriter;
			if (tw == null) { return; }
			tw.OnRevealStarted += HandleRevealStarted;
			tw.OnRevealCompleted += HandleRevealCompleted;
			tw.OnHideStarted += HandleHideStarted;
			tw.OnHideCompleted += HandleHideCompleted;
			tw.OnCharacterRevealStarted += HandleCharacterRevealStarted;
			subscribedTypewriter = tw;
		}

		private void Unsubscribe()
		{
			if (subscribedParent != null)
			{
				subscribedParent.OnEmitted -= HandleParentEmitted;
				subscribedParent = null;
			}
			if (subscribedTypewriter == null) { return; }
			subscribedTypewriter.OnRevealStarted -= HandleRevealStarted;
			subscribedTypewriter.OnRevealCompleted -= HandleRevealCompleted;
			subscribedTypewriter.OnHideStarted -= HandleHideStarted;
			subscribedTypewriter.OnHideCompleted -= HandleHideCompleted;
			subscribedTypewriter.OnCharacterRevealStarted -= HandleCharacterRevealStarted;
			subscribedTypewriter = null;
		}

		private void HandleRevealStarted() => HandleTextEvent(TextParticlesPlayOn.RevealStarted, TextParticlesStopOn.Never);
		private void HandleRevealCompleted() => HandleTextEvent(TextParticlesPlayOn.RevealCompleted, TextParticlesStopOn.Never);
		private void HandleHideStarted() => HandleTextEvent(TextParticlesPlayOn.HideStarted, TextParticlesStopOn.HideStarted);
		private void HandleHideCompleted() => HandleTextEvent(TextParticlesPlayOn.HideCompleted, TextParticlesStopOn.HideCompleted);

		private void HandleTextEvent(TextParticlesPlayOn playEvent, TextParticlesStopOn stopEvent)
		{
			if (stopEvent != TextParticlesStopOn.Never && textLink.stopOn == stopEvent) { Stop(true, textLink.clearOnStop); }
			if (textLink.playOn == playEvent) { Play(true); }
		}

		private void HandleParentEmitted(int n)
		{
			if (!emission.enabled || paused) { return; }
			int total = 0;
			for (int i = 0; i < n; i++) { total += Mathf.RoundToInt(emission.perParentCount.Lerp(R())); }
			EmitBatch(total, -1);
		}

		private void HandleCharacterRevealStarted(char c, int charIndex)
		{
			if (!emission.enabled || !emission.perLetter || paused) { return; }
			EmitBatch(PerLetterCount(), charIndex);
		}

		//The particle mesh carries its HDR intensity in uv1
		private void EnsureShaderChannels()
		{
			Canvas c = canvas;
			if (c == null) { return; }
			const AdditionalCanvasShaderChannels NEEDED = AdditionalCanvasShaderChannels.TexCoord1;
			if ((c.additionalShaderChannels & NEEDED) != NEEDED) { c.additionalShaderChannels |= NEEDED; }
			Canvas root = c.rootCanvas;
			if (root != null && (root.additionalShaderChannels & NEEDED) != NEEDED) { root.additionalShaderChannels |= NEEDED; }
		}

		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();
			if (isActiveAndEnabled)
			{
				EnsureShaderChannels();
				Subscribe();
			}
		}
		#endregion

		#region DRAWING
		public override Texture mainTexture => rendererModule.texture != null ? rendererModule.texture : s_WhiteTexture;

		public override Material defaultMaterial
		{
			get
			{
				Shader s = particleShader != null ? particleShader : Shader.Find(SHADER_NAME);
				if (s == null) { return base.defaultMaterial; }
				int key = s.GetInstanceID() * 4 + (int)rendererModule.blend;
				if (!sharedMaterials.TryGetValue(key, out Material m) || m == null)
				{
					m = new Material(s);
					m.name = "TextParticles " + rendererModule.blend;
					m.hideFlags = HideFlags.HideAndDontSave;
					m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
					m.SetFloat("_DstBlend", (float)(rendererModule.blend == TextParticlesBlend.Additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
					sharedMaterials[key] = m;
				}
				return m;
			}
		}

		private int FrameCount()
		{
			if (!textureSheet.enabled) { return 1; }
			return Mathf.Max(1, textureSheet.tilesX) * Mathf.Max(1, textureSheet.tilesY);
		}

		private float ResolveAspect()
		{
			if (rendererModule.aspect > 0f) { return rendererModule.aspect; }
			Texture tex = rendererModule.texture;
			if (tex == null || tex.height == 0) { return 1f; }
			float tilesX = textureSheet.enabled ? Mathf.Max(1, textureSheet.tilesX) : 1f;
			float tilesY = textureSheet.enabled ? Mathf.Max(1, textureSheet.tilesY) : 1f;
			return (tex.width / tilesX) / (tex.height / tilesY);
		}

		private void EnsureLifeState()
		{
			MeshEffectGroupConfig preset = lifePreset.enabled ? lifePreset.preset : null;
			if (!lifeStateDirty && preset == builtLifePreset && (preset == null || lifeLayouts.Length == particles.Length)) { return; }
			lifeStateDirty = false;
			builtLifePreset = preset;
			lifeState = null;
			if (preset == null) { return; }

			int n = particles.Length;
			lifeLayouts = new VertexGroupLayout[n];
			for (int q = 0; q < n; q++)
			{
				VertexGroupLayout layout = new VertexGroupLayout();
				layout.groupIndex = q;
				layout.startVertexIndex = q * 4;
				layout.endVertexIndex = q * 4 + 3;
				layout.enabledEffectsMask = EffectFlag.All;
				layout.playbackMode = PlaybackMode.ONE_SHOT;
				lifeLayouts[q] = layout;
			}
			lifeState = new MeshEffectGroupState(preset, ref lifeLayouts, PlaybackMode.ONE_SHOT, GroupType.PER_CHARACTER, EffectFlag.All, "TextParticlesLife");
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			if (proxy != null) { return; }
			PopulateParticles(vh, Matrix4x4.identity);
		}

		public override void SetVerticesDirty()
		{
			base.SetVerticesDirty();
			if (proxy != null) { proxy.SetVerticesDirty(); }
		}

		public override void SetMaterialDirty()
		{
			base.SetMaterialDirty();
			if (proxy != null) { proxy.SetMaterialDirty(); }
		}

		//Writes the particle quads into vh; toTarget maps this object's local space into the graphic that draws them (identity, or the draw stand in)
		internal void PopulateParticles(VertexHelper vh, Matrix4x4 toTarget)
		{
			if (count == 0) { return; }
			bool transformOut = toTarget != Matrix4x4.identity;

			RefreshSpace();
			EnsureLifeState();

			int tilesX = textureSheet.enabled ? Mathf.Max(1, textureSheet.tilesX) : 1;
			int tilesY = textureSheet.enabled ? Mathf.Max(1, textureSheet.tilesY) : 1;
			int frames = tilesX * tilesY;
			float aspect = ResolveAspect();
			float intensity = Mathf.Max(0.0001f, rendererModule.intensity);
			Color tint = color;
			Vector3 normal = Vector3.back;
			Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

			for (int i = 0; i < count; i++)
			{
				Particle p = particles[i];
				float k = Mathf.Clamp01(p.age / p.life);

				float size = p.size;
				if (sizeOverLifetime.enabled && sizeOverLifetime.size != null) { size *= sizeOverLifetime.size.Evaluate(k); }
				if (size <= 0f) { continue; }

				Color c = p.color * tint;
				if (colorOverLifetime.enabled && colorOverLifetime.color != null) { c *= colorOverLifetime.color.Evaluate(k); }
				if (flicker.enabled) { c.a *= 1f - flicker.amount * Mathf.PerlinNoise(p.random * 53f, p.age * flicker.speed); }
				if (c.a <= 0f) { continue; }

				Vector2 pos = p.pos;
				Vector2 heading = p.vel;
				if (forces.enabled && forces.sway != 0f)
				{
					float phase = (p.age * forces.swayFrequency + p.random) * Mathf.PI * 2f;
					pos.x += Mathf.Sin(phase) * forces.sway;
					//The drawn path includes the sway, so a particle turned to its motion leans with it
					heading.x += Mathf.Cos(phase) * forces.sway * forces.swayFrequency * Mathf.PI * 2f;
				}

				float w = size * aspect;
				if (rotationOverLifetime.enabled && rotationOverLifetime.flutter > 0f)
				{
					float turn = Mathf.Abs(Mathf.Cos((p.age * rotationOverLifetime.flutterSpeed + p.random) * Mathf.PI));
					w *= 1f - rotationOverLifetime.flutter * (1f - turn);
				}
				float h = size;
				float angle = p.rotation * Mathf.Deg2Rad;
				if (rendererModule.alignToVelocity && heading.sqrMagnitude > 1e-4f)
				{
					angle = Mathf.Atan2(heading.y, heading.x);
					w += heading.magnitude * rendererModule.velocityStretch;
				}
				float cos = Mathf.Cos(angle);
				float sin = Mathf.Sin(angle);
				Vector2 ax = new Vector2(cos, sin) * (0.5f * w * p.flipX);
				Vector2 ay = new Vector2(-sin, cos) * (0.5f * h * p.flipY);
				pos -= 2f * (rendererModule.pivot.x * ax + rendererModule.pivot.y * ay);

				quadVerts[0] = simToLocal.MultiplyPoint3x4(pos - ax - ay);
				quadVerts[1] = simToLocal.MultiplyPoint3x4(pos - ax + ay);
				quadVerts[2] = simToLocal.MultiplyPoint3x4(pos + ax + ay);
				quadVerts[3] = simToLocal.MultiplyPoint3x4(pos + ax - ay);

				float hdr = Mathf.Max(1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)));
				Color32 c32 = new Color(c.r / hdr, c.g / hdr, c.b / hdr, Mathf.Clamp01(c.a));
				for (int v = 0; v < 4; v++) { quadColors[v] = c32; }

				if (lifeState != null && i < lifeLayouts.Length) { ApplyLifePreset(i, k); }
				if (transformOut)
				{
					for (int v = 0; v < 4; v++) { quadVerts[v] = toTarget.MultiplyPoint3x4(quadVerts[v]); }
				}

				int frame = p.frame;
				if (textureSheet.enabled && textureSheet.mode == TextParticlesFrameMode.OverLifetime)
				{
					frame = Mathf.FloorToInt(k * frames * Mathf.Max(0.01f, textureSheet.cycles)) % frames;
				}
				frame = Mathf.Clamp(frame, 0, frames - 1);
				float u0 = (frame % tilesX) / (float)tilesX;
				float v0 = 1f - (frame / tilesX + 1) / (float)tilesY;
				float u1 = u0 + 1f / tilesX;
				float v1 = v0 + 1f / tilesY;

				Vector4 uv1 = new Vector4(hdr * intensity, 0f, 0f, 0f);
				int start = vh.currentVertCount;
				vh.AddVert(quadVerts[0], quadColors[0], new Vector4(u0, v0, 0f, 0f), uv1, normal, tangent);
				vh.AddVert(quadVerts[1], quadColors[1], new Vector4(u0, v1, 0f, 0f), uv1, normal, tangent);
				vh.AddVert(quadVerts[2], quadColors[2], new Vector4(u1, v1, 0f, 0f), uv1, normal, tangent);
				vh.AddVert(quadVerts[3], quadColors[3], new Vector4(u1, v0, 0f, 0f), uv1, normal, tangent);
				vh.AddTriangle(start, start + 1, start + 2);
				vh.AddTriangle(start + 2, start + 3, start);
			}
		}

		//Same steps as the spline's enter and exit zones: pivots from the live quad, each effect in the preset's order
		private void ApplyLifePreset(int index, float progress)
		{
			ref VertexGroupLayout layout = ref lifeLayouts[index];
			List<MeshEffectState> states = lifeState.states;
			for (int s = 0; s < states.Count; s++)
			{
				layout.center = (quadVerts[0] + quadVerts[2]) * 0.5f;
				layout.centerTop = (quadVerts[1] + quadVerts[2]) * 0.5f;
				layout.SetRight(quadVerts[0], quadVerts[3]);
				states[s].UpdateAtProgress(ref layout, VertexGroupDisplayState.Revealing, progress);
				for (int v = 0; v < 4; v++)
				{
					quadVerts[v] = states[s].TransformVertex(ref layout, quadVerts[v]);
					quadColors[v] = states[s].TransformColor(ref layout, quadColors[v]);
				}
			}
		}
		#endregion

		#region DRAW ORDER
		//UGUI draws in hierarchy order, so when this object's own place is on the wrong side of the text, a hidden stand in next to the text draws the particles
		private void UpdateDrawRoute()
		{
			TMP_Text t = LinkedText;
			bool wantFront = rendererModule.drawOrder == TextParticlesDrawOrder.InFront;
			bool useProxy = t != null && t.transform.parent != null && DrawsAfter(transform, t.transform) != wantFront;
			if (!useProxy)
			{
				if (proxy != null)
				{
					DestroyProxy();
					SetVerticesDirty();
				}
				return;
			}
			if (proxy == null)
			{
				proxy = TextParticlesDrawProxy.Create(this);
				SetVerticesDirty();
				SetMaterialDirty();
			}
			proxy.Place(t.transform, !wantFront);
		}

		//Depth first: a parent before its children, siblings by index
		private static bool DrawsAfter(Transform a, Transform b)
		{
			if (a == b) { return false; }
			FillPath(a, pathA);
			FillPath(b, pathB);
			int n = Mathf.Min(pathA.Count, pathB.Count);
			for (int i = 0; i < n; i++)
			{
				if (pathA[i] != pathB[i]) { return pathA[i] > pathB[i]; }
			}
			return pathA.Count > pathB.Count;
		}

		private static void FillPath(Transform t, List<int> path)
		{
			path.Clear();
			for (Transform x = t; x != null; x = x.parent) { path.Add(x.GetSiblingIndex()); }
			path.Reverse();
		}

		private void DestroyProxy()
		{
			if (proxy == null) { return; }
			GameObject go = proxy.gameObject;
			proxy = null;
			if (go == null) { return; }
			if (Application.isPlaying) { Destroy(go); }
			else { DestroyImmediate(go); }
		}
		#endregion

#if UNITY_EDITOR
		private void DeferredDrawRoute()
		{
			if (this != null && isActiveAndEnabled) { UpdateDrawRoute(); }
		}

		protected override void Reset()
		{
			base.Reset();
			raycastTarget = false;
			particleShader = Shader.Find(SHADER_NAME);
			RectTransform rt = rectTransform;
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.sizeDelta = Vector2.zero;
			rt.anchoredPosition = Vector2.zero;
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			main.maxParticles = Mathf.Clamp(main.maxParticles, 1, MAX_PARTICLES);
			if (particleShader == null) { particleShader = Shader.Find(SHADER_NAME); }
			lifeStateDirty = true;
			SetMaterialDirty();
			if (isActiveAndEnabled) { Subscribe(); }
			UnityEditor.EditorApplication.delayCall -= DeferredDrawRoute;
			UnityEditor.EditorApplication.delayCall += DeferredDrawRoute;
		}
#endif
	}
}
