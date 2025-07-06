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

    [Header("Other Components")]
    public TransitionSettings transitionAnimation;

    // Other Variables Needed
    private readonly string mainMenuSceneName = "Main Menu";
    private readonly string minigameSelectSceneName = "Minigame Selection";

    private void Update()
    {
        /*if (Input.GetButtonDown("Start"))
        {
            TransitionManager.Instance().Transition(minigameSelectSceneName, transitionAnimation, 0);
        }

        if (Input.GetButtonDown("Cancel"))
        {
            TransitionManager.Instance().Transition(mainMenuSceneName, transitionAnimation, 0);
        }*/
    }

    public void OnPlayerJoined(PlayerInput joinedPlayer)
    {


        int playerIndex = joinedPlayer.playerIndex;

        joinMessages[playerIndex].SetActive(false);
        customizationMenus[playerIndex].SetActive(true);

        joinedPlayer.gameObject.GetComponent<CS_PlayerCustomizer>().
            SpawnNewPlayer(playerIndex + 1, playerPositions[playerIndex], customizationMenus[playerIndex]);

        

        CS_PlayerJoinData.Instance.RegisterPlayer(joinedPlayer);
    }
}