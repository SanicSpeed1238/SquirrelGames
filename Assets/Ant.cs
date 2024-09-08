using UnityEngine;

public class Ant : MonoBehaviour
{
    public enum ObjectType
    {
        Ant
    }
    public ObjectType objType = ObjectType.Ant;
    public float AntSquishTime = 1.5f; // how long the ant spends squished
    public Material TransparentAnt;
    public Vector4 Transparency;
    private bool AntSquished;
    [Range(1f, 10f)]
    public float moveSpeed;

    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * transform.forward), Space.World);
        if (AntSquished)
        {
            AntSquishTime -= Time.deltaTime;
            if (AntSquishTime < 1)
            {
                Transparency.w = AntSquishTime;
                gameObject.GetComponentInChildren<Renderer>().material.color = Transparency;
            }
            if (AntSquishTime < 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Deadzone"))
        {
            Destroy(gameObject);
        }
    }
    public void onAntSquish()
    {
        AntSquished = true;
        gameObject.GetComponentInChildren<Renderer>().material = TransparentAnt;
        Transparency = GetComponentInChildren<Renderer>().material.color;
    }
}