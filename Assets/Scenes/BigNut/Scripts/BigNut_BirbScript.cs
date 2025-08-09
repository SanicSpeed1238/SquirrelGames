using UnityEngine;

public class BigNut_BirbScript : MonoBehaviour
{
    public BigNut_ComputerController computerController;

    [Header("Bird Movement Components")]
    public float spawnTimeLowerBound = 3.0f;
    public float spawnTimeUpperBound = 7.0f;
    public float maxDistanceFromCharacter = 100f;
    public float birbApproachSpeed = 0.1f;
    public float birbAwaySpeed = 0.3f;

    // For debugging
    public bool birbEnabled = true;

    private bool reachedPlayer = false;
    private bool spawned = false;
    private bool isComputer = false;
    private float spawnTime;

    private Transform birbDestination;
    private Transform birbSource;
    private Transform shadow;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        shadow.localScale = Vector3.zero;
        spawnTime = Random.Range(spawnTimeLowerBound, spawnTimeUpperBound);
        transform.position = birbSource.position;

        computerController = player.GetComponent<BigNut_ComputerController>();
        if (computerController != null)
        {
            isComputer = true;
        }

        birbDestination = GameObject.FindFirstObjectByType<BigNut_BirdDestinationScript>().transform;
    }


    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
        {
            return;
        }

        if (birbEnabled)
        {
            if (spawned)
            {
                if (Vector3.Distance(transform.position, birbDestination.position) <= 0.1)
                {
                    shadow.localScale = Vector3.zero;
                    spawned = false;
                    BigNut_Controller playerController = player.GetComponent<BigNut_Controller>();
                    if (!playerController.IsAlive)
                    {
                        Destroy(shadow.gameObject);
                        Destroy(gameObject);
                    }
                    else if (isComputer)
                    {
                        computerController.BirdGone();
                    }
                }
                if (!reachedPlayer)
                {
                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, birbApproachSpeed);
                    shadow.localScale += new Vector3(0.006f, 0, 0.006f);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, birbDestination.position, birbAwaySpeed);
                    if (shadow.localScale.x > 0)
                    {
                        shadow.localScale -= new Vector3(0.01f, 0, 0.01f);
                    }
                }
            }
            else
            {
                spawnTime -= Time.deltaTime;
                if (spawnTime <= 0)
                {
                    transform.position = birbSource.position;
                    spawned = true;
                    spawnTime = Random.Range(spawnTimeLowerBound, spawnTimeUpperBound);
                    reachedPlayer = false;
                    if (isComputer)
                    {
                        computerController.isBirdComing = true;
                    }
                }
            }
        }        
    }

    public void Initialize(GameObject player, Transform source)
    {
        this.player = player;
        birbSource = source;
        shadow = player.transform.Find("BirbShadow");
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject == player)
        {
            reachedPlayer = true;
            player.GetComponent<BigNut_Controller>().Squish();
        }
    }
}
