using System.Collections.Generic;
using UnityEngine;

public class BigNut_HidyHole : MonoBehaviour
{
    public float hidingCooldown = 2.0f;

    private float currentHidingCooldown;
    private bool canHide = true;
    private GameObject hidingPlayer = null;
    private HashSet<GameObject> nearbyPlayers = new HashSet<GameObject>();

    public bool CanHide { get => canHide; }
    public int NearbyPlayerCount {  get => nearbyPlayers.Count; }


    public GameObject HidingPlayer { get => hidingPlayer; set
            { 
                hidingPlayer = value; 
                if (hidingPlayer != null)
                {
                    canHide = false;
                }        
            } }

    void Start()
    {
        currentHidingCooldown = hidingCooldown;
    }

    void Update()
    {
        if (!canHide)
        {
            currentHidingCooldown -= Time.deltaTime;
            if (currentHidingCooldown < 0)
            {
                currentHidingCooldown = hidingCooldown;
                canHide = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<BigNut_Controller>() != null) 
        {
            nearbyPlayers.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<BigNut_Controller>() == null) 
        { 
            nearbyPlayers.Remove(other.gameObject);
        }
    }
}
