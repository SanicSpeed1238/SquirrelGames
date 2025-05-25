using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{

    [SerializeField] AudioClip _pSound = null;

    protected abstract void Collect();

}
