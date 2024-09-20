using UnityEngine;

public class TreeHazardScript : MonoBehaviour
{
    [Header("Attributes")]
    public float stunDuration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<ClimbController>() != null)
        {
            other.GetComponentInParent<ClimbController>().SquishSquirrel(stunDuration);
            Destroy(gameObject);
        }
    }
}