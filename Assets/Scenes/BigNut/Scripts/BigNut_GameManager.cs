using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNut_GameManager : MonoBehaviour
{
    public GameObject[] bushPatterns;

    // Start is called before the first frame update
    void Start()
    {
        System.Random rand = new System.Random();
        int randomIndex = rand.Next(bushPatterns.Length);
        GameObject.Instantiate(bushPatterns[randomIndex]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
