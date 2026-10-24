using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System.Collections;
using DG.Tweening;

public class CustomizerController : MonoBehaviour
{
    private enum CurrentlyCustomizing
    {
        None,
        Hat,
        Body,
        Name,
    }
    private CurrentlyCustomizing currentlyCustomizing;

    [Header("Customization Objects")]
    public GameObject squirrelHats;
    public GameObject squirrelBodies;

    [Header("Squirrel Mesh Renders")]
    public SkinnedMeshRenderer[] squirrelBodyParts;

    // Controller References
    private PlayerSetupManager playersetupManager;
    [HideInInspector] public int playerID = 0;
    private bool isFinished = false;
    private bool canPress = false;
    private bool canMoveY = true;
    private bool canMoveX = true;

    // Menu References
    private MultiplayerEventSystem menuController;
    private GameObject menuButtons;
    private GameObject hatOptions;
    private GameObject bodyOptions;
    private GameObject nameOptions;
    private GameObject readyText;

    // Current Selections
    private int currentHatOption;
    private int currentBodyOption;
    private string currentName;

    // Variables for Name Customization
    private bool customizingName;
    private string newName;
    private int letterIndex;
    private int charCode;
    private readonly int underscoreCharCode = 95;
    private readonly int maxNameLength = 6;

    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        canPress = true;
    }

    public void SpawnNewPlayer(int ID, Transform spawn, GameObject menu, PlayerSetupManager manager)
    {
        playersetupManager = manager;
        playerID = ID;
        transform.position = spawn.position;

        currentHatOption = 0;
        currentBodyOption = 0;
        currentName = playersetupManager.GetDefaultName();
        SetPlayerCustomizations();

        menuButtons = menu.transform.Find("OPTIONS").gameObject;
        hatOptions = menu.transform.Find("Hats").gameObject;
        bodyOptions = menu.transform.Find("Bodies").gameObject;
        nameOptions = menu.transform.Find("Name").gameObject;
        readyText = menu.transform.Find("Ready!").gameObject;
        StartCustomizing();
    }

    private void SetPlayerCustomizations()
    {
        PlayerJoinData.Instance.playerCustomizations[playerID - 1].playerName = currentName;
        PlayerJoinData.Instance.playerCustomizations[playerID - 1].hatOption = currentHatOption;
        PlayerJoinData.Instance.playerCustomizations[playerID - 1].bodyOption = currentBodyOption;
    }

    #region Input: Horizontal Navigation (Choosing Specific Customization Option)
    public void OnLeftAndRight(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.x != 0 && canMoveX && !isFinished)
        {
            switch (currentlyCustomizing)
            {
                case CurrentlyCustomizing.Hat:
                    {
                        int hatCount = squirrelHats.transform.childCount;
                        
                        squirrelHats.transform.GetChild(currentHatOption).gameObject.SetActive(false);
                        UpdateHatIcon(false);

                        currentHatOption = (currentHatOption + (input.x > 0 ? 1 : -1) + hatCount) % hatCount;
                        UpdateHatIcon(true);

                        squirrelHats.transform.GetChild(currentHatOption).gameObject.SetActive(true);
                        break;
                    }

                case CurrentlyCustomizing.Body:
                    {
                        int bodyCount = squirrelBodies.transform.childCount;

                        UpdateBodyIcon(false);
                        currentBodyOption = (currentBodyOption + (input.x > 0 ? 1 : -1) + bodyCount) % bodyCount;
                        UpdateBodyIcon(true);

                        Material newMat = squirrelBodies.transform.GetChild(currentBodyOption).GetComponent<MeshRenderer>().material;
                        foreach (SkinnedMeshRenderer bodyPart in squirrelBodyParts) {
                        bodyPart.material = newMat; }

                        break;
                    }

                case CurrentlyCustomizing.Name:
                    if (customizingName)
                    {
                        if (input.x > 0) charCode++;
                        else if (input.x < 0) charCode--;

                        if (charCode+1 == '_' || charCode-1 == '_') charCode = (input.x > 0 ? 'A' : 'Z');
                        else if (charCode > 'Z') charCode = 'A';
                        else if (charCode < 'A') charCode = 'Z';

                        newName = newName[..^1] + (char)charCode;
                        UpdateNameCustomizing(newName);
                    }
                    break;
            }

            canMoveX = false;
        }
        else canMoveX = true;
    }
    #endregion

    #region Input: Vertical Navigation (Change Current Customizable Part)
    public void OnUpAndDown(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.y != 0 && canMoveY && !isFinished)
        {
            switch (currentlyCustomizing)
            {
                case CurrentlyCustomizing.Hat:
                    if (input.y < 0) currentlyCustomizing = CurrentlyCustomizing.Body;
                    break;

                case CurrentlyCustomizing.Body:
                    if (input.y > 0) currentlyCustomizing = CurrentlyCustomizing.Hat;
                    else if (input.y < 0) currentlyCustomizing = CurrentlyCustomizing.Name;
                    break;

                case CurrentlyCustomizing.Name:
                    if (input.y > 0 && !customizingName)
                    {
                        currentlyCustomizing = CurrentlyCustomizing.Body;
                    }
                    break;
            }

            canMoveY = false;
        }
        else
        {
            canMoveY = true;
        }
    }
    #endregion

    #region Input: Confirm Selection
    public void ConfirmSelection(InputAction.CallbackContext context)
    {
        if (!context.started || !canPress || isFinished) return;

        if (currentlyCustomizing == CurrentlyCustomizing.Name)
        {
            if (!customizingName)
            {
                // Start Customizing Name

                menuController.SetSelectedGameObject(null);
                nameOptions.transform.Find("Inputs").gameObject.SetActive(true);             
                customizingName = true;

                letterIndex = 0;
                charCode = underscoreCharCode;
                newName = string.Empty;
                newName += (char)charCode;

                UpdateNameCustomizing(newName);
            }
            else
            {
                if (charCode == underscoreCharCode) return;

                if (++letterIndex == maxNameLength)
                {
                    // Finish Customizing Name
                    UpdateNameCompleted(newName);
                    
                }
                else 
                {
                    // Continue Customizing Name
                    charCode = underscoreCharCode;
                    newName += (char)charCode;
                    UpdateNameCustomizing(newName);
                }
            }
        }
    }
    #endregion

    #region Input: Cancel Selection
    public void CancelSelection(InputAction.CallbackContext context)
    {
        if (!context.started || playersetupManager.transitioningScene) return;

        if (!isFinished)
        {
            if (customizingName)
            {
                customizingName = false;
                menuController.SetSelectedGameObject(menuButtons.transform.Find("Name").gameObject);
                UpdateNameCompleted(currentName);
            }
            else
            {
                playersetupManager.LoadMainMenu();
            }
        }
        else
        {
            isFinished = false;
            playersetupManager.UpdatePlayerReady(playerID, false);
            GetComponentInChildren<Animator>().SetTrigger("Back");

            readyText.SetActive(false);
            hatOptions.SetActive(true);
            bodyOptions.SetActive(true);
            nameOptions.SetActive(true);
            StartCustomizing();
        }
    }
    #endregion

    #region Input: Complete Customization
    public void CompleteSelection(InputAction.CallbackContext context)
    {
        if (customizingName)
        {
            if (newName.Length == 1) return;
            else newName = newName[..^1];

            UpdateNameCompleted(newName); return;
        }

        if (context.started && !isFinished)
        {
            menuController.SetSelectedGameObject(null);
            hatOptions.SetActive(false);
            bodyOptions.SetActive(false);
            nameOptions.SetActive(false);

            readyText.SetActive(true);
            readyText.GetComponent<TextMeshProUGUI>().text = currentName + "\nIS READY!";
            readyText.GetComponent<DOTweenAnimation>().RecreateTweenAndPlay();
            GetComponentInChildren<Animator>().SetTrigger("Complete");

            isFinished = true;
            playersetupManager.UpdatePlayerReady(playerID, true);
            SetPlayerCustomizations();
        }     
    }
    #endregion

    #region Customization UI

    private void StartCustomizing()
    {
        menuController = GetComponent<MultiplayerEventSystem>();
        menuController.playerRoot = menuButtons;
        menuController.SetSelectedGameObject(menuButtons.transform.Find("Name").gameObject);
        UpdateNameCompleted(currentName);
        currentlyCustomizing = CurrentlyCustomizing.Name;
    }

    private void UpdateHatIcon(bool setActive)
    {
        hatOptions.transform.Find("Icons").GetChild(currentHatOption).gameObject.SetActive(setActive);
    }
    private void UpdateBodyIcon(bool setActive)
    {
        bodyOptions.transform.Find("Icons").GetChild(currentBodyOption).gameObject.SetActive(setActive);
    }
   
    private void UpdateNameCustomizing(string name)
    {       
        string coloredText = name[..^1] + "<color=yellow>" + name[^1] + "</color>";
        nameOptions.GetComponent<TextMeshProUGUI>().text = coloredText;
    }
    private void UpdateNameCompleted(string name)
    {
        name = new string(name).Replace("_", "");
        currentName = name;
        SetPlayerCustomizations();

        customizingName = false;
        nameOptions.transform.Find("Inputs").gameObject.SetActive(false);
        menuController.SetSelectedGameObject(menuButtons.transform.Find("Name").gameObject);
        nameOptions.GetComponent<TextMeshProUGUI>().text = name;
    }

    #endregion
}