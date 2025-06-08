using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_PickupSpawner : MonoBehaviour
{

    [Header("Spawn Settings")]
    [SerializeField] private Pickup[] _possiblePickupsToSpawn;
    [SerializeField] private float _spawnRate = 1;
    [SerializeField] private float _timeBetweenSpawnRateChange = 10;
    [SerializeField] private float _spawnRateReductionAmount = 0.1f;
    [SerializeField] private float _gameTime;
    private float _eTime;

    private Coroutine _spawnRoutine;

    private IEnumerator SpawnRoutine()
    {
        Vector3 spawnPoint;

        while (_eTime < _gameTime)
        {
            yield return new WaitForSeconds(_spawnRate);

            if (_eTime >= _gameTime)
            {

            }

        }
    }



    private Pickup ChoosePickup()
    {
        int randomPickIndex;
        randomPickIndex = Random.Range(0, _possiblePickupsToSpawn.Length);

        return _possiblePickupsToSpawn[randomPickIndex];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
