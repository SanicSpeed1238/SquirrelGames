using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_PlayerController : MonoBehaviour
{
    //Controls the speed of rotation for character
    public float rot_Speed = 120f;
    //public float ext_Speed = 10f;
   
    //Shows which player the character is connected to for multiplayer(if it is viable)
    public int playerID = 0;

    private Rigidbody rb;

    private float rotationInput;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();//The Entire Player Character
        switch (playerID)
        {
            case 0:
                transform.position = new Vector3(18, 5, 18);
                break;
            case 1:
                transform.position = new Vector3(-18, 5, 18);
                break;
            case 2:
                transform.position = new Vector3(-18, 5, -18);
                break;
            case 3:
                transform.position = new Vector3(18, 5, -18);
                break;
            default:
                transform.position = new Vector3(0, 5, 0);
                break;
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        //moveInput = Input.GetAxis("Vertical");//Forward & Backward Input
        rotationInput = Input.GetAxis("Horizontal");//Left & Right input
        RotateArm(rotationInput);

    }

    private void FixedUpdate()
    {

    }

    void RotateArm(float Input)
    {
        float rotation = Input * rot_Speed * Time.fixedDeltaTime;
        //Quaternion turnRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        //rb_MainBody.MoveRotation(rb_MainBody.rotation * turnRotation);
        //rb.MoveRotation(rb.rotation * turnRotation);
        transform.Rotate(Vector3.up, rotation);
        //print(rb.rotation);
    }
}
