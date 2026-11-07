using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class RotateController : MonoBehaviour
	{
		public float rotationSpeed;
		public float minAngle;
		public float maxAngle;

		private void Update()
		{
			float sinValue = (Mathf.Sin(Time.time * rotationSpeed) + 1f) * 0.5f;

			Quaternion from = Quaternion.AngleAxis(minAngle, Vector3.up);
			Quaternion to = Quaternion.AngleAxis(maxAngle, Vector3.up);

			transform.localRotation = Quaternion.Slerp(from, to, sinValue);
		}
	}
}