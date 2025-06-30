using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerCustomizer : MonoBehaviour
{
    private enum CurrentlyCustomizing
    {
        None,
        Hat,
        Body,
    }

    [Header("Important Components")]
    public SkinnedMeshRenderer[] squirrelBodyParts;

    [Header("Customization Objects")]
    public GameObject squirrelHats;
    public GameObject squirrelBodies;

    // Variables Needed
    private int playerID;
    private int currentHatOption;
    private int currentBodyOption;
    private GameObject customizationOptions;
    private GameObject hatOptions;
    private GameObject bodyOptions;

    private CurrentlyCustomizing currentlyCustomizing;

    public void SpawnNewPlayer(int ID, Transform spawn, GameObject menu)
    {
        playerID = ID;
        transform.position = spawn.position;
        customizationOptions = menu;

        currentHatOption = 0;
        currentBodyOption = 0;
        currentlyCustomizing = CurrentlyCustomizing.Hat;
    }

    #region Hat Customization
    public void OnLeftAndRight(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.x != 0)
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
                    var newMat = squirrelBodies.transform.GetChild(currentBodyOption).GetComponent<MeshRenderer>().material;
                    foreach (SkinnedMeshRenderer bodyPart in squirrelBodyParts) { bodyPart.material = newMat; }
                    break;
            }
        }
    }
    #endregion

    #region Body Customization
    public void OnUpAndDown(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.y != 0)
        {
            switch (currentlyCustomizing)
            {
                case CurrentlyCustomizing.Hat:
                    if (input.y < 0)
                    {
                        currentlyCustomizing = CurrentlyCustomizing.Body;
                    }
                    break;

                case CurrentlyCustomizing.Body:
                    if (input.y > 0)
                    {
                        currentlyCustomizing = CurrentlyCustomizing.Hat;
                    }
                    break;
            }
        }
    }
    #endregion
}