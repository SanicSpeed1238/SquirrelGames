using UnityEngine;

public class HammerController : MonoBehaviour
{
    [Header("Movement Controls")]
    [Range(1f, 20f)]
    public float moveSpeed;
    public float rangeX;
    public float rangeYTop;
    public float rangeYBottom;

    [Header("Important Components")]
    [SerializeField] private Animator hammerAnimator;
    [SerializeField] private ParticleSystem smashEffect;

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 newPosition = transform.position + moveSpeed * Time.deltaTime * new Vector3(horizontalInput, 0, verticalInput);
        newPosition.x = Mathf.Clamp(newPosition.x, -rangeX, rangeX);
        newPosition.z = Mathf.Clamp(newPosition.z, rangeYBottom, rangeYTop);
        transform.position = newPosition;

        if (Input.GetButtonDown("Action"))
        {
            SmashHammer();
        }
    }

    private void SmashHammer()
    {
        if(hammerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Idle"))
        {
            hammerAnimator.SetTrigger("Smash");
            smashEffect.Play();
        }      
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponentInParent<ObjectScript>() != null)
        {
            ObjectScript hitObject = other.GetComponentInParent<ObjectScript>();

            if(hitObject.objType == ObjectScript.ObjectType.Ant)
            {
                // Do Something
            }
            if(hitObject.objType == ObjectScript.ObjectType.Squirrel)
            {
                // Do Something
            }
            
            Destroy(other.gameObject);
        }
    }
}