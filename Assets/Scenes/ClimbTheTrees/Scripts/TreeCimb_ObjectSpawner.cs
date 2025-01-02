using UnityEngine;
using System.Collections;

public class TreeClimb_ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Attributes")]
    [Range(0.5f, 3.0f)]
    public float spawnTime;

    [Header("Important Components")]
    public GameObject[] objectPrefabs;

    // Other Variables Needed
    private TreeClimb_GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.FindAnyObjectByType<TreeClimb_GameManager>();
        StartCoroutine(SpawnAcorn());
    }

    IEnumerator SpawnAcorn()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnTime - Random.Range(0f, 0.15f));

            GameObject newCollectable = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
            Vector3 spawnPosition = transform.position;
            spawnPosition.x = gameManager.treePositions[Random.Range(0, gameManager.treePositions.Length)].transform.position.x;
            Instantiate(newCollectable, spawnPosition, Quaternion.identity);
        }
    }
}