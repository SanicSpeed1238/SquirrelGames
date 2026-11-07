using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//One click setups for the inspector's Presets menu, drawn with the textures in the plugin's Textures/Particles folder
	public static class TextParticlesPresets
	{
		private const string TEXTURES_FOLDER_NAME = "Textures";
		private const string PARTICLES_FOLDER_NAME = "Particles";
		private const string DEFAULT_REVEAL_FILE = "RevealConfig_DEFAULT.asset";

		public static readonly string[] NAMES =
		{
			"Embers",
			"Torch Embers",
			"Sparks Burst",
			"Dust Puff",
			"Shockwave Flash",
			"Typing Sparks",
			"Floating Motes",
			"Electric Bolts",
			"Static Sparks",
			"Glints",
			"Hearts",
		};

		public static void Apply(AllIn1TextParticles e, string name)
		{
			var text = e.textLink.text;
			e.main = new TextParticlesMain();
			e.textLink = new TextParticlesTextLink { text = text };
			e.emission = new TextParticlesEmission();
			e.shape = new TextParticlesShapeModule();
			e.forces = new TextParticlesForces();
			e.sizeOverLifetime = new TextParticlesSizeOverLifetime();
			e.colorOverLifetime = new TextParticlesColorOverLifetime();
			e.rotationOverLifetime = new TextParticlesRotationOverLifetime();
			e.flicker = new TextParticlesFlicker();
			e.textureSheet = new TextParticlesTextureSheet();
			e.lifePreset = new TextParticlesLifePreset();
			e.rendererModule = new TextParticlesRenderer();

			switch (name)
			{
				case "Embers": Embers(e); break;
				case "Torch Embers": TorchEmbers(e); break;
				case "Sparks Burst": SparksBurst(e); break;
				case "Dust Puff": DustPuff(e); break;
				case "Shockwave Flash": ShockwaveFlash(e); break;
				case "Typing Sparks": TypingSparks(e); break;
				case "Floating Motes": FloatingMotes(e); break;
				case "Electric Bolts": ElectricBolts(e); break;
				case "Static Sparks": StaticSparks(e); break;
				case "Glints": Glints(e); break;
				case "Hearts": Hearts(e); break;
			}
		}

		//FIRE!'s embers: hot dots and dashes rising off the letter tops, wandering, cooling and shrinking away
		private static void Embers(AllIn1TextParticles e)
		{
			e.main.duration = 4f;
			e.main.prewarm = true;
			e.main.startLifetime = R(1.2f, 2.4f);
			e.main.startSpeed = R(40f, 110f);
			e.main.startSize = R(7f, 11.5f);
			e.main.startColor = Grad(new Color(1.8f, 1.6f, 1.46f), new Color(2.5f, 2.92f, 3.24f));
			e.emission.rateOverTime = 22f;
			e.shape.shape = TextParticlesShape.LetterTops;
			e.shape.spread = 40f;
			e.forces.enabled = true;
			e.forces.gravity = new Vector2(0f, 60f);
			e.forces.drag = 0.4f;
			e.forces.turbulence = 90f;
			e.forces.turbulenceFrequency = 1.2f;
			e.sizeOverLifetime.enabled = true;
			e.sizeOverLifetime.size = Curve(0f, 0f, 0.12f, 1f, 1f, 0f);
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (new Color(1f, 0.55f, 0.35f), 0.5f), (new Color(0.8f, 0.2f, 0.1f), 1f) }, new[] { (0f, 0f), (1f, 0.06f), (1f, 0.6f), (0f, 1f) });
			e.flicker.enabled = true;
			e.flicker.amount = 0.3f;
			e.flicker.speed = 18f;
			//Dots and dashes baked from the demo's text embers, a dash in every three or so
			e.textureSheet.enabled = true;
			e.textureSheet.tilesX = 10;
			e.textureSheet.mode = TextParticlesFrameMode.RandomFrame;
			e.rendererModule.texture = Tex("EmberSheet");
			e.rendererModule.alignToVelocity = true;
		}

		//The torch's sparks: born at a point that moves, kept in canvas space so they trail behind it, buoyant, swaying, yellow to deep red
		private static void TorchEmbers(AllIn1TextParticles e)
		{
			e.main.duration = 3f;
			e.main.startLifetime = R(0.8f, 1.7f);
			e.main.startSpeed = R(70f, 150f);
			e.main.startSize = R(4f, 9f);
			e.main.startColor = Grad(new Color(2.2f, 1.8f, 1f), new Color(2.2f, 1.8f, 1f));
			e.main.simulationSpace = TextParticlesSpace.Canvas;
			e.main.maxParticles = 120;
			e.emission.rateOverTime = 14f;
			e.shape.shape = TextParticlesShape.Circle;
			e.shape.radius = 14f;
			e.shape.spread = 45f;
			e.forces.enabled = true;
			e.forces.gravity = new Vector2(0f, 40f);
			e.forces.drag = 0.8f;
			e.forces.sway = 6f;
			e.forces.swayFrequency = 0.8f;
			e.sizeOverLifetime.enabled = true;
			e.sizeOverLifetime.size = Curve(0f, 1f, 1f, 0.4f);
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (new Color(0.45f, 0.14f, 0.03f), 1f) }, new[] { (0f, 0f), (1f, 0.08f), (0.25f, 0.5f), (0f, 1f) });
			e.rendererModule.texture = Tex("Glow");
		}

		//GO!'s fountain of sparks when it lands: fast streaks thrown up and pulled down, stretched along their motion
		private static void SparksBurst(AllIn1TextParticles e)
		{
			e.main.duration = 1f;
			e.main.looping = false;
			e.main.startLifetime = R(0.35f, 0.8f);
			e.main.startSpeed = R(250f, 650f);
			e.main.startSize = R(3f, 6f);
			e.main.startColor = Grad(new Color(3f, 2.6f, 1.6f), new Color(3f, 1.9f, 0.8f));
			e.main.maxParticles = 120;
			e.textLink.playOn = TextParticlesPlayOn.RevealCompleted;
			e.emission.rateOverTime = 0f;
			e.emission.bursts.Add(new TextParticlesBurst { time = 0f, count = R(40f, 55f), cycles = 1, interval = 0.1f });
			e.shape.shape = TextParticlesShape.LetterBottoms;
			e.shape.spread = 150f;
			e.forces.enabled = true;
			e.forces.gravity = new Vector2(0f, -900f);
			e.forces.drag = 1.2f;
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (new Color(1f, 0.5f, 0.2f), 1f) }, new[] { (1f, 0f), (1f, 0.5f), (0f, 1f) });
			e.rendererModule.texture = Tex("Glow");
			e.rendererModule.aspect = 1f;
			e.rendererModule.alignToVelocity = true;
			e.rendererModule.velocityStretch = 0.035f;
		}

		//GO!'s dust: soft clouds thrown sideways along the ground, growing and thinning out
		private static void DustPuff(AllIn1TextParticles e)
		{
			e.main.duration = 0.5f;
			e.main.looping = false;
			e.main.startLifetime = R(0.5f, 0.8f);
			e.main.startSpeed = R(220f, 380f);
			e.main.startSize = R(50f, 90f);
			e.main.startColor = Grad(new Color(0.92f, 0.93f, 0.85f, 0.55f), new Color(0.85f, 0.82f, 0.75f, 0.55f));
			e.main.maxParticles = 40;
			e.textLink.playOn = TextParticlesPlayOn.RevealCompleted;
			e.emission.rateOverTime = 0f;
			e.emission.bursts.Add(new TextParticlesBurst { time = 0f, count = R(10f, 14f), cycles = 1, interval = 0.1f });
			e.shape.shape = TextParticlesShape.LetterBottoms;
			e.shape.direction = TextParticlesDirection.Sideways;
			e.shape.angle = 8f;
			e.shape.spread = 14f;
			e.forces.enabled = true;
			e.forces.gravity = new Vector2(0f, 25f);
			e.forces.drag = 5f;
			e.sizeOverLifetime.enabled = true;
			e.sizeOverLifetime.size = Curve(0f, 0.35f, 0.4f, 1.05f, 1f, 1.3f);
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (Color.white, 1f) }, new[] { (1f, 0f), (0.3f, 0.5f), (0f, 1f) });
			e.rotationOverLifetime.enabled = true;
			e.rotationOverLifetime.speed = R(-40f, 40f);
			e.rendererModule.drawOrder = TextParticlesDrawOrder.Behind;
			e.rendererModule.texture = Tex("Haze");
			e.rendererModule.blend = TextParticlesBlend.AlphaBlend;
		}

		//One wide flat flash on the ground under a landing word
		private static void ShockwaveFlash(AllIn1TextParticles e)
		{
			e.main.duration = 0.3f;
			e.main.looping = false;
			e.main.startLifetime = R(0.3f, 0.3f);
			e.main.startSpeed = R(0f, 0f);
			e.main.startSize = R(70f, 70f);
			e.main.startColor = Grad(new Color(2f, 2.2f, 1.4f), new Color(2f, 2.2f, 1.4f));
			e.main.maxParticles = 4;
			e.textLink.playOn = TextParticlesPlayOn.RevealCompleted;
			e.emission.rateOverTime = 0f;
			e.emission.bursts.Add(new TextParticlesBurst { time = 0f, count = R(1f, 1f), cycles = 1, interval = 0.1f });
			e.sizeOverLifetime.enabled = true;
			e.sizeOverLifetime.size = Curve(0f, 0.5f, 1f, 1.4f);
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (Color.white, 1f) }, new[] { (1f, 0f), (0f, 1f) });
			e.rendererModule.drawOrder = TextParticlesDrawOrder.Behind;
			e.rendererModule.texture = Tex("Ellipse");
			e.rendererModule.aspect = 6f;
		}

		//A spray at each letter the moment the typewriter starts it
		private static void TypingSparks(AllIn1TextParticles e)
		{
			e.main.duration = 1f;
			e.main.looping = false;
			e.main.startLifetime = R(0.3f, 0.7f);
			e.main.startSpeed = R(90f, 260f);
			e.main.startSize = R(2.5f, 5f);
			e.main.startColor = Grad(new Color(2.6f, 2f, 0.8f), new Color(2.4f, 2.4f, 2.4f));
			e.main.maxParticles = 300;
			e.textLink.playOn = TextParticlesPlayOn.Manual;
			e.emission.rateOverTime = 0f;
			e.emission.perLetter = true;
			e.emission.perLetterCount = R(6f, 9f);
			e.shape.shape = TextParticlesShape.Letters;
			e.shape.revealedLettersOnly = false;
			e.shape.direction = TextParticlesDirection.Outward;
			e.shape.spread = 60f;
			e.forces.enabled = true;
			e.forces.gravity = new Vector2(0f, -500f);
			e.forces.drag = 2f;
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (new Color(1f, 0.6f, 0.3f), 1f) }, new[] { (1f, 0f), (1f, 0.4f), (0f, 1f) });
			e.rendererModule.texture = Tex("Glow");
			e.rendererModule.aspect = 1f;
			e.rendererModule.alignToVelocity = true;
			e.rendererModule.velocityStretch = 0.02f;
		}

		//Dust hanging in the air: slow wandering dots that twinkle, never ending
		private static void FloatingMotes(AllIn1TextParticles e)
		{
			e.main.duration = 6f;
			e.main.prewarm = true;
			e.main.startLifetime = R(6f, 10f);
			e.main.startSpeed = R(2f, 10f);
			e.main.startSize = R(2f, 5f);
			e.main.startColor = Grad(new Color(1f, 0.85f, 0.65f, 0.55f), new Color(1f, 0.95f, 0.85f, 0.4f));
			e.main.maxParticles = 60;
			e.emission.rateOverTime = 6f;
			e.shape.shape = TextParticlesShape.Box;
			e.shape.boxSize = new Vector2(560f, 380f);
			e.shape.spread = 360f;
			e.forces.enabled = true;
			e.forces.gravity = new Vector2(0f, 3f);
			e.forces.turbulence = 12f;
			e.forces.turbulenceFrequency = 0.4f;
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (Color.white, 1f) }, new[] { (0f, 0f), (1f, 0.2f), (1f, 0.8f), (0f, 1f) });
			e.flicker.enabled = true;
			e.flicker.amount = 0.5f;
			e.flicker.speed = 2f;
			e.rendererModule.texture = Tex("Glow");
			e.rendererModule.intensity = 1.3f;
			e.rendererModule.drawOrder = TextParticlesDrawOrder.Behind;
		}

		//SHOCK!'s lightning: bolts flashing on the letters at random angles, crackling through the frames of a sheet
		private static void ElectricBolts(AllIn1TextParticles e)
		{
			e.main.duration = 2f;
			e.main.startLifetime = R(0.12f, 0.36f);
			e.main.startSpeed = R(0f, 0f);
			e.main.startSize = R(26f, 40f);
			e.main.startRotation = R(-180f, 180f);
			e.main.maxParticles = 40;
			e.emission.rateOverTime = 14f;
			e.shape.shape = TextParticlesShape.Letters;
			e.flicker.enabled = true;
			e.flicker.amount = 0.35f;
			e.flicker.speed = 40f;
			e.textureSheet.enabled = true;
			e.textureSheet.tilesX = 4;
			e.textureSheet.tilesY = 2;
			e.textureSheet.mode = TextParticlesFrameMode.Crackle;
			e.textureSheet.fps = 22f;
			e.rendererModule.drawOrder = TextParticlesDrawOrder.Behind;
			e.rendererModule.texture = Tex("BoltSheet");
			e.rendererModule.intensity = 2.4f;
			e.rendererModule.randomFlipY = true;
		}

		//SHOCK!'s tiny sparks: pinpricks blinking on the letters
		private static void StaticSparks(AllIn1TextParticles e)
		{
			e.main.duration = 2f;
			e.main.startLifetime = R(0.08f, 0.3f);
			e.main.startSpeed = R(0f, 25f);
			e.main.startSize = R(3f, 8f);
			e.main.startColor = Grad(new Color(3.5f, 3.4f, 3.2f), new Color(3.5f, 3f, 1.8f));
			e.main.maxParticles = 80;
			e.emission.rateOverTime = 30f;
			e.shape.shape = TextParticlesShape.Letters;
			e.shape.edgeOnly = true;
			e.shape.spread = 360f;
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (Color.white, 1f) }, new[] { (1f, 0f), (1f, 0.5f), (0f, 1f) });
			e.rendererModule.texture = Tex("Glow");
		}

		//Star flares popping on the letters and fading
		private static void Glints(AllIn1TextParticles e)
		{
			e.main.duration = 2f;
			e.main.startLifetime = R(0.35f, 0.75f);
			e.main.startSpeed = R(0f, 0f);
			e.main.startSize = R(90f, 170f);
			e.main.startRotation = R(-10f, 10f);
			e.main.startColor = Grad(new Color(1f, 0.95f, 0.82f), new Color(1f, 0.9f, 0.7f));
			e.main.maxParticles = 20;
			e.emission.rateOverTime = 3f;
			e.shape.shape = TextParticlesShape.Letters;
			e.shape.edgeOnly = true;
			e.sizeOverLifetime.enabled = true;
			e.sizeOverLifetime.size = Curve(0f, 0.4f, 0.08f, 1f, 1f, 1f);
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (Color.white, 1f) }, new[] { (1f, 0f), (1f, 0.25f), (0f, 1f) });
			e.rendererModule.texture = Tex("Flare");
			e.rendererModule.intensity = 3f;
		}

		//Hearts floating off the letters; each one pops in through a reveal preset (Life Preset) instead of a size curve
		private static void Hearts(AllIn1TextParticles e)
		{
			e.main.duration = 3f;
			e.main.prewarm = true;
			e.main.startLifetime = R(2f, 3f);
			e.main.startSpeed = R(30f, 70f);
			e.main.startSize = R(18f, 30f);
			e.main.startRotation = R(-15f, 15f);
			e.main.startColor = Grad(new Color(1f, 0.35f, 0.6f), new Color(1f, 0.62f, 0.8f));
			e.main.maxParticles = 40;
			e.emission.rateOverTime = 5f;
			e.shape.shape = TextParticlesShape.LetterTops;
			e.shape.spread = 30f;
			e.forces.enabled = true;
			e.forces.gravity = new Vector2(0f, 20f);
			e.forces.sway = 14f;
			e.forces.swayFrequency = 0.6f;
			e.colorOverLifetime.enabled = true;
			e.colorOverLifetime.color = Grad(new[] { (Color.white, 0f), (Color.white, 1f) }, new[] { (1f, 0f), (1f, 0.7f), (0f, 1f) });
			e.lifePreset.enabled = true;
			e.lifePreset.preset = DefaultReveal();
			e.rendererModule.texture = Tex("Heart");
			e.rendererModule.blend = TextParticlesBlend.AlphaBlend;
		}

		//Particle_<name>.png in the plugin's Textures/Particles folder, found through the config so a moved plugin folder still works
		public static Texture Tex(string name)
		{
			AllIn1TextShaderConfig config = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);
			if (config == null) { return null; }
			string folder = EditorUtils.CombineAssetPath(EditorUtils.CombineAssetPath(config.RootPluginPath, TEXTURES_FOLDER_NAME), PARTICLES_FOLDER_NAME);
			return AssetDatabase.LoadAssetAtPath<Texture>(EditorUtils.CombineAssetPath(folder, "Particle_" + name + ".png"));
		}

		private static MeshEffectGroupConfig DefaultReveal()
		{
			AllIn1TextShaderConfig config = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);
			if (config == null) { return null; }
			return AssetDatabase.LoadAssetAtPath<MeshEffectGroupConfig>(EditorUtils.CombineAssetPath(config.DefaultTypewriterConfigsFolderPath, DEFAULT_REVEAL_FILE));
		}

		private static TextParticlesRange R(float min, float max) => new TextParticlesRange(min, max);

		//Pairs of time, value
		public static AnimationCurve Curve(params float[] keys)
		{
			Keyframe[] k = new Keyframe[keys.Length / 2];
			for (int i = 0; i < k.Length; i++) { k[i] = new Keyframe(keys[i * 2], keys[i * 2 + 1]); }
			AnimationCurve res = new AnimationCurve(k);
			for (int i = 0; i < k.Length; i++) { AnimationUtility.SetKeyLeftTangentMode(res, i, AnimationUtility.TangentMode.ClampedAuto); AnimationUtility.SetKeyRightTangentMode(res, i, AnimationUtility.TangentMode.ClampedAuto); }
			return res;
		}

		//Two color gradient for a random start color
		public static Gradient Grad(Color a, Color b)
		{
			Gradient g = new Gradient();
			g.SetKeys(new[] { new GradientColorKey(a, 0f), new GradientColorKey(b, 1f) }, new[] { new GradientAlphaKey(a.a, 0f), new GradientAlphaKey(b.a, 1f) });
			return g;
		}

		public static Gradient Grad((Color color, float time)[] colors, (float alpha, float time)[] alphas)
		{
			List<GradientColorKey> ck = new List<GradientColorKey>();
			foreach (var c in colors) { ck.Add(new GradientColorKey(c.color, c.time)); }
			List<GradientAlphaKey> ak = new List<GradientAlphaKey>();
			foreach (var a in alphas) { ak.Add(new GradientAlphaKey(a.alpha, a.time)); }
			Gradient g = new Gradient();
			g.SetKeys(ck.ToArray(), ak.ToArray());
			return g;
		}
	}
}
