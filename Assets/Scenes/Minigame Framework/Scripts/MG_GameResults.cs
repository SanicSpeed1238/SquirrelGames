using UnityEngine;
using TMPro;
using EasyTransition;

public class MG_GameResults : MonoBehaviour
{
    public enum ScoringMethod
    {
        Points,
        Goal,
    }
    [Header("Scoring Type")]
    public ScoringMethod scoringMethod = ScoringMethod.Points;

    [Header("Results Sequence Components")]
    public TextMeshProUGUI winnersText;
    public AudioSource resultsMusic;
    public TransitionSettings exitTransition;

    [Header("Animated Squirrels Reference")]
    public GameObject[] animatedSquirrels;

    // Variables for Scoring
    [HideInInspector] public int[] playerScores = new int[4];
    [HideInInspector] public bool[] playersFinished = new bool[4];
    [HideInInspector] public bool[] playersWon = new bool[4]; 

    // Other Variables Needed
    private readonly string minigameSelectSceneName = "Minigame Selection";
    private readonly string[] playerPlacements = new string[] { "1st", "2nd", "3rd", "4th" };
    private int playerPlacementNext = 0;

    public string GetNextPlacement()
    {
        return playerPlacements[playerPlacementNext++];
    }

    public void GetWinners()
    {
        #region Calculate Winners

        int highestScore = 1;

        for (int i = 0; i < playerScores.Length; i++)
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

        #endregion

        #region Display Winners

        string playersWonText = "";
        int playersWonNum = 0;

        for (int p = 0;  p < playersWon.Length; p++)
        {
            if (playersWon[p] == true)
            {
                if (CS_PlayerJoinData.Instance != null)
                {
                    playersWonText += CS_PlayerJoinData.Instance.playerCustomizations[p].playerName + " ";
                }
                else
                {
                    playersWonText += "P" + (p+1) + " ";
                }  
                playersWonNum++;
            }
        }

        if (playersWonNum > 1) playersWonText += "WIN!";
        else if (playersWonNum == 1) playersWonText += "WINS!";
        else playersWonText = "TIE...";

        winnersText.text = playersWonText;

        #endregion
    }

    public void PlayResultsSequence()
    {
        resultsMusic.Play();

        for (int i = 0; i < animatedSquirrels.Length; i++)
        {
            if (playersWon[i])
            {
                animatedSquirrels[i].GetComponentInChildren<Animator>().SetTrigger("Win");
            }
            else
            {
                animatedSquirrels[i].GetComponentInChildren<Animator>().SetTrigger("Lose");
            }
        }

        TransitionManager.Instance().Transition(minigameSelectSceneName, exitTransition, resultsMusic.clip.length);
    }
}