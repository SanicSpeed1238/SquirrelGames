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
        SetRotationInput();

    }

    private void SetRotationInput()
    {
        float dur = Random.Range(0, 5);

        //controller.SetLeftStickInput();
    }

}
