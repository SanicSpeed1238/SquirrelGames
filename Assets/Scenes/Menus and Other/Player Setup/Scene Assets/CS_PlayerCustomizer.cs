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
    private char[] currentNameSelections;
    private int nameIndex;
    private int charCode;
    private readonly int startingCharCode = 95;
    private readonly int maxNameLength = 6;

    public void SpawnNewPlayer(int ID, Transform spawn, GameObject menu)
    {
        playerID = ID;
        transform.position = spawn.position;

        currentHatOption = 0;
        currentBodyOption = 0;
        currentName = "SQUISH";

        customizationMenu = menu;
        menuButtons = menu.transform.Find("Buttons").gameObject;
        hatOptions = menu.transform.Find("Hat Options").gameObject;
        bodyOptions = menu.transform.Find("Body Options").gameObject;
        nameOptions = menu.transform.Find("Name").gameObject;

        currentlyCustomizing = CurrentlyCustomizing.Hat;
        menuController = GetComponent<MultiplayerEventSystem>();
        menuController.playerRoot = menuButtons;
        menuController.firstSelectedGameObject = menuButtons.transform.Find("Hat Button").gameObject;
        menuController.SetSelectedGameObject(menuController.firstSelectedGameObject);
    }

    #region Input: Horizontal Navigation (Hat / Body)
    public void OnLeftAndRight(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.x != 0 && canMoveX)
        {
            switch (currentlyCustomizing)
            {
                case CurrentlyCustomizing.Hat:
                    squirrelHats.transform.GetChild(currentHatOption).gameObject.SetActive(false);
                    int hatCount = squirrelHats.transform.childCount;
                    currentHatOption = (currentHatOption + (input.x > 0 ? 1 : -1) + hatCount) % hatCount;
                    squirrelHats.transform.GetChild(currentHatOption).gameObject.SetActive(true);
                    break;

                case CurrentlyCustomizing.Body:
                    int bodyCount = squirrelBodies.transform.childCount;
                    currentBodyOption = (currentBodyOption + (input.x > 0 ? 1 : -1) + bodyCount) % bodyCount;
                    Material newMat = squirrelBodies.transform.GetChild(currentBodyOption).GetComponent<MeshRenderer>().material;
                    foreach (SkinnedMeshRenderer bodyPart in squirrelBodyParts)
                        bodyPart.material = newMat;
                    break;
            }

            canMoveX = false;
        }
        else
        {
            canMoveX = true;
        }
    }
    #endregion

    #region Input: Vertical Navigation (Switch Option / Name Entry)
    public void OnUpAndDown(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.y != 0 && canMoveY)
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
                    if (customizingName)
                    {
                        if (input.y < 0) charCode++;
                        else if (input.y > 0) charCode--;

                        if (charCode > 'Z') charCode = 'A';
                        if (charCode < 'A') charCode = 'Z';

                        currentNameSelections[nameIndex] = (char)charCode;
                        UpdateLetter((char)charCode);
                    }
                    else
                    {
                        if (input.y > 0) currentlyCustomizing = CurrentlyCustomizing.Body;
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

    #region Input: Confirm Select (Name Entry)
    public void ConfirmSelection(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (currentlyCustomizing == CurrentlyCustomizing.Name)
        {
            if (!customizingName)
            {
                customizingName = true;
                currentNameSelections = new char[maxNameLength];
                UpdateCompleteName("");              
                menuController.SetSelectedGameObject(null);             

                nameIndex = 0;
                charCode = startingCharCode;

                ActivateLetterObject(nameIndex);
                UpdateLetter((char)charCode);
            }
            else
            {
                nameIndex++;  
                if (nameIndex == maxNameLength || currentNameSelections[nameIndex-1] == '\0')
                {
                    // Finish Customizing Name

                    customizingName = false;
                    DeactivateLetterObjects();
                    menuController.SetSelectedGameObject(menuButtons.transform.Find("Name Button").gameObject);

                    if (nameIndex > 1) currentName = new string(currentNameSelections).Replace("\0", "");
                    UpdateCompleteName(currentName);
                }
                else // Continue Customizing Name
                {
                    charCode = startingCharCode;
                    UpdateLetter((char)charCode);
                    ActivateLetterObject(nameIndex);
                }
            }
        }
    }
    #endregion

    #region Input: Complete Selection (Player Customizations / Name Entry)
    public void CompleteSelection(InputAction.CallbackContext context)
    {

    }
    #endregion

    #region Name Customization UI
    private void UpdateCompleteName(string newName)
    {
        nameOptions.GetComponent<TextMeshProUGUI>().text = newName;
    }
    private void UpdateLetter(char letter)
    {
        nameOptions.transform.GetChild(nameIndex).GetComponent<TextMeshProUGUI>().text = letter.ToString();
    }
    private void ActivateLetterObject(int index)
    {
        nameOptions.transform.GetChild(index).gameObject.SetActive(true);
    }
    private void DeactivateLetterObjects()
    {
        for (int i = 0; i < maxNameLength; i++)
        {
            nameOptions.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    #endregion
}