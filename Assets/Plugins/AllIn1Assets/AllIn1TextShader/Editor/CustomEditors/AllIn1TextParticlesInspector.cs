using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(AllIn1TextParticles)), CanEditMultipleObjects]
	public class AllIn1TextParticlesInspector : Editor
	{
		private struct Module
		{
			public string field;
			public string title;
			public bool toggle;
			public bool open;

			public Module(string field, string title, bool toggle, bool open)
			{
				this.field = field;
				this.title = title;
				this.toggle = toggle;
				this.open = open;
			}
		}

		private static readonly Module[] MODULES =
		{
			new Module("main", "Main", false, true),
			new Module("textLink", "Text", false, true),
			new Module("emission", "Emission", true, true),
			new Module("shape", "Shape", false, true),
			new Module("forces", "Forces", true, false),
			new Module("sizeOverLifetime", "Size over Lifetime", true, false),
			new Module("colorOverLifetime", "Color over Lifetime", true, false),
			new Module("rotationOverLifetime", "Rotation over Lifetime", true, false),
			new Module("flicker", "Flicker", true, false),
			new Module("textureSheet", "Texture Sheet Animation", true, false),
			new Module("lifePreset", "Life Preset", true, false),
			new Module("rendererModule", "Renderer", false, true),
		};

		public override bool RequiresConstantRepaint() => Application.isPlaying;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawersSharedConfig.Setup();
			DrawToolbar();
			EditorGUILayout.Space(2f);
			foreach (Module m in MODULES) { DrawModule(m); }
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawToolbar()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				if (EditorGUILayout.DropdownButton(new GUIContent("Presets"), FocusType.Passive, GUILayout.Width(90f)))
				{
					GenericMenu menu = new GenericMenu();
					foreach (string name in TextParticlesPresets.NAMES)
					{
						string n = name;
						menu.AddItem(new GUIContent(n), false, () => ApplyPreset(n));
					}
					menu.ShowAsContext();
				}

				AllIn1TextParticles e = (AllIn1TextParticles)target;
				string status = Application.isPlaying
					? $"{e.ParticleCount} particles, {(e.IsEmitting ? "emitting" : "idle")}"
					: "Preview in the Scene view overlay";
				EditorGUILayout.LabelField(status, EditorStyles.miniLabel);
			}

			AllIn1TextParticles first = (AllIn1TextParticles)target;
			if (first.LinkedText == null && NeedsText(first))
			{
				EditorGUILayout.HelpBox("No text linked: set Text, or put this object under a TextMeshPro text. Letter shapes and text triggers need one.", MessageType.Warning);
			}
		}

		private static bool NeedsText(AllIn1TextParticles e)
		{
			TextParticlesShape s = e.shape.shape;
			bool letterShape = s == TextParticlesShape.Letters || s == TextParticlesShape.LetterTops || s == TextParticlesShape.LetterBottoms || s == TextParticlesShape.TextBox;
			bool textTrigger = e.textLink.playOn != TextParticlesPlayOn.OnEnable && e.textLink.playOn != TextParticlesPlayOn.Manual;
			return letterShape || textTrigger || e.emission.perLetter;
		}

		private void ApplyPreset(string name)
		{
			foreach (Object o in targets)
			{
				AllIn1TextParticles e = (AllIn1TextParticles)o;
				Undo.RecordObject(e, "Text Particles Preset " + name);
				TextParticlesPresets.Apply(e, name);
				EditorUtility.SetDirty(e);
			}
			serializedObject.Update();
			if (!Application.isPlaying && TextParticlesPreview.HasTargets) { TextParticlesPreview.Restart(); }
		}

		private void DrawModule(Module m)
		{
			SerializedProperty prop = serializedObject.FindProperty(m.field);
			if (prop == null) { return; }
			SerializedProperty enabled = m.toggle ? prop.FindPropertyRelative("enabled") : null;

			string key = "AllIn1TextParticles." + m.field;
			bool open = SessionState.GetBool(key, m.open);
			bool on = enabled == null || enabled.boolValue || enabled.hasMultipleDifferentValues;

			//The suite's section header (a full width toggle button, as in the spline and typewriter inspectors), with the module's switch inside it
			Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.button, GUILayout.Height(22f), GUILayout.ExpandWidth(true));
			Rect toggleRect = new Rect(r.x + 6f, r.y + (r.height - 16f) * 0.5f, 16f, 16f);
			Event ev = Event.current;
			if (ev.type == EventType.Repaint)
			{
				Color prev = GUI.color;
				if (!on) { GUI.color = new Color(prev.r, prev.g, prev.b, prev.a * 0.55f); }
				GUI.skin.button.Draw(r, new GUIContent(m.title), false, false, open, false);
				GUI.color = prev;
			}
			if (enabled != null)
			{
				EditorGUI.showMixedValue = enabled.hasMultipleDifferentValues;
				EditorGUI.BeginChangeCheck();
				bool value = EditorGUI.Toggle(toggleRect, enabled.boolValue);
				if (EditorGUI.EndChangeCheck()) { enabled.boolValue = value; }
				EditorGUI.showMixedValue = false;
			}
			if (ev.type == EventType.MouseDown && ev.button == 0 && r.Contains(ev.mousePosition))
			{
				open = !open;
				SessionState.SetBool(key, open);
				ev.Use();
			}
			if (!open)
			{
				EditorGUILayout.Space(1f);
				return;
			}

			using (new EditorGUI.DisabledScope(!on))
			{
				EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);
				SerializedProperty it = prop.Copy();
				SerializedProperty end = prop.GetEndProperty();
				bool enter = true;
				while (it.NextVisible(enter) && !SerializedProperty.EqualContents(it, end))
				{
					enter = false;
					if (it.name == "enabled") { continue; }
					if (!Visible(m.field, it.name)) { continue; }
					EditorGUILayout.PropertyField(it, true);
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.Space(1f);
		}

		//Fields that only mean something for some settings stay out of the way otherwise
		private bool Visible(string module, string field)
		{
			AllIn1TextParticles e = (AllIn1TextParticles)target;
			switch (module)
			{
				case "main":
					if (field == "prewarm") { return e.main.looping; }
					return true;
				case "textLink":
					if (field == "clearOnStop") { return e.textLink.stopOn != TextParticlesStopOn.Never; }
					return true;
				case "emission":
					if (field == "perLetterCount") { return e.emission.perLetter; }
					if (field == "perParentCount") { return e.emission.perParentParticle; }
					return true;
				case "shape":
				{
					TextParticlesShape s = e.shape.shape;
					bool letters = s == TextParticlesShape.Letters || s == TextParticlesShape.LetterTops || s == TextParticlesShape.LetterBottoms;
					switch (field)
					{
						case "radius": return s == TextParticlesShape.Circle;
						case "boxSize": return s == TextParticlesShape.Box;
						case "edgeOnly": return s == TextParticlesShape.Circle || s == TextParticlesShape.Box || s == TextParticlesShape.TextBox || s == TextParticlesShape.Letters;
						case "revealedLettersOnly": return letters;
						case "angle": return e.shape.direction == TextParticlesDirection.Angle || e.shape.direction == TextParticlesDirection.Sideways;
					}
					return true;
				}
				case "forces":
					if (field == "turbulenceFrequency") { return e.forces.turbulence != 0f; }
					if (field == "swayFrequency") { return e.forces.sway != 0f; }
					return true;
				case "rotationOverLifetime":
					if (field == "flutterSpeed") { return e.rotationOverLifetime.flutter > 0f; }
					return true;
				case "textureSheet":
					if (field == "fps") { return e.textureSheet.mode == TextParticlesFrameMode.Crackle; }
					if (field == "cycles") { return e.textureSheet.mode == TextParticlesFrameMode.OverLifetime; }
					return true;
				case "rendererModule":
					if (field == "velocityStretch") { return e.rendererModule.alignToVelocity; }
					return true;
			}
			return true;
		}
	}

	[CustomPropertyDrawer(typeof(TextParticlesRange))]
	public class TextParticlesRangeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			Rect field = EditorGUI.PrefixLabel(position, label);
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			float gap = 18f;
			float w = (field.width - gap) * 0.5f;
			EditorGUI.PropertyField(new Rect(field.x, field.y, w, field.height), property.FindPropertyRelative("min"), GUIContent.none);
			EditorGUI.LabelField(new Rect(field.x + w, field.y, gap, field.height), "to", EditorStyles.centeredGreyMiniLabel);
			EditorGUI.PropertyField(new Rect(field.x + w + gap, field.y, w, field.height), property.FindPropertyRelative("max"), GUIContent.none);
			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}
	}
}
