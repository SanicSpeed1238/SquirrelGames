using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableSpawnerScript : MonoBehaviour
{
    [Header("Important Components")]
    public GameObject collectableObject;
    public GameObject forest;
    public float timeBetweenAcorns;
    private float resetTimer;
    private GameObject[] lanes;
    private Vector3 acornSpawnLocation; // where each acorn spawns
    // Start is called before the first frame update
    private void Start()
    {
        lanes = new GameObject[forest.transform.childCount];
        for (int i = 0; i < lanes.Length; i++)
        {
           lanes[i] = forest.transform.GetChild(i).gameObject;
        }
        acornSpawnLocation = transform.position;
        resetTimer = timeBetweenAcorns;
        InvokeRepeating("SpawnNewCollectable", 0, timeBetweenAcorns);
    }
    public void SpawnNewCollectable()
    {
        acornSpawnLocation.x = lanes[Random.Range(0, lanes.Length)].transform.position.x; // randomly choose one of the trees and set the acorn spawn location to it
        Instantiate(collectableObject, acornSpawnLocation, Quaternion.identity);
    }
}
