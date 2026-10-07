using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFight_StateMachineMB : MonoBehaviour
{

    public State CurrState { get; private set; }
    public State PrevState { get; private set; }

    private bool _inTransition = false;

    public void ChangeState(State newState)
    {
        if (CurrState == newState || _inTransition)
        {
            return;
        }
        ChangeStateSequence(newState);
    }

    private void ChangeStateSequence(State newState)
    {
        _inTransition = true;
        CurrState?.Exit();
        StoreStateAsPrevious(newState);

        CurrState = newState;

        CurrState?.Enter();
        _inTransition = false;
    }

    public void StoreStateAsPrevious(State newState)
    {
        if (PrevState == null && newState != null)
        {
            PrevState = newState;
        }
        else if (PrevState != null && CurrState != null)
        {
            PrevState = CurrState;
        }
    }

    public void ChangeStateToPrevious()
    {
        if (PrevState != null)
        {
            ChangeState(PrevState);
        }
        else
        {
            Debug.LogWarning("This is no previous state to change to!");
        }
    }

    protected virtual void Update()
    {
        if (CurrState != null && !_inTransition)
        {
            CurrState.Tick();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (CurrState != null && !_inTransition)
        {
            CurrState.FixedTick();
        }
    }

    protected virtual void OnDestroy()
    {
        CurrState?.Exit();
    }

}
