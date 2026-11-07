using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// Starts the Shine sweep from its edge. The demo calls Restart from a typewriter's On Reveal Started event, so the shine crosses the text as it appears
	public class DemoShineSync : MonoBehaviour
	{
		private static readonly int SHINE_LOCATION_ID = Shader.PropertyToID("_ShineLocation");
		private static readonly int SHINE_SPEED_ID = Shader.PropertyToID("_ShineSpeed");
		private static readonly int SHINE_WIDTH_ID = Shader.PropertyToID("_ShineWidth");
		private static readonly int SHINE_ROTATE_ID = Shader.PropertyToID("_ShineRotate");

		public float delay = 0.1f;

		public void Restart()
		{
			TextMaterialConfiguratorMaster configurator = GetComponent<TextMaterialConfiguratorMaster>();
			Material mat = configurator != null ? configurator.TextMaterial : null;
			if (mat == null) { return; }

			float speed = mat.GetFloat(SHINE_SPEED_ID);
			float width = mat.GetFloat(SHINE_WIDTH_ID);
			float angle = mat.GetFloat(SHINE_ROTATE_ID);

			// Half the range the rotated rect projects onto, around its middle at 0.5
			float c = Mathf.Cos(angle);
			float s = Mathf.Sin(angle);
			float half = 0.25f * (Mathf.Abs(c + s) + Mathf.Abs(c - s));
			float start = speed >= 0f ? 0.5f + half + width : 0.5f - half - width;

			// The shader places the shine at _ShineLocation - _Time.y * _ShineSpeed
			mat.SetFloat(SHINE_LOCATION_ID, start + (Time.timeSinceLevelLoad + delay) * speed);
		}
	}
}
