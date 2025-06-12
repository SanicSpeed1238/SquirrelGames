using UnityEngine;
using System;

public class TreeClimb_GameManager : MonoBehaviour
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

    private int nextPlayerPlacement;
    private float finalTimer;

    private int currentSequenceIndex;
    private readonly int[] treeStartingIndex = { 1, 2, 4, 5 };

    private void Awake()
    {
        Instance = this;       
    }
    private void Start()
    {
        currentSequenceIndex = -1;
        treeLanesRemoved = 0;

        objectsSpawning = true;
        treesMoving = true;
        finalTimerStarted = false;

        nextPlayerPlacement = 1;
        finalTimer = 5f;
    }

    private void Update()
    {
        if (MG_GameManager.Instance.gamePlaying)
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
                foreach (GameObject player in MG_GameManager.Instance.GetAllPlayers())
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
}