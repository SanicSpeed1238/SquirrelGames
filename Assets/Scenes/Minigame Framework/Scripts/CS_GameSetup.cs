using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class CS_GameSetup : MonoBehaviour
{
    [Header("Player Prefabs")]
    public GameObject humanPlayer;
    public GameObject computerPlayer;

    [Header("Player Variables")]
    public Color[] playerColors;
    public Transform[] spawnPositions;

    // Other Variables Needed
    [HideInInspector] public GameObject[] playersInGame;
    private int lastJoinedPlayer;
    private bool canStart;

    private void Update()
    {
        if (Input.GetButtonDown("Start") && canStart)
        {
            if (!CS_GameManager.Instance.gameStarted) StartGame();
        }
    }

    private void Awake()
    {
        playersInGame = new GameObject[4];
        StartCoroutine(WaitToStart());
    }
    IEnumerator WaitToStart()
    {
        yield return new WaitForSeconds(3f);
        canStart = true;
    }

    private void SetUpPlayerObjects(List<int> humanPlayers)
    {
        if (humanPlayers != null && humanPlayers.Count > 0)
        {
            foreach (int playerID in humanPlayers)
            {
                GameObject newPlayer = PlayerInput.Instantiate(humanPlayer, playerID - 1, "Controller", playerID - 1).gameObject;
                SetUpPlayer(newPlayer, playerID);
                playersInGame[playerID - 1] = newPlayer;
                lastJoinedPlayer = playerID;
            }
        }
        if (computerPlayer != null)
        {
            for (int playerID = (lastJoinedPlayer + 1); playerID <= 4; playerID++)
            {
                GameObject newComputer = Instantiate(computerPlayer);
                SetUpComputer(newComputer, playerID);
                playersInGame[playerID - 1] = newComputer;
            }
        }
    }
    private void SetUpPlayer(GameObject playerController, int playerID)
    {
        if (playerController.TryGetComponent<Base_PlayerController>(out var playerObject))
        {
            playerObject.SetPlayerData(playerID, playerColors[playerID-1], spawnPositions[playerID-1]);
        }
        else Debug.LogWarning("Player GameObject does not inherit from Base_PlayerController.");
    }
    private void SetUpComputer(GameObject computerController, int playerID)
    {
        if (computerController.TryGetComponent<Base_PlayerController>(out var computerObject))
        {
            computerObject.SetPlayerData(playerID, playerColors[playerID-1], spawnPositions[playerID-1]);
        }
        else Debug.LogWarning("Computer GameObject does not inherit from Base_PlayerController.");
    }

    public void StartGame()
    {
        if (CS_PlayerJoinData.Instance != null) SetUpPlayerObjects(CS_PlayerJoinData.Instance.joinedPlayers);
        else SetUpPlayerObjects(new List<int>());

        GameObject.FindAnyObjectByType<CS_GameManager>().StartGame();
    }
}