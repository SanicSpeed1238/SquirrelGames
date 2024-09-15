using UnityEngine;
using UnityEngine.InputSystem;

public class HammerController : MonoBehaviour
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
    private SmashManager gameManager;
    private MinigameStartup minigameStartup;

    private void Update()
    {
        // If game is not playing, ignore player input
        if (!gameManager.gamePlaying) return;

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
        if(other.GetComponentInParent<SmashObject>() != null)
        {
            other.GetComponentInParent<SmashObject>().SquishObject();

            playerScore += other.GetComponentInParent<SmashObject>().points;
            if (playerScore < 0) playerScore = 0;
            gameManager.UpdateScore(playerID, playerScore);
        }
    }

    public void ConfigurePlayer(int IDNumber, Color playerColor, Transform startPosition) // Initializes hammer game object
    {
        playerID = IDNumber;
        GetComponentInChildren<Outline>().OutlineColor = playerColor;

        playerScore = 0;
        transform.position = startPosition.position;

        gameManager = GameObject.FindAnyObjectByType<SmashManager>();
        minigameStartup = GameObject.FindAnyObjectByType<MinigameStartup>();
    }
    public void OnMove(InputAction.CallbackContext context) // Input for moving hammer
    {
        movementInput = context.ReadValue<Vector2>();
    }
    public void OnAction(InputAction.CallbackContext context) // Input for swinging hammer
    {
        actionPressed = context.ReadValueAsButton();
    }
    public void OnStart(InputAction.CallbackContext context) // Input for starting and pausing game
    {
        if(!gameManager.gameStarted)
        {
            gameManager.gameStarted = context.ReadValueAsButton();
            minigameStartup.StartGame();
        }
    }
}