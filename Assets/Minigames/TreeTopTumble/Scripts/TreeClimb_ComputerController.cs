using UnityEngine;

[RequireComponent(typeof(TreeClimb_PlayerController))]
public class TreeClimb_ComputerPlayer : MonoBehaviour
{
    // Variables Needed
    private TreeClimb_PlayerController controller;

    // Variables for Vertical Left Stick input
    private float verticalInput;
    private float verticalMoveTimer;
    
    // Variables for Horizontal Left Stick input
    private float jumpTimer;   

    [Header("Computer Behavior")]
    public float minTimeMoving;
    public float maxTimeMoving;
    public float maxDistanceBetweenBounds;
    public int jumpFrequency;

    private void Awake()
    {
        controller = GetComponent<TreeClimb_PlayerController>();
    }

    private void Start()
    {
        verticalInput = 1;
        verticalMoveTimer = Random.Range(minTimeMoving, maxTimeMoving);
    }

    private void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
            return;

        SetVerticalInput();
        SetHorizontalInput();
    }

    private void SetVerticalInput()
    {
        // GO FOR THE FINISH
        if (TreeClimb_GameManager.Instance.finalTimerStarted)
        {
            verticalInput = 1.0f;
            controller.SetVerticalInput(verticalInput);
            return;
        }

        // Move vertically in one direction for a period of time, then change directions
        if (verticalMoveTimer < 0)
        {
            SwitchVerticalInput();
        }
        else
        {
            // If computer is too close to the bottom/top, change directions
            if (CheckVerticalDistance())
                SwitchVerticalInput();

            verticalMoveTimer -= Time.deltaTime;
        }
        controller.SetVerticalInput(verticalInput);
    }
    private void SwitchVerticalInput()
    {
        if (verticalInput < 0)
        {
            verticalInput = Random.Range(0.1f, 1.0f);
            verticalMoveTimer = Random.Range(minTimeMoving, maxTimeMoving);
        }
        else if (verticalInput > 0)
        {
            verticalInput = Random.Range(-1.0f, -0.1f);
            verticalMoveTimer = Random.Range(minTimeMoving, maxTimeMoving);
        }
    }
    private bool CheckVerticalDistance()
    {
        float playerY = transform.position.y;

        if (verticalInput < 0 && playerY - (controller.minHeight + 1) <= maxDistanceBetweenBounds)
        {
            return true;
        }
        else if (verticalInput > 0 && controller.maxHeight - playerY <= maxDistanceBetweenBounds)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SetHorizontalInput()
    {
        // GO FOR THE FINISH
        if (TreeClimb_GameManager.Instance.finalTimerStarted)
        {
            controller.SetHorizontalInput(0);
            return;
        }

        // Jump at random time intervals and randomly choose left/right
        if (jumpTimer < 0)
        {
            jumpTimer = 0.01f;
            int randomInputValue = Random.Range(1, 101);

            // Jump to stay off a tree on the edge
            if (controller.GetBlockedState())
            {
                if (controller.GetCurrentTreeIndex() == TreeClimb_GameManager.Instance.treeLanesRemoved)
                    controller.SetHorizontalInput(1);
                else if (controller.GetCurrentTreeIndex() == TreeClimb_GameManager.Instance.treePositions.Length - (1 + TreeClimb_GameManager.Instance.treeLanesRemoved))
                    controller.SetHorizontalInput(-1);
            }
            // Jump to a tree randomly to the left/right
            else if (randomInputValue <= jumpFrequency)
            {
                if (Random.value >= 0.5f) controller.SetHorizontalInput(1);
                else controller.SetHorizontalInput(-1);

                if (controller.GetCurrentTreeIndex() == 0 + TreeClimb_GameManager.Instance.treeLanesRemoved) 
                    controller.SetHorizontalInput(1);
                else if (controller.GetCurrentTreeIndex() == TreeClimb_GameManager.Instance.treePositions.Length - (1 + TreeClimb_GameManager.Instance.treeLanesRemoved)) 
                    controller.SetHorizontalInput(-1);    
            }
            // If chosen to not jump, stay on current tree
            else
            {
                controller.SetHorizontalInput(0);
            }
        }
        else
        {
            jumpTimer -= Time.deltaTime;
        }       
    }
}