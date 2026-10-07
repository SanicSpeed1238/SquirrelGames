using UnityEngine;
using System.Collections;

public class TreeClimb_ObjectSpawner : MonoBehaviour
{
    public static TreeClimb_ObjectSpawner Instance;

    [Header("Acorn Variables")]
    [Range(0.1f, 2.0f)]
    public float acornSpawnTimer;
    public GameObject[] acornPrefabs;

    [Header("Hazard Variables")]
    [Range(0.1f, 2.0f)]
    public float hazardSpawnTimer;
    public GameObject[] hazardPrefabs;

    // Other Variables Needed
    private bool[] availableTreeLanes;

    private void Start()
    {
        Instance = this;

        availableTreeLanes = new bool[7];
        for (int i = 0; i < availableTreeLanes.Length; i++) {availableTreeLanes[i] = true;}

        StartCoroutine(SpawnAcorns());
        StartCoroutine(SpawnHazards());
    }

    IEnumerator SpawnAcorns()
    {
        while (true)
        {
            yield return new WaitUntil(() => MG_GameManager.Instance.gamePlaying);
            yield return new WaitUntil(() => TreeClimb_GameManager.Instance.objectsSpawning);

            float spawnTimer = acornSpawnTimer - Random.Range(0f, 0.2f);
            yield return new WaitForSeconds(spawnTimer);

            Vector3 spawnPosition = transform.position;
            int treeIndex = TreeClimb_GameManager.Instance.GetRandomTreeLane();
            spawnPosition.x = TreeClimb_GameManager.Instance.treePositions[treeIndex].transform.position.x;
            
            if (availableTreeLanes[treeIndex])
            {
                GameObject newObject = acornPrefabs[Random.Range(0, acornPrefabs.Length)];
                GameObject spawnedObject = Instantiate(newObject, spawnPosition, Quaternion.identity);

                StartCoroutine(UseTree(treeIndex, (Random.Range(0.0f, 0.2f))));
            }
        }
    }

    IEnumerator SpawnHazards()
    {
        while (true)
        {
            yield return new WaitUntil(() => MG_GameManager.Instance.gamePlaying);
            yield return new WaitUntil(() => TreeClimb_GameManager.Instance.objectsSpawning);

            float spawnTimer = hazardSpawnTimer - Random.Range(0f, 0.2f);
            yield return new WaitForSeconds(spawnTimer);

            Vector3 spawnPosition = transform.position;
            int treeIndex = TreeClimb_GameManager.Instance.GetRandomTreeLane();
            spawnPosition.x = TreeClimb_GameManager.Instance.treePositions[treeIndex].transform.position.x;

            if (availableTreeLanes[treeIndex])
            {
                GameObject newObject = hazardPrefabs[Random.Range(0, hazardPrefabs.Length)];
                GameObject spawnedObject = Instantiate(newObject, spawnPosition, Quaternion.identity);

                StartCoroutine(UseTree(treeIndex, (0.1f + Random.Range(0.0f, 0.5f))));
                spawnedObject.GetComponent<TreeClimb_HazardScript>().SetTreeIndex(treeIndex);
            }
        }
    }

    IEnumerator UseTree(int treeIndex, float treeTimer)
    {
        availableTreeLanes[treeIndex] = false;
        yield return new WaitForSeconds(treeTimer);
        availableTreeLanes[treeIndex] = true;
    }
}