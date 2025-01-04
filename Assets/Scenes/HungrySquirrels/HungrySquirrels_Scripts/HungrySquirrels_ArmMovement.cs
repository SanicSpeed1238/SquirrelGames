using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_ArmMovement : MonoBehaviour
{
    //Controls Speed & limit of arm extension for character
    public float ext_Speed = 10f;
    public float arm_Max;
    public float arm_Min;
    private float curr_Arm_Length = 0f;

    private Rigidbody rb;

    //Input needed for arm
    private float moveInput;
    float translate;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();//The Arm Area
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = Input.GetAxis("Vertical");//Forward & Backward Input
    
    }

    private void FixedUpdate()
    {
        ExtendArm(moveInput);
    }

    void ExtendArm(float Input)
    {
        translate = Input * ext_Speed * Time.fixedDeltaTime;
        if (transform.localPosition.x <= arm_Min)
            transform.localPosition = new Vector3(arm_Min, 0, 0);
        else if (transform.localPosition.x >= arm_Max)
            transform.localPosition = new Vector3(arm_Max, 0, 0);
        else
            transform.Translate(translate, 0, 0, Space.Self);
        //rb_Grabber.MovePosition(rb_Grabber.position * translate);
    }
    void ShrinkArm(float Input)
    { 
        
    }
}
