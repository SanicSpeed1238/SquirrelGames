using UnityEngine;

public class TreeClimb_ObjectPhysics : MonoBehaviour
{
    [Header("Fall Speed Values")]
    [Range(1.0f, 10.0f)]
    public float baseSpeed;
    [Range(5.0f, 50.0f)]
    public float fallAcceleration;
    [Range(1.0f, 20.0f)]
    public float freeFallMultiplier;

    [Header("Rotation Values")]
    public bool rotateX;
    public bool rotateY;
    public bool rotateZ;
    public float rotationSpeed;

    [Header("Important Components")]
    [SerializeField] private GameObject objectMesh;

    // Other Variables Needed
    private Rigidbody objectRB;
    private float currentSpeed;
    private readonly float destroyHeight = -10f;

    private void Start()
    {
        currentSpeed = baseSpeed;
        objectRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (objectRB.isKinematic)
        {
            MoveDown();
            RotateObject();
        }
        else
        {
            objectRB.AddForce(Vector3.down * freeFallMultiplier, ForceMode.Acceleration);
        }

        CheckPosition();
    }

    private void MoveDown()
    {
        float deltaTime = Time.fixedDeltaTime;
        currentSpeed += fallAcceleration * deltaTime;

        Vector3 newPosition = objectRB.position + currentSpeed * deltaTime * Vector3.down;
        objectRB.MovePosition(newPosition);
    }

    private void RotateObject()
    {
        float deltaTime = Time.fixedDeltaTime;

        Vector3 rotationVector = new(
            rotateX ? rotationSpeed * deltaTime : 0f,
            rotateY ? rotationSpeed * deltaTime : 0f,
            rotateZ ? rotationSpeed * deltaTime : 0f
        );

        objectMesh.transform.rotation *= Quaternion.Euler(rotationVector);
    }

    private void CheckPosition()
    {
        if (objectRB.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }

    public void EnableFreefall(Vector3 bounceDirection, Vector3 rotationForce)
    {
        objectRB.isKinematic = false;
        objectRB.velocity = Vector3.zero;     
        objectRB.AddForce(new Vector3(bounceDirection.x, bounceDirection.y, -bounceDirection.z), ForceMode.Impulse);
        objectRB.AddTorque(rotationForce, ForceMode.Impulse);
    }
}