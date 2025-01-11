using UnityEngine;
using UnityEngine.EventSystems;

public class CS_MinigameEventSystem : MonoBehaviour
{
    private void Awake()
    {
        CS_MinigamePause pauseScreen = GameObject.FindAnyObjectByType<CS_MinigamePause>();
        GetComponent<EventSystem>().firstSelectedGameObject = pauseScreen.resumeButton;
    }
}