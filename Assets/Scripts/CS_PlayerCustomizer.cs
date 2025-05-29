using UnityEngine;

public class CS_PlayerCustomizer : MonoBehaviour
{
    private enum CurrentlyCustomizing
    {
        None,
        Hat,
        Body,
    }

    [Header("Important Components")]
    public SkinnedMeshRenderer squirrelMeshRenderer;

    [Header("Customization Objects")]
    public GameObject squirrelHats;
    public GameObject squirrelBodies;

    [Header("UI Objects")]
    public GameObject hatOptions;
    public GameObject bodyOptions;

    // Variables Needed
    private int playerID;

    private int currentHatOption;
    private int currentBodyOption;

    private CurrentlyCustomizing currentlyCustomizing;

    private void Start()
    {
        currentHatOption = 0;
        currentBodyOption = 0;

        currentlyCustomizing = CurrentlyCustomizing.Hat;
    }

    private void Update()
    {
        switch(currentlyCustomizing)
        {
            case CurrentlyCustomizing.Hat:
                if (Input.GetKeyDown(KeyCode.D))
                {
                    squirrelHats.transform.GetChild(currentHatOption).gameObject.SetActive(false);
                    currentHatOption++;
                    squirrelHats.transform.GetChild(currentHatOption).gameObject.SetActive(true);
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    squirrelHats.transform.GetChild(currentHatOption).gameObject.SetActive(false);
                    currentHatOption--;
                    squirrelHats.transform.GetChild(currentHatOption).gameObject.SetActive(true);
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    currentlyCustomizing = CurrentlyCustomizing.Body;
                }
                break;

            case CurrentlyCustomizing.Body:
                if (Input.GetKeyDown(KeyCode.D))
                {
                    currentBodyOption++;
                    squirrelMeshRenderer.material = squirrelBodies.transform.GetChild(currentBodyOption).gameObject.GetComponent<MeshRenderer>().material;
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    currentBodyOption--;
                    squirrelMeshRenderer.material = squirrelBodies.transform.GetChild(currentBodyOption).gameObject.GetComponent<MeshRenderer>().material;
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    currentlyCustomizing = CurrentlyCustomizing.Hat;
                }
                break;
        }
    }
}