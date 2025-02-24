using UnityEngine;

public class Roadkill_PlayerController : MonoBehaviour
{
    private enum Roadkill_PlayerState
    {
        Running,
        Rolling,
    }

    [Header("Movement Values")]
    public float runSpeed;

    private float horizontalInput;
    private float verticalInput;

    private Rigidbody playerRB;
    private Roadkill_PlayerState state;

    private void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        state = Roadkill_PlayerState.Running;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        switch(state)
        {
            case Roadkill_PlayerState.Running:
                MoveSquirrel();
                break;
        }
    }

    private void MoveSquirrel()
    {
        Vector3 movement = new(horizontalInput, 0, verticalInput);
        playerRB.velocity = movement.normalized * runSpeed;
    }
}