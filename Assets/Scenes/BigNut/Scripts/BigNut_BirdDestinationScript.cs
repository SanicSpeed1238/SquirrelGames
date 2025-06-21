using UnityEngine;

public class BigNut_BirdDestinationScript : MonoBehaviour
{
    private float movementSpeed;

    void Start()
    {
        movementSpeed = Camera.main.GetComponent<BigNut_CameraScript>().CameraMovementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
        {
            return;
        }

        transform.position += Vector3.right * movementSpeed * Time.deltaTime;
    }
}
