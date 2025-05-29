using UnityEngine;
using TMPro;
using EasyTransition;

public class CS_GameResults : MonoBehaviour
{
    [Header("Results Sequence Components")]
    public TextMeshProUGUI winnersText;
    public AudioSource resultsMusic;
    public TransitionSettings endingTransition;

    // Variables for Scoring
    [HideInInspector] public int[] playerScores = new int[4];
    [HideInInspector] public bool[] playersFinished = new bool[4];
    [HideInInspector] public bool[] playersWon = new bool[4];

    // Other Variables Needed
    private readonly string minigameSelectSceneName = "Minigame Selection";

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

        for(int p = 0;  p < playersWon.Length; p++)
        {
            if(playersWon[p] == true)
            {
                playersWonText += "P" + (p + 1) + " ";
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
        TransitionManager.Instance().Transition(minigameSelectSceneName, endingTransition, resultsMusic.clip.length + 1);
    }
}