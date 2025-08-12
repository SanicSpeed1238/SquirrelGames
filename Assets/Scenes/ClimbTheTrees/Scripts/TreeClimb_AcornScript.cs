using UnityEngine;

public class TreeClimb_AcornScript : MonoBehaviour
{
    [Header("Attributes")]
    [Range(1,3)]
    public int points;
    public GameObject collectEffect;
    public GameObject destroyEffect;

    public void CollectAcorn(Vector3 playerPosition)
    {
        Instantiate(collectEffect, playerPosition, Quaternion.identity);
        Destroy(gameObject);
    }

    public void DestroyAcorn()
    {
        Instantiate(destroyEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}