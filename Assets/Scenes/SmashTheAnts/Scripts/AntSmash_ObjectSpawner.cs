using UnityEngine;
using System.Collections;

public class AntSmash_ObjectSpawner : MonoBehaviour
{
    [Header("Spawner Values")]
    [Range(0.75f, 1.75f)]
    public float spawnTimeRate; // How long it takes before game objects are spawned
    [Range(0.1f, 1f)]
    public float spawnTimeRateDecrease; // How much the spawn time rate should decrease
    [Range(1, 10)]
    public int increaseTimeInterval; // How long it takes before the spawn time rate is lowered

    [Header("Object Prefabs")]
    [SerializeField] private GameObject squirrelObject;
    [SerializeField] private GameObject antObject;
    [SerializeField] private GameObject fireAntObject;
    [SerializeField] private GameObject goldenAntObject;

    [Header("Object Appearance Rates (Must Add up to 100)")]
    [Range(0, 100)]
    public int squirrelSpawnRate; // Percentage of how likely a Squirrel will spawn
    [Range(0, 100)]
    public int antSpawnRate; // Percentage of how likely an Ant will spawn
    [Range(0, 100)]
    public int fireAntSpawnRate; // Percentage of how likely a Fire Ant will spawn
    [Range(0, 100)]
    public int goldenAntSpawnRate; // Percentage of how likely a Golden Ant will spawn

    // Other Variables Needed
    private float positionRange;
    private float increaseSpawnRateTimer;

    private void Start()
    {
        positionRange = transform.localScale.x / 2;
        increaseSpawnRateTimer = 0f;

        StartCoroutine(SpawnObjects());
    }

    private void Update()
    {
        if(AntSmash_GameManager.Instance.gamePlaying) IncreaseSpawnRate();
    }

    IEnumerator SpawnObjects() // Spawns game objects while the game is playing
    {
        yield return new WaitUntil(() => AntSmash_GameManager.Instance.gamePlaying);

        while (AntSmash_GameManager.Instance.gamePlaying)
        {
            yield return new WaitForSeconds(spawnTimeRate + Random.Range(-0.5f, 0.0f));
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
        int randomValue = Random.Range(1, 101);

        if (randomValue <= antSpawnRate)
        {
            return antObject;
        }
        else if (randomValue <= antSpawnRate + fireAntSpawnRate)
        {
            return fireAntObject;
        }
        else if (randomValue <= antSpawnRate + fireAntSpawnRate + goldenAntSpawnRate)
        {
            return goldenAntObject;
        }
        else
        {
            return squirrelObject;
        }
    }

    private void IncreaseSpawnRate() // Increases the spawn rate by decreasing the time it takes to spawn objects
    {
        increaseSpawnRateTimer += Time.deltaTime;

        if(increaseSpawnRateTimer >= increaseTimeInterval)
        {
            spawnTimeRate -= spawnTimeRateDecrease;
            if (spawnTimeRate < 0.51f) spawnTimeRate = 0.51f;
            increaseSpawnRateTimer = 0f;
        }
    }
}