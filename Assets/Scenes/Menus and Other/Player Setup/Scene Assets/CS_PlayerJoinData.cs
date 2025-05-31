using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerJoinData : MonoBehaviour
{
    public static CS_PlayerJoinData Instance;

    [HideInInspector] public List<int> joinedPlayers = new();
    [HideInInspector] public List<InputDevice> inputDevices = new();

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