using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float lifetime = 3f;

    private void Start()
    {
        lifetime = Mathf.Abs(lifetime);
        Destroy(gameObject, lifetime);
    }
}