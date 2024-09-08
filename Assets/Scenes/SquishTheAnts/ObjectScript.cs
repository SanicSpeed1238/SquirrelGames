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
    public float SquishTime = 1.5f; // how long the ant spends squished
    public Material TransparentAnt;
    public Vector4 Transparency;
    private bool Squished;

    [Range(1f, 10f)]
    public float moveSpeed;

    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * transform.forward), Space.World);
        if (Squished)
        {
            SquishTime -= Time.deltaTime;
            if (SquishTime < 1)
            {
                Transparency.w = SquishTime;
                gameObject.GetComponentInChildren<Renderer>().material.color = Transparency;
            }
            if (SquishTime < 0)
            {
                Destroy(gameObject);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Deadzone"))
        {
            Destroy(gameObject);
        }
    }
    public void onSquish()
    {
        Squished = true;
        gameObject.GetComponentInChildren<Renderer>().material = TransparentAnt;
        Transparency = GetComponentInChildren<Renderer>().material.color;
    }

}