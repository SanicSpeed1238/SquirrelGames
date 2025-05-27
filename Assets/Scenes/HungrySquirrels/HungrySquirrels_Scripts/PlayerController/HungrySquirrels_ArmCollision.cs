using UnityEngine;

public class HungrySquirrels_ArmCollision : MonoBehaviour
{
    //Input needed for arm
    public BoxCollider hitBox;
    public HungrySquirrels_PlayerController mainBody;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        hitBox = GetComponent<BoxCollider>();
    }

    private void OnTriggerStay(Collider other)
    {
        switch (other.gameObject.tag) 
        {
            /*case "Pickup":
                if (mainBody.moveInput == true && mainBody.collectActive == false)
                {
                    mainBody.collectActive = true;
                }
                if (mainBody.collectActive == true)
                {
                    other.gameObject.GetComponent<HungrySquirrels_Acorn>().AcornCollected(mainBody);
                }
                
                break;*/
            case "Player"://If player collides with other player arm, knock acorn up
                mainBody.collided = true;
                break;
            case "Hazard"://If player collides with hazard, stun for a second before retracting(?)
                print("AAAAAAA");
                break;
            default:
                mainBody.collided=true;//If player collided with wall, bounce back
                break;
        }

    }

}
