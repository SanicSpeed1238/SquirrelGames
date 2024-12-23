using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using TMPro;
using System;

public class TreeClimb_GameManager : Base_GameManager
{
    [Header("Game UI Components")]
    public TextMeshProUGUI messageUI;
    public GameObject scoresUI;
    private int[] playerScores;

    [Header("Gameplay Attributes")]
    public Transform[] treePositions;
    public GameObject[] treeSequence;
    private int currentSequenceIndex;
    private readonly int[] treeStartingIndex = {1, 2, 4, 5};

    private void Awake()
    {
        //Base_GameManager_Awake();
        currentSequenceIndex = -1;
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

    public void RemoveTreeLanes()
    {
        if (treePositions.Length <= 3) return;

        Transform[] newTreePositions = new Transform[treePositions.Length - 2];
        Array.Copy(treePositions, 1, newTreePositions, 0, newTreePositions.Length);
        treePositions = newTreePositions;
    }

    public int GetStartingIndex(int playerID)
    {
        return treeStartingIndex[playerID - 1];
    }
}