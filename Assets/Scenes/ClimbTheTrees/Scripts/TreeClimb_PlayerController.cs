using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class TreeClimb_PlayerController : MonoBehaviour
{
    [Header("Movement Values")]
    [Range(10f, 20f)]
    public float moveSpeed;
    [Range(20f, 30f)]
    public float jumpSpeed;
    [Range(0.1f, 0.5f)]
    public float jumpDelay;
    [Range(1f, 3f)]
    public float invincibilityDuration;

    [Header("Important Components")]
    public GameObject squirrelModel;
    public Animator squirrelAnimator;

    // Other Variables Needed
    private float verticalInput;
    private float horizontalInput;
    private bool canInputHorizontal;

    private bool isAlive;
    private bool isFinished;
    private int playerID;
    private int playerScore; 
    
    private readonly float minHeight = -2f;
    private readonly float maxHeight = 14f;
    private LayerMask raycastLayer;
    private Vector3 moveToPosition;
    private GameObject finishPosition;

    private int currentTreeIndex;
    private bool isJumping;
    private bool jumpPerformed;
    private float jumpTimer;
    private Vector3 jumpDirection;
    private Transform[] treePositions;

    private bool isStunned;
    private bool isInvincible;
    private float fallSpeed;
    private Vector3 originalScale;

    private void Start()
    {
        isAlive = true;
        playerScore = 0;

        canInputHorizontal = true;
        jumpTimer = jumpDelay;

        fallSpeed = GameObject.FindAnyObjectByType<TreeClimb_TreesScript>().moveSpeed;
        raycastLayer = LayerMask.GetMask("Player", "Important Objects");

        originalScale = squirrelModel.transform.localScale;     
    }

    private void Update()
    {
        if(isAlive)
        {
            if (!isFinished)
            {
                if (!TreeClimb_GameManager.Instance.gamePlaying) return;

                MoveVertically();
                MoveHorizontally(); 
            }
            else
            {
                MoveToFinishPosition();
            }
        } 
    }

    private void MoveVertically()
    {
        if (isJumping) // If player is in the middle of jumping, cancel up/down movement
            return;

        if (verticalInput != 0 && !isStunned) // Move the player up/down the visible tree space
        {
            moveToPosition = transform.position + moveSpeed * Time.deltaTime * new Vector3(0, verticalInput, 0);
        }
        else
        {
            if(TreeClimb_GameManager.Instance.treesMoving)
                moveToPosition = transform.position + Time.deltaTime * new Vector3(0, -fallSpeed, 0);
        }
        
        // Check if there is another player blocking the direction this player is trying to move in
        moveToPosition.y = Mathf.Clamp(moveToPosition.y, VerifyVerticalMovement(Vector3.down), VerifyVerticalMovement(Vector3.up));
        transform.position = moveToPosition;

        // Check if the player has fallen behind
        if (transform.position.y < minHeight)
        {
            EliminatePlayer();
        }
    }
    private void MoveHorizontally()
    {
        if (isStunned) // If player is stunned, disable jumping
            return;

        // Calculate the direction the player is trying to jump in
        if (horizontalInput == 1) jumpDirection = Vector3.right;
        else if (horizontalInput == -1) jumpDirection = Vector3.left;
        else jumpDirection = Vector3.zero;

        if (jumpDirection != Vector3.zero)
        {
            // Check if there is another player blocking the direction this player is trying to jump in
            // If no other player is blocking this player's jump, check if there is a tree to jump to
            if (VerifyHorizontalMovement(jumpDirection) && !jumpPerformed)
            {
                if(jumpDirection == Vector3.right && currentTreeIndex < (treePositions.Length - 1))
                {
                    currentTreeIndex++;
                    moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, transform.position.y, transform.position.z);
                    jumpPerformed = true;
                    isJumping = true;                   
                }
                else if (jumpDirection == Vector3.left && currentTreeIndex > 0)
                {
                    currentTreeIndex--;
                    moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, transform.position.y, transform.position.z);
                    jumpPerformed = true;
                    isJumping = true;
                }
            }   
        }

        if (isJumping) // Translate player to the new position
        {
            if (transform.position != moveToPosition)
            {
                isInvincible = true;
                float step = jumpSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, moveToPosition, step);                
            }
            else
            {
                isInvincible = false;
                isJumping = false;
                jumpTimer = jumpDelay;
            }
        }
        else // Delay between new jump inputs
        {
            if (jumpTimer > 0) jumpTimer -= Time.deltaTime;
            else jumpPerformed = false;
        }
    }

    private float VerifyVerticalMovement(Vector3 direction)
    {
        float verticalRaycastLength = 1f;
        Vector3 raycastOrigin = transform.position;
        raycastOrigin.y += direction == Vector3.up ? .5f : -.5f;

        //Debug.DrawRay(raycastOrigin, direction * verticalRaycastLength, Color.yellow);

        if (Physics.Raycast(raycastOrigin, direction, out RaycastHit _, verticalRaycastLength, raycastLayer))
        {
            return this.gameObject.transform.position.y;
        }
        else return direction == Vector3.up ? maxHeight : minHeight - 1;
    }
    private bool VerifyHorizontalMovement(Vector3 direction)
    {
        float horizontalRaycastLength = 5.5f;
        Vector3 boxHalfExtents = new(1f, 1f, 1f);
        Vector3 boxOrigin = transform.position;
        boxOrigin.x += direction == Vector3.right ? 0.5f : -0.5f;

        if (Physics.BoxCast(boxOrigin, boxHalfExtents, direction, out RaycastHit _, Quaternion.identity, horizontalRaycastLength, raycastLayer))
        {
            return false;
        }
        else return true;
    }

    public bool CanSquishSquirrel() // Called from HazardScript
    {
        if (!isStunned && !isInvincible) return true;
        else return false;
    }
    public void SquishSquirrel(float stunForce, float squishScale) // Called from HazardScript
    {
        squirrelModel.transform.localScale = new(originalScale.x, originalScale.y, squishScale);
        StartCoroutine(StunSquirrel(stunForce));
    }
    IEnumerator StunSquirrel(float stunDuration)
    {
        isStunned = true;

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;

        LeanTween.scale(squirrelModel, originalScale, .5f).setEase(LeanTweenType.easeOutElastic);
        StartCoroutine(EnableInvincibility());
    }
    IEnumerator EnableInvincibility()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
    }

    public void AddToPlayerScore(int points)
    {
        if (isAlive)
        {
            playerScore += points;
            TreeClimb_GameManager.Instance.UpdateScore(playerID, playerScore);
        }  
    }

    private void EliminatePlayer() 
    {
        isAlive = false;
        verticalInput = 0;
        horizontalInput = 0;

        transform.position = new(0, -10, 0);

        playerScore = 0;
        TreeClimb_GameManager.Instance.UpdateScore(playerID, playerScore);
    }

    private void MoveToFinishPosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, finishPosition.transform.position, moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            isFinished = true;
            TreeClimb_GameManager.Instance.UpdatePlayersFinished(playerID);

            string playerFinishPosition = "Player" + playerID;
            finishPosition = other.gameObject.transform.Find("Finish Positions").gameObject.transform.Find(playerFinishPosition).gameObject;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    public void ConfigurePlayer(int IDNumber, Color playerColor, Transform startPosition) // Initializes squirrel game object
    {
        playerID = IDNumber;
        transform.position = startPosition.position;

        currentTreeIndex = TreeClimb_GameManager.Instance.GetStartingIndex(playerID);
        treePositions = TreeClimb_GameManager.Instance.treePositions;
    }
    public void OnMove(InputAction.CallbackContext context) // Input for moving squirrel
    {
        verticalInput = context.ReadValue<Vector2>().y;

        // Jump input only registered when TAPPING in a direction
        float xInput = context.ReadValue<Vector2>().x;
        if (xInput > .5 && canInputHorizontal)
        {
            horizontalInput = 1;
            canInputHorizontal = false;
        }
        else if (xInput < -.5 && canInputHorizontal)
        {
            horizontalInput = -1;
            canInputHorizontal = false;
        }
        else if (-.1 < xInput && xInput < .1)
        {
            horizontalInput = 0;
            canInputHorizontal = true;
        }
        else
        {
            horizontalInput = 0;
        }
    }
    public void OnAction(InputAction.CallbackContext context) // Action button not used.
    {
        if (!TreeClimb_GameManager.Instance.gameStarted && context.ReadValueAsButton() == true)
        {
            TreeClimb_MinigameSetup.Instance.ConfirmJoin(playerID);
        }
    }
    public void OnStart(InputAction.CallbackContext context) // Input for starting and pausing game
    {
        if (!TreeClimb_GameManager.Instance.gameStarted)
        {
            TreeClimb_MinigameSetup.Instance.StartGame();
        }
    }
}