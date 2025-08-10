using UnityEngine;
using UnityEngine.InputSystem;

public class HungrySquirrels_PlayerController : Base_PlayerController
{
    

    [Header("Dependencies")]
    //Referencing the arm to stop movement when collision with wall or player happens
    public GameObject arm;//Reference to Arm Child/Cube.1
    public GameObject armNoodle;//Reference to the Arm Extension
    public HungrySquirrels_ArmCollision aBehav;//Referencing arm values
    public BoxCollider hitBox;//Hit box for arm

    protected float translate;//Moves Arm forward(put a negative at front to go backwards)
    protected bool pressed;//Checks if button has already been pressed

    //Rotation Controls
    private Vector2 rotationInput;//Check for keys/input for rotation
    private float initialRotation;
    public float curr_Rot;//Checks for current rotation of the Player
    public float rot_Max;//Maximum rotation of Player/Limits

    [Header("Values to Be Adjusted")]
    //Arm Controls
    private bool moveInput;//Check for key/input for rotation
    public bool collectActive;//Checks for if acorn has been collected for arm to retract
    public bool collided;//Checks to see if arm collided with anything for arm to retract or a button prompt
    public bool rebound;

    //Controls the speed of rotation for character
    public float rot_Speed = 3f;
    //Controls Arm Speed
    public float ext_Speed = 0.2f;

    //Stun Options
    [SerializeField] private float _stunDur;
    [SerializeField] private GameObject mainBodyModel;
    private float _eTime;
    public bool _stunActive;

    // Start is called before the first frame update
    private void Start()
    {
        hitBox = arm.GetComponent<BoxCollider>();//Sets Arm Collision Trigger
        playerColor.a = 0.5f;
        mainBodyModel.GetComponentInChildren<Renderer>().material.color = playerColor;
        arm.GetComponentInChildren<Renderer>().material.color = playerColor;
        armNoodle.GetComponent<Renderer>().material.color = playerColor;
        initialRotation = transform.eulerAngles.y;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
            return;

        if (_stunActive == true)
        {
            _eTime += Time.deltaTime;
            if(_eTime >= _stunDur)
            {
                _stunActive = false;
                _eTime = 0;
            }
        }
        else
        {
            _eTime = 0;
            RotateArm();//More consistent rotation than in fixed update
            ExtendArm();//Moving Arm Input(I need to apply button input for simplicity sake)
        }
        
    }

    private void RotateArm()
    {
        float rotation = rotationInput.x * rot_Speed * Time.deltaTime * 60;//Applies rot_speed to input
        //See if arm is moving so player cannot rotate
        if (pressed == false)
        {

            //Limit's The Rotation of arm so they can stay within the box
            if ((rotationInput.x < 0 && curr_Rot > -rot_Max && transform.eulerAngles.y > initialRotation-rot_Max) || (rotationInput.x > 0 && curr_Rot < rot_Max/* && transform.eulerAngles.y < initialRotation + rot_Max*/))
            {
                //Moves the arm by using transforms instead of the actual rigidbody physics(removes problem of flinging by clipping)
                if(playerID == 1 || playerID == 2)
                {
                    transform.Rotate(Vector3.up, -rotation);
                }
                else
                {
                    transform.Rotate(Vector3.up, rotation);
                }
                curr_Rot += rotation;//Updates Current Rotation for limit checkers
            }

        }
    }

    private void ExtendArm()
    {
        //Arm extend & dextend code
        translate = ext_Speed * Time.deltaTime * 60;//Applies ext_Speed to input

        if (moveInput == true && collided == false && rebound == false)
        {
            arm.transform.Translate(translate, 0, 0, Space.Self);
            if (aBehav.aOn == 0 && aBehav.bActive == false)
            {
                collectActive = true;
            }
            else
            {
                collectActive = false;
            }
            pressed = true;
            
        }
        else if (moveInput == false && collided == false && rebound == false)
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
            arm.transform.Translate(-translate * 1.5f, 0, 0, Space.Self);
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
                playerScore += aBehav.aOn;
                MG_GameManager.Instance.UpdatePlayerScore(playerID, playerScore, aBehav.aOn);
                if(aBehav.aOn == 1)
                {
                    aBehav.screwModel.SetActive(false);
                }
                else if (aBehav.aOn == 2)
                {
                    aBehav.acornModel.SetActive(false);
                }
                aBehav.aOn = 0;
            }
        }
    }

    //HUMAN INPUT
    public void OnRotate(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<Vector2>();
    }
    public void OnExtend(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValueAsButton();
    }

    //COMPUTER INPUT
    public void SetLeftStickInput(Vector2 input)
    {
        rotationInput = input;
    }
    public void SetActionInput(bool input)
    {
        moveInput = input;
    }

}
