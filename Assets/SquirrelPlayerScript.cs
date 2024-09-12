using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquirrelPlayerScript : MonoBehaviour
{
    [SerializeField] private Animator hammerAnimator;
    public float moveSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 newPosition = transform.position + moveSpeed * Time.deltaTime * new Vector3(0, verticalInput, 0);
        transform.position = newPosition;
    }
}
