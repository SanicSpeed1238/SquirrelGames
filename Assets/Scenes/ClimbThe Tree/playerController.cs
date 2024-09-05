using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class playerController : MonoBehaviour
{

    public float moveSpeed = 5f;
    public Rigidbody rb;
    Vector3 movement;
    Vector3 mousePos;
    string A;
    string D;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.W) == true)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
            //print("Works");

        }
        //rb.MovePosition(rb.position + (movement.y * 10));
    }

}
