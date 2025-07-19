using UnityEngine;

public class TreeClimb_HazardScript : MonoBehaviour
{
    [Header("Squish Attributes")]
    [Range(0.1f, 1.0f)]
    public float stunDuration;
    [Range(0.1f, 0.5f)]
    public float squishFactor;   
    public GameObject impactEffect;

    [Header("Freefall Attributes")]
    [Range(1f, 50f)]
    public float forwardImpulse;
    public Vector3 rotationImpulse;

    // Other Variables Needed
    private int treeIndex;

    public void HitHazard()
    {
        Instantiate(impactEffect, GetComponent<Rigidbody>().position, Quaternion.identity);

        GetComponent<BoxCollider>().enabled = false;
        GetComponent<TreeClimb_ObjectPhysics>().EnableFreefall(forwardImpulse, rotationImpulse);
    }

    // Following methods are used for collision purposes
    public void SetTreeIndex(int index)
    {
        treeIndex = index;
    }
    public int GetTreeIndex()
    {
        return treeIndex;
    }
}