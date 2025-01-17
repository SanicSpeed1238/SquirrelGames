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
    private readonly float destroyHeight = -25f;
    private readonly float stopHeight = -4f;
    private bool newTreeSpawned;
    private Rigidbody treeRB;

    private void Start()
    {
        treeRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Moves the set of trees down
        if (TreeClimb_GameManager.Instance.gamePlaying || treeGroupType == TreeGroup.Goal)
        {
            Vector3 movement = moveSpeed * Time.fixedDeltaTime * Vector3.down;
            treeRB.MovePosition(treeRB.position + movement);
        }
          
        /*
        // Performs a function based on the TreeGroup type once this object has fallen below a certain height
        */

        // Spawns the next chunk of trees based on the game manager
        if (treeRB.position.y <= 0.0f && !newTreeSpawned)
        {
            SpawnNewChunk();            
        }
        // Destroys the gameobject if it has gone too low on the y-axis
        else if (treeRB.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
        // If this tree group is a Goal, stop moving the trees, as well as the player, down automatically
        else if (treeRB.position.y <= stopHeight && treeGroupType == TreeGroup.Goal)
        {
            moveSpeed = 0f;
            TreeClimb_GameManager.Instance.treesMoving = false;
        }
    }

    public void SpawnNewChunk()
    {
        newTreeSpawned = true;
        GameObject nextTreeGroup = TreeClimb_GameManager.Instance.GetNextTreeGroup();

        if (nextTreeGroup != null)
        {
            Vector3 newTreeSpawnPosition = new(treeRB.position.x, treeRB.position.y + treesHeight, treeRB.position.z);
            Instantiate(nextTreeGroup, newTreeSpawnPosition, Quaternion.identity);

            if (nextTreeGroup.GetComponent<TreeClimb_TreesScript>().treeGroupType == TreeGroup.Treetops)
            {
                TreeClimb_GameManager.Instance.RemoveTreeLanes();
            }
        }     
    }
}