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
    [SerializeField] private GameObject hammerModel;
    [SerializeField] private Animator hammerAnimator;
    [SerializeField] private AudioSource slamSound;
    [SerializeField] private ParticleSystem smashEffect;

    // Other Variables Needed
    protected Vector2 movementInput;
    protected bool actionPressed;

    private int playerID;
    private int playerScore;
    
    private void Update()
    {
        if (!AntSmash_GameManager.Instance.gamePlaying) 
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
            }
            actionPressed = false;
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        // If hammer detects an object that can be squished, calls object's SquishObject function
        // Also adds or subtracts appropriate amount of points and updates score UI
        if(other.gameObject.TryGetComponent<AntSmash_AnimalScript>(out var animal))
        {
            animal.SquishObject();

            int addedScore = animal.points;
            playerScore += addedScore;
            if (playerScore < 0) playerScore = 0;
            AntSmash_GameManager.Instance.UpdateScore(playerID, playerScore, addedScore);
        }
    }

    public void InitializePlayer(int IDNumber, Color playerColor, Transform startPosition) // Initializes hammer game object
    {
        playerID = IDNumber;

        playerScore = 0;
        transform.position = startPosition.position;
        
        GetComponentInChildren<Outline>().OutlineColor = playerColor;
        playerColor.a = 0.5f;
        hammerModel.GetComponentInChildren<Renderer>().material.color = playerColor;
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