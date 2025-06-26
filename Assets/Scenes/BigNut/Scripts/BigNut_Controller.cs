using UnityEngine;

public abstract class BigNut_Controller : Base_PlayerController
{
    [Header("Camera Values")]
    public Camera mainCamera;
    public float bufferFromForward = 0.9f;

    [Header("Important Components")]
    public Animator squirrelAnimator;

    public float stunTime = 3.0f;

    protected bool isHiding = false;
    protected bool isAlive = true;
    protected bool isStunned = false;
    protected float pushDistance = 2.0f;
    protected Transform birbThatCaught;

    protected float currentStunTime;

    public bool IsAlive { get => isAlive; }

    public abstract void HideAndUnhide();

    public abstract void AttemptCatch(Transform birb);

    public void StunPlayer()
    {
        isStunned = true;
        currentStunTime = stunTime;
        squirrelAnimator.speed = 0;
    }
}
