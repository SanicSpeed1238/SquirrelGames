using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// Where a Banner is held still, like a flag on its pole: the wave grows from that end
	public enum TextWarpPin
	{
		[InspectorName("None")] NONE = 0,
		[InspectorName("Start (left or top)")] START = 1,
		[InspectorName("End (right or bottom)")] END = 2
	}
}
