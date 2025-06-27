using UnityEngine;

[RequireComponent(typeof(BushShake))]
public class AntSmash_BushScript : MonoBehaviour
{
    private BushShake bushShake;
    private AudioSource bushSound;

    public float targetStrength;
    public float targetSpeed;
    public float shakeDuration;

    private void Start()
    {
        bushShake = GetComponent<BushShake>();
        bushSound = GetComponent<AudioSource>();
    }

    public void ShakeBush()
    {
        bushShake.ShakeBush(targetStrength, targetSpeed, shakeDuration);
        bushSound.PlayOneShot(bushSound.clip);
    }
}