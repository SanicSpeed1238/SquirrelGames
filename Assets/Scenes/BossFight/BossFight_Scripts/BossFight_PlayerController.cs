using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossFight_PlayerController : Base_PlayerController
{
    public int _acorninv { get; private set;}
    private CapsuleCollider hitbox;
    [SerializeField]private float _moveSpeed;
    private Vector2 _movementInput;
    private Rigidbody _rb;


    // Start is called before the first frame update
    void Start()
    {
        hitbox = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();
        playerColor.a = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        Interaction();
    }

    private void MovePlayer()
    {
        Vector3 schmovement = new Vector3(_movementInput.x, 0, _movementInput.y).normalized * _moveSpeed * Time.deltaTime * 60;
        //Getting Direction for Rotating Character
        Vector3 direction = new Vector3(_movementInput.x, 0, _movementInput.y) - this.transform.position;
        //transform.Rotate(Vector3.up, 45, Space.Self);
        _rb.MovePosition(_rb.position + schmovement);
    }

    private void Interaction()
    {
        //If buttons not pressed, use A to dash
        //If all buttons are pressed & boss is in stun stage, use A to throw
    }

    //HUMAN INPUT
    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

}
