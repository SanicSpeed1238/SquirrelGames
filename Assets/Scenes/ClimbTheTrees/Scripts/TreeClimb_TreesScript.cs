using UnityEngine;

public class TreeClimb_TreesScript : MonoBehaviour
{
    [Header("Movement Values")]
    [Range(0.5f, 1.5f)]
    public float moveSpeed;

    [Header("Important Components")]  
    public GameObject treeObject;

    // Other Variables Needed
    private bool newTreeSpawned;
    private readonly float treesHeight = 16.0f;
    private readonly float destroyHeight = -20f;

    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * -Vector3.up), Space.World);

        if (transform.position.y <= 0.0f && !newTreeSpawned)
        {
            SpawnNewChunk();
            newTreeSpawned = true;
        }
        else if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }

    public void SpawnNewChunk()
    {
        Vector3 newTreeSpawnPosition = new(transform.position.x, transform.position.y + treesHeight, transform.position.z);
        Instantiate(treeObject, newTreeSpawnPosition, Quaternion.identity);
    }
}