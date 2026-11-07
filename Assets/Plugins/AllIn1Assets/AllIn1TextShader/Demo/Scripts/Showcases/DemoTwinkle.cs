using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Pulses a UI element's scale between two sizes with a slight turn, like a glint catching the light
	public class DemoTwinkle : MonoBehaviour
	{
		public float minScale = 0.3f;
		public float maxScale = 1f;
		public float period = 1.6f;
		[Range(0f, 1f)] public float phase;
		public float turnDegrees = 20f;

		private float time;

		private void OnEnable()
		{
			time = phase * period;
		}

		private void Update()
		{
			time += Time.deltaTime;
			float wave = 0.5f + 0.5f * Mathf.Sin(time / period * 2f * Mathf.PI);
			float pulse = wave * wave * wave;
			float s = Mathf.Lerp(minScale, maxScale, pulse);
			transform.localScale = new Vector3(s, s, 1f);
			transform.localRotation = Quaternion.Euler(0f, 0f, turnDegrees * (wave - 0.5f));
		}
	}
}
