using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class playerController : MonoBehaviour
{
    private bool isMoving;
    private Vector3 origPos, targetPos;
    private float timeToMove = 0.2f;
    public float moveSpeed = 5f;
    public Rigidbody rb;
    public Vector3 soup;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
            transform.position = Vector3.Lerp(origPos, targetPos, (elapsedTime / timeToMove));
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
    }

    void FixedUpdate()
    {
        // Any fixed update logic can go here
    }
}
