using UnityEngine;

public class HungrySquirrels_ArmCollision : MonoBehaviour
{
    //Input needed for arm
    public BoxCollider hitBox;
    public HungrySquirrels_PlayerController mainBody;
    public HungrySquirrels_Acorn acorn;
    public int aOn;
    public float bTimer;
    public bool bActive;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        hitBox = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag) 
        {
            case "Pickup":
                /*if (mainBody.moveInput == true && mainBody.collectActive == false)
                {
                    mainBody.collectActive = true;
                }*/
                
                break;
            case "Player"://If player collides with other player arm, knock acorn up
                if (aOn != 0)
                {
                    HungrySquirrels_Acorn flungAcorn = Instantiate(acorn, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), Quaternion.identity);
                    flungAcorn.aInitialize(true);
                    aOn = 0;
                }
                mainBody.collided = true;
                break;
            /*case "Hazard"://If player collides with hazard, stun for a second before retracting(?)
                print("AAAAAAA");
                break;*/
             default:
                mainBody.collided=true;//If player collided with wall, bounce back
                //Debug.Log("Waka Waka");
                break;
        }

    }

}
