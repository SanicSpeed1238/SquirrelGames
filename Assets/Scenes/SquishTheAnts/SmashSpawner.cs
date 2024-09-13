using UnityEngine;

public class SmashSpawner : MonoBehaviour
{
    [Header("Object Prefabs")]
    [SerializeField] private GameObject squirrelObject;
    [SerializeField] private GameObject antObject;

    [Header("Spawner Values")]
    [Range(0.1f, 3.0f)]
    public float spawnRate;
    [Range(0, 100)]
    public int squirrelSpawnRate;
    [Range(0, 100)]
    public int antSpawnRate;

    // Other Variables Needed
    private float positionRange;

    private void Start()
    {
        positionRange = transform.localScale.x / 2;
        StartSpawning();
    }

    public void StartSpawning()
    {
        InvokeRepeating(nameof(SpawnObjects), 1f, spawnRate);
    }

    private void SpawnObjects() // Spawns a random game object within a certain range of the spawner object
    {
        Vector3 offset = transform.right * Random.Range(-positionRange, positionRange);
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z) + offset;

        GameObject spawnedObject = GetRandomObject();
        Instantiate(spawnedObject, spawnPosition, transform.rotation);
    }

    private GameObject GetRandomObject() // Randomly chooses a game object based on their appearance rate
    {
        int totalRate = squirrelSpawnRate + antSpawnRate;
        int randomValue = Random.Range(0, totalRate);

        if (randomValue < squirrelSpawnRate)
        {
            return squirrelObject;
        }
        else
        {
            return antObject;
        }
    }
}