using UnityEngine;

public class TreeClimb_HazardScript : MonoBehaviour
{
    [Header("Squish Attributes")]
    [Range(0.25f, 0.75f)]
    public float squishFactor;
    [Range(0.5f, 3.0f)]
    public float stunDuration;
    public GameObject impactEffect;

    [Header("Freefall Attributes")]
    [Range(1f, 10f)]
    public float forwardImpulse;
    public Vector3 rotationImpulse;

    // Other Variables Needed
    private int treeIndex;

    public void SquishSquirrel(TreeClimb_PlayerController player)
    {
        player.SquishSquirrel(stunDuration, squishFactor);

        Instantiate(impactEffect, GetComponent<Rigidbody>().position, Quaternion.identity);

        GetComponent<TreeClimb_ObjectPhysics>().EnableFreefall(forwardImpulse, rotationImpulse);
        GetComponent<BoxCollider>().enabled = false;
    }

    public void SetTreeIndex(int index)
    {
        treeIndex = index;
    }
    public int GetTreeIndex() // Used for collision purposes
    {
        return treeIndex;
    }
}