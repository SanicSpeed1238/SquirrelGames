using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace AllIn1Assets.AllIn1TextShader
{
	// Runs before the typewriter so the progress it hands over is applied in the same frame the letters move
	[DefaultExecutionOrder(-10)]
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[AddComponentMenu("AllIn1TextShader/Spline")]
	public class AllIn1TextSpline : MonoBehaviour, ITextLayoutStage
	{
		[System.Serializable]
		public struct SplinePoint
		{
			public Vector2 position;
			public Vector2 tangentIn;
			public Vector2 tangentOut;
			public SplineTangentMode tangentMode;

			public SplinePoint(Vector2 position)
			{
				this.position = position;
				this.tangentIn = Vector2.zero;
				this.tangentOut = Vector2.zero;
				this.tangentMode = SplineTangentMode.AUTO;
			}
		}

		public const int MIN_CURVE_RESOLUTION = 4;
		public const int MAX_CURVE_RESOLUTION = 64;
		public const int DEFAULT_CURVE_RESOLUTION = 16;
		public const int MIN_POINTS = 2;
		public const float MIN_SPACING = 0.01f;

		public const bool DEFAULT_CLOSED = false;
		public const float DEFAULT_SPACING = 1f;
		public const bool DEFAULT_AUTO_FIT = false;
		public const float DEFAULT_BASELINE_OFFSET = 0f;
		public const bool DEFAULT_CLAMP_ENDS = false;
		public const bool DEFAULT_ROTATE_WITH_CURVE = true;
		public const SplineScrollMode DEFAULT_SCROLL_MODE = SplineScrollMode.NONE;
		public const float DEFAULT_SCROLL_SPEED = 50f;
		public const float DEFAULT_POSITION = 0f;
		public const bool DEFAULT_ENTER_EXIT_ENABLED = false;
		public const float DEFAULT_VISIBLE_START = 0f;
		public const float DEFAULT_VISIBLE_END = 1f;
		public const float DEFAULT_ENTER_LENGTH = 0.15f;
		public const float DEFAULT_EXIT_LENGTH = 0.15f;
		public const bool DEFAULT_USE_ENTER_AS_EXIT = true;
		public const bool DEFAULT_START_HIDDEN = false;
		public const SplineTravelDirection DEFAULT_TRAVEL_DIRECTION = SplineTravelDirection.FORWARD;
		public const float DEFAULT_TRAVEL_DURATION = 1f;
		public const float MIN_TRAVEL_DURATION = 0.01f;
		public const bool DEFAULT_DRIVE_TYPEWRITER = true;
		public const bool DEFAULT_OVERRIDE_TYPEWRITER_TIMING = true;
		public const bool DEFAULT_INDIVIDUAL_LETTER_TRAVEL = true;
		private const float MIN_LETTER_DISTANCE = 0.0001f;

		// Parked text sits this far past the window edge so no character touches it
		private const float PARK_MARGIN = 1f;

		private const string STR_ENTER_STATE = "SplineEnterState";
		private const string STR_EXIT_STATE = "SplineExitState";

		private const float DEFAULT_HALF_WIDTH = 100f;
		private const float DEFAULT_ARCH_HEIGHT = 40f;
		private const float AUTO_TANGENT_FACTOR = 1f / 3f;
		private const int VERTICES_PER_CHARACTER = 4;

		// Positions and tangents are in the text's local space, XY plane
		[SerializeField] private List<SplinePoint> points = new List<SplinePoint>();
		[SerializeField, Tooltip("Joins the last point back to the first. Text can travel around the loop with the Loop scroll mode.")] private bool closed = DEFAULT_CLOSED;
		[SerializeField, Range(MIN_CURVE_RESOLUTION, MAX_CURVE_RESOLUTION), Tooltip("Samples per segment for the arc length table. Higher keeps character spacing even on tight bends, at a small CPU cost.")] private int curveResolution = DEFAULT_CURVE_RESOLUTION;

		[SerializeField, Tooltip("Stretches or compresses the text along the curve. 1 keeps the flat text's own spacing.")] private float spacing = DEFAULT_SPACING;
		[SerializeField, Tooltip("Sets the spacing so the text covers the whole curve. Spacing is ignored while this is on.")] private bool autoFit = DEFAULT_AUTO_FIT;
		[SerializeField, Tooltip("Moves the text sideways from the curve, along its normal. Positive is the left side of the travel direction.")] private float baselineOffset = DEFAULT_BASELINE_OFFSET;
		[SerializeField, Tooltip("Open curves only. Characters past the ends pile up at the end instead of disappearing.")] private bool clampEnds = DEFAULT_CLAMP_ENDS;
		[SerializeField, Tooltip("Turns each character to follow the curve's tangent, effects included. Off keeps every character upright and only moves it.")] private bool rotateWithCurve = DEFAULT_ROTATE_WITH_CURVE;

		[SerializeField, Tooltip("How the text moves along the curve in play mode. None stays at Position, Ping Pong travels the free length and bounces, Loop travels the whole curve and wraps.")] private SplineScrollMode scrollMode = DEFAULT_SCROLL_MODE;
		[SerializeField, Tooltip("Scroll speed in curve units per second (the text's local units, pixels for UI text).")] private float scrollSpeed = DEFAULT_SCROLL_SPEED;
		// Normalized place of the text along its travel range. Edit mode scrubs it, play mode mirrors the live offset into it
		[SerializeField, Range(0f, 1f), Tooltip("Where the text sits along its travel range, 0 to 1. Scrub it in edit mode to place the text. It is also the rest spot Show() travels to. In play mode it follows the live scroll.")] private float position = DEFAULT_POSITION;

		// Enter/exit: a visible window along the curve (normalized 0..1 of its length) with a feather at each end
		// where the enter or exit effects play, driven by the character's position instead of time
		[SerializeField, Tooltip("Only a window of the curve shows characters. Characters play the Enter effects as they move into the window and the Exit effects as they move out. Driven by position, not time.")] private bool enterExitEnabled = DEFAULT_ENTER_EXIT_ENABLED;
		[SerializeField, Range(0f, 1f), Tooltip("Start of the visible window as a fraction of the curve length.")] private float visibleStart = DEFAULT_VISIBLE_START;
		[SerializeField, Range(0f, 1f), Tooltip("End of the visible window as a fraction of the curve length.")] private float visibleEnd = DEFAULT_VISIBLE_END;
		[SerializeField, Range(0f, 1f), Tooltip("Length of the entering zone at the window's start, as a fraction of the curve. A character crossing it plays the Enter effects from 0 to 1.")] private float enterLength = DEFAULT_ENTER_LENGTH;
		[SerializeField, Range(0f, 1f), Tooltip("Length of the leaving zone at the window's end, as a fraction of the curve. A character crossing it plays the Exit effects from 0 to 1.")] private float exitLength = DEFAULT_EXIT_LENGTH;
		[SerializeField, Tooltip("Effects played while a character crosses the entering zone. Same presets as the typewriter's Reveal.")] private MeshEffectGroupConfig enterConfig;
		[SerializeField, Tooltip("Effects played while a character crosses the leaving zone. Same presets as the typewriter's Hide.")] private MeshEffectGroupConfig exitConfigSelected;
		[SerializeField, Tooltip("Builds the exit from the Enter effects played backwards, like the typewriter's Hide does with Reveal. Off lets you pick a separate Exit preset.")] private bool useEnterAsExitConfig = DEFAULT_USE_ENTER_AS_EXIT;

		// Travel: Show() moves the text from outside the window to the Position slider's rest spot, Hide() moves it out the other side
		[SerializeField, Tooltip("In play mode the text starts parked outside the window and waits for Show().")] private bool startHidden = DEFAULT_START_HIDDEN;
		[SerializeField, Tooltip("Forward enters through the window's start and leaves through its end. Backward does the opposite.")] private SplineTravelDirection travelDirection = DEFAULT_TRAVEL_DIRECTION;
		[SerializeField, Tooltip("Seconds Show() and Hide() take. With Individual Letter Travel it is the time of the letter with the longest trip; the others arrive earlier at the same speed.")] private float travelDuration = DEFAULT_TRAVEL_DURATION;
		[SerializeField, Tooltip("Each letter makes its own trip between the window edge and its spot, all at one speed, so letters near the edge land first and every letter finishes its effects the frame it lands. Off moves the text as one block.")] private bool individualLetterTravel = DEFAULT_INDIVIDUAL_LETTER_TRAVEL;
		[SerializeField, Tooltip("Shapes the travel's motion over time. Only the motion: the reveal and hide follow the travel's linear progress.")] private AnimationCurve travelEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		// Show() also starts the typewriter's reveal and Hide() its hide, so its time-driven effects and loops play along
		[SerializeField, Tooltip("Show() also plays the typewriter's Reveal and Hide() its Hide, which also starts its looped effects.")] private bool driveTypewriter = DEFAULT_DRIVE_TYPEWRITER;
		// The travel sets the reveal and hide progress (0 at departure, 1 at arrival) instead of the typewriter's own timing
		[SerializeField, Tooltip("The travel sets the reveal and hide progress: it starts on departure and ends on arrival. As one block every letter shares it; with Individual Letter Travel each letter follows its own trip. Off lets the typewriter use its own timing and the preset's stagger.")] private bool overrideTypewriterTiming = DEFAULT_OVERRIDE_TYPEWRITER_TIMING;

		public UnityEvent onShowCompletedEvent = new UnityEvent();
		public UnityEvent onHideCompletedEvent = new UnityEvent();

#if UNITY_EDITOR
		[HideInInspector] public int selectedPointIndex = -1;
		[HideInInspector] public bool splineUnfolded = true;
		[HideInInspector] public bool layoutUnfolded = true;
		[HideInInspector] public bool movementUnfolded = true;
		[HideInInspector] public bool enterExitUnfolded = true;
		[HideInInspector] public bool eventsUnfolded = false;
#endif

		private SplinePath path;
		private List<SplineKnot> resolvedKnots;
		private bool pathDirty = true;

		// Arc-length offset added to every character
		private float scrollOffset = 0f;
		private float scrollRange = 0f;
		private float pingPongDirection = 1f;
		private bool scrollInitialized = false;

		private MeshEffectGroupState enterState;
		private MeshEffectGroupState exitState;
		private VertexGroupLayout[] enterLayouts = new VertexGroupLayout[0];
		private VertexGroupLayout[] exitLayouts = new VertexGroupLayout[0];
		private MeshEffectGroupConfig exitConfigRuntime;
		private bool ownsExitConfigRuntime = false;
		private bool enterExitStatesDirty = true;
		private int builtQuadCount = -1;

		private bool traveling = false;
		private bool travelIsShow = false;
		private float travelTimer = 0f;
		private float travelFrom = 0f;
		private float travelTo = 0f;
		// Progress handed to the typewriter: starts here (mid-travel turnarounds continue from the current state) and ends at 1
		private float typewriterProgressFrom = 0f;
		private float lastTypewriterProgress = 0f;

		// Per quad (main submesh) data for the individual letter travel
		private float[] quadRestS = new float[0];
		private float[] letterFromS = new float[0];
		private float[] letterToS = new float[0];
		private float[] letterProgressFrom = new float[0];
		private float[] letterDelay = new float[0];
		private float[] quadCurrentS = new float[0];
		private float[] quadProgress = new float[0];
		private float letterSpeed = 0f;
		private bool letterTravelActive = false;
		private bool shown = true;

		public event System.Action OnShowCompleted;
		public event System.Action OnHideCompleted;

		// One character at a time goes through the enter/exit states and the placement
		private readonly Vector3[] quadVerts = new Vector3[VERTICES_PER_CHARACTER];
		private readonly Color32[] quadColors = new Color32[VERTICES_PER_CHARACTER];

		public int PointCount { get { return points.Count; } }
		public bool Closed
		{
			get { return closed; }
			set { closed = value; SetPathDirty(); }
		}
		public int CurveResolution
		{
			get { return curveResolution; }
			set { curveResolution = Mathf.Clamp(value, MIN_CURVE_RESOLUTION, MAX_CURVE_RESOLUTION); SetPathDirty(); }
		}
		public float Spacing
		{
			get { return spacing; }
			set { spacing = Mathf.Max(MIN_SPACING, value); }
		}
		public bool AutoFit
		{
			get { return autoFit; }
			set { autoFit = value; }
		}
		public float BaselineOffset
		{
			get { return baselineOffset; }
			set { baselineOffset = value; }
		}
		public bool ClampEnds
		{
			get { return clampEnds; }
			set { clampEnds = value; }
		}
		public bool RotateWithCurve
		{
			get { return rotateWithCurve; }
			set { rotateWithCurve = value; }
		}

		public SplineScrollMode ScrollMode
		{
			get { return scrollMode; }
			set { scrollMode = value; scrollInitialized = false; }
		}
		public float ScrollSpeed
		{
			get { return scrollSpeed; }
			set { scrollSpeed = value; }
		}
		// 0..1 along the travel range; setting it in play mode restarts the scroll from there
		public float Position
		{
			get { return position; }
			set { position = Mathf.Clamp01(value); scrollInitialized = false; }
		}

		public bool EnterExitEnabled
		{
			get { return enterExitEnabled; }
			set { enterExitEnabled = value; }
		}
		public float VisibleStart
		{
			get { return visibleStart; }
			set { visibleStart = Mathf.Clamp01(value); }
		}
		public float VisibleEnd
		{
			get { return visibleEnd; }
			set { visibleEnd = Mathf.Clamp01(value); }
		}
		public float EnterLength
		{
			get { return enterLength; }
			set { enterLength = Mathf.Clamp01(value); }
		}
		public float ExitLength
		{
			get { return exitLength; }
			set { exitLength = Mathf.Clamp01(value); }
		}
		public MeshEffectGroupConfig EnterConfig
		{
			get { return enterConfig; }
			set { enterConfig = value; MarkEnterExitDirty(); }
		}
		public MeshEffectGroupConfig ExitConfigSelected
		{
			get { return exitConfigSelected; }
			set { exitConfigSelected = value; MarkEnterExitDirty(); }
		}
		public bool UseEnterAsExitConfig
		{
			get { return useEnterAsExitConfig; }
			set { useEnterAsExitConfig = value; MarkEnterExitDirty(); }
		}

		public bool StartHidden
		{
			get { return startHidden; }
			set { startHidden = value; }
		}
		public SplineTravelDirection TravelDirection
		{
			get { return travelDirection; }
			set { travelDirection = value; }
		}
		public float TravelDuration
		{
			get { return travelDuration; }
			set { travelDuration = Mathf.Max(MIN_TRAVEL_DURATION, value); }
		}
		public AnimationCurve TravelEase
		{
			get { return travelEase; }
			set { travelEase = value; }
		}
		public bool DriveTypewriter
		{
			get { return driveTypewriter; }
			set { driveTypewriter = value; }
		}
		public bool OverrideTypewriterTiming
		{
			get { return overrideTypewriterTiming; }
			set { overrideTypewriterTiming = value; }
		}
		public bool IndividualLetterTravel
		{
			get { return individualLetterTravel; }
			set { individualLetterTravel = value; }
		}
		public bool IsShown { get { return shown; } }
		public bool IsTraveling { get { return traveling; } }

		// Flat text width (max x - min x) from the last placement, before spacing
		public float FlatTextWidth { get; private set; }

		// Width of the text along the curve after spacing, from the last placement
		public float TextArcWidth { get; private set; }

		public SplinePath Path
		{
			get
			{
				if (path == null || pathDirty)
				{
					RebuildPath();
				}
				return path;
			}
		}

		private void OnEnable()
		{
			scrollInitialized = false;
			MarkEnterExitDirty();
			AttachToMeshHandler();
		}

		private void OnDisable()
		{
			DetachFromMeshHandler();
		}

		private void OnDestroy()
		{
			ReleaseExitConfigRuntime();
		}

		// Also runs after undo and redo, which bypass the inspector's own write-back
		private void OnValidate()
		{
			spacing = Mathf.Max(MIN_SPACING, spacing);
			travelDuration = Mathf.Max(MIN_TRAVEL_DURATION, travelDuration);
			scrollInitialized = false;
			MarkEnterExitDirty();
			SetPathDirty();
			RequestWriteBack();
		}

		// Missing configs fall back to the first of each collection, like the typewriter does
		public bool RefreshConfigReferencesIfNeeded(MeshEffectConfigCollection revealCollection, MeshEffectConfigCollection hideCollection)
		{
			bool res = false;

			if (enterConfig == null && revealCollection != null && revealCollection.configs != null && revealCollection.configs.Length > 0)
			{
				enterConfig = revealCollection.configs[0];
				res = true;
			}

			if (exitConfigSelected == null && hideCollection != null && hideCollection.configs != null && hideCollection.configs.Length > 0)
			{
				exitConfigSelected = hideCollection.configs[0];
				res = true;
			}

			if (res)
			{
				MarkEnterExitDirty();
			}

			return res;
		}

		private void Update()
		{
			if (!Application.isPlaying || !scrollInitialized) { return; }

			bool moved = false;
			if (traveling)
			{
				AdvanceTravel(Time.deltaTime);
				moved = true;
			}
			else if (scrollMode != SplineScrollMode.NONE)
			{
				AdvanceScroll(Time.deltaTime);
				moved = true;
			}

			// A playing typewriter writes the mesh every frame on its own; without one, or with one stopped, the spline has to
			if (moved && !IsTypewriterWritingEveryFrame())
			{
				RequestWriteBack();
			}
		}

		// Travels from the entry edge of the window to the rest spot. Every letter goes from 0 to 1 on the way and lands finished
		public void Show()
		{
			EnsureScrollInitialized();

			// Already on its way in: nothing to do. Already resting: replay the entrance from the edge
			if (traveling && travelIsShow) { return; }
			bool turnaround = traveling;
			if (!traveling && shown) { Park(); }

			float restOffset = GetRestOffset();
			float progressFrom = turnaround ? 1f - lastTypewriterProgress : 0f;

			if (individualLetterTravel && BeginLetterTravel(true, restOffset, turnaround))
			{
				StartTravel(scrollOffset, restOffset, true, progressFrom);
				scrollOffset = restOffset;
			}
			else
			{
				letterTravelActive = false;
				float from = (traveling || shown) ? scrollOffset : GetInsideEdgeOffset(travelDirection == SplineTravelDirection.FORWARD);
				StartTravel(from, restOffset, true, progressFrom);
			}

			DriveTypewriterStart(true, progressFrom);
		}

		// Travels from where it is to the exit edge of the window, letters going from 1 to 0, then parks out of sight
		public void Hide()
		{
			EnsureScrollInitialized();

			// Already on its way out, or already hidden: nothing to do
			if (traveling && !travelIsShow) { return; }
			if (!traveling && !shown) { return; }
			bool turnaround = traveling;

			float restOffset = GetRestOffset();
			float progressFrom = turnaround ? 1f - lastTypewriterProgress : 0f;

			if (individualLetterTravel && BeginLetterTravel(false, restOffset, turnaround))
			{
				StartTravel(scrollOffset, scrollOffset, false, progressFrom);
			}
			else
			{
				letterTravelActive = false;
				StartTravel(scrollOffset, GetInsideEdgeOffset(travelDirection != SplineTravelDirection.FORWARD), false, progressFrom);
			}

			DriveTypewriterStart(false, progressFrom);
		}

		private void DriveTypewriterStart(bool isShow, float progressFrom)
		{
			AllIn1TextTypewriter typewriter = GetDrivenTypewriter();
			if (typewriter == null) { return; }

			if (!overrideTypewriterTiming)
			{
				if (isShow) { typewriter.Reveal(); } else { typewriter.Hide(); }
				return;
			}

			if (letterTravelActive)
			{
				if (isShow) { typewriter.RevealAtProgress(quadProgress); } else { typewriter.HideAtProgress(quadProgress); }
			}
			else
			{
				if (isShow) { typewriter.RevealAtProgress(progressFrom); } else { typewriter.HideAtProgress(progressFrom); }
			}
		}

		private AllIn1TextTypewriter GetDrivenTypewriter()
		{
			AllIn1TextTypewriter res = null;
			if (driveTypewriter && Application.isPlaying && TryGetComponent<AllIn1TextTypewriter>(out AllIn1TextTypewriter typewriter) && typewriter.isActiveAndEnabled)
			{
				res = typewriter;
			}
			return res;
		}

		// The ranges the travel needs come from the first placement
		private void EnsureScrollInitialized()
		{
			if (!scrollInitialized)
			{
				RequestWriteBack();
			}
		}

		public void ShowImmediate()
		{
			EnsureScrollInitialized();
			traveling = false;
			letterTravelActive = false;
			scrollOffset = GetRestOffset();
			shown = true;

			AllIn1TextTypewriter typewriter = GetDrivenTypewriter();
			if (typewriter != null) { typewriter.RevealImmediate(); }

			RequestWriteBack();
		}

		public void HideImmediate()
		{
			EnsureScrollInitialized();
			traveling = false;
			letterTravelActive = false;
			Park();

			AllIn1TextTypewriter typewriter = GetDrivenTypewriter();
			if (typewriter != null)
			{
				typewriter.HideAtProgress(1f);
				typewriter.FinishProgressControl();
			}

			RequestWriteBack();
		}

		private void Park()
		{
			scrollOffset = GetParkedOffset(travelDirection == SplineTravelDirection.FORWARD);
			shown = false;
		}

		private void StartTravel(float from, float to, bool isShow, float progressFrom)
		{
			traveling = true;
			travelIsShow = isShow;
			travelTimer = 0f;
			travelFrom = from;
			travelTo = to;
			typewriterProgressFrom = Mathf.Clamp01(progressFrom);
			lastTypewriterProgress = typewriterProgressFrom;
		}

		// Each letter gets its own trip: from where it is (or the entry edge) to its spot, or from its spot to the exit edge.
		// One shared speed, set so the longest trip takes Travel Duration
		private bool BeginLetterTravel(bool isShow, float restOffset, bool turnaround)
		{
			if (!TryComputeRestArcLengths(restOffset)) { return false; }

			int numQuads = quadRestS.Length;
			EnsureLetterArrays(numQuads);

			bool entryAtStart = travelDirection == SplineTravelDirection.FORWARD;
			float entryEdge = GetWindowEdge(entryAtStart);
			float exitEdge = GetWindowEdge(!entryAtStart);
			// A turnaround continues each letter from where it is; its progress flips because show and hide are inverses
			bool continuing = turnaround && letterTravelActive && quadCurrentS.Length == numQuads;

			Vector3[] src = GetMeshHandler().GetMeshCache().subMeshes[0].srcVerts;

			float maxDistance = 0f;
			for (int q = 0; q < numQuads; q++)
			{
				float from;
				if (continuing) { from = quadCurrentS[q]; }
				else if (shown) { from = quadRestS[q]; }
				else { from = entryEdge; }

				float to = isShow ? quadRestS[q] : exitEdge;

				letterFromS[q] = from;
				letterToS[q] = to;
				letterProgressFrom[q] = continuing ? Mathf.Clamp01(1f - quadProgress[q]) : 0f;
				quadCurrentS[q] = from;
				quadProgress[q] = letterProgressFrom[q];

				// Collapsed quads (decorations, spare capacity) are never drawn, so their trip must not set the speed
				if (!IsDegenerateQuad(src, q * VERTICES_PER_CHARACTER))
				{
					maxDistance = Mathf.Max(maxDistance, Mathf.Abs(to - from));
				}
			}

			float duration = Mathf.Max(MIN_TRAVEL_DURATION, travelDuration);
			letterSpeed = maxDistance / duration;

			// Hide: everyone leaves at once and the nearest letters reach the edge first. Show is its time reverse:
			// the farthest letter leaves the edge first and every letter settles in the same frame
			for (int q = 0; q < numQuads; q++)
			{
				float tripDuration = letterSpeed > 0f ? Mathf.Abs(letterToS[q] - letterFromS[q]) / letterSpeed : 0f;
				letterDelay[q] = (isShow && !continuing) ? Mathf.Max(0f, duration - tripDuration) : 0f;
			}

			letterTravelActive = true;
			return true;
		}

		private void EnsureLetterArrays(int numQuads)
		{
			if (letterFromS.Length == numQuads) { return; }

			letterFromS = new float[numQuads];
			letterToS = new float[numQuads];
			letterProgressFrom = new float[numQuads];
			letterDelay = new float[numQuads];
			quadCurrentS = new float[numQuads];
			quadProgress = new float[numQuads];
		}

		// Arc length of each quad's spot along the curve at a given scroll offset, from the flat rest pose
		private bool TryComputeRestArcLengths(float scrollOffsetForRest)
		{
			TextMeshHandler meshHandler = GetMeshHandler();
			if (meshHandler == null) { return false; }

			MeshCache meshCache = meshHandler.GetMeshCache();
			if (meshCache == null || meshCache.subMeshes == null || meshCache.subMeshes.Length == 0) { return false; }

			Vector3[] src = meshCache.subMeshes[0].srcVerts;
			if (src == null || !Path.IsValid) { return false; }
			if (!ComputeLayoutMetrics(meshCache, Path, out float minX, out _, out float effectiveSpacing)) { return false; }

			int numQuads = src.Length / VERTICES_PER_CHARACTER;
			if (quadRestS.Length != numQuads)
			{
				quadRestS = new float[numQuads];
			}

			for (int q = 0; q < numQuads; q++)
			{
				int i = q * VERTICES_PER_CHARACTER;
				float pivotX = (src[i].x + src[i + 3].x) * 0.5f;
				quadRestS[q] = (pivotX - minX) * effectiveSpacing + scrollOffsetForRest;
			}

			return true;
		}

		private void AdvanceTravel(float deltaTime)
		{
			travelTimer += deltaTime;
			float duration = Mathf.Max(MIN_TRAVEL_DURATION, travelDuration);
			float t = Mathf.Clamp01(travelTimer / duration);

			AllIn1TextTypewriter typewriter = overrideTypewriterTiming ? GetDrivenTypewriter() : null;

			if (letterTravelActive)
			{
				AdvanceLetters();
				lastTypewriterProgress = Mathf.Lerp(typewriterProgressFrom, 1f, t);

				if (typewriter != null)
				{
					if (travelIsShow) { typewriter.RevealAtProgress(quadProgress); }
					else { typewriter.HideAtProgress(quadProgress); }
				}
			}
			else
			{
				float eased = travelEase != null ? travelEase.Evaluate(t) : t;
				scrollOffset = Mathf.LerpUnclamped(travelFrom, travelTo, eased);

				// Same progress for every letter: 0 when it starts moving, 1 the frame it lands
				lastTypewriterProgress = Mathf.Lerp(typewriterProgressFrom, 1f, t);
				if (typewriter != null)
				{
					if (travelIsShow) { typewriter.RevealAtProgress(lastTypewriterProgress); }
					else { typewriter.HideAtProgress(lastTypewriterProgress); }
				}
			}

			if (t < 1f) { return; }

			if (!letterTravelActive)
			{
				scrollOffset = travelTo;
			}
			traveling = false;
			letterTravelActive = false;
			shown = travelIsShow;
			pingPongDirection = 1f;

			if (typewriter != null)
			{
				typewriter.FinishProgressControl();
			}

			if (travelIsShow)
			{
				onShowCompletedEvent.Invoke();
				if (OnShowCompleted != null) { OnShowCompleted(); }
			}
			else
			{
				// The letters are at their hidden values now, so moving them out of the window changes nothing visible
				Park();
				onHideCompletedEvent.Invoke();
				if (OnHideCompleted != null) { OnHideCompleted(); }
			}
		}

		// Every letter moves at the shared speed along its own trip; its progress is its own trip fraction
		private void AdvanceLetters()
		{
			for (int q = 0; q < quadCurrentS.Length; q++)
			{
				float distance = Mathf.Abs(letterToS[q] - letterFromS[q]);
				float letterTimer = travelTimer - letterDelay[q];
				float letterT = distance > MIN_LETTER_DISTANCE ? Mathf.Clamp01(letterTimer * letterSpeed / distance) : Mathf.Clamp01(letterTimer >= 0f ? 1f : 0f);
				float eased = travelEase != null ? travelEase.Evaluate(letterT) : letterT;

				quadCurrentS[q] = Mathf.LerpUnclamped(letterFromS[q], letterToS[q], eased);
				quadProgress[q] = Mathf.Lerp(letterProgressFrom[q], 1f, letterT);
			}
		}

		private float GetWindowEdge(bool startEdge)
		{
			GetWindowRange(out float windowStart, out float windowEnd);
			return startEdge ? windowStart : windowEnd;
		}

		// The text fully inside the window and touching one of its edges
		private float GetInsideEdgeOffset(bool startEdge)
		{
			GetWindowRange(out float windowStart, out float windowEnd);

			float res;
			if (startEdge)
			{
				res = windowStart;
			}
			else
			{
				res = Mathf.Max(windowStart, windowEnd - TextArcWidth);
			}
			return res;
		}

		private float GetRestOffset()
		{
			return position * scrollRange;
		}

		// An offset that leaves every character outside the window: before its start or after its end
		private float GetParkedOffset(bool beforeStart)
		{
			GetWindowRange(out float windowStart, out float windowEnd);

			float res;
			if (beforeStart)
			{
				res = windowStart - TextArcWidth - PARK_MARGIN;
			}
			else
			{
				res = windowEnd + PARK_MARGIN;
			}
			return res;
		}

		// The whole curve when enter/exit is off
		private void GetWindowRange(out float windowStart, out float windowEnd)
		{
			float length = Path.IsValid ? Path.TotalLength : 0f;
			if (enterExitEnabled)
			{
				windowStart = Mathf.Min(visibleStart, visibleEnd) * length;
				windowEnd = Mathf.Max(visibleStart, visibleEnd) * length;
			}
			else
			{
				windowStart = 0f;
				windowEnd = length;
			}
		}

		private bool IsTypewriterWritingEveryFrame()
		{
			return TryGetComponent<AllIn1TextTypewriter>(out AllIn1TextTypewriter typewriter) && typewriter.IsWritingEveryFrame();
		}

		private void AdvanceScroll(float deltaTime)
		{
			SplinePath currentPath = Path;
			if (!currentPath.IsValid) { return; }

			switch (scrollMode)
			{
				case SplineScrollMode.PING_PONG:
					scrollOffset += pingPongDirection * scrollSpeed * deltaTime;
					if (scrollOffset >= scrollRange)
					{
						scrollOffset = scrollRange;
						pingPongDirection = -1f;
					}
					else if (scrollOffset <= 0f)
					{
						scrollOffset = 0f;
						pingPongDirection = 1f;
					}
					break;
				case SplineScrollMode.LOOP:
					scrollOffset = Mathf.Repeat(scrollOffset + scrollSpeed * deltaTime, currentPath.TotalLength);
					break;
			}

			position = scrollRange > 0f ? Mathf.Clamp01(scrollOffset / scrollRange) : 0f;
		}

		// PING_PONG and NONE travel the free length of the curve; LOOP travels the whole curve and wraps
		private float GetScrollRange(SplinePath currentPath)
		{
			float res;
			if (scrollMode == SplineScrollMode.LOOP)
			{
				res = currentPath.TotalLength;
			}
			else
			{
				res = Mathf.Max(0f, currentPath.TotalLength - TextArcWidth);
			}
			return res;
		}

		// Edit mode follows the slider every time; play mode reads it once and then advances on its own
		private void ResolveScrollOffset(SplinePath currentPath)
		{
			scrollRange = GetScrollRange(currentPath);

			if (!Application.isPlaying || !scrollInitialized)
			{
				scrollOffset = position * scrollRange;
				pingPongDirection = 1f;
				shown = true;

				// Parked out of sight until Show() is called
				if (Application.isPlaying && startHidden && !traveling)
				{
					scrollOffset = GetParkedOffset(travelDirection == SplineTravelDirection.FORWARD);
					shown = false;
				}

				scrollInitialized = true;
			}
		}

		private TextMeshHandler GetMeshHandler()
		{
			return TextMeshHandlerRegistry.GetTextMeshHandler(gameObject.GetInstanceID());
		}

		// The handler may not exist yet; the master configurator attaches the spline when it creates one
		private void AttachToMeshHandler()
		{
			TextMeshHandler meshHandler = GetMeshHandler();
			if (meshHandler == null) { return; }

			meshHandler.layoutStage = this;
			meshHandler.WriteChanges();
		}

		private void DetachFromMeshHandler()
		{
			TextMeshHandler meshHandler = GetMeshHandler();
			if (meshHandler == null || meshHandler.layoutStage != (ITextLayoutStage)this) { return; }

			meshHandler.layoutStage = null;
			if (meshHandler.tmpText != null)
			{
				meshHandler.WriteChanges();
			}
		}

		// Pushes the current curve and layout to the mesh; a playing typewriter does this on its own every frame
		public void RequestWriteBack()
		{
			TextMeshHandler meshHandler = GetMeshHandler();
			if (meshHandler == null || meshHandler.tmpText == null) { return; }

			meshHandler.WriteChanges();
		}

		public void SetPathDirty()
		{
			pathDirty = true;
		}

		public void RebuildPath()
		{
			if (path == null)
			{
				path = new SplinePath();
			}
			if (resolvedKnots == null)
			{
				resolvedKnots = new List<SplineKnot>();
			}

			ResolveAutoTangents();

			resolvedKnots.Clear();
			for (int i = 0; i < points.Count; i++)
			{
				SplinePoint point = points[i];
				resolvedKnots.Add(new SplineKnot(point.position, point.tangentIn, point.tangentOut));
			}

			path.Rebuild(resolvedKnots, closed, curveResolution);
			pathDirty = false;
		}

		// Rigid per-character placement: each animated quad is moved from its flat pivot to the curve and turned by the tangent.
		// Pivot and arc length come from the rest pose so the effects' own motion rides along the curve frame
		public void ApplyLayout(MeshCache meshCache)
		{
			SplinePath currentPath = Path;
			if (!enabled || !currentPath.IsValid || !ComputeLayoutMetrics(meshCache, currentPath, out float minX, out float yRef, out float effectiveSpacing))
			{
				meshCache.CopyCurrentToOutput();
				return;
			}

			ResolveScrollOffset(currentPath);

			float totalLength = currentPath.TotalLength;
			// LOOP wraps on open curves too, so the text streams around instead of being culled at the ends
			bool wrap = currentPath.Closed || scrollMode == SplineScrollMode.LOOP;
			bool cull = !wrap && !clampEnds;

			int mainQuadCount = meshCache.subMeshes.Length > 0 && meshCache.subMeshes[0].srcVerts != null ? meshCache.subMeshes[0].srcVerts.Length / VERTICES_PER_CHARACTER : 0;

			bool runEnterExit = enterExitEnabled && mainQuadCount > 0;
			if (runEnterExit)
			{
				EnsureEnterExitStates(mainQuadCount);
			}

			// While letters travel individually, each quad of the main submesh has its own arc length this frame
			bool useLetterPositions = traveling && letterTravelActive && quadCurrentS.Length == mainQuadCount;

			// Hidden and not travelling: nothing is drawn, whatever the curve or window would do with the parked offset (closed curves wrap it back into view)
			bool parked = Application.isPlaying && !shown && !traveling;
			currentPath.SampleByArcLength(0f, out Vector2 parkPoint, out _);

			for (int m = 0; m < meshCache.subMeshes.Length; m++)
			{
				Vector3[] src = meshCache.subMeshes[m].srcVerts;
				Vector3[] current = meshCache.subMeshes[m].currentVerts;
				Vector3[] output = meshCache.subMeshes[m].outputVerts;
				Color32[] currentColors = meshCache.subMeshes[m].currentColors;
				Color32[] outputColors = meshCache.subMeshes[m].outputColors;
				if (src == null || current == null || output == null || currentColors == null || outputColors == null) { continue; }

				for (int i = 0; i + VERTICES_PER_CHARACTER - 1 < src.Length; i += VERTICES_PER_CHARACTER)
				{
					// Extrude's back copies take their letter's placement when the output is finished
					if (m == 0 && meshCache.IsBackVertex(i)) { continue; }

					for (int k = 0; k < VERTICES_PER_CHARACTER; k++)
					{
						quadVerts[k] = current[i + k];
						quadColors[k] = currentColors[i + k];
					}

					if (IsDegenerateQuad(src, i))
					{
						WriteQuad(output, outputColors, i);
						continue;
					}

					if (parked)
					{
						WriteCollapsedQuad(output, outputColors, i, parkPoint);
						continue;
					}

					int quadIndex = i / VERTICES_PER_CHARACTER;

					// Bottom-center of the flat quad: the point that lands on the curve
					Vector2 pivot = new Vector2((src[i].x + src[i + 3].x) * 0.5f, Mathf.Min(src[i].y, src[i + 3].y));

					float s;
					if (useLetterPositions && m == 0)
					{
						s = quadCurrentS[quadIndex];
					}
					else
					{
						s = (pivot.x - minX) * effectiveSpacing + scrollOffset;
					}

					if (wrap)
					{
						s = Mathf.Repeat(s, totalLength);
					}

					currentPath.SampleByArcLength(s, out Vector2 curvePoint, out Vector2 tangent);

					if (cull && (s < 0f || s > totalLength))
					{
						WriteCollapsedQuad(output, outputColors, i, curvePoint);
						continue;
					}

					// Enter/exit is per character and only on the main submesh
					if (runEnterExit && m == 0)
					{
						if (!ApplyEnterExit(s, totalLength, quadIndex))
						{
							WriteCollapsedQuad(output, outputColors, i, curvePoint);
							continue;
						}
					}

					Vector2 normal = new Vector2(-tangent.y, tangent.x);
					float perpendicular = (pivot.y - yRef) + baselineOffset;
					Vector2 placedPivot = curvePoint + normal * perpendicular;

					for (int k = 0; k < VERTICES_PER_CHARACTER; k++)
					{
						Vector3 vertex = quadVerts[k];
						Vector2 fromPivot = new Vector2(vertex.x - pivot.x, vertex.y - pivot.y);

						Vector2 placed;
						if (rotateWithCurve)
						{
							placed = placedPivot + tangent * fromPivot.x + normal * fromPivot.y;
						}
						else
						{
							placed = placedPivot + fromPivot;
						}

						quadVerts[k] = new Vector3(placed.x, placed.y, vertex.z);
					}

					WriteQuad(output, outputColors, i);
				}
			}
		}

		// Flat extents, spacing (auto fit included) and the vertical reference the placement shares with the letter travel
		private bool ComputeLayoutMetrics(MeshCache meshCache, SplinePath currentPath, out float minX, out float yRef, out float effectiveSpacing)
		{
			minX = 0f;
			yRef = 0f;
			effectiveSpacing = spacing;

			if (!TryGetFlatExtents(meshCache, out minX, out float maxX, out float minY, out float maxY)) { return false; }

			FlatTextWidth = maxX - minX;

			if (autoFit && FlatTextWidth > 0f)
			{
				effectiveSpacing = currentPath.TotalLength / FlatTextWidth;
			}
			effectiveSpacing = Mathf.Max(MIN_SPACING, effectiveSpacing);
			TextArcWidth = FlatTextWidth * effectiveSpacing;

			yRef = (minY + maxY) * 0.5f;
			return true;
		}

		private void WriteQuad(Vector3[] output, Color32[] outputColors, int firstVertex)
		{
			for (int k = 0; k < VERTICES_PER_CHARACTER; k++)
			{
				output[firstVertex + k] = quadVerts[k];
				outputColors[firstVertex + k] = quadColors[k];
			}
		}

		// A zero-area quad on the curve: the character is not drawn
		private void WriteCollapsedQuad(Vector3[] output, Color32[] outputColors, int firstVertex, Vector2 curvePoint)
		{
			Vector3 collapsed = new Vector3(curvePoint.x, curvePoint.y, quadVerts[0].z);
			for (int k = 0; k < VERTICES_PER_CHARACTER; k++)
			{
				output[firstVertex + k] = collapsed;
				outputColors[firstVertex + k] = quadColors[k];
			}
		}

		public void MarkEnterExitDirty()
		{
			enterExitStatesDirty = true;
		}

		// The states are built from the configs the way the dispatcher builds reveal/hide, one group per quad
		private void EnsureEnterExitStates(int numQuads)
		{
			if (!enterExitStatesDirty && builtQuadCount == numQuads) { return; }

			enterState = BuildEnterExitState(enterConfig, ref enterLayouts, numQuads, STR_ENTER_STATE);
			exitState = BuildEnterExitState(ResolveExitConfig(), ref exitLayouts, numQuads, STR_EXIT_STATE);

			builtQuadCount = numQuads;
			enterExitStatesDirty = false;
		}

		private static MeshEffectGroupState BuildEnterExitState(MeshEffectGroupConfig config, ref VertexGroupLayout[] layouts, int numQuads, string stateName)
		{
			if (layouts.Length != numQuads)
			{
				layouts = new VertexGroupLayout[numQuads];
			}

			for (int q = 0; q < numQuads; q++)
			{
				VertexGroupLayout layout = new VertexGroupLayout();
				layout.groupIndex = q;
				layout.startVertexIndex = q * VERTICES_PER_CHARACTER;
				layout.endVertexIndex = layout.startVertexIndex + VERTICES_PER_CHARACTER - 1;
				layout.enabledEffectsMask = EffectFlag.All;
				layout.playbackMode = PlaybackMode.ONE_SHOT;
				layouts[q] = layout;
			}

			MeshEffectGroupState res = null;
			if (config != null)
			{
				res = new MeshEffectGroupState(config, ref layouts, PlaybackMode.ONE_SHOT, GroupType.PER_CHARACTER, EffectFlag.All, stateName);
			}
			return res;
		}

		// Same trick as the typewriter's hide: the exit is the enter config inverted, unless the user picked one
		private MeshEffectGroupConfig ResolveExitConfig()
		{
			ReleaseExitConfigRuntime();

			MeshEffectGroupConfig res = exitConfigSelected;
			if (useEnterAsExitConfig)
			{
				res = null;
				if (enterConfig != null)
				{
					exitConfigRuntime = ScriptableObject.CreateInstance<MeshEffectGroupConfig>();
					exitConfigRuntime.hideFlags = HideFlags.HideAndDontSave;
					exitConfigRuntime.CopyValuesFrom(enterConfig);
					exitConfigRuntime.Invert();
					ownsExitConfigRuntime = true;
					res = exitConfigRuntime;
				}
			}
			return res;
		}

		private void ReleaseExitConfigRuntime()
		{
			if (ownsExitConfigRuntime && exitConfigRuntime != null)
			{
				DestroyImmediate(exitConfigRuntime);
			}
			exitConfigRuntime = null;
			ownsExitConfigRuntime = false;
		}

		// Returns false when the character sits outside the visible window
		private bool ApplyEnterExit(float s, float totalLength, int quadIndex)
		{
			float windowStart = Mathf.Min(visibleStart, visibleEnd) * totalLength;
			float windowEnd = Mathf.Max(visibleStart, visibleEnd) * totalLength;
			if (s < windowStart || s > windowEnd) { return false; }

			float enterSpan = enterLength * totalLength;
			float exitSpan = exitLength * totalLength;

			if (enterSpan > 0f && s < windowStart + enterSpan)
			{
				ApplyStateAtProgress(enterState, enterLayouts, quadIndex, (s - windowStart) / enterSpan);
			}
			else if (exitSpan > 0f && s > windowEnd - exitSpan)
			{
				ApplyStateAtProgress(exitState, exitLayouts, quadIndex, (s - (windowEnd - exitSpan)) / exitSpan);
			}

			return true;
		}

		// Mirrors MeshCache.ApplyStates on the one quad in quadVerts/quadColors: pivots from the live verts, effects in config order
		private void ApplyStateAtProgress(MeshEffectGroupState groupState, VertexGroupLayout[] layouts, int quadIndex, float progress)
		{
			if (groupState == null || quadIndex >= layouts.Length) { return; }

			ref VertexGroupLayout layout = ref layouts[quadIndex];
			progress = Mathf.Clamp01(progress);

			List<MeshEffectState> states = groupState.states;
			for (int stateIdx = 0; stateIdx < states.Count; stateIdx++)
			{
				layout.center = (quadVerts[0] + quadVerts[2]) * 0.5f;
				layout.centerTop = (quadVerts[1] + quadVerts[2]) * 0.5f;
				layout.SetRight(quadVerts[0], quadVerts[3]);

				states[stateIdx].UpdateAtProgress(ref layout, VertexGroupDisplayState.Revealing, progress);

				for (int k = 0; k < VERTICES_PER_CHARACTER; k++)
				{
					quadVerts[k] = states[stateIdx].TransformVertex(ref layout, quadVerts[k]);
					quadColors[k] = states[stateIdx].TransformColor(ref layout, quadColors[k]);
				}
			}
		}

		// Bounds of the visible quads in the flat rest pose
		private static bool TryGetFlatExtents(MeshCache meshCache, out float minX, out float maxX, out float minY, out float maxY)
		{
			minX = float.MaxValue; maxX = float.MinValue; minY = float.MaxValue; maxY = float.MinValue;
			bool any = false;

			if (meshCache == null || meshCache.subMeshes == null) { return false; }

			for (int m = 0; m < meshCache.subMeshes.Length; m++)
			{
				Vector3[] src = meshCache.subMeshes[m].srcVerts;
				if (src == null) { continue; }

				for (int i = 0; i + VERTICES_PER_CHARACTER - 1 < src.Length; i += VERTICES_PER_CHARACTER)
				{
					if (IsDegenerateQuad(src, i)) { continue; }
					// Underlines and marks reach below and around the letters: adding one must not move the text
					if (m == 0 && meshCache.IsDecorationVertex(i)) { continue; }

					for (int k = 0; k < VERTICES_PER_CHARACTER; k++)
					{
						Vector3 v = src[i + k];
						if (v.x < minX) { minX = v.x; }
						if (v.x > maxX) { maxX = v.x; }
						if (v.y < minY) { minY = v.y; }
						if (v.y > maxY) { maxY = v.y; }
					}
					any = true;
				}
			}

			return any;
		}

		// Quads no visible glyph owns (decorations, spare capacity) are collapsed onto a point by MeshCache.Rebuild
		private static bool IsDegenerateQuad(Vector3[] verts, int i)
		{
			Vector3 a = verts[i];
			return verts[i + 1] == a && verts[i + 2] == a && verts[i + 3] == a;
		}

		// AUTO points get Catmull-Rom tangents from their neighbours. They are written into the point so switching to MIRRORED or BROKEN keeps the shape
		private void ResolveAutoTangents()
		{
			int count = points.Count;
			for (int i = 0; i < count; i++)
			{
				SplinePoint point = points[i];
				if (point.tangentMode != SplineTangentMode.AUTO) { continue; }

				Vector2 direction = GetAutoTangentDirection(i);
				point.tangentOut = direction * AUTO_TANGENT_FACTOR;
				point.tangentIn = -point.tangentOut;
				points[i] = point;
			}
		}

		private Vector2 GetAutoTangentDirection(int index)
		{
			int count = points.Count;
			if (count < 2) { return Vector2.zero; }

			bool hasPrevious = closed || index > 0;
			bool hasNext = closed || index < count - 1;

			Vector2 previous = hasPrevious ? points[(index - 1 + count) % count].position : points[index].position;
			Vector2 next = hasNext ? points[(index + 1) % count].position : points[index].position;

			Vector2 res;
			if (hasPrevious && hasNext)
			{
				res = (next - previous) * 0.5f;
			}
			else
			{
				res = next - previous;
			}
			return res;
		}

		public SplinePoint GetPoint(int index)
		{
			return points[index];
		}

		public void SetPoint(int index, SplinePoint point)
		{
			points[index] = point;
			SetPathDirty();
		}

		public void SetPointPosition(int index, Vector2 position)
		{
			SplinePoint point = points[index];
			point.position = position;
			points[index] = point;
			SetPathDirty();
		}

		// Dragging a tangent of an AUTO point makes it MIRRORED so the drag sticks
		public void SetTangentOut(int index, Vector2 tangentOut)
		{
			SplinePoint point = points[index];
			if (point.tangentMode == SplineTangentMode.AUTO)
			{
				point.tangentMode = SplineTangentMode.MIRRORED;
			}

			point.tangentOut = tangentOut;
			if (point.tangentMode == SplineTangentMode.MIRRORED)
			{
				point.tangentIn = -tangentOut;
			}

			points[index] = point;
			SetPathDirty();
		}

		public void SetTangentIn(int index, Vector2 tangentIn)
		{
			SplinePoint point = points[index];
			if (point.tangentMode == SplineTangentMode.AUTO)
			{
				point.tangentMode = SplineTangentMode.MIRRORED;
			}

			point.tangentIn = tangentIn;
			if (point.tangentMode == SplineTangentMode.MIRRORED)
			{
				point.tangentOut = -tangentIn;
			}

			points[index] = point;
			SetPathDirty();
		}

		public void SetTangentMode(int index, SplineTangentMode mode)
		{
			SplinePoint point = points[index];
			if (point.tangentMode == mode) { return; }

			// Leaving AUTO keeps the computed tangents; entering MIRRORED averages the two so nothing jumps far
			if (mode == SplineTangentMode.MIRRORED && point.tangentMode == SplineTangentMode.BROKEN)
			{
				Vector2 averaged = (point.tangentOut - point.tangentIn) * 0.5f;
				point.tangentOut = averaged;
				point.tangentIn = -averaged;
			}

			point.tangentMode = mode;
			points[index] = point;
			SetPathDirty();
		}

		public int AddPoint(Vector2 position)
		{
			points.Add(new SplinePoint(position));
			SetPathDirty();
			return points.Count - 1;
		}

		public void InsertPoint(int index, Vector2 position)
		{
			index = Mathf.Clamp(index, 0, points.Count);
			points.Insert(index, new SplinePoint(position));
			SetPathDirty();
		}

		// Adds a point after the last one, continuing in the direction of the last segment
		public int AppendPoint()
		{
			Vector2 position;
			int count = points.Count;
			if (count >= 2)
			{
				position = points[count - 1].position + (points[count - 1].position - points[count - 2].position);
			}
			else if (count == 1)
			{
				position = points[0].position + Vector2.right * DEFAULT_HALF_WIDTH;
			}
			else
			{
				position = Vector2.zero;
			}

			return AddPoint(position);
		}

		// Inserts halfway along the curve between the selected point and the next one
		public int InsertPointAfter(int index)
		{
			int count = points.Count;
			if (count == 0) { return AddPoint(Vector2.zero); }

			index = Mathf.Clamp(index, 0, count - 1);
			bool hasNext = closed || index < count - 1;

			Vector2 position;
			if (hasNext)
			{
				SplinePath currentPath = Path;
				if (currentPath.IsValid)
				{
					position = currentPath.EvaluateSegment(index, 0.5f);
				}
				else
				{
					position = (points[index].position + points[(index + 1) % count].position) * 0.5f;
				}
			}
			else
			{
				position = points[index].position + (points[index].position - points[Mathf.Max(0, index - 1)].position);
			}

			InsertPoint(index + 1, position);
			return index + 1;
		}

		// Inserts on the curve at the nearest point to a local-space position, keeping the point order
		public int InsertPointOnCurve(Vector2 localPosition)
		{
			SplinePath currentPath = Path;
			if (!currentPath.IsValid) { return AddPoint(localPosition); }

			float s = currentPath.ClosestArcLength(localPosition, out Vector2 onCurve);
			int segment = currentPath.GetSegmentIndexAtArcLength(s);

			InsertPoint(segment + 1, onCurve);
			return segment + 1;
		}

		public bool RemovePoint(int index)
		{
			if (index < 0 || index >= points.Count || points.Count <= MIN_POINTS) { return false; }

			points.RemoveAt(index);
			SetPathDirty();
			return true;
		}

		// Add Component and the Reset menu start from the same arch as the Add Spline button
		private void Reset()
		{
			SeedDefaultPoints();
		}

		// Gentle arch spanning the host rect: left-center, top-center, right-center
		public void SeedDefaultPoints()
		{
			points.Clear();

			GetHostExtents(out float halfWidth, out float halfHeight);

			points.Add(new SplinePoint(new Vector2(-halfWidth, 0f)));
			points.Add(new SplinePoint(new Vector2(0f, halfHeight)));
			points.Add(new SplinePoint(new Vector2(halfWidth, 0f)));

			closed = false;
			SetPathDirty();
		}

		private void GetHostExtents(out float halfWidth, out float halfHeight)
		{
			halfWidth = DEFAULT_HALF_WIDTH;
			halfHeight = DEFAULT_ARCH_HEIGHT;

			RectTransform rectTransform = transform as RectTransform;
			if (rectTransform != null && rectTransform.rect.width > 0f)
			{
				halfWidth = rectTransform.rect.width * 0.5f;
				halfHeight = rectTransform.rect.height * 0.5f;
				return;
			}

			TMP_Text text = GetComponent<TMP_Text>();
			if (text != null && text.textBounds.size.x > 0f)
			{
				halfWidth = text.textBounds.extents.x;
				halfHeight = text.textBounds.extents.y;
			}
		}

		public Vector3 LocalToWorld(Vector2 localPosition)
		{
			return transform.TransformPoint(new Vector3(localPosition.x, localPosition.y, 0f));
		}

		public Vector2 WorldToLocal(Vector3 worldPosition)
		{
			Vector3 local = transform.InverseTransformPoint(worldPosition);
			return new Vector2(local.x, local.y);
		}
	}
}
