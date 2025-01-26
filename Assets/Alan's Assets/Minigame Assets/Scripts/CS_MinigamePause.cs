using UnityEngine;
using UnityEngine.UI;
using EasyTransition;

public class CS_MinigamePause : MonoBehaviour
{
    [Header("UI Buttons")]
    public GameObject resumeButton;
    public GameObject quitButton;

    [Header("Important Components")]
    public TransitionSettings quitTransition;
    public GameObject[] otherElements;

    private Base_GameManager gameManager;
    private readonly string minigameSelectSceneName = "Minigame Selection";

    private void Awake()
    {
        gameManager = FindAnyObjectByType<Base_GameManager>();

        resumeButton.GetComponent<Button>().onClick.AddListener(() => gameManager.PauseGame(false));
        quitButton.GetComponent<Button>().onClick.AddListener(() => gameManager.QuitGame());
    }

    public void PauseMinigame()
    {
        Time.timeScale = 0;
        gameManager.gamePlaying = false;
    }

    public void ResumeMinigame()
    {
        Time.timeScale = 1;
        gameManager.gamePlaying = true;
    }

    public void QuitMinigame()
    {
        resumeButton.SetActive(false);
        quitButton.SetActive(false);
        foreach (var otherElement in otherElements)
        {
            otherElement.gameObject.SetActive(false);
        }

        TransitionManager.Instance().Transition(minigameSelectSceneName, quitTransition, 0f);
    }
}