using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System;

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

    private bool isAlive;
    private bool isFinished;
    private int playerID;
    private int playerScore; 
    
    private Vector3 moveToPosition;
    private Rigidbody playerRB;
    private readonly float minHeight = -2f;
    private readonly float maxHeight = 14f;

    private int currentTreeIndex;
    private bool isJumping;
    private float jumpTimer;
    private Vector3 jumpDirection;
    private Transform[] treePositions;
    private GameObject finishPosition;

    private float fallSpeed;
    private bool isStunned;
    private bool isInvincible;  
    private Vector3 originalScale;

    private LayerMask playerLayer;
    private LayerMask treeLayer;

    private void FixedUpdate()
    {
        if(isAlive)
        {
            if (!isFinished)
            {
                if (!TreeClimb_GameManager.Instance.gamePlaying)
                    return;

                MoveVertically();
                MoveHorizontally(); 
            }
            else
            {
                MoveToFinishPosition();
            }
        } 
    }

    private void MoveVertically() // Moves up and down the tree the player is currently on
    {
        if (isJumping) // If the player is in the middle of jumping, ignore vertical input
        {
            return;
        }
        else
        {
            moveToPosition = playerRB.position;
        }

        if (verticalInput != 0 && !isStunned) // Move the player based on input. Otherwise, move them down with tree
        {
            moveToPosition += moveSpeed * Time.fixedDeltaTime * new Vector3(0, verticalInput, 0);
        }
        else
        {
            if(TreeClimb_GameManager.Instance.treesMoving)
                moveToPosition += Time.fixedDeltaTime * new Vector3(0, -fallSpeed, 0);
        }
        
        // Check if the player is actually able to move in the desired direction
        moveToPosition.y = Mathf.Clamp(moveToPosition.y, VerifyVerticalMovement(Vector3.down), VerifyVerticalMovement(Vector3.up));
        playerRB.MovePosition(moveToPosition);

        // Check if the player has fallen behind
        if (playerRB.position.y < minHeight)
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
            if (VerifyHorizontalMovement(jumpDirection) && !isJumping)
            {
                if (jumpDirection == Vector3.right && currentTreeIndex < (treePositions.Length - 1))
                {
                    currentTreeIndex++;
                    moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, playerRB.position.y, playerRB.position.z);
                    isJumping = true;                   
                }
                else if (jumpDirection == Vector3.left && currentTreeIndex > 0)
                {
                    currentTreeIndex--;
                    moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, playerRB.position.y, playerRB.position.z);
                    isJumping = true;
                }
            }   
        }

        if (isJumping) // Translate player to the new position
        {
            if (playerRB.position != moveToPosition)
            {
                isInvincible = true;
                float step = jumpSpeed * Time.fixedDeltaTime;
                Vector3 newPosition = Vector3.MoveTowards(playerRB.position, moveToPosition, step);
                playerRB.MovePosition(newPosition);
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
        }
    }

    private float VerifyVerticalMovement(Vector3 direction) // Checks if a player or the top of a tree is blocking their path
    {
        float verticalRaycastLength = 1f;
        Vector3 raycastOrigin = playerRB.position;
        raycastOrigin.y += direction == Vector3.up ? .5f : -.5f;

        //Debug.DrawRay(raycastOrigin, direction * verticalRaycastLength, Color.red);

        if (Physics.Raycast(raycastOrigin, direction, out RaycastHit _, verticalRaycastLength, playerLayer))
        {
            return playerRB.position.y;
        }
        if (Physics.Raycast(raycastOrigin, direction, out RaycastHit _, verticalRaycastLength, treeLayer))
        {
            Vector3 moveDown = playerRB.position;
            moveDown += Time.fixedDeltaTime * new Vector3(0, -fallSpeed, 0);
            return moveDown.y;
        }

        return direction == Vector3.up ? maxHeight : minHeight - 1;
    }
    private bool VerifyHorizontalMovement(Vector3 direction) // Checks if there is actually a tree to jump to
    {
        float horizontalRaycastLength = 5f;
        Vector3 raycastOrigin = playerRB.position;

        //Debug.DrawRay(raycastOrigin, direction * horizontalRaycastLength, Color.blue);

        if (Physics.Raycast(raycastOrigin, direction, out RaycastHit _, horizontalRaycastLength, treeLayer))
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
        if (other.gameObject.GetComponent<TreeClimb_AcornScript>() != null)
        {

        }

        if (other.gameObject.GetComponent<TreeClimb_HazardScript>() != null)
        {

        }

        if (other.CompareTag("Finish"))
        {
            isFinished = true;
            TreeClimb_GameManager.Instance.UpdatePlayersFinished(playerID);

            string playerFinishPosition = "Player" + playerID;
            finishPosition = other.gameObject.transform.Find("Finish Positions").gameObject.transform.Find(playerFinishPosition).gameObject;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    public void InitializePlayer(int IDNumber, Color playerColor, Transform startPosition) // Initializes squirrel game object
    {
        playerID = IDNumber;

        isAlive = true;
        playerScore = 0;

        playerRB = GetComponent<Rigidbody>();
        playerRB.position = startPosition.position;
        originalScale = squirrelModel.transform.localScale;

        fallSpeed = GameObject.FindAnyObjectByType<TreeClimb_TreesScript>().moveSpeed;
        jumpTimer = jumpDelay;

        currentTreeIndex = TreeClimb_GameManager.Instance.GetStartingIndex(playerID);
        treePositions = TreeClimb_GameManager.Instance.treePositions;

        playerLayer = LayerMask.GetMask("Player");
        treeLayer = LayerMask.GetMask("Tree");
    }
    public void OnMove(InputAction.CallbackContext context) // Input for moving squirrel
    {
        verticalInput = context.ReadValue<Vector2>().y;

        #region GetHorizontalInput
        if (TreeClimb_GameManager.Instance.gamePlaying)
        {
            float xInput = context.ReadValue<Vector2>().x;
            if (xInput > .5)
            {
                horizontalInput = 1;
            }
            else if (xInput < -.5)
            {
                horizontalInput = -1;
            }
            else
            {
                horizontalInput = 0;
            }       
        }
        #endregion
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
        if (!TreeClimb_GameManager.Instance.gameStarted && context.ReadValueAsButton() == true)
            TreeClimb_MinigameSetup.Instance.StartGame();

        if (TreeClimb_GameManager.Instance.gamePlaying && context.ReadValueAsButton() == true)
            TreeClimb_GameManager.Instance.PauseGame(true);
    }
}