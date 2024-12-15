using UnityEngine;

public class TreeClimb_HazardScript : MonoBehaviour
{
    [Header("Attributes")]
    public float stunDuration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<TreeClimb_PlayerController>() != null)
        {
            other.GetComponentInParent<TreeClimb_PlayerController>().SquishSquirrel(stunDuration);
            Destroy(gameObject);
        }
    }
}