using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HungrySquirrels_PlayerController))]
public class HungrySquirrels_ComputerController : MonoBehaviour
{

    //Variables Needed
    private HungrySquirrels_PlayerController controller;

    //Variables to Control Arm Rotation
    private float dur;
    private int _armDir;
    private bool _armMOVE = false;
    private float _eTime;

    //Variable for Arm Extension
    private float _extDur;
    private bool _armExtStart = false;

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

        if (_armMOVE == true || _armExtStart == true)
        {
            _eTime += Time.deltaTime;
            //Debug.Log("Elapsed Time: " + _eTime);
        }
        SetRotationInput();
        ExtendInput();
    }

    private void SetRotationInput()
    {

        if(_armExtStart == false)
        {

            if (_armMOVE == false)
            {
                //Debug.Log("Setting Direction");
                _armDir = Random.Range(-1, 1);
                if (_armDir == 0)
                {
                    _armDir = Random.Range(-1, 1);
                }
                dur = Random.Range(1, 5);
            }

            if (_eTime < dur)
            {
                //Debug.Log("Time Left: " + (dur-_eTime));
                Vector2 input = new(_armDir, 0);
                controller.SetLeftStickInput(input);
            }
            if (_eTime >= dur)
            {
                _armMOVE = false;
                _eTime = 0;
                controller.SetLeftStickInput(Vector2.zero);
                _armExtStart = true;
            }
        }

    }

    private void ExtendInput()
    {
        if(_armMOVE == false)
        {
            if (_armExtStart == true)
            {
                _extDur = Random.Range(0.3f, 0.7f);
            }

            if (_eTime < _extDur)
            {
                controller.SetActionInput(true);
            }
            if(_eTime >= _extDur)
            {
                _armExtStart = false;
                _eTime = 0;
                controller.SetActionInput(false);
                _armMOVE = true;
            }
        }

    }

}
