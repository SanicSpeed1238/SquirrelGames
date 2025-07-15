using UnityEngine;

public class TreeClimb_TreesScript : MonoBehaviour
{
    public enum TreeGroup
    {
        Undefined,
        Trunks,
        Treetops,
        Goal
    }
    public TreeGroup treeGroupType = TreeGroup.Undefined;

    // Other Variables Needed
    private Rigidbody treeRB;
    private float moveSpeed;
    private bool newTreeSpawned;

    // Height Variables
    private readonly float treesHeight = 19.825f;
    private readonly float destroyHeight = -25f;
    private readonly float stopHeight = 4f;
    private readonly float finishHeight = -2.5f;

    private void Start()
    {
        treeRB = GetComponent<Rigidbody>();
        moveSpeed = TreeClimb_GameManager.Instance.gameMoveSpeed;
    }

    private void FixedUpdate()
    {
        // Moves the set of trees down
        if ((MG_GameManager.Instance.gamePlaying && TreeClimb_GameManager.Instance.treesMoving) || treeGroupType == TreeGroup.Goal)
        {
            Vector3 movement = moveSpeed * Time.fixedDeltaTime * Vector3.down;
            treeRB.MovePosition(treeRB.position + movement);
        }
          
        // Performs a function based on the TreeGroup type once this object has fallen below a certain height
        if (treeGroupType == TreeGroup.Trunks || treeGroupType == TreeGroup.Treetops)
        {
            if (treeRB.position.y <= 0.0f)
            {
                if (!newTreeSpawned)
                {
                    SpawnNewChunk();
                }
                else if (treeRB.position.y < destroyHeight)
                {
                    Destroy(gameObject);
                }
            }
        }
        else if (treeGroupType == TreeGroup.Goal)
        {
            if (treeRB.position.y <= stopHeight)
            {
                if (TreeClimb_GameManager.Instance.objectsSpawning)
                {
                    TreeClimb_GameManager.Instance.objectsSpawning = false;
                }
                else if (TreeClimb_GameManager.Instance.treesMoving && treeRB.position.y <= finishHeight)
                {
                    moveSpeed = 0f;
                    TreeClimb_GameManager.Instance.treesMoving = false;
                    TreeClimb_GameManager.Instance.finalTimerStarted = true;
                }
            }         
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
                TreeClimb_GameManager.Instance.treeLanesRemoved++;
            }
        }     
    }
}