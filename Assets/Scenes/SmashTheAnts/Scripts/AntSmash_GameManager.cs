using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using TMPro;

public class AntSmash_GameManager : MonoBehaviour
{
    [Header("Gameplay Attributes")]
    public float gameLength;
    [HideInInspector] public bool gameStarted;
    [HideInInspector] public bool gamePlaying;

    [Header("Game UI Components")]
    public TextMeshProUGUI timeUI;
    public TextMeshProUGUI messageUI;
    public GameObject scoresUI;
    private float gameTime;
    private int[] playerScores;

    [Header("UI Components")]
    public GameObject startScreen;
    public GameObject transitionScreen;
    public GameObject gameUIScreen;

    [Header("Cutscene Components")]
    public GameObject openingCutscene;
    public GameObject gameCamera;    
    private PlayableDirector playableDirector;

    // UI Animation Variables
    private readonly float countdownDelay = 2.0f;
    private readonly float readyAppearSpeed = 1.0f;
    private readonly float readyDisplayDuration = 1.5f;
    private readonly float goDisplayDuration = 1.5f;
    private readonly float goDisappearSpeed = 0.2f;
    private readonly float finishAppearSpeed = 1.0f;
    private readonly float finishDisplayDuration = 2.0f;
    private readonly float finishDisappearSpeed = 0.2f;
    private readonly float endingDelay = 2.0f;
    private readonly float fontSizeIncrease = 100f;

    private void Awake()
    {      
        gameCamera.SetActive(false);
        openingCutscene.SetActive(true);
        playableDirector = openingCutscene.GetComponentInChildren<PlayableDirector>();

        startScreen.SetActive(true);
        gameUIScreen.SetActive(false);

        timeUI.text = string.Empty;
        messageUI.text = string.Empty;

        gameTime = gameLength;
        playerScores = new int[4];
    }

    private void Update()
    {
        if(gamePlaying) UpdateGameTime();
    }

    private void UpdateGameTime()
    {
        gameTime -= Time.deltaTime;

        if (Mathf.Round(gameTime) == 0)
        {
            gameTime = 0.0f;
            StartCoroutine(EndGame());
        }

        timeUI.text = gameTime.ToString("F0");
    }

    public void UpdateScore(int playerID, int newScore, int addedScore)
    {
        string playerTextObject = "Player" + playerID;
        GameObject playerScoreUI = scoresUI.transform.Find(playerTextObject).gameObject;

        playerScores[playerID - 1] = newScore;
        playerScoreUI.GetComponentInChildren<TextMeshProUGUI>().text = newScore.ToString("D2");

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

    private void GetWinner()
    {
        int highestScore = playerScores[0];
        int winningPlayer = 1;

        for (int i = 1; i < playerScores.Length; i++)
        {
            if (playerScores[i] > highestScore)
            {
                highestScore = playerScores[i];
                winningPlayer = i+1;
            }
        }
     
        messageUI.text = ("PLAYER " + winningPlayer + " WINS!");
    }
    
    public void StartMinigame()
    {
        StartCoroutine(StartGame());
    }
    IEnumerator StartGame()
    {
        // Transitioning to the Opening Cutscene
        gameStarted = true;
        transitionScreen.GetComponentInChildren<Animator>().SetTrigger("Transition");        
        yield return new WaitForSeconds(1.5f);
        gameUIScreen.SetActive(false);
        startScreen.SetActive(false);
        playableDirector.Play();

        // Transitioning to the Game Camera
        yield return new WaitUntil(() => playableDirector.state != PlayState.Playing);
        transitionScreen.GetComponentInChildren<Animator>().SetTrigger("Transition");
        yield return new WaitForSeconds(1.0f);
        playableDirector.gameObject.SetActive(false);
        openingCutscene.SetActive(false);
        gameCamera.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        gameUIScreen.SetActive(true);

        // Countdown to Start Gameplay

        yield return new WaitForSeconds(countdownDelay);
        messageUI.gameObject.GetComponent<RectTransform>().localScale = Vector3.zero;
        LeanTween.scale(messageUI.gameObject.GetComponent<RectTransform>(), Vector3.one, readyAppearSpeed).setEase(LeanTweenType.easeOutExpo);
        messageUI.text = ("READY...");

        yield return new WaitForSeconds(readyDisplayDuration);
        messageUI.fontSize += fontSizeIncrease;
        messageUI.text = ("GO !!!");       
        LeanTween.scale(messageUI.gameObject.GetComponent<RectTransform>(), Vector3.zero, goDisappearSpeed).setDelay(goDisplayDuration).setEase(LeanTweenType.linear);

        // Finish Up StartGame Coroutine
        gamePlaying = true;
        yield return new WaitForSeconds(goDisplayDuration + goDisappearSpeed);
        messageUI.text = string.Empty;
        messageUI.fontSize -= fontSizeIncrease;
        messageUI.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    IEnumerator EndGame()
    {
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
        GetWinner();
    }
}