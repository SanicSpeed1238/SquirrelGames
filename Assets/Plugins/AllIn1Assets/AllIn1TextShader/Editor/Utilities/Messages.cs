namespace AllIn1Assets.AllIn1TextShader
{
	public static class Messages
	{
		public const string STR_WARNING_FONT_ASSET_NOT_CORRECTLY_CONFIGURED
			= "To ensure the best displaying result, it's recommended to reconstruct the font atlas";

		public const string STR_FONT_ASSET_CORRECTLY_CONFIGURED
			= "Font asset is correctly configured";

		public const string STR_WARNING_DYNAMIC_FONT_BLURRED_ATLAS
			= "This font asset is Dynamic. Bevel, Outline and Drop Shadow read a blurred copy of the atlas made in the editor, so glyphs the font adds at runtime won't get them. Set the font asset's Atlas Population Mode to Static.";

		public const string STR_WARNING_NO_TEXT_COMPONENT
			= "This object has no TextMeshPro text (TextMeshPro or TextMeshPro - Text (UI)), so this component has nothing to drive. Add one to the object, or remove this component.";

		public const string STR_WARNING_OTHER_FONT_CHARACTERS
			= "{0} of this text's characters are missing from its font asset, so TextMeshPro draws them from another asset: a fallback font, a bold or italic typeface from the font's weight table, or a sprite. Those characters don't get this text's shader effects, and they don't reveal, hide or animate with it (they show from the start). To include them, add them to this font asset, or use a font asset that has them.";
	}
}