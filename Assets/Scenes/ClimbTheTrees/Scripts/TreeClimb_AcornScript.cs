using UnityEngine;

public class TreeClimb_AcornScript : MonoBehaviour
{
    [Header("Attributes")]
    public int points;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<TreeClimb_PlayerController>() != null)
        {
            Destroy(gameObject);
        }
    }
}