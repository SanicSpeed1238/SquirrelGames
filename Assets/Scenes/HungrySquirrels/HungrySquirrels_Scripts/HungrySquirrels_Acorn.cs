using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_Acorn : MonoBehaviour
{

    //When the Acorn hits the grabber, it will give the player a point
    public void AcornCollected(HungrySquirrels_PlayerController player)
    {
        player.collided = true;

        Destroy(gameObject);
    }
}
