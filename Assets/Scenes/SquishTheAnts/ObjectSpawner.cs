using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Object Prefabs")]
    [SerializeField] private GameObject antObject;
    [SerializeField] private GameObject squirrelObject;

    [Header("Spawner Values")]
    [Range(0.1f, 3.0f)]
    public float spawnRate;
    [Range(0, 100)]
    public int antSpawnRate;
    [Range(0, 100)]
    public int squirrelSpawnRate;

    // Variables Needed
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

    private void SpawnObjects()
    {
        Vector3 offset = transform.right * Random.Range(-positionRange, positionRange);
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z) + offset;
        Instantiate(antObject, spawnPosition, transform.rotation);
    }
}