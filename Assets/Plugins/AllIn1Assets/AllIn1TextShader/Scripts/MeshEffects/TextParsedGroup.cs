namespace AllIn1Assets.AllIn1TextShader
{
	public struct TextParsedGroup
	{
		public int initIndex;
		public int length;
		public EffectFlag effectMask;

		public InitialRotationStateValues initialRotationStateValues;
		public FadeStateValues alphaStateValues;
		
		public ScaleStateValues scaleStateValues;
		public OffsetStateValues offsetStateValues;
		public PendulumStateValues pendulumStateValues;
		public ShakeStateValues shakeStateValues;
		public RainbowStateValues rainbowStateValues;
		public SwingStateValues swingStateValues;
		public RotationStateValues rotationStateValues;
		public DangleStateValues dangleStateValues;
		public SlideStateValues slideStateValues;
		public VerticalMovementStateValues verticalMovementValues;
	}
}