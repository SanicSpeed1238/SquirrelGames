using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public enum GroupType
	{
		[InspectorName("Per Character")] PER_CHARACTER = 0,
		[InspectorName("Per Word")] PER_WORD = 1,
		[InspectorName("Per Tags")] PER_TAGS = 2,
	}
}