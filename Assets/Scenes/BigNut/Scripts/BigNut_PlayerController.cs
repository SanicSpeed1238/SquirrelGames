using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BigNut_PlayerController : MonoBehaviour
{
    [Header("Movement Values")]
    public float moveSpeed;

    [Header("Important Components")]
    public GameObject squirrelModel;
    public Animator squirrelAnimator;

    // Other Variables Needed
    private bool isAlive;
    private bool isHiding;
    private CharacterController controller;

    private Transform birbThatCaught;


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
        if (isAlive)
        {
            squirrelAnimator.speed = 0;
            float xinput = Input.GetAxisRaw("Horizontal");
            float zinput = Input.GetAxisRaw("Vertical");
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
