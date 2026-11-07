namespace AllIn1Assets.AllIn1TextShader
{
	// Runs after the layout stage on every write-back and edits outputVerts in place. The layout stage rewrites the whole output first, so edits never accumulate
	public interface ITextOutputModifier
	{
		void ModifyOutput(MeshCache meshCache);
	}
}
