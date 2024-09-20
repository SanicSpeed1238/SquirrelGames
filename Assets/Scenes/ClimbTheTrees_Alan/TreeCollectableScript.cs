using UnityEngine;

public class TreeCollectableScript : MonoBehaviour
{
    [Header("Attributes")]
    public int points;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<ClimbController>() != null)
        {
            Destroy(gameObject);
        }
    }
}