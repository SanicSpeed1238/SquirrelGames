using System.Collections.Generic;
using UnityEngine;

public class BigNut_HidyHole : MonoBehaviour
{
    public static List<GameObject> HidyHoles = new List<GameObject>();

    private GameObject hidingPlayer = null;

    public GameObject HidingPlayer {  get => hidingPlayer; set => hidingPlayer = value; }

    void Start()
    {
        HidyHoles.Add(gameObject);
    }
}
