using UnityEngine;

public class TreeClimb_AcornScript : MonoBehaviour
{
    [Header("Attributes")]
    public int points;
    public GameObject collectEffect;

    public void CollectAcorn(TreeClimb_PlayerController player)
    {
        Instantiate(collectEffect, transform.position, Quaternion.identity);

        player.AddToPlayerScore(points);

        Destroy(gameObject);
    }
}