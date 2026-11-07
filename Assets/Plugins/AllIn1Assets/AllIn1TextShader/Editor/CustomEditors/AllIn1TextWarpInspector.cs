using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(AllIn1TextWarp))]
	[CanEditMultipleObjects]
	public class AllIn1TextWarpInspector : Editor
	{
		private const string STR_SESSION_KEY_EDITING = "AllIn1TextWarp.Editing.";
		private const string STR_SESSION_KEY_SYMMETRIC = "AllIn1TextWarp.Symmetric";
		private const string STR_UNDO_MOVE_CAGE_POINT = "Move Warp Cage Point";
		private const string STR_UNDO_CUSTOMIZE = "Customize Warp";
		private const string STR_UNDO_RESET_CAGE = "Reset Warp Cage";
		private const string STR_EDIT_HINT = "Drag the 16 points in the Scene view. Corners pin the box, the edge points bend its sides and the four inner points pull the middle. Escape stops editing.";
		private const string STR_FLAT_CAGE_HINT = "The cage is flat. Drag its points, or pick a style and press Customize to start from it.";
		private const string STR_AMOUNT_HINT = "The cage is shown at full strength. With Amount below 1 the text bends less than the cage.";
		private const string STR_CUSTOMIZE_FAILED = "Warp: nothing to customize, the text has no layout box yet (it is empty, or the object is not in a scene).";
		private const string STR_MIXED_STYLES = "Mixed";
		private const string STR_NOT_DRIVEN = "This object has no All In 1 Text Shader component, so the warp has nothing to bend. Add it to this object (the warp must sit on the text that has it).";
		private const string STR_NOT_INITIALISED = "The text is not set up here yet (Prefab Mode, or just activated), so it does not bend in this view. The envelope and the cage still show and edit the shape.";
		private const string STR_NO_BOX = "The text has no layout box yet (it is empty), so there is no cage to edit.";

		private static readonly GUIContent LABEL_SHAPE = new GUIContent("Shape", "The warp style and how strongly it bends.");
		private static readonly GUIContent LABEL_OPTIONS = new GUIContent("Options", "What the warp is stretched over and how letters follow it.");
		private static readonly GUIContent LABEL_STYLE = new GUIContent("Style", "The warp's shape. Most are Photoshop's Warp Text styles, and the ones that look alike on a single line are under More. Banner is a wave that travels along the text in play mode. Circle wraps the text around a circle. Custom is a 4x4 cage you drag in the Scene view.");
		private static readonly GUIContent LABEL_BEND = new GUIContent("Bend", "How strong the warp is. Negative bends the other way. Photoshop's range is -100 to 100; Flag, Banner, Wave, Fish, Rise, Twist, Inflate and Squeeze go to 300. For Banner, 100 lifts the crests by the text's full height. For Circle, 100 closes the circle.");
		private static readonly GUIContent LABEL_CUSTOMIZE = new GUIContent("Customize", "Copies this style, as it looks now, into the Custom cage and switches to Custom, so you can finish the shape by dragging its points. Amount becomes 1.");
		private static readonly GUIContent LABEL_EDIT_CAGE = new GUIContent("Edit Cage", "Shows the 16 cage points in the Scene view and hides the transform gizmo while editing.");
		private static readonly GUIContent LABEL_EDITING_CAGE = new GUIContent("Editing Cage (click to finish)", "Click or press Escape in the Scene view to stop editing.");
		private static readonly GUIContent LABEL_SYMMETRIC = new GUIContent("Symmetric", "Dragging a point also moves its mirror across the box's vertical middle.");
		private static readonly GUIContent LABEL_RESET_CAGE = new GUIContent("Reset Cage", "Flattens the cage back to the plain box.");
		private static readonly GUIContent LABEL_RESET = new GUIContent("R", "Resets to default value");
		private const float RESET_BUTTON_WIDTH = 20f;

		private static readonly Color COLOR_ENVELOPE = new Color(0.25f, 0.85f, 1f, 0.9f);
		private static readonly Color COLOR_GRID = new Color(0.25f, 0.85f, 1f, 0.35f);
		private static readonly Color COLOR_BOX = new Color(1f, 1f, 1f, 0.25f);
		private static readonly Color COLOR_CAGE = new Color(1f, 0.75f, 0.2f, 0.6f);
		private static readonly Color COLOR_CAGE_CORNER = new Color(1f, 1f, 1f, 1f);
		private static readonly Color COLOR_CAGE_POINT = new Color(1f, 0.75f, 0.2f, 1f);
		private const float ENVELOPE_WIDTH = 3f;
		private const float GRID_WIDTH = 1.5f;
		private const float CAGE_WIDTH = 1.5f;
		private const float CORNER_HANDLE_SIZE = 0.08f;
		private const float POINT_HANDLE_SIZE = 0.06f;
		private const int GRID_LINES = 4;
		private const int LINE_SAMPLES = 32;

		// Menu entries in declaration order: each style's saved value and its InspectorName path ("More/Arc Lower")
		private TextWarpStyle[] styleValues;
		private string[] stylePaths;

		private AllIn1TextWarp warp;

		private SerializedProperty spStyle;
		private SerializedProperty spBend;
		private SerializedProperty spHorizontalDistortion;
		private SerializedProperty spVerticalDistortion;
		private SerializedProperty spOrientation;
		private SerializedProperty spReferenceBox;
		private SerializedProperty spKeepLettersRectangular;
		private SerializedProperty spAmount;
		private SerializedProperty spWaves;
		private SerializedProperty spSpeed;
		private SerializedProperty spPhase;
		private SerializedProperty spPin;
		private SerializedProperty spUnscaledTime;

		private bool editing;

		private readonly Vector3[] linePoints = new Vector3[LINE_SAMPLES + 1];
		private readonly Vector3[] cageLinePoints = new Vector3[TextWarpField.CAGE_SIZE];

		private void OnEnable()
		{
			warp = (AllIn1TextWarp)target;

			spStyle = serializedObject.FindProperty("style");
			spBend = serializedObject.FindProperty("bend");
			spHorizontalDistortion = serializedObject.FindProperty("horizontalDistortion");
			spVerticalDistortion = serializedObject.FindProperty("verticalDistortion");
			spOrientation = serializedObject.FindProperty("orientation");
			spReferenceBox = serializedObject.FindProperty("referenceBox");
			spKeepLettersRectangular = serializedObject.FindProperty("keepLettersRectangular");
			spAmount = serializedObject.FindProperty("amount");
			spWaves = serializedObject.FindProperty("waves");
			spSpeed = serializedObject.FindProperty("speed");
			spPhase = serializedObject.FindProperty("phase");
			spPin = serializedObject.FindProperty("pin");
			spUnscaledTime = serializedObject.FindProperty("unscaledTime");

			editing = SessionState.GetBool(GetSessionKey(), false);
			ApplyToolsVisibility();

			Undo.undoRedoPerformed += OnUndoRedo;
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= OnUndoRedo;
			SceneToolsHider.SetHidden(this, false);
		}

		// Undo can take the style out of Custom (or back into it) under a cage being edited
		private void OnUndoRedo()
		{
			ApplyToolsVisibility();
			Repaint();
			SceneView.RepaintAll();
		}

		private string GetSessionKey()
		{
			return STR_SESSION_KEY_EDITING + warp.GetInstanceID();
		}

		private void SetEditing(bool value)
		{
			editing = value;
			SessionState.SetBool(GetSessionKey(), value);
			ApplyToolsVisibility();
			Repaint();
			SceneView.RepaintAll();
		}

		private bool IsEditingCage()
		{
			return editing && warp != null && warp.isActiveAndEnabled && warp.Style == TextWarpStyle.CUSTOM && warp.TryGetReferenceBox(out _);
		}

		// Called on every pass, so the gizmo follows the style whatever changed it (dropdown, undo, code, animation)
		private void ApplyToolsVisibility()
		{
			SceneToolsHider.SetHidden(this, IsEditingCage());
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawersSharedConfig.Setup();
			ApplyToolsVisibility();

			if (!serializedObject.isEditingMultipleObjects && warp.gameObject.scene.IsValid() && warp.isActiveAndEnabled && !warp.IsDriven)
			{
				bool hasTextComponent = warp.GetComponent<AllIn1TextShaderComponent>() != null || warp.GetComponent<TextMaterialConfiguratorMaster>() != null;
				EditorGUILayout.HelpBox(hasTextComponent ? STR_NOT_INITIALISED : STR_NOT_DRIVEN, hasTextComponent ? MessageType.Info : MessageType.Warning);
			}

			warp.shapeUnfolded = GUILayout.Toggle(warp.shapeUnfolded, LABEL_SHAPE, GUI.skin.button);
			if (warp.shapeUnfolded)
			{
				DrawShapeSection();
			}

			warp.optionsUnfolded = GUILayout.Toggle(warp.optionsUnfolded, LABEL_OPTIONS, GUI.skin.button);
			if (warp.optionsUnfolded)
			{
				DrawOptionsSection();
			}

			serializedObject.ApplyModifiedProperties();
		}

		// ApplyModifiedProperties runs OnValidate, which writes the mesh
		private void DrawShapeSection()
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			bool changed = DrawStyle();

			TextWarpStyle style = (TextWarpStyle)spStyle.intValue;
			bool mixedStyles = spStyle.hasMultipleDifferentValues;
			bool isCircle = !mixedStyles && style == TextWarpStyle.CIRCLE;
			bool isBanner = !mixedStyles && style == TextWarpStyle.BANNER;
			bool isCustom = !mixedStyles && style == TextWarpStyle.CUSTOM;

			if (isCustom)
			{
				if (changed)
				{
					serializedObject.ApplyModifiedProperties();
				}
				DrawCageControls();
			}
			else
			{
				changed |= DrawBend(GetBendRange(mixedStyles, style));

				EditorGUI.BeginDisabledGroup(isCircle || style == TextWarpStyle.TWIST);
				changed |= ResettablePropertyDrawer.DrawEnum(spOrientation, (int)AllIn1TextWarp.DEFAULT_ORIENTATION);
				EditorGUI.EndDisabledGroup();

				if (isBanner)
				{
					changed |= ResettablePropertyDrawer.Draw(spWaves, AllIn1TextWarp.DEFAULT_WAVES);
					changed |= ResettablePropertyDrawer.Draw(spSpeed, AllIn1TextWarp.DEFAULT_SPEED);
					changed |= ResettablePropertyDrawer.Draw(spPhase, AllIn1TextWarp.DEFAULT_PHASE);
					changed |= ResettablePropertyDrawer.DrawEnum(spPin, (int)AllIn1TextWarp.DEFAULT_PIN);
					changed |= ResettablePropertyDrawer.Draw(spUnscaledTime, AllIn1TextWarp.DEFAULT_UNSCALED_TIME);
				}
				else
				{
					EditorGUI.BeginDisabledGroup(isCircle);
					changed |= ResettablePropertyDrawer.Draw(spHorizontalDistortion, AllIn1TextWarp.DEFAULT_HORIZONTAL_DISTORTION);
					changed |= ResettablePropertyDrawer.Draw(spVerticalDistortion, AllIn1TextWarp.DEFAULT_VERTICAL_DISTORTION);
					EditorGUI.EndDisabledGroup();
				}

				if (changed)
				{
					serializedObject.ApplyModifiedProperties();
				}

				if (!isCircle && !isBanner && !mixedStyles)
				{
					EditorUIUtils.AddSpaces(1);
					if (GUILayout.Button(LABEL_CUSTOMIZE))
					{
						Customize();
					}
				}
			}

			if (changed)
			{
				SceneView.RepaintAll();
			}

			EditorGUILayout.EndVertical();
		}

		// A dropdown showing the plain style name, with the look alike styles in a More submenu. Works on the saved value, so the menu order is free
		private bool DrawStyle()
		{
			BuildStyleMenu();
			bool res = false;

			EditorGUILayout.BeginHorizontal();

			Rect rect = EditorGUILayout.GetControlRect();
			GUIContent label = EditorGUI.BeginProperty(rect, LABEL_STYLE, spStyle);
			Rect fieldRect = EditorGUI.PrefixLabel(rect, label);

			int value = spStyle.intValue;
			bool mixed = spStyle.hasMultipleDifferentValues;
			string shown = mixed ? STR_MIXED_STYLES : GetLeafName(GetStylePath(value));
			if (EditorGUI.DropdownButton(fieldRect, new GUIContent(shown), FocusType.Keyboard, EditorStyles.popup))
			{
				GenericMenu menu = new GenericMenu();
				for (int i = 0; i < styleValues.Length; i++)
				{
					int itemValue = (int)styleValues[i];
					menu.AddItem(new GUIContent(stylePaths[i]), !mixed && itemValue == value, OnStyleSelected, itemValue);
				}
				menu.DropDown(fieldRect);
			}
			EditorGUI.EndProperty();

			if (GUILayout.Button(LABEL_RESET, GUILayout.Width(RESET_BUTTON_WIDTH)))
			{
				spStyle.intValue = (int)AllIn1TextWarp.DEFAULT_STYLE;
				GUI.changed = true;
				res = true;
			}

			EditorGUILayout.EndHorizontal();

			return res;
		}

		// Runs after the menu closes, outside this inspector's GUI pass
		private void OnStyleSelected(object userData)
		{
			serializedObject.Update();
			spStyle.intValue = (int)userData;
			serializedObject.ApplyModifiedProperties();
			ApplyToolsVisibility();
			SceneView.RepaintAll();
		}

		// Declaration order is the menu order; each entry keeps its own saved value
		private void BuildStyleMenu()
		{
			if (styleValues != null) { return; }

			FieldInfo[] fields = typeof(TextWarpStyle).GetFields(BindingFlags.Public | BindingFlags.Static);
			styleValues = new TextWarpStyle[fields.Length];
			stylePaths = new string[fields.Length];
			for (int i = 0; i < fields.Length; i++)
			{
				styleValues[i] = (TextWarpStyle)fields[i].GetValue(null);
				InspectorNameAttribute inspectorName = fields[i].GetCustomAttribute<InspectorNameAttribute>();
				stylePaths[i] = inspectorName != null ? inspectorName.displayName : ObjectNames.NicifyVariableName(fields[i].Name);
			}
		}

		private string GetStylePath(int value)
		{
			for (int i = 0; i < styleValues.Length; i++)
			{
				if ((int)styleValues[i] == value) { return stylePaths[i]; }
			}
			return string.Empty;
		}

		// With several styles selected the slider takes the widest range among them; OnValidate clamps each object to its own
		private float GetBendRange(bool mixedStyles, TextWarpStyle style)
		{
			if (!mixedStyles) { return TextWarpField.GetMaxBend(style); }

			float res = TextWarpField.MAX_BEND;
			foreach (Object selected in targets)
			{
				res = Mathf.Max(res, TextWarpField.GetMaxBend(((AllIn1TextWarp)selected).Style));
			}
			return res;
		}

		private static string GetLeafName(string path)
		{
			return path.Substring(path.LastIndexOf('/') + 1);
		}

		// A slider whose range follows the style: Photoshop's 100, or 300 for the styles that grow linearly
		private bool DrawBend(float maxBend)
		{
			bool res = false;

			EditorGUILayout.BeginHorizontal();

			Rect rect = EditorGUILayout.GetControlRect();
			GUIContent label = EditorGUI.BeginProperty(rect, LABEL_BEND, spBend);
			EditorGUI.BeginChangeCheck();
			float value = EditorGUI.Slider(rect, label, Mathf.Clamp(spBend.floatValue, -maxBend, maxBend), -maxBend, maxBend);
			if (EditorGUI.EndChangeCheck())
			{
				spBend.floatValue = value;
				res = true;
			}
			EditorGUI.EndProperty();

			if (GUILayout.Button(LABEL_RESET, GUILayout.Width(RESET_BUTTON_WIDTH)))
			{
				spBend.floatValue = AllIn1TextWarp.DEFAULT_BEND;
				GUI.changed = true;
				res = true;
			}

			EditorGUILayout.EndHorizontal();

			return res;
		}

		// The button shows whether the cage is really being edited: not on a disabled warp, an inactive object, a prefab asset or an empty text
		private void DrawCageControls()
		{
			EditorUIUtils.AddSpaces(1);

			bool hasBox = warp.TryGetReferenceBox(out _);
			if (!hasBox)
			{
				EditorGUILayout.HelpBox(STR_NO_BOX, MessageType.Info);
			}

			bool editingNow = IsEditingCage();
			Color previousColor = GUI.backgroundColor;
			if (editingNow)
			{
				GUI.backgroundColor = COLOR_CAGE_POINT;
			}
			EditorGUI.BeginDisabledGroup(!hasBox || !warp.isActiveAndEnabled);
			if (GUILayout.Button(editingNow ? LABEL_EDITING_CAGE : LABEL_EDIT_CAGE, GUILayout.Height(28f)))
			{
				SetEditing(!editingNow);
			}
			EditorGUI.EndDisabledGroup();
			GUI.backgroundColor = previousColor;

			if (editingNow)
			{
				EditorGUILayout.HelpBox(STR_EDIT_HINT, MessageType.Info);
				if (spAmount.floatValue < 1f)
				{
					EditorGUILayout.HelpBox(STR_AMOUNT_HINT, MessageType.Info);
				}
			}

			bool symmetric = SessionState.GetBool(STR_SESSION_KEY_SYMMETRIC, false);
			bool newSymmetric = EditorGUILayout.Toggle(LABEL_SYMMETRIC, symmetric);
			if (newSymmetric != symmetric)
			{
				SessionState.SetBool(STR_SESSION_KEY_SYMMETRIC, newSymmetric);
			}

			if (GUILayout.Button(LABEL_RESET_CAGE))
			{
				ResetCage();
			}

			if (!editingNow && warp.IsCageFlat())
			{
				EditorGUILayout.HelpBox(STR_FLAT_CAGE_HINT, MessageType.Info);
			}
		}

		private void Customize()
		{
			bool anyCustomized = false;
			foreach (Object selected in targets)
			{
				AllIn1TextWarp selectedWarp = (AllIn1TextWarp)selected;
				Undo.RecordObject(selectedWarp, STR_UNDO_CUSTOMIZE);
				if (selectedWarp.CustomizeFromStyle())
				{
					EditorUtility.SetDirty(selectedWarp);
					selectedWarp.RequestWriteBack();
					anyCustomized = true;
				}
			}

			serializedObject.Update();
			if (anyCustomized)
			{
				SetEditing(true);
			}
			else
			{
				Debug.LogWarning(STR_CUSTOMIZE_FAILED, warp);
			}
		}

		private void ResetCage()
		{
			foreach (Object selected in targets)
			{
				AllIn1TextWarp selectedWarp = (AllIn1TextWarp)selected;
				Undo.RecordObject(selectedWarp, STR_UNDO_RESET_CAGE);
				selectedWarp.ResetCage();
				EditorUtility.SetDirty(selectedWarp);
				selectedWarp.RequestWriteBack();
			}

			serializedObject.Update();
			SceneView.RepaintAll();
		}

		private void DrawOptionsSection()
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			bool changed = false;
			changed |= ResettablePropertyDrawer.DrawEnum(spReferenceBox, (int)AllIn1TextWarp.DEFAULT_REFERENCE_BOX);
			changed |= ResettablePropertyDrawer.Draw(spKeepLettersRectangular, AllIn1TextWarp.DEFAULT_KEEP_LETTERS_RECTANGULAR);
			changed |= ResettablePropertyDrawer.Draw(spAmount, AllIn1TextWarp.DEFAULT_AMOUNT);

			if (changed)
			{
				serializedObject.ApplyModifiedProperties();
				SceneView.RepaintAll();
			}

			EditorGUILayout.EndVertical();
		}

		// Box, bent box and grid; the cage only for the inspected warp (first of a selection), so Escape and Edit Cage always reach it
		private void OnSceneGUI()
		{
			AllIn1TextWarp sceneWarp = (AllIn1TextWarp)target;
			bool inspected = sceneWarp == warp;
			if (inspected)
			{
				ApplyToolsVisibility();
				// Before any early return, so Escape always ends editing
				if (editing)
				{
					HandleKeyboard();
				}
			}

			// Drawn even when no handler drives the text (Prefab Mode), so the shape can still be seen and the cage edited
			if (sceneWarp == null || !sceneWarp.isActiveAndEnabled || !sceneWarp.TryGetReferenceBox(out Rect box)) { return; }

			Transform warpTransform = sceneWarp.transform;
			Handles.color = COLOR_BOX;
			Handles.DrawAAPolyLine(GRID_WIDTH,
				warpTransform.TransformPoint(new Vector3(box.xMin, box.yMin, 0f)),
				warpTransform.TransformPoint(new Vector3(box.xMin, box.yMax, 0f)),
				warpTransform.TransformPoint(new Vector3(box.xMax, box.yMax, 0f)),
				warpTransform.TransformPoint(new Vector3(box.xMax, box.yMin, 0f)),
				warpTransform.TransformPoint(new Vector3(box.xMin, box.yMin, 0f)));

			if (sceneWarp.ConfigureField())
			{
				DrawEnvelope(sceneWarp, box);
			}

			if (!inspected || !IsEditingCage()) { return; }

			// Keeps clicks on empty space from selecting other objects while the cage is being edited
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			DrawCage(sceneWarp, box);
			DrawCageHandles(sceneWarp, box);
		}

		private void HandleKeyboard()
		{
			Event current = Event.current;
			if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
			{
				SetEditing(false);
				current.Use();
			}
		}

		private void DrawEnvelope(AllIn1TextWarp sceneWarp, Rect box)
		{
			for (int line = 0; line <= GRID_LINES; line++)
			{
				float fraction = (float)line / GRID_LINES;
				bool edge = line == 0 || line == GRID_LINES;
				Handles.color = edge ? COLOR_ENVELOPE : COLOR_GRID;
				float width = edge ? ENVELOPE_WIDTH : GRID_WIDTH;

				DrawWarpedLine(sceneWarp, new Vector2(box.xMin, Mathf.Lerp(box.yMin, box.yMax, fraction)), new Vector2(box.xMax, Mathf.Lerp(box.yMin, box.yMax, fraction)), width);
				DrawWarpedLine(sceneWarp, new Vector2(Mathf.Lerp(box.xMin, box.xMax, fraction), box.yMin), new Vector2(Mathf.Lerp(box.xMin, box.xMax, fraction), box.yMax), width);
			}
		}

		private void DrawWarpedLine(AllIn1TextWarp sceneWarp, Vector2 from, Vector2 to, float width)
		{
			Transform warpTransform = sceneWarp.transform;
			for (int i = 0; i <= LINE_SAMPLES; i++)
			{
				Vector2 warped = sceneWarp.WarpPoint(Vector2.Lerp(from, to, (float)i / LINE_SAMPLES));
				linePoints[i] = warpTransform.TransformPoint(new Vector3(warped.x, warped.y, 0f));
			}
			Handles.DrawAAPolyLine(width, linePoints);
		}

		// The control net: each row and each column of the cage joined by straight lines
		private void DrawCage(AllIn1TextWarp sceneWarp, Rect box)
		{
			Handles.color = COLOR_CAGE;
			int size = TextWarpField.CAGE_SIZE;
			for (int line = 0; line < size; line++)
			{
				for (int k = 0; k < size; k++)
				{
					cageLinePoints[k] = CageToWorld(sceneWarp, box, line * size + k);
				}
				Handles.DrawAAPolyLine(CAGE_WIDTH, cageLinePoints);

				for (int k = 0; k < size; k++)
				{
					cageLinePoints[k] = CageToWorld(sceneWarp, box, k * size + line);
				}
				Handles.DrawAAPolyLine(CAGE_WIDTH, cageLinePoints);
			}
		}

		// Slider2D keeps each point on the text's plane under the cursor, even on rotated world space text; ids reserved in a fixed order
		private void DrawCageHandles(AllIn1TextWarp sceneWarp, Rect box)
		{
			Transform warpTransform = sceneWarp.transform;
			int size = TextWarpField.CAGE_SIZE;
			for (int i = 0; i < TextWarpField.CAGE_POINT_COUNT; i++)
			{
				int controlId = GUIUtility.GetControlID(FocusType.Passive);

				int row = i / size;
				int column = i % size;
				bool corner = (row == 0 || row == size - 1) && (column == 0 || column == size - 1);

				Vector3 worldPosition = CageToWorld(sceneWarp, box, i);
				float handleSize = HandleUtility.GetHandleSize(worldPosition) * (corner ? CORNER_HANDLE_SIZE : POINT_HANDLE_SIZE);

				Handles.color = corner ? COLOR_CAGE_CORNER : COLOR_CAGE_POINT;
				EditorGUI.BeginChangeCheck();
				Vector3 newWorldPosition = Handles.Slider2D(controlId, worldPosition, Vector3.zero, warpTransform.forward, warpTransform.right, warpTransform.up, handleSize, Handles.DotHandleCap, Vector2.zero);
				if (!EditorGUI.EndChangeCheck()) { continue; }

				Vector3 local = warpTransform.InverseTransformPoint(newWorldPosition);
				Vector2 cagePoint = TextWarpField.LocalToCagePoint(new Vector2(local.x, local.y), box);

				Undo.RecordObject(sceneWarp, STR_UNDO_MOVE_CAGE_POINT);
				sceneWarp.SetCagePoint(i, cagePoint);
				if (SessionState.GetBool(STR_SESSION_KEY_SYMMETRIC, false))
				{
					sceneWarp.SetCagePoint(row * size + (size - 1 - column), new Vector2(1f - cagePoint.x, cagePoint.y));
				}

				EditorUtility.SetDirty(sceneWarp);
				sceneWarp.RequestWriteBack();
				Repaint();
			}
		}

		private static Vector3 CageToWorld(AllIn1TextWarp sceneWarp, Rect box, int index)
		{
			Vector2 local = TextWarpField.CagePointToLocal(sceneWarp.GetCagePoint(index), box);
			return sceneWarp.transform.TransformPoint(new Vector3(local.x, local.y, 0f));
		}
	}
}
