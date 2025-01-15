using UnityEngine;

public class TreeClimb_AcornScript : MonoBehaviour
{
    [Header("Attributes")]
    public int points;

    public void CollectAcorn(TreeClimb_PlayerController player)
    {
        // FX

        player.AddToPlayerScore(points);

        Destroy(gameObject);
    }
}