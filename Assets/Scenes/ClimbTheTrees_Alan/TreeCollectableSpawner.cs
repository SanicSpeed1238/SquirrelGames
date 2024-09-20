using UnityEngine;

public class TreeCollectableSpawner : MonoBehaviour
{
    [Header("Spawn Attributes")]
    public float spawnRate;

    [Header("Important Components")]
    public GameObject[] collectableObjects;
    public Transform[] treePositions;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnNewCollectable), 0, spawnRate);
    }

    public void SpawnNewCollectable()
    {
        GameObject newCollectable = collectableObjects[Random.Range(0, collectableObjects.Length)];

        Vector3 spawnPosition = transform.position;
        spawnPosition.x = treePositions[Random.Range(0, treePositions.Length)].transform.position.x;
        Instantiate(newCollectable, spawnPosition, Quaternion.identity);
    }
}