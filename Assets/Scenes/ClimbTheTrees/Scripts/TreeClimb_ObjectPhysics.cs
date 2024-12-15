using UnityEngine;

public class TreeClimb_ObjectPhysics : MonoBehaviour
{
    [Header("Fall Speed Values")]
    public float baseSpeed;

    [Header("Rotation Values")]
    public bool rotateObject;
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
        transform.Translate((baseSpeed * Time.deltaTime * -transform.up), Space.World);

        if (transform.position.y < destroyHeight)
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