using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BigNut_PlayerController : MonoBehaviour
{
    [Header("Movement Values")]
    public float moveSpeed;
    public bool p2;

    [Header("Important Components")]
    public GameObject squirrelModel;
    public Animator squirrelAnimator;

    // Other Variables Needed
    private bool isAlive;
    private bool isHiding;
    private CharacterController controller;

    private Transform birbThatCaught;

    private float pushDistance = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;
        isHiding = false;
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float xinput;
        float zinput;

        if (isAlive)
        {
            squirrelAnimator.speed = 0;

            if(!p2) xinput = Input.GetAxisRaw("Horizontal2");
            else xinput = Input.GetAxisRaw("Horizontal");
            if (!p2) zinput = Input.GetAxisRaw("Vertical2");
            else zinput = Input.GetAxisRaw("Vertical");

            if (xinput != 0 || zinput != 0)
            {
                squirrelAnimator.speed = 1;
                Vector3 movement = transform.right * xinput + transform.forward * zinput;
                controller.Move(movement*moveSpeed*Time.deltaTime);
            }
        }
        else
        {
            transform.position = birbThatCaught.position;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "HidyHole")
        {
            BigNut_HidyHole hidyHole = collision.gameObject.GetComponent<BigNut_HidyHole>();
            if (hidyHole.HidingPlayer != null)
            {
                Vector3 direction = (hidyHole.HidingPlayer.transform.position - gameObject.transform.position).normalized;
                hidyHole.HidingPlayer.GetComponent<CharacterController>().Move(direction*pushDistance);
            }
            hidyHole.HidingPlayer = this.gameObject;
            isHiding = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "HidyHole")
        {
            isHiding = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isHiding && collision.gameObject.tag == "Birb")
        {
            isAlive = false;
            birbThatCaught = collision.gameObject.transform;
        }
    }
}
