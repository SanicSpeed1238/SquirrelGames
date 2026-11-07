using TMPro;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// LateUpdate sees what the Animator wrote this frame and still runs before the master configurator refreshes the per character buffer
	[DefaultExecutionOrder(-10)]
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TMP_Text))]
	[AddComponentMenu("AllIn1TextShader/Warp")]
	public class AllIn1TextWarp : MonoBehaviour, ITextOutputModifier
	{
		public const TextWarpStyle DEFAULT_STYLE = TextWarpStyle.ARC;
		public const float DEFAULT_BEND = 50f;
		public const float DEFAULT_HORIZONTAL_DISTORTION = 0f;
		public const float DEFAULT_VERTICAL_DISTORTION = 0f;
		public const TextWarpOrientation DEFAULT_ORIENTATION = TextWarpOrientation.HORIZONTAL;
		public const TextWarpBox DEFAULT_REFERENCE_BOX = TextWarpBox.TEXT;
		public const bool DEFAULT_KEEP_LETTERS_RECTANGULAR = false;
		public const float DEFAULT_AMOUNT = 1f;
		public const float DEFAULT_WAVES = 1.5f;
		public const float DEFAULT_SPEED = 1f;
		public const float DEFAULT_PHASE = 0f;
		public const TextWarpPin DEFAULT_PIN = TextWarpPin.NONE;
		public const bool DEFAULT_UNSCALED_TIME = false;
		public const float MIN_WAVES = 0f;
		public const float MAX_WAVES = 10f;
		public const float MIN_SPEED = -5f;
		public const float MAX_SPEED = 5f;

		private const int VERTICES_PER_CHARACTER = 4;
		private const int SPLINE_BOUNDS_SAMPLES = 48;
		private const float MIN_AXIS_LENGTH = 1e-6f;

		[SerializeField, Tooltip("The warp's shape. Most are Photoshop's Warp Text styles, and the ones that look alike on a single line are under More. Banner is a wave that travels along the text in play mode. Circle wraps the text around a circle. Custom is a 4x4 cage you drag in the Scene view.")] private TextWarpStyle style = DEFAULT_STYLE;
		[SerializeField, Range(-TextWarpField.EXTENDED_MAX_BEND, TextWarpField.EXTENDED_MAX_BEND), Tooltip("How strong the warp is. Negative bends the other way. Photoshop's range is -100 to 100; Flag, Banner, Wave, Fish, Rise, Twist, Inflate and Squeeze go to 300. For Banner, 100 lifts the crests by the text's full height. For Circle, 100 closes the circle. Custom ignores it.")] private float bend = DEFAULT_BEND;
		[SerializeField, Range(TextWarpField.MIN_DISTORTION, TextWarpField.MAX_DISTORTION), Tooltip("Squeezes one side of the warp and widens the other, left against right. Same as Photoshop's Horizontal Distortion. Circle, Banner and Custom ignore it.")] private float horizontalDistortion = DEFAULT_HORIZONTAL_DISTORTION;
		[SerializeField, Range(TextWarpField.MIN_DISTORTION, TextWarpField.MAX_DISTORTION), Tooltip("Squeezes one side of the warp and widens the other, top against bottom. Same as Photoshop's Vertical Distortion. Circle, Banner and Custom ignore it.")] private float verticalDistortion = DEFAULT_VERTICAL_DISTORTION;
		[SerializeField, Tooltip("Horizontal bends along the text's width, Vertical along its height. Twist, Circle and Custom ignore it.")] private TextWarpOrientation orientation = DEFAULT_ORIENTATION;
		[SerializeField, Tooltip("The rectangle the warp is stretched over. Text is the text's layout box (the curve's bounds when a spline is on), which stays still while the typewriter reveals letters. Rect Transform is the object's rect, so the warp stays put when the text changes.")] private TextWarpBox referenceBox = DEFAULT_REFERENCE_BOX;
		[SerializeField, Tooltip("Each letter follows the warp by turning and scaling as one rectangle instead of bending at its corners. Outlines, glows and shadows stay exact and strongly warped letters show no crease, but letters no longer taper. Underlines always bend.")] private bool keepLettersRectangular = DEFAULT_KEEP_LETTERS_RECTANGULAR;
		[SerializeField, Range(0f, 1f), Tooltip("Scales Bend and both distortions, or the Custom cage's offsets, from no warp (0) to the full warp (1). Every value in between is a real warp, so animate this to warp the text in and out.")] private float amount = DEFAULT_AMOUNT;
		// Custom style: 4x4 points normalized to the reference box, row major from the top row, x right and y down
		[SerializeField] private Vector2[] customCage = TextWarpField.CreateIdentityCage();
		[SerializeField, Range(MIN_WAVES, MAX_WAVES), Tooltip("Banner only. How many full waves fit along the text.")] private float waves = DEFAULT_WAVES;
		[SerializeField, Range(MIN_SPEED, MAX_SPEED), Tooltip("Banner only. Waves per second the banner travels in play mode, toward the end for positive values. 0 holds it still.")] private float speed = DEFAULT_SPEED;
		[SerializeField, Range(0f, 1f), Tooltip("Banner only. Where the wave starts, as a fraction of one wave. Scrub it to preview the motion in edit mode.")] private float phase = DEFAULT_PHASE;
		[SerializeField, Tooltip("Banner only. Holds one end still, like a flag on its pole, so the wave grows from there.")] private TextWarpPin pin = DEFAULT_PIN;
		[SerializeField, Tooltip("Banner only. Travels on unscaled time, so it keeps waving while the game is paused with Time.timeScale.")] private bool unscaledTime = DEFAULT_UNSCALED_TIME;

#if UNITY_EDITOR
		[HideInInspector] public bool shapeUnfolded = true;
		[HideInInspector] public bool optionsUnfolded = true;
#endif

		private struct Settings
		{
			public TextWarpStyle style;
			public float bend;
			public float horizontalDistortion;
			public float verticalDistortion;
			public TextWarpOrientation orientation;
			public TextWarpBox referenceBox;
			public bool keepLettersRectangular;
			public float amount;
			public float waves;
			public float speed;
			public float phase;
			public TextWarpPin pin;
			public bool unscaledTime;

			public bool SameAs(Settings other)
			{
				return style == other.style && bend == other.bend && horizontalDistortion == other.horizontalDistortion && verticalDistortion == other.verticalDistortion &&
					orientation == other.orientation && referenceBox == other.referenceBox && keepLettersRectangular == other.keepLettersRectangular && amount == other.amount &&
					waves == other.waves && speed == other.speed && phase == other.phase && pin == other.pin && unscaledTime == other.unscaledTime;
			}
		}

		private readonly TextWarpField field = new TextWarpField();
		private readonly Vector2[] scaledCage = new Vector2[TextWarpField.CAGE_POINT_COUNT];
		private readonly Vector2[] appliedCage = new Vector2[TextWarpField.CAGE_POINT_COUNT];
		private Settings appliedSettings;
		private bool hasAppliedSettings = false;
		private int lastWrittenFrame = -1;
		// The Banner's travel since play began, in waves wrapped into 0..1, and the clock reading it was last advanced at
		private double bannerTravel = 0.0;
		private double bannerClock = -1.0;
		private bool bannerClockUnscaled = false;
		private TMP_Text tmpText;

		public TextWarpStyle Style
		{
			get { return style; }
			set { style = value; }
		}
		// The setters ignore NaN and infinity, which would never compare equal and would rewrite the mesh every frame
		public float Bend
		{
			get { return bend; }
			// Only the widest range here, so code can set Style and Bend in either order; each style clamps to its own range, before Amount, when the warp is built
			set { if (IsFinite(value)) { bend = Mathf.Clamp(value, -TextWarpField.EXTENDED_MAX_BEND, TextWarpField.EXTENDED_MAX_BEND); } }
		}
		public float HorizontalDistortion
		{
			get { return horizontalDistortion; }
			set { if (IsFinite(value)) { horizontalDistortion = Mathf.Clamp(value, TextWarpField.MIN_DISTORTION, TextWarpField.MAX_DISTORTION); } }
		}
		public float VerticalDistortion
		{
			get { return verticalDistortion; }
			set { if (IsFinite(value)) { verticalDistortion = Mathf.Clamp(value, TextWarpField.MIN_DISTORTION, TextWarpField.MAX_DISTORTION); } }
		}
		public TextWarpOrientation Orientation
		{
			get { return orientation; }
			set { orientation = value; }
		}
		public TextWarpBox ReferenceBox
		{
			get { return referenceBox; }
			set { referenceBox = value; }
		}
		public bool KeepLettersRectangular
		{
			get { return keepLettersRectangular; }
			set { keepLettersRectangular = value; }
		}
		public float Amount
		{
			get { return amount; }
			set { if (IsFinite(value)) { amount = Mathf.Clamp01(value); } }
		}
		public float Waves
		{
			get { return waves; }
			set { if (IsFinite(value)) { waves = Mathf.Clamp(value, MIN_WAVES, MAX_WAVES); } }
		}
		public float Speed
		{
			get { return speed; }
			set { if (IsFinite(value)) { speed = Mathf.Clamp(value, MIN_SPEED, MAX_SPEED); } }
		}
		public float Phase
		{
			get { return phase; }
			set { if (IsFinite(value)) { phase = Mathf.Repeat(value, 1f); } }
		}
		public TextWarpPin Pin
		{
			get { return pin; }
			set { pin = value; }
		}
		public bool UnscaledTime
		{
			get { return unscaledTime; }
			set { unscaledTime = value; }
		}

		// A Banner with speed travels on its own clock in play mode, so it rewrites the mesh every frame while that clock runs
		public bool IsAnimating
		{
			get { return Application.isPlaying && isActiveAndEnabled && style == TextWarpStyle.BANNER && speed != 0f && bend != 0f && amount > 0f && (unscaledTime || Time.timeScale > 0f); }
		}

		// True when an AllIn1 text on this object feeds the warp its mesh; without one the warp has nothing to act on
		public bool IsDriven
		{
			get
			{
				TextMeshHandler meshHandler = GetMeshHandler();
				return meshHandler != null && meshHandler.tmpText != null;
			}
		}

		private void OnEnable()
		{
			hasAppliedSettings = false;
			bannerClock = -1.0;
			AttachToMeshHandler();
		}

		private void OnDisable()
		{
			DetachFromMeshHandler();
		}

		// Also runs after undo and redo, which bypass the inspector's own write-back
		private void OnValidate()
		{
			ClampSettings();
			RequestWriteBack();
		}

		// The Animation window preview writes the fields directly in edit mode, where LateUpdate only runs on scene changes
		private void OnDidApplyAnimationProperties()
		{
			if (!Application.isPlaying)
			{
				RequestWriteBack();
			}
		}

		// Values set from code, Animator or Timeline after the frame's write are pushed here. A frame whose write already used them costs nothing
		private void LateUpdate()
		{
			// A frame the typewriter or the spline already wrote used this frame's time too, unless a value changed after that write
			if (IsAnimating)
			{
				if (lastWrittenFrame != Time.frameCount || !hasAppliedSettings || !CaptureSettings().SameAs(appliedSettings) || !CageMatchesApplied()) { RequestWriteBack(); }
				return;
			}

			if (hasAppliedSettings && CaptureSettings().SameAs(appliedSettings) && CageMatchesApplied()) { return; }

			RequestWriteBack();
		}

		public void ModifyOutput(MeshCache meshCache)
		{
			if (!enabled || !gameObject.activeInHierarchy) { return; }

			appliedSettings = CaptureSettings();
			EnsureCustomCage();
			System.Array.Copy(customCage, appliedCage, TextWarpField.CAGE_POINT_COUNT);
			hasAppliedSettings = true;
			lastWrittenFrame = Time.frameCount;

			if (!ConfigureField() || meshCache == null || meshCache.subMeshes == null) { return; }

			for (int m = 0; m < meshCache.subMeshes.Length; m++)
			{
				Vector3[] output = meshCache.subMeshes[m].outputVerts;
				if (output == null) { continue; }

				int firstVertex = 0;
				for (; firstVertex + VERTICES_PER_CHARACTER - 1 < output.Length; firstVertex += VERTICES_PER_CHARACTER)
				{
					// Extrude's back copies take their letter's shape when the output is finished
					if (m == 0 && meshCache.IsBackVertex(firstVertex)) { continue; }

					// Spare capacity and hidden letters are collapsed onto a point: one evaluation keeps them collapsed
					if (IsCollapsedQuad(output, firstVertex))
					{
						Vector3 collapsed = WarpVertex(output[firstVertex]);
						for (int k = 0; k < VERTICES_PER_CHARACTER; k++)
						{
							output[firstVertex + k] = collapsed;
						}
						continue;
					}

					// Neighbouring underline slices share their edges, so they bend per vertex to stay joined
					if (keepLettersRectangular && !(m == 0 && meshCache.IsDecorationVertex(firstVertex)))
					{
						WarpQuadAsRectangle(output, firstVertex);
						continue;
					}

					for (int k = 0; k < VERTICES_PER_CHARACTER; k++)
					{
						output[firstVertex + k] = WarpVertex(output[firstVertex + k]);
					}
				}

				for (; firstVertex < output.Length; firstVertex++)
				{
					output[firstVertex] = WarpVertex(output[firstVertex]);
				}
			}
		}

		// Amount scales Bend and the distortions (a preset) or the cage's offsets (Custom), so every value in between is a real warp; false when nothing would move
		public bool ConfigureField()
		{
			if (!TryGetReferenceBox(out Rect box)) { return false; }

			Vector2[] cage = null;
			if (style == TextWarpStyle.CUSTOM)
			{
				EnsureCustomCage();
				for (int i = 0; i < TextWarpField.CAGE_POINT_COUNT; i++)
				{
					Vector2 identityPoint = TextWarpField.GetIdentityCagePoint(i);
					scaledCage[i] = identityPoint + (customCage[i] - identityPoint) * amount;
				}
				cage = scaledCage;
			}

			field.Configure(style, ClampBend(bend, style) * amount, ClampDistortion(horizontalDistortion) * amount, ClampDistortion(verticalDistortion) * amount, orientation, box, cage, waves, GetBannerPhase(), pin);
			return !field.IsIdentity;
		}

		public Vector2 GetCagePoint(int index)
		{
			EnsureCustomCage();
			return customCage[index];
		}

		public void SetCagePoint(int index, Vector2 cagePoint)
		{
			EnsureCustomCage();
			customCage[index] = cagePoint;
		}

		public void ResetCage()
		{
			customCage = TextWarpField.CreateIdentityCage();
		}

		public bool IsCageFlat()
		{
			EnsureCustomCage();
			return TextWarpField.IsIdentityCage(customCage);
		}

		// Bakes the style as it looks now into the cage and switches to Custom, to be finished by hand; Amount becomes 1 so nothing moves. Circle and Banner have no cage
		public bool CustomizeFromStyle()
		{
			if (style == TextWarpStyle.CUSTOM || style == TextWarpStyle.CIRCLE || style == TextWarpStyle.BANNER || !TryGetReferenceBox(out Rect box)) { return false; }

			EnsureCustomCage();
			field.Configure(style, ClampBend(bend, style) * amount, ClampDistortion(horizontalDistortion) * amount, ClampDistortion(verticalDistortion) * amount, orientation, box);
			if (!field.TryGetCage(customCage)) { return false; }

			style = TextWarpStyle.CUSTOM;
			amount = 1f;
			return true;
		}

		// Phase in waves: the scrubbed phase plus the travel, which accumulates speed over time so changing Speed never jumps the wave and 0 freezes it where it is
		private float GetBannerPhase()
		{
			if (!Application.isPlaying) { return phase; }

			double now = unscaledTime ? Time.unscaledTimeAsDouble : Time.timeAsDouble;
			if (bannerClock < 0.0 || bannerClockUnscaled != unscaledTime)
			{
				bannerClock = now;
				bannerClockUnscaled = unscaledTime;
			}

			bannerTravel = (bannerTravel + speed * (now - bannerClock)) % 1.0;
			bannerClock = now;
			return (float)((phase + bannerTravel) % 1.0);
		}

		private void EnsureCustomCage()
		{
			if (customCage == null || customCage.Length != TextWarpField.CAGE_POINT_COUNT)
			{
				customCage = TextWarpField.CreateIdentityCage();
			}
		}

		// Where a local point lands with the field as last configured
		public Vector2 WarpPoint(Vector2 localPosition)
		{
			return field.Evaluate(localPosition);
		}

		public bool TryGetReferenceBox(out Rect box)
		{
			box = default;

			if (referenceBox == TextWarpBox.RECT_TRANSFORM)
			{
				RectTransform rectTransform = transform as RectTransform;
				if (rectTransform != null && rectTransform.rect.width > 0f && rectTransform.rect.height > 0f)
				{
					box = rectTransform.rect;
					return true;
				}
			}

			if (!TryGetLayoutBox(out box)) { return false; }

			if (TryGetComponent<AllIn1TextSpline>(out AllIn1TextSpline spline) && spline.isActiveAndEnabled && TryGetSplineBox(spline, box.height, out Rect splineBox))
			{
				box = splineBox;
			}

			return true;
		}

		// Photoshop's layout box rather than the ink: origin to advance (both ends, for right to left lines) and descender to ascender
		private bool TryGetLayoutBox(out Rect box)
		{
			box = default;

			TMP_Text text = GetText();
			TMP_TextInfo textInfo = text != null ? text.textInfo : null;
			if (textInfo == null || textInfo.characterInfo == null) { return false; }

			// The page TMP really draws: it clamps pageToDisplay the same way
			bool pageMode = text.overflowMode == TextOverflowModes.Page;
			int lastPage = textInfo.pageInfo != null && textInfo.pageInfo.Length > 0 ? textInfo.pageInfo.Length - 1 : 0;
			int shownPage = Mathf.Clamp(text.pageToDisplay - 1, 0, lastPage);

			float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
			int numCharacters = Mathf.Min(textInfo.characterCount, textInfo.characterInfo.Length);
			for (int c = Mathf.Max(0, text.firstVisibleCharacter); c < numCharacters; c++)
			{
				ref TMP_CharacterInfo characterInfo = ref textInfo.characterInfo[c];
				// Letters hidden by maxVisibleCharacters, Words or Lines keep their layout, so a reveal driven by those does not reshape the box
				if (!characterInfo.isVisible && (char.IsWhiteSpace(characterInfo.character) || char.IsControl(characterInfo.character))) { continue; }
				if (pageMode && characterInfo.pageNumber != shownPage) { continue; }
				// A line limit is usually a fixed clamp rather than a reveal, so lines past it never shape the box
				if (characterInfo.lineNumber >= text.maxVisibleLines) { continue; }

				minX = Mathf.Min(minX, Mathf.Min(characterInfo.origin, characterInfo.xAdvance));
				maxX = Mathf.Max(maxX, Mathf.Max(characterInfo.origin, characterInfo.xAdvance));
				minY = Mathf.Min(minY, characterInfo.descender);
				maxY = Mathf.Max(maxY, characterInfo.ascender);
			}

			if (maxX <= minX || maxY <= minY) { return false; }

			box = Rect.MinMaxRect(minX, minY, maxX, maxY);
			return true;
		}

		// On a spline the letters sit in a band along the curve, half the text's height on each side plus the baseline offset
		private static bool TryGetSplineBox(AllIn1TextSpline spline, float textHeight, out Rect box)
		{
			box = default;

			SplinePath path = spline.Path;
			if (path == null || !path.IsValid) { return false; }

			Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 max = new Vector2(float.MinValue, float.MinValue);
			for (int i = 0; i <= SPLINE_BOUNDS_SAMPLES; i++)
			{
				path.SampleByArcLength(path.TotalLength * i / SPLINE_BOUNDS_SAMPLES, out Vector2 point, out _);
				min = Vector2.Min(min, point);
				max = Vector2.Max(max, point);
			}

			float padding = textHeight * 0.5f + Mathf.Abs(spline.BaselineOffset);
			box = Rect.MinMaxRect(min.x - padding, min.y - padding, max.x + padding, max.y + padding);
			return true;
		}

		private Vector3 WarpVertex(Vector3 vertex)
		{
			Vector2 warped = WarpPoint(new Vector2(vertex.x, vertex.y));
			return new Vector3(warped.x, warped.y, vertex.z);
		}

		// The warp's local linear map at the quad's center, applied along the letter's own axes: turn plus a scale per axis, so a rectangle stays one
		private void WarpQuadAsRectangle(Vector3[] output, int firstVertex)
		{
			Vector2 bottomLeft = output[firstVertex];
			Vector2 topLeft = output[firstVertex + 1];
			Vector2 topRight = output[firstVertex + 2];
			Vector2 bottomRight = output[firstVertex + 3];

			Vector2 center = (bottomLeft + topLeft + topRight + bottomRight) * 0.25f;

			Vector2 right = bottomRight - bottomLeft;
			if (right.sqrMagnitude < MIN_AXIS_LENGTH) { right = topRight - topLeft; }
			right = right.sqrMagnitude < MIN_AXIS_LENGTH ? Vector2.right : right.normalized;
			Vector2 up = new Vector2(-right.y, right.x);

			Vector2 warpedCenter = field.Evaluate(center, out Vector2 derivativeX, out Vector2 derivativeY);

			Vector2 mappedRight = derivativeX * right.x + derivativeY * right.y;
			Vector2 mappedUp = derivativeX * up.x + derivativeY * up.y;

			float scaleX = mappedRight.magnitude;
			Vector2 newRight = scaleX > MIN_AXIS_LENGTH ? mappedRight / scaleX : right;
			Vector2 newUp = new Vector2(-newRight.y, newRight.x);
			// Negative where the warp folds over, which mirrors the letter the way the per vertex path does
			float scaleY = Vector2.Dot(mappedUp, newUp);

			for (int k = 0; k < VERTICES_PER_CHARACTER; k++)
			{
				Vector3 vertex = output[firstVertex + k];
				Vector2 fromCenter = new Vector2(vertex.x, vertex.y) - center;
				Vector2 placed = warpedCenter + newRight * (Vector2.Dot(fromCenter, right) * scaleX) + newUp * (Vector2.Dot(fromCenter, up) * scaleY);
				output[firstVertex + k] = new Vector3(placed.x, placed.y, vertex.z);
			}
		}

		private static bool IsCollapsedQuad(Vector3[] vertices, int firstVertex)
		{
			Vector3 a = vertices[firstVertex];
			return vertices[firstVertex + 1] == a && vertices[firstVertex + 2] == a && vertices[firstVertex + 3] == a;
		}

		private Settings CaptureSettings()
		{
			Settings res = new Settings();
			res.style = style;
			res.bend = bend;
			res.horizontalDistortion = horizontalDistortion;
			res.verticalDistortion = verticalDistortion;
			res.orientation = orientation;
			res.referenceBox = referenceBox;
			res.keepLettersRectangular = keepLettersRectangular;
			res.amount = amount;
			res.waves = waves;
			res.speed = speed;
			res.phase = phase;
			res.pin = pin;
			res.unscaledTime = unscaledTime;
			return res;
		}

		private bool CageMatchesApplied()
		{
			if (customCage == null || customCage.Length != TextWarpField.CAGE_POINT_COUNT) { return false; }

			for (int i = 0; i < TextWarpField.CAGE_POINT_COUNT; i++)
			{
				if (customCage[i].x != appliedCage[i].x || customCage[i].y != appliedCage[i].y) { return false; }
			}
			return true;
		}

		private static float ClampBend(float value, TextWarpStyle forStyle)
		{
			float maxBend = TextWarpField.GetMaxBend(forStyle);
			return Mathf.Clamp(value, -maxBend, maxBend);
		}

		// Clamped before Amount scales it, like Bend, so values the Animator writes past the range still ramp evenly
		private static float ClampDistortion(float value)
		{
			return Mathf.Clamp(value, TextWarpField.MIN_DISTORTION, TextWarpField.MAX_DISTORTION);
		}

		// Custom ignores Bend, so a preset's value (Flag at 250) survives a Customize and comes back with the preset
		private void ClampSettings()
		{
			bend = style == TextWarpStyle.CUSTOM ? Mathf.Clamp(bend, -TextWarpField.EXTENDED_MAX_BEND, TextWarpField.EXTENDED_MAX_BEND) : ClampBend(bend, style);
			horizontalDistortion = Mathf.Clamp(horizontalDistortion, TextWarpField.MIN_DISTORTION, TextWarpField.MAX_DISTORTION);
			verticalDistortion = Mathf.Clamp(verticalDistortion, TextWarpField.MIN_DISTORTION, TextWarpField.MAX_DISTORTION);
			amount = Mathf.Clamp01(amount);
			waves = Mathf.Clamp(waves, MIN_WAVES, MAX_WAVES);
			speed = Mathf.Clamp(speed, MIN_SPEED, MAX_SPEED);
			phase = Mathf.Clamp01(phase);
		}

		private static bool IsFinite(float value)
		{
			return !float.IsNaN(value) && !float.IsInfinity(value);
		}

		private TMP_Text GetText()
		{
			if (tmpText == null)
			{
				tmpText = GetComponent<TMP_Text>();
			}
			return tmpText;
		}

		private TextMeshHandler GetMeshHandler()
		{
			return TextMeshHandlerRegistry.GetTextMeshHandler(gameObject.GetInstanceID());
		}

		// The handler may not exist yet; the master configurator attaches the warp when it creates one
		private void AttachToMeshHandler()
		{
			TextMeshHandler meshHandler = GetMeshHandler();
			if (meshHandler == null || meshHandler.tmpText == null) { return; }

			meshHandler.outputModifier = this;
			meshHandler.WriteChanges();
		}

		private void DetachFromMeshHandler()
		{
			TextMeshHandler meshHandler = GetMeshHandler();
			if (meshHandler == null || meshHandler.outputModifier != (ITextOutputModifier)this) { return; }

			meshHandler.outputModifier = null;
			if (meshHandler.tmpText != null)
			{
				meshHandler.WriteChanges();
			}
		}

		// Pushes the warp to the mesh; a playing typewriter does this on its own every frame. Reattaches if a handler reset dropped the warp while it stayed enabled
		public void RequestWriteBack()
		{
			TextMeshHandler meshHandler = GetMeshHandler();
			if (meshHandler == null || meshHandler.tmpText == null) { return; }

			if (isActiveAndEnabled)
			{
				meshHandler.outputModifier = this;
			}
			meshHandler.WriteChanges();
		}
	}
}
