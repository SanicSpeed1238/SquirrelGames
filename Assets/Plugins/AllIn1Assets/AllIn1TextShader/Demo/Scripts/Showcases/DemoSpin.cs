using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Turns a UI element around its own pivot at a constant speed; positive is counterclockwise
	public class DemoSpin : MonoBehaviour
	{
		public float degreesPerSecond = 15f;

		private void Update()
		{
			transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
		}
	}
}
