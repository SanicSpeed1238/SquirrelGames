using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Swings a warp's Bend between two values and back, easing at both ends. Runs before the typewriter so each frame writes the mesh once
	[DefaultExecutionOrder(-20)]
	public class DemoWarpSwing : MonoBehaviour
	{
		public AllIn1TextWarp warp;
		public float bendA = -40f;
		public float bendB = 40f;
		public float period = 2f;
		[Range(0f, 1f)] public float phase;
		//Plays A to B once each time the showcase is shown, then holds B
		public bool once;
		//Swings Amount (0 to 1) instead of Bend, for Custom cages
		public bool driveAmount;

		private float time;

		private void OnEnable()
		{
			time = phase * period;
			Apply();
		}

		private void Update()
		{
			time += Time.deltaTime;
			if (once) { time = Mathf.Min(time, period * 0.5f); }
			else if (time >= period) { time -= period; }
			Apply();
		}

		private void Apply()
		{
			if (warp == null || period <= 0f) { return; }
			float t = 0.5f - 0.5f * Mathf.Cos(time / period * 2f * Mathf.PI);
			if (driveAmount) { warp.Amount = Mathf.Lerp(bendA, bendB, t); }
			else { warp.Bend = Mathf.Lerp(bendA, bendB, t); }
		}
	}
}
