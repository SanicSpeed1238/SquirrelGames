using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SmashManager : MonoBehaviour
{
    [Header("Gameplay Attributes")]
    public float gameLength;

    [Header("Important Components")]
    public TextMeshProUGUI timeUI;
    public GameObject scoresUI;

    // Other Variables Needed
    private PlayerInputManager playerInputManager;
    private float gameTime;

    private void Awake()
    {
        playerInputManager = GetComponentInParent<PlayerInputManager>();
        if (playerInputManager == null) Debug.LogError("Input Manager Not Detected.");
    }

    private void Start()
    {
        gameTime = gameLength;
    }

    private void Update()
    {
        UpdateGameTime();
    }

    public void UpdateScore(int playerID, int score)
    {
        string playerTextObject = "Player" + playerID;
        scoresUI.transform.Find(playerTextObject).GetComponent<TextMeshProUGUI>().text = score.ToString("D2");
    }

    private void UpdateGameTime()
    {
        gameTime -= Time.deltaTime;
        timeUI.text = gameTime.ToString("F0");
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject player = playerInput.gameObject;
        player.GetComponent<HammerController>().playerID = playerInput.playerIndex + 1;
    }   
}