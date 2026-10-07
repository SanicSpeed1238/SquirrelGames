using UnityEngine;
using System.Collections;

public class Roadkill_AcornSpawner : MonoBehaviour
{
    [Header("Acorn Objects")]
    public GameObject[] acornPrefabs;

    [Header("Spawner Values")]
    [Range(0.1f, 2f)]
    public float spawnTimer;

    // Other Variables Needed
    private float xRange;
    private float yRange;

    private void Start()
    {
        xRange = transform.localScale.x / 2;
        yRange = transform.localScale.z / 2;

        StartCoroutine(SpawnAcorns());
    }

    IEnumerator SpawnAcorns()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnTimer - Random.Range(0.0f, 0.5f));

            float randomX = Random.Range(-xRange, xRange);
            float randomY = Random.Range(-yRange, yRange);
            Vector3 spawnPos = transform.position + new Vector3(randomX, 0f, randomY);

            Instantiate(acornPrefabs[Random.Range(0, acornPrefabs.Length)], spawnPos, Quaternion.identity);
        }
    }
}