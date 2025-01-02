using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNut_CameraFollowPlayer : MonoBehaviour
{
    public Transform target; // Reference to the character (drag in the Inspector)

    private Vector3 offset = new Vector3(10, 0, 0); // Offset from the target

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
