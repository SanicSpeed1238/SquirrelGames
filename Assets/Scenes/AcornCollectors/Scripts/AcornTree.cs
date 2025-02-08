using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AcornTree : MonoBehaviour
{
    [Header("Inscribed")]

    public GameObject acornPrefab;

    public float speed = 1f;
    // Distance where AcornTree turns around
    public float leftAndRightEdge = 10f;

    // Change that the AcornTree will change directions
    public float changeDirChance = 0.1f;
    //Seconds between Acorns and instantiations
    public float acornDropDelay = 1f;
    // Start is called before the first frame update
    void Start()
    {
        // Start dropping acorns
        Invoke ("DropAcorn", 2f);
        
    }

    void DropAcorn(){
        GameObject acorn = Instantiate<GameObject>(acornPrefab);

        acorn.transform.position = transform.position;

        Invoke("DropAcorn", acornDropDelay);
    }

    // Update is called once per frame
    void Update()
    {
        // Basic Movement

        Vector3 pos = transform.position;

        pos.x += speed * Time.deltaTime;

        transform.position = pos;

        // Changing Direction

        if ( pos.x < -leftAndRightEdge){
            speed = Mathf.Abs(speed);
        } else if (pos.x > leftAndRightEdge){
            speed = -Mathf.Abs(speed);
        } 
        
    }

    void FixedUpdate() {
        // Random direction changes are now time-based due to FixedUpdate()

        if(Random.value < changeDirChance) {
            speed *= -1;
        }
    }
}
