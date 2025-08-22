using UnityEngine;
using UnityEngine.AI;

public class BigNut_ComputerController : BigNut_Controller
{
    public float moveSpeed = 1f;
    public NavMeshAgent agent;

    public bool isBirdComing = false;

    private Transform acorn;
    private GameObject hidyHole;

    // Start is called before the first frame update
    void Start()
    {
        acorn = GameObject.FindGameObjectWithTag("Acorn").transform;
        agent.SetDestination(acorn.position);
        agent.speed = 0;
        squirrelAnimator.speed = 0;

        BirdInitialization();
    }

    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
        {
            return;
        }
        agent.speed = moveSpeed;
        

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (isAlive)
        {
            if (!isHiding)
            {
                if (!isStunned)
                {
                    squirrelAnimator.speed = 1;
                    if (isBirdComing)
                    {
                        Vector3 destinationViewPort = mainCamera.WorldToViewportPoint(agent.destination);
                        if (hidyHole != null)
                        {
                            BigNut_HidyHole hidyHoleScript = hidyHole.GetComponent<BigNut_HidyHole>();
                            if (hidyHoleScript.NearbyPlayerCount >= 3)
                            {
                                hidyHole = null;
                            }
                        }

                        if (hidyHole == null || destinationViewPort.y <= 0.3 || destinationViewPort.y > 1)
                        {
                            Vector3 closestHole = Vector3.zero;
                            float minDistance = float.MaxValue;
                            BigNut_HidyHole[] hidyHoles = GameObject.FindObjectsByType<BigNut_HidyHole>(FindObjectsSortMode.None);
                            foreach (BigNut_HidyHole hole in hidyHoles)
                            {
                                Vector3 holeViewPort = mainCamera.WorldToViewportPoint(hole.transform.position);
                                float distance = Vector3.Distance(hole.transform.position, transform.position);
                                if (minDistance > distance && holeViewPort.y > 0.1 && holeViewPort.y < 1 && hole.NearbyPlayerCount < 3)
                                {
                                    minDistance = distance;
                                    hidyHole = hole.gameObject;
                                }
                            }
                            if (hidyHole == null)
                            {
                                hidyHole = acorn.gameObject;
                            }
                            agent.SetDestination(hidyHole.transform.position);
                        }
                        if (hidyHole != null)
                        {
                            if (Vector3.Distance(hidyHole.transform.position, transform.position) < 2)
                            {
                                HideAndUnhide();
                            }
                        }
                    }
                }
                else
                {
                    HandleStunning();
                }
            }
            ConstraintPlayerInCamera();
        }
    }

    public void BirdGone()
    {
        if (isHiding) 
        {
            HideAndUnhide();
        }       
        isBirdComing = false;
        agent.enabled = true;
        agent.SetDestination(acorn.position);
        hidyHole = null;
        squirrelAnimator.speed = 1;
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
        else if (hidyHoleScript != null && hidyHoleScript.CanHide)
        {

            agent.enabled = false;
            squirrelAnimator.speed = 0;

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

    public override void Squish()
    {
        if (!isHiding)
        {
            isAlive = false;
            agent.enabled = false;
            squirrelAnimator.speed = 0;
            transform.localScale = new Vector3(transform.localScale.x, 0.4f, transform.localScale.z);
            MG_GameManager.Instance.UpdatePlayerQualified(this.playerID, false);
            Invoke("DestroySquirrel", 3);            
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Acorn")
        {
            MG_GameManager.Instance.UpdatePlayerQualified(playerID, true);
            hasWon = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (isHiding && collision.gameObject.GetComponent<BigNut_HidyHole>() != null)
        {
            HideAndUnhide();
            agent.enabled = true;
            squirrelAnimator.speed = 1;
        }
    }

    private void HandleStunning()
    {
        currentStunTime -= Time.deltaTime;
        if (currentStunTime < 0)
        {
            isStunned = false;
            agent.enabled = true;
            squirrelAnimator.speed = 1;
        }
    }

    private void ConstraintPlayerInCamera()
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        Vector3 clampPosition = transform.position;
        if (viewportPos.y <= 0)
        {
            clampPosition = transform.position + Vector3.right;
        }
        else if (viewportPos.y > bufferFromForward)
        {
            Debug.Log("here");
            Vector3 clampedWorld = mainCamera.ViewportToWorldPoint(new Vector3(bufferFromForward, viewportPos.y, viewportPos.z));
            clampPosition = new Vector3(clampedWorld.x, transform.position.y, transform.position.z);
        }
        else
        {
            return;
        }
        if (agent.enabled)
        {
            agent.Warp(clampPosition);
        }
        else
        {
            transform.position = clampPosition;
        }
    }
}
