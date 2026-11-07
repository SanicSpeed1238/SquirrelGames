using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// Saved by value: a new style takes the next free number wherever it is listed. Look alike Photoshop styles sit under More
	public enum TextWarpStyle
	{
		[InspectorName("Arc")] ARC = 0,
		[InspectorName("Arch")] ARCH = 1,
		[InspectorName("Bulge")] BULGE = 2,
		[InspectorName("Flag")] FLAG = 3,
		[InspectorName("Banner")] BANNER = 17,
		[InspectorName("Rise")] RISE = 4,
		[InspectorName("Fisheye")] FISHEYE = 5,
		[InspectorName("Twist")] TWIST = 6,
		[InspectorName("Circle")] CIRCLE = 7,
		[InspectorName("Custom")] CUSTOM = 8,
		[InspectorName("More/Arc Lower")] ARC_LOWER = 9,
		[InspectorName("More/Arc Upper")] ARC_UPPER = 10,
		[InspectorName("More/Shell Lower")] SHELL_LOWER = 11,
		[InspectorName("More/Shell Upper")] SHELL_UPPER = 12,
		[InspectorName("More/Wave")] WAVE = 13,
		[InspectorName("More/Fish")] FISH = 14,
		[InspectorName("More/Inflate")] INFLATE = 15,
		[InspectorName("More/Squeeze")] SQUEEZE = 16
	}
}
