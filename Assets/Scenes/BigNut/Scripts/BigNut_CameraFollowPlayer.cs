using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNut_CameraFollowPlayer : MonoBehaviour
{
    public Transform target; // Reference to the character (drag in the Inspector)

    private Vector3 offset = new Vector3(-12, 5, 0); // Offset from the target

    void Start()
    {
        transform.rotation = Quaternion.Euler(11, 90, 0);
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
