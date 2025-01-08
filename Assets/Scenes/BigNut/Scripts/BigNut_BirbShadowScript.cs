using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNut_BirbShadowScript : MonoBehaviour
{
    public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + new Vector3(2, 8, 0);
    }
}
