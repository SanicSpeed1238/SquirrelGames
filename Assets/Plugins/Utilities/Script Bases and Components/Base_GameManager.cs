using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class Base_GameManager : MonoBehaviour
{
    [Header("Cutscene Components")]
    public GameObject openingCutscene;
    public GameObject endingCutscene;
    public GameObject gameCamera;

    [Header("UI Components")]
    public GameObject transitionScreen;
    public GameObject startScreen;
    public GameObject pauseScreen;
    public GameObject gameUIScreen;

    [Header("Game UI Components")]
    public TextMeshProUGUI messageUI;
    public TextMeshProUGUI timeUI;
    public GameObject scoresUI;

    // Game States
    [HideInInspector] public bool gameStarted;
    [HideInInspector] public bool gamePlaying;
    [HideInInspector] public bool[] playersJoined = new bool[4];
    [HideInInspector] public bool[] playersWon = new bool[4];

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

    protected void Base_GameManager_Awake()
    {
        gameCamera.SetActive(false);
        endingCutscene.SetActive(false);
        openingCutscene.SetActive(true);

        gameUIScreen.SetActive(false);
        pauseScreen.SetActive(false);
        startScreen.SetActive(true);
        
        timeUI.text = string.Empty;
        messageUI.text = string.Empty;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            pauseScreen.SetActive(true);

            Time.timeScale = 0;
            gamePlaying = false;
        }
        else
        {
            pauseScreen.SetActive(false);

            Time.timeScale = 1;
            gamePlaying = true;
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartMinigame());
    }
    public void EndGame()
    {
        StartCoroutine(EndMinigame());
    }

    IEnumerator StartMinigame()
    {
        // Transitioning to the Opening Cutscene
        // -------------------------------------        
        gameStarted = true;
        transitionScreen.GetComponentInChildren<Animator>().SetTrigger("Transition");
        yield return new WaitForSeconds(1.5f);
        gameUIScreen.SetActive(false);
        startScreen.SetActive(false);
        PlayableDirector playableDirector = openingCutscene.GetComponentInChildren<PlayableDirector>();
        playableDirector.Play();

        // Transitioning to the Game Camera
        // --------------------------------      
        yield return new WaitUntil(() => playableDirector.state != PlayState.Playing);
        transitionScreen.GetComponentInChildren<Animator>().SetTrigger("Transition");
        yield return new WaitForSeconds(1.0f);
        playableDirector.gameObject.SetActive(false);
        openingCutscene.SetActive(false);
        gameCamera.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        gameUIScreen.SetActive(true);

        // Countdown to Start Gameplay
        // ---------------------------        
        yield return new WaitForSeconds(countdownDelay);
        messageUI.gameObject.GetComponent<RectTransform>().localScale = Vector3.zero;
        LeanTween.scale(messageUI.gameObject.GetComponent<RectTransform>(), Vector3.one, readyAppearSpeed).setEase(LeanTweenType.easeOutExpo);
        messageUI.text = ("READY...");
        yield return new WaitForSeconds(readyDisplayDuration);
        messageUI.fontSize += fontSizeIncrease;
        messageUI.text = ("GO !!!");
        LeanTween.scale(messageUI.gameObject.GetComponent<RectTransform>(), Vector3.zero, goDisappearSpeed).setDelay(goDisplayDuration).setEase(LeanTweenType.linear);

        // Finish Up StartMinigame Coroutine
        // ---------------------------------  
        gamePlaying = true;
        yield return new WaitForSeconds(goDisplayDuration + goDisappearSpeed);
        messageUI.text = string.Empty;
        messageUI.fontSize -= fontSizeIncrease;
        messageUI.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    IEnumerator EndMinigame()
    {
        // TEMPORARY

        gamePlaying = false;

        messageUI.gameObject.GetComponent<RectTransform>().localScale = Vector3.zero;
        LeanTween.scale(messageUI.gameObject.GetComponent<RectTransform>(), Vector3.one, finishAppearSpeed).setEase(LeanTweenType.easeOutExpo);
        messageUI.fontSize += fontSizeIncrease;
        messageUI.text = ("FINISH!");
        LeanTween.scale(messageUI.gameObject.GetComponent<RectTransform>(), Vector3.zero, finishDisappearSpeed).setDelay(finishDisplayDuration).setEase(LeanTweenType.linear);

        yield return new WaitForSeconds(finishDisplayDuration);
        timeUI.text = string.Empty;
        yield return new WaitForSeconds(finishDisappearSpeed);
        messageUI.text = string.Empty;
        messageUI.fontSize -= fontSizeIncrease;
        messageUI.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;

        yield return new WaitForSeconds(endingDelay);
    }
}