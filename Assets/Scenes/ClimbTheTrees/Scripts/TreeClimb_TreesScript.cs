using UnityEngine;

public class TreeClimb_TreesScript : MonoBehaviour
{
    [Header("Important Components")]
    public GameObject treeObject;
    public string treeSpawnerName;

    // Other Variables Needed
    private bool newTreeSpawned;

    private void Update()
    {
        if (transform.position.y <= 0.01f && !newTreeSpawned)
        {
            SpawnNewChunk();
            newTreeSpawned = true;
        }
    }

    public void SpawnNewChunk()
    {
        Transform newTreeSpawnPosition = GameObject.Find(treeSpawnerName).transform;
        Instantiate(treeObject, newTreeSpawnPosition.position, Quaternion.identity);
    }
}