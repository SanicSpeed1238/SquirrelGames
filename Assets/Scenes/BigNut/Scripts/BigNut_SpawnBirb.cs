using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNut_SpawnBirb : MonoBehaviour
{
    public GameObject birb;

    private float spawnTime;

    // Start is called before the first frame update
    void Start()
    {
        spawnTime = Random.Range(3, 10);
    }

    // Update is called once per frame
    void Update()
    {
        //if (GameObject.FindGameObjectsWithTag("Birb").Length == 0)
        //{
        //    spawnTime -= Time.deltaTime;
        //    if (spawnTime <= 0)
        //    {
        //        SpawnBirb();
        //        spawnTime = Random.Range(3, 10);
        //    }
        //}                
    }

    private void SpawnBirb()
    {
        Instantiate(birb, transform.position, Quaternion.identity);
    }
}
