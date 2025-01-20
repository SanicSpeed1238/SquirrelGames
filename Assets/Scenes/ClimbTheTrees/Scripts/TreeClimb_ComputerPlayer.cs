using UnityEngine;

public class TreeClimb_ComputerPlayer : TreeClimb_PlayerController
{
    // Variables Needed

    private float currentVerticalInput;
    private float currentVerticalMoveTimer;
    private readonly float minTimeMoving = 1f;
    private readonly float maxTimeMoving = 3f;
    private readonly float maxDistanceBetweenBounds = 2f;

    private float currentJumpTimer;
    private readonly int jumpFrequency = 25;
    
    private void Start()
    {
        currentVerticalInput = 1;
        currentVerticalMoveTimer = Random.Range(minTimeMoving, maxTimeMoving);
    }

    private void LateUpdate()
    {
        if (!TreeClimb_GameManager.Instance.gamePlaying)
            return;

        SetVerticalInput();
        SetHorizontalInput();
    }

    private void SetVerticalInput()
    {
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
        verticalInput = currentVerticalInput;
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
        float playerY = playerRB.position.y;

        if (currentVerticalInput < 0 && playerY - minHeight <= maxDistanceBetweenBounds)
        {
            return true;
        }
        else if (currentVerticalInput > 0 && maxHeight - playerY <= maxDistanceBetweenBounds)
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
        // Jump at random time intervals and randomly choose left/right
        if (currentJumpTimer < 0)
        {
            int randomInputValue = Random.Range(1, 101);
            if (randomInputValue <= jumpFrequency)
            {
                if (Random.value >= 0.5f)
                {
                    horizontalInput = 1;
                }
                else
                {
                    horizontalInput = -1;
                }
                currentJumpTimer = jumpDelay;
            }
            else
            {
                horizontalInput = 0;
                currentJumpTimer = 0.1f;
            }
        }
        else
        {
            currentJumpTimer -= Time.deltaTime;
        }       
    }
}