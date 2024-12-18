using UnityEngine;
using System.Collections;

public class TreeClimb_HazardSpawner : MonoBehaviour
{
    [Header("Spawn Attributes")]
    [Range(1f, 3f)]
    public float spawnTime;

    [Header("Important Components")]
    public GameObject[] hazardPrefabs;
    public Transform[] treePositions;

    private void Start()
    {
        StartCoroutine(SpawnHazard());
    }

    IEnumerator SpawnHazard()
    {
        while(true)
        {
            yield return new WaitForSeconds(spawnTime + Random.Range(0f, 0.5f));

            GameObject newCollectable = hazardPrefabs[Random.Range(0, hazardPrefabs.Length)];
            Vector3 spawnPosition = transform.position;
            spawnPosition.x = treePositions[Random.Range(0, treePositions.Length)].transform.position.x;
            Instantiate(newCollectable, spawnPosition, Quaternion.identity);
        }
    }
}