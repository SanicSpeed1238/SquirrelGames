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
    [SerializeField] private float _spawnRadiusMax = 20f;
    [SerializeField] private LayerMask _layerToTest;
    private int _maxSpawnAttempts = 4;

    private bool _gameOver = false;
    private Coroutine _spawnRoutine;

    private void Start()
    {
        SpawnRoutine();
    }

    private void Update()
    {
        if (MG_GameManager.Instance.gamePlaying)
        {
            Debug.Log("Update Spawner");
        }
        
    }

    private IEnumerator SpawnRoutine()
    {
        
        Vector3 spawnPoint;

        while (_gameOver != true)
        {
            yield return new WaitForSeconds(_spawnRate);
            
            if (_gameOver == true)
            {
                StopSpawning();
                yield break;
            }
            spawnPoint = GetValidWorldSpawnPoint();

            if(spawnPoint != Vector3.zero)
            {
                Spawn(ChoosePickup(), spawnPoint);
            }

        }
    }

    public void Spawn(Pickup pickup, Vector3 positon)
    {
        if(pickup == _possiblePickupsToSpawn[0])
        {
            int coinflip = Random.Range(0, 5);
            if(coinflip >= 3)
            {
                Pickup bomb = Instantiate(pickup, positon, Quaternion.identity);
            }
            else
            {
                Pickup acorn = Instantiate(_possiblePickupsToSpawn[1], positon, Quaternion.identity);
            }
        }
        else if(pickup == _possiblePickupsToSpawn[1])
        {
            Pickup acorn = Instantiate(pickup, positon, Quaternion.identity);
            //Debug.Log("Lil Treat");
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

        Vector3 randDirection = new Vector3(Random.insideUnitSphere.x, 0, Random.insideUnitSphere.z).normalized;

        Vector3 spawnPos = this.transform.position;

        Vector3 testPoint = spawnPos + (randDirection * Random.Range(0, _spawnRadiusMax));

        Collider[] colliders = Physics.OverlapSphere(testPoint, .5f, _layerToTest);

        for (int i = 0; i< _maxSpawnAttempts; i++)
        {
            Debug.Log(testPoint);
            if (colliders.Length<1)
            {
                return testPoint;
            }
            else
            {
                testPoint = new Vector3(testPoint.z, this.transform.position.y, -testPoint.x);
            }
        }

        return testPoint = Vector3.zero;
    }

    private Pickup ChoosePickup()
    {
        int randomPickIndex;
        randomPickIndex = Random.Range(0, _possiblePickupsToSpawn.Length);

        return _possiblePickupsToSpawn[randomPickIndex];
    }

}
