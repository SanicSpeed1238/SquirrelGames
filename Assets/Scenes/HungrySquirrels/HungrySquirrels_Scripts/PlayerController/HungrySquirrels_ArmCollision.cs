using UnityEngine;

public class HungrySquirrels_ArmCollision : MonoBehaviour
{
    //Input needed for arm
    public BoxCollider hitBox;
    public HungrySquirrels_PlayerController mainBody;
    public HungrySquirrels_Acorn acorn;
    public HungrySquirrels_Bomb bomb;
    public int aOn;
    public bool bActive = false;

    private float eTime;

    private void Start()
    {
        hitBox = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bActive == true)
        {
            eTime += Time.deltaTime;
            if (eTime >= bomb.bTimer)
            {
                mainBody._stunActive = true;
                bActive = false;
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
        switch (other.gameObject.tag) 
        {
            case "Pickup":

                break;
            case "Player"://If player collides with other player arm, knock acorn up
                if (aOn != 0)
                {
                    HungrySquirrels_Acorn flungAcorn = Instantiate(acorn, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
                    flungAcorn.aInitialize();
                    aOn = 0;
                }
                else if (bActive == true)
                {
                    HungrySquirrels_Bomb flungBomb = Instantiate(bomb, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), Quaternion.FromToRotation(RandomRotation(), RandomRotation()));
                    flungBomb.bInitialize(eTime);
                    eTime = 0;
                    bActive = false;
                }
                mainBody.collided = true;
                break;
            case "Hazard"://If player collides with hazard, stun for a second before retracting(?)
                
                break;
            case "Wall":
                mainBody.collided=true;//If player collided with wall, bounce back
                break;
        }

    }

}
