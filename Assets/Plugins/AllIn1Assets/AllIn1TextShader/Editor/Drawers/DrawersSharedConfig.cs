namespace AllIn1Assets.AllIn1TextShader
{
	public static class DrawersSharedConfig
	{
		public static readonly CommonStyles s_commonStyles = new CommonStyles();

		public static void Setup()
		{
			s_commonStyles.InitStylesIfNeeded();
		}
	}
}