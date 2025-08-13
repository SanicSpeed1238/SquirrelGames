using UnityEngine;
using UnityEngine.InputSystem;

public class BigNut_PlayerController : BigNut_Controller
{
    public float bufferFromBehind = 0.1f;
    public float pushSpeed = 2f;

    [Header("Movement Values")]
    public float moveSpeed = 2.0f;

    private bool isNearHidyHole;

    private CharacterController controller;
    private GameObject hidyHole;
    private PlayerInput input;
    private InputAction move;
    private InputAction action;

    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;
        isHiding = false;
        isNearHidyHole = false;
        controller = GetComponent<CharacterController>();

        BirdInitialization();
    }

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        move = input.actions["Move"];
        action = input.actions["Action"];
    }

    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (isAlive)
        {
            squirrelAnimator.speed = 0;
            Vector3 velocity = Vector3.zero;
            if (!isHiding)
            {
                if (!isStunned)
                {
                    Vector2 moveInput = move.ReadValue<Vector2>();

                    if (moveInput != Vector2.zero)
                    {
                        squirrelAnimator.speed = 1;
                        Vector3 movement = transform.right * moveInput.x + transform.forward * moveInput.y;
                        velocity = movement * moveSpeed;
                    }
                }
                else
                {
                    HandleStunning();   
                }                
            }
            velocity = ConstraintPlayerInCamera(velocity);
            controller.Move(velocity * Time.deltaTime);

            if (isNearHidyHole && action.WasPressedThisFrame()) 
            {
                HideAndUnhide();
            }
        }
        else
        {
            controller.enabled = false;
        }
    }

    public override void HideAndUnhide()
    {
        BigNut_HidyHole hidyHoleScript = hidyHole.GetComponent<BigNut_HidyHole>();
        Collider playerCollider = gameObject.GetComponent<Collider>();
        BoxCollider hidyHoleCollider = hidyHole.GetComponent<BoxCollider>();
        if (isHiding)
        {
            Vector3 outPosition = hidyHole.transform.position + Vector3.right * pushDistance;
            outPosition.y = 0;
            controller.enabled = false;
            transform.position = outPosition;
            controller.enabled = true;
            isHiding = false;
            hidyHoleScript.HidingPlayer = null;
            Physics.IgnoreCollision(playerCollider, hidyHoleCollider, false);
        }
        else if (hidyHoleScript.CanHide)
        {
            Physics.IgnoreCollision(playerCollider, hidyHoleCollider, true);

            GameObject hidingPlayer = hidyHoleScript.HidingPlayer;
            if (hidingPlayer != null && hidingPlayer != gameObject)
            {
                BigNut_Controller hidingPlayerController = hidingPlayer.GetComponent<BigNut_Controller>();
                hidingPlayerController.HideAndUnhide();
                hidingPlayerController.StunPlayer();
            }
            hidyHoleScript.HidingPlayer = this.gameObject;
            controller.enabled = false;
            transform.position = new Vector3(hidyHole.transform.position.x, 0, hidyHole.transform.position.z);
            controller.enabled = true;
            isHiding = true;
        }
    }

    public override void Squish()
    {
        if (!isHiding)
        {
            isAlive = false;
            squirrelAnimator.speed = 0;
            transform.localScale = new Vector3(transform.localScale.x, 0.4f, transform.localScale.z);
            Invoke("DestroySquirrel", 3);
        }
    }


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<BigNut_HidyHole>() != null)
        {
            if ((SphereCollider)collision != null) 
            {
                hidyHole = collision.gameObject;
                isNearHidyHole = true;
            }
        }
        if (collision.tag == "Acorn")
        {
            MG_GameManager.Instance.UpdatePlayerQualified(playerID, true);
            hasWon = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.GetComponent<BigNut_HidyHole>() != null)
        {
            SphereCollider collider = (SphereCollider)collision;
            if (collider != null)
            {
                isNearHidyHole = false;
            }
        }
    }

    private Vector3 ConstraintPlayerInCamera(Vector3 velocity)
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        if (viewportPos.y < bufferFromBehind)
        {
            velocity.x += pushSpeed;
        }
        else if (viewportPos.y > bufferFromForward)
        {
            Vector3 clampedWorld = mainCamera.ViewportToWorldPoint(new Vector3(bufferFromForward, viewportPos.y, viewportPos.z));
            velocity = new Vector3(clampedWorld.x, transform.position.y, transform.position.z) - transform.position;
        }
        return velocity;
    }

    private void HandleStunning()
    {
        currentStunTime -= Time.deltaTime;
        if (currentStunTime < 0)
        {
            isStunned = false;
        }
    }
}
