using UnityEngine;
using System.Collections;

public class Roadkill_CarSpawner : MonoBehaviour
{
    public GameObject car;
    public Transform[] spawnPositions;

    private void Start()
    {
        InvokeRepeating("SpawnCar", 2f, 2f);
    }

    private void SpawnCar()
    {
        Transform spawnPos = spawnPositions[Random.Range(0, 2)];
        Instantiate(car, spawnPos.position, spawnPos.rotation);
    }
}