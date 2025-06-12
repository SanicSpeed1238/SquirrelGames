using UnityEngine;
using UnityEngine.InputSystem.XR;

public abstract class BigNut_Controller : Base_PlayerController
{
    [Header("Camera Values")]
    public Camera mainCamera;

    public float stunTime = 2.0f;

    protected bool isHiding = false;
    protected bool isAlive = true;
    protected bool isStunned = false;
    protected float pushDistance = 2.0f;
    protected Transform birbThatCaught;

    private float currentStunTime;

    public bool IsAlive { get => isAlive; }

    public abstract void HideAndUnhide();

    public void StunPlayer()
    {
        isStunned = true;
        currentStunTime = stunTime;
    }

    public void HandleStunning()
    {
        currentStunTime -= Time.deltaTime;
        if (currentStunTime < 0)
        {
            isStunned = false;
        }
    }
}
