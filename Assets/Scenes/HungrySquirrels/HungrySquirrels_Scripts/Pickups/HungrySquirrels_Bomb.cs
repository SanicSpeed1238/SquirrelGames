using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_Bomb : Pickup
{

    [SerializeField] public float bTimer = 10;
    [SerializeField] private float _launchFCE = 10f;

    private Rigidbody _rb;

    protected override void Collect(HungrySquirrels_ArmCollision arm)
    {
        throw new System.NotImplementedException();
    }

    public void bInitialize(HungrySquirrels_ArmCollision arm)
    {

    }

}
