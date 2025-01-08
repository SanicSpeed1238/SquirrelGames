using UnityEngine;
using UnityEngine.UI;

public class CS_MinigamePause : MonoBehaviour
{
    public GameObject resumeButton;
    public GameObject quitButton;

    private Base_GameManager gameManager;

    private void Awake()
    {
        gameManager = GetComponent<Base_GameManager>();

        resumeButton.GetComponent<Button>().onClick.AddListener(() => gameManager.PauseGame(false));
    }
}