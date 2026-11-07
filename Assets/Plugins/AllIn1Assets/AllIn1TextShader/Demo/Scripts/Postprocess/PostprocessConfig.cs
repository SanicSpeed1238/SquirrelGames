using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public struct PostprocessConfig
	{
		public static PostprocessConfig DEFAULT => new PostprocessConfig()
		{
			bloomEnabled = true,
			bloomIntensity = 0f,
			bloomColor = Color.white,
			bloomThreshold = 1.0f,
		};
		

		public bool bloomEnabled;
		public float bloomIntensity;
		public Color bloomColor;
		public float bloomThreshold;

		public static PostprocessConfig Lerp(PostprocessConfig from, PostprocessConfig to, float t)
		{
			PostprocessConfig res = new PostprocessConfig();

			res.bloomEnabled = to.bloomEnabled;
			res.bloomIntensity = Mathf.Lerp(from.bloomIntensity, to.bloomIntensity, t);
			res.bloomColor = Color.Lerp(from.bloomColor, to.bloomColor, t);
			res.bloomThreshold = Mathf.Lerp(from.bloomThreshold, to.bloomThreshold, t);

			return res;
		}
	}
}