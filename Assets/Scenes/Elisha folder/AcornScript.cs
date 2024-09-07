using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcornScript : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    void Start()
    {
        rb.angularVelocity = new Vector3(0,3,0); 
    }

    void OnTriggerEnter(Collider collision)
    {
        if(collision.tag == "Player") 
        {
            collision.GetComponent<AcornCounter>().acorns += 1; 
        }
        Debug.Log(collision.GetComponent<AcornCounter>().acorns); 
        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        
    }
}
