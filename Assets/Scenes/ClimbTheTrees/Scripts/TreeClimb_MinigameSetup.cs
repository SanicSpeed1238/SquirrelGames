using UnityEngine.InputSystem;

public class TreeClimb_MinigameSetup : Base_MinigameSetup
{
    protected override void SetUpPlayer(PlayerInput playerController, int playerID)
    {
        playerController.gameObject.GetComponent<TreeClimb_PlayerController>().
            ConfigurePlayer((playerID + 1), playerColors[playerID], spawnPositions[playerID]);
    }
}