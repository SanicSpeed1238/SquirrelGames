using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Base_MinigameSetup : MonoBehaviour
{
    [Header("Start Screen Components")]   
    public GameObject joinedPlayers;
    public GameObject startText;

    [Header("Computer Player")]
    public GameObject computerPlayer;

    [Header("Player Colors")]
    public Color[] playerColors;

    [Header("Player Spawn Positions")]
    public Transform[] spawnPositions;

    // Other Variables Needed
    private PlayerInputManager playerInputManager;
    private readonly float joinDelaySeconds = 3f;
    private int lastJoinedPlayer;

    protected void Base_MinigameSetup_Awake()
    {
        playerInputManager = GetComponentInParent<PlayerInputManager>();

        StartCoroutine(WaitToJoin());
    }
    IEnumerator WaitToJoin()
    {
        if (playerInputManager != null)
        {
            playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
            yield return new WaitForSeconds(joinDelaySeconds);
            playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenJoinActionIsTriggered;
        }
        else Debug.LogError("Input Manager Not Found");
    }

    public void OnPlayerJoined(PlayerInput playerController)
    {
        int playerID = playerController.playerIndex + 1;
        lastJoinedPlayer = playerID;

        // Initializes player GameObject with player ID and any other necessary attributes
        SetUpPlayer(playerController, playerID);

        // Updates the icon on the startup screen
        string playerTextObject = "Player" + playerID.ToString();
        GameObject playerIconObject = joinedPlayers.transform.Find(playerTextObject).gameObject;
        playerIconObject.transform.Find("Unready").gameObject.SetActive(false);
        playerIconObject.transform.Find("Ready").gameObject.SetActive(true);

        if (playerID == 1) startText.SetActive(true);
    }
    public void ConfirmJoin(int playerID)
    {
        string playerTextObject = "Player" + (playerID).ToString();
        GameObject playerIconObject = joinedPlayers.transform.Find(playerTextObject).gameObject;

        LeanTween.scale(playerIconObject.GetComponent<RectTransform>(), new Vector3(1.15f, 1.15f, 1.15f), 0.1f).setEase(LeanTweenType.pingPong)
            .setOnComplete(() => { playerIconObject.GetComponent<RectTransform>().localScale = Vector3.one; });
    }

    // Override this function with a "InitializePlayer" function found within the Player GameObject!
    protected virtual void SetUpPlayer(PlayerInput playerController, int playerID)
    {
        Debug.LogWarning("No PlayerSetup has been defined.");
    }
    // Override this function using the same "InitializePlayer" function found within the Player GameObject!
    protected virtual void SetUpComputer(GameObject computerController, int playerID)
    {
        Debug.LogWarning("No ComputerSetup has been defined.");
    }

    public void StartGame() // Disables joining and hides the startup screen when player pushes Start
    {
        playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;

        if (computerPlayer != null)
        {
            for (int ID = (lastJoinedPlayer + 1); ID <= 4; ID++)
            {
                GameObject newComputer = Instantiate(computerPlayer);
                SetUpComputer(newComputer, ID);
            }
        }

        GameObject.FindAnyObjectByType<Base_GameManager>().StartGame();
    }
}