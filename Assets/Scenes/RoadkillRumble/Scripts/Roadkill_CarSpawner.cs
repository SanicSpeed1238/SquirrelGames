using UnityEngine;
using System.Collections;

public class Roadkill_CarSpawner : MonoBehaviour
{
    [Header("Car Prefabs")]
    public GameObject[] carObjects;

    [Header("Important References")]
    [Range(1f,2f)]
    public float spawnTimer;
    public Transform[] spawnPositions;

    private void Start()
    {
        StartCoroutine(SpawnCars());
    }

    IEnumerator SpawnCars()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnTimer);

            int spawnNum = Random.Range(1, (2 + 1));
            int usedLane = -1;

            for (int car = 0; car < spawnNum; car++)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 1f));

                int randomLame = Random.Range(0, 4);
                if (randomLame == usedLane) break;
                usedLane = randomLame;

                Transform spawnPos = spawnPositions[randomLame];
                Instantiate(carObjects[Random.Range(0, carObjects.Length)], spawnPos.position, spawnPos.rotation);
            }
        }
    }
}