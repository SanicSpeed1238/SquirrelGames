using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BirdFight_CharacterController : MonoBehaviour
{
    public float moveSpeed = 2f;

    private PlayerInput input;
    private InputAction move;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        move = input.actions["Move"];
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = move.ReadValue<Vector2>();
    }
}
