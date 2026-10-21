using UnityEngine;
using UnityEngine.InputSystem;
//using EasyTransition;

public static class CustomizationList
{
    public static int numHatOptions = 8;
    public static int numBodyOptions = 6;

    public static string[] defaultPlayerNames = 
    { 
      "Alan",
      "Jeremy",
      "Nayuki",
      "Jacob",
      "Chase",
      "Nutty",
      "Waffle",     
      "Sammy",
      "Thomas",
      "Gerald",
      "Lucky",
      "Bonker",
      "Ray",
    };
}

public class CS_PlayerCustomizingManager : MonoBehaviour
{
    [Header("Player Positions")]
    public Transform[] playerPositions;

    [Header("Join Messages")]
    public GameObject[] joinMessages;

    [Header("Customization Menus")]
    public GameObject[] customizationMenus;

    [Header("Other Components")]
    //public TransitionSettings transitionAnimation;

    // Other Variables Needed
    [HideInInspector] public bool[] playersReady = new bool[4];
    [HideInInspector] public bool transitioningScene = false;
    private float backButtonTimer = 0f;
    private readonly string mainMenuSceneName = "Main Menu";
    private readonly string minigameSelectSceneName = "Minigame Selection";

    public void OnPlayerJoined(PlayerInput joinedPlayer)
    {
        int playerIndex = joinedPlayer.playerIndex;

        joinMessages[playerIndex].SetActive(false);
        customizationMenus[playerIndex].SetActive(true);

        joinedPlayer.gameObject.GetComponent<CS_PlayerCustomizer>().
            SpawnNewPlayer(playerIndex + 1, playerPositions[playerIndex], customizationMenus[playerIndex], this);

        CS_PlayerJoinData.Instance.RegisterPlayer(joinedPlayer);
    }

    public void UpdatePlayerReady(int playerID, bool ready)
    {
        playersReady[playerID - 1] = ready;

        for (int i = 0; i < CS_PlayerJoinData.Instance.joinedPlayers.Count; i++)
        {
            if (!playersReady[i]) return;
        }
        CS_PlayerJoinData.Instance.SetComputerCustomization();

        transitioningScene = true;
        //TransitionManager.Instance().Transition(minigameSelectSceneName, transitionAnimation, 3f);
    }

    private void Update()
    {
        if (Input.GetButton("Cancel") && !transitioningScene)
        {
            backButtonTimer += Time.deltaTime;
            if (backButtonTimer >= 2f)
            {
                transitioningScene = true;
                //TransitionManager.Instance().Transition(mainMenuSceneName, transitionAnimation, 0f);
            }
        }
        else backButtonTimer = 0f;
    }
}