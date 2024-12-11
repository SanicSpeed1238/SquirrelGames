using UnityEngine;

public class AntSmash_ObjectScript : MonoBehaviour
{
    public enum ObjectType
    {
        Squirrel,
        Ant
    }

    [Header("Gameplay Attributes")]
    public ObjectType objType = ObjectType.Ant;
    [Range(1f, 10f)]
    public float moveSpeed;
    [Range(-3, 3)]
    public int points;

    [Header("Squish Attributes")]
    public float squishTime;
    private float squishTimer;
    public Vector3 squishScale;
    private Vector4 transparentValue;
    public Material transparentMaterial;
    private bool isSquished;

    private void Update()
    {
        if (!isSquished) // If object is still alive, have them move forward
        {
            transform.Translate((moveSpeed * Time.deltaTime * transform.forward), Space.World);
        }
        else // Show squish animation if object has been squished by the player
        {
            ShowSquished();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Destroy object if it goes too far offscreen
        if (other.CompareTag("Deadzone"))
        {
            Destroy(gameObject);
        }
    }

    public void SquishObject() // Starts the squish animation and reconfigures components
    {
        isSquished = true;
        squishTimer = squishTime;
        moveSpeed = 0f;
        transform.localScale = squishScale;

        gameObject.GetComponentInChildren<Renderer>().material = transparentMaterial;
        transparentValue = GetComponentInChildren<Renderer>().material.color;
        GetComponentInChildren<Animator>().enabled = false;
        GetComponentInChildren<BoxCollider>().enabled = false;
    }
    private void ShowSquished() // Plays the squish animation and destroys object when done
    {
        squishTimer -= Time.deltaTime;
        if (squishTimer < 1)
        {
            transparentValue.w = squishTimer;
            gameObject.GetComponentInChildren<Renderer>().material.color = transparentValue;
        }
        if (squishTimer < 0)
        {
            Destroy(gameObject);
        }
    }
}