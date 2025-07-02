using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFight_PlayerController : Base_PlayerController
{
    public int _acorninv { get; private set;}
    private CapsuleCollider hitbox;
    [SerializeField]private float _moveSpeed;
    private Vector2 _movementInput;


    // Start is called before the first frame update
    void Start()
    {
        hitbox = GetComponent<CapsuleCollider>();
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
        //float transform = _movementInput * _moveSpeed * Time.deltaTime * 60;
        transform.Translate(new Vector3(_movementInput.x, 0, _movementInput.y), Space.Self);
    }

    private void Interaction()
    {
        //If buttons not pressed, use A to dash
        //If all buttons are pressed & boss is in stun stage, use A to throw
    }
}
