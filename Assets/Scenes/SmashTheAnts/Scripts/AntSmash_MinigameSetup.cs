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
            ConfigurePlayer((playerID + 1), playerColors[playerID], spawnPositions[playerID]);
    }
}