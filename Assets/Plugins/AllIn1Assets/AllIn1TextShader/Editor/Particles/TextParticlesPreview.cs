using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Drives the Text Particles of the selection and their children: edit mode simulates them here, play mode pauses, scrubs and restarts the live ones
	[InitializeOnLoad]
	public static class TextParticlesPreview
	{
		private const float LOOP_HOLD = 0.5f;

		public static readonly List<AllIn1TextParticles> targets = new List<AllIn1TextParticles>();
		public static bool playing;
		public static float time;
		public static float speed = 1f;
		public static bool loop = true;

		private static double lastTick;
		private static float restartAt = -1f;

		static TextParticlesPreview()
		{
			Selection.selectionChanged += OnSelectionChanged;
			EditorApplication.update += Tick;
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
			AssemblyReloadEvents.beforeAssemblyReload += ClearEditModeTargets;
			EditorApplication.delayCall += OnSelectionChanged;
		}

		public static bool HasTargets
		{
			get
			{
				targets.RemoveAll(e => e == null);
				return targets.Count > 0;
			}
		}

		public static int ParticleCount
		{
			get
			{
				int n = 0;
				foreach (AllIn1TextParticles e in targets) { if (e != null) { n += e.ParticleCount; } }
				return n;
			}
		}

		public static float Length
		{
			get
			{
				float len = 0.1f;
				foreach (AllIn1TextParticles e in targets) { if (e != null) { len = Mathf.Max(len, e.PreviewLength); } }
				return len;
			}
		}

		public static bool HasTypewriters
		{
			get
			{
				foreach (AllIn1TextParticles e in targets) { if (e != null && e.LinkedTypewriter != null) { return true; } }
				return false;
			}
		}

		private static void OnPlayModeChanged(PlayModeStateChange change)
		{
			if (change == PlayModeStateChange.ExitingEditMode) { ClearEditModeTargets(); }
			targets.Clear();
			playing = false;
			time = 0f;
			EditorApplication.delayCall += OnSelectionChanged;
		}

		private static void OnSelectionChanged()
		{
			List<AllIn1TextParticles> found = new List<AllIn1TextParticles>();
			foreach (GameObject go in Selection.gameObjects)
			{
				if (go == null || !go.scene.IsValid()) { continue; }
				foreach (AllIn1TextParticles e in go.GetComponentsInChildren<AllIn1TextParticles>())
				{
					if (!found.Contains(e)) { found.Add(e); }
				}
			}
			if (SameSet(found)) { return; }

			if (!Application.isPlaying)
			{
				foreach (AllIn1TextParticles e in targets)
				{
					if (e != null && !found.Contains(e)) { e.Clear(false); }
				}
			}
			targets.Clear();
			targets.AddRange(found);
			restartAt = -1f;

			if (Application.isPlaying)
			{
				playing = true;
				foreach (AllIn1TextParticles e in targets) { playing &= !e.paused; }
			}
			else if (targets.Count > 0) { Restart(); }
			else { playing = false; }
			Repaint();
		}

		private static bool SameSet(List<AllIn1TextParticles> found)
		{
			if (found.Count != targets.Count) { return false; }
			foreach (AllIn1TextParticles e in found) { if (!targets.Contains(e)) { return false; } }
			return true;
		}

		private static void ClearEditModeTargets()
		{
			if (Application.isPlaying) { return; }
			foreach (AllIn1TextParticles e in targets) { if (e != null) { e.Clear(false); } }
			playing = false;
		}

		public static void Restart()
		{
			time = 0f;
			restartAt = -1f;
			foreach (AllIn1TextParticles e in targets) { if (e != null) { e.Restart(false, true); } }
			playing = true;
			lastTick = EditorApplication.timeSinceStartup;
			Repaint();
		}

		public static void Play()
		{
			bool alive = false;
			foreach (AllIn1TextParticles e in targets) { if (e != null) { alive |= e.IsAlive; } }
			if (!alive)
			{
				Restart();
				return;
			}
			foreach (AllIn1TextParticles e in targets) { if (e != null) { e.paused = false; } }
			playing = true;
			lastTick = EditorApplication.timeSinceStartup;
			Repaint();
		}

		public static void Pause()
		{
			playing = false;
			foreach (AllIn1TextParticles e in targets) { if (e != null) { e.paused = true; } }
			Repaint();
		}

		public static void Stop()
		{
			playing = false;
			time = 0f;
			restartAt = -1f;
			foreach (AllIn1TextParticles e in targets)
			{
				if (e == null) { continue; }
				e.Stop(false, true);
				e.paused = false;
			}
			Repaint();
		}

		public static void Scrub(float t)
		{
			playing = false;
			time = t;
			restartAt = -1f;
			AllIn1TextParticles.SimulateTogether(new List<AllIn1TextParticles>(targets), t);
			Repaint();
		}

		//Play mode only: hides each linked text at once and reveals it again, so the real typewriter events drive the particles
		public static void ReplayText()
		{
			HashSet<AllIn1TextTypewriter> done = new HashSet<AllIn1TextTypewriter>();
			foreach (AllIn1TextParticles e in targets)
			{
				if (e == null) { continue; }
				e.paused = false;
				AllIn1TextTypewriter tw = e.LinkedTypewriter;
				if (tw == null || !done.Add(tw)) { continue; }
				tw.HideImmediate();
				tw.Reveal();
			}
			playing = true;
		}

		private static void Tick()
		{
			if (targets.Count == 0) { return; }
			if (Application.isPlaying)
			{
				foreach (AllIn1TextParticles e in targets)
				{
					if (e != null)
					{
						time = e.PlaybackTime;
						break;
					}
				}
				if (playing) { Repaint(); }
				return;
			}
			if (!playing) { return; }

			double now = EditorApplication.timeSinceStartup;
			float dt = Mathf.Min(0.1f, (float)(now - lastTick)) * speed;
			lastTick = now;
			time += dt;

			bool alive = false;
			foreach (AllIn1TextParticles e in targets)
			{
				if (e == null) { continue; }
				e.Advance(dt);
				alive |= e.IsAlive;
			}

			if (!alive && loop)
			{
				if (restartAt < 0f) { restartAt = time + LOOP_HOLD; }
				else if (time >= restartAt) { Restart(); }
			}
			Repaint();
		}

		private static void Repaint()
		{
			EditorApplication.QueuePlayerLoopUpdate();
			SceneView.RepaintAll();
		}
	}

	[Overlay(typeof(SceneView), ID, "Text Particles", true)]
	public class TextParticlesOverlay : IMGUIOverlay, ITransientOverlay
	{
		public const string ID = "allin1-text-particles";

		public bool visible => TextParticlesPreview.HasTargets;

		public override void OnGUI()
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 92f;
			using (new GUILayout.VerticalScope(GUILayout.Width(270f)))
			{
				using (new GUILayout.HorizontalScope())
				{
					if (GUILayout.Button(TextParticlesPreview.playing ? "Pause" : "Play"))
					{
						if (TextParticlesPreview.playing) { TextParticlesPreview.Pause(); } else { TextParticlesPreview.Play(); }
					}
					if (GUILayout.Button("Restart")) { TextParticlesPreview.Restart(); }
					if (GUILayout.Button("Stop")) { TextParticlesPreview.Stop(); }
				}

				float len = TextParticlesPreview.Length;
				EditorGUI.BeginChangeCheck();
				float t = EditorGUILayout.Slider("Playback Time", Mathf.Min(TextParticlesPreview.time, len), 0f, len);
				if (EditorGUI.EndChangeCheck()) { TextParticlesPreview.Scrub(t); }

				if (!Application.isPlaying)
				{
					TextParticlesPreview.speed = EditorGUILayout.Slider("Speed", TextParticlesPreview.speed, 0.05f, 2f);
					TextParticlesPreview.loop = EditorGUILayout.Toggle(new GUIContent("Loop Preview", "Replays one shot systems shortly after their last particle dies"), TextParticlesPreview.loop);
				}

				EditorGUILayout.LabelField("Particles", $"{TextParticlesPreview.ParticleCount}  ({TextParticlesPreview.targets.Count} emitters)");

				if (Application.isPlaying && TextParticlesPreview.HasTypewriters)
				{
					if (GUILayout.Button(new GUIContent("Replay Text", "Hides the linked texts and reveals them again, so their real events fire"))) { TextParticlesPreview.ReplayText(); }
				}
				else if (!Application.isPlaying && TextParticlesPreview.HasTypewriters)
				{
					EditorGUILayout.HelpBox("Edit mode fires text triggers at time 0; per letter emission follows the letters at the reveal's pace.", MessageType.None);
				}
			}
			EditorGUIUtility.labelWidth = labelWidth;
		}
	}
}
