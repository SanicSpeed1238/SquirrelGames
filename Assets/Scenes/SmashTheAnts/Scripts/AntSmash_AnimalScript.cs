using UnityEngine;

public class AntSmash_AnimalScript : MonoBehaviour
{
    [Header("Gameplay Attributes")]
    [Range(1f, 10f)]
    public float moveSpeed;
    [Range(-3, 3)]
    public int points;

    [Header("Squish Attributes")]
    [Range(0.1f, 1f)]
    public float squishedDuration;
    public Vector3 squishedScale;
    public Material squishedMaterial;
    public GameObject smashedEffect;

    // Other Variables Needed
    private bool isSquished;
    private float squishedTimer;
    private Renderer materialRender;
    private Vector4 transparentValue;

    private void Start()
    {
        materialRender = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        if (!isSquished)
        {
            transform.Translate((moveSpeed * Time.deltaTime * transform.forward), Space.World);
        }
        else
        {
            ShowSquished();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Deadzone"))
        {
            Destroy(gameObject);
        }
    }

    public void SquishObject() // Starts the squish animation and reconfigures components
    {
        isSquished = true;

        moveSpeed = 0f;
        squishedTimer = squishedDuration + 1f;

        GetComponent<CS_SquishModel>().SquishPermanently(squishedScale);
        materialRender.material = squishedMaterial;
        transparentValue = materialRender.material.color;

        GetComponentInChildren<Animator>().enabled = false;
        GetComponentInChildren<BoxCollider>().enabled = false;

        Instantiate(smashedEffect, transform.position, Quaternion.identity);
    }
    private void ShowSquished() // Plays the squish animation and destroys object when done
    {
        squishedTimer -= Time.deltaTime;
        if (squishedTimer < 1)
        {
            transparentValue.w = squishedTimer;
            materialRender.material.color = transparentValue;
        }
        if (squishedTimer < 0)
        {
            Destroy(gameObject);
        }
    }
}