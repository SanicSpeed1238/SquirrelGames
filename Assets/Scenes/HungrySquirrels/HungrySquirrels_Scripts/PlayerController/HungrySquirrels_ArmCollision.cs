using UnityEngine;

public class HungrySquirrels_ArmCollision : MonoBehaviour
{
    //Input needed for arm
    public BoxCollider hitBox;
    public HungrySquirrels_PlayerController mainBody;
    public GameObject acornModel;
    public GameObject screwModel;
    public GameObject bombModel;
    public HungrySquirrels_Acorn acorn;
    public HungrySquirrels_Bomb bomb;
    public HungrySquirrels_Bolt bolt;
    public int aOn;
    public bool bActive = false;

    [SerializeField] private ParticleSystem _explosionFX;
    [SerializeField] private ParticleSystem _fuseFX;
    private float _expPower = 20f;
    private float _expRadius = 10f;
    private float _expLiftPower = 10f;
    private bool doOnce = false;
    private float eTime;

    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (bActive == true)
        {
            if (doOnce == false)
            {
                _fuseFX.Play();
                doOnce = true;
            }
            eTime += Time.deltaTime;
            if (eTime >= bomb.bTimer)
            {
                _fuseFX.Stop();
                BombOnArmBoom();
                mainBody._stunActive = true;
                eTime = 0;
                doOnce = false;
                bActive = false;
            }
        }
    }

    private void BombOnArmBoom()
    {
        Vector3 ePos = this.transform.position;

        bombModel.SetActive(false);
        _explosionFX.Play();

        Collider[] pickups = Physics.OverlapSphere(ePos, 10);

        //Collider[] players = Physics.OverlapSphere(ePos, 10, _playerLayer);

        foreach (Collider hit in pickups)
        {

            if (hit.tag == "Pickup" || hit.tag == "Hazard")
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddExplosionForce(_expPower, ePos, _expRadius, _expLiftPower, ForceMode.Impulse);
                }

            }
            else if (hit.tag == "Player")
            {
                HungrySquirrels_ArmCollision arm = hit.GetComponent<HungrySquirrels_ArmCollision>();

                if (arm != null)
                {
                    arm.mainBody._stunActive = true;
                }
            }

        }

    }

    private Vector3 RandomRotation()
    {
        Vector3 ranRot;

        ranRot.x = Random.Range(-5, 5);
        ranRot.y = Random.Range(-5, 5);
        ranRot.z = Random.Range(-5, 5);

        return ranRot;
    }

    private void OnTriggerEnter(Collider other)
    {
        //other.gameObject.CompareTag("Player");
        switch (other.gameObject.tag) 
        {
            case "Player"://If player collides with other player arm, knock acorn up
                //Debug.Log("ENEMY");
                if (aOn == 2)
                {
                    HungrySquirrels_Acorn flungAcorn = Instantiate(acorn, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
                    flungAcorn.aInitialize();
                    acornModel.SetActive(false);
                    aOn = 0;
                }
                else if (aOn == 1)
                {
                    HungrySquirrels_Bolt flungBolt = Instantiate(bolt, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
                    flungBolt.aInitialize();
                    screwModel.SetActive(false);
                    aOn = 0;
                }
                else if (bActive == true)
                {
                    _fuseFX.Stop();
                    HungrySquirrels_Bomb flungBomb = Instantiate(bomb, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
                    flungBomb.bInitialize(eTime);
                    eTime = 0;
                    bombModel.SetActive(false);
                    bActive = false;
                }
                mainBody.collided = true;
                break;
            case "Wall":
                //Debug.Log("WALL");
                mainBody.collided=true;//If player collided with wall, bounce back
                break;
        }

    }

}
