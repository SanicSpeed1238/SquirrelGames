using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class HungrySquirrels_PlayerController : MonoBehaviour
{
    //Controls the speed of rotation for character
    public float rot_Speed = 120f;
   
    //Shows which player the character is connected to for multiplayer(if it is viable)
    public int playerID = 0;

    //Referencing the arm to stop movement when collision with wall or player happens
    public HungrySquirrels_ArmMovement arm;

    private float rotationInput;
    // Start is called before the first frame update
    void Start()
    {
        arm = FindObjectOfType<HungrySquirrels_ArmMovement>();

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
        RotateArm(rotationInput);//More consistent rotation than in fixed update
    }

    private void FixedUpdate()
    {

    }

    void RotateArm(float Input)
    {
        float rotation = Input * rot_Speed * Time.fixedDeltaTime;//Applies rot_speed to input
        if (arm.pressed == false)
        {
            transform.Rotate(Vector3.up, rotation);//Moves the arm by using transforms instead of the actual rigidbody physics(removes problem of flinging by clipping)
        }
        //print(rb.rotation);//Print statemenet was for Bug testing pruposes
    }
}
