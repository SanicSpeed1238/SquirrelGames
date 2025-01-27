using UnityEngine;
using System;
using TMPro;

public class TreeClimb_GameManager : Base_GameManager
{
    public static TreeClimb_GameManager Instance;

    [Header("Gameplay Attributes")]
    [Range(1f,3f)]
    public float gameMoveSpeed;
    public Transform[] treePositions;
    public GameObject[] treeSequence;

    // Variables Needed
    [HideInInspector] public bool objectsSpawning;
    [HideInInspector] public bool treesMoving;
    [HideInInspector] public bool finalTimerStarted;
    [HideInInspector] public int treeLanesRemoved;

    private bool[] playersFinished;
    private int[] playerScores;
    private int nextPlayerPlacement;
    private float finalTimer;

    private int currentSequenceIndex;
    private readonly int[] treeStartingIndex = { 1, 2, 4, 5 };

    private void Awake()
    {
        Base_GameManager_Awake();
        Instance = this;       
    }
    private void Start()
    {
        currentSequenceIndex = -1;
        treeLanesRemoved = 0;

        objectsSpawning = true;
        treesMoving = true;
        finalTimerStarted = false;

        playerScores = new int[4];
        playersFinished = new bool[4];

        nextPlayerPlacement = 1;
        finalTimer = 5f;
    }

    private void Update()
    {
        if (gamePlaying)
        {
            UpdateFinalTimer();
        }
    }
    private void UpdateFinalTimer()
    {
        if (finalTimerStarted)
        {
            if (finalTimer > 0)
            {
                finalTimer -= Time.deltaTime;
            }
            else
            {
                foreach (GameObject player in TreeClimb_MinigameSetup.Instance.playersInGame)
                {
                    player.GetComponent<TreeClimb_PlayerController>().EliminatePlayer();
                }
            }
        }
    }

    public int GetStartingIndex(int playerID)
    {
        return treeStartingIndex[playerID - 1];
    }

    public int GetRandomTreeLane()
    {
        int startingIndex = treeLanesRemoved;
        int endingIndex = treePositions.Length - treeLanesRemoved;

        int randomIndex = UnityEngine.Random.Range(startingIndex, endingIndex);
        return randomIndex;
    }

    public GameObject GetNextTreeGroup()
    {
        if ((currentSequenceIndex + 1) < treeSequence.Length)
        {
            currentSequenceIndex++;              
            return treeSequence[currentSequenceIndex];
        }
        else return null;
    }

    public void UpdateScore(int playerID, int newScore, int addedScore)
    {
        string playerTextObject = "Player" + playerID;
        GameObject playerScoreUI = scoresUI.transform.Find(playerTextObject).gameObject;

        if (newScore > 0)
        {
            if (addedScore > 0)
            {
                playerScores[playerID - 1] = newScore;
                playerScoreUI.GetComponentInChildren<TextMeshProUGUI>().text = newScore.ToString("D2");

                playerScoreUI.GetComponentInChildren<Animator>().ResetTrigger("Add");
                playerScoreUI.GetComponentInChildren<Animator>().SetTrigger("Add");
            }          
        }
        else
        {
            playerScores[playerID - 1] = newScore;
            playerScoreUI.GetComponentInChildren<TextMeshProUGUI>().text = "OUT";

            UpdatePlayersFinished(playerID);
        }     
    }
    public void UpdatePlayersFinished(int playerID)
    {
        playersFinished[playerID - 1] = true;

        foreach (bool playerFinished in playersFinished)
        {
            if (!playerFinished) return;
        }

        GetWinners();
        EndGame();
    }
    public int GainBonusPoints()
    {
        var bonusPoints = nextPlayerPlacement switch
        {
            1 => 5,
            2 => 3,
            3 => 1,
            _ => 0,
        };
        nextPlayerPlacement++;
        return bonusPoints;
    }

    private void GetWinners()
    {
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
                minigameResults.playersWon[i] = true;
            }
        }
    }
}