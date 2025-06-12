using UnityEngine;
using EasyTransition;

public class MG_PauseScreen : MonoBehaviour
{
    [Header("UI Buttons")]
    public GameObject resumeButton;
    public GameObject quitButton;

    [Header("Important Components")]
    public TransitionSettings quitTransition;
    public GameObject[] screenElements;

    // Other Variables Needed
    private MG_GameManager gameManager;
    private readonly string minigameSelectSceneName = "Minigame Selection";

    private void Awake()
    {
        gameManager = FindAnyObjectByType<MG_GameManager>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Start"))
        {
            if (gameManager.gamePlaying) PauseMinigame();
        }
    }

    public void PauseMinigame()
    {
        this.gameObject.SetActive(true);
        Time.timeScale = 0;
        gameManager.gamePlaying = false;
    }

    public void ResumeMinigame()
    {
        Time.timeScale = 1;
        gameManager.gamePlaying = true;
        this.gameObject.SetActive(false);
    }

    public void QuitMinigame()
    {
        foreach (var element in screenElements)
        {
            element.SetActive(false);
        }
        TransitionManager.Instance().Transition(minigameSelectSceneName, quitTransition, 0f);
    }
}