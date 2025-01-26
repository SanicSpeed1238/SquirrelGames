using UnityEngine;
using TMPro;
using EasyTransition;

public class CS_MinigameResults : MonoBehaviour
{
    [HideInInspector] public bool[] playersWon = new bool[4];

    public TextMeshProUGUI winnersText;
    public AudioSource resultsMusic;
    public TransitionSettings endingTransition;

    private readonly string minigameSelectSceneName = "Minigame Selection";

    public void DisplayWinnersText()
    {
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
    }

    public void PlayResultsSequence()
    {
        resultsMusic.Play();
        TransitionManager.Instance().Transition(minigameSelectSceneName, endingTransition, resultsMusic.clip.length + 1);
    }
}