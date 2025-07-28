using UnityEngine;

[RequireComponent(typeof(TreeClimb_PlayerController))]
public class TreeClimb_ComputerPlayer : MonoBehaviour
{
    // Variables Needed
    private TreeClimb_PlayerController controller;

    // Variables for Vertical Left Stick input
    private float currentVerticalInput;
    private float currentVerticalMoveTimer;
    
    // Variables for Horizontal Left Stick input
    private float currentJumpTimer;   

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
        currentVerticalInput = 1;
        currentVerticalMoveTimer = Random.Range(minTimeMoving, maxTimeMoving);
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
        if (TreeClimb_GameManager.Instance.finalTimerStarted)
        {
            currentVerticalInput = 1.0f;
            controller.SetVerticalInput(currentVerticalInput);
            return;
        }

        // Move vertically in one direction for a period of time, then change directions
        if (currentVerticalMoveTimer < 0)
        {
            SwitchVerticalInput();
        }
        else
        {
            // If computer is too close to the bottom/top, change directions
            if (CheckVerticalDistance())
                SwitchVerticalInput();

            currentVerticalMoveTimer -= Time.deltaTime;
        }
        controller.SetVerticalInput(currentVerticalInput);
    }
    private void SwitchVerticalInput()
    {
        if (currentVerticalInput < 0)
        {
            currentVerticalInput = Random.Range(0.1f, 1.0f);
            currentVerticalMoveTimer = Random.Range(minTimeMoving, maxTimeMoving);
        }
        else if (currentVerticalInput > 0)
        {
            currentVerticalInput = Random.Range(-1.0f, -0.1f);
            currentVerticalMoveTimer = Random.Range(minTimeMoving, maxTimeMoving);
        }
    }
    private bool CheckVerticalDistance()
    {
        float playerY = transform.position.y;

        if (currentVerticalInput < 0 && playerY - (controller.minHeight + 1) <= maxDistanceBetweenBounds)
        {
            return true;
        }
        else if (currentVerticalInput > 0 && controller.maxHeight - playerY <= maxDistanceBetweenBounds)
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
        if (TreeClimb_GameManager.Instance.finalTimerStarted)
        {
            controller.SetHorizontalInput(0);
            return;
        }

        // Jump at random time intervals and randomly choose left/right
        if (currentJumpTimer < 0)
        {
            currentJumpTimer = 0.01f;
            int randomInputValue = Random.Range(1, 101);

            if (controller.GetBlockedState())
            {
                if (controller.GetCurrentTreeIndex() == TreeClimb_GameManager.Instance.treeLanesRemoved)
                    controller.SetHorizontalInput(1);
                else if (controller.GetCurrentTreeIndex() == TreeClimb_GameManager.Instance.treePositions.Length - (1 + TreeClimb_GameManager.Instance.treeLanesRemoved))
                    controller.SetHorizontalInput(-1);
            }
            else if (randomInputValue <= jumpFrequency)
            {
                if (Random.value >= 0.5f) controller.SetHorizontalInput(1);
                else controller.SetHorizontalInput(-1);

                if (controller.GetCurrentTreeIndex() == 0 + TreeClimb_GameManager.Instance.treeLanesRemoved) 
                    controller.SetHorizontalInput(1);
                else if (controller.GetCurrentTreeIndex() == TreeClimb_GameManager.Instance.treePositions.Length - (1 + TreeClimb_GameManager.Instance.treeLanesRemoved)) 
                    controller.SetHorizontalInput(-1);    
            }
            else
            {
                controller.SetHorizontalInput(0);
            }
        }
        else
        {
            currentJumpTimer -= Time.deltaTime;
        }       
    }
}