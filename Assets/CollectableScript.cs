using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableScript : MoveDown
{
    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * -transform.up), Space.World);
        if (transform.position.y < deadHeight)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
