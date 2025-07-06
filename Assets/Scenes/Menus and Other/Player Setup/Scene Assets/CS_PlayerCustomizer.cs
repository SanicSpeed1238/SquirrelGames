using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class CS_PlayerCustomizer : MonoBehaviour
{
    private enum CurrentlyCustomizing
    {
        None,
        Hat,
        Body,
        Name,
    }
    private CurrentlyCustomizing currentlyCustomizing;

    [Header("Important Components")]
    public SkinnedMeshRenderer[] squirrelBodyParts;

    [Header("Customization Objects")]
    public GameObject squirrelHats;
    public GameObject squirrelBodies;

    // Controller References
    private int playerID;
    private bool canMove = true;
    private bool canMoveY = true;
    private bool canMoveX = true;

    // Menu References
    private MultiplayerEventSystem menuController;
    private GameObject customizationMenu;
    private GameObject menuButtons;
    private GameObject hatOptions;
    private GameObject bodyOptions;
    private GameObject nameOptions;

    // Current Selections
    private int currentHatOption;
    private int currentBodyOption;
    private string currentName;

    // Variables for Name Customization
    private bool customizingName;
    private string newName;
    private int letterIndex;
    private int charCode;
    private readonly int startingCharCode = 95;
    private readonly int maxNameLength = 6;

    public void SpawnNewPlayer(int ID, Transform spawn, GameObject menu)
    {
        playerID = ID;
        transform.position = spawn.position;

        currentHatOption = 0;
        currentBodyOption = 0;
        currentName = "PLAYER";

        customizationMenu = menu;
        menuButtons = menu.transform.Find("Buttons").gameObject;
        hatOptions = menu.transform.Find("Hats").gameObject;
        bodyOptions = menu.transform.Find("Bodies").gameObject;
        nameOptions = menu.transform.Find("Name").gameObject;

        currentlyCustomizing = CurrentlyCustomizing.Hat;
        menuController = GetComponent<MultiplayerEventSystem>();
        menuController.playerRoot = menuButtons;
        menuController.firstSelectedGameObject = menuButtons.transform.Find("Hat").gameObject;
        menuController.SetSelectedGameObject(menuController.firstSelectedGameObject);
    }

    #region Input: Horizontal Navigation (Choosing Specific Customization Option)
    public void OnLeftAndRight(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.x != 0 && canMoveX && canMove)
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
                        else if (charCode > 'Z') charCode = '_';
                        else if (charCode < 'A') charCode = '_';

                        newName = newName[..^1] + (char)charCode;
                        UpdateNameEntry(newName);
                        DisplayArrows(nameOptions, false);
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

        if (input.y != 0 && canMoveY && canMove)
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

    #region Input: Confirm Select
    public void ConfirmSelection(InputAction.CallbackContext context)
    {
        if (!context.started || !canMove) return;

        if (currentlyCustomizing == CurrentlyCustomizing.Name)
        {
            if (!customizingName)
            {
                customizingName = true;
                newName = string.Empty;            
                menuController.SetSelectedGameObject(null);             

                letterIndex = 0;
                charCode = startingCharCode;
                newName += (char)charCode;

                DisplayArrows(nameOptions, true);
                UpdateNameEntry(newName);
            }
            else
            {
                letterIndex++;  
                if (letterIndex == maxNameLength || charCode == '_')
                {
                    // Finish Customizing Name

                    customizingName = false;
                    menuController.SetSelectedGameObject(menuButtons.transform.Find("Name").gameObject);

                    if (letterIndex > 1) currentName = new string(newName).Replace("_", ""); ;
                    UpdateNameComplete(currentName);
                }
                else // Continue Customizing Name
                {
                    charCode = startingCharCode;
                    newName += (char)charCode;
                    UpdateNameEntry(newName);
                }
            }
        }
    }
    #endregion

    #region Input: Complete Selection
    public void CompleteSelection(InputAction.CallbackContext context)
    {
        if (!context.started || !canMove || customizingName) return;

        hatOptions.SetActive(false);
        bodyOptions.SetActive(false);
        menuController.SetSelectedGameObject(null);
        canMove = false;

        GetComponentInChildren<Animator>().SetTrigger("Complete");
    }
    #endregion

    #region Customization UI
    private void UpdateHatIcon(bool setActive)
    {
        hatOptions.transform.Find("Icons").GetChild(currentHatOption).gameObject.SetActive(setActive);
    }
    private void UpdateBodyIcon(bool setActive)
    {
        bodyOptions.transform.Find("Icons").GetChild(currentBodyOption).gameObject.SetActive(setActive);
    }
    private void UpdateNameComplete(string name)
    {
        nameOptions.GetComponent<TextMeshProUGUI>().text = name;
    }
    private void UpdateNameEntry(string name)
    {
        string coloredText = name[..^1] + "<color=yellow>" + name[^1] + "</color>";
        nameOptions.GetComponent<TextMeshProUGUI>().text = coloredText;
    }
    private void DisplayArrows(GameObject currentCustomization, bool setActive)
    {
        currentCustomization.transform.Find("Arrows").gameObject.SetActive(setActive);
    }
    #endregion
}