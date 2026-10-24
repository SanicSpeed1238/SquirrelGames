using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using PL.Transitions;

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

public class PlayerSetupManager : MonoBehaviour
{
    [Header("Player Positions")]
    public Transform[] playerPositions;

    [Header("Join Messages")]
    public GameObject[] joinMessages;

    [Header("Customization Menus")]
    public GameObject[] customizationMenus;

    [Header("Other Components")]
    public TransitionEffect transitionAnimation;
    public string mainMenuScene = "Main Menu";
    public string gameSelectScene = "Minigame Selection";

    // Other Variables Needed
    [HideInInspector] public bool[] playersReady = new bool[4];
    [HideInInspector] public bool transitioningScene = false;

    public void OnPlayerJoined(PlayerInput joinedPlayer)
    {
        int playerIndex = joinedPlayer.playerIndex;

        joinMessages[playerIndex].SetActive(false);
        customizationMenus[playerIndex].SetActive(true);

        joinedPlayer.gameObject.GetComponent<CustomizerController>().
            SpawnNewPlayer(playerIndex + 1, playerPositions[playerIndex], customizationMenus[playerIndex], this);

        PlayerJoinData.Instance.RegisterPlayer(joinedPlayer);
    }

    public void UpdatePlayerReady(int playerID, bool ready)
    {
        playersReady[playerID - 1] = ready;

        for (int i = 0; i < PlayerJoinData.Instance.joinedPlayers.Count; i++)
        {
            if (!playersReady[i]) return;
        }
        PlayerJoinData.Instance.SetComputerCustomization();

        // After all players are verified to be ready
        LoadGameSelect();
    }

    public string GetDefaultName()
    {
        return CustomizationList.defaultPlayerNames
            [Random.Range(0, CustomizationList.defaultPlayerNames.Length)];
    }
    
    public void LoadGameSelect()
    {
        StartCoroutine(LoadGameSelectCoroutine());
    }
    IEnumerator LoadGameSelectCoroutine()
    {
        if (transitioningScene) yield break;
        transitioningScene = true;

        yield return new WaitForSeconds(3f);
        SceneTransitioner.Instance.LoadScene(gameSelectScene, transitionAnimation);
    }
    public void LoadMainMenu()
    {
        StartCoroutine(LoadMainMenuCoroutine());
    }
    IEnumerator LoadMainMenuCoroutine()
    {
        if (transitioningScene) yield break;
        transitioningScene = true;

        yield return new WaitForEndOfFrame();
        SceneTransitioner.Instance.LoadScene(mainMenuScene, transitionAnimation);
    }
}