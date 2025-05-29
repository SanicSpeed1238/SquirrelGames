using UnityEngine;

public class BigNut_BirbScript : MonoBehaviour
{
    public GameObject player;

    public BigNut_GameManagerScript gameManagerScript;

    [Header("Bird Movement Components")]
    public Transform birbDestination;
    public Transform birbSource;

    public Transform shadow;

    public float maxDistanceFromCharacter = 100f;


    private bool reachedPlayer = false;
    private bool spawned = false;
    private float spawnTime;

    // Start is called before the first frame update
    void Start()
    {
        shadow.localScale = Vector3.zero;
        spawnTime = Random.Range(1, 4);
        transform.position = birbSource.position;
    }


    // Update is called once per frame
    void Update()
    {
        if (spawned)
        {

            if (Vector3.Distance(transform.position, birbDestination.position) == 0 || Vector3.Distance(transform.position, player.transform.position) > maxDistanceFromCharacter)
            {
                shadow.localScale = Vector3.zero;
                spawned = false;
                BigNut_PlayerController playerController = player.GetComponent<BigNut_PlayerController>();
                if (!playerController.IsAlive)
                {
                    gameManagerScript.PlayerKilled(player.name);
                    Destroy(player);
                    Destroy(shadow);
                    Destroy(gameObject);
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
                spawnTime = Random.Range(1, 4);
                reachedPlayer = false;
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
