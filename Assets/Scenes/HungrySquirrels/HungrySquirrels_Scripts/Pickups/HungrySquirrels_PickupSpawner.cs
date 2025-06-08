using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_PickupSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private HungrySquirrels_GameController _gController;

    [Header("Spawn Settings")]
    [SerializeField] private Pickup[] _possiblePickupsToSpawn;
    [SerializeField] private float _spawnRate = 1;
    [SerializeField] private float _timeBetweenSpawnRateChange = 10;
    [SerializeField] private float _spawnRateReductionAmount = 0.1f;

    private bool _gameOn;
    private Coroutine _spawnRoutine;

    //private void 

    private IEnumerator SpawnRoutine()
    {
        Vector3 spawnPoint;

        while (_gameOn != true)
        {
            yield return new WaitForSeconds(_spawnRate);

            if (_gameOn == true)
            {

            }

        }
    }

    public void StartSpawning()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);
    }

    public Vector3 GetValidWorldSpawnPoint()
    {
        Vector3 randPoint = Random.insideUnitSphere.normalized;

        return randPoint;//Temporary until I can figure out the best formula to create the spawn positions
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
