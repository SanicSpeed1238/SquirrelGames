using UnityEngine;

public class TreeClimb_PlayerController : MonoBehaviour
{
    [Header("Movement Values")]
    public float moveSpeed;
    public float jumpSpeed;
    public float jumpCoolDown;
    public float knockBackSpeed;
    public float maxHeight;

    [Header("Important Components")]
    public Animator squirrelAnimator;
    public Transform[] treePositions;

    // Other Variables Needed
    private float verticalInput;
    private float horizontalInput;

    private int currentTreeIndex;
    private bool isJumping;
    private bool justJumped;
    private float jumpTimer;

    private bool isStunned;
    private float stunTimer;

    private Vector3 moveToPosition;
    private float fallSpeed;

    private void Start()
    {
        currentTreeIndex = 3;
        jumpTimer = jumpCoolDown;

        stunTimer = 0.0f;

        fallSpeed = GameObject.FindAnyObjectByType<TreeClimb_TreesScript>().moveSpeed;
    }

    private void Update()
    {
        if(!isStunned)
        {
            MoveLeftAndRight();
            MoveUpAndDown();           
            JumpCooldown();
        }
        else
        {
            StunSquirrel();
        }      
    }

    private void MoveUpAndDown()
    {
        if (isJumping)
        {
            squirrelAnimator.speed = 0;
            return;
        }

        verticalInput = Input.GetAxis("Vertical");

        if (verticalInput != 0)
        {
            moveToPosition = transform.position + moveSpeed * Time.deltaTime * new Vector3(0, verticalInput, 0);
            squirrelAnimator.speed = 1;
        }
        else
        {
            moveToPosition = transform.position + Time.deltaTime * new Vector3(0, -fallSpeed, 0);
            squirrelAnimator.speed = 0;
        }

        moveToPosition.y = Mathf.Clamp(moveToPosition.y, -1, maxHeight);
        transform.position = moveToPosition;
    }

    private void MoveLeftAndRight()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        bool jumpRight = horizontalInput > 0;

        if (horizontalInput != 0)
        {
            if (!justJumped)
            {
                if(jumpRight && currentTreeIndex < treePositions.Length - 1)
                {
                    currentTreeIndex++;
                    moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, transform.position.y, transform.position.z);
                    isJumping = true;
                    justJumped = true;
                }
                else if (!jumpRight && currentTreeIndex > 0)
                {
                    currentTreeIndex--;
                    moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, transform.position.y, transform.position.z);
                    isJumping = true;
                    justJumped = true;
                }
            }
        }

        if (isJumping)
        {
            if (transform.position != moveToPosition)
            {
                float step = jumpSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, moveToPosition, step);
            }
            else
            {
                isJumping = false;
            }
        }
    }

    private void JumpCooldown()
    {
        if (justJumped)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer < 0)
            {
                jumpTimer = jumpCoolDown;
                justJumped = false;
            }
        }
    }

    public void SquishSquirrel(float stunDuration)
    {
        if(!isStunned)
        {
            fallSpeed *= knockBackSpeed;
            stunTimer = stunDuration;
            isStunned = true;
        }
    }
    private void StunSquirrel()
    {
        if(stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;

            moveToPosition = transform.position + Time.deltaTime * new Vector3(0, -fallSpeed, 0);
            transform.position = moveToPosition;
            squirrelAnimator.speed = 0;
        }
        else
        {
            fallSpeed /= knockBackSpeed;
            jumpTimer = jumpCoolDown;
            justJumped = false;
            isStunned = false;
        }
    }
}