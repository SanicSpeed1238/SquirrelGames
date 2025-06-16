using UnityEngine;

public class Base_PlayerController : MonoBehaviour
{
    // Variables available for ALL player controllers
    [HideInInspector] public int playerID;
    [HideInInspector] public int playerScore;
    [HideInInspector] public Color playerColor;
    [HideInInspector] public Transform startLocation;

    public void SetPlayerData(int number, Color color, Transform location)
    {
        playerID = number;
        playerColor = color;

        startLocation = location;
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.position = startLocation.position;
            rb.rotation = startLocation.rotation;
        }
        else
        {
            transform.SetPositionAndRotation(startLocation.position, startLocation.rotation);
        }

        playerScore = 0;
    }
}