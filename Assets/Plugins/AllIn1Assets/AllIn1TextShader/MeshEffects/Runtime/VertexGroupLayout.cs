using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public struct VertexGroupLayout
	{
		public int groupIndex;
		public int startVertexIndex;
		public int endVertexIndex;

		public Vector3 center;
		public Vector3 centerTop;
		// Unit direction of the first letter's bottom edge, the axis Scale's X runs along
		public Vector2 right;

		public EffectFlag enabledEffectsMask;
		public VertexGroupDisplayState displayState;
		public PlaybackMode playbackMode;

		// Group-local normalized time when the reveal or hide is scrubbed instead of ticked (editor preview, spline travel)
		public float timerNormalized;

		public InitialRotationStateValues initialRotationStateValues;
		public ScaleStateValues scaleStateValues;
		public FadeStateValues alphaStateValues;
		public OffsetStateValues offsetStateValues;
		public PendulumStateValues pendulumStateValues;
		public ShakeStateValues shakeStateValues;
		public RainbowStateValues rainbowStateValues;
		public SwingStateValues swingStateValues;
		public RotationStateValues rotationStateValues;
		public DangleStateValues dangleStateValues;
		public SlideStateValues slideStateValues;
		public VerticalMovementStateValues verticalMovementStateValues;

		public void InitAtRandomPoint()
		{
			initialRotationStateValues.InitAtRandomPoint();
			scaleStateValues.InitAtRandomPoint();
			alphaStateValues.InitAtRandomPoint();
			offsetStateValues.InitAtRandomPoint();
			pendulumStateValues.InitAtRandomPoint();
			shakeStateValues.InitAtRandomPoint();
			rainbowStateValues.InitAtRandomPoint();
			swingStateValues.InitAtRandomPoint();
			rotationStateValues.InitAtRandomPoint();
			dangleStateValues.InitAtRandomPoint();
			slideStateValues.InitAtRandomPoint();
			verticalMovementStateValues.InitAtRandomPoint();
		}

		public void Reset()
		{
			timerNormalized = 0f;

			displayState = VertexGroupDisplayState.Hidden;

			initialRotationStateValues.Reset();
			scaleStateValues.Reset();
			alphaStateValues.Reset();
			offsetStateValues.Reset();
			pendulumStateValues.Reset();
			shakeStateValues.Reset();
			rainbowStateValues.Reset();
			swingStateValues.Reset();
			rotationStateValues.Reset();
			dangleStateValues.Reset();
			slideStateValues.Reset();
			verticalMovementStateValues.Reset();
		}

		public void SetDisplayState(VertexGroupDisplayState displayState)
		{
			this.displayState = displayState;
		}

		public void SetRight(Vector3 bottomLeft, Vector3 bottomRight)
		{
			Vector2 edge = bottomRight - bottomLeft;
			float sqrLength = edge.sqrMagnitude;
			right = sqrLength > 1e-12f ? edge / Mathf.Sqrt(sqrLength) : Vector2.right;
		}

		public void Update(float deltaTime)
		{
			if (displayState == VertexGroupDisplayState.Revealing)
			{
				initialRotationStateValues.Update(deltaTime);
				scaleStateValues.Update(deltaTime);
				alphaStateValues.Update(deltaTime);
				offsetStateValues.Update(deltaTime);
				pendulumStateValues.Update(deltaTime);
				shakeStateValues.Update(deltaTime);
				rainbowStateValues.Update(deltaTime);
				swingStateValues.Update(deltaTime);
				rotationStateValues.Update(deltaTime);
				dangleStateValues.Update(deltaTime);
				slideStateValues.Update(deltaTime);
				verticalMovementStateValues.Update(deltaTime);
			}
		}

		public void SetNormalizedProgress(float normalizedProgress)
		{
			initialRotationStateValues.SetNormalizedProgress(normalizedProgress);
			scaleStateValues.SetNormalizedProgress(normalizedProgress);
			alphaStateValues.SetNormalizedProgress(normalizedProgress);
			offsetStateValues.SetNormalizedProgress(normalizedProgress);
			pendulumStateValues.SetNormalizedProgress(normalizedProgress);
			shakeStateValues.SetNormalizedProgress(normalizedProgress);
			rainbowStateValues.SetNormalizedProgress(normalizedProgress);
			swingStateValues.SetNormalizedProgress(normalizedProgress);
			rotationStateValues.SetNormalizedProgress(normalizedProgress);
			dangleStateValues.SetNormalizedProgress(normalizedProgress);
			slideStateValues.SetNormalizedProgress(normalizedProgress);
			verticalMovementStateValues.SetNormalizedProgress(normalizedProgress);
		}

		public float GetProgressNormalized(int numEffectsEnabled)
		{
			// No effects to wait for (the None preset): done once the stagger has started the group, as IsComplete counts it
			if (numEffectsEnabled <= 0)
			{
				return displayState == VertexGroupDisplayState.Hidden ? 0f : 1f;
			}

			// Each effect counts up to 1: its progress keeps running past it, and a fast effect would cover for a slow one
			float res = 0f;

			res += Mathf.Min(1f, initialRotationStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, scaleStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, alphaStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, offsetStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, pendulumStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, shakeStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, rainbowStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, swingStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, rotationStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, dangleStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, slideStateValues.GetNormalizedProgress());
			res += Mathf.Min(1f, verticalMovementStateValues.GetNormalizedProgress());

			res = res / numEffectsEnabled;
			res = Mathf.Clamp01(res);

			return res;
		}

		// One effect's own progress, not averaged with the others. Shake and Rotation have no end, so they count as done
		public float GetEffectProgress(EffectFlag effect)
		{
			float res = 1f;

			switch (effect)
			{
				case EffectFlag.Fade: res = alphaStateValues.GetNormalizedProgress(); break;
				case EffectFlag.InitialRotation: res = initialRotationStateValues.GetNormalizedProgress(); break;
				case EffectFlag.Scale: res = scaleStateValues.GetNormalizedProgress(); break;
				case EffectFlag.Offset: res = offsetStateValues.GetNormalizedProgress(); break;
				case EffectFlag.Pendulum: res = pendulumStateValues.GetNormalizedProgress(); break;
				case EffectFlag.Rainbow: res = rainbowStateValues.GetNormalizedProgress(); break;
				case EffectFlag.Swing: res = swingStateValues.GetNormalizedProgress(); break;
				case EffectFlag.Dangle: res = dangleStateValues.GetNormalizedProgress(); break;
				case EffectFlag.Slide: res = slideStateValues.GetNormalizedProgress(); break;
				case EffectFlag.VerticalMovement: res = verticalMovementStateValues.GetNormalizedProgress(); break;
			}

			return res;
		}

		public int GetNumVertices()
		{
			int res = endVertexIndex - startVertexIndex + 1;
			return res;
		}
	}
}