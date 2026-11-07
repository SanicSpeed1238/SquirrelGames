using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[CreateAssetMenu(menuName = "AllIn1Text/Demo/Postprocess Config")]
	public class PostprocessConfigSO : ScriptableObject
	{
		public PostprocessConfig postprocessConfig;
		public PostprocessConfig postprocessConfigURP;
	}
}