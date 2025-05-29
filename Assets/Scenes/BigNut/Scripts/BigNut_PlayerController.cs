using UnityEngine;

public class BigNut_PlayerController : MonoBehaviour
{
    [Header("Movement Values")]
    public float moveSpeed;
    public bool isPlayer2;

    [Header("Important Components")]
    public GameObject squirrelModel;
    public Animator squirrelAnimator;

    [Header("Camera Values")]
    public Camera mainCamera;
    public float bufferFromBehind = 0.1f; 
    public float bufferFromForward = 0.9f;
    public float pushSpeed = 2f;        

    private bool isAlive;
    private bool isHiding;
    private float pushDistance = 2f;

    private Transform birbThatCaught;
    private CharacterController controller;


    public bool IsAlive { get => isAlive; }

    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;
        isHiding = false;
        controller = GetComponent<CharacterController>();
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

            if(!isPlayer2) xInput = Input.GetAxisRaw("Horizontal2");
            else xInput = Input.GetAxisRaw("Horizontal");
            if (!isPlayer2) zInput = Input.GetAxisRaw("Vertical2");
            else zInput = Input.GetAxisRaw("Vertical");

            if (xInput != 0 || zInput != 0)
            {
                squirrelAnimator.speed = 1;
                Vector3 movement = transform.right * xInput + transform.forward * zInput;
                velocity = movement * moveSpeed;
            }

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
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            transform.position = birbThatCaught.position;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "HidyHole")
        {
            BigNut_HidyHole hidyHole = collision.gameObject.GetComponent<BigNut_HidyHole>();
            GameObject hidingPlayer = hidyHole.HidingPlayer;
            if (hidingPlayer != null && hidingPlayer != gameObject)
            {
                Debug.Log(hidingPlayer.ToString());
                PushPlayerAway(hidingPlayer, collision.gameObject);
            }
            hidyHole.HidingPlayer = this.gameObject;
            isHiding = true;
        }
        if (!isHiding && collision.gameObject.tag == "Birb")
        {
            isAlive = false;
            birbThatCaught = collision.gameObject.transform;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "HidyHole")
        {
            BigNut_HidyHole hidyHole = collision.gameObject.GetComponent<BigNut_HidyHole>();
            if (hidyHole.HidingPlayer == gameObject)
            {
                hidyHole.HidingPlayer = null;
            }
            isHiding = false;
        }
    }

    private void PushPlayerAway(GameObject hidingPlayer, GameObject hidyHole)
    {
        Vector3 pushDirection = (hidingPlayer.transform.position - transform.position).normalized;
        pushDirection.y = 0;
        Vector3 targetPosition = hidyHole.transform.position + pushDirection * pushDistance;
        Vector3 push = targetPosition - hidingPlayer.transform.position;
        push.y = 0;
        
        hidingPlayer.GetComponent<CharacterController>().Move(push);
    }
}
