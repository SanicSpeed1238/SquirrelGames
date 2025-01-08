using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BigNut_BirbScript : MonoBehaviour
{
    public Transform player;
    public Transform birbDestination;

    public Transform shadow;

    public float maxDistanceFromCharacter = 100f;

    private bool reachedPlayer = false;

    // Start is called before the first frame update

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }    
        
        if (birbDestination == null)
        {
            birbDestination = GameObject.FindGameObjectWithTag("BirbDestination").transform;
        }

        if (shadow == null)
        {
            shadow = GameObject.FindGameObjectWithTag("BirbShadow").transform;
        }
        shadow.localScale = Vector3.zero;
    }


    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, birbDestination.position) == 0)
        {
            Destroy(gameObject);
        }

        if (!reachedPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, 0.02f);
            shadow.localScale += new Vector3(0.003f, 0, 0.003f);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, birbDestination.position, 0.02f);
            shadow.localScale -= new Vector3(0.003f, 0, 0.003f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            reachedPlayer = true;
        }
    }
}
