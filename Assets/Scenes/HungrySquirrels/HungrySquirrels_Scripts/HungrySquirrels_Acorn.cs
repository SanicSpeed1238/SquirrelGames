using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_Acorn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //When the Acorn hits the grabber, it will give the player a point
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponentInParent<HungrySquirrels_ArmMovement>()!=null)
        {
            
        }
    }
}
