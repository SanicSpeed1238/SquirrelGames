using UnityEngine;

public class TreeClimb_HazardScript : MonoBehaviour
{
    [Header("Attributes")]
    [Range(0.5f, 3.0f)]
    public float stunDuration;
    [Range(0.25f, 0.75f)]
    public float squishFactor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<TreeClimb_PlayerController>() != null)
        {
            other.GetComponent<TreeClimb_PlayerController>().SquishSquirrel(stunDuration, squishFactor);
            Destroy(gameObject);
        }
    }
}