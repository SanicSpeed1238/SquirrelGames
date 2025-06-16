using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HungrySquirrels_PlayerController))]
public class HungrySquirrels_ComputerController : MonoBehaviour
{

    //Variables Needed
    private HungrySquirrels_PlayerController controller;

    //Variables to Control Arm
    private float dur;
    private int _armDir;
    private bool _armMOVE = true;
    private float _eTime;
    private bool _doOnce = false;

    private void Awake()
    {
        controller = GetComponent<HungrySquirrels_PlayerController>();
    }

    private void Start()
    {
        SetRotationInput();
    }

    private void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
            return;

        if (dur != 0)
        {
            _eTime += Time.deltaTime;
            //Debug.Log("Elapsed Time: " + _eTime);
        }
        

        if(_armMOVE == true)
        {
            SetRotationInput();
        }
        else if(_armMOVE == false)
        {
            ExtendInput();
        }
    }

    private void SetRotationInput()
    {
        
        if (_doOnce == false)
        {
            //Debug.Log("Setting Direction");
            _armDir = Random.Range(-1, 1);
            if (_armDir == 0)
            {
                _armDir = Random.Range(-1, 1);
            }
            dur = Random.Range(0.2f, 0.5f);
            
        }

        if (_eTime < dur)
        {
            Vector2 input = new(_armDir, 0);
            controller.SetLeftStickInput(input);
        }
        else if (_eTime >= dur)
        {
            Debug.Log("END ROTATE");
            controller.SetLeftStickInput(Vector2.zero);
            /*_doOnce = false;
            dur = 0;
            _eTime = 0;*/
            _armMOVE = false;
        }
    }

    private void ExtendInput()
    {
        
        if (_doOnce == false)
        {
            dur = Random.Range(0.3f, 0.7f);
        }

        if (_eTime < dur)
        {
            controller.SetActionInput(true);
        }
        else if(_eTime >= dur)
        {
            Debug.Log("EXTENDed");
            controller.SetActionInput(false);
            _doOnce = false;
            dur = 0;
            _eTime = 0;
            _armMOVE = true;

        }

    }

}
