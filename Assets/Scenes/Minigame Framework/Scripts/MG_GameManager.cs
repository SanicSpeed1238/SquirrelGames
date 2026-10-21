using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using TMPro;
using DG.Tweening;

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
    private TransitionManager transitionManager;

    private void Awake()
    {     
        Instance = this;

        gameSetup = GetComponent<MG_GameSetup>();
        gameResults = GetComponent<MG_GameResults>();
        audioPlayer = FindAnyObjectByType<MG_AudioController>();
        transitionManager = FindAnyObjectByType<TransitionManager>();

        gameUIScreen.SetActive(false);    
        messageUI.text = string.Empty;
        timeUI.text = string.Empty;

        startScreen.SetActive(true);
        resultsScreen.SetActive(false);
        pauseScreen.SetActive(false);

        gameCamera.SetActive(true);
        openingCutscene.SetActive(false);
        resultsCutscene.SetActive(false);
    }
    private void Start()
    {
        SetUpScoresUI();
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
            else if (addedScore < 0)
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

    // For this script only.
    private void SetUpScoresUI()
    {
        scoresUI = scoresUI.transform.GetChild((int)gameResults.scoresUILayout).gameObject;
        scoresUI.SetActive(true);

        if (gameResults.scoringMethod == MG_GameResults.ScoringMethod.Goal)
        {
            for (int p = 0; p < scoresUI.transform.childCount; p++)
            {
                scoresUI.transform.GetChild(p).gameObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "...";
            }
        }
    }

    #endregion

    #region Start Game Sequence
    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }
    IEnumerator StartGameCoroutine()
    {
        // Transition to Opening Cutscene
        // ------------------------------
        gameStarted = true;
        audioPlayer.FadeOutMusic(1.5f);      
        if(!gameSetup.skipStartupScreen)
        {
            transitionManager.PlayTransition();
            yield return new WaitForSeconds(transitionManager.midpoint);
        }   
        startScreen.SetActive(false);

        // Play Opening Cutscene
        // ---------------------
        if (openingCutscene.GetComponent<PlayableDirector>().playableAsset != null && !gameSetup.skipStartupScreen)
        {
            openingCutscene.SetActive(true);
            PlayableDirector playableDirector = openingCutscene.GetComponent<PlayableDirector>();
            playableDirector.Play();

            yield return new WaitUntil(() => playableDirector.state != PlayState.Playing);
            transitionManager.PlayTransition();
            yield return new WaitForSeconds(transitionManager.midpoint);
            playableDirector.gameObject.SetActive(false);
            openingCutscene.SetActive(false);
        }

        // Transition to Game Camera
        // -------------------------
        gameCamera.SetActive(true);
        if(!gameSetup.skipStartupScreen) yield return new WaitForSeconds(transitionManager.midpoint);
        gameUIScreen.SetActive(true);

        // Countdown to Gameplay
        // ---------------------
        yield return new WaitForSeconds(2f);
        audioPlayer.PlayGameMusic();
        StartGameCountdownDOTween();
        yield return new WaitForSeconds(gameSetup.countdownDuration);

        // Finish Coroutine
        // ----------------
        gamePlaying = true;
    }
    private void StartGameCountdownDOTween()
    {   
        Sequence sequence = DOTween.Sequence();
        float animTime = 0.3f;

        // READY animation
        // ---------------
        sequence.AppendCallback(() =>
        {
            messageUI.text = "READY";
            messageUI.transform.localScale = Vector3.zero;
        });
        sequence.Append(
            messageUI.transform.DORotate(new Vector3(0f, 0f, 360f), animTime,
            RotateMode.FastBeyond360).SetEase(Ease.OutCubic)
        );
        sequence.Join(
            messageUI.transform.DOScale(Vector3.one, animTime).SetEase(Ease.OutBack)
        );
        sequence.AppendInterval((gameSetup.countdownDuration/2f) - animTime);

        // SET animation
        // -------------
        sequence.AppendCallback(() =>
        {
            messageUI.text = "SET";
            messageUI.transform.localScale = Vector3.zero;
        });
        sequence.Append(
            messageUI.transform.DORotate(new Vector3(0f, 0f, 360f), animTime,
            RotateMode.FastBeyond360).SetEase(Ease.OutCubic)
        );
        sequence.Join(
            messageUI.transform.DOScale(Vector3.one, animTime).SetEase(Ease.OutBack)
        );
        sequence.AppendInterval((gameSetup.countdownDuration / 2f) - animTime);

        // GO! animation
        // -------------
        float fontSizeIncrease = 150f;
        sequence.AppendCallback(() =>
        {
            messageUI.fontSize += fontSizeIncrease;
            messageUI.text = "GO!";
        });
        sequence.Append(
            messageUI.transform.DORotate(new Vector3(0f, 0f, 375f), animTime/2f,
            RotateMode.FastBeyond360).SetEase(Ease.OutCubic)
        );
        for (int i = 0; i < 6; i++)
        {
            sequence.Append(
                messageUI.transform.DOScale(Vector3.one * 1.1f, 0.05f).SetEase(Ease.OutQuad));
            sequence.Append(
                messageUI.transform.DOScale(Vector3.one, 0.05f).SetEase(Ease.InOutQuad));
        }
        sequence.AppendInterval(animTime);
        sequence.Append(
            messageUI.transform.DOScale(Vector3.zero, animTime).SetEase(Ease.InQuad)
        );

        // Complete DOTween
        sequence.OnComplete(() =>
        {
            messageUI.text = string.Empty;
            messageUI.fontSize -= fontSizeIncrease;
            messageUI.transform.localScale = Vector3.one;
            messageUI.transform.localEulerAngles = Vector3.zero;
        });
    }
    #endregion

    #region End Game Sequence
    public void EndGame()
    {      
        StartCoroutine(EndGameCoroutine());
    }
    IEnumerator EndGameCoroutine()
    {
        // Stop Minigame
        // -------------
        gamePlaying = false;
        audioPlayer.PlayFinishSound();
        audioPlayer.FadeOutMusic(0.2f);

        // Display "Finish!" Message
        // -------------------------
        EndGameFinishDOTween();

        // Transition to Results Cutscene
        // ------------------------------
        yield return new WaitForSeconds(4f);
        transitionManager.PlayTransition();
        yield return new WaitForSeconds(transitionManager.midpoint);
        gameUIScreen.SetActive(false);
        gameCamera.SetActive(false);
        resultsCutscene.SetActive(true);

        // Play Results Cutscene
        // ---------------------
        foreach (GameObject playerObject in gameSetup.playersInGame) {playerObject.SetActive(false);}
        if (resultsCutscene.GetComponent<PlayableDirector>().playableAsset != null)
        {
            PlayableDirector playableDirector = resultsCutscene.GetComponent<PlayableDirector>();
            playableDirector.Play();
            yield return new WaitUntil(() => playableDirector.state != PlayState.Playing);
            yield return new WaitForSeconds(1f);
        }
        else yield return new WaitForSeconds(transitionManager.midpoint + 1f);

        // Show Results then Exit Game
        // ---------------------------
        resultsScreen.SetActive(true);
        gameResults.GetWinners();
        gameResults.PlayResultsSequence();
        yield return new WaitForSeconds((transitionManager.midpoint*2f) + 3f);
        transitionManager.ExitMinigame();
    }
    private void EndGameFinishDOTween()
    {
        Sequence sequence = DOTween.Sequence();
        float animTime = 0.3f;
        float endTime = 1.5f;
        float fontSizeIncrease = 100f;

        // Show "FINISH!"
        // --------------
        sequence.AppendCallback(() =>
        {
            messageUI.text = "FINISH!";
            messageUI.fontSize += fontSizeIncrease;
            messageUI.transform.localScale = Vector3.zero;
        });
        sequence.Append(
            messageUI.transform.DORotate(new Vector3(0f, 0f, 365f), animTime / 2f,
            RotateMode.FastBeyond360).SetEase(Ease.OutCubic)
        );
        for (int i = 0; i < 8; i++)
        {
            sequence.Append(
                messageUI.transform.DOScale(Vector3.one * 1.1f, 0.05f).SetEase(Ease.OutQuad));
            sequence.Append(
                messageUI.transform.DOScale(Vector3.one, 0.05f).SetEase(Ease.InOutQuad));
        }
        sequence.AppendInterval(endTime);

        // Hide "FINISH!"
        // --------------
        sequence.Append(
            messageUI.transform.DOScale(Vector3.zero, animTime).SetEase(Ease.InQuad)
        );
        sequence.OnComplete(() =>
        {
            messageUI.text = string.Empty;
            messageUI.fontSize -= fontSizeIncrease;
            messageUI.transform.localScale = Vector3.one;
            messageUI.transform.localEulerAngles = Vector3.zero;
        });
    }
    #endregion
}