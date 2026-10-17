using UnityEngine;
using TMPro;
using EZ.Transitions;

public class MG_GameResults : MonoBehaviour
{
    public enum ScoringMethod
    {
        Points,
        Goal,
    }
    public enum ScoresUILayout
    {
        Bottom,
        Corners,
    }
    [Header("Scoring Components")]
    public ScoringMethod scoringMethod = ScoringMethod.Points;
    public ScoresUILayout scoresUILayout = ScoresUILayout.Bottom;

    [Header("Results Sequence Components")]
    public TextMeshProUGUI winnersText;
    public AudioSource resultsMusic;
    public TransitionEffect exitTransition;

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
        // Check Player Scores
        // -------------------
        int highestScore = 1;
        for (int i = 0; i < playerScores.Length; i++)
        {
            if (playerScores[i] > highestScore)
                highestScore = playerScores[i];
        }
        for (int i = 0; i < playerScores.Length; i++)
        {
            if (playerScores[i] == highestScore)
                playersWon[i] = true;
        }

        // Set Players Won Text
        // --------------------
        string playersWonText = "";
        int playersWonNum = 0;
        for (int p = 0;  p < playersWon.Length; p++)
        {
            if (playersWon[p] == true)
            {
                string player;

                if (CS_PlayerJoinData.Instance != null)
                    player = CS_PlayerJoinData.Instance.playerCustomizations[p].playerName + " ";
                else
                    player = "P" + (p+1) + " ";

                playersWonText += $"<size=110%>{player}</size> ";
                playersWonNum++;
            }
        }
        if (playersWonNum > 1) playersWonText += "\nWIN!";
        else if (playersWonNum == 1) playersWonText += "\nWINS!";
        else playersWonText = "TIE...";
        winnersText.text = playersWonText;
    }

    public void PlayResultsSequence()
    {
        winnersText.gameObject.SetActive(true);
        for (int i = 0; i < animatedSquirrels.Length; i++)
        {
            if (playersWon[i])
                animatedSquirrels[i].GetComponentInChildren<Animator>().SetTrigger("Win");
            else
                animatedSquirrels[i].GetComponentInChildren<Animator>().SetTrigger("Lose");
        }

        resultsMusic.Play();
        SceneTransitioner.Instance.LoadScene(minigameSelectSceneName, resultsMusic.clip.length, exitTransition);
    }
}