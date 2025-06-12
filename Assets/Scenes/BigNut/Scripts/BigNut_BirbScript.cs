using UnityEngine;

public class BigNut_BirbScript : MonoBehaviour
{
    public GameObject player;

    public BigNut_GameManagerScript gameManagerScript;
    public BigNut_ComputerController computerController;

    [Header("Bird Movement Components")]
    public Transform birbDestination;
    public Transform birbSource;
    public float spawnTimeLowerBound = 3.0f;
    public float spawnTimeUpperBound = 7.0f;
    public float maxDistanceFromCharacter = 100f;


    public Transform shadow;

    // For debugging
    public bool birbEnabled = true;

    private bool reachedPlayer = false;
    private bool spawned = false;
    private bool isComputer = false;
    private float spawnTime;

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
    }


    // Update is called once per frame
    void Update()
    {
        if (birbEnabled)
        {
            if (spawned)
            {

                if (Vector3.Distance(transform.position, birbDestination.position) == 0 || Vector3.Distance(transform.position, player.transform.position) > maxDistanceFromCharacter)
                {
                    shadow.localScale = Vector3.zero;
                    spawned = false;
                    BigNut_Controller playerController = player.GetComponent<BigNut_Controller>();
                    if (!playerController.IsAlive)
                    {
                        gameManagerScript.PlayerKilled(player.name);
                        Destroy(player);
                        Destroy(shadow);
                        Destroy(gameObject);
                    }
                    else if (isComputer)
                    {
                        computerController.BirdGone();
                    }
                }

                if (!reachedPlayer)
                {
                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, 0.02f);
                    shadow.localScale += new Vector3(0.005f, 0, 0.005f);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, birbDestination.position, 0.08f);
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

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            reachedPlayer = true;
        }
    }
}
