namespace AllIn1Assets.AllIn1TextShader
{
	// Runs after the effects and writes the final vertex positions: reads currentVerts, writes outputVerts
	public interface ITextLayoutStage
	{
		void ApplyLayout(MeshCache meshCache);
	}
}
