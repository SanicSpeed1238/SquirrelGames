using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CS_PlayerJoinData : MonoBehaviour
{
    public static CS_PlayerJoinData Instance;

    [HideInInspector] public List<int> joinedPlayers = new();
    [HideInInspector] public List<InputDevice> inputDevices = new();

    public struct PlayerCustomization
    {
        public string playerName;
        public int hatOption;
        public int bodyOption;
    }
    [HideInInspector] public PlayerCustomization[] 
        playerCustomizations = new PlayerCustomization[4];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPlayer(PlayerInput joinedPlayer)
    {
        joinedPlayers.Add(joinedPlayer.playerIndex+1);
        inputDevices.Add(joinedPlayer.devices[0]);
    }
}