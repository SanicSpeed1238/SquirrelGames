using UnityEngine;
using System.Collections;

public class Roadkill_PlayerController : MonoBehaviour
{
    private enum Roadkill_PlayerState
    {
        Running,
        Rolling,
        Squished,
    }

    [Header("Movement Values")]
    public float runSpeed;
    public float rollSpeed;

    [Header("Important References")]
    public Animator animator;

    // Other Variables Needed

    private float horizontalInput;
    private float verticalInput;

    private Rigidbody playerRB;
    private Coroutine currentStateRoutine;
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
        
        if (Input.GetKeyDown(KeyCode.Space) && state == Roadkill_PlayerState.Running)
        {
            currentStateRoutine = StartCoroutine(RollSquirrel(transform.forward, 1f));
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
        playerRB.linearVelocity = movement.normalized * runSpeed;
        animator.SetFloat("Movement", movement.normalized.magnitude);

        if (movement == Vector3.zero) return;
        Quaternion facingDirection = Quaternion.LookRotation(movement, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, facingDirection, Time.deltaTime * 10f);
    }

    IEnumerator RollSquirrel(Vector3 direction, float speedMultiplier)
    {
        state = Roadkill_PlayerState.Rolling;

        Vector3 rollDirection = direction;
        playerRB.linearVelocity = Vector3.zero;
        playerRB.AddForce(rollDirection * (rollSpeed * speedMultiplier * 10f), ForceMode.Impulse);
        animator.SetBool("Rolling", true);

        yield return new WaitForSeconds(0.3f);

        animator.SetBool("Rolling", false);

        state = Roadkill_PlayerState.Running;
    }

    IEnumerator SquishSquirrel()
    {
        state = Roadkill_PlayerState.Squished;

        playerRB.linearVelocity = Vector3.zero;
        animator.SetBool("Squished", true);

        yield return new WaitForSeconds(1.5f);

        animator.SetBool("Squished", false);

        state = Roadkill_PlayerState.Running;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Roadkill_AcornScript>(out var acorn))
        {
            acorn.CollectAcorn(transform.position);
        }

        if (other.TryGetComponent<Roadkill_CarCollider>(out var carCollider))
        {
            switch (carCollider.carColliderTag)
            {
                case Roadkill_CarCollider.CarColliderTag.FrontBack:
                    if (state != Roadkill_PlayerState.Squished) currentStateRoutine = StartCoroutine(SquishSquirrel());
                    break;
                case Roadkill_CarCollider.CarColliderTag.Sides:
                    if (state != Roadkill_PlayerState.Squished) currentStateRoutine = StartCoroutine(RollSquirrel(-transform.forward, 0.5f));
                    break;
            }
        }
    }
}