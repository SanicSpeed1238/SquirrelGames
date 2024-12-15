using UnityEngine;

public class TreeClimb_HazardSpawner : MonoBehaviour
{
    [Header("Spawn Attributes")]
    public float spawnRate;

    [Header("Important Components")]
    public GameObject[] hazardObjects;
    public Transform[] treePositions;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnNewCollectable), spawnRate, spawnRate);
    }

    public void SpawnNewCollectable()
    {
        GameObject newCollectable = hazardObjects[Random.Range(0, hazardObjects.Length)];

        Vector3 spawnPosition = transform.position;
        spawnPosition.x = treePositions[Random.Range(0, treePositions.Length)].transform.position.x;
        Instantiate(newCollectable, spawnPosition, Quaternion.identity);
    }
}