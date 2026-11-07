using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// A resolved control point: tangents are offsets from the position, already computed for AUTO points
	public struct SplineKnot
	{
		public Vector2 position;
		public Vector2 tangentIn;
		public Vector2 tangentOut;

		public SplineKnot(Vector2 position, Vector2 tangentIn, Vector2 tangentOut)
		{
			this.position = position;
			this.tangentIn = tangentIn;
			this.tangentOut = tangentOut;
		}
	}
}
