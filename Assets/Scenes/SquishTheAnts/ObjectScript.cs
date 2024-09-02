using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    public enum ObjectType
    {
        None,
        Squirrel,
        Ant
    }
    public ObjectType objType = ObjectType.None;

    [Range(1f, 10f)]
    public float moveSpeed;

    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * transform.forward), Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Deadzone"))
        {
            Destroy(gameObject);
        }
    }
}