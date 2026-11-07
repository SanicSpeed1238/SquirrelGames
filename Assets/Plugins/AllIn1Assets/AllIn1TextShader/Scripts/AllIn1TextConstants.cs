namespace AllIn1Assets.AllIn1TextShader
{
	public static class AllIn1TextConstants
	{
#if UNITY_EDITOR

		//
		public const string PREFIX_MAT_TEXT_LAYER = "MAT_TextLayer_";

		//Material Property Names
		public const string MAT_PROP_FONT_ATLAS = "_MainTex";
		public const string MAT_PROP_BLURRED_ATLAS = "_BlurredAtlas";

		//Shader Attributes
		public const string SHADER_ATTRIBUTE_FONT_PROPERTY = "FontProperty";

		//Drawers IDs
		public const string BEVEL_EFFECT_DRAWER_ID = "BEVEL_EFFECT_DRAWER";
		public const string OUTLINE_EFFECT_DRAWER_ID = "OUTLINE_EFFECT_DRAWER";
		public const string DROP_SHADOW_EFFECT_DRAWER_ID = "DROP_SHADOW_EFFECT_DRAWER";
		public const string OUTER_GLOW_EFFECT_DRAWER_ID = "OUTER_GLOW_EFFECT_DRAWER";
		public const string EXTRUDE_EFFECT_DRAWER_ID = "EXTRUDE_EFFECT_DRAWER";

		//Folder names
		public const string MESH_EFFECTS_FOLDER_NAME = "MeshEffects";

		//File Names
		public const string REVEAL_EFFECT_DATABASE_FILE_NAME = "RevealEffectDatabase.asset";
		public const string LOOPED_EFFECT_DATABASE_FILE_NAME = "LoopedEffectDatabase.asset";

		public const string HIDE_CONFIG_COLLECTION_FILE_NAME = "HideConfigCollection.asset";
		public const string LOOPED_CONFIG_COLLECTION_FILE_NAME = "LoopedConfigCollection.asset";
		public const string REVEAL_CONFIG_COLLECTION_FILE_NAME = "RevealConfigCollection.asset";

		public const string EASE_FUNCTION_COLLECTION_LOOPED_FILE_NAME = "EaseFunctionCollection_Looped.asset";
		public const string EASE_FUNCTION_COLLECTION_ONCE_FILE_NAME = "EaseFunctionCollection_Once.asset";

		public const string DEFAULT_REVEAL_CONFIG_FILE_NAME = "RevealConfig_DEFAULT.asset";
		public const string DEFAULT_LOOPED_CONFIG_FILE_NAME = "LoopedConfig_DEFAULT.asset";
		public const string DEFAULT_HIDE_CONFIG_FILE_NAME = "HideConfig_DEFAULT.asset";
		public const string NONE_REVEAL_CONFIG_FILE_NAME = "RevealConfig_NONE.asset";


		public static readonly string[] EFFECTS_BLURRED_ATLAS = new string[]
		{
			"BEVEL",
			"OUTLINE",
			"DROP_SHADOW"
		};

#endif
	}
}