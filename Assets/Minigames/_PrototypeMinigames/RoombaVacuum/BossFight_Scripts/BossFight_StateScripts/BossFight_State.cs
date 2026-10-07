using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{

    public float StateDur { get; private set; } = 0;

    public virtual void Enter()
    {
        StateDur = 0;
    }

    // for Physics
    public virtual void FixedTick()
    {

    }

    //For Update
    public virtual void Tick()
    {
        StateDur += Time.deltaTime;
    }

    /*public virtual void Fixed()
    {

    }*/

    public virtual void Exit()
    {

    }

}