using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_PickupSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private HungrySquirrels_GameController _gController;

    [Header("Spawn Settings")]
    [SerializeField] private Pickup[] _possiblePickupsToSpawn;
    //[SerializeField] private float _startingDelay = 60;
    [SerializeField] private float _spawnRate = 1;
    [SerializeField] private float _spawnRadiusMax = 20f;
    [SerializeField] private LayerMask _layerToTest;
    private int _maxSpawnAttempts = 4;

    private bool _gameOver = false;
    private bool _panicMode = false;
    private Coroutine _spawnRoutine;


    public void IncreaseSpawnRate()
    {
        _spawnRate -= 0.1f;
    }

    public void ActivateRush()
    {
        _panicMode = true;
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
            if(_panicMode == true)
            {
                Pickup bomb = Instantiate(pickup, positon, Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
            }
            else
            {
                int coinflip = Random.Range(0, 5);
                if (coinflip >= 2)
                {
                    Pickup bomb = Instantiate(pickup, positon, Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
                }
                else
                {
                    Pickup bolt = Instantiate(_possiblePickupsToSpawn[2], positon, Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
                }
            }

        }
        else if(pickup == _possiblePickupsToSpawn[1])
        {
            if (_panicMode == true)
            {
                Pickup acorn = Instantiate(pickup, positon, Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
            }
            else
            {
                int coinflip = Random.Range(0, 5);
                if (coinflip >= 2)
                {
                    Pickup acorn = Instantiate(pickup, positon, Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
                }
                else
                {
                    Pickup bolt = Instantiate(_possiblePickupsToSpawn[2], positon, Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
                }
            }
        }
        else if (pickup == _possiblePickupsToSpawn[2])
        {
            Pickup bolt = Instantiate(_possiblePickupsToSpawn[2], positon, Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
        }
    }

    private Vector3 RandomRotation()
    {
        Vector3 ranRot;

        ranRot.x = Random.Range(-5, 5);
        ranRot.y = Random.Range(-5, 5);
        ranRot.z = Random.Range(-5, 5);

        return ranRot;
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
