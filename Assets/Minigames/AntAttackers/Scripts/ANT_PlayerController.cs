using UnityEngine;
using UnityEngine.InputSystem;

public class ANT_PlayerController : Base_PlayerController
{
    [Header("Movement Controls")]
    [Range(1f, 20f)]
    public float moveSpeed;
    public float rangeX;
    public float rangeYTop;
    public float rangeYBottom;

    [Header("Important Components")]
    [SerializeField] private Animator hammerAnimator;
    [SerializeField] private AudioSource slamSound;
    [SerializeField] private ParticleSystem smashEffect;

    // Other Variables Needed
    private Vector2 movementInput;
    private bool actionPressed;

    private void Start()
    {
        GetComponentInChildren<QuickOutline>().OutlineColor = playerColor;
    }

    private void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying) 
            return;

        MoveHammer();
        SmashHammer();
    }

    private void MoveHammer() // Player can move the hammer within the play area
    {
        Vector3 newPosition = transform.position + moveSpeed * Time.deltaTime * new Vector3(movementInput.x, 0, movementInput.y);
        newPosition.x = Mathf.Clamp(newPosition.x, -rangeX, rangeX);
        newPosition.z = Mathf.Clamp(newPosition.z, rangeYBottom, rangeYTop);
        transform.position = newPosition;
    }
    private void SmashHammer() // Player can swing hammer again when current swing animation finishes
    {   
        if (actionPressed)
        {
            if (hammerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Idle"))
            {
                hammerAnimator.SetTrigger("Smash");
                smashEffect.Play();

                slamSound.pitch = Random.Range(0.5f, 1.0f);
                slamSound.PlayOneShot(slamSound.clip);

                actionPressed = false;
            }
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        // If hammer detects an object that can be squished, calls object's SquishObject function
        // Also adds or subtracts appropriate amount of points and updates score UI
        if(other.gameObject.TryGetComponent<ANT_ObjectScript>(out var animal))
        {
            animal.SquishObject();

            int addedScore = animal.points;
            playerScore += addedScore;
            if (playerScore < 0) playerScore = 0;
            MG_GameManager.Instance.UpdatePlayerScore(playerID, playerScore, addedScore);
        }
    }

    // Methods for retrieving HUMAN input
    // ----------------------------------
    public void OnMove(InputAction.CallbackContext context) // Input for moving hammer
    {
        movementInput = context.ReadValue<Vector2>();
    }
    public void OnAction(InputAction.CallbackContext context) // Input for swinging hammer
    {
        actionPressed = context.ReadValueAsButton();
    }

    // Methods for calculating COMPUTER input
    // -------------------------------------
    public void SetLeftStickInput(Vector2 input)
    {
        movementInput = input;
    }
    public void SetActionInput(bool input)
    {
        actionPressed = input;
    }
    public bool GetActionInputState()
    {
        return actionPressed;
    }
}