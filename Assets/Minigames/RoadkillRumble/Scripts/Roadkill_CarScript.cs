using UnityEngine;

public class Roadkill_CarScript : MonoBehaviour
{
    [Header("Attributes")]
    [Range(10f, 25f)]
    public float moveSpeed;

    private Rigidbody rigidBody;
    private bool atMaxSpeed;
    private readonly float distanceBetweenCars = 15f;
    private readonly float deadZone = 100f;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        atMaxSpeed = true;
    }

    private void FixedUpdate()
    {
        rigidBody.MovePosition(rigidBody.position + moveSpeed * Time.fixedDeltaTime * transform.forward);
        if (rigidBody.position.x > deadZone || rigidBody.position.x < -deadZone) Destroy(gameObject);

        if (atMaxSpeed) DetectCarAhead();
    }

    private void DetectCarAhead()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitCar, distanceBetweenCars, LayerMask.GetMask("Object")))
        {
            moveSpeed = hitCar.collider.gameObject.GetComponentInParent<Roadkill_CarScript>().moveSpeed;
            atMaxSpeed = false;
        }
    }
}