using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CustomEditor(typeof(AllIn1TextSpline))]
	public class AllIn1TextSplineInspector : Editor
	{
		private const string STR_REVEAL_CONFIG_PREFIX = "RevealConfig";
		private const string STR_HIDE_CONFIG_PREFIX = "Hide Config";
		private const string STR_UNDO_ENTER_CONFIG = "Change Spline Enter Config";
		private const string STR_UNDO_EXIT_CONFIG = "Change Spline Exit Config";
		private const string STR_EDIT_HINT = "Drag points and tangents in the Scene view. Shift+click adds a point at the cursor (on the curve if you click on it), Delete removes the selected one.";
		private const string STR_SESSION_KEY_EDITING = "AllIn1TextSpline.Editing.";
		private const string STR_UNDO_MOVE_POINT = "Move Spline Point";
		private const string STR_UNDO_MOVE_TANGENT = "Move Spline Tangent";
		private const string STR_UNDO_ADD_POINT = "Add Spline Point";
		private const string STR_UNDO_REMOVE_POINT = "Remove Spline Point";
		private const string STR_UNDO_RESET_POINTS = "Reset Spline Points";
		private const string STR_UNDO_TANGENT_MODE = "Change Spline Tangent Mode";

		private static readonly GUIContent[] TANGENT_MODE_LABELS =
		{
			new GUIContent("Auto", "Tangents are computed from the neighbouring points for a smooth curve. Dragging a tangent handle switches to Mirrored."),
			new GUIContent("Mirrored", "The two tangent handles stay opposite and equal, so the curve is smooth through the point."),
			new GUIContent("Broken", "The two tangent handles move independently, allowing a corner at the point.")
		};

		private static readonly GUIContent LABEL_SPLINE = new GUIContent("Spline", "The curve itself: its points, whether it is closed and how finely it is sampled.");
		private static readonly GUIContent LABEL_LAYOUT = new GUIContent("Layout", "How the text is laid along the curve.");
		private static readonly GUIContent LABEL_MOVEMENT = new GUIContent("Movement", "Where the text sits on the curve and how it scrolls in play mode.");
		private static readonly GUIContent LABEL_ENTER_EXIT = new GUIContent("Enter / Exit", "A visible window on the curve with position driven entrance and exit effects, and the Show() and Hide() travel.");
		private static readonly GUIContent LABEL_EVENTS = new GUIContent("Events", "Called when the Show() and Hide() travel lands. A typewriter driven by the spline has its own reveal and hide events too.");
		private static readonly GUIContent LABEL_SHOW_COMPLETED = new GUIContent("On Show Completed", "The Show() travel landed: every letter is at its spot with its Enter effects finished.");
		private static readonly GUIContent LABEL_HIDE_COMPLETED = new GUIContent("On Hide Completed", "The Hide() travel ended and the text is parked out of sight.");
		private static readonly GUIContent LABEL_EDIT_SPLINE = new GUIContent("Edit Spline", "Shows the point and tangent handles in the Scene view and hides the transform gizmo while editing.");
		private static readonly GUIContent LABEL_EDITING_SPLINE = new GUIContent("Editing Spline (click to finish)", "Click or press Escape in the Scene view to stop editing.");
		private static readonly GUIContent LABEL_SELECTED = new GUIContent("Selected", "Index of the selected point. Click a point in the Scene view or drag this slider.");
		private static readonly GUIContent LABEL_ADD = new GUIContent("Add", "Adds a point after the last one, continuing the last segment's direction.");
		private static readonly GUIContent LABEL_INSERT_AFTER = new GUIContent("Insert After Selected", "Inserts a point halfway along the curve after the selected point.");
		private static readonly GUIContent LABEL_REMOVE = new GUIContent("Remove Selected", "Removes the selected point. A curve keeps at least two points.");
		private static readonly GUIContent LABEL_RESET_POINTS = new GUIContent("Reset Points", "Replaces all points with the default arch across the text's rect.");
		private static readonly GUIContent LABEL_POINT_POSITION = new GUIContent("Position", "Point position in the text's local space.");
		private static readonly GUIContent LABEL_CURVATURE = new GUIContent("Curvature", "How the selected point's tangents behave.");
		private static readonly GUIContent LABEL_TANGENT_IN = new GUIContent("Tangent In", "Offset of the incoming handle from the point.");
		private static readonly GUIContent LABEL_TANGENT_OUT = new GUIContent("Tangent Out", "Offset of the outgoing handle from the point.");
		private static readonly GUIContent LABEL_VISIBLE_RANGE = new GUIContent("Visible Range", "The part of the curve where characters are shown, as fractions of its length. Characters outside it are not drawn.");
		private static readonly GUIContent LABEL_TRAVEL = new GUIContent("Travel", "Show() moves the text from outside the window to Position. Hide() moves it out the other side.");
		private static readonly GUIContent LABEL_ENTER_EFFECTS = new GUIContent("Enter Effects", "Played by position as a character crosses the entering zone.");
		private static readonly GUIContent LABEL_EXIT_EFFECTS = new GUIContent("Exit Effects", "Played by position as a character crosses the leaving zone.");
		private static readonly GUIContent LABEL_SHOW = new GUIContent("Show", "Calls Show(): travels in from the parked spot to Position.");
		private static readonly GUIContent LABEL_HIDE = new GUIContent("Hide", "Calls Hide(): travels out to the parked spot on the far side.");
		private static readonly GUIContent LABEL_RESET = new GUIContent("R", "Resets to default value");

		private static readonly Color COLOR_CURVE = new Color(0.25f, 0.85f, 1f, 1f);
		private static readonly Color COLOR_CURVE_UNSELECTED = new Color(0.25f, 0.85f, 1f, 0.45f);
		private static readonly Color COLOR_POINT = new Color(1f, 1f, 1f, 1f);
		private static readonly Color COLOR_POINT_SELECTED = new Color(1f, 0.75f, 0.2f, 1f);
		private static readonly Color COLOR_TANGENT = new Color(0.55f, 0.75f, 1f, 1f);
		private static readonly Color COLOR_WINDOW = new Color(1f, 0.75f, 0.2f, 0.9f);
		private static readonly Color COLOR_FEATHER = new Color(1f, 0.75f, 0.2f, 0.4f);
		private const float WINDOW_WIDTH = 6f;
		private const int WINDOW_SAMPLES = 96;

		private const float CURVE_WIDTH = 3f;
		private const float POINT_HANDLE_SIZE = 0.08f;
		private const float TANGENT_HANDLE_SIZE = 0.05f;
		private const float INSERT_GHOST_SIZE = 0.06f;
		private const float INSERT_PICK_DISTANCE_PIXELS = 14f;
		private const float PLANE_HIT_TOLERANCE_PIXELS = 2f;
		private const int ARROWS_ALONG_CURVE = 6;
		private const float ARROW_SIZE = 0.12f;
		private const int DRAW_SAMPLES_PER_SEGMENT = 24;

		private AllIn1TextSpline spline;

		private SerializedProperty spPoints;
		private SerializedProperty spClosed;
		private SerializedProperty spCurveResolution;
		private SerializedProperty spSpacing;
		private SerializedProperty spAutoFit;
		private SerializedProperty spBaselineOffset;
		private SerializedProperty spClampEnds;
		private SerializedProperty spRotateWithCurve;
		private SerializedProperty spScrollMode;
		private SerializedProperty spScrollSpeed;
		private SerializedProperty spPosition;
		private SerializedProperty spEnterExitEnabled;
		private SerializedProperty spVisibleStart;
		private SerializedProperty spVisibleEnd;
		private SerializedProperty spEnterLength;
		private SerializedProperty spExitLength;
		private SerializedProperty spUseEnterAsExitConfig;
		private SerializedProperty spStartHidden;
		private SerializedProperty spTravelDirection;
		private SerializedProperty spTravelDuration;
		private SerializedProperty spTravelEase;
		private SerializedProperty spIndividualLetterTravel;
		private SerializedProperty spDriveTypewriter;
		private SerializedProperty spOverrideTypewriterTiming;
		private SerializedProperty spOnShowCompleted;
		private SerializedProperty spOnHideCompleted;

		private AllIn1TextShaderConfig config;
		private EasingFunctionsSO easeFunctionOnceCollection;
		private string[] easeFunctionOnceNames;

		private RevealConfigDrawer revealConfigDrawer;

		private bool editing;

		// The curve as drawn this frame, reused for screen-space picking
		private Vector3[] curveWorldPoints = new Vector3[0];

		private void OnEnable()
		{
			spline = (AllIn1TextSpline)target;

			spPoints = serializedObject.FindProperty("points");
			spClosed = serializedObject.FindProperty("closed");
			spCurveResolution = serializedObject.FindProperty("curveResolution");
			spSpacing = serializedObject.FindProperty("spacing");
			spAutoFit = serializedObject.FindProperty("autoFit");
			spBaselineOffset = serializedObject.FindProperty("baselineOffset");
			spClampEnds = serializedObject.FindProperty("clampEnds");
			spRotateWithCurve = serializedObject.FindProperty("rotateWithCurve");
			spScrollMode = serializedObject.FindProperty("scrollMode");
			spScrollSpeed = serializedObject.FindProperty("scrollSpeed");
			spPosition = serializedObject.FindProperty("position");
			spEnterExitEnabled = serializedObject.FindProperty("enterExitEnabled");
			spVisibleStart = serializedObject.FindProperty("visibleStart");
			spVisibleEnd = serializedObject.FindProperty("visibleEnd");
			spEnterLength = serializedObject.FindProperty("enterLength");
			spExitLength = serializedObject.FindProperty("exitLength");
			spUseEnterAsExitConfig = serializedObject.FindProperty("useEnterAsExitConfig");
			spStartHidden = serializedObject.FindProperty("startHidden");
			spTravelDirection = serializedObject.FindProperty("travelDirection");
			spTravelDuration = serializedObject.FindProperty("travelDuration");
			spTravelEase = serializedObject.FindProperty("travelEase");
			spIndividualLetterTravel = serializedObject.FindProperty("individualLetterTravel");
			spDriveTypewriter = serializedObject.FindProperty("driveTypewriter");
			spOverrideTypewriterTiming = serializedObject.FindProperty("overrideTypewriterTiming");
			spOnShowCompleted = serializedObject.FindProperty("onShowCompletedEvent");
			spOnHideCompleted = serializedObject.FindProperty("onHideCompletedEvent");

			if(revealConfigDrawer == null)
			{
				revealConfigDrawer = new RevealConfigDrawer();
			}

			LoadConfigReferences();

			editing = SessionState.GetBool(GetSessionKey(), false);
			ApplyToolsVisibility();
		}

		// Same collections and ease functions the typewriter's Reveal and Hide sections use
		private void LoadConfigReferences()
		{
			config = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);
			if (config == null) { return; }

			if (spline.RefreshConfigReferencesIfNeeded(config.revealConfigCollection, config.hideConfigCollection))
			{
				EditorUtility.SetDirty(spline);
			}

			easeFunctionOnceCollection = AssetDatabase.LoadAssetAtPath<EasingFunctionsSO>(config.EaseFunctionCollectionOncePath);
			if (easeFunctionOnceCollection != null)
			{
				easeFunctionOnceNames = easeFunctionOnceCollection.GetEaseFunctionNamesArray();
			}
		}

		private void OnDisable()
		{
			SceneToolsHider.SetHidden(this, false);
		}

		private string GetSessionKey()
		{
			return STR_SESSION_KEY_EDITING + spline.GetInstanceID();
		}

		private void SetEditing(bool value)
		{
			editing = value;
			SessionState.SetBool(GetSessionKey(), value);
			ApplyToolsVisibility();
			SceneView.RepaintAll();
		}

		// The transform gizmo sits on top of the seeded points, so it is hidden while the curve is being edited
		private void ApplyToolsVisibility()
		{
			SceneToolsHider.SetHidden(this, editing);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawersSharedConfig.Setup();

			DrawEditToggle();
			EditorUIUtils.AddSpaces(1);

			spline.splineUnfolded = GUILayout.Toggle(spline.splineUnfolded, LABEL_SPLINE, GUI.skin.button);
			if (spline.splineUnfolded)
			{
				DrawSplineSection();
			}

			spline.layoutUnfolded = GUILayout.Toggle(spline.layoutUnfolded, LABEL_LAYOUT, GUI.skin.button);
			if (spline.layoutUnfolded)
			{
				DrawLayoutSection();
			}

			spline.movementUnfolded = GUILayout.Toggle(spline.movementUnfolded, LABEL_MOVEMENT, GUI.skin.button);
			if (spline.movementUnfolded)
			{
				DrawMovementSection();
			}

			spline.enterExitUnfolded = GUILayout.Toggle(spline.enterExitUnfolded, LABEL_ENTER_EXIT, GUI.skin.button);
			if (spline.enterExitUnfolded)
			{
				DrawEnterExitSection();
			}

			spline.eventsUnfolded = GUILayout.Toggle(spline.eventsUnfolded, LABEL_EVENTS, GUI.skin.button);
			if (spline.eventsUnfolded)
			{
				EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);
				EditorGUILayout.PropertyField(spOnShowCompleted, LABEL_SHOW_COMPLETED);
				EditorGUILayout.PropertyField(spOnHideCompleted, LABEL_HIDE_COMPLETED);
				EditorGUILayout.EndVertical();
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawEnterExitSection()
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			bool changed = false;
			changed |= ResettablePropertyDrawer.Draw(spEnterExitEnabled, AllIn1TextSpline.DEFAULT_ENTER_EXIT_ENABLED);

			GUI.enabled = spEnterExitEnabled.boolValue;
			changed |= DrawVisibleRange();
			changed |= ResettablePropertyDrawer.Draw(spEnterLength, AllIn1TextSpline.DEFAULT_ENTER_LENGTH);
			changed |= ResettablePropertyDrawer.Draw(spExitLength, AllIn1TextSpline.DEFAULT_EXIT_LENGTH);
			changed |= ResettablePropertyDrawer.Draw(spUseEnterAsExitConfig, AllIn1TextSpline.DEFAULT_USE_ENTER_AS_EXIT);
			GUI.enabled = true;

			// ApplyModifiedProperties runs OnValidate, which rebuilds the states and writes the mesh
			if (changed)
			{
				serializedObject.ApplyModifiedProperties();
				SceneView.RepaintAll();
			}

			if (spEnterExitEnabled.boolValue && config != null && easeFunctionOnceCollection != null)
			{
				EditorUIUtils.AddSpaces(1);
				DrawEnterExitConfigs();
			}

			EditorUIUtils.AddSpaces(1);
			DrawTravel();

			EditorGUILayout.EndVertical();
		}

		// Show() and Hide() settings, plus buttons to fire them while playing
		private void DrawTravel()
		{
			EditorGUILayout.LabelField(LABEL_TRAVEL, EditorStyles.boldLabel);

			bool changed = false;
			changed |= ResettablePropertyDrawer.Draw(spStartHidden, AllIn1TextSpline.DEFAULT_START_HIDDEN);
			changed |= ResettablePropertyDrawer.DrawEnum(spTravelDirection, (int)AllIn1TextSpline.DEFAULT_TRAVEL_DIRECTION);
			changed |= ResettablePropertyDrawer.Draw(spTravelDuration, AllIn1TextSpline.DEFAULT_TRAVEL_DURATION);
			changed |= ResettablePropertyDrawer.Draw(spIndividualLetterTravel, AllIn1TextSpline.DEFAULT_INDIVIDUAL_LETTER_TRAVEL);
			changed |= ResettablePropertyDrawer.Draw(spTravelEase, () => spTravelEase.animationCurveValue = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));
			changed |= ResettablePropertyDrawer.Draw(spDriveTypewriter, AllIn1TextSpline.DEFAULT_DRIVE_TYPEWRITER);
			GUI.enabled = spDriveTypewriter.boolValue;
			changed |= ResettablePropertyDrawer.Draw(spOverrideTypewriterTiming, AllIn1TextSpline.DEFAULT_OVERRIDE_TYPEWRITER_TIMING);
			GUI.enabled = true;

			if (changed)
			{
				serializedObject.ApplyModifiedProperties();
			}

			if (Application.isPlaying)
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button(LABEL_SHOW)) { spline.Show(); }
				if (GUILayout.Button(LABEL_HIDE)) { spline.Hide(); }
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.LabelField($"Shown: {spline.IsShown}    Traveling: {spline.IsTraveling}");
			}
		}

		private bool DrawVisibleRange()
		{
			bool res = false;

			EditorGUILayout.BeginHorizontal();

			float start = spVisibleStart.floatValue;
			float end = spVisibleEnd.floatValue;
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.MinMaxSlider(LABEL_VISIBLE_RANGE, ref start, ref end, 0f, 1f);
			if (EditorGUI.EndChangeCheck())
			{
				spVisibleStart.floatValue = start;
				spVisibleEnd.floatValue = end;
				res = true;
			}

			if (GUILayout.Button(LABEL_RESET, GUILayout.Width(20f)))
			{
				spVisibleStart.floatValue = AllIn1TextSpline.DEFAULT_VISIBLE_START;
				spVisibleEnd.floatValue = AllIn1TextSpline.DEFAULT_VISIBLE_END;
				GUI.changed = true;
				res = true;
			}

			EditorGUILayout.EndHorizontal();

			return res;
		}

		// The same preset dropdown and effect drawers as the typewriter's Reveal and Hide sections
		private void DrawEnterExitConfigs()
		{
			bool configChanges = false;

			EditorGUILayout.LabelField(LABEL_ENTER_EFFECTS, EditorStyles.boldLabel);
			bool enterChanges = false;
			MeshEffectGroupConfig enterConfig = revealConfigDrawer.Draw
			(
				typewriter:				null,
				configCollection:		config.revealConfigCollection,
				meshEffectCollection:	config.revealEffectDatabase,
				config:					spline.EnterConfig,
				configsFolderPath:		config.RevealConfigsFolderPath,
				assetPrefix:			STR_REVEAL_CONFIG_PREFIX,
				easeFunctionNames:		easeFunctionOnceNames,
				easeFunctionCollection:	easeFunctionOnceCollection,
				commonStyles:			DrawersSharedConfig.s_commonStyles,
				changes:				ref enterChanges
			);
			if (enterConfig != spline.EnterConfig)
			{
				Undo.RecordObject(spline, STR_UNDO_ENTER_CONFIG);
				spline.EnterConfig = enterConfig;
				EditorUtility.SetDirty(spline);
				configChanges = true;
			}
			if (enterChanges && enterConfig != null)
			{
				EditorUtility.SetDirty(enterConfig);
				configChanges = true;
			}

			if (!spUseEnterAsExitConfig.boolValue)
			{
				EditorUIUtils.AddSpaces(1);
				EditorGUILayout.LabelField(LABEL_EXIT_EFFECTS, EditorStyles.boldLabel);
				bool exitChanges = false;
				MeshEffectGroupConfig exitConfig = revealConfigDrawer.Draw
				(
					typewriter:				null,
					configCollection:		config.hideConfigCollection,
					meshEffectCollection:	config.revealEffectDatabase,
					config:					spline.ExitConfigSelected,
					configsFolderPath:		config.HideConfigsFolderPath,
					assetPrefix:			STR_HIDE_CONFIG_PREFIX,
					easeFunctionNames:		easeFunctionOnceNames,
					easeFunctionCollection:	easeFunctionOnceCollection,
					commonStyles:			DrawersSharedConfig.s_commonStyles,
					changes:				ref exitChanges
				);
				if (exitConfig != spline.ExitConfigSelected)
				{
					Undo.RecordObject(spline, STR_UNDO_EXIT_CONFIG);
					spline.ExitConfigSelected = exitConfig;
					EditorUtility.SetDirty(spline);
					configChanges = true;
				}
				if (exitChanges && exitConfig != null)
				{
					EditorUtility.SetDirty(exitConfig);
					configChanges = true;
				}
			}

			if (configChanges)
			{
				EditorUtils.SetDirtyCurrentScene();
				spline.MarkEnterExitDirty();
				spline.RequestWriteBack();
				SceneView.RepaintAll();
			}
		}

		// The slider follows the live scroll in play mode
		public override bool RequiresConstantRepaint()
		{
			return Application.isPlaying && spline != null && (spline.ScrollMode != SplineScrollMode.NONE || spline.IsTraveling);
		}

		private void DrawMovementSection()
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			bool changed = false;

			changed |= ResettablePropertyDrawer.DrawEnum(spScrollMode, (int)AllIn1TextSpline.DEFAULT_SCROLL_MODE);

			GUI.enabled = spScrollMode.enumValueIndex != (int)SplineScrollMode.NONE;
			changed |= ResettablePropertyDrawer.Draw(spScrollSpeed, AllIn1TextSpline.DEFAULT_SCROLL_SPEED);
			GUI.enabled = true;

			bool scrollingLive = Application.isPlaying && spScrollMode.enumValueIndex != (int)SplineScrollMode.NONE;
			GUI.enabled = !scrollingLive;
			changed |= ResettablePropertyDrawer.Draw(spPosition, AllIn1TextSpline.DEFAULT_POSITION);
			GUI.enabled = true;

			// ApplyModifiedProperties runs OnValidate, which restarts the scroll from the new values and writes the mesh
			if (changed)
			{
				serializedObject.ApplyModifiedProperties();
				SceneView.RepaintAll();
			}

			EditorGUILayout.EndVertical();
		}

		private void DrawLayoutSection()
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			bool changed = false;

			changed |= ResettablePropertyDrawer.Draw(spAutoFit, AllIn1TextSpline.DEFAULT_AUTO_FIT);
			GUI.enabled = !spAutoFit.boolValue;
			changed |= ResettablePropertyDrawer.Draw(spSpacing, AllIn1TextSpline.DEFAULT_SPACING);
			GUI.enabled = true;

			changed |= ResettablePropertyDrawer.Draw(spBaselineOffset, AllIn1TextSpline.DEFAULT_BASELINE_OFFSET);

			GUI.enabled = !spClosed.boolValue;
			changed |= ResettablePropertyDrawer.Draw(spClampEnds, AllIn1TextSpline.DEFAULT_CLAMP_ENDS);
			GUI.enabled = true;

			changed |= ResettablePropertyDrawer.Draw(spRotateWithCurve, AllIn1TextSpline.DEFAULT_ROTATE_WITH_CURVE);

			if (changed)
			{
				serializedObject.ApplyModifiedProperties();
				spline.RequestWriteBack();
				SceneView.RepaintAll();
			}

			EditorGUILayout.EndVertical();
		}

		private void DrawEditToggle()
		{
			Color previousColor = GUI.backgroundColor;
			if (editing)
			{
				GUI.backgroundColor = COLOR_POINT_SELECTED;
			}

			if (GUILayout.Button(editing ? LABEL_EDITING_SPLINE : LABEL_EDIT_SPLINE, GUILayout.Height(28f)))
			{
				SetEditing(!editing);
			}

			GUI.backgroundColor = previousColor;

			if (editing)
			{
				EditorGUILayout.HelpBox(STR_EDIT_HINT, MessageType.Info);
			}
		}

		private void DrawSplineSection()
		{
			EditorGUILayout.BeginVertical(DrawersSharedConfig.s_commonStyles.boxStyle);

			bool changed = false;
			changed |= ResettablePropertyDrawer.Draw(spClosed, AllIn1TextSpline.DEFAULT_CLOSED);
			changed |= ResettablePropertyDrawer.Draw(spCurveResolution, AllIn1TextSpline.DEFAULT_CURVE_RESOLUTION);
			if (changed)
			{
				serializedObject.ApplyModifiedProperties();
				spline.SetPathDirty();
				spline.RequestWriteBack();
				SceneView.RepaintAll();
			}

			EditorUIUtils.AddSpaces(1);
			DrawPointsToolbar();
			EditorUIUtils.AddSpaces(1);
			DrawSelectedPoint();

			EditorGUILayout.EndVertical();
		}

		private void DrawPointsToolbar()
		{
			int pointCount = spline.PointCount;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent($"Points: {pointCount}", "Number of control points on the curve."), GUILayout.Width(80f));

			EditorGUI.BeginChangeCheck();
			int newSelected = EditorGUILayout.IntSlider(LABEL_SELECTED, spline.selectedPointIndex, -1, pointCount - 1);
			if (EditorGUI.EndChangeCheck())
			{
				spline.selectedPointIndex = newSelected;
				SceneView.RepaintAll();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(LABEL_ADD))
			{
				Undo.RecordObject(spline, STR_UNDO_ADD_POINT);
				spline.selectedPointIndex = spline.AppendPoint();
				MarkDirty();
			}

			GUI.enabled = spline.selectedPointIndex >= 0;
			if (GUILayout.Button(LABEL_INSERT_AFTER))
			{
				Undo.RecordObject(spline, STR_UNDO_ADD_POINT);
				spline.selectedPointIndex = spline.InsertPointAfter(spline.selectedPointIndex);
				MarkDirty();
			}

			GUI.enabled = spline.selectedPointIndex >= 0 && pointCount > AllIn1TextSpline.MIN_POINTS;
			if (GUILayout.Button(LABEL_REMOVE))
			{
				RemoveSelectedPoint();
			}
			GUI.enabled = true;

			if (GUILayout.Button(LABEL_RESET_POINTS))
			{
				Undo.RecordObject(spline, STR_UNDO_RESET_POINTS);
				spline.SeedDefaultPoints();
				spline.selectedPointIndex = -1;
				MarkDirty();
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawSelectedPoint()
		{
			int index = spline.selectedPointIndex;
			if (index < 0 || index >= spline.PointCount) { return; }

			AllIn1TextSpline.SplinePoint point = spline.GetPoint(index);

			EditorGUILayout.LabelField($"Point {index}", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			Vector2 newPosition = EditorGUILayout.Vector2Field(LABEL_POINT_POSITION, point.position);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(spline, STR_UNDO_MOVE_POINT);
				spline.SetPointPosition(index, newPosition);
				MarkDirty();
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(LABEL_CURVATURE);
			EditorGUI.BeginChangeCheck();
			int newMode = GUILayout.Toolbar((int)point.tangentMode, TANGENT_MODE_LABELS);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(spline, STR_UNDO_TANGENT_MODE);
				spline.SetTangentMode(index, (SplineTangentMode)newMode);
				MarkDirty();
			}
			EditorGUILayout.EndHorizontal();

			if (point.tangentMode != SplineTangentMode.AUTO)
			{
				EditorGUI.BeginChangeCheck();
				Vector2 newIn = EditorGUILayout.Vector2Field(LABEL_TANGENT_IN, point.tangentIn);
				Vector2 newOut = EditorGUILayout.Vector2Field(LABEL_TANGENT_OUT, point.tangentOut);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(spline, STR_UNDO_MOVE_TANGENT);
					if (newIn != point.tangentIn) { spline.SetTangentIn(index, newIn); }
					if (newOut != point.tangentOut) { spline.SetTangentOut(index, newOut); }
					MarkDirty();
				}
			}
		}

		private void RemoveSelectedPoint()
		{
			Undo.RecordObject(spline, STR_UNDO_REMOVE_POINT);
			if (spline.RemovePoint(spline.selectedPointIndex))
			{
				spline.selectedPointIndex = Mathf.Min(spline.selectedPointIndex, spline.PointCount - 1);
				MarkDirty();
			}
		}

		// Curve edits push the text onto the new curve right away
		private void MarkDirty()
		{
			EditorUtility.SetDirty(spline);
			serializedObject.Update();
			spline.RequestWriteBack();
			SceneView.RepaintAll();
		}

		private void OnSceneGUI()
		{
			SplinePath path = spline.Path;

			curveWorldPoints = BuildCurveWorldPoints(spline, path, curveWorldPoints);
			DrawCurve(curveWorldPoints, COLOR_CURVE);
			DrawVisibleWindow(path);
			DrawDirectionArrows(path);

			if (!editing) { return; }

			// Keeps clicks on empty space from selecting other objects while the curve is being edited
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

			HandleKeyboard();
			HandleShiftInsert();
			DrawPointHandles();
		}

		private void HandleKeyboard()
		{
			Event current = Event.current;
			if (current.type != EventType.KeyDown) { return; }

			if (current.keyCode == KeyCode.Delete || current.keyCode == KeyCode.Backspace)
			{
				if (spline.selectedPointIndex >= 0 && spline.PointCount > AllIn1TextSpline.MIN_POINTS)
				{
					RemoveSelectedPoint();
					current.Use();
				}
			}
			else if (current.keyCode == KeyCode.Escape)
			{
				SetEditing(false);
				current.Use();
			}
		}

		// Shift shows a ghost point under the cursor and shift+click adds it: on the curve it is inserted in place, anywhere else it goes after the selected point
		private void HandleShiftInsert()
		{
			Event current = Event.current;
			if (!current.shift) { return; }

			if (current.type == EventType.MouseMove)
			{
				HandleUtility.Repaint();
				return;
			}

			bool onCurve = curveWorldPoints.Length >= 2 && HandleUtility.DistanceToPolyLine(curveWorldPoints) <= INSERT_PICK_DISTANCE_PIXELS;

			Vector3 worldPosition;
			if (onCurve)
			{
				worldPosition = HandleUtility.ClosestPointToPolyLine(curveWorldPoints);
			}
			else if (!TryGetMouseWorldPosition(out worldPosition))
			{
				return;
			}

			if (current.type == EventType.Repaint)
			{
				Handles.color = COLOR_POINT_SELECTED;
				float size = HandleUtility.GetHandleSize(worldPosition) * INSERT_GHOST_SIZE;
				Handles.DotHandleCap(0, worldPosition, Quaternion.identity, size, EventType.Repaint);
			}
			else if (current.type == EventType.MouseDown && current.button == 0)
			{
				Vector2 localPosition = spline.WorldToLocal(worldPosition);

				Undo.RecordObject(spline, STR_UNDO_ADD_POINT);
				if (onCurve)
				{
					spline.selectedPointIndex = spline.InsertPointOnCurve(localPosition);
				}
				else
				{
					int insertAt = spline.selectedPointIndex >= 0 ? spline.selectedPointIndex + 1 : spline.PointCount;
					spline.InsertPoint(insertAt, localPosition);
					spline.selectedPointIndex = insertAt;
				}
				MarkDirty();
				current.Use();
			}
		}

		// Cursor projected onto the text plane, rejected if it does not land back under the cursor
		private bool TryGetMouseWorldPosition(out Vector3 worldPosition)
		{
			worldPosition = Vector3.zero;

			Vector2 mousePosition = Event.current.mousePosition;
			Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
			Plane plane = new Plane(spline.transform.forward, spline.transform.position);
			if (!plane.Raycast(ray, out float distance)) { return false; }

			worldPosition = ray.GetPoint(distance);

			Vector2 backProjected = HandleUtility.WorldToGUIPoint(worldPosition);
			return Vector2.Distance(backProjected, mousePosition) <= PLANE_HIT_TOLERANCE_PIXELS;
		}

		// Every point is a move handle so a single click+drag grabs it; grabbing also selects it.
		// Ids are reserved in a fixed order (point, tangent in, tangent out) so a selection change mid-drag does not shift them
		private void DrawPointHandles()
		{
			int pointCount = spline.PointCount;

			for (int i = 0; i < pointCount; i++)
			{
				int pointId = GUIUtility.GetControlID(FocusType.Passive);
				int tangentInId = GUIUtility.GetControlID(FocusType.Passive);
				int tangentOutId = GUIUtility.GetControlID(FocusType.Passive);

				bool selected = i == spline.selectedPointIndex;
				AllIn1TextSpline.SplinePoint point = spline.GetPoint(i);
				Vector3 worldPosition = spline.LocalToWorld(point.position);
				float handleSize = HandleUtility.GetHandleSize(worldPosition) * POINT_HANDLE_SIZE;

				if (selected)
				{
					DrawTangentHandles(i, point, worldPosition, tangentInId, tangentOutId);
				}

				Handles.color = selected ? COLOR_POINT_SELECTED : COLOR_POINT;
				EditorGUI.BeginChangeCheck();
				Vector3 newWorldPosition = Handles.FreeMoveHandle(pointId, worldPosition, handleSize, Vector3.zero, Handles.DotHandleCap);
				bool moved = EditorGUI.EndChangeCheck();

				if (!selected && GUIUtility.hotControl == pointId)
				{
					spline.selectedPointIndex = i;
					Repaint();
				}

				if (moved)
				{
					Undo.RecordObject(spline, STR_UNDO_MOVE_POINT);
					spline.SetPointPosition(i, spline.WorldToLocal(newWorldPosition));
					MarkDirty();
				}
			}
		}

		private void DrawTangentHandles(int index, AllIn1TextSpline.SplinePoint point, Vector3 worldPosition, int tangentInId, int tangentOutId)
		{
			Vector3 worldIn = spline.LocalToWorld(point.position + point.tangentIn);
			Vector3 worldOut = spline.LocalToWorld(point.position + point.tangentOut);

			Handles.color = COLOR_TANGENT;
			Handles.DrawLine(worldPosition, worldIn);
			Handles.DrawLine(worldPosition, worldOut);

			float inSize = HandleUtility.GetHandleSize(worldIn) * TANGENT_HANDLE_SIZE;
			EditorGUI.BeginChangeCheck();
			Vector3 newWorldIn = Handles.FreeMoveHandle(tangentInId, worldIn, inSize, Vector3.zero, Handles.DotHandleCap);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(spline, STR_UNDO_MOVE_TANGENT);
				spline.SetTangentIn(index, spline.WorldToLocal(newWorldIn) - point.position);
				MarkDirty();
			}

			float outSize = HandleUtility.GetHandleSize(worldOut) * TANGENT_HANDLE_SIZE;
			EditorGUI.BeginChangeCheck();
			Vector3 newWorldOut = Handles.FreeMoveHandle(tangentOutId, worldOut, outSize, Vector3.zero, Handles.DotHandleCap);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(spline, STR_UNDO_MOVE_TANGENT);
				spline.SetTangentOut(index, spline.WorldToLocal(newWorldOut) - point.position);
				MarkDirty();
			}
		}

		private static Vector3[] BuildCurveWorldPoints(AllIn1TextSpline spline, SplinePath path, Vector3[] reuse)
		{
			if (!path.IsValid) { return new Vector3[0]; }

			int segmentCount = path.SegmentCount;
			int count = segmentCount * DRAW_SAMPLES_PER_SEGMENT + 1;
			Vector3[] res = reuse != null && reuse.Length == count ? reuse : new Vector3[count];

			int idx = 0;
			for (int segment = 0; segment < segmentCount; segment++)
			{
				for (int step = 0; step < DRAW_SAMPLES_PER_SEGMENT; step++)
				{
					float t = (float)step / DRAW_SAMPLES_PER_SEGMENT;
					res[idx++] = spline.LocalToWorld(path.EvaluateSegment(segment, t));
				}
			}
			res[idx] = spline.LocalToWorld(path.EvaluateSegment(segmentCount - 1, 1f));

			return res;
		}

		private static void DrawCurve(Vector3[] worldPoints, Color color)
		{
			if (worldPoints.Length < 2) { return; }

			Handles.color = color;
			Handles.DrawAAPolyLine(CURVE_WIDTH, worldPoints);
		}

		// The visible window as a thick overlay, its enter and exit feathers in a lighter tint
		private void DrawVisibleWindow(SplinePath path)
		{
			if (!spline.EnterExitEnabled || !path.IsValid) { return; }

			float length = path.TotalLength;
			float windowStart = Mathf.Min(spline.VisibleStart, spline.VisibleEnd) * length;
			float windowEnd = Mathf.Max(spline.VisibleStart, spline.VisibleEnd) * length;
			float enterEnd = Mathf.Min(windowEnd, windowStart + spline.EnterLength * length);
			float exitStart = Mathf.Max(windowStart, windowEnd - spline.ExitLength * length);

			DrawArcRange(path, windowStart, windowEnd, COLOR_WINDOW);
			DrawArcRange(path, windowStart, enterEnd, COLOR_FEATHER);
			DrawArcRange(path, exitStart, windowEnd, COLOR_FEATHER);
		}

		private void DrawArcRange(SplinePath path, float sFrom, float sTo, Color color)
		{
			if (sTo <= sFrom) { return; }

			int samples = Mathf.Max(2, Mathf.CeilToInt(WINDOW_SAMPLES * (sTo - sFrom) / path.TotalLength));
			Vector3[] points = new Vector3[samples];
			for (int i = 0; i < samples; i++)
			{
				float s = Mathf.Lerp(sFrom, sTo, (float)i / (samples - 1));
				path.SampleByArcLength(s, out Vector2 position, out _);
				points[i] = spline.LocalToWorld(position);
			}

			Handles.color = color;
			Handles.DrawAAPolyLine(WINDOW_WIDTH, points);
		}

		private void DrawDirectionArrows(SplinePath path)
		{
			if (!path.IsValid) { return; }

			Handles.color = COLOR_CURVE;
			for (int i = 1; i <= ARROWS_ALONG_CURVE; i++)
			{
				float s = path.TotalLength * i / (ARROWS_ALONG_CURVE + 1);
				path.SampleByArcLength(s, out Vector2 position, out Vector2 tangent);

				Vector3 tip = spline.LocalToWorld(position);
				float size = HandleUtility.GetHandleSize(tip) * ARROW_SIZE;
				Vector2 normal = new Vector2(-tangent.y, tangent.x);

				Vector3 left = spline.LocalToWorld(position - tangent * size + normal * size * 0.5f);
				Vector3 right = spline.LocalToWorld(position - tangent * size - normal * size * 0.5f);

				Handles.DrawLine(left, tip);
				Handles.DrawLine(right, tip);
			}
		}

		// The curve stays visible when the object is not selected so the layout can be read at a glance
		[DrawGizmo(GizmoType.NonSelected | GizmoType.Pickable)]
		private static void DrawUnselectedCurve(AllIn1TextSpline spline, GizmoType gizmoType)
		{
			DrawCurve(BuildCurveWorldPoints(spline, spline.Path, null), COLOR_CURVE_UNSELECTED);
		}
	}
}
