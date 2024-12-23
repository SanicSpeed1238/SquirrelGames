using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Base_MinigameSetup : MonoBehaviour
{
    [Header("Start Screen Components")]   
    public GameObject joinedPlayers;
    public GameObject startText;

    [Header("Player Colors")]
    public Color[] playerColors;

    [Header("Player Spawn Positions")]
    public Transform[] spawnPositions;

    // Other Variables Needed
    private PlayerInputManager playerInputManager;
    private readonly float joinDelaySeconds = 3f;

    private void Awake()
    {
        playerInputManager = GetComponentInParent<PlayerInputManager>();

        if (playerInputManager == null) Debug.LogError("Input Manager Not Found");
        else playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;

        StartCoroutine(WaitToJoin());
    }
    IEnumerator WaitToJoin()
    {
        yield return new WaitForSeconds(joinDelaySeconds);
        playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenJoinActionIsTriggered;
    }

    public void OnPlayerJoined(PlayerInput playerController)
    {
        int playerID = playerController.playerIndex;

        // Initializes player GameObject with player ID and any other necessary attributes
        SetUpPlayer(playerController, playerID);

        // Updates the icon on the startup screen
        string playerTextObject = "Player" + (playerID + 1).ToString();
        GameObject playerIconObject = joinedPlayers.transform.Find(playerTextObject).gameObject;

        playerIconObject.transform.Find("Unready").gameObject.SetActive(false);
        playerIconObject.transform.Find("Ready").gameObject.SetActive(true);

        if ((playerID == 1) || (playerID == 0 && Application.isEditor)) startText.SetActive(true);
    }

    // Override this function with a "ConfigurePlayer" function found within the Player GameObject!
    protected virtual void SetUpPlayer(PlayerInput playerController, int playerID)
    {
        Debug.LogWarning("No PlayerSetup has been defined.");
    }

    public void ConfirmJoin(int playerID)
    {
        string playerTextObject = "Player" + (playerID).ToString();
        GameObject playerIconObject = joinedPlayers.transform.Find(playerTextObject).gameObject;

        LeanTween.scale(playerIconObject.GetComponent<RectTransform>(), new Vector3(1.15f, 1.15f, 1.15f), 0.1f).setEase(LeanTweenType.pingPong)
            .setOnComplete(() => { playerIconObject.GetComponent<RectTransform>().localScale = Vector3.one; });
    }

    public void StartGame() // Disables joining and hides the startup screen when player pushes Start
    {
        playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        GameObject.FindAnyObjectByType<Base_GameManager>().StartGame();
    }
}