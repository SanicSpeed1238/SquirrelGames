using UnityEngine;

public class BigNut_AcornScript : MonoBehaviour
{
    public BigNut_GameManagerScript gameManagerScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            gameManagerScript.GameEnd(other.gameObject.name);
        }
    }
}
