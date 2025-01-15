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
    private readonly float treesHeight = 15f;
    private float destroyHeight = -25f;
    private bool newTreeSpawned;
    private Rigidbody treeRB;

    private void Start()
    {
        treeRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!TreeClimb_GameManager.Instance.gamePlaying) return;

        // Moves the set of trees down
        Vector3 movement = moveSpeed * Time.fixedDeltaTime * Vector3.down;
        treeRB.MovePosition(treeRB.position + movement);

        // Performs a function based on the TreeGroup type once this object has fallen below a certain height
        if (treeRB.position.y <= 0.0f && !newTreeSpawned)
        {
            SpawnNewChunk();
            newTreeSpawned = true;

            if (treeGroupType == TreeGroup.Treetops)
                TreeClimb_GameManager.Instance.RemoveTreeLanes();
            else if (treeGroupType == TreeGroup.Goal)
                destroyHeight /= 2f;
        }
        else if (treeRB.position.y < destroyHeight)
        {
            if (treeGroupType == TreeGroup.Goal)
            {
                moveSpeed = 0f;
                TreeClimb_GameManager.Instance.treesMoving = false;

                this.enabled = false;
                return;
            }

            Destroy(gameObject);
        }
    }

    public void SpawnNewChunk()
    {  
        GameObject nextTreeGroup = TreeClimb_GameManager.Instance.GetNextTreeGroup();

        if (nextTreeGroup != null)
        {
            Vector3 newTreeSpawnPosition = new(treeRB.position.x, treeRB.position.y + treesHeight, treeRB.position.z);
            Instantiate(nextTreeGroup, newTreeSpawnPosition, Quaternion.identity);
        }     
    }
}