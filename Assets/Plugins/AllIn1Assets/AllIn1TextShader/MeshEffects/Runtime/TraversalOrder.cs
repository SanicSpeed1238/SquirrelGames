using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public enum TraversalOrder
	{
		[InspectorName("Left to Right")] LEFT_TO_RIGHT,
		[InspectorName("Right to Left")] RIGHT_TO_LEFT,
		[InspectorName("Center Outward")] FROM_CENTER_TO_OUTWARD,
		[InspectorName("Outside In")] FROM_OUTWARD_TO_CENTER
	}
}