using UnityEngine;
using TMPro;

public class AntSmash_GameManager : Base_GameManager
{
    [Header("Gameplay Attributes")]
    [Range(30f, 60f)]
    public float gameLength;

    // Variables Needed
    private float gameTime;
    private int[] playerScores;

    private void Awake()
    {
        Base_GameManager_Awake();

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
            GetWinners();
            EndGame();
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

    private void GetWinners()
    {
        int highestScore = 1;

        for (int i = 1; i < playerScores.Length; i++)
        {
            if (playerScores[i] > highestScore)
            {
                highestScore = playerScores[i];
            }
        }

        for (int i = 0; i < playerScores.Length; i++)
        {
            if (playerScores[i] == highestScore)
            {
                playersWon[i] = true;
            }
        }
    }
}