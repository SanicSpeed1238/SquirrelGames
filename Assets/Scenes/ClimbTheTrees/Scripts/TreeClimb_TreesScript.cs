using UnityEngine;

public class TreeClimb_TreesScript : MonoBehaviour
{
    [Header("Movement Values")]
    [Range(1f, 3f)]
    public float moveSpeed;

    [Header("Tree Group Type")]
    public bool hasTreeTop;

    // Other Variables Needed
    private TreeClimb_GameManager gameManager;
    private readonly float treesHeight = 16.0f;
    private readonly float destroyHeight = -20f;
    private bool newTreeSpawned;

    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * -Vector3.up), Space.World);

        if (transform.position.y <= 0.0f && !newTreeSpawned)
        {
            SpawnNewChunk();
            newTreeSpawned = true;

            if (hasTreeTop)
                gameManager.RemoveTreeLanes();
        }
        else if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }

    public void SpawnNewChunk()
    {  
        gameManager = GameObject.FindAnyObjectByType<TreeClimb_GameManager>();
        GameObject nextTreeGroup = gameManager.GetNextTreeGroup();

        if (nextTreeGroup != null)
        {
            Vector3 newTreeSpawnPosition = new(transform.position.x, transform.position.y + treesHeight, transform.position.z);
            Instantiate(nextTreeGroup, newTreeSpawnPosition, Quaternion.identity);
        }     
    }
}