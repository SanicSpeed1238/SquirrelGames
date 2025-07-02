using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class MG_GameManager : MonoBehaviour
{
    public static MG_GameManager Instance;

    [Header("Game UI Components")]
    public TextMeshProUGUI messageUI;
    public TextMeshProUGUI timeUI;
    public GameObject scoresUI;

    [Header("All UI Components")]
    public GameObject gameUIScreen; 
    public GameObject startScreen;
    public GameObject pauseScreen;
    public GameObject resultsScreen;
    public GameObject transitionScreen;

    [Header("Camera Components")]
    public GameObject gameCamera;
    public GameObject openingCutscene;
    public GameObject resultsCutscene;

    // Game States
    [HideInInspector] public bool gameStarted;
    [HideInInspector] public bool gamePlaying;
    [HideInInspector] public bool gameEnded;

    // Important References
    private MG_GameSetup gameSetup;
    private MG_GameResults gameResults;
    private MG_AudioController audioPlayer;

    // UI Animation Variables
    private readonly float countdownDelay = 2.0f;
    private readonly float readyAppearSpeed = 1.0f;
    private readonly float readyDisplayDuration = 3f;
    private readonly float goDisplayDuration = 1.5f;
    private readonly float goDisappearSpeed = 0.2f;
    private readonly float finishAppearSpeed = 1.0f;
    private readonly float finishDisplayDuration = 2.0f;
    private readonly float finishDisappearSpeed = 0.2f;
    private readonly float resultsDelay = 2.0f;
    private readonly float fontSizeIncrease = 100f;

    private void Awake()
    {     
        Instance = this;

        openingCutscene.SetActive(false);
        resultsCutscene.SetActive(false);
        gameCamera.SetActive(true);
     
        transitionScreen.SetActive(true);
        startScreen.SetActive(true);
        resultsScreen.SetActive(false);
        pauseScreen.SetActive(false);
        gameUIScreen.SetActive(false);

        timeUI.text = string.Empty;
        messageUI.text = string.Empty;

        gameSetup = GetComponent<MG_GameSetup>();
        gameResults = GetComponent<MG_GameResults>();
        audioPlayer = FindAnyObjectByType<MG_AudioController>();
    }
    private void Start()
    {
        if (gameResults.scoringMethod == MG_GameResults.ScoringMethod.Goal)
        {
            for (int p = 0;  p < scoresUI.transform.childCount; p++)
            {
                scoresUI.transform.GetChild(p).gameObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "...";
            }
        }
    }

    #region Public Functions

    public GameObject[] GetAllPlayers()
    {
        return gameSetup.playersInGame;
    }
    public GameObject GetPlayer(int playerID)
    {
        return gameSetup.playersInGame[playerID - 1];
    }

    public void UpdateUITimer(float timeValue)
    {
        timeUI.text = timeValue.ToString("F0");
    }

    #endregion

    #region Updates Players Scores

    // Simply updates a player's score. If the newScore is 0 or less, that player is marked as "out".
    public void UpdatePlayerScore(int playerID, int newScore, int addedScore)
    {
        string playerTextObject = "Player" + playerID;
        GameObject playerScoreUI = scoresUI.transform.Find(playerTextObject).gameObject;

        gameResults.playerScores[playerID - 1] = newScore;
        if (newScore >= 0)
        {
            playerScoreUI.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = newScore.ToString("D2");

            playerScoreUI.GetComponentInChildren<Animator>().ResetTrigger("Add");
            playerScoreUI.GetComponentInChildren<Animator>().ResetTrigger("Subtract");
            if (addedScore > 0)
            {
                playerScoreUI.GetComponentInChildren<Animator>().SetTrigger("Add");
            }
            else
            {
                playerScoreUI.GetComponentInChildren<Animator>().SetTrigger("Subtract");
            }
        }
        else
        {
            playerScoreUI.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "OUT";
            UpdatePlayerFinished(playerID);
        }
    }

    // This marks a player as either "qualified" or "out". Players marked as "qualified" all win.
    public void UpdatePlayerQualified(int playerID, bool successful)
    {
        string playerTextObject = "Player" + playerID;
        GameObject playerScoreUI = scoresUI.transform.Find(playerTextObject).gameObject;

        if (successful)
        {
            gameResults.playerScores[playerID - 1] = 1;
            playerScoreUI.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = gameResults.GetNextPlacement();
        }
        else
        {   
            gameResults.playerScores[playerID - 1] = -1;
            playerScoreUI.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "OUT";
        }

        UpdatePlayerFinished(playerID);
    }

    // Use this in case you want to mark a player as "finished" WITHOUT modifying their score.
    public void UpdatePlayerFinished(int playerID)
    {
        gameResults.playersFinished[playerID - 1] = true;

        foreach (bool playerFinished in gameResults.playersFinished)
        {
            if (!playerFinished) return;
        }
        EndGame();
    }

    #endregion

    #region Start Game Sequence
    public void StartGame()
    {
        StartCoroutine(StartMinigame());
    }
    IEnumerator StartMinigame()
    {
        // Transitioning to the Opening Cutscene
        // -------------------------------------
        gameStarted = true;
        audioPlayer.FadeOutMusic(1.5f);      
        if(!gameSetup.skipStartupScreen)
        {
            transitionScreen.GetComponentInChildren<Animator>().SetTrigger("Transition");
            yield return new WaitForSeconds(1.5f);
        }   
        startScreen.SetActive(false);

        // Play the Opening Cutscene
        // -------------------------
        if (openingCutscene.GetComponent<PlayableDirector>().playableAsset != null && !gameSetup.skipStartupScreen)
        {
            openingCutscene.SetActive(true);
            PlayableDirector playableDirector = openingCutscene.GetComponent<PlayableDirector>();
            playableDirector.Play();

            yield return new WaitUntil(() => playableDirector.state != PlayState.Playing);
            transitionScreen.GetComponentInChildren<Animator>().SetTrigger("Transition");
            yield return new WaitForSeconds(1.0f);
            playableDirector.gameObject.SetActive(false);
            openingCutscene.SetActive(false);
        }

        // Transitioning to the Game Camera
        // --------------------------------
        gameCamera.SetActive(true);
        if(!gameSetup.skipStartupScreen) yield return new WaitForSeconds(1.5f);
        gameUIScreen.SetActive(true);

        // Countdown to Start Gameplay
        // ---------------------------
        yield return new WaitForSeconds(countdownDelay);
        audioPlayer.PlayGameMusic();
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
    #endregion

    #region End Game Sequence
    public void EndGame()
    {      
        StartCoroutine(EndMinigame());
    }
    IEnumerator EndMinigame()
    {
        // Stop the Minigame
        // -----------------
        gamePlaying = false;
        gameResults.GetWinners();
        audioPlayer.PlayFinishSound();
        audioPlayer.FadeOutMusic(0.5f);

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
        yield return new WaitForSeconds(resultsDelay);
        transitionScreen.GetComponentInChildren<Animator>().SetTrigger("Transition");
        yield return new WaitForSeconds(1.5f);
        gameUIScreen.SetActive(false);
        gameCamera.SetActive(false);
        resultsCutscene.SetActive(true);

        // Play the Results Cutscene
        // -------------------------
        if (resultsCutscene.GetComponent<PlayableDirector>().playableAsset != null)
        {
            PlayableDirector playableDirector = resultsCutscene.GetComponent<PlayableDirector>();
            playableDirector.Play();
            yield return new WaitUntil(() => playableDirector.state != PlayState.Playing);
        }
        yield return new WaitForSeconds(1.5f);

        // Show Results then Transition out of Scene
        // -----------------------------------------
        resultsScreen.SetActive(true);
        gameResults.PlayResultsSequence();
    }
    #endregion
}