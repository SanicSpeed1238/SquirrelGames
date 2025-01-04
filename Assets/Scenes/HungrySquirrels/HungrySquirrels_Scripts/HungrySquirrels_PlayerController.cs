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

    //public Rigidbody rb_MainBody;
    //public Rigidbody rb_Grabber;
    private Rigidbody rb;
    //private float moveInput;
    private float rotationInput;
    // Start is called before the first frame update
    void Start()
    {
        //rb_MainBody = GetComponent<Rigidbody>();//Rotatey Bit/Huge Bit
        //rb_Grabber = GetComponent<Rigidbody>();//The Squirrel Grabby Bit's Rigidbody
        rb = GetComponent<Rigidbody>();//The Entire Player Character
    }

    // Update is called once per frame
    void Update()
    {
        //moveInput = Input.GetAxis("Vertical");//Forward & Backward Input
        rotationInput = Input.GetAxis("Horizontal");//Left & Right input

    }

    private void FixedUpdate()
    {

        RotateArm(rotationInput);
        //ExtendArm(moveInput);

    }

    void ExtendArm(float Input) 
    {
        //float translate = Input * ext_Speed * Time.fixedDeltaTime;
        //transform.Translate(translate, 0, 0, Space.Self);
        //rb_Grabber.MovePosition(rb_Grabber.position * translate);
    }

    void RotateArm(float Input)
    {
        float rotation = Input * rot_Speed * Time.fixedDeltaTime;
        //Quaternion turnRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        //rb_MainBody.MoveRotation(rb_MainBody.rotation * turnRotation);
        //rb.MoveRotation(rb.rotation * turnRotation);
        transform.Rotate(Vector3.up, rotation);
        print(rb.rotation);
    }
}
