using AllIn1Assets.SharedExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	public class OtherTextShaderTabDrawer : OtherTabDrawer
	{
		public OtherTextShaderTabDrawer(CommonStyles commonStyles, AllIn1CommonWindow parentWindow, Plugin3DConfig pluginConfig) : base(commonStyles, parentWindow, pluginConfig)
		{
		}

		public override void Draw()
		{
			base.Draw();

			EditorGUILayout.Space();
			EditorUtils.DrawThinLine();
			EditorGUILayout.Space();

			if (GUILayout.Button("Refresh Mesh Effects configuration"))
			{
				AllIn1TextShaderConfig allIn1TextShaderConfig = (AllIn1TextShaderConfig)pluginConfig;
				allIn1TextShaderConfig.RefreshMeshEffectConfigCollections();
			}
		}
	}
}