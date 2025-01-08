using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class Base_GameManager : MonoBehaviour
{
    [Header("Camera Components")]
    public GameObject gameCamera;
    public GameObject openingCutscene;
    public GameObject resultsCutscene; 

    [Header("UI Components")]
    public GameObject gameUIScreen;
    public GameObject pauseScreen;
    public GameObject resultsScreen;
    public GameObject startScreen;
    public GameObject transitionScreen;

    [Header("Game UI Components")]
    public TextMeshProUGUI messageUI;
    public TextMeshProUGUI timeUI;
    public GameObject scoresUI;

    // Game States
    [HideInInspector] public bool gameStarted;
    [HideInInspector] public bool gamePlaying;

    // Important References
    protected CS_MinigameMusic musicPlayer;
    protected CS_MinigameResults minigameResults;

    // UI Animation Variables
    protected readonly float countdownDelay = 2.0f;
    protected readonly float readyAppearSpeed = 1.0f;
    protected readonly float readyDisplayDuration = 2f;
    protected readonly float goDisplayDuration = 1.5f;
    protected readonly float goDisappearSpeed = 0.2f;
    protected readonly float finishAppearSpeed = 1.0f;
    protected readonly float finishDisplayDuration = 2.0f;
    protected readonly float finishDisappearSpeed = 0.2f;
    protected readonly float endingDelay = 2.0f;
    protected readonly float fontSizeIncrease = 100f;

    protected void Base_GameManager_Awake()
    {     
        openingCutscene.SetActive(false);
        resultsCutscene.SetActive(false);
        gameCamera.SetActive(true);
     
        startScreen.SetActive(true);
        resultsScreen.SetActive(false);
        pauseScreen.SetActive(false);
        gameUIScreen.SetActive(false);

        timeUI.text = string.Empty;
        messageUI.text = string.Empty;

        musicPlayer = GameObject.FindAnyObjectByType<CS_MinigameMusic>();
        minigameResults = GetComponent<CS_MinigameResults>();
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
        musicPlayer.FadeOutMusic(1.5f);
        yield return new WaitForSeconds(1.5f);
        startScreen.SetActive(false);
        openingCutscene.SetActive(true);
        PlayableDirector playableDirector = openingCutscene.GetComponent<PlayableDirector>();
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
        musicPlayer.PlayGameMusic();
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
        // Stop the Minigame
        // -----------------
        gamePlaying = false;
        musicPlayer.StopMusic();

        // Display the "Finish!" Message
        // -----------------------------
        messageUI.gameObject.GetComponent<RectTransform>().localScale = Vector3.zero;
        LeanTween.scale(messageUI.gameObject.GetComponent<RectTransform>(), Vector3.one, finishAppearSpeed).setEase(LeanTweenType.easeOutExpo);
        messageUI.fontSize += fontSizeIncrease;
        messageUI.text = ("FINISH!");
        LeanTween.scale(messageUI.gameObject.GetComponent<RectTransform>(), Vector3.zero, finishDisappearSpeed).setDelay(finishDisplayDuration).setEase(LeanTweenType.linear);

        // Clear the "Finish!" Message
        // ---------------------------
        yield return new WaitForSeconds(finishDisplayDuration);
        timeUI.text = string.Empty;
        yield return new WaitForSeconds(finishDisappearSpeed);
        messageUI.text = string.Empty;
        messageUI.fontSize -= fontSizeIncrease;
        messageUI.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;

        // Transitioning to the Results Cutscene
        // -------------------------------------
        yield return new WaitForSeconds(endingDelay);
        transitionScreen.GetComponentInChildren<Animator>().SetTrigger("Transition");
        yield return new WaitForSeconds(1.5f);
        gameUIScreen.SetActive(false);
        gameCamera.SetActive(false);
        resultsCutscene.SetActive(true);
        PlayableDirector playableDirector = resultsCutscene.GetComponent<PlayableDirector>();
        playableDirector.Play();

        // Play the Results Cutscene
        // -------------------------
        yield return new WaitUntil(() => playableDirector.state != PlayState.Playing);
        minigameResults.DisplayWinners();
        minigameResults.PlayResultsMusic();
        resultsScreen.SetActive(true);
    }
}