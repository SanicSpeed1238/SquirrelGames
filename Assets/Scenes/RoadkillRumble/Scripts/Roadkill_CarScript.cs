using UnityEngine;

public class Roadkill_CarScript : MonoBehaviour
{
    public float moveSpeed;

    private Rigidbody rigidBody;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        Vector3 moveDirection = transform.forward * moveSpeed;
        rigidBody.velocity = moveDirection;
    }

    private void FixedUpdate()
    {
        if (rigidBody.position.x > 50 || rigidBody.position.x < -50)
            Destroy(gameObject);
    }
}