using UnityEditor;

namespace AllIn1Assets.AllIn1TextShader
{
	public class TextShaderMaterialEditor : MaterialEditor
	{
		public override void OnInspectorGUI()
		{
			MaterialProperty[] materialProperties = GetMaterialProperties(base.targets);
			customShaderGUI.OnGUI(this, materialProperties);
		}
	}
}