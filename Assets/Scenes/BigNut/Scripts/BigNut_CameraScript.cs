using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNut_CameraScript : MonoBehaviour
{
    public float CameraMovementSpeed = 0.5f;

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position += Vector3.right * CameraMovementSpeed * Time.deltaTime;
    }
}
