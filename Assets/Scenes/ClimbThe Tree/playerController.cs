using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject forest;
    private GameObject[] trees;
    private int WhichTreeIsSquirrelOn = 0; // starts on the leftmost tree, 0 corresponds to the tree in the array
    // Start is called before the first frame update
    void Start()
    {
        trees = new GameObject[forest.transform.childCount];
        for (int i = 0; i < trees.Length; i++)
        {
            trees[i] = forest.transform.GetChild(i).gameObject;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W) && !isMoving)
            StartCoroutine(MovePlayerVert(Vector3.up));
        if (Input.GetKey(KeyCode.A) && !isMoving)
            StartCoroutine(MovePlayerHori(Vector3.left));
        if (Input.GetKey(KeyCode.S) && !isMoving)
            StartCoroutine(MovePlayerVert(Vector3.down));
        if (Input.GetKey(KeyCode.D) && !isMoving)
            StartCoroutine(MovePlayerHori(Vector3.right));
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
        if (WhichTreeIsSquirrelOn == 0 && direction == Vector3.left) // if we are on leftmost tree and we press left dont go anywhere
        {
            targetPos = origPos;
        }
        else if (WhichTreeIsSquirrelOn == trees.Length - 1 && direction == Vector3.right) // if we are on right most tree and press right dont go anywhere
        {
            targetPos = origPos;
        }
        else
        {
            targetPos = origPos;
            
            if (direction.x < 0)
            {
                WhichTreeIsSquirrelOn -= 1;
                targetPos.x = trees[WhichTreeIsSquirrelOn].transform.position.x;
            }
            else
            {
                WhichTreeIsSquirrelOn += 1;
                targetPos.x = trees[WhichTreeIsSquirrelOn].transform.position.x;
            }
        }
        Debug.Log(WhichTreeIsSquirrelOn);
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
