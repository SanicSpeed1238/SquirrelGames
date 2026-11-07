using AllIn1Assets.SharedExtensions;

namespace AllIn1Assets.AllIn1TextShader
{
	public class OuterGlowDrawer : AbstractEffectDrawer
	{
		public OuterGlowDrawer(Plugin3DMaterialInspectorReferences references, PropertiesConfig propertiesConfig, Plugin3DConfig pluginConfig) : base(references, propertiesConfig, pluginConfig)
		{
			this.drawerID = AllIn1TextConstants.OUTER_GLOW_EFFECT_DRAWER_ID;

			effectConfig = propertiesConfig.FindEffectConfigByID("OUTER_GLOW");
		}

		protected override void DrawProperties()
		{
			base.DrawProperties();
			AtlasRangeNote.DrawOutsideStackNote(references);
		}
	}
}
