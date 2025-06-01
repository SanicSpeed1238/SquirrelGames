using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_Bomb : Pickup
{

    [SerializeField] public float bTimer = 10;
    protected override void Collect(HungrySquirrels_ArmCollision arm)
    {
        throw new System.NotImplementedException();
    }

    public void bInitialize(HungrySquirrels_ArmCollision arm)
    {

    }

}
