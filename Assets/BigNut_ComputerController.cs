using UnityEngine;
using UnityEngine.AI;

public class BigNut_ComputerController : BigNut_Controller
{
    public float speed = 2.0f;
    public Transform acorn;
    public NavMeshAgent agent;

    public bool isBirdComing = false;

    private GameObject hidyHole;

    // Start is called before the first frame update
    void Start()
    {
        agent.SetDestination(acorn.position);
        agent.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive) 
        {
            transform.position = birbThatCaught.position;
        }
        else if (!isHiding)
        {
            if (!isStunned)
            {
                if (isBirdComing)
                {
                    Vector3 destinationViewPort = mainCamera.WorldToViewportPoint(agent.destination);
                    if (hidyHole == null || destinationViewPort.x < 0)
                    {
                        Vector3 closestHole = Vector3.zero;
                        float minDistance = float.MaxValue;
                        foreach (GameObject hole in BigNut_HidyHole.HidyHoles)
                        {
                            Vector3 holeViewPort = mainCamera.WorldToViewportPoint(hole.transform.position);
                            float distance = Vector3.Distance(hole.transform.position, transform.position);
                            if (minDistance > distance && holeViewPort.x > 0)
                            {
                                minDistance = distance;
                                closestHole = hole.transform.position;
                                hidyHole = hole;
                            }
                        }
                        agent.SetDestination(closestHole);
                    }

                    if (agent.remainingDistance < 2)
                    {
                        agent.enabled = false;
                        HideAndUnhide();
                    }
                }
            }
            else
            {
                HandleStunning();
            }
        }
    }

    public void BirdGone()
    {
        HideAndUnhide();
        isBirdComing = false;
        agent.enabled = true;
        agent.SetDestination(acorn.position);
        hidyHole = null;
    }

    public override void HideAndUnhide()
    {
        BigNut_HidyHole hidyHoleScript = hidyHole.GetComponent<BigNut_HidyHole>();
        Collider playerCollider = gameObject.GetComponent<Collider>();
        BoxCollider hidyHoleCollider = hidyHole.GetComponent<BoxCollider>();
        if (isHiding)
        {
            Vector3 outPosition = hidyHole.transform.position + Vector3.right * pushDistance;
            outPosition.y = 0;
            transform.position = outPosition;
            isHiding = false;
            hidyHoleScript.HidingPlayer = null;
            Physics.IgnoreCollision(playerCollider, hidyHoleCollider, false);
        }
        else
        {
            Physics.IgnoreCollision(playerCollider, hidyHoleCollider, true);

            GameObject hidingPlayer = hidyHoleScript.HidingPlayer;
            if (hidingPlayer != null && hidingPlayer != gameObject)
            {
                BigNut_Controller hidingPlayerController = hidingPlayer.GetComponent<BigNut_Controller>();
                hidingPlayerController.HideAndUnhide();
                hidingPlayerController.StunPlayer();
            }
            hidyHoleScript.HidingPlayer = this.gameObject;
            transform.position = new Vector3(hidyHole.transform.position.x, 0, hidyHole.transform.position.z);
            isHiding = true;
        }
    }

    private void OnTriggerEnter(Collider collision)
    { 
        if (!isHiding && collision.gameObject.tag == "Birb")
        {
            isAlive = false;
            birbThatCaught = collision.gameObject.transform;
            agent.enabled = false;
        }
    }

    private void HandleStunning()
    {
        currentStunTime -= Time.deltaTime;
        if (currentStunTime < 0)
        {
            isStunned = false;
            agent.enabled = true;
        }
    }
}
