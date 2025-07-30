using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class TreeClimb_PlayerController : Base_PlayerController
{
    private enum TreeClimb_PlayerState
    {
        Climbing,
        Jumping,
        Squished,
        Finished
    }

    [Header("Movement Values")]
    [Range(5f, 25f)]
    public float moveUpSpeed;
    [Range(5f, 25f)]
    public float moveDownSpeed;
    [Range(5f, 25f)]
    public float jumpSpeed;
    [Range(5f, 25f)]
    public float bounceSpeed;
    [Range(0.1f, 1f)]
    public float jumpCooldown;
    [Range(0.0f, 0.1f)]
    public float stunHeightMultiplier;
    [Range(1f, 3f)]
    public float invincibilityDuration;
    public float maxHeight = 10f;
    public float minHeight = 1f;

    [Header("Important Components")]
    public GameObject squirrelModel;
    public Animator squirrelAnimator;
    public GameObject bounceEffect;
    public ParticleSystem invincibleEffect;

    // Other Variables Needed

    private float verticalInput;
    private float horizontalInput;

    private bool isAlive;
    private bool isBlocked;
    private bool isStumbling;
    private bool isInvincible;
    private TreeClimb_PlayerState state;

    private Rigidbody playerRB;
    private Vector3 moveToPosition;
    private float moveSpeed;
    private float fallSpeed;

    private int previousTreeIndex;
    private int currentTreeIndex;
    private float jumpTimer;
    private Vector3 jumpDirection;
    private Transform[] treePositions;
    private GameObject finishPosition;

    private LayerMask playerLayer;
    private LayerMask treeLayer;

    private Vector3 originalScale;
    private readonly float freeFallStopHeight = -100f;

    private void Start()
    {
        isAlive = true;
        state = TreeClimb_PlayerState.Climbing;

        playerRB = GetComponent<Rigidbody>();
        originalScale = squirrelModel.transform.localScale;

        fallSpeed = TreeClimb_GameManager.Instance.gameMoveSpeed;
        jumpTimer = jumpCooldown;

        currentTreeIndex = TreeClimb_GameManager.Instance.GetStartingIndex(playerID);
        treePositions = TreeClimb_GameManager.Instance.treePositions;

        playerLayer = LayerMask.GetMask("Player");
        treeLayer = LayerMask.GetMask("Tree");
    }

    private void FixedUpdate()
    {
        if(isAlive)
        {
            if (state != TreeClimb_PlayerState.Finished)
            {
                if (!MG_GameManager.Instance.gamePlaying)
                    return;

                MoveVertically();
                MoveHorizontally(); 
            }
        }
        else
        {
            if (playerRB.position.y < freeFallStopHeight && !playerRB.isKinematic)
            {
                playerRB.isKinematic = true;
            }       
        }
    }

    #region Movement

    #region Vertical Movement
    private void MoveVertically() // Moves up and down the tree the player is currently on
    {
        if (state == TreeClimb_PlayerState.Jumping) // If player is airborne, ignore vertical input
            return;

        moveToPosition = playerRB.position;
        if (state == TreeClimb_PlayerState.Climbing && verticalInput != 0) // Move the player based on input. Otherwise, move them down with tree
        {
            moveSpeed = (verticalInput > 0 ? moveUpSpeed : moveDownSpeed);
            if (-0.5f < verticalInput && verticalInput < 0) verticalInput = Mathf.Clamp(verticalInput, -1f, -0.5f);
            moveToPosition += moveSpeed * Time.fixedDeltaTime * new Vector3(0, verticalInput, 0);
        }
        else
        {
            if(TreeClimb_GameManager.Instance.treesMoving)
                moveToPosition += Time.fixedDeltaTime * new Vector3(0, -fallSpeed, 0);
        }
        squirrelAnimator.SetFloat("Movement", verticalInput);
        squirrelAnimator.SetFloat("Direction", 0);

        // Check if the player is actually able to move in the desired direction
        moveToPosition.y = Mathf.Clamp(moveToPosition.y, VerifyVerticalMovement(Vector3.down), VerifyVerticalMovement(Vector3.up));
        playerRB.MovePosition(moveToPosition);

        // Check if the player has fallen too far behind
        if (playerRB.position.y < minHeight)
        {
            EliminatePlayer();
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
    #endregion

    #region Horizontal Movement
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
                        }
                    }
                    break;
                }

            case TreeClimb_PlayerState.Jumping:
                {
                    // If the player is in the jumping state, automatically move in the assigned direction
                    if (playerRB.position != moveToPosition)
                    {
                        float step = (!isStumbling ? jumpSpeed : bounceSpeed) * Time.fixedDeltaTime;
                        Vector3 newPosition = Vector3.MoveTowards(playerRB.position, moveToPosition, step);
                        playerRB.MovePosition(newPosition);
                    }
                    else
                    {
                        previousTreeIndex = currentTreeIndex;
                        isStumbling = false;
                        isBlocked = false;                        
                        
                        state = TreeClimb_PlayerState.Climbing;
                        jumpDirection = Vector3.zero;
                        jumpTimer = jumpCooldown;

                        squirrelAnimator.SetBool("Jumping", false);
                        squirrelAnimator.SetBool("Bouncing", false);
                    }
                    break;
                }
        }
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
    #endregion

    #region Airborne Movement
    private void SetJumpDirection() // Returns a target position based on the array of tree positions
    {
        state = TreeClimb_PlayerState.Jumping;
        squirrelAnimator.SetBool("Jumping", true);

        if (jumpDirection == Vector3.right && currentTreeIndex < (treePositions.Length - 1))
        {
            previousTreeIndex = currentTreeIndex;
            currentTreeIndex++;
            moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, playerRB.position.y, playerRB.position.z);
            squirrelAnimator.SetFloat("Direction", 1);
        }
        else if (jumpDirection == Vector3.left && currentTreeIndex > 0)
        {
            previousTreeIndex = currentTreeIndex;
            currentTreeIndex--;
            moveToPosition = new Vector3(treePositions[currentTreeIndex].position.x, playerRB.position.y, playerRB.position.z);
            squirrelAnimator.SetFloat("Direction", -1);
        }
    }
    private void StumbleSquirrel() // Reverse the jump direction if colliding with another player or hazard
    {
        isStumbling = true;
        squirrelAnimator.SetBool("Bouncing", true);

        Vector3 impactPosition = playerRB.position;
        jumpDirection = -jumpDirection;
        if (jumpDirection == Vector3.right)
        {
            impactPosition.x += 0.1f;
            Instantiate(bounceEffect, impactPosition, Quaternion.identity);
            squirrelAnimator.SetFloat("Direction", 1);
        }
        else
        {
            impactPosition.x -= 0.1f;
            Instantiate(bounceEffect, impactPosition, Quaternion.identity);
            squirrelAnimator.SetFloat("Direction", -1);
        }

        SetJumpDirection();
    }
    public void VerifyStumbleMovement(Vector3 otherJumpDirection, Rigidbody otherRB)
    {
        if (jumpDirection == otherJumpDirection)
        {
            if (playerRB.position.x < otherRB.position.x)
            {
                if (jumpDirection != Vector3.left)
                {
                    StumbleSquirrel();
                }
            }
            else
            {
                if (jumpDirection != Vector3.right)
                {
                    StumbleSquirrel();
                }
            }
        }
        else
        {
            StumbleSquirrel();
        }
    }
    #endregion

    #region Finish Movement
    IEnumerator MoveToFinishPosition()
    {
        Vector3 targetPosition = playerRB.position;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(startRot.eulerAngles + new Vector3(0, 179f, 0));

        float height = 8f;
        float duration = 1f;
        float elapsed = 0f;  

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            Vector3 linearPos = Vector3.Lerp(targetPosition, finishPosition.transform.position, t);
            float arc = 4 * height * t * (1 - t);
            linearPos.y += arc;
            playerRB.position = linearPos;

            if (t <= 0.7f)
            {
                float rotT = t / 0.7f;
                playerRB.rotation = Quaternion.Slerp(startRot, endRot, rotT);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        while (state == TreeClimb_PlayerState.Finished)
        {
            playerRB.position = finishPosition.transform.position;
            yield return null;
        }
    }
    #endregion

    #endregion

    #region Squishing

    private void SquishSquirrel(float stunDuration, float squishFactor)
    {       
        StartCoroutine(SquishCoroutine(stunDuration, squishFactor));
    }
    IEnumerator SquishCoroutine(float stunDuration, float squishFactor)
    {
        float totalStunDuration = stunDuration + (playerRB.position.y * stunHeightMultiplier);

        Vector3 squishScale = new(originalScale.x, squishFactor, originalScale.z);
        squirrelModel.GetComponent<SquishModel>().SquishTemporarily(squishScale, totalStunDuration);

        state = TreeClimb_PlayerState.Squished;
        squirrelAnimator.SetBool("Squished", true);

        yield return new WaitForSeconds(totalStunDuration);

        state = TreeClimb_PlayerState.Climbing;
        squirrelAnimator.SetBool("Squished", false);

        StartCoroutine(EnableInvincibility());
    }
    IEnumerator EnableInvincibility()
    {
        isInvincible = true;
        invincibleEffect.Play();

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        invincibleEffect.Stop();
    }

    #endregion

    #region Scoring

    public void AddToPlayerScore(int points)
    {
        playerScore += points;
        MG_GameManager.Instance.UpdatePlayerScore(playerID, playerScore, points);
    }

    public void EliminatePlayer() 
    {
        if (state == TreeClimb_PlayerState.Finished) return;

        isAlive = false;
        verticalInput = 0;
        horizontalInput = 0;
        squirrelAnimator.SetTrigger("Fall");

        playerRB.isKinematic = false;
        playerRB.velocity = new Vector3(0, -20, -2);

        playerScore = -1;
        MG_GameManager.Instance.UpdatePlayerScore(playerID, playerScore, 0);
    }

    private void FinishPlayer(GameObject finishPositions)
    {
        state = TreeClimb_PlayerState.Finished;

        AddToPlayerScore(TreeClimb_GameManager.Instance.GetBonusPoints());
        MG_GameManager.Instance.UpdatePlayerFinished(playerID);

        string playerFinishPosition = "Player" + playerID;
        finishPosition = finishPositions.transform.Find(playerFinishPosition).gameObject;

        squirrelAnimator.SetTrigger("Finish");
        StartCoroutine(MoveToFinishPosition());
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<TreeClimb_AcornScript>(out var acorn))
        {
            if (state != TreeClimb_PlayerState.Squished && isAlive)
            {
                AddToPlayerScore(acorn.points);
                acorn.CollectAcorn(transform.position);
            }       
        }

        if (other.gameObject.TryGetComponent<TreeClimb_HazardScript>(out var hazard))
        {
            if (state == TreeClimb_PlayerState.Climbing && (isAlive && !isInvincible))
            {
                SquishSquirrel(hazard.stunDuration,hazard.squishFactor);
                hazard.HitHazard();
            }             
            else if (state == TreeClimb_PlayerState.Jumping)
            {
                if (previousTreeIndex != hazard.GetTreeIndex())
                {
                    StumbleSquirrel();
                }
            }              
        }

        if (other.gameObject.TryGetComponent<TreeClimb_PlayerController>(out var otherPlayer))
        {
            if (state == TreeClimb_PlayerState.Jumping)
            {
                VerifyStumbleMovement(jumpDirection, otherPlayer.GetComponent<Rigidbody>());
            }
        }

        if (other.CompareTag("Finish"))
        {
            if (state != TreeClimb_PlayerState.Finished) FinishPlayer(other.gameObject.transform.Find("Positions").gameObject);   
        }

        if (other.CompareTag("Deadzone") && isStumbling)
        {
            if (isAlive) EliminatePlayer();
        }
    }

    #region Input

    // Methods for retrieving HUMAN input
    // ----------------------------------
    public void OnMove(InputAction.CallbackContext context) // Input for moving squirrel
    {
        verticalInput = context.ReadValue<Vector2>().y;

        if (MG_GameManager.Instance.gamePlaying)
        {
            float xInput = context.ReadValue<Vector2>().x;

            if (xInput > .5) horizontalInput = 1;
            else if (xInput < -.5) horizontalInput = -1;
            else horizontalInput = 0;   
        }
    }

    // Methods for calculating COMPUTER input
    // --------------------------------------
    public void SetVerticalInput(float input)
    {
        verticalInput = input;
    }
    public void SetHorizontalInput(float input)
    {
        horizontalInput = input;
    }
    public int GetCurrentTreeIndex()
    {
        return currentTreeIndex;
    }
    public bool GetBlockedState()
    {
        return isBlocked;
    }

    #endregion
}