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
    private Quaternion _armDir;
    private bool _armMOVE = true;
    private float _eTime;
    private bool _doOnce = false;

    private void Awake()
    {
        controller = GetComponent<HungrySquirrels_PlayerController>();
    }

    private void Start()
    {
        //SetRotationInput();
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
         //_coinFlip;
        
        if (_doOnce == false)
        {
            _armDir = Random.rotation;
            dur = Random.Range(0.2f, 1f);
            _doOnce = true;

        }

        if (_eTime < dur)
        {
            Vector2 input = new(_armDir.x, 0);
            controller.SetLeftStickInput(input);
        }
        else if (_eTime >= dur)
        {
            controller.SetLeftStickInput(Vector2.zero);
            _armDir.x = 0;
            _doOnce = false;
            dur = 0;
            _eTime = 0;
            _armMOVE = false;
        }
    }

    private void ExtendInput()
    {
        
        if (_doOnce == false)
        {
            dur = Random.Range(1, 2);
            _doOnce = true;
        }

        if (_eTime < dur)
        {
            controller.SetActionInput(true);
        }
        else if(_eTime >= dur)
        {
            controller.SetActionInput(false);
            _doOnce = false;
            dur = 0;
            _eTime = 0;
            _armMOVE = true;

        }

    }

}
