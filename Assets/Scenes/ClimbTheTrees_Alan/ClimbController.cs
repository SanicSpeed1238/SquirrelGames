using UnityEngine;

public class ClimbController : MonoBehaviour
{
    [Header("Movement Values")]
    public float moveSpeed;
    public float jumpSpeed;
    public float fallSpeed;
    public float maxHeight;

    [Header("Important Components")]
    [SerializeField] private Animator squirrelAnimator;
    [SerializeField] private Transform[] treePositions;

    // Other Variables Needed
    private float verticalInput;
    private float horizontalInput;
    private int currentTreeIndex;
    private bool isJumping;
    private Vector3 moveToPosition;

    private void Start()
    {
        currentTreeIndex = 2;
    }

    private void Update()
    {
        MoveUpAndDown();
        MoveLeftAndRight();
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
            if (!isJumping)
            {
                if(jumpRight && currentTreeIndex < treePositions.Length - 1)
                {
                    currentTreeIndex++;
                    moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, transform.position.y, transform.position.z);
                    isJumping = true;
                }
                else if (!jumpRight && currentTreeIndex > 0)
                {
                    currentTreeIndex--;
                    moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, transform.position.y, transform.position.z);
                    isJumping = true;
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
}