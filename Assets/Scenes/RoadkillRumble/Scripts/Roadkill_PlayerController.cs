using UnityEngine;
using System.Collections;

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
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(RollSquirrel());
        }
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

        if (movement == Vector3.zero) return;
        Quaternion facingDirection = Quaternion.LookRotation(movement, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, facingDirection, Time.deltaTime * 10f);
    }

    IEnumerator RollSquirrel()
    {
        state = Roadkill_PlayerState.Rolling;
        Vector3 dashDirection = transform.forward;
        playerRB.velocity = Vector3.zero;
        playerRB.AddForce(dashDirection * 50f, ForceMode.Impulse);

        float timepassed = 0f;
        while (timepassed < 0.3f)
        {
            transform.Rotate(Vector3.right * 2000f * Time.deltaTime, Space.Self);
            timepassed += Time.deltaTime;
            yield return null;
        }
        state = Roadkill_PlayerState.Running;
    }
}