using UnityEngine;

public class TreeClimb_ObjectPhysics : MonoBehaviour
{
    [Header("Fall Speed Values")]
    [Range(1.0f, 5.0f)]
    public float baseSpeed;
    [Range(5.0f, 25.0f)]
    public float fallAcceleration;

    [Header("Rotation Values")]
    public bool rotateX;
    public bool rotateY;
    public bool rotateZ;
    public float rotationSpeed;

    // Other Variables Needed
    private bool freeFalling;
    private float currentSpeed;
    private readonly float destroyHeight = -10f;
    private Rigidbody objectRB;

    private void Start()
    {
        freeFalling = false;
        currentSpeed = baseSpeed;
        objectRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!freeFalling)
        {
            MoveDown();
            RotateObject();
        }     

        CheckPosition();
    }

    private void MoveDown()
    {
        currentSpeed += fallAcceleration * Time.fixedDeltaTime;
        objectRB.velocity = currentSpeed * Vector3.down;
    }

    private void RotateObject()
    {
        Vector3 rotationVector = new(
            rotateX ? rotationSpeed * Time.fixedDeltaTime : 0f,
            rotateY ? rotationSpeed * Time.fixedDeltaTime : 0f,
            rotateZ ? rotationSpeed * Time.fixedDeltaTime : 0f
        );

        Quaternion newRotation = objectRB.rotation * Quaternion.Euler(rotationVector);
        objectRB.MoveRotation(newRotation);
    }


    private void CheckPosition()
    {
        if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }

    public void EnableFreefall(float forwardForce, Vector3 rotationForce)
    {
        freeFalling = true;

        objectRB.velocity = Vector3.zero;
        objectRB.useGravity = true;
        objectRB.AddForce(new Vector3(0, 0, -forwardForce), ForceMode.Impulse);
        objectRB.AddTorque(rotationForce, ForceMode.Impulse);
    }
}