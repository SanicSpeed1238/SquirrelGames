using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MinigameStartup : MonoBehaviour
{
    [Header("Start Screen Components")]
    public GameObject startScreen;
    public GameObject startText;
    public GameObject joinedPlayers;

    [Header("Player Spawn Positions")]
    public Transform[] spawnPositions;

    [Header("Player Colors")]
    public Color[] playerColors;

    // Other Variables Needed
    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        playerInputManager = GetComponentInParent<PlayerInputManager>();
        if (playerInputManager == null) Debug.LogError("Input Manager Not Found");
    }

    public void OnPlayerJoined(PlayerInput playerController)
    {
        int playerID = playerController.playerIndex;

        // Initializes new hammer gameobject according to Player ID number
        playerController.gameObject.GetComponent<HammerController>().
            ConfigurePlayer((playerID + 1), playerColors[playerID], spawnPositions[playerID]);

        // Updates the text on the startup screen
        string playerTextObject = "Player" + (playerID + 1).ToString();
        joinedPlayers.transform.Find(playerTextObject).GetComponentInChildren<TextMeshProUGUI>().text = ("READY");
        if (playerID == 1) startText.SetActive(true);
    }

    public void StartGame() // Disables joining and hides the startup screen when player pushes Start
    {
        playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        startScreen.SetActive(false);
    }
}