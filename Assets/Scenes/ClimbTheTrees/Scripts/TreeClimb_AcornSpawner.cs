using UnityEngine;
using System.Collections;

public class TreeClimb_AcornSpawner : MonoBehaviour
{
    [Header("Spawn Attributes")]
    [Range(0.5f, 1.0f)]
    public float spawnTime;

    [Header("Important Components")]
    public GameObject[] acornPrefabs;
    public Transform[] treePositions;

    private void Start()
    {
        StartCoroutine(SpawnAcorn());
    }

    IEnumerator SpawnAcorn()
    {
        while(true)
        {
            yield return new WaitForSeconds(spawnTime - Random.Range(0f, 0.5f));

            GameObject newCollectable = acornPrefabs[Random.Range(0, acornPrefabs.Length)];
            Vector3 spawnPosition = transform.position;
            spawnPosition.x = treePositions[Random.Range(0, treePositions.Length)].transform.position.x;
            Instantiate(newCollectable, spawnPosition, Quaternion.identity);
        }     
    }
}