using UnityEngine;

public class CS_SquirrelCustomize : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 4)]
    public int playerID = 0;
    public bool randomizeCustomizations;

    [Header("Customization Objects")]
    public GameObject squirrelHats;
    public GameObject squirrelBodies;

    [Header("Squirrel Mesh Renders")]
    public SkinnedMeshRenderer[] squirrelBodyParts;

    private void Start()
    {
        if (CS_PlayerJoinData.Instance != null)
        {
            ApplyName();
            ApplyBody();
            ApplyHat();
        }
    }

    private void ApplyName()
    {

    }

    private void ApplyHat()
    {
        
    }

    private void ApplyBody()
    {

    }
}