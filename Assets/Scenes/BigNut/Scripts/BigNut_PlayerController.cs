using UnityEngine;

public class BigNut_PlayerController : BigNut_Controller
{
    public float bufferFromBehind = 0.1f;
    public float bufferFromForward = 0.9f;
    public float pushSpeed = 2f;

    [Header("Movement Values")]
    public float moveSpeed;
    public bool isPlayer2;

    [Header("Important Components")]
    public Animator squirrelAnimator;

    private bool isNearHidyHole;

    private string hideButton;
    private CharacterController controller;
    private GameObject hidyHole;

    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;
        isHiding = false;
        isNearHidyHole = false;
        controller = GetComponent<CharacterController>();

        hideButton = isPlayer2 ? "Action2" : "Action";
    }

    // Update is called once per frame
    void Update()
    {
        float xInput;
        float zInput;

        if (isAlive)
        {
            squirrelAnimator.speed = 0;
            Vector3 velocity = Vector3.zero;
            if (!isHiding)
            {
                if (!isStunned)
                {
                    if (!isPlayer2) xInput = Input.GetAxisRaw("Horizontal");
                    else xInput = Input.GetAxisRaw("Horizontal2");
                    if (!isPlayer2) zInput = Input.GetAxisRaw("Vertical");
                    else zInput = Input.GetAxisRaw("Vertical2");

                    if (xInput != 0 || zInput != 0)
                    {
                        squirrelAnimator.speed = 1;
                        Vector3 movement = transform.right * xInput + transform.forward * zInput;
                        velocity = movement * moveSpeed;
                    }
                    velocity = ConstraintPlayerInCamera(velocity);
                }
                else
                {
                    HandleStunning();   
                }
                
                controller.Move(velocity * Time.deltaTime);
            }

            if (isNearHidyHole && Input.GetButtonDown(hideButton)) 
            {
                HideAndUnhide();
            }
        }
        else
        {
            controller.enabled = false;
            transform.position = birbThatCaught.position;
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
        else
        {
            Physics.IgnoreCollision(playerCollider, hidyHoleCollider, true);

            GameObject hidingPlayer = hidyHoleScript.HidingPlayer;
            if (hidingPlayer != null && hidingPlayer != gameObject)
            {
                BigNut_Controller hidingPlayerController = hidingPlayer.GetComponent<BigNut_PlayerController>();
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

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "HidyHole")
        {
            if ((SphereCollider)collision != null) 
            {
                hidyHole = collision.gameObject;
                isNearHidyHole = true;
            }               
        }
        if (!isHiding && collision.gameObject.tag == "Birb")
        {
            isAlive = false;
            birbThatCaught = collision.gameObject.transform;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        SphereCollider collider = (SphereCollider)collision;
        if (collider != null && collision.gameObject.tag == "HidyHole")
        {
            isNearHidyHole = false;
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
            transform.position = new Vector3(clampedWorld.x, transform.position.y, transform.position.z);
            velocity.x = 0;
        }
        return velocity;
    }

}
