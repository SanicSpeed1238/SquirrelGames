using UnityEngine;

public class SquirrelScript : MonoBehaviour
{
    public enum ObjectType
    {
        Squirrel,
    }
    public ObjectType objType = ObjectType.Squirrel;
    public float SquirrelDeathTime = .5f; //How long the animation for the blood explosion takes
    public Material TransparentSquirrel;
    private bool SquirrelSquished;
    public ParticleSystem smashEffect;
    [Range(1f, 10f)]
    public float moveSpeed;

    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * transform.forward), Space.World);
        if (SquirrelSquished)
        {
            SquirrelDeathTime -= Time.deltaTime;
            if (SquirrelDeathTime < 0)
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
    public void onSquirrelSquish()
    {
        if (!SquirrelSquished)
        {
            SquirrelSquished = true;
            gameObject.GetComponentInChildren<Renderer>().material = TransparentSquirrel;
            smashEffect.Play();
        }
    }
}