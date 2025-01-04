using UnityEngine;
using UnityEngine.UI;

public class CS_MinigamePause : MonoBehaviour
{
    public GameObject resumeButton;
    public GameObject quitButton;

    private void Awake()
    {
        Base_GameManager gameManager = GameObject.FindAnyObjectByType<Base_GameManager>();

        resumeButton.GetComponent<Button>().onClick.AddListener(() => gameManager.PauseGame(false));
    }
}