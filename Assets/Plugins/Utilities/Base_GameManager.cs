using UnityEngine;

public class Base_GameManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject startScreen;
    public GameObject transitionScreen;
    public GameObject gameUIScreen;

    [Header("Cutscene Components")]
    public GameObject openingCutscene;
    public GameObject gameCamera;

    // Game States
    [HideInInspector] public bool gameStarted;
    [HideInInspector] public bool gamePlaying;

    // UI Animation Variables
    protected readonly float countdownDelay = 2.0f;
    protected readonly float readyAppearSpeed = 1.0f;
    protected readonly float readyDisplayDuration = 1.5f;
    protected readonly float goDisplayDuration = 1.5f;
    protected readonly float goDisappearSpeed = 0.2f;
    protected readonly float finishAppearSpeed = 1.0f;
    protected readonly float finishDisplayDuration = 2.0f;
    protected readonly float finishDisappearSpeed = 0.2f;
    protected readonly float endingDelay = 2.0f;
    protected readonly float fontSizeIncrease = 100f;

    // Override this function with a "StartMinigame" Coroutine!
    public virtual void StartGame()
    {
        Debug.LogWarning("No StartMinigame Coroutine has been defined.");
    }

    protected void Base_GameManager_Awake()
    {
        gameCamera.SetActive(false);
        openingCutscene.SetActive(true);

        startScreen.SetActive(true);
        gameUIScreen.SetActive(false);
    }
}