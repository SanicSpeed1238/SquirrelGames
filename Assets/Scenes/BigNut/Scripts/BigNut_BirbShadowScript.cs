using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNut_BirbShadowScript : MonoBehaviour
{
    public Transform player;

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + new Vector3(0, 10, 0);
    }
}
