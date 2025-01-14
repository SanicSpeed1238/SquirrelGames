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

    //Input needed for arm
    public float moveInput;
    public BoxCollider hitBox;
    public bool pressed;
    protected bool collectActive;
    protected bool collided;
    protected float translate;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        hitBox = GetComponent<BoxCollider>();
        moveInput = Input.GetAxis("Vertical");//Forward & Backward Input
        ExtendArm(moveInput);//Extending arm code moved to here instead of fixed update due to the arm locking up at the minimum and maximums.
    }

    private void FixedUpdate()
    {

    }

    void ExtendArm(float Input)
    {
        //Arm extend & dextend code
        translate = ext_Speed * Time.fixedDeltaTime;
        if(Input > 0)
        {
            pressed = true;
        }

        if (pressed == true)
        {
            if (collided == true)
            {
                transform.Translate(-translate, 0, 0, Space.Self);
            }
            else
            {
                transform.Translate(translate, 0, 0, Space.Self);
            }
        }

        if(transform.localPosition.x < arm_Min)
        {
            collectActive = false;
            collided = false;
            pressed = false;
            transform.localPosition= new Vector3 (arm_Min, 0, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag) 
        {
            case "Pickup":
                if(moveInput > 0 && collectActive == false)
                {
                    collectActive = true;
                }
                if(collectActive == true)
                {
                    collided = true;
                    Destroy(other);
                    print("Works");
                }
                break;
            case "Player":
                collided = true;
                break;
            default:
                collided=true;
                break;
        }

    }

    void reboundArm()
    {

    }

}
