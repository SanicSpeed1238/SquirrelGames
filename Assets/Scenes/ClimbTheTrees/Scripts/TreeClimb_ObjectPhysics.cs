using UnityEngine;

public class TreeClimb_ObjectPhysics : MonoBehaviour
{
    [Header("Movement Values")]
    public float moveSpeed;

    [Header("Rotation Values")]
    public bool rotateObject;
    public float rotationSpeed;

    // Other Variables Needed
    private readonly float deadHeight = -10;

    private void Update()
    {
        MoveDown();
        RotateObject();
    }

    private void MoveDown()
    {
        transform.Translate((moveSpeed * Time.deltaTime * -transform.up), Space.World);

        if (transform.position.y < deadHeight)
        {
            Destroy(gameObject);
        }
    }

    private void RotateObject()
    {
        if (rotateObject)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}