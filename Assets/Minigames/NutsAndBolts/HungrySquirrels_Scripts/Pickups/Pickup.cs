using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{

    //[SerializeField] AudioClip _pSound = null;

    protected abstract void Collect(HungrySquirrels_ArmCollision arm);

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out HungrySquirrels_ArmCollision arm))
        {
            //AudioSource audioSource = AudioHelper.PlayClip2D(_pickupSound, 1, false);
            //audioSource.pitch = UnityEngine.Random.Range(.5f, 1);
            //play particles
            Collect(arm);
        }
    }

}
