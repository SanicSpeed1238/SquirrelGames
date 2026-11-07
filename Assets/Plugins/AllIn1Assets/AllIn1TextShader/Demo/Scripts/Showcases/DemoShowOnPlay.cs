using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	//Hidden in the scene as it opens (its CanvasGroup saved at alpha 0), shown once Play starts. For UI that places itself at runtime,
	//like the Effect Info popup, which takes its saved position as its open one
	[RequireComponent(typeof(CanvasGroup))]
	public class DemoShowOnPlay : MonoBehaviour
	{
		private void Awake()
		{
			GetComponent<CanvasGroup>().alpha = 1f;
		}
	}
}
