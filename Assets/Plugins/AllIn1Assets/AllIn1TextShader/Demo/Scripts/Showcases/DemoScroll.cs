using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	//Scrolls a RawImage's texture across it at a constant speed in tiles per second; the texture must import with Repeat
	[RequireComponent(typeof(RawImage))]
	public class DemoScroll : MonoBehaviour
	{
		public Vector2 tilesPerSecond = new Vector2(-0.5f, 0f);

		private RawImage image;

		private void Awake()
		{
			image = GetComponent<RawImage>();
		}

		private void Update()
		{
			Rect r = image.uvRect;
			r.x = Mathf.Repeat(r.x + tilesPerSecond.x * Time.deltaTime, 1f);
			r.y = Mathf.Repeat(r.y + tilesPerSecond.y * Time.deltaTime, 1f);
			image.uvRect = r;
		}
	}
}
