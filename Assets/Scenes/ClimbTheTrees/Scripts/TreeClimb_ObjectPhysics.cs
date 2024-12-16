using UnityEngine;

public class TreeClimb_ObjectPhysics : MonoBehaviour
{
    [Header("Fall Speed Values")]
    [Range(1.0f, 5.0f)]
    public float baseSpeed;
    [Range(1.0f, 10.0f)]
    public float fallAcceleration;

    [Header("Rotation Values")]
    public bool rotateX;
    public bool rotateY;
    public bool rotateZ;
    public float rotationSpeed;

    // Other Variables Needed
    private readonly float destroyHeight = -10f;

    private void Update()
    {
        MoveDown();
        RotateObject();
    }

    private void MoveDown()
    {
        baseSpeed += fallAcceleration * Time.deltaTime;
        transform.Translate((baseSpeed * Time.deltaTime * -Vector3.up), Space.World);

        if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }

    private void RotateObject()
    {
        Vector3 rotationVector = new(
            rotateX ? rotationSpeed * Time.deltaTime : 0f,
            rotateY ? rotationSpeed * Time.deltaTime : 0f,
            rotateZ ? rotationSpeed * Time.deltaTime : 0f
        );
        transform.Rotate(rotationVector, Space.Self);
    }
}