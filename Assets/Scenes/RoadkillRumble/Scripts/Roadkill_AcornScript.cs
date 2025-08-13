using UnityEngine;

public class Roadkill_AcornScript : MonoBehaviour
{
    [Header("Attributes")]
    [Range(1,3)]
    public int points;
    [Range(1f, 20f)]
    public float initialRotation;
    public GameObject collectEffect;
    public GameObject destroyEffect;

    private void Start()
    {
        ApplyInitialRotation();
    }

    public void CollectAcorn(Vector3 playerPosition)
    {
        Instantiate(collectEffect, playerPosition, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Roadkill_CarCollider>(out var carCollider))
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void ApplyInitialRotation()
    {
        float x = Random.value < 0.5f ? -1f : 1f;
        float z = Random.value < 0.5f ? -1f : 1f;
        Vector3 rotationDirection = new(x, 0f, z);

        GetComponent<Rigidbody>().AddTorque(rotationDirection * 10f, ForceMode.Impulse);
    }
}