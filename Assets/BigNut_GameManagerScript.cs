using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BigNut_GameManagerScript : MonoBehaviour
{
    public TMP_Text gameEndText;
    public GameObject gameEndUI;

    private const string GAME_END_STRING = "{0} won!!";

    private HashSet<string> players = new HashSet<string>();

    void Start()
    {
        gameEndUI.SetActive(false);
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in playerObjects)
        {
            players.Add(player.name);
        }
    }

    public void PlayerKilled(string playerName)
    {
        players.Remove(playerName);
        if (players.Count == 1)
        {
            foreach (string player in players)
            {
                GameEnd(player);

            }
        }
    }

    public void GameEnd(string winningPlayer)
    {
        Time.timeScale = 0;
        gameEndText.text = string.Format(GAME_END_STRING, winningPlayer);
        gameEndUI.SetActive(true);
    }
}
