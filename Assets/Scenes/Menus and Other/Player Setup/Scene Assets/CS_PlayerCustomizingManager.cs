using UnityEngine;
using UnityEngine.InputSystem;
using EasyTransition;

public class CS_PlayerCustomizingManager : MonoBehaviour
{
    [Header("Player Positions")]
    public Transform[] playerPositions;

    [Header("Join Messages")]
    public GameObject[] joinMessages;

    [Header("Customization Menus")]
    public GameObject[] customizationMenus;

    [Header("Other Important References")]
    public TransitionSettings transitionAnimation;

    // Other Variables Needed
    private readonly string minigameSelectSceneName = "Minigame Selection";

    private void Update()
    {
        if (Input.GetButtonDown("Start"))
        {
            TransitionManager.Instance().Transition(minigameSelectSceneName, transitionAnimation, 0);
        }

        if (Input.GetButtonDown("Cancel"))
        {
            // Go back to Main Menu
        }
    }

    public void OnPlayerJoined(PlayerInput joinedPlayer)
    {
        int playerIndex = joinedPlayer.playerIndex;
        joinedPlayer.gameObject.GetComponent<CS_PlayerCustomizer>().
            SpawnNewPlayer(playerIndex + 1, playerPositions[playerIndex], customizationMenus[playerIndex]);

        joinMessages[playerIndex].SetActive(false);
        customizationMenus[playerIndex].SetActive(true);

        CS_PlayerJoinData.Instance.RegisterPlayer(joinedPlayer);
    }
}