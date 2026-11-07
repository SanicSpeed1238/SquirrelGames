using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	[System.Serializable]
	public class TypewriterEntry
	{
		public enum VerticalAlignment
		{
			TOP,
			MIDDLE,
			BOTTOM,
		}

		public enum HorizontalAlignment
		{
			LEFT,
			CENTER,
			RIGHT,
		}

		[TextArea] public string text;
		public VerticalAlignment verticalAlignment = VerticalAlignment.TOP;
		public HorizontalAlignment horizontalAlignment = HorizontalAlignment.LEFT;
		public int fontSize = 20;

		public MeshEffectGroupConfig revealConfig;
		public MeshEffectGroupConfig loopedConfig;
		public MeshEffectGroupConfig hideConfig;
	}
}