using UnityEngine;

public class Roadkill_CarScript : MonoBehaviour
{
    public float moveSpeed;

    private Rigidbody rigidBody;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        Vector3 moveDirection = transform.forward * moveSpeed;
    }

    private void FixedUpdate()
    {
        rigidBody.MovePosition(rigidBody.position + transform.forward * moveSpeed * Time.fixedDeltaTime);

        if (rigidBody.position.x > 50 || rigidBody.position.x < -50)
            Destroy(gameObject);
    }
}