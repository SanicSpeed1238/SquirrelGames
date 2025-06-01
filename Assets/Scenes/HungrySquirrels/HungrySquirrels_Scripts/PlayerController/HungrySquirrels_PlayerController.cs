using UnityEngine;
using UnityEngine.InputSystem;

public class HungrySquirrels_PlayerController : MonoBehaviour
{
    //Controls the speed of rotation for character
    public float rot_Speed = 3f;
    //Controls Arm Speed
    public float ext_Speed = 0.2f;

    //Shows which player the character is connected to for multiplayer(if it is viable)
    public int playerID = 0;

    //Referencing the arm to stop movement when collision with wall or player happens
    public GameObject arm;//Reference to Arm Child/Cube.1
    public HungrySquirrels_ArmCollision aBehav;//Referencing arm values
    public BoxCollider hitBox;//Hit box for arm

    protected float translate;//Moves Arm forward(put a negative at front to go backwards)
    protected bool pressed;//Checks if button has already been pressed

    //Rotation Controls
    private float rotationInput;//Check for keys/input for rotation
    protected float curr_Rot;//Checks for current rotation of the Player
    public float rot_Max;//Maximum rotation of Player/Limits
    
    //Arm Controls
    public bool moveInput { get; private set; }//Check for key/input for rotation
    [SerializeField] private Key buttonName;
    public bool collectActive;//Checks for if acorn has been collected for arm to retract
    public bool collided;//Checks to see if arm collided with anything for arm to retract or a button prompt
    public bool rebound;

    // Start is called before the first frame update
    void Start()
    {
        hitBox = arm.GetComponent<BoxCollider>();//Sets Arm Collision Trigger
        //Sets default position and rotation of character when chosen as a specific playerID
        switch (playerID)
        {
            case 0:
                transform.position = new Vector3(18, 1.6f, 18);
                transform.localEulerAngles = new Vector3 (0, 135, 0);
                break;
            case 1:
                transform.position = new Vector3(-18, 1.6f, 18);
                transform.localEulerAngles = new Vector3(0, 45, 0);
                break;
            case 2:
                transform.position = new Vector3(-18, 1.6f, -18);
                transform.localEulerAngles = new Vector3(0, -45, 0);
                break;
            case 3:
                transform.position = new Vector3(18, 1.6f, -18);
                transform.localEulerAngles = new Vector3(0, -135, 0);
                break;
            default:
                transform.position = new Vector3(0, 1.6f, 0);
                transform.localEulerAngles = new Vector3(0, 135, 0);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        rotationInput = Input.GetAxis("Horizontal");//Left & Right input
        moveInput = Input.GetKey(KeyCode.W/*JoystickButton14*/);//Forward & Backward Input
        RotateArm(rotationInput);//More consistent rotation than in fixed update
        ExtendArm(moveInput);//Moving Arm Input(I need to apply button input for simplicity sake)
    }

    void RotateArm(float Input)
    {
        float rotation = Input * rot_Speed * Time.deltaTime * 60;//Applies rot_speed to input
        //See if arm is moving so player cannot rotate
        if (pressed == false)
        {
            //Limit's The Rotation of arm so they can stay within the box
            if ((rotationInput < 0 && curr_Rot > -rot_Max) || (rotationInput > 0 && curr_Rot < rot_Max))
            {
                //Moves the arm by using transforms instead of the actual rigidbody physics(removes problem of flinging by clipping)
                transform.Rotate(Vector3.up, rotation);
                curr_Rot += rotation;//Updates Current Rotation for limit checkers
            }
        }
    }

    void ExtendArm(bool Input)
    {
        //Arm extend & dextend code
        translate = ext_Speed * Time.deltaTime * 60;//Applies ext_Speed to input

        if (Input == true && collided == false && rebound == false)
        {
            arm.transform.Translate(translate, 0, 0, Space.Self);
            collectActive = true;
            pressed = true;
            
        }
        else if (Input == false && collided == false && rebound == false)
        {
            arm.transform.Translate(-translate, 0, 0, Space.Self);
            collectActive = false;
            pressed = false;
        }
        else if (collided == true)
        {
            arm.transform.Translate(-translate * 2, 0, 0, Space.Self);
            collectActive = false;
        }
        else if (rebound == true)
        {
            arm.transform.Translate(-translate, 0, 0, Space.Self);
            collectActive = false;
        }

        if (arm.transform.localPosition.x < 0)//When the arm reaches origin, stops the arm from dextending
        {
            //Change previous values false to enable arm rotation & interaction
            collided = false;
            rebound = false;
            pressed = false;
            arm.transform.localPosition = new Vector3(0, 0, 0);//Sets arm to original place
            if(aBehav.aOn != 0)
            {
                Debug.Log(aBehav.aOn);
                aBehav.aOn = 0;
            }
        }
    }
}
