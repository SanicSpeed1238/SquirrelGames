using System;

namespace AllIn1Assets.AllIn1TextShader
{
	[Flags]
	public enum EffectFlag
	{
		None = 0,
		Fade = 1,
		InitialRotation = 2,
		Scale = 4,
		Offset = 8,
		Pendulum = 16,
		Shake = 32,
		Rainbow = 64,
		Swing = 128,
		Rotation = 256,
		Dangle = 512,
		Slide = 1024,
		VerticalMovement = 2048,
		All = ~None,
	}
}