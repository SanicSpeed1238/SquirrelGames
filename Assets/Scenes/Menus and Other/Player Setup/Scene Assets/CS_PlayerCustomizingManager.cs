using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerCustomizingManager : MonoBehaviour
{
    [Header("Player Positions")]
    public Transform[] playerPositions;

    [Header("Join Messages")]
    public GameObject[] joinMessages;

    [Header("Customization Menus")]
    public GameObject[] customizationMenus;

    public void OnPlayerJoined(PlayerInput joinedPlayer)
    {
        int playerIndex = joinedPlayer.playerIndex;
        joinedPlayer.gameObject.GetComponent<CS_PlayerCustomizer>().
            SpawnNewPlayer(playerIndex + 1, playerPositions[playerIndex], customizationMenus[playerIndex]);

        joinMessages[playerIndex].SetActive(false);
        customizationMenus[playerIndex].SetActive(true);
    }
}