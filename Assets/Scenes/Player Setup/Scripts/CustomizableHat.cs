using UnityEngine;

public class CustomizableHat : MonoBehaviour
{
    [Header("Player Colors")]
    public Color player1 = Color.red;
    public Color player2 = Color.blue;
    public Color player3 = Color.yellow;
    public Color player4 = Color.green;

    // Variables Needed
    private Material hatMaterial;

    private void Start()
    {
        GetMaterialReference();
        GetPlayerData();
    }

    private void SetColor(int playerID)
    {
        hatMaterial.color = playerID switch
        {
            1 => player1,
            2 => player2,
            3 => player3,
            4 => player4,
            _ => Color.black,
        };
    }

    private void GetMaterialReference()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        foreach (Material mat in meshRenderer.materials)
        {
            if (mat.name.StartsWith("mat_Hat_PlayerColor"))
            {
                hatMaterial = mat;
            }
        }
    }

    private void GetPlayerData()
    {
        if (transform.root.TryGetComponent<CustomizerController>(out CustomizerController customizerController))
        {
            SetColor(customizerController.playerID);
        }
        else if (transform.root.TryGetComponent<CustomizedPlayer>(out CustomizedPlayer customizedPlayer))
        {
            SetColor(customizedPlayer.playerID);
        }
        else
        {
            SetColor(0);
        }
    }
}