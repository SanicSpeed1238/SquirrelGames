using UnityEngine;
using System.Collections;

public class AntSmash_AnimalSpawner : MonoBehaviour
{
    [Header("Spawner Values")]
    [Range(0.1f, 3f)]
    public float spawnTimer;
    [Range(0.01f, 0.5f)]
    public float spawnTimerDecreaseAmount;
    [Range(0.1f, 0.5f)]
    public float spawnTimerMinimumDelay;
    [Range(1, 10)]
    public int timeToDecreaseSpawnTimer;

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

    [Header("Important References")]
    public AntSmash_BushScript bushObject;

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
        if(MG_GameManager.Instance.gamePlaying) IncreaseSpawnRate();
    }

    IEnumerator SpawnObjects() // Spawns game objects while the game is playing
    {
        yield return new WaitUntil(() => MG_GameManager.Instance.gamePlaying);

        while (MG_GameManager.Instance.gamePlaying)
        {
            float waitSeconds = spawnTimer + Random.Range(-0.5f, 0.0f);
            if (waitSeconds < spawnTimerMinimumDelay) waitSeconds = spawnTimerMinimumDelay;
            yield return new WaitForSeconds(waitSeconds);
   
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
            bushObject.ShakeBush();
            return squirrelObject;
        }
    }

    private void IncreaseSpawnRate() // Increases the spawn rate by decreasing the time it takes to spawn objects
    {
        increaseSpawnRateTimer += Time.deltaTime;

        if(increaseSpawnRateTimer >= timeToDecreaseSpawnTimer)
        {
            spawnTimer -= spawnTimerDecreaseAmount;
            if (spawnTimer < spawnTimerMinimumDelay) spawnTimer = spawnTimerMinimumDelay;
            increaseSpawnRateTimer = 0f;
        }
    }
}