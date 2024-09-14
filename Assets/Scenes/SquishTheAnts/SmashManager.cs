using UnityEngine;
using System.Collections;
using TMPro;

public class SmashManager : MonoBehaviour
{
    [Header("Gameplay Attributes")]
    public float startupTime;
    public float gameLength;

    [Header("Important Components")]
    public TextMeshProUGUI timeUI;
    public TextMeshProUGUI messageUI;
    public GameObject scoresUI;

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

    private void Start()
    {
        StartCoroutine(StartGame());
    }

    private void Update()
    {
        if(gamePlaying) UpdateGameTime();
    }

    public void UpdateScore(int playerID, int score)
    {
        playerScores[playerID-1] = score;

        string playerTextObject = "Player" + playerID;
        scoresUI.transform.Find(playerTextObject).GetComponentInChildren<TextMeshProUGUI>().text = score.ToString("D2");
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
    
    IEnumerator StartGame()
    {
        yield return new WaitUntil(() => gameStarted);

        yield return new WaitForSeconds(1f);
        messageUI.text = ("READY...");

        yield return new WaitForSeconds(startupTime);
        messageUI.text = ("GO !!!");
        gamePlaying = true;

        yield return new WaitForSeconds(1f);
        messageUI.text = string.Empty;
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