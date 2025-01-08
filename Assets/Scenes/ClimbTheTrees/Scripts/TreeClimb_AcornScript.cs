using UnityEngine;

public class TreeClimb_AcornScript : MonoBehaviour
{
    [Header("Attributes")]
    public int points;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<TreeClimb_PlayerController>() != null)
        {
            other.GetComponent<TreeClimb_PlayerController>().AddToPlayerScore(points);

            Destroy(gameObject);
        }
    }
}