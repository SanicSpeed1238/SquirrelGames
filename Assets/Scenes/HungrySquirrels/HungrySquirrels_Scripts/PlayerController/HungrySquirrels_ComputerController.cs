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

}
