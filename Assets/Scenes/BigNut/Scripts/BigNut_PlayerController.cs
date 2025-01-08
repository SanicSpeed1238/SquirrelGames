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

    public LayerMask groundMask;

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
            float input = Input.GetAxisRaw("Horizontal");
            if (input != 0)
            {
                squirrelAnimator.speed = 1;
                RaycastHit hit;
                if (Physics.Raycast(transform.position - Vector3.down * 0.07f, Vector3.down, out hit, 1.5f, groundMask))
                {
                    Vector3 slopeNormal = hit.normal;
                    Quaternion rotation = Quaternion.Euler(90, 0, 0);
                    Vector3 slopeDirection = rotation * slopeNormal;

                    Vector3 movement = slopeDirection * input * moveSpeed * Time.deltaTime;
                    Debug.Log(movement);
                    controller.Move(movement);
                }
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
