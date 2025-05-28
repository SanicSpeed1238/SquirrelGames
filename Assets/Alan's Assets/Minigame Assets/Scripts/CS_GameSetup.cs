using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

    private void Awake()
    {
        playersInGame = new GameObject[4];

        StartCoroutine(WaitToJoin());
    }
    IEnumerator WaitToJoin()
    {
        if (playerInputManager != null)
        {
            yield return new WaitForSeconds(pressStartDelay);
        }
        else Debug.LogError("Input Manager Not Found");
    }

    private void Update()
    {
        if (Input.GetButtonDown("Start"))
        {
            if (!CS_GameManager.Instance.gameStarted) StartGame();
        }
    }

    public void OnPlayerJoined(PlayerInput playerController)
    {
        int playerID = playerController.playerIndex + 1;
        lastJoinedPlayer = playerID;

        // Initializes player GameObject with player ID and any other necessary attributes
        playersInGame[playerController.playerIndex] = playerController.gameObject;
        SetUpPlayer(playerController, playerID);

        // Updates the icon on the startup screen
        /*string playerTextObject = "Player" + playerID.ToString();
        GameObject playerIconObject = joinedPlayers.transform.Find(playerTextObject).gameObject;
        playerIconObject.transform.Find("Unready").gameObject.SetActive(false);
        playerIconObject.transform.Find("Ready").gameObject.SetActive(true);*/

        //if (playerID == 1) startText.SetActive(true);
    }
    /*public void ConfirmJoin(int playerID)
    {
        string playerTextObject = "Player" + (playerID).ToString();
        GameObject playerIconObject = joinedPlayers.transform.Find(playerTextObject).gameObject;

        LeanTween.scale(playerIconObject.GetComponent<RectTransform>(), new Vector3(1.15f, 1.15f, 1.15f), 0.1f).setEase(LeanTweenType.pingPong)
            .setOnComplete(() => { playerIconObject.GetComponent<RectTransform>().localScale = Vector3.one; });
    }*/

    // Override this function with a "InitializePlayer" function found within the Player GameObject!
    private void SetUpPlayer(PlayerInput playerController, int playerID)
    {
        Debug.LogWarning("No PlayerSetup has been defined.");
    }
    // Override this function using the same "InitializePlayer" function found within the Player GameObject!
    private void SetUpComputer(GameObject computerController, int playerID)
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
                playersInGame[ID - 1] = newComputer;
                SetUpComputer(newComputer, ID);
            }
        }

        GameObject.FindAnyObjectByType<CS_GameManager>().StartGame();
    }
}