using UnityEngine;

public class TreeClimb_AcornScript : MonoBehaviour
{
    [Header("Attributes")]
    public int points;
    public GameObject collectEffect;

    public void CollectAcorn()
    {
        Instantiate(collectEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}