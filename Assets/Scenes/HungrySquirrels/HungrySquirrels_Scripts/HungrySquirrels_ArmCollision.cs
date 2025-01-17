using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private void FixedUpdate()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag) 
        {
            case "Pickup":
                other.gameObject.GetComponent<HungrySquirrels_Acorn>().AcornCollected(mainBody);
                break;
            case "Player":
                mainBody.collided = true;
                break;
            default:
                mainBody.collided=true;
                break;
        }

    }

}
