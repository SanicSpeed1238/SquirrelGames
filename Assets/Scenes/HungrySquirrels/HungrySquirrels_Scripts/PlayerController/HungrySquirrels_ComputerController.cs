using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HungrySquirrels_PlayerController))]
public class HungrySquirrels_ComputerController : MonoBehaviour
{

    //Variables Needed
    private HungrySquirrels_PlayerController controller;

    //Variables to Control Arm Rotation
    private int _armDir;
    private bool _armMOVE = false;
    private float _eTime;

    //Variable for Arm Extension
    private float _extDur;

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

        if (_armMOVE == true)
        {
            _eTime += Time.deltaTime;
        }
        SetRotationInput();

    }

    private void SetRotationInput()
    {
        float dur = Random.Range(0, 5);

        if(_armMOVE == false)
        {
            
            _armDir = Random.Range(-1, 1);
            if(_armDir == 0)
            {
                _armDir = Random.Range(-1, 1);
            }
            //dur = Random.Range(0, 5);
        }

        if (_eTime < dur)
        {
            _armMOVE = true; 
            Vector2 input = new(_armDir, 0);
            controller.SetLeftStickInput(input);
        }
        if(_eTime >= dur)
        {
            _armMOVE = false;
            _eTime = 0;
            controller.SetLeftStickInput(Vector2.zero);
        }

        //controller.SetLeftStickInput();
    }

}
