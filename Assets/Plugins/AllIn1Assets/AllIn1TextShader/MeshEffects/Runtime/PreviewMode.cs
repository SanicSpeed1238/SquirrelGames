using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public enum PreviewMode
	{
		[InspectorName("None")] NONE,
		[InspectorName("Reveal")] PREVIEW_REVEAL,
		[InspectorName("Looped")] PREVIEW_LOOPED,
		[InspectorName("Hide")] PREVIEW_HIDE,
	}
}