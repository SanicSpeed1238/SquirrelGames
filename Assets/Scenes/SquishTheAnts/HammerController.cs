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
    [HideInInspector] public int playerID;
    private Vector2 movementInput;
    private bool actionPressed;
    private SmashManager gameManager;
    private int playerScore;

    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<SmashManager>();
        playerScore = 0;
    }

    private void Update()
    {
        // Move the hammer left, right, up, and down while keeping it in bounds
        Vector3 newPosition = transform.position + moveSpeed * Time.deltaTime * new Vector3(movementInput.x, 0, movementInput.y);
        newPosition.x = Mathf.Clamp(newPosition.x, -rangeX, rangeX);
        newPosition.z = Mathf.Clamp(newPosition.z, rangeYBottom, rangeYTop);
        transform.position = newPosition;

        // Swing the hammer when the player presses the action button
        if (actionPressed)
        {
            SmashHammer();
        }
    }

    private void SmashHammer() // Player can swing hammer again when current swing animation finishes
    {
        actionPressed = false;
        if(hammerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Idle"))
        {
            hammerAnimator.SetTrigger("Smash");
            smashEffect.Play();
        }      
    }

    private void OnTriggerEnter(Collider other)
    {
        // If hammer detects an object that can be squished, calls object's SquishObject function
        // Also adds or subtracts appropriate amount of points and updates score UI
        if(other.GetComponentInParent<SmashObject>() != null)
        {
            other.GetComponentInParent<SmashObject>().SquishObject();
            playerScore += other.GetComponentInParent<SmashObject>().points;
            gameManager.UpdateScore(playerID, playerScore);
        }
    }

    public void OnMove(InputAction.CallbackContext context) // Input for moving hammer
    {
        movementInput = context.ReadValue<Vector2>();
    }
    public void OnAction(InputAction.CallbackContext context) // Input for swinging hammer
    {
        actionPressed = context.ReadValueAsButton();
    }
}