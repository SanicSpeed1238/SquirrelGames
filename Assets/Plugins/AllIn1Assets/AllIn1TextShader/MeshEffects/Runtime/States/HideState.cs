namespace AllIn1Assets.AllIn1TextShader
{
	public class HideState : MeshEffectGroupState
	{
		public HideState(MeshEffectGroupConfig hideConfig, ref VertexGroupLayout[] vertexGroupLayouts, string groupStateName) : base(hideConfig, ref vertexGroupLayouts, 
			PlaybackMode.ONE_SHOT, GetGroupType(hideConfig), EffectFlag.All, groupStateName)
		{}
	}
}