using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNut_TreeScript : MonoBehaviour
{
    public Transform character;
    public GameObject treeObject;

    private bool treeSpawned = false;
    public float treeHeightHalf = 1f;


    // Update is called once per frame
    void Update()
    {
        float yzDiff = (float)Math.Sqrt(treeHeightHalf*treeHeightHalf/2);
        Vector3 topPoint = new Vector3(transform.position.x, transform.position.y + yzDiff, transform.position.z + yzDiff);

        // Check the distance between the character and the top point
        float distanceToTop = Vector3.Distance(character.position, topPoint);

        if (distanceToTop <= treeHeightHalf && !treeSpawned)
        {            
            SpawnNewTree(topPoint);
            treeSpawned = true; // Prevent multiple spawns
        }

        if (treeSpawned && distanceToTop > treeHeightHalf*2)
        {
            Destroy(gameObject);
        }
    }

    private void SpawnNewTree(Vector3 topPoint)
    {
        Vector3 newPosition = topPoint + transform.up*treeHeightHalf;
        // Instantiate the new tree
        Instantiate(treeObject, newPosition, transform.rotation);
    }
}
