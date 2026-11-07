using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace AllIn1Assets.AllIn1TextShader
{
	public class TextShaderDemoInputController : MonoBehaviour
	{
		private EventSystem eventSystem;

		private void Start()
		{
			CheckEventSystem();
		}

		private void CheckEventSystem()
		{
			EventSystem eventSystemInScene = GameObject.FindAnyObjectByType<EventSystem>();
			if (eventSystemInScene != null)
			{
				GameObject.Destroy(eventSystemInScene.gameObject);
			}

			GameObject goEventSystem = new GameObject("Event System");
			eventSystem = goEventSystem.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
			goEventSystem.AddComponent<InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
			goEventSystem.AddComponent<StandaloneInputModule>();
#endif
		}
	}
}
