using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class TreeClimb_PlayerController : MonoBehaviour
{
    protected enum TreeClimb_PlayerState
    {
        Climbing,
        Jumping,
        Squished,
    }

    [Header("Movement Values")]
    [Range(5f, 20f)]
    public float moveUpSpeed;
    [Range(5f, 20f)]
    public float moveDownSpeed;
    [Range(5f, 20f)]
    public float jumpSpeed;
    [Range(5f, 20f)]
    public float bounceSpeed;
    [Range(0.1f, 1f)]
    public float jumpDelay;
    [Range(1f, 3f)]
    public float invincibilityDuration;

    [Header("Important Components")]
    public GameObject squirrelModel;
    public Animator squirrelAnimator;
    public GameObject bounceEffect;
    public Material invincibleMaterial;

    // Other Variables Needed
    protected float verticalInput;
    protected float horizontalInput;

    private bool isAlive;
    private bool isFinished;
    private int playerID;
    private int playerScore;

    protected Rigidbody playerRB;
    private Vector3 moveToPosition;
    private float moveSpeed;
    private float fallSpeed;
    protected readonly float minHeight = 1f;
    protected readonly float maxHeight = 14f;

    protected int previousTreeIndex;
    protected int currentTreeIndex;
    private float jumpTimer;
    private Vector3 jumpDirection;
    private Transform[] treePositions;
    private GameObject finishPosition;

    private LayerMask playerLayer;
    private LayerMask treeLayer;

    private Vector3 originalScale;
    private Material originalMaterial;
    private Renderer materialRender;

    protected bool isBlocked;
    protected bool isBouncing;
    protected bool isInvincible;
    protected TreeClimb_PlayerState state;

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
        else
        {
            if (playerRB.position.y < -10)
                playerRB.isKinematic = true;
        }
    }

    private void MoveVertically() // Moves up and down the tree the player is currently on
    {
        if (state == TreeClimb_PlayerState.Jumping) // If player is airborne, ignore vertical input
            return;

        moveToPosition = playerRB.position;
        if (state == TreeClimb_PlayerState.Climbing && verticalInput != 0) // Move the player based on input. Otherwise, move them down with tree
        {
            moveSpeed = (verticalInput > 0 ? moveUpSpeed : moveDownSpeed);
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

        // Check if the player has fallen too far behind
        if (playerRB.position.y < minHeight)
        {
            EliminatePlayer();
        }
    }
    private void MoveHorizontally() // Makes the character jump left and right to another tree
    {
        switch (state)
        {
            case TreeClimb_PlayerState.Climbing:
                {
                    if (jumpTimer > 0)
                    {
                        jumpTimer -= Time.deltaTime;
                        return;
                    }

                    // Calculate the direction the player is trying to jump in
                    if (horizontalInput == 1) jumpDirection = Vector3.right;
                    else if (horizontalInput == -1) jumpDirection = Vector3.left;
                    else jumpDirection = Vector3.zero;

                    if (jumpDirection != Vector3.zero)
                    {
                        if (VerifyHorizontalMovement(jumpDirection))
                        {
                            SetJumpDirection();
                            state = TreeClimb_PlayerState.Jumping;
                        }
                    }
                    break;
                }

            case TreeClimb_PlayerState.Jumping:
                {
                    // If the player is in the jumping state, automatically move in the assigned direction
                    if (playerRB.position != moveToPosition)
                    {
                        float step = (!isBouncing ? jumpSpeed : bounceSpeed) * Time.fixedDeltaTime;
                        Vector3 newPosition = Vector3.MoveTowards(playerRB.position, moveToPosition, step);
                        playerRB.MovePosition(newPosition);
                    }
                    else
                    {
                        previousTreeIndex = currentTreeIndex;
                        isBouncing = false;
                        isBlocked = false;                        
                        
                        state = TreeClimb_PlayerState.Climbing;
                        jumpDirection = Vector3.zero;
                        jumpTimer = jumpDelay;
                    }
                    break;
                }
        }
    }

    private float VerifyVerticalMovement(Vector3 direction) // Checks if a player or the top of a tree is blocking their path
    {
        float verticalCastLength = 1f;
        Vector3 boxSize = new(4f, 2f, 2f);
        Vector3 castOrigin = playerRB.position;

        if (Physics.BoxCast(castOrigin, boxSize / 2, direction, out RaycastHit _, Quaternion.identity, verticalCastLength, playerLayer))
        {
            if (direction == Vector3.up)
            {
                Vector3 moveDown = playerRB.position;
                moveDown += Time.fixedDeltaTime * new Vector3(0, -fallSpeed, 0);
                return moveDown.y;
            }
            else
            {
                return playerRB.position.y;
            }   
        }
        if (Physics.BoxCast(castOrigin, boxSize / 2, direction, out RaycastHit _, Quaternion.identity, verticalCastLength, treeLayer))
        {
            isBlocked = true;
            Vector3 moveDown = playerRB.position;
            moveDown += Time.fixedDeltaTime * new Vector3(0, -fallSpeed, 0);
            return moveDown.y;
        }

        return direction == Vector3.up ? maxHeight : minHeight - 1;
    }
    private bool VerifyHorizontalMovement(Vector3 direction) // Checks if there is actually a tree to jump to
    {
        float horizontalCastLength = 4f;
        Vector3 boxSize = new(2f, 4f, 2f);
        Vector3 castOrigin = playerRB.position;

        if (Physics.BoxCast(castOrigin, boxSize / 2, direction, out RaycastHit _, Quaternion.identity, horizontalCastLength, treeLayer))
        {
            return false;
        }
        else return true;
    }

    private void SetJumpDirection() // Returns a target position based on the array of tree positions
    {
        if (jumpDirection == Vector3.right && currentTreeIndex < (treePositions.Length - 1))
        {
            previousTreeIndex = currentTreeIndex;
            currentTreeIndex++;
            moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, playerRB.position.y, playerRB.position.z);
        }
        else if (jumpDirection == Vector3.left && currentTreeIndex > 0)
        {
            previousTreeIndex = currentTreeIndex;
            currentTreeIndex--;
            moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, playerRB.position.y, playerRB.position.z);
        }
    }
    private void BounceSquirrel() // Reverse the jump direction if colliding with another player or hazard
    {
        isBouncing = true;

        Vector3 impactPosition = playerRB.position;
        jumpDirection = -jumpDirection;
        if (jumpDirection == Vector3.right)
        {
            impactPosition.x += 0.1f;
            Instantiate(bounceEffect, impactPosition, Quaternion.identity);
        }
        else
        {
            impactPosition.x -= 0.1f;
            Instantiate(bounceEffect, impactPosition, Quaternion.identity);
        }

        SetJumpDirection();
    }
    public void VerifySquirrelsBouncing(Vector3 otherJumpDirection, Rigidbody otherRB)
    {
        if (jumpDirection == otherJumpDirection)
        {
            if (playerRB.position.x < otherRB.position.x)
            {
                if (jumpDirection != Vector3.left)
                {
                    BounceSquirrel();
                }
            }
            else
            {
                if (jumpDirection != Vector3.right)
                {
                    BounceSquirrel();
                }
            }
        }
        else
        {
            BounceSquirrel();
        }
    } // Used to fix player collision issues

    public void SquishSquirrel(float stunForce, float squishFactor)
    {       
        StartCoroutine(SquishCoroutine(stunForce, squishFactor));
    }
    IEnumerator SquishCoroutine(float stunDuration, float squishFactor)
    {
        Vector3 squishScale = new(originalScale.x, squishFactor, originalScale.z);
        squirrelModel.GetComponent<CS_SquishModel>().SquishTemporarily(squishScale, stunDuration);

        state = TreeClimb_PlayerState.Squished;
        yield return new WaitForSeconds(stunDuration);
        state = TreeClimb_PlayerState.Climbing;

        StartCoroutine(EnableInvincibility());
    }
    IEnumerator EnableInvincibility()
    {
        isInvincible = true;
        Vector4 transparentColor = invincibleMaterial.color;
        transparentColor.w = 0.2f;
        invincibleMaterial.color = transparentColor;
        materialRender.material = invincibleMaterial;

        yield return new WaitForSeconds(invincibilityDuration);

        materialRender.material = originalMaterial;
        isInvincible = false;
    }

    public void AddToPlayerScore(int points)
    {
        playerScore += points;
        TreeClimb_GameManager.Instance.UpdateScore(playerID, playerScore, points);
    }

    private void MoveToFinishPosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, finishPosition.transform.position, moveSpeed * Time.deltaTime);
    }

    public void EliminatePlayer() 
    {
        if (isFinished) return;

        isAlive = false;
        verticalInput = 0;
        horizontalInput = 0;

        playerRB.isKinematic = false;
        playerRB.velocity = new Vector3(0, -10, 0);
        playerRB.AddForce(new Vector3(0, 0, -1), ForceMode.Impulse);
        playerRB.AddTorque(new Vector3(-10, 0, 0), ForceMode.Impulse);

        playerScore = 0;
        TreeClimb_GameManager.Instance.UpdateScore(playerID, playerScore, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<TreeClimb_AcornScript>(out var acorn))
        {
            if (isAlive && !isFinished)
            {
                acorn.CollectAcorn(this);
            }       
        }

        if (other.gameObject.TryGetComponent<TreeClimb_HazardScript>(out var hazard))
        {
            if (state == TreeClimb_PlayerState.Climbing && !isInvincible && !isFinished)
            {
                hazard.SquishSquirrel(this);
            }             
            else if (state == TreeClimb_PlayerState.Jumping)
            {
                if (previousTreeIndex != hazard.GetTreeIndex())
                {
                    BounceSquirrel();
                }
            }              
        }

        if (other.gameObject.TryGetComponent<TreeClimb_PlayerController>(out var otherPlayer))
        {
            if (state == TreeClimb_PlayerState.Jumping)
            {
                VerifySquirrelsBouncing(jumpDirection, otherPlayer.GetComponent<Rigidbody>());
            }
        }

        if (other.CompareTag("Finish"))
        {
            if (!isFinished)
            {
                isFinished = true;
                AddToPlayerScore(TreeClimb_GameManager.Instance.GainBonusPoints());
                TreeClimb_GameManager.Instance.UpdatePlayersFinished(playerID);

                string playerFinishPosition = "Player" + playerID;
                finishPosition = other.gameObject.transform.Find("Finish Positions").gameObject.transform.Find(playerFinishPosition).gameObject;
            }          
        }

        if (other.CompareTag("Deadzone") && isBouncing)
        {
            if (isAlive) EliminatePlayer();
        }
    }

    public void InitializePlayer(int IDNumber, Color playerColor, Transform startPosition) // Initializes squirrel game object
    {
        playerID = IDNumber;

        playerScore = 0;
        isAlive = true;
        state = TreeClimb_PlayerState.Climbing;

        playerRB = GetComponent<Rigidbody>();
        playerRB.position = startPosition.position;

        originalScale = squirrelModel.transform.localScale;
        originalMaterial = squirrelModel.GetComponentInChildren<Renderer>().material;
        materialRender = GetComponentInChildren<Renderer>();
        GetComponentInChildren<Outline>().OutlineColor = playerColor;

        fallSpeed = TreeClimb_GameManager.Instance.gameMoveSpeed;
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