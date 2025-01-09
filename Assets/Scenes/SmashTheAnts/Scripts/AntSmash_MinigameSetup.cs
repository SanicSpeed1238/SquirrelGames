using UnityEngine;
using UnityEngine.InputSystem;

public class AntSmash_MinigameSetup : Base_MinigameSetup
{
    public static AntSmash_MinigameSetup Instance;

    private void Awake()
    {
        Base_MinigameSetup_Awake();
        Instance = this;
    }

    protected override void SetUpPlayer(PlayerInput playerController, int playerID)
    {
        playerController.gameObject.GetComponent<AntSmash_HammerController>().
            InitializePlayer(playerID, playerColors[playerID - 1], spawnPositions[playerID - 1]);
    }

    protected override void SetUpComputer(GameObject computerController, int playerID)
    {
        computerController.GetComponent<AntSmash_HammerController>().
            InitializePlayer(playerID, playerColors[playerID - 1], spawnPositions[playerID - 1]);
    }
}