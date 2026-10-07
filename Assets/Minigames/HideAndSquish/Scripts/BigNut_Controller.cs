using UnityEngine;

public abstract class BigNut_Controller : Base_PlayerController
{
    [Header("Camera Values")]
    public Camera mainCamera;
    public float bufferFromForward = 0.9f;

    [Header("Important Components")]
    public Animator squirrelAnimator;

    public GameObject BirdShadowPrefab;

    public float stunTime = 3.0f;

    protected bool isHiding = false;
    protected bool isAlive = true;
    protected bool hasWon = false;
    protected bool isStunned = false;
    protected float pushDistance = 2.0f;

    protected GameObject birdShadow;
    protected GameObject bird;

    protected float currentStunTime;

    public bool IsAlive { get => isAlive; }

    public bool HasWon { get => hasWon; }

    public abstract void HideAndUnhide();

    public abstract void Squish();

    public void StunPlayer()
    {
        isStunned = true;
        currentStunTime = stunTime;
        squirrelAnimator.speed = 0;
    }

    protected void BirdInitialization()
    {
        if (BirdShadowPrefab != null)
        {
            Vector3 shadowPosition = new Vector3(transform.position.x, transform.position.y + 10, transform.position.z);
            birdShadow = Instantiate(BirdShadowPrefab, shadowPosition, Quaternion.Euler(0f, -90f, 0f));
            birdShadow.GetComponent<BigNut_BirdShadow>().Instantiate(transform);
        }
        BigNut_SpawnBirb spawner = GameObject.FindAnyObjectByType<BigNut_SpawnBirb>();
        spawner.SpawnBirb(gameObject, birdShadow.transform);
    }

    protected void DestroySquirrel()
    {
        birdShadow.GetComponent<BigNut_BirdShadow>().UnInstantiate();
        Destroy(gameObject);
    }
}
