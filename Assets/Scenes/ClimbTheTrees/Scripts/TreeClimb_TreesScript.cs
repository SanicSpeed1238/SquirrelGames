using UnityEngine;

public class TreeClimb_TreesScript : MonoBehaviour
{
    [Header("Movement Values")]
    public float moveSpeed;

    [Header("Important Components")]  
    public GameObject treeObject;

    // Other Variables Needed
    private bool newTreeSpawned;
    private readonly float treesHeight = 8.0f;
    private readonly float destroyHeight = -10f;

    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * -transform.up), Space.World);

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
        Vector3 newTreeSpawnPosition = new Vector3(transform.position.x, transform.position.y + treesHeight, transform.position.z);
        Instantiate(treeObject, newTreeSpawnPosition, Quaternion.identity);
    }
}