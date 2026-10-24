using TMPro;
using UnityEngine;

public class CustomizedPlayer : MonoBehaviour
{
    public enum CustomizationReference
    {
        Controller,
        Player,
        NPC,
    }
    [Header("Reference for Customization Data")]
    public CustomizationReference customizationReference = CustomizationReference.Controller;

    [Header("Controller Settings")]
    public bool computerPlayer;

    [Header("Player Settings")]
    [Range(1, 4)]
    public int playerID = 1;

    [Header("NPC Settings")]
    public bool randomizeCustomizations;
    public int hatOption;
    public int bodyOption;

    [Header("Customization Objects")]
    public GameObject squirrelHats;
    public GameObject squirrelBodies;

    [Header("Squirrel Mesh Renders")]
    public SkinnedMeshRenderer[] squirrelBodyParts;

    // Chosen Customizations
    [HideInInspector] public string currentName = "PLAYER";
    [HideInInspector] public int currentHat = 0;
    [HideInInspector] public int currentBody = 0;

    private void OnValidate()
    {
        if (playerID < 1) playerID = 1;
    }

    private void Start()
    {
        playerID = GetComponentInParent<Base_PlayerController>().playerID;

        ApplyName();
        ApplyBody();
        ApplyHat();
    }

    private void ApplyName()
    {
        switch (customizationReference)
        {
            case CustomizationReference.Controller:
                if (ValidJoinData() && ValidPlayerController() && ValidGameManager())
                {                 
                    string playerTextObject = "Player" + playerID;
                    currentName = PlayerJoinData.Instance.playerCustomizations[playerID-1].playerName;

                    GameObject uiName = MG_GameManager.Instance.scoresUI.transform.Find(playerTextObject).Find("Name").gameObject;
                    uiName.GetComponent<TextMeshProUGUI>().text = currentName;
                }
                break;

            case CustomizationReference.Player:
                break;

            case CustomizationReference.NPC:
                break;
        }
    }

    private void ApplyHat()
    {
        switch (customizationReference)
        {
            case CustomizationReference.Controller:
                if (ValidJoinData() && ValidPlayerController())
                {
                    currentHat = PlayerJoinData.Instance.playerCustomizations[playerID - 1].hatOption;

                    squirrelHats.transform.GetChild(0).gameObject.SetActive(false);
                    squirrelHats.transform.GetChild(currentHat).gameObject.SetActive(true);
                }
                break;

            case CustomizationReference.Player:
                if (ValidJoinData())
                {
                    currentHat = PlayerJoinData.Instance.playerCustomizations[playerID - 1].hatOption;

                    squirrelHats.transform.GetChild(0).gameObject.SetActive(false);
                    squirrelHats.transform.GetChild(currentHat).gameObject.SetActive(true);
                }
                break;

            case CustomizationReference.NPC:
                break;
        }
    }

    private void ApplyBody()
    {
        switch (customizationReference)
        {
            case CustomizationReference.Controller:
                if (ValidJoinData() && ValidPlayerController())
                {
                    currentBody = PlayerJoinData.Instance.playerCustomizations[playerID - 1].bodyOption;
                    Material newMat = squirrelBodies.transform.GetChild(currentBody).GetComponent<MeshRenderer>().material;

                    foreach (SkinnedMeshRenderer bodyPart in squirrelBodyParts) {
                    bodyPart.material = newMat; }
                }
                break;

            case CustomizationReference.Player:
                if (ValidJoinData())
                {
                    currentBody = PlayerJoinData.Instance.playerCustomizations[playerID - 1].bodyOption;
                    Material newMat = squirrelBodies.transform.GetChild(currentBody).GetComponent<MeshRenderer>().material;

                    foreach (SkinnedMeshRenderer bodyPart in squirrelBodyParts) { 
                    bodyPart.material = newMat; }
                }
                break;

            case CustomizationReference.NPC:
                break;
        }
    }

    private bool ValidJoinData()
    {
        if (PlayerJoinData.Instance != null) return true;
        else return false;
    }
    private bool ValidPlayerController()
    {
        if (GetComponentInParent<Base_PlayerController>() != null) return true;
        else return false;
    }
    private bool ValidGameManager()
    {
        if (PlayerJoinData.Instance != null) return true;
        else return false;
    }
}