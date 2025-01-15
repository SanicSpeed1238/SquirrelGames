using UnityEngine;
using UnityEngine.InputSystem;

public class TreeClimb_MinigameSetup : Base_MinigameSetup
{
    public static TreeClimb_MinigameSetup Instance;

    private void Awake()
    {
        Base_MinigameSetup_Awake();
        Instance = this;
    }

    protected override void SetUpPlayer(PlayerInput playerController, int playerID)
    {
        playerController.gameObject.GetComponent<TreeClimb_PlayerController>().
            InitializePlayer(playerID, spawnPositions[playerID - 1]);
    }

    protected override void SetUpComputer(GameObject computerController, int playerID)
    {
        computerController.GetComponent<TreeClimb_PlayerController>().
            InitializePlayer(playerID, spawnPositions[playerID - 1]);
    }
}