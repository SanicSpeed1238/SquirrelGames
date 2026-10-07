using UnityEngine;

public class BigNut_BirdDestinationScript : MonoBehaviour
{
    private float distanceFromCamera;

    void Start()
    {
        distanceFromCamera = transform.position.x - Camera.main.transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
        {
            return;
        }

        transform.position = new Vector3(Camera.main.transform.position.x + distanceFromCamera, transform.position.y, transform.position.z);
    }
}
