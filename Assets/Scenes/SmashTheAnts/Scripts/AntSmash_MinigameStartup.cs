using UnityEngine;
using UnityEngine.InputSystem;

public class AntSmash_MinigameStartup : MonoBehaviour
{
    [Header("Start Screen Components")]
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
        playerController.gameObject.GetComponent<AntSmash_HammerController>().
            ConfigurePlayer((playerID + 1), playerColors[playerID], spawnPositions[playerID]);

        // Updates the icon on the startup screen
        string playerTextObject = "Player" + (playerID + 1).ToString();
        GameObject playerIconObject = joinedPlayers.transform.Find(playerTextObject).gameObject;

        playerIconObject.transform.Find("Unready").gameObject.SetActive(false);
        playerIconObject.transform.Find("Ready").gameObject.SetActive(true);

        if ((playerID == 1) || (playerID == 0 && Application.isEditor)) startText.SetActive(true);
    }

    public void StartGame() // Disables joining and hides the startup screen when player pushes Start
    {
        playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        GameObject.FindAnyObjectByType<AntSmash_GameManager>().StartMinigame();
    }
}