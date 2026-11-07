using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// FORWARD enters through the window's start and leaves through its end, BACKWARD the reverse
	public enum SplineTravelDirection
	{
		[InspectorName("Forward")] FORWARD,
		[InspectorName("Backward")] BACKWARD
	}
}
