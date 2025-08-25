using UnityEngine;

public class BigNut_BirdShadow : MonoBehaviour
{
    private bool instantiated = false;
    private Transform player;

    private void Start()
    {
        transform.localScale = Vector3.zero;
    }

    public void Instantiate(Transform player)
    {
        this.player = player;
        instantiated = true;
    }

    public void UnInstantiate()
    {
        instantiated = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (instantiated)
        { 
            transform.position = new Vector3 (player.transform.position.x, transform.position.y, player.transform.position.z);
        }        
    }
}
