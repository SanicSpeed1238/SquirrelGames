using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using TMPro;

public class AntSmash_GameManager : MonoBehaviour
{
    [Header("Gameplay Attributes")]
    public float startupTime;
    public float gameLength;

    [Header("Important Components")]
    public TextMeshProUGUI timeUI;
    public TextMeshProUGUI messageUI;
    public GameObject scoresUI;

    [Header("Opening Cutscene Components")]
    public PlayableDirector playableDirector;
    public GameObject startScreen;
    public GameObject transitionScreen;
    public GameObject gameUIScreen;

    // Other Variables Needed
    [HideInInspector] public bool gameStarted;
    [HideInInspector] public bool gamePlaying;
    private float gameTime;
    private int[] playerScores;

    private void Awake()
    {      
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
        transitionScreen.GetComponentInChildren<Animator>().SetTrigger("Start");
        gameUIScreen.SetActive(false);
        gameStarted = true;
    
        yield return new WaitForSeconds(1.0f);
        startScreen.SetActive(false);

        yield return new WaitForSeconds(1.5f);
        playableDirector.Play();

        yield return new WaitUntil(() => playableDirector.state != PlayState.Playing);
        yield return new WaitForSeconds(0.5f);
        gameUIScreen.SetActive(true);

        yield return new WaitForSeconds(1f);
        messageUI.text = ("READY...");

        yield return new WaitForSeconds(startupTime);
        messageUI.text = ("GO !!!");
        messageUI.fontSize += 50;
        gamePlaying = true;

        yield return new WaitForSeconds(1f);
        messageUI.text = string.Empty;
        messageUI.fontSize -= 50;
    }
    IEnumerator EndGame()
    {
        gamePlaying = false;
        messageUI.text = ("FINISH!");

        yield return new WaitForSeconds(2f);
        messageUI.text = string.Empty;
        timeUI.text = string.Empty;

        yield return new WaitForSeconds(2f);
        GetWinner();
    }
}