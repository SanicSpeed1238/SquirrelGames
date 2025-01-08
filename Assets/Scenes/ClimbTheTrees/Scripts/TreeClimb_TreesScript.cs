using UnityEngine;

public class TreeClimb_TreesScript : MonoBehaviour
{
    public enum TreeGroup
    {
        Trunks,
        Treetops,
        Goal
    }
    [Header("Tree Group Type")]
    public TreeGroup treeGroupType = TreeGroup.Trunks;

    [Header("Movement Values")]
    [Range(1f, 3f)]
    public float moveSpeed;

    // Other Variables Needed
    private TreeClimb_GameManager gameManager;
    private readonly float treesHeight = 15f;
    private float destroyHeight = -20f;
    private bool newTreeSpawned;

    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * -Vector3.up), Space.World);

        if (transform.position.y <= 0.0f && !newTreeSpawned)
        {
            SpawnNewChunk();
            newTreeSpawned = true;

            if (treeGroupType == TreeGroup.Treetops)
                gameManager.RemoveTreeLanes();
            else if (treeGroupType == TreeGroup.Goal)
                destroyHeight /= 2f;
        }
        else if (transform.position.y < destroyHeight)
        {
            if (treeGroupType == TreeGroup.Goal)
            {
                moveSpeed = 0f;
                this.enabled = false;
                return;
            }

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