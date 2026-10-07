using UnityEngine;
using System.Collections;

public class AntSmash_AnimalScript : MonoBehaviour
{
    public enum AnimalType
    {
        Null,
        Ant,
        Squirrel,
    }

    [Header("Gameplay Attributes")]
    public AnimalType animalType = AnimalType.Null;
    [Range(5f, 15f)]
    public float moveSpeed;
    [Range(-3, 3)]
    public int points;

    [Header("Other Components")]
    public GameObject hitFX;

    // Other Variables Needed
    private bool isSquished;
    private Animator animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!isSquished)
        {
            transform.Translate((moveSpeed * Time.deltaTime * transform.forward), Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Deadzone"))
        {
            Destroy(gameObject);
        }
    }

    public void SquishObject()
    {
        isSquished = true;
        Instantiate(hitFX, transform.position, Quaternion.identity);

        moveSpeed = 0f;
        GetComponentInChildren<BoxCollider>().enabled = false;
        
        switch (animalType)
        {
            case AnimalType.Ant:
                Destroy(gameObject);
                break;

            case AnimalType.Squirrel:
                animator.SetTrigger("Smash");
                StartCoroutine(RemoveObject(0.6f));
                break;
        }   
    }

    IEnumerator RemoveObject(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        float currentScale = 1f;
        float shrinkSpeed = 10f;
        while (true)
        {
            currentScale -= Time.deltaTime * shrinkSpeed;
            if (currentScale > 0f)
            {
                transform.localScale = new(currentScale, currentScale, currentScale);
            }
            else
            {
                currentScale = 0f;
                transform.localScale = Vector3.zero;
                yield return new WaitForEndOfFrame();
                Destroy(gameObject);
            }

            yield return null;
        }
    }
}