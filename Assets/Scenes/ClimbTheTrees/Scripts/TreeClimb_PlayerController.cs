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
    private int playerID;
    private TreeClimb_GameManager gameManager;
    private TreeClimb_MinigameSetup minigameSetup;

    private int playerScore;
    private bool isAlive;
    private float verticalInput;
    private float horizontalInput; 
    
    private readonly float minHeight = -1f;
    private readonly float maxHeight = 14f;
    private LayerMask raycastLayer;
    private Vector3 moveToPosition;

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
       
        jumpTimer = jumpDelay;
        fallSpeed = GameObject.FindAnyObjectByType<TreeClimb_TreesScript>().moveSpeed;
        originalScale = squirrelModel.transform.localScale;       
        raycastLayer = LayerMask.GetMask("Player", "Important Objects");
    }

    private void Update()
    {
        if(isAlive)
        {
            MoveVertically();
            MoveHorizontally();

            CheckIfEliminated();
        }  
    }

    private void MoveVertically()
    {
        if (isJumping) // If player is in the middle of jumping, cancel up/down movement
            return;

        verticalInput = Input.GetAxis("Vertical");

        if (verticalInput != 0 && !isStunned) // Move the player up/down the visible tree space
        {
            moveToPosition = transform.position + moveSpeed * Time.deltaTime * new Vector3(0, verticalInput, 0);
            squirrelAnimator.speed = 1;
        }
        else
        {
            moveToPosition = transform.position + Time.deltaTime * new Vector3(0, -fallSpeed, 0);
            squirrelAnimator.speed = 0;
        }
        
        // Check if there is another player blocking the direction this player is trying to move in
        moveToPosition.y = Mathf.Clamp(moveToPosition.y, VerifyVerticalMovement(Vector3.down), VerifyVerticalMovement(Vector3.up));
        transform.position = moveToPosition;
    }
    private void MoveHorizontally()
    {
        if (isStunned) // If player is stunned, disable jumping
            return;

        // Jump input only registered when tapping the left stick in a specific direction
        if (Input.GetKeyDown(KeyCode.D)) horizontalInput = 1;
        else if (Input.GetKeyDown(KeyCode.A)) horizontalInput = -1;
        else horizontalInput = 0;

        // Calculate the direction the player is trying to jump in
        if (horizontalInput > .5f) jumpDirection = Vector3.right;
        else if (horizontalInput < -.5f) jumpDirection = Vector3.left;
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

    private void CheckIfEliminated() 
    {
        // Player is out when they fall below the minHeight
        if (transform.position.y < minHeight)
        {
            isAlive = false;

            verticalInput = 0;
            horizontalInput = 0;

            transform.position = new(0, -10, 0);
        }
    }

    public void ConfigurePlayer(int IDNumber, Color playerColor, Transform startPosition) // Initializes squirrel game object
    {
        gameManager = GameObject.FindAnyObjectByType<TreeClimb_GameManager>();
        minigameSetup = GameObject.FindAnyObjectByType<TreeClimb_MinigameSetup>();

        playerID = IDNumber;
        transform.position = startPosition.position;

        playerScore = 0;
        currentTreeIndex = gameManager.GetStartingIndex(playerID);
        treePositions = gameManager.treePositions;
    }
    public void OnMove(InputAction.CallbackContext context) // Input for moving squirrel
    {
        // = context.ReadValue<Vector2>();
    }
    public void OnAction(InputAction.CallbackContext context) // Action button not used.
    {
        // = context.ReadValueAsButton();

        if (!gameManager.gameStarted && context.ReadValueAsButton() == true)
        {
            minigameSetup.ConfirmJoin(playerID);
        }
    }
    public void OnStart(InputAction.CallbackContext context) // Input for starting and pausing game
    {
        if (!gameManager.gameStarted)
        {
            minigameSetup.StartGame();
        }
    }
}