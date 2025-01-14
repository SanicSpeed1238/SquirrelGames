using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class HungrySquirrels_PlayerController : MonoBehaviour
{
    //Controls the speed of rotation for character
    public float rot_Speed = 5f;
    public float ext_Speed = 0.2f;

    //Shows which player the character is connected to for multiplayer(if it is viable)
    public int playerID = 0;

    //Referencing the arm to stop movement when collision with wall or player happens
    public GameObject arm;
    public BoxCollider hitBox;
    protected float translate;
    protected bool pressed;

    private float rotationInput;
    protected float curr_Rot;
    
    private float moveInput;
    protected bool collectActive;
    public bool collided;
    // Start is called before the first frame update
    void Start()
    {
        hitBox = arm.GetComponent<BoxCollider>();
        //Sets default position and rotation of character when chosen as a specific playerID
        switch (playerID)
        {
            case 0:
                transform.position = new Vector3(18, 5, 18);
                transform.localEulerAngles = new Vector3 (0, 135, 0);
                break;
            case 1:
                transform.position = new Vector3(-18, 5, 18);
                transform.localEulerAngles = new Vector3(0, 45, 0);
                break;
            case 2:
                transform.position = new Vector3(-18, 5, -18);
                transform.localEulerAngles = new Vector3(0, -45, 0);
                break;
            case 3:
                transform.position = new Vector3(18, 5, -18);
                transform.localEulerAngles = new Vector3(0, -135, 0);
                break;
            default:
                transform.position = new Vector3(0, 5, 0);
                transform.localEulerAngles = new Vector3(0, 135, 0);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        rotationInput = Input.GetAxis("Horizontal");//Left & Right input
        moveInput = Input.GetAxis("Vertical");//Forward & Backward Input
        RotateArm(rotationInput);//More consistent rotation than in fixed update
        ExtendArm(moveInput);//Moving Arm Input(I need to apply button input for simplicity sake)
    }

    void RotateArm(float Input)
    {
        float rotation = Input * rot_Speed * Time.deltaTime * 60;//Applies rot_speed to input
        if (pressed == false)
        {
            //Limit's The Rotation of arm so they can stay within the box
            if ((rotationInput < 0 && curr_Rot > -50) || (rotationInput > 0 && curr_Rot < 50))
            {
                //Moves the arm by using transforms instead of the actual rigidbody physics(removes problem of flinging by clipping)
                transform.Rotate(Vector3.up, rotation);
                curr_Rot += rotation;//Updates Current Rotation for limit checkers
            }
        }
    }

    void ExtendArm(float Input)
    {
        //Arm extend & dextend code
        translate = ext_Speed * Time.deltaTime * 60;
        if (Input > 0)
        {
            pressed = true;
        }

        if (pressed == true)
        {
            if (collided == true)
            {
                arm.transform.Translate(-translate, 0, 0, Space.Self);
            }
            else
            {
                arm.transform.Translate(translate, 0, 0, Space.Self);
            }
        }

        if (transform.localPosition.x < 0)
        {
            collectActive = false;
            collided = false;
            pressed = false;
            arm.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Pickup":
                //if (moveInput > 0 && collectActive == false)
                //{
                //    collectActive = true;
                //}
                //if (collectActive == true)
                //{
                    collided = true;
                    Destroy(other);
                //    print("Works");
                //}
                //else
                //{
                //    print("Uh Oh");
                //}
                break;
            case "Player":
                collided = true;
                break;
            default:
                collided = true;
                break;
        }

    }
}
