using UnityEngine;
using System.Collections;

public class TreeClimb_ObjectSpawner : MonoBehaviour
{
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
            yield return new WaitUntil(() => TreeClimb_GameManager.Instance.gamePlaying);
            yield return new WaitUntil(() => TreeClimb_GameManager.Instance.objectsSpawning);

            yield return new WaitForSeconds(spawnTimer - Random.Range(0f, 0.15f));

            GameObject newCollectable = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
            Vector3 spawnPosition = transform.position;
            spawnPosition.x = TreeClimb_GameManager.Instance.treePositions[Random.Range(0, TreeClimb_GameManager.Instance.treePositions.Length)].transform.position.x;
            Instantiate(newCollectable, spawnPosition, Quaternion.identity);
        }
    }
}