using UnityEngine;
using UnityEngine.InputSystem;

public class AntSmash_HammerController : MonoBehaviour
{
    [Header("Movement Controls")]
    [Range(1f, 20f)]
    public float moveSpeed;
    public float rangeX;
    public float rangeYTop;
    public float rangeYBottom;

    [Header("Important Components")]
    [SerializeField] private Animator hammerAnimator;
    [SerializeField] private ParticleSystem smashEffect;

    // Other Variables Needed
    private int playerID;
    private int playerScore;
    private bool actionPressed;
    private Vector2 movementInput;

    private void Update()
    {
        // If game is not playing, ignore player input
        if (!AntSmash_GameManager.Instance.gamePlaying) return;

        // Move the hammer left, right, up, and down while keeping it in bounds
        MoveHammer();

        // Swing the hammer when the player presses the action button
        if (actionPressed) SmashHammer();
    }

    private void MoveHammer()
    {
        Vector3 newPosition = transform.position + moveSpeed * Time.deltaTime * new Vector3(movementInput.x, 0, movementInput.y);
        newPosition.x = Mathf.Clamp(newPosition.x, -rangeX, rangeX);
        newPosition.z = Mathf.Clamp(newPosition.z, rangeYBottom, rangeYTop);
        transform.position = newPosition;
    }
    private void SmashHammer() // Player can swing hammer again when current swing animation finishes
    {      
        if(hammerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Idle"))
        {
            hammerAnimator.SetTrigger("Smash");
            smashEffect.Play();
        }
        actionPressed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If hammer detects an object that can be squished, calls object's SquishObject function
        // Also adds or subtracts appropriate amount of points and updates score UI
        if(other.GetComponent<AntSmash_ObjectScript>() != null)
        {
            other.GetComponent<AntSmash_ObjectScript>().SquishObject();

            int addedScore = other.GetComponent<AntSmash_ObjectScript>().points;
            playerScore += addedScore;
            if (playerScore < 0) playerScore = 0;
            AntSmash_GameManager.Instance.UpdateScore(playerID, playerScore, addedScore);
        }
    }

    public void ConfigurePlayer(int IDNumber, Color playerColor, Transform startPosition) // Initializes hammer game object
    {
        playerID = IDNumber;
        transform.position = startPosition.position;

        playerScore = 0;
        
        GetComponentInChildren<Outline>().OutlineColor = playerColor;
    }

    public void OnMove(InputAction.CallbackContext context) // Input for moving hammer
    {
        movementInput = context.ReadValue<Vector2>();
    }
    public void OnAction(InputAction.CallbackContext context) // Input for swinging hammer
    {
        actionPressed = context.ReadValueAsButton();

        if (!AntSmash_GameManager.Instance.gameStarted && context.ReadValueAsButton() == true)
            AntSmash_MinigameSetup.Instance.ConfirmJoin(playerID);
    }
    public void OnStart(InputAction.CallbackContext context) // Input for starting and pausing game
    {
        if (!AntSmash_GameManager.Instance.gameStarted && context.ReadValueAsButton() == true)
            AntSmash_MinigameSetup.Instance.StartGame();

        if (AntSmash_GameManager.Instance.gamePlaying && context.ReadValueAsButton() == true)
            AntSmash_GameManager.Instance.PauseGame(true);
    }
}