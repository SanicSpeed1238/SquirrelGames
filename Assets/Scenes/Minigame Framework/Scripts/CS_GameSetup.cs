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

    [Header("Important References")]
    public PlayerInputManager playerInputManager;
    [HideInInspector] public GameObject[] playersInGame;

    // Other Variables Needed
    private readonly float pressStartDelay = 3f;
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
        yield return new WaitForSeconds(pressStartDelay);
        canStart = true;
    }

    private void SetUpPlayerObjects(List<PlayerInput> humanPlayers)
    {
        if (humanPlayers != null && humanPlayers.Count > 0)
        {
            foreach (PlayerInput player in humanPlayers)
            {
                int playerID = player.playerIndex + 1;
                lastJoinedPlayer = playerID;

                playersInGame[player.playerIndex] = player.gameObject;
                SetUpPlayer(player, playerID);
            }
        }
        else
        {
            if (computerPlayer != null)
            {
                for (int playerID = (lastJoinedPlayer + 1); playerID <= 4; playerID++)
                {
                    GameObject newComputer = Instantiate(computerPlayer);
                    playersInGame[playerID - 1] = newComputer;
                    SetUpComputer(newComputer, playerID);
                }
            }
        }
    }
    private void SetUpPlayer(PlayerInput playerController, int playerID)
    {
        if (humanPlayer.TryGetComponent<Base_PlayerController>(out var playerObject))
        {
            // Assign playerController to this Player Input Component
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
        SetUpPlayerObjects(new List<PlayerInput>());
        GameObject.FindAnyObjectByType<CS_GameManager>().StartGame();
    }
}