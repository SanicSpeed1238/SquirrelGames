using System.Collections;
using System.Collections.Generic;
<<<<<<< Updated upstream
=======
using System.Collections.Specialized;
>>>>>>> Stashed changes
using System.Runtime.InteropServices;
using UnityEngine;

public class playerController : MonoBehaviour
{

<<<<<<< Updated upstream
    public float moveSpeed = 5f;
    public Rigidbody rb;
    Vector3 movement;
    Vector3 mousePos;
    string A;
    string D;
=======
    private bool isMoving;
    private Vector3 origPos, targetPos;
    private float timeToMove = 0.2f;
    public float moveSpeed = 5f;
    public Rigidbody rb;
    public Vector3 soup;
>>>>>>> Stashed changes

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

<<<<<<< Updated upstream
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
=======
        if (Input.GetKey(KeyCode.W) && !isMoving)
            StartCoroutine(MovePlayerVert(Vector3.up));
        if (Input.GetKey(KeyCode.A) && !isMoving)
            StartCoroutine(MovePlayerHori(Vector3.left - new Vector3(3, 0, 0)));
        if (Input.GetKey(KeyCode.S) && !isMoving)
            StartCoroutine(MovePlayerVert(Vector3.down));
        if (Input.GetKey(KeyCode.D) && !isMoving)
            StartCoroutine(MovePlayerHori(Vector3.right + new Vector3(3, 0, 0)));

    }

    private IEnumerator MovePlayerVert(Vector3 direction)
    {

        isMoving = true;

        float elapsedTime = 0;

        origPos = transform.position;
        targetPos = origPos + direction;

        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp (origPos, targetPos, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;

    }

    private IEnumerator MovePlayerHori(Vector3 direction)
    {

        isMoving = true;

        float elapsedTime = 0;

        origPos = transform.position;
        targetPos = origPos + direction;

        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(origPos, targetPos, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
>>>>>>> Stashed changes

    }

    void FixedUpdate()
    {
<<<<<<< Updated upstream
        if (Input.GetKeyDown(KeyCode.W) == true)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
            //print("Works");

        }
        //rb.MovePosition(rb.position + (movement.y * 10));
=======



>>>>>>> Stashed changes
    }

}
