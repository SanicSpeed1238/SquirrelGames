using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public enum TextParticlesSpace
	{
		Local,
		Canvas,
	}

	public enum TextParticlesPlayOn
	{
		[InspectorName("On Enable")] OnEnable,
		[InspectorName("Reveal Started")] RevealStarted,
		[InspectorName("Reveal Completed")] RevealCompleted,
		[InspectorName("Hide Started")] HideStarted,
		[InspectorName("Hide Completed")] HideCompleted,
		[InspectorName("Parent or Code")] Manual,
	}

	public enum TextParticlesStopOn
	{
		Never,
		[InspectorName("Hide Started")] HideStarted,
		[InspectorName("Hide Completed")] HideCompleted,
	}

	public enum TextParticlesShape
	{
		Point,
		Circle,
		Box,
		[InspectorName("Text Box")] TextBox,
		Letters,
		[InspectorName("Letter Tops")] LetterTops,
		[InspectorName("Letter Bottoms")] LetterBottoms,
	}

	public enum TextParticlesDirection
	{
		Angle,
		Outward,
		Sideways,
		[InspectorName("Shape Normal")] ShapeNormal,
	}

	public enum TextParticlesBlend
	{
		Additive,
		[InspectorName("Alpha Blend")] AlphaBlend,
	}

	public enum TextParticlesDrawOrder
	{
		[InspectorName("In Front of Text")] InFront,
		[InspectorName("Behind Text")] Behind,
	}

	public enum TextParticlesFrameMode
	{
		[InspectorName("Over Lifetime")] OverLifetime,
		[InspectorName("Random Frame")] RandomFrame,
		[InspectorName("Crackle (new random frame at FPS)")] Crackle,
	}

	[Serializable]
	public struct TextParticlesRange
	{
		public float min;
		public float max;

		public TextParticlesRange(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float Lerp(float t) => min + (max - min) * t;
	}

	[Serializable]
	public struct TextParticlesBurst
	{
		[Tooltip("Seconds into the emission cycle")] public float time;
		public TextParticlesRange count;
		[Tooltip("How many times the burst repeats; 0 or 1 fires once")] public int cycles;
		[Tooltip("Seconds between repeats")] public float interval;
	}

	[Serializable]
	public class TextParticlesMain
	{
		[Tooltip("Length of one emission cycle in seconds")] public float duration = 5f;
		[Tooltip("Restarts the emission cycle when it ends")] public bool looping = true;
		[Tooltip("A looping system starts as if it had already run one full cycle")] public bool prewarm;
		[Tooltip("Seconds between Play and the first particle")] public float startDelay;
		[Tooltip("Seconds a particle lives")] public TextParticlesRange startLifetime = new TextParticlesRange(1f, 1.5f);
		[Tooltip("Above 0, a particle lives this distance over its start speed instead, so slow and fast ones end at the same place")]
		public TextParticlesRange travelDistance = new TextParticlesRange(0f, 0f);
		[Tooltip("Units per second, along the direction the Shape gives")] public TextParticlesRange startSpeed = new TextParticlesRange(60f, 120f);
		[Tooltip("Height in canvas units; the width follows the texture's aspect (Renderer)")] public TextParticlesRange startSize = new TextParticlesRange(10f, 16f);
		[Tooltip("Degrees")] public TextParticlesRange startRotation = new TextParticlesRange(0f, 0f);
		[Tooltip("Each particle picks a random color along this gradient. HDR colors glow with bloom")]
		[GradientUsage(true)] public Gradient startColor = new Gradient();
		[Tooltip("Local: particles move with this object. Canvas: they stay where they were born when it moves, leaving trails")]
		public TextParticlesSpace simulationSpace = TextParticlesSpace.Local;
		public float simulationSpeed = 1f;
		public int maxParticles = 200;
		[Tooltip("0: a new pattern on every play, kept while scrubbing. Any other value: the same particles on every play")] public int seed;
	}

	[Serializable]
	public class TextParticlesTextLink
	{
		[Tooltip("The text whose letters the shapes and triggers use. Empty uses the text on a parent")] public TMP_Text text;
		[Tooltip("When emission starts. Parent or Code: when a parent emitter plays, or Play() from a script")]
		public TextParticlesPlayOn playOn = TextParticlesPlayOn.OnEnable;
		public TextParticlesStopOn stopOn = TextParticlesStopOn.Never;
		[Tooltip("Stopping also removes the particles already alive")] public bool clearOnStop;
	}

	[Serializable]
	public class TextParticlesEmission
	{
		public bool enabled = true;
		[Tooltip("Particles per second while playing")] public float rateOverTime = 10f;
		public List<TextParticlesBurst> bursts = new List<TextParticlesBurst>();
		[Tooltip("Emits at each letter the moment the typewriter starts revealing it, playing or not")] public bool perLetter;
		public TextParticlesRange perLetterCount = new TextParticlesRange(4f, 6f);
		[Tooltip("Sub emitter: emits for every particle the nearest Text Particles above this object emits, at this emitter's own shape")]
		public bool perParentParticle;
		public TextParticlesRange perParentCount = new TextParticlesRange(1f, 1f);
	}

	[Serializable]
	public class TextParticlesShapeModule
	{
		public TextParticlesShape shape = TextParticlesShape.Point;
		public float radius = 50f;
		public Vector2 boxSize = new Vector2(200f, 100f);
		[Tooltip("Emit from the outline instead of the inside")] public bool edgeOnly;
		[Tooltip("Moves the shape; for letter shapes it moves the spawn points")] public Vector2 offset;
		[Tooltip("Letter shapes skip letters that are hidden or still collapsed by the typewriter")] public bool revealedLettersOnly = true;
		[Tooltip("Angle: a fixed direction. Outward: away from the shape's center (a letter's own center for per letter emission). Sideways: left or right away from the center. Shape Normal: straight out of the edge a particle is born on (Edge Only shapes, letter tops and bottoms, circles), Outward elsewhere")]
		public TextParticlesDirection direction = TextParticlesDirection.Angle;
		[Tooltip("Angle: 0 is right, 90 is up. Sideways: tilt up from horizontal, mirrored on the left")] public float angle = 90f;
		[Tooltip("Random spread around the direction, in degrees")] public float spread = 30f;
	}

	[Serializable]
	public class TextParticlesForces
	{
		public bool enabled;
		[Tooltip("Units per second squared, in the simulation space. Positive Y rises like hot air")] public Vector2 gravity = new Vector2(0f, -200f);
		[Tooltip("How fast speed dies off, per second")] public float drag;
		[Tooltip("Random wandering, units per second squared")] public float turbulence;
		public float turbulenceFrequency = 1f;
		[Tooltip("Sideways wobble in units, drawn on top of the motion")] public float sway;
		[Tooltip("Wobbles per second")] public float swayFrequency = 1f;
	}

	[Serializable]
	public class TextParticlesSizeOverLifetime
	{
		public bool enabled;
		[Tooltip("Multiplies the start size; X is the particle's life from 0 to 1")] public AnimationCurve size = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 1f), new Keyframe(1f, 0f));
	}

	[Serializable]
	public class TextParticlesColorOverLifetime
	{
		public bool enabled;
		[Tooltip("Multiplies the start color over the particle's life")] [GradientUsage(true)] public Gradient color = new Gradient();
	}

	[Serializable]
	public class TextParticlesRotationOverLifetime
	{
		public bool enabled;
		[Tooltip("Degrees per second, picked per particle")] public TextParticlesRange speed = new TextParticlesRange(-90f, 90f);
		[Tooltip("Squashes the width back and forth like a leaf turning over. 0 is off, 1 turns it fully edge on")]
		[Range(0f, 1f)] public float flutter;
		[Tooltip("Turns per second")] public float flutterSpeed = 0.4f;
	}

	[Serializable]
	public class TextParticlesFlicker
	{
		public bool enabled;
		[Range(0f, 1f)] public float amount = 0.35f;
		[Tooltip("How fast the brightness wanders")] public float speed = 20f;
	}

	[Serializable]
	public class TextParticlesTextureSheet
	{
		public bool enabled;
		[Tooltip("Columns of frames in the texture")] public int tilesX = 1;
		[Tooltip("Rows of frames in the texture")] public int tilesY = 1;
		public TextParticlesFrameMode mode = TextParticlesFrameMode.OverLifetime;
		[Tooltip("Crackle: frames per second")] public float fps = 20f;
		[Tooltip("Over Lifetime: times the frames play during a life")] public float cycles = 1f;
	}

	[Serializable]
	public class TextParticlesLifePreset
	{
		public bool enabled;
		[Tooltip("A reveal preset played over each particle's life, the way a letter reveals")] public MeshEffectGroupConfig preset;
	}

	[Serializable]
	public class TextParticlesRenderer
	{
		[Tooltip("Which side of the linked text the particles draw on, wherever this object sits in the hierarchy")]
		public TextParticlesDrawOrder drawOrder = TextParticlesDrawOrder.InFront;
		public Texture texture;
		public TextParticlesBlend blend = TextParticlesBlend.Additive;
		[Tooltip("Brightness multiplier; above 1 reaches the bloom threshold")] public float intensity = 1f;
		[Tooltip("Width over height. 0 uses the texture's (one frame's) aspect")] public float aspect;
		[Tooltip("Where the particle's position sits on its quad: (0, 0) the center, (-0.5, 0) the left end, so a particle turned to its motion grows out from where it was born")]
		public Vector2 pivot;
		[Tooltip("Turns each particle to face where it moves")] public bool alignToVelocity;
		[Tooltip("Extra length per unit of speed, in seconds")] public float velocityStretch;
		public bool randomFlipX;
		public bool randomFlipY;
	}
}
