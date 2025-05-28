using UnityEngine;
using System.Collections;

public class TreeClimb_ObjectSpawner : MonoBehaviour
{
    public enum SpawnerType
    {
        None,
        Acorn,
        Hazard,
    }

    [Header("Spawner Type")]
    public SpawnerType spawnerType;

    [Header("Spawn Attributes")]
    [Range(0.1f, 2.0f)]
    public float spawnTimer;

    [Header("Important Components")]
    public GameObject[] objectPrefabs;

    private void Start()
    {
        StartCoroutine(SpawnObject());
    }

    IEnumerator SpawnObject()
    {
        while (true)
        {
            yield return new WaitUntil(() => CS_GameManager.Instance.gamePlaying);
            yield return new WaitUntil(() => TreeClimb_GameManager.Instance.objectsSpawning);

            yield return new WaitForSeconds(spawnTimer - Random.Range(0f, 0.15f));

            GameObject newObject = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
            
            Vector3 spawnPosition = transform.position;
            int treeIndex = TreeClimb_GameManager.Instance.GetRandomTreeLane();
            spawnPosition.x = TreeClimb_GameManager.Instance.treePositions[treeIndex].transform.position.x;
            
            GameObject spawnedObject = Instantiate(newObject, spawnPosition, Quaternion.identity);

            if (spawnerType == SpawnerType.Hazard)
            {
                spawnedObject.GetComponent<TreeClimb_HazardScript>().SetTreeIndex(treeIndex);
            }
        }
    }
}