using UnityEngine;
using System.Collections;

public class AntSmash_ObjectSpawner : MonoBehaviour
{
    [Header("Spawner Values")]
    [Range(1f, 5f)]
    public float spawnTimeRate; // How long it takes before game objects are spawned
    [Range(0.1f, 1f)]
    public float spawnTimeRateDecrease; // How much the spawn time rate should decrease
    [Range(5, 20)]
    public int increaseTimeInterval; // How long it takes before the spawn time rate is lowered

    [Header("Object Prefabs")]
    [SerializeField] private GameObject squirrelObject;
    [SerializeField] private GameObject antObject;

    [Header("Object Appearance Rates")]
    [Range(0, 100)]
    public int squirrelSpawnRate; // Percentage of how likely a Squirrel will spawn
    [Range(0, 100)]
    public int antSpawnRate; // Percentage of how likely an Ant will spawn

    // Other Variables Needed
    private AntSmash_GameManager gameManager;
    private float positionRange;
    private float increaseSpawnRateTimer;

    private void Start()
    {
        gameManager = GameObject.FindAnyObjectByType<AntSmash_GameManager>();
        positionRange = transform.localScale.x / 2;
        increaseSpawnRateTimer = 0f;

        StartCoroutine(SpawnObjects());
    }

    private void Update()
    {
        if(gameManager.gamePlaying) IncreaseSpawnRate();
    }

    IEnumerator SpawnObjects() // Spawns game objects while the game is playing
    {
        yield return new WaitUntil(() => gameManager.gamePlaying);

        while (gameManager.gamePlaying)
        {
            yield return new WaitForSeconds(spawnTimeRate + Random.Range(-0.5f, 0.1f));
            SpawnGameObject();
        }
    }

    private void SpawnGameObject() // Spawns a random game object within a certain range of the spawner object
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

    private void IncreaseSpawnRate() // Increases the spawn rate by decreasing the time it takes to spawn objects
    {
        increaseSpawnRateTimer += Time.deltaTime;

        if(increaseSpawnRateTimer >= increaseTimeInterval)
        {
            spawnTimeRate -= spawnTimeRateDecrease;
            if (spawnTimeRate < 0.6f) spawnTimeRate = 0.6f;
            increaseSpawnRateTimer = 0f;
        }
    }
}