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
        if (Vector3.Distance(transform.position, birbDestination.position) == 0 || Vector3.Distance(transform.position, player.position) > maxDistanceFromCharacter)
        {
            shadow.localScale = Vector3.zero;
            Destroy(gameObject);
        }

        if (!reachedPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, 0.01f);
            shadow.localScale += new Vector3(0.005f, 0, 0.005f);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, birbDestination.position, 0.08f);
            if (shadow.localScale.x > 0)
            {
                shadow.localScale -= new Vector3(0.01f, 0, 0.01f);
            }
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
