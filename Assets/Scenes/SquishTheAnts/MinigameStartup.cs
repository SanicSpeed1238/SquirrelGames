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
        playerController.gameObject.GetComponent<HammerController>().ConfigurePlayer(playerController.playerIndex + 1);
        playerController.gameObject.transform.position = spawnPositions[playerController.playerIndex].position;
        playerController.gameObject.GetComponentInChildren<Outline>().OutlineColor = playerColors[playerController.playerIndex];

        string playerTextObject = "Player" + (playerController.playerIndex + 1).ToString();
        joinedPlayers.transform.Find(playerTextObject).GetComponentInChildren<TextMeshProUGUI>().text = ("READY");
        if (playerController.playerIndex == 1) startText.SetActive(true);
    }

    public void StartGame()
    {
        playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        startScreen.SetActive(false);
    }
}