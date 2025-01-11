using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HungrySquirrels_ArmMovement : MonoBehaviour
{
    //Controls Speed & limit of arm extension for character
    public float ext_Speed = 10f;
    public float arm_Max;
    public float arm_Min;

    //Setting bools for if the object is within touching distance of another object for collision
    public bool stopright;
    public bool stopleft;
    public bool stoptop;
    public bool stopbottom;

    private Rigidbody rb;

    //Input needed for arm
    private float moveInput;
    float translate;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();//The Arm Area(Kind of obsolete with the transform code)
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = Input.GetAxis("Vertical");//Forward & Backward Input
        ExtendArm(moveInput);//Extending arm code moved to here instead of fixed update due to the arm locking up at the minimum and maximums.

    }

    private void FixedUpdate()
    {
        //Raycasting for collision with walls or any solids(currently disabled in the mainbody/rotation script)
        stopright = Physics.Raycast(transform.position, Vector3.right, .67f);
        stopleft = Physics.Raycast(transform.position, Vector3.left, .67f);
        stoptop = Physics.Raycast(transform.position, Vector3.forward, .67f);
        stopbottom = Physics.Raycast(transform.position, Vector3.back, .67f);
    }

    void ExtendArm(float Input)
    {
        //Arm extend & dextend code
        translate = Input * ext_Speed * Time.fixedDeltaTime;
        if (transform.localPosition.x < arm_Min)
        {
            transform.localPosition = new Vector3(arm_Min, 0, 0);
        }
        else if (transform.localPosition.x > arm_Max)
        {
            transform.localPosition = new Vector3(arm_Max, 0, 0);
        }
        else
        {
            transform.Translate(translate, 0, 0, Space.Self);
        }
    }

}
